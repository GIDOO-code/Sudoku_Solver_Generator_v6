using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Windows.Interop;
using System.Windows.Media;
using GIDOO_space;

namespace GNPX_space{
    public partial class ALSTechGen: AnalyzerBaseV2{

        //Algorithm using ALS and RCC((Restricted Common Candidate).
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page51.html

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //8....5..7.7.1.8.6...6.9.8..64.9.7.3...3...7...9.8.2.46..9.8.4...1.5.4.2.4..3....1
        //4..1....39.7.3.54..539..7....5...3..2963.7154..8...6....4..389..39.4.2.56....9..7

		//471258369935647281628391457592813746316724895784569123859172634267435918143986572 ...
		//47...83.993..4728.628.9.4.7.9.81..4..167.489..84.69.2.85..7...4.6.4.5..8143986572

		private bool debug_with_ID=false;

        public bool ALS_XY_Wing( ){
		
			PrepareStage();
            if(ALSMan.ALSList==null || ALSMan.ALSList.Count<=2) return false;

            for(int szT=4; szT<15; szT++ ){    //Search in descending order of the total size of 3 ALS
                foreach( var _ in IE_ALS_XY_Wing_Sub(szT) ){
                    if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                }
            }
            return false;
        }

        private IEnumerable<bool> IE_ALS_XY_Wing_Sub( int szT ){

            //(ALS sorted by size)    
            foreach( var Ustem in ALSMan.ALSList.Where(p=>p.Size<=szT-2) ){  // ===== Stem ALS =====
                int szS=szT-Ustem.Size;

                UAnLS UA, UB, UApre=null;

                // RCC(Restricted Common Candidate)
                // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page26.html
                int nxt =0, RccAC=-1, RccBC=-1;

                var cmb = new Combination(ALSMan.ALSList.Count,2);       
                while(cmb.Successor(skip:nxt)){
                    if( pAnMan.Check_TimeLimit() ) yield break; 
                    
                    // ----- UA connects with Stem ALS with RCC?
                    nxt=0;
                    UA = ALSMan.ALSList[ cmb.Index[0] ];
                    if( UA==Ustem || UA.Size>szS-1 ) continue;
					if( (Ustem.bitExp & UA.bitExp).IsNotZero() )  continue;
                    if( UA!=UApre ){
                        RccAC = ALSMan.Get_AlsAlsRcc(UA,Ustem);
                        if( RccAC.BitCount()!=1 ) continue;             // Singly-RCC connected?
                        UApre=UA;
                    }

                    // ----- UB connects with Stem ALS with RCC?
                    nxt=1;     
                    UB = ALSMan.ALSList[ cmb.Index[1] ];
                    if( UB.Size>(szS-UA.Size))  continue;  // Skip using "Sort by size"
					if( (Ustem.bitExp & UB.bitExp).IsNotZero() )  continue;  // Overlap
               
                    if( UB==Ustem || UB.Size!=(szS-UA.Size) )  continue;
                    RccBC = ALSMan.Get_AlsAlsRcc(UB,Ustem);              // RCC(Restricted Common Candidate)
                    if( RccBC.BitCount()!=1 ) continue;                  // Singly-RCC connected?
                    if( RccAC==RccBC ) continue;                         // Different RCCs?

                    // ----- Candidate Digits -----
                    int CandidateDigitsB = (UA.FreeB&UB.FreeB).DifSet(RccAC|RccBC); // Common and non-RCC
                    if( CandidateDigitsB==0 ) continue;


                    foreach( var no in CandidateDigitsB.IEGet_BtoNo() ){
                        int noB = (1<<no);

                        // ... inside
                        UInt128 UE = UInt128.Zero;
                        foreach( var P in UA.UCellLst.Where(p=>(p.FreeB&noB)>0)) UE = UE.Set(P.rc);
                        foreach( var P in UB.UCellLst.Where(p=>(p.FreeB&noB)>0)) UE = UE.Set(P.rc);
                    
                        // ... outside
                        UInt128 TBD = FreeCell81b9[no] .DifSet( UA.bitExp | UB.bitExp | Ustem.bitExp );
                        if( TBD.IsZero() )  continue;
                        
                        UInt128 elmB = UInt128.Zero;
                        foreach( var rc in TBD.IEGet_rc() ){
                            if( (UE.DifSet(ConnectedCells81[rc])).IsNotZero() ) continue; //cell-rc and all digits inside are connected
                            elmB = elmB.Set(rc);
                            pBOARD[rc].CancelB=noB;
                            SolCode=2;
                        }
                    
                        if( elmB.IsNotZero() ){ //===== ALS XY-Wing found =====
                            ALS_XY_Wing_SolResult( UA, UB, Ustem, RccAC, RccBC, no, elmB );
                            yield return (SolCode>0);
                        }
                    }
                }
            }
            yield break;
        }  
        private void ALS_XY_Wing_SolResult( UAnLS UA, UAnLS UB, UAnLS Ustem, int RccAC, int RccBC, int noX, UInt128 elmB ){
            string st = "ALS XY-Wing ";
            string st2 = $"Eliminated: {elmB.ToRCStringComp()} #{noX+1}";          
        
            if( SolInfoB ){            
                UA.UCellLst.IE_SetNoBBgColor( pBOARD, RccAC, AttCr, SolBkCr );
                UB.UCellLst.IE_SetNoBBgColor( pBOARD, RccBC, AttCr, SolBkCr2 );
                Ustem.UCellLst.IE_SetNoBBgColor( pBOARD, RccAC|RccBC, AttCr, SolBkCr3 );
       
                st += "\r ALS Stem: "+Ustem.ToStringRCN();
															if(debug_with_ID)  st += $"  [ID:{Ustem.ID}]";
                st += "\r ALS    A: "+UA.ToStringRCN();
															if(debug_with_ID)  st += $"  [ID:{UA.ID}]";
                st += "\r ALS    B: "+UB.ToStringRCN();
															if(debug_with_ID)  st += $"  [ID:{UB.ID}]";

                st += "\r  RCC Stem-A: #"+RccAC.ToBitStringN(9);
                st += "\r  RCC Stem-B: #"+RccBC.ToBitStringN(9);


                ResultLong = $"{st}\n {st2}";
            }
            Result = "ALS XY-Wing " + st2;
        }
    }
}