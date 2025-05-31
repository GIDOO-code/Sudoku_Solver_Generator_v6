using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;

namespace GNPX_space{

    public partial class FishMan{
        protected AnalyzerBaseV2 analyzer;
        protected List<UCell>    pBOARD    => analyzer.pBOARD;
        protected UInt128[]      pFreeCell81b9 => analyzer.FreeCell81b9;

        protected UInt128[]     pHouseCells81		=> AnalyzerBaseV2.HouseCells81;
        protected UInt128[]     pConnectedCells81	=> AnalyzerBaseV2.ConnectedCells81;

        protected int           sz;               //fish size (2D:Xwing 3D:SwordFish 4D:JellyFish 5D:Squirmbag 6D:Whale 7D:Leviathan)
        protected int           no;               //digit
        protected int           FMSize;

        protected List<ULogical_Node>   FishElementList;
        protected bool extFlag;                   //3D or more sizes. ...

        public FishMan( ){ }
        public FishMan( AnalyzerBaseV2 analyzer, int FMSize, int no, int sz, bool extFlag=false ){
            this.analyzer = analyzer;

            this.extFlag = extFlag;                          //sz>2 or ...                 
            this.no = no; 
            this.sz = sz;
            this.FMSize = FMSize;

            UInt128 FreeCell81b9no = pFreeCell81b9[no];
            FishElementList = new List<ULogical_Node>();
            for( int h=0; h<FMSize; h++ ){                   //crate element cells of fish
                UInt128  b081 = FreeCell81b9no & pHouseCells81[h];
                if( b081.IsZero() )  continue;
                FishElementList.Add( new ULogical_Node( noB9:1<<no, b081:b081, pmCnd:0, ID:h ) );   
            }
            FishElementList.DistinctBy(p=>p.matchKey2);     //If the row/column and block have the same GFish, leave the row/column GFish.
               //int n=0; FishElementList.ForEach( P=> WriteLine( $"sq:{n++} #{no+1} {P}" ) );
        }


         //81.....32.2783149.......7182963481..478.1.369..1796824182.....3.4512.98.....8.241

        public IEnumerable<UFish> IEGet_BaseSet( int baseSelecter, bool FinnedFlag=false, bool EndoFlag=false ){
            if( FishElementList==null || FishElementList.Count<sz )  yield break;

            bool basicFish = (baseSelecter.BitCount()<=9) & !FinnedFlag;  //not F/M & notF/M
            int  BaseSelR  = 0x3FFFF ^ baseSelecter;
                
            Combination cmbBas = new Combination( FishElementList.Count, sz );
            int nxt = int.MaxValue;
						
			List<int>  rcbList = new();
            while( cmbBas.Successor(skip:nxt) ){
                    //WriteLine( cmbBas );
                UInt128  BaseB128    = UInt128.Zero;
                UInt128  EndoFinB128 = UInt128.Zero;
                UInt128  Q;

				int      rcbB = 0;	// ##GeneralLogic   ulong    rcbB = 0;	@@@@@@@@@@@@@@@@@@@@@@@
				int      HouseB = 0;

				rcbList.Clear();
                for( int k=0; k<sz; k++ ){
                    nxt=k;
                    int nx = cmbBas.Index[k];
                    ULogical_Node GF = FishElementList[nx];

                    if( ((1<<GF.ID) & baseSelecter) == 0 )  goto nxtCmb;  // not match RCB specification
                    if( (Q=BaseB128&GF.b081).IsNotZero() ){                // overlap
                        if( !EndoFlag )   goto nxtCmb;                     // Endo Fish?
                        EndoFinB128 |= Q;                                  // overlaped cells
                    }
                    HouseB   |= 1<<GF.ID;                                  // set house Number
                    BaseB128 |= GF.b081;                                   // BaseSet	// ##GeneralLogic @@@@@@@@@@@@@@@@@@@@@@@
                    rcbB     |= GF.rcbFrame;                               // rcb

                    if( basicFish && k>0 && (rcbB&BaseSelR).BitCount()>sz ) goto nxtCmb; 
					rcbList.Add(GF.rcbFrame);
                }
				if( !Check_rcbConection(rcbList) )    goto nxtCmb;

                if( EndoFlag && EndoFinB128.IsZero() )  goto nxtCmb;
                UFish UF = new UFish( no, sz, HouseB, BaseB128, EndoFinB128 );    //Baseset
                      //  __Debug_PattenPrint(UF);      //@@@@@@@@@@@

                yield return UF;

              nxtCmb:
                continue;
            }
            yield break;
        }

