using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.Windows.Media;


namespace GNPX_space{
    public partial class ALSTechGen: AnalyzerBaseV2{

        //Algorithm using ALS and RCC((Restricted Common Candidate).
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page50.html

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/   
		//.2...783..47.2...13..1....7....38.15...5.4...58.79....6....2..82...8.57..793...6.   //**
        //2.548...9.9..3..2...7.923.4..8.....2541...6879.....1..4.961.2...8..7..9.7...584.6
        //87........9.81.65....79...8.....67316..5.1..97124.....3...57....57.48.1........74
        //.9..4..6.4..15...2..6..91....4....7.36.....15.8....3....82..4..9...34..1.4..8..3.
			
		//3248615976579438219185273642.143....5.32.6...4..7.....1..6.....8..1746..7..39..1. //Q1

		internal string fName_debug = "ALS_XZ.txt";
		internal string debug_Result="";
		internal void Debug_StreamWriter( string LRecord ){
//#if DEBUG
			//if(!debugPrintFSDC)  return;
            using( var fpW=new StreamWriter(fName_debug,true,Encoding.UTF8) ){
                fpW.WriteLine(LRecord);
            }
//#endif
		}

        private bool break_ALS_XZ=false; //True if the number of solutions reaches the specified number.
        public bool ALS_XZ( ){
			PrepareStage();
            if( ALSMan.ALSList==null || ALSMan.ALSList.Count<=2 ) return false;
        
			szMax = 0;
		    for(int sz=4; sz<=18; sz++ ){
				foreach( bool solFound in IE_ALSXZsub(sz) ){
					if( sz > szMax )  break;
					if( !solFound )   continue;
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
				}
            }
            return false;
        }

		// =============== _ALSXZsub ==============================================
		private int szMax; 
        private IEnumerable<bool> IE_ALSXZsub( int sz ){
            break_ALS_XZ=false;

			szMax = 0;
            var cmb = new Combination(ALSMan.ALSList.Count,2);
            while( cmb.Successor() ){                        //Select two ALSs
                if( pAnMan.Check_TimeLimit() )  yield break;

                UAnLS UA = ALSMan.ALSList[ cmb.Index[1] ];
                UAnLS UB = ALSMan.ALSList[ cmb.Index[0] ];
				if( (UA.bitExp & UB.bitExp).IsNotZero() )  continue;

				int szP = UA.Size+UB.Size;
				szMax = Max(szMax, szP);
				if( szP != sz ) continue;

                //=======================================================

                int RCC = ALSMan.Get_AlsAlsRcc( UA, UB );          //Common numbers, House contact, Without overlap
                if( RCC==0 ) continue;               

                if( RCC.BitCount()==1 ){        //===== Singly Linked =====
                    int othersB = (UA.FreeB&UB.FreeB).DifSet(RCC); //Eliminate candidate digit. digits are included in both UA and UB.
                    if( othersB>0 ){
						var singlyB = _ALSXZ_IsSinglyLinked(UA,UB,RCC,othersB);	// 1:singly  2:doubly  0:nothing
						if( singlyB ){
							SolCode = 2;
							ALSXZ_SolResult( RCC, UA, UB );
							yield return true;
						}
					}
                }
                else if( RCC.BitCount()==2 ){   //===== Doubly Linked =====
                    if( _ALSXZ_DoublyLinked( UA, UB, RCC) ){
                        SolCode=2;
                        ALSXZ_SolResult( RCC, UA, UB );
                    }
                }
            }
            yield break;
        }

        private void ALSXZ_SolResult( int RCC, UAnLS UA, UAnLS UB ){
            string st = "ALS-XZ "+((RCC.BitCount()==1)? "(Singly Linked)": "(Doubly Linked)");
            Result = st;
            
            string st2="", st3="";
            if( SolInfoB ){
				Color SolBkCrL = Colors.Coral;
				Color SolBkCrR = Colors.Pink;
				Color SolBkCr1 = SolBkCrR * 0.8f;

                foreach( var P in UA.UCellLst ) P.Set_CellColorBkgColor_noBit(P.FreeB,AttCr3,SolBkCr);
                foreach( var P in UB.UCellLst ) P.Set_CellColorBkgColor_noBit(P.FreeB,AttCr3,SolBkCr1);

				foreach( var P in pBOARD.Where(p=>p.CancelB>0) )  P.Set_CellColorBkgColor_noBit(P.FreeB,AttCr,SolBkCr2);

                st2 += "\r ALS1: "+UA.ToStringRCN();
                st2 += "\r ALS2: "+UB.ToStringRCN();
                st2 += "\r  RCC: #"+RCC.ToBitStringN(9);

                st3 += $" ALSs:{UA.ToStringRC()} / {UB.ToStringRC()} RCC:#{RCC.ToBitStringN(9)}";
                Result = ($"{st}{st3}").Replace(" Linked","");
			
#if DEBUG
				st2 += $"\n\n... ID: {UA.ID} / {UB.ID}";
				debug_Result = $"ID: {UA.ID} / {UB.ID}  {Result}";						
					//if(debugPrint)  Debug_StreamWriter( debug_Result );
#endif
				ResultLong = $"{st}{st2}";
            }
        }       
  
