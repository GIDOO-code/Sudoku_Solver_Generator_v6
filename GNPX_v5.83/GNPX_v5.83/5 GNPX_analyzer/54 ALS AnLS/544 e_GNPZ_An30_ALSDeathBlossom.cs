using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Navigation;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Windows.Interop;

namespace GNPX_space{
    public partial class ALSTechGen: AnalyzerBaseV2{

        //DeathBlossom is an algorithm based on the arrangement of ALS.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page53.html
        
        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //...8...4....21...7...7.5981315..9..8.8....4....41.83.5..1.82.646.8...1...236.18..
        //..2956.485.6....9.4...7.5...538...1.2.......6.1...582...1.8...2.9....1.582.4316..
        //285..91.47.3..1...16..24.7........8...2.4.6.7597..84..4...92..3..84....29...8.74.
        //594..26.31.8..6...26..39.8........2...6.5.4.7321..48..4...27..8..38....28...9.76.

        //1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9  for develop(1)
        //1.2956.485.6...29.4.9.7.56..538...1.248....56.17..582.761589432394...185825431679  for develop(2) 
        //285..91.4743..1...169.24.7.6.4....8.8.2.4.6.7597..84.14...928.33.84....292..8.74.  for develop(3,4)

        private bool break_ALS_DeathBlossom=false; //True if the number of solutions reaches the specified number.


        public bool ALS_DeathBlossom()   => ALS_DeathBlossomEx( connected:false );

        public bool ALS_DeathBlossomEx() => ALS_DeathBlossomEx( connected:true );


        public bool ALS_DeathBlossomEx( bool connected=false ){
            break_ALS_DeathBlossom=false;
			PrepareStage();
            if( ALSMan.ALSList==null || ALSMan.ALSList.Count<=2 ) return false;

            ALSMan.QSearch_Cell2ALS_Link();

            for(int sz=2; sz<=4; sz++){     //Size 5 and over ALS DeathBlossom was not found?
				foreach( bool solfound in IE_ALS_DeathBlossomSub( sz, connected:connected) ){
					if( !solfound ) continue;
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
				}
            }
            return false;
        }



