using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using System.IO;

using GIDOO_space;
using System.Collections;
using System.Text;
using System.Windows.Controls;

namespace GNPX_space{

    // SueDeCoq
    // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page45.html

    //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
    //.4...1.286..5....7..7.46...7.3..98..9.......2..12..3.9...95.1..1....2..559.1...7.

    //87........9.81.65....79...8.....67316..5.1..97124.....3...57....57.48.1........74
    //87.....9..9381.657...79...8.4.286731638571..97124398653.415798..57.48.1..8....574  //for develop

    //.2...3..4.4....25.6...243.8256..8....8..9..2....2..4868.463...2.63....4.9..7...6.
    //.2...36.434..6.25.69..243.82564.8.3.48.396.25.3925.48687463.5.2.63..2.4.912745863  //for develop

    //hodoku
    //3248615976579438219185273642.143....5.32.6...4..7.....1..6.....8..1746..7..39..1. //Q1
    //641..8329873291645592..4187.8..2.4.1.....92.3...41.8.6......73..6.9..51...714.968 //Q2
    //82..6.....6.8...2...32..568641...37.53......4.87...6..4563.97..37...1.......5..3. //Q3 -> Extend (step3 )		code error
    //329...5.........29.6.....3.9342..765716..5482258764..3.73....5.1954.3.7...25..3.. //Q4 -> Extend
	
    public partial class AnLSTechGen: AnalyzerBaseV2{
		internal bool		debugPrintFSDC = false;

        public bool Franken_SDCEx3( ){
			
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				fALS.Initialize();
			}
            fALS.Prepare_ALSLink_Man(nPlsB:+3, maxSize:5 );
            if( fALS.ALSList==null || fALS.ALSList.Count<=3 ) return false;
					//if(debugPrintFSDC)  fALS.ALSList.ForEach( P => WriteLine( $"\n{P}" ) )

			 // ----- << 0 >> -----
				IEnumerable<(UAnLS,int)> IEGet_AnLS_as_Stem( List<UAnLS> ALSList, int Level ){
					foreach( UAnLS Stem in ALSList.Where(q => (q.Level>=Level) && (q.Size>=2) ) ){					// << 1 >>   ... define as stem cells(Stem). 			
						foreach( int Stem_houseNo in _IEGet_SingleHouse(Stem) ){
							yield return (Stem,Stem_houseNo);
						}
					}
					yield break;

					IEnumerable<int> _IEGet_SingleHouse( UAnLS als ){
						bool bhbp = false;	// block has been processed
						if( als.houseNo_Row > 0 ){ bhbp |= (als.houseNo_Blk>0); yield return als.houseNo_Row; }
						if( als.houseNo_Col > 0 ){ bhbp |= (als.houseNo_Blk>0); yield return als.houseNo_Col; }
						if( !bhbp && als.houseNo_Blk>0 )  yield return als.houseNo_Blk;
						yield break;
					}
				}
			foreach( var (Stem, Stem_houseNo) in IEGet_AnLS_as_Stem( fALS.ALSList, Level:3 ) ){

				// ----- << 1 >> -----
					IEnumerable<(UAnLS,int)> IEGet_ALS_as_Leaf( List<UAnLS> ALSList, UAnLS Stem, UAnLS ALS_P=null, int RCC_P=0 ){
						UInt128	Stem_connected = Stem.connectedB81;
						foreach( UAnLS ALS_Q in ALSList.Where( q =>
								( q.Level==1 )												// is ALS (Level=1)
							// The following are the necessary conditions for the existence of RCC. Not a sufficient condition.
								&& ( (q.bitExp & Stem.bitExp) == UInt128.Zero )				// no overlap with Stem.
								&& ( (q.bitExp & Stem_connected) != UInt128.Zero )			// two or more cells in the connected region of Stem
								&& ( (q.FreeB & Stem.FreeB) != 0 )							// one or more digits in common with Stem.
							) ){

							var RCC_SvsQ = fALS.Get_AlsAlsRcc( Stem, ALS_Q );
							if( RCC_SvsQ.BitCount() < 2 )  continue;
							
							yield return (ALS_Q, RCC_SvsQ);
						}
						yield break;
					}
				foreach( var (ALS_A,RCC_SvsA) in IEGet_ALS_as_Leaf( fALS.ALSList, Stem) ){
					UInt128[] ALS_A_ConnectedCellsB9 = ALS_A.Get_ConnectedCellsB9(RCC_SvsA);


					// ----- << 2 >> -----
					foreach( var (ALS_B,RCC_SvsB) in IEGet_ALS_as_Leaf( fALS.ALSList, Stem) ){
						if( ALS_A.ID > ALS_B.ID )  continue;		// Exclude symmetric solutions of ALS
						UInt128[] ALS_B_ConnectedCellsB9 = ALS_B.Get_ConnectedCellsB9(RCC_SvsB);
						foreach( int no in RCC_SvsB.IEGet_BtoNo(9) ){
							if( (ALS_B.BitExp9[no] & ALS_A_ConnectedCellsB9[no]) > UInt128.Zero ) continue;
						}
						if( (RCC_SvsA & RCC_SvsB)>0 )  continue;

						// ----- << 3 >> -----
						foreach( var (ALS_C,RCC_SvsC) in IEGet_ALS_as_Leaf( fALS.ALSList, Stem) ){
							if( ALS_B.ID > ALS_C.ID )  continue;		// Exclude symmetric solutions of ALS

							{ // *** Conditions for establishing SDC ***
								if( (RCC_SvsB & RCC_SvsC)>0 | (RCC_SvsC & RCC_SvsA)>0 ) continue;
							  
								foreach( int no in RCC_SvsC.IEGet_BtoNo(9) ){
									if( (ALS_C.BitExp9[no] & ALS_A_ConnectedCellsB9[no]) > UInt128.Zero ) continue;
									if( (ALS_C.BitExp9[no] & ALS_B_ConnectedCellsB9[no]) > UInt128.Zero ) continue;
								}

								bool RCC_Cells_Cond = (Stem.FreeB.BitCount() - 3) == Stem.Size;
								if( !RCC_Cells_Cond )  continue;
							} // ---------------------------------------
							
							// ----- Solution Found -----------------------------------------------------
							bool solFound = Franken_SDCEx3_SolResult( Stem, ALS_A,RCC_SvsA, ALS_B,RCC_SvsB, ALS_C,RCC_SvsC );
							if( solFound ){
								if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
							
								if(debugPrintFSDC)  Debug_StreamWriter( debug_Result );
								solFound=false;
							}
						}
					}
				}
			}
			return false;
		}

