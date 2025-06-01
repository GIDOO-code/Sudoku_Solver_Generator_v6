using System;
using System.Linq;
using static System.Diagnostics.Debug;
using GIDOO_space;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace GNPX_space{
    public partial class FishGen: AnalyzerBaseV2{

//        public int[] rcbCntrl = { 0x1FF, 0x1ff<<9, 0x3FFFF, 0x7FFFFFF };


        //=======================================================================
        //Fish:
        // Understand this algorithm, you need to know BaseSet and CoverSet.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page34.html
        //
        //81........27.3149.......718.9.34.....7.....6.....96.2.182.......4512.98........41
        //81.....32.2783149.......7182963481..478.1.369..1796824182.....3.4512.98.....8.241


		//417..35.62.5..7...93..56.7........6...3.9.7.1691..58..3...71..9..98....77...3.21.


		// GNPX 39 step6  ... error
		//2.548...9.9..3..2...7.923.4..8.....2541...6879.....1..4.961.2...8..7..9.7...584.6
		//23548.7.9.94.3.82.8.7.923.4..8...9.25413296879..8..1..459613278.8.27459.7..9584.6
		//235481769194736825867592314378165942541329687926847153459613278683274591712958436 


		//.8.....9.6...3...8...6.1.....4..516..7.8...3...316.8.....4.6...5...9...2.3.....1.
		//38....69161.93..48.4.681...8.43.516.1768...3..5316.8....8416...56179348243...8.16

		private int		 stageNoPMemo = -9;

        public bool XWing()     => Fish_Basic(2);
        public bool SwordFish() => Fish_Basic(3);
        public bool JellyFish() => Fish_Basic(4);
        public bool Squirmbag() => Fish_Basic(5);    //complementary to 4D 
        public bool Whale()     => Fish_Basic(6);    //complementary to 3D 
        public bool Leviathan() => Fish_Basic(7);    //complementary to 2D 

        //FinnedFish
        public bool FinnedXWing()     => Fish_Basic(2,fin:true);
        public bool FinnedSwordFish() => Fish_Basic(3,fin:true);
        public bool FinnedJellyFish() => Fish_Basic(4,fin:true);
        public bool FinnedSquirmbag() => Fish_Basic(5,fin:true);
        public bool FinnedWhale()     => Fish_Basic(6,fin:true);
        public bool FinnedLeviathan() => Fish_Basic(7,fin:true);

        // Basic Fish


        public bool Fish_Basic( int sz, bool fin=false ){

			// ===== Prepare =====
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
			}

            int  _rcbSel=0x1FF, _colSel=_rcbSel<<9;
            for( int no=0; no<9; no++ ){

                foreach( var _ in ExtFishSub_Basic( sz, no, FMSize:18, _rcbSel, _colSel, FinnedFlag:fin) ){
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                }
    
                foreach( var _ in ExtFishSub_Basic( sz, no, FMSize:18, _colSel, _rcbSel, FinnedFlag:fin) ){
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                }
            }
            return false;	//(SolCode>0);
        }   

        public IEnumerable<bool> ExtFishSub_Basic( int sz, int no, int FMSize, int baseSelecter, int coverSelecter, bool FinnedFlag, bool _Fdef=true ){       
            int noB = 1<<no;
            bool extFlag = (sz>=3 && ((baseSelecter|coverSelecter).BitCount()>18));
            FMan = new FishMan( this, FMSize:FMSize, no, sz, extFlag );                  // ..... FMSize: 18  ( row, column )


            // ===== select BaseSet =====
            foreach( var UFbase in FMan.IEGet_BaseSet_Basic( baseSelecter, FinnedFlag:FinnedFlag )){ 
                // ===== select CoverSet =====

                foreach( var UFcover in FMan.IEGet_CoverSet_Basic( UFbase, coverSelecter, FinnedFlag ) ){

                    UInt128 FinB81 = UFcover.FinB81;
                    UInt128 ELM = UInt128.Zero;
                    var FinZeroB = FinB81.IsZero();

                    //===== no Fin =====
                    if( !FinnedFlag && FinZeroB ){         
                        if( !FinnedFlag && (ELM=UFcover.CoverB81.DifSet(UFbase.BaseB81) ).BitCount()>0 ){                      
                            foreach( var P in ELM.IEGet_UCell_noB(pBOARD,noB) ){ P.CancelB=noB; SolCode=2; }
                            if(SolCode>0){              //solved!

                                if( SolInfoB )  _FishResult(no,sz,UFbase,UFcover,(FMSize==27), ELM ); //FMSize 18:regular 27:Franken/Mutant
                                yield return true;
                            }
                        }
                    }

                     //===== Finned ===== 
                    else if( FinnedFlag && !FinZeroB ){    //===== Finned ===== 
                        UInt128 Ecand=UFcover.CoverB81.DifSet(UFbase.BaseB81);
                        ELM = UInt128.Zero;
                        foreach( var P in Ecand.IEGet_UCell_noB(pBOARD,noB) ){
                            if( (FinB81.DifSet(ConnectedCells81[P.rc]) ).BitCount()==0 ) ELM = ELM.Set(P.rc);
                        }
                        if( ELM.BitCount() > 0 ){    //there are cells/digits can be eliminated                        
                            foreach( var P in ELM.IEGet_rc().Select(p=>pBOARD[p]) ){ P.CancelB=noB; SolCode=2; }   
                            if(SolCode>0){  //solved!
                                if( SolInfoB )  _FishResult(no,sz,UFbase,UFcover,(FMSize==27), ELM ); //FMSize 18:regular 27:Franken/Mutant
                                yield return true          ;
                            }
                        }
                    }
                    continue;
                }
            }
            yield break;       
        }
    }




    public partial class FishMan{

        public IEnumerable<UFish> IEGet_BaseSet_Basic( int baseSelecter, bool FinnedFlag=false, bool EndoFlag=false ){

//			int loop = 0;
            if( FishElementList==null || FishElementList.Count<sz )  yield break;
                //int n=0; FishElementList.ForEach( P=> WriteLine( $" FishElementList sq:{n++} {P}" ) );  //@@@@@@@@@@@@@@@@

            bool basicFish = (baseSelecter.BitCount()==9) & !FinnedFlag;  //not F/M & notF/M

//			UInt128 bp81_no = pFreeCell81b9[no];

            int nxt = 0;
            Combination cmbBas = new Combination( FishElementList.Count, sz );
			List<int>  rcbList = new();
            while( cmbBas.Successor(skip:nxt) ){
                UInt128  BaseB128 = UInt128.Zero;
                int      usedLK=0,  rcbB=0;
				rcbList.Clear();
                for( int k=0; k<sz; nxt=++k ){
                    ULogical_Node GF = FishElementList[ cmbBas.Index[k] ];
                    if( !baseSelecter.IsHit(GF.ID) )  goto nxtCmb;              // not match RCB specification
                    usedLK   |= 1<<GF.ID;                                       // set house Number
                    BaseB128 |= GF.b081;                                        // BaseSet
                    rcbB     |= GF.rcbFrame;                                    // rcb ( row/column/block)
                    if( basicFish && (rcbB&baseSelecter).BitCount()>sz )  goto nxtCmb;
					rcbList.Add(GF.rcbFrame);
                }
				if( !Check_rcbConection(rcbList) )    goto nxtCmb;
//					WriteLine( $"\nloop : {loop++}" );
//					WriteLine( $" no:#{no+1}  nonp_no81:{bp81_no.ToBitString81N()}" );
//					WriteLine( $"         BaseB128:{BaseB128.ToBitString81N()}" ) ;
//					WriteLine( $" bp81_no&BaseB128:{(bp81_no&BaseB128).ToBitString81N()}" ) ;


                UFish UFbase = new UFish( no, sz, usedLK, BaseB128, EndoFinB81:UInt128.Zero );    //Baseset             
                        //WriteLine( $"usedLK:{usedLK.ToBitString(18)}" );
                        //__Debug_PattenPrint(UFbase);

//###					WriteLine( $"IEGet_BaseSet_Basic UFbase:{UFbase}");
                yield return UFbase;

              nxtCmb:
                continue;
            }
            yield break;
        }

		private bool Check_rcbConection( List<int> rcbList ){
			int sz = rcbList.Count;
			if( sz == 2 )  return (rcbList[0]&rcbList[1])>0;

			//rcbList.ForEach( P => WriteLine( P.ToBitString27rcb() ) );
			for( int k=0; k<sz; k++ ){
				int Q =0;
				foreach( var (q,kx) in rcbList.WithIndex() ){ if(k!=kx)  Q |= q; }
				if( (Q&rcbList[k]) == 0 )  return false;
			}
			return true;
		}

        public IEnumerable<UFish> IEGet_CoverSet_Basic( UFish UFbase, int coverSelecter, bool FinnedFlag ){
            if( FishElementList==null )  yield break;
      
            List<ULogical_Node> HCLst = FishElementList.FindAll( p => coverSelecter.IsHit(p.ID) && UFbase.BaseB81.IsHit(p.b081) );
                        //int n=0; HCLst.ForEach( P=> WriteLine( $"sq:{n++} {P}" ) );
            if(HCLst.Count<sz) yield break;
            
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
                        CannFinB81 |= Q;
                    }
                    usedLK   |= 1<<HCLst[nx].ID;  //house number
                    CoverB81 |= HCLst[nx].b081;        //Bit81
                }

                // case no Fin, BaseSet is covered with CoverSet.
                // case with Fin, the part not covered by CoverSet of BaseSet is Fin.
                UInt128 FinB81 = UFbase.BaseB81 .DifSet(CoverB81);     //(oprator- : difference set)
                if( FinnedFlag != (FinB81.BitCount()>0) ) continue;      //
                UFish UFcover = new( UFbase, usedLK, CoverB81, FinB81, CannFinB81 );
                //==================
                yield return UFcover;
            }
            yield break;
        }
    }  
}