		private bool _ALSXZ_IsSinglyLinked( UAnLS UA, UAnLS UB, int RCC, int othersB ){   
        // *=*=* SinglyLinked subroutine *=*=*
        //othersB : Common digits for UA and UB other than RCC. Bit representation.

            // Suppose that two ALSs have RCC(digit x).
            // And, let z be the digit contained in both ALS different from RCC.
	        // digit z outside the ALS and associated with all z in both ALS can be eliminated from the candidate.
	        // If z is true, both ALSs are changed to LockedSet, and both ALSs include RCC.
            bool solF=false;
            foreach( var no in othersB.IEGet_BtoNo() ){ 
                int noB = 1<<no; 
                UInt128 ELM128 = FreeCell81b9[no].DifSet( UA.bitExp | UB.bitExp );
                foreach( var P in UA.UCellLst.Where(p=> (p.FreeB&noB)>0) )  ELM128 = ELM128 & ConnectedCells81[P.rc];
                foreach( var P in UB.UCellLst.Where(p=> (p.FreeB&noB)>0) )  ELM128 = ELM128 & ConnectedCells81[P.rc];

                if( ELM128.IsNotZero() ){
                    foreach( var rc in ELM128.IEGet_rc() ){ pBOARD[rc].CancelB |= noB; solF = true; }
                }
            }

            return solF;
        }

        private bool _ALSXZ_DoublyLinked( UAnLS UA, UAnLS UB, int RCC ){              // *=*=* DoublyLinked subroutine *=*=*
            UInt128 B81inALS = UInt128.Zero; //Covered cells
            bool solF=false;

            //----- RCC -----
            // A digit belonging to the same house as non-ALS RCC can be eliminated.
            // If this applies to RCC-1, then the two ALS are LockedSet and
            // the RCC-2 is eliminated from both ALS.
            foreach( int no in RCC.IEGet_BtoNo() ){
                int noB=1<<no;
                UInt128 B81inALS128 = UInt128.Zero;
                foreach( var uc in UA.UCellLst.Where(p=>(p.FreeB&noB)>0) ) B81inALS128 = B81inALS128.Set(uc.rc);
                foreach( var uc in UB.UCellLst.Where(p=>(p.FreeB&noB)>0) ) B81inALS128 = B81inALS128.Set(uc.rc);
                UInt128 B81outside128 = FreeCell81b9[no] .DifSet(UA.bitExp | UB.bitExp);    //Cells outside ALS(UA,UB)

                foreach( var rc in B81outside128.IEGet_rc() ){
                    if( (B81inALS128.DifSet( ConnectedCells81[rc] )).IsZero() ){   // no cells linked with all #no in ALS.
                        pBOARD[rc].CancelB|=noB; solF=true;
                    }
                }
            }

            //----- ALS element digit other than RCC -----
            // For a element of ALS digited z(different from RCC),
		    // z outside the ALS and related to all z in the ALS can be eliminated from the candidate.
		    // If this is true, that ALS is a LockedSet and two RCCs belong to this ALS.
		    // In the other ALS, there are n-1 candidate digits in n cells, and ALS collapses.

            int nRCC = UB.FreeB.DifSet(RCC);                
            foreach( int no in nRCC.IEGet_BtoNo() ){    // a digit other than RCC
                int noB = 1<<no;
                B81inALS = UInt128.Zero;

                foreach( var uc in UB.UCellLst.Where(p=>(p.FreeB&noB)>0) ) B81inALS |= UInt128.One<<uc.rc; //Cells#no inside UB
                UInt128 B81inALS128 = UInt128.Zero;
                foreach( var uc in UB.UCellLst.Where(p=>(p.FreeB&noB)>0) ) B81inALS128 |= UInt128.One<<uc.rc; //Cells#no inside UB
                UInt128 B81outside128 = FreeCell81b9[no] .DifSet( UA.bitExp | UB.bitExp );                     //Cells#no outside UA & UB
           
                foreach( var rc in B81outside128.IEGet_rc() ){
                    if( (B81inALS128. DifSet(ConnectedCells81[rc])).IsZero() ){   //If rc#no is true, eliminate all UB#no.
                        pBOARD[rc].CancelB|=noB; solF=true;           //Therefore rc#no is eliminated.
                    }
                }
            }

            return solF;
        }
    }
}
 