		private bool Franken_SDCEx3_SolResult( UAnLS Stem, UAnLS ALS_A,int RCC_SvsA, UAnLS ALS_B,int RCC_SvsB, UAnLS ALS_C,int RCC_SvsC ){
			string stCan(UCell U) => U.rc.ToRCString()+"#"+U.CancelB.ToBitStringN(9);
			bool solFound = false;

			int RCC_AB = ALS_A.FreeB | ALS_B.FreeB;
			UInt128 Candidate_A = Stem.connectedB81 | ALS_A.connectedB81 | ALS_B.connectedB81 | ALS_C.connectedB81;
			Candidate_A = Candidate_A.DifSet(Stem.bitExp | ALS_A.bitExp | ALS_B.bitExp | ALS_C.bitExp);
			UInt128 UCatt = new();
			foreach( int no in RCC_AB.IEGet_BtoNo(9) ){
				UInt128 exludeCandidate = Stem.BitExp9[no] | ALS_A.BitExp9[no] | ALS_B.BitExp9[no] | ALS_C.BitExp9[no];
				int noB = 1<<no;
				foreach( UCell P in Candidate_A.IEGet_UCell(pBOARD) ){
					if( (P.FreeB&noB) > 0 ){
						if( exludeCandidate.DifSet(ConnectedCells81[P.rc]) ==0 ){
							P.CancelB |= noB; UCatt=UCatt.Set(P.rc);  solFound=true;
						}
					}
				}
			}
			UCatt.IE_SetNoBBgColor( pBOARD, RCC_SvsA|RCC_SvsB, AttCr, SolBkCr2 );

			if( solFound ){
				SolCode=2;
				Color SolBkCrL = Colors.Coral;
				Color SolBkCrR = Colors.Pink;
				Color SolBkCr1 = SolBkCrL;
				Color SolBkCr2 = SolBkCrL*0.5f+SolBkCrR*0.5f;
				Color SolBkCr3 = SolBkCrR*0.8f;

				Stem.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr);
				ALS_A.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr1);
				ALS_B.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr2);
				ALS_C.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr3);

				string st_Stem = Stem.bitExp.ToRCStringComp(Stem.FreeB);
				string st_ALS_A    = ALS_A.bitExp.ToRCStringComp(ALS_A.FreeB);
				string st_ALS_B    = ALS_B.bitExp.ToRCStringComp(ALS_B.FreeB);
				string st_ALS_C    = ALS_C.bitExp.ToRCStringComp(ALS_B.FreeB);
				string st_exclude  = pBOARD.Where(p=>p.CancelB>0).Aggregate( "", (p,q)=> p+" "+stCan(q) );
				Result    = $"Franken_SDCEx3 {st_Stem.Replace(" #","#")}" +
						    $"/ {st_ALS_A.Replace(" #","#")} / {st_ALS_B.Replace(" #","#")} / {st_ALS_C.Replace(" #","#")}";

				st_ALS_A += $"  RCC;#{RCC_SvsA.ToBitStringN(9)}";
				st_ALS_B += $"  RCC;#{RCC_SvsB.ToBitStringN(9)}";
				st_ALS_C += $"  RCC;#{RCC_SvsC.ToBitStringN(9)}";
				string st2 = $"Franken_SDCEx3\n\n   stem AnLS : {st_Stem}\n     1st ALS : {st_ALS_A}";
				st2 += $"\n     2nd ALS : {st_ALS_B}\n     3rd ALS : {st_ALS_C}\n\n  exclude:{st_exclude}";

#if DEBUG
				st2 += $"\n\n... ID: {Stem.ID} / {ALS_A.ID} / {ALS_B.ID} / {ALS_C.ID}";
				debug_Result = $"ID: {Stem.ID} / {ALS_A.ID} / {ALS_B.ID} / {ALS_C.ID}  {Result}";
#endif
				ResultLong = st2;


			}
			return solFound;
		}
    }
}