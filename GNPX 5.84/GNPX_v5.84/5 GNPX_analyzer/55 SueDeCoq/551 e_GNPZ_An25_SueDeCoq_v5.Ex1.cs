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
	    
	//Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
    //8.9..3..4.24...61..7...6.297.2.4.......3.2.......5.1.225.1...4..47...83.1..7..2.5
    //..3..4...1.86539...5.7...83..6.37.9.7..4....54.....72.....9.51...9..6...8..3..26.

    //hodoku
    //3248615976579438219185273642.143....5.32.6...4..7.....1..6.....8..1746..7..39..1. //Q1
    //641..8329873291645592..4187.8..2.4.1.....92.3...41.8.6......73..6.9..51...714.968 //Q2 
    //82..6.....6.8...2...32..568641...37.53......4.87...6..4563.97..37...1.......5..3. //Q3 -> Extend (step3 )		code error
    //329...5.........29.6.....3.9342..765716..5482258764..3.73....5.1954.3.7...25..3.. //Q4 -> Extend

    public partial class AnLSTechGen: AnalyzerBaseV2{

        public bool SueDeCoqEx1( ){			
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				fALS.Initialize();
			}
            fALS.Prepare_ALSLink_Man(nPlsB:+2, maxSize:5 );
            if( fALS.ALSList==null || fALS.ALSList.Count<=3 ) return false;

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
			foreach( var (Stem, Stem_houseNo) in IEGet_AnLS_as_Stem( fALS.ALSList, Level:1 ) ){

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
							if( RCC_SvsQ.BitCount() < 1 )  continue;
							yield return (ALS_Q, RCC_SvsQ);
						}
						yield break;
					}
				foreach( var (ALS_A,RCC_SvsA) in IEGet_ALS_as_Leaf( fALS.ALSList, Stem) ){
					if( Stem.ID < ALS_A.ID )   continue;		// exclude symmetric solutions.
					if( RCC_SvsA.BitCount() < 2 )  continue;

					UInt128[] ALS_A_ConnectedCellsB9 = ALS_A.Get_ConnectedCellsB9(RCC_SvsA);
				

					bool RCC_Cells_Cond = (Stem.FreeB.BitCount() - 1) == Stem.Size; //RCC and cell number conditions.
					if( !RCC_Cells_Cond )  continue;
							
					// ----- Solution Found -----------------------------------------------------
					bool solFound = SDCEx1_SolResult( Stem, ALS_A, RCC_SvsA);
					if( solFound ){
						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
							
						////if(debugPrint)  Debug_StreamWriter( debug_Result );
						solFound=false;
					}
			}
			}
			return false;
		}

		private bool SDCEx1_SolResult( UAnLS Stem, UAnLS ALS_A, int RCC_SvsA ){
			string stCan(UCell U) => U.rc.ToRCString()+"#"+U.CancelB.ToBitStringN(9);
			bool solFound = false;

			int RCC_AB = ALS_A.FreeB;
			UInt128 Candidate_A = Stem.connectedB81 | ALS_A.connectedB81;
			Candidate_A = Candidate_A.DifSet(Stem.bitExp | ALS_A.bitExp);
			UInt128 UCatt = new();
			foreach( int no in RCC_AB.IEGet_BtoNo(9) ){
				UInt128 exludeCandidate = Stem.BitExp9[no] | ALS_A.BitExp9[no];
				int noB = 1<<no;
				foreach( UCell P in Candidate_A.IEGet_UCell(pBOARD) ){
					if( (P.FreeB&noB) > 0 ){
						if( exludeCandidate.DifSet(ConnectedCells81[P.rc]) ==0 ){
							P.CancelB |= noB; UCatt=UCatt.Set(P.rc);  solFound=true;
						}
					}
				}
			}
			UCatt.IE_SetNoBBgColor( pBOARD, RCC_SvsA, AttCr, SolBkCr2 );

			if( solFound ){
				SolCode=2;
				Color SolBkCrL = Colors.Coral;
				Color SolBkCrR = Colors.Pink;
				Color SolBkCr1 = SolBkCrR * 0.8f;

				Stem.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr);
				ALS_A.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr1);

				string st_Stem = Stem.bitExp.ToRCStringComp(Stem.FreeB);
				string st_ALS_A    = ALS_A.bitExp.ToRCStringComp(ALS_A.FreeB);
				string st_exclude  = pBOARD.Where(p=>p.CancelB>0).Aggregate( "", (p,q)=> p+" "+stCan(q) );
				Result     = $"SueDeCoqEx1 {st_Stem.Replace(" #","#")} / {st_ALS_A.Replace(" #","#")}";
				st_ALS_A += $"  RCC;#{RCC_SvsA.ToBitStringN(9)}";
				string st2 = $"SueDeCoqEx1\n\n   stem AnLS : {st_Stem}\n     1st ALS : {st_ALS_A}";
				st2 += $"\n\n  exclude:{st_exclude}";

#if DEBUG
				st2 += $"\n\n... ID: {Stem.ID} / {ALS_A.ID}";
				debug_Result = $"ID: {Stem.ID} / {ALS_A.ID}  {Result}";
#endif
				ResultLong = st2;


			}
			return solFound;
		}
    }
}