        private void __Debug_PattenPrint( UFish UF ){
            WriteLine( $"no={no} sz={sz}  BaseSet:{UF.HouseB.HouseToString()}" );
            UInt128 BPnoB = pFreeCell81b9[no];  //new Bit81(pBOARD,1<<no);
            string noST=" "+no.ToString();
            for(int r=0; r<9; r++ ){
                string st="";
                BPnoB.GetRowList(r).ForEach(p=>st+=(p==0? " .": noST));
                st+=" ";
                UF.BaseB81.GetRowList(r).ForEach(p=>st+=(p==0? " .": " B"));
                st+=" ";
                (BPnoB.DifSet(UF.BaseB81)).GetRowList(r).ForEach(p=>st+=(p==0? " .": " X"));
                WriteLine(st);
            }
        }




        public IEnumerable<UFish> IEGet_CoverSet( UFish UFish_BaseSet, int coverSelecter, bool FinnedFlag, bool CannFlag=false, bool debugPrint=false ){
            if( FishElementList==null )  yield break;

            //Selects only elements related to the BaseSet as candidates for the CoverSet.
            //This process is very effective. It explicitly considers the relationship between BaseSet and CoverSet.
            List<ULogical_Node> HCLst = FishElementList.FindAll( p => 
                    !UFish_BaseSet.HouseB.IsHit(p.ID) && coverSelecter.IsHit(p.ID) && UFish_BaseSet.BaseB81.IsHit(p.b081)  );
                if( HCLst.Count<sz ) yield break;

                if( debugPrint ){
                    int kx=0; HCLst.ForEach( P => WriteLine( $"kx:{kx++} {P}" ) ); 
                }

            Combination cmbCov=new Combination(HCLst.Count,sz);
            int nxt=int.MaxValue;
            while( cmbCov.Successor(skip:nxt) ){
                int   usedLK=0;
                UInt128  CoverB81 = UInt128.Zero;
                UInt128  CannFinB81 = UInt128.Zero;
                UInt128 Q;
                for(int k=0; k<sz; k++ ){
                    nxt=k;
                    int nx=cmbCov.Index[k];                   
                    if( (Q=CoverB81&HCLst[nx].b081).IsNotZero() ){ //overlap
                        if( !CannFlag )  goto nxtCmb;
                        CannFinB81 |= Q;
                    }
                    usedLK   |= 1<<HCLst[nx].ID;  //house number
                    CoverB81 |= HCLst[nx].b081;        //Bit81
                }

                //If you just want to solve Sudoku, eliminate the following code.
                if( CannFlag && CannFinB81.IsZero() ) goto nxtCmb; //Eliminate other "fish" when CannFlag is true!


                // case no Fin, BaseSet is covered with CoverSet.
                // case with Fin, the part not covered by CoverSet of BaseSet is Fin.
                UInt128 FinB81 = UFish_BaseSet.BaseB81 .DifSet(CoverB81);     //(oprator- : difference set)
                if( FinnedFlag != (FinB81.BitCount()>0) ) continue;      //
                UFish UF = new( UFish_BaseSet, usedLK, CoverB81, FinB81, CannFinB81 );
                //==================
                yield return UF;
                //------------------
              nxtCmb:
                continue;
            }
            yield break;
        }
    }


}