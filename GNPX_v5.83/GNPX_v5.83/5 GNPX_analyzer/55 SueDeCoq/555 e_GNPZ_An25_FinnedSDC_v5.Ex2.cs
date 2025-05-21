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

        public bool Finned_SueDeCoqEx2( ){
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				fALS.Initialize();
			}
            fALS.Prepare_ALSLink_Man(nPlsB:+2, maxSize:5 );
            if( fALS.ALSList==null || fALS.ALSList.Count<=3 ) return false;
					//if(debugPrint)  fALS.ALSList.ForEach( P => WriteLine( $"\n{P}" ) )

			// ----- << 0 >> -----
				IEnumerable<(UAnLS,int)> IEGet_AnLS_as_Stem( List<UAnLS> ALSList, int Level ){
					foreach( UAnLS Stem in ALSList.Where(q => (q.Level==Level) && (q.Size>=2) ) ){					// << 1 >>   ... define as stem cells(Stem). 			
						foreach( int Stem_houseNo in _IEGet_SingleHouse(Stem) ){
							yield return (Stem,Stem_houseNo);
						}
					}
					yield break;

					IEnumerable<int> _IEGet_SingleHouse( UAnLS als ){
						bool bhbp = false;	// block has been processed
						if( als.houseNo_Row > 0 ){ bhbp |= (als.houseNo_Blk>0); yield return als.houseNo_Row; }
						if( als.houseNo_Col > 0 ){ bhbp |= (als.houseNo_Blk>0); yield return als.houseNo_Col; }
						if( !bhbp && als.houseNo_Blk>0)  yield return als.houseNo_Blk;
						yield break;
					}
				}
			foreach( var (Stem, Stem_houseNo) in IEGet_AnLS_as_Stem( fALS.ALSList, Level:2 ) ){
						//if(debugPrint)  WriteLine( $"\n\n@@@  Stem_houseNo:{Stem_houseNo}\n Stem:{Stem}" );

				// ----- << 1 >> -----
					IEnumerable<(UAnLS,int)> IEGet_ALS_as_Leaf( List<UAnLS> ALSList, UAnLS Stem ){
						UInt128	Stem_connected = Stem.connectedB81;
						foreach( UAnLS ALS_Q in ALSList.Where( q =>
								( q.Level==1 )												// is ALS (Level=1)
									// The following are the necessary conditions for the existence of RCC. Not a sufficient condition.
								&& ( (q.bitExp & Stem.bitExp) == UInt128.Zero )				// no overlap with Stem.
								&& ( (q.bitExp & Stem_connected) != UInt128.Zero )			// two or more cells in the connected region of Stem
								&& ( (q.FreeB & Stem.FreeB) != 0 )							// one or more digits in common with Stem.
							) ){

							var RCC_SvsQ = fALS.Get_AlsAlsRcc( Stem, ALS_Q );
							if( RCC_SvsQ.BitCount() <= 0 )  continue;
							yield return (ALS_Q, RCC_SvsQ);
						}
						yield break;
					}
				foreach( var (ALS_A,RCC_SvsA) in IEGet_ALS_as_Leaf( fALS.ALSList, Stem) ){
					int RCC_SvsA_BC = RCC_SvsA.BitCount();
					if( RCC_SvsA_BC != 2  )  continue;
							//if(debugPrint)  WriteLine( $"\n\n===  ALS_A:{ALS_A}\n RCC_SvsA{RCC_SvsA.ToBitString(9)}" );
					UInt128[] ALS_A_ConnectedCellsB9 = ALS_A.Get_ConnectedCellsB9(RCC_SvsA);

					// ----- << 2 >> -----
					foreach( var (ALS_B,RCC_SvsB) in IEGet_ALS_as_Leaf( fALS.ALSList, Stem) ){
						if( (ALS_A.bitExp & ALS_B.bitExp).IsNotZero() )  continue;
						//if( ALS_A.ID > ALS_B.ID )  continue;		// Exclude symmetric solutions of ALS
						if( RCC_SvsB.BitCount() != 1 )  continue;

						bool RCC_Cells_Cond = (Stem.FreeB.BitCount() - 2) == Stem.Size; //RCC and cell number conditions.
						if( !RCC_Cells_Cond )  continue;
						if( (RCC_SvsA & RCC_SvsB) > 0 ) continue;						// ALS_A and ALS_B do not overlap

						// search Fin connected to ALS_B.
						int noBFin = (Stem.FreeB & ALS_B.FreeB).DifSet(RCC_SvsA | RCC_SvsB );
						if( noBFin == 0 )  continue;

/*
							string st = $"@#- Stem.ID:{Stem.ID}  ALS_A.ID:{ALS_A.ID}  ALS_B.ID:{ALS_B.ID}";
							st += $"\n\n Stem:{Stem}";
							st += $"\n\n ALS_A:{ALS_A}\nRCC_SvsA:{RCC_SvsA.ToBitString(9)}";
							st += $"\n\n ALS_B:{ALS_B}\nRCC_SvsB:{RCC_SvsB.ToBitString(9)}";
							WriteLine( st );
*/

						UInt128   rcBPattern = Stem.bitExp | ALS_A.bitExp | ALS_B.bitExp;
						foreach( int noF in noBFin.IEGet_BtoNo(9) ){
							UInt128 rcBbaseSet = Stem.BitExp9[noF] | ALS_B.BitExp9[noF];
							UInt128 rcBCoverSet = rcBbaseSet.Aggregate_ConnectedAnd() & FreeCell81b9[noF];
							rcBCoverSet = rcBCoverSet.DifSet(rcBPattern);
							if( rcBCoverSet.IsZero() ) continue;

							// ----- Solution Found -----------------------------------------------------
							bool solFound = Finned_SDCEx2_SolResult( Stem, ALS_A, RCC_SvsA, ALS_B, RCC_SvsB, noF, rcBCoverSet );

							if( solFound ){
								if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
							
								//if(debugPrint)  Debug_StreamWriter( debug_Result );
								solFound=false;
							}
						}
					}
				}
			}
			return false;
		}

		private bool Finned_SDCEx2_SolResult( UAnLS Stem, UAnLS ALS_A, int RCC_SvsA, UAnLS ALS_B, int RCC_SvsB, int noF, UInt128 rcBCoverSet ){
			string stCan(UCell U) => U.rc.ToRCString()+"#"+U.CancelB.ToBitStringN(9);
			bool solFound = true;

			int noFB = 1<<noF;
			foreach( UCell P in rcBCoverSet.IEGet_UCell(pBOARD) ){ P.CancelB |= noFB; }
			rcBCoverSet.IE_SetNoBBgColor( pBOARD, 0x1FF, AttCr, SolBkCr2 );

			SolCode=2;
			Color SolBkCrL = Colors.Coral;
			Color SolBkCrR = Colors.Pink;
			Color SolBkCr10 = SolBkCrL;
			Color SolBkCr20 = SolBkCrL*0.5f+SolBkCrR*0.5f;

			Stem.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr);
			ALS_A.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr10);
			ALS_B.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr20);

			string st_Stem = Stem.bitExp.ToRCStringComp(Stem.FreeB);
			string st_ALS_A    = ALS_A.bitExp.ToRCStringComp(ALS_A.FreeB);
			string st_ALS_B    = ALS_B.bitExp.ToRCStringComp(ALS_B.FreeB);

			string st_exclude  = pBOARD.Where(p=>p.CancelB>0).Aggregate( "", (p,q)=> p+" "+stCan(q) );
			Result     = $"Finned_SueDeCoqEx2 {st_Stem.Replace(" #","#")} / {st_ALS_A.Replace(" #","#")} / {st_ALS_B.Replace(" #","#")}";

			st_ALS_A += $"  RCC;#{RCC_SvsA.ToBitStringN(9)}";
			st_ALS_B += $"  RCC;#{RCC_SvsB.ToBitStringN(9)}";
			string st2 = $"Finned_SueDeCoqEx2\n\n   stem AnLS : {st_Stem}\n     1st ALS : {st_ALS_A}";
			st2 += $"\n     2nd ALS : {st_ALS_B}\n\n      exclude:{st_exclude}";

#if DEBUG
			st2 += $"\n\n     ... ID: {Stem.ID} / {ALS_A.ID} / {ALS_B.ID}";
			debug_Result = $"ID: {Stem.ID} / {ALS_A.ID} / {ALS_B.ID}  {Result}";
#endif
			ResultLong = st2;
			return solFound;
		}
	}
}