        private IEnumerable<bool> IE_ALS_DeathBlossomSub( int sz, bool connected=false ){
            int szM= (connected? sz-1: sz);
            foreach( var StemCell in pBOARD.Where(p=>p.FreeBC==sz) ){                       //Stem Cell

                if(pAnMan.Check_TimeLimit()) yield break;

                List<Link_CellALS> LinkCeAlsLst=ALSMan.LinkCeAlsLst[StemCell.rc];
                if( LinkCeAlsLst==null || LinkCeAlsLst.Count<sz) continue;
                      //  int mx=0;
                      //  LinkCeAlsLst.ForEach( X => WriteLine( $"#{mx++} {X}") );

                int nxt=0, PFreeB=StemCell.FreeB;
                var cmb = new Combination( LinkCeAlsLst.Count ,szM );               //Select szM ALSs in Combination
                while( cmb.Successor(skip:nxt) ){
                    nxt = szM;
                    int FreeB = StemCell.FreeB;     // Concatenated digits of stems(remaining digits)
                    int AFreeB = 0x1FF;       // Common digits for ALS
                    for( int k=0; k<szM; k++ ){
                        nxt = k;
                        var LK = LinkCeAlsLst[cmb.Index[k]];                        //Link[cell-ALS]
                        if( (FreeB&(1<<LK.nRCC)) ==0 ) goto LNxtCmb;               
                        FreeB = FreeB.BitReset(LK.nRCC);                            //nRCC:RCC of stemCell-ALS                      
                        AFreeB &= LK.ALS.FreeB; 
                        if( AFreeB == 0 )  goto LNxtCmb;
                    }

                    // =============== connected=true ===============
                    UInt128 E = new UInt128();
                    if( connected ){
                        if( FreeB.BitCount()!=1 || (FreeB&AFreeB)==0 )  continue;
                        int no  = FreeB.BitToNum();
                        int noB = FreeB;

                        UInt128 Ez = UInt128.Zero;
                        for( int k=0; k<szM; k++ ){
                            var ALS = LinkCeAlsLst[cmb.Index[k]].ALS;
                            var UClst = ALS.UCellLst;
                            foreach( var P in UClst.Where(p=>(p.FreeB&noB)>0) )  Ez = Ez.Set(P.rc);
                        }

                        foreach( var P in ConnectedCells81[StemCell.rc].IEGet_rc().Select(rc=>pBOARD[rc]) ){
                            if( (P.FreeB&noB)==0 ) continue;
                            if( (Ez.DifSet(ConnectedCells81[P.rc])).IsZero() ){ P.CancelB=noB; E=E.Set(P.rc); SolCode=2;  }
                        }
                        if(SolCode<1) continue;
                        
                        var LKCAsol=new List<Link_CellALS>();
                        Array.ForEach( cmb.Index,nx => LKCAsol.Add(LinkCeAlsLst[nx]) );
                        _DeathBlossom_SolResult( LKCAsol, StemCell, no, E, connected );

						yield return true;
                    }



                    // =============== connected=false ===============
                    else if( FreeB==0 && AFreeB>0 ){
                        AFreeB = AFreeB.DifSet(StemCell.FreeB);
                        foreach( var no in AFreeB.IEGet_BtoNo() ){
                            int noB=(1<<no);
                            UInt128 Ez = new UInt128();
                            for( int k=0; k<sz; k++ ){
                                var ALS = LinkCeAlsLst[cmb.Index[k]].ALS;
                                var UClst = ALS.UCellLst;
                                foreach( var P in UClst.Where(p=>(p.FreeB&noB)>0) )  Ez = Ez.Set(P.rc);
                            }

                            foreach( var P in pBOARD.Where(p=>(p.FreeB&noB)>0) ){
                                if( (Ez.DifSet(ConnectedCells81[P.rc])).IsZero() ){ P.CancelB=noB; E=E.Set(P.rc); SolCode=2; }
                            }
                            if( SolCode<1 ) continue;
                        
                            var LKCAsol = new List<Link_CellALS>();
                            Array.ForEach( cmb.Index,nx=> LKCAsol.Add(LinkCeAlsLst[nx]) );
                            _DeathBlossom_SolResult( LKCAsol, StemCell, no, E, connected);

							yield return true;
                        }
                    }
                
                LNxtCmb:
                    continue;
                }
            }
            yield break;
        }

        private void _DeathBlossom_SolResult( List<Link_CellALS> LKCAsol, UCell StemCell, int no, UInt128 E, bool connected=false ){
            string st0 = "ALS Death Blossom";
            if(connected) st0 += "Ex";

            Color cr, crStemFg=Colors.Navy, crStemBg = SolBkCr;//Colors.HotPink;
            StemCell.Set_CellColorBkgColor_noBit(StemCell.FreeB,crStemFg,crStemBg);
            StemCell.Set_CellFrameColor(Colors.Blue);
            string st = $"\r Stem cell: {StemCell.rc.ToRCString()} #{StemCell.FreeB.ToBitStringNZ(9)}";

            bool  overlap = false;
            UInt128 OV = UInt128.Zero;
            int   k=0, noB=(1<<no);

            foreach( var LK in LKCAsol ){
                int noB2 = 1<<LK.nRCC;
                cr = _ColorsLst[++k];
                LK.ALS.UCellLst.ForEach( p=> { 
                    UCell P = pBOARD[p.rc];
                    P.Set_CellColorBkgColor_noBit(noB,AttCr,cr);
                    P.Set_CellColorBkgColor_noBit(noB2,AttCr3,cr);
                    if( OV.IsHit(P.rc) ) overlap=true;
                    OV = OV.Set(P.rc);
                } );
                st += $"\r   -#{(LK.nRCC+1)}-ALS{k} : {LK.ALS.ToStringRCN()}";
            }

            st += $"\r eliminated : { E.ToRCStringComp()} #{no+1}";

            if( overlap ) st0+=" [overlap]";
            if( connected )  st0+=" [connected]";
            st0 = st0.Replace("] [","," );
            Result = st0;
            if( SolInfoB ) ResultLong=st0+st;
        }
    }
}