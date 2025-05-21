using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;
using System.IO;

using GIDOO_space;

#if false
namespace GNPXcore{

	//9....3...54......1...2..6....9.3..4..2.1..5.77....8.6..7.........54..71.8....6..5
	//967813254542697831183245679659732148328164597714958362476521983235489716891376425

	//https://www20.big.or.jp/~morm-e/puzzle/techniques/number_place/aligned_pair_exclusion/index.html

	// https://www.sudokuwiki.org/Aligned_Pair_Exclusion
	// Aligned Pair Exclusion - Type 1 
	//.9.....4..3..4.7.....67...32..9..5.6..6...1..1.4..8..77...91.....9.3..5..6.....7
	//.971.3.4..3..4.7.....67...3273914586986..71.4154.68..77...91.....9736.5.36.....7. 
	//597183642638249715412675893273914586986527134154368927725491368849736251361852479

	//  Aligned Pair Exclusion Example 2
	//....23...62.598..4...7.652.598.62.4.2.1.758.6.768...52.62..7...8..6342.9...2.....
	//754123968623598714189746523598362147231475896476819352362957481815634279947281635  // Types including ALS

    public partial class SubsetTechGen: AnalyzerBaseV2{

        public bool SubsetExclusion( ){
			try{
				if( stageNoP != stageNoPMemo ){
					stageNoPMemo = stageNoP;
					base.AnalyzerBaseV2_PrepareStage();
					fALS.Initialize();
					Prepare_SubsetTec();
					 // ALSList_Leaf.ForEach( p=> WriteLine( p ));
				}
				debugPrint = false;	// true;

				if( ALSList_Leaf==null || ALSList_Leaf.Count<=3 ) return false;
						//if(debugPrint)  Debug_StreamWriter( $"\n{DateTime.Now}\n{TandE_st_Puzzle}\n{TandE_st_sol_int81}" );	// @@@					


				// ===== Prepare =====
				// Select an ALS that can become Leaf. (Level=1, Size=1, FreeBC<=3)
				List<UCell> LeafsCandidate = fALS.ALSList.FindAll( p=> p.Size==1 && p.UCellLst[0].FreeBC<=3).ConvertAll( P=> P.UCellLst[0] );
				LeafsCandidate.Sort( (a,b) => a.rc-b.rc ); 
							// { int k=0; LeafsCandidate.ForEach( p => WriteLine( $"{k++} {p}") ); }

				int IDID=0;

				// ===== Analize =====
				SubsetExcludeMan  SubsetMan = new( pAnMan );
				for( int StemSize=2; StemSize<=3; StemSize++ ){ // 2:Pair 3:Triple 
					
					// Choose Stem and Leafs as a set. Analyze the selected Stem and Leafs.					
					foreach( var (Stem,LeafsUCell) in IEGet_Stem_LeafA(LeafsCandidate, StemSize:StemSize, debugPrint:debugPrint ) ){

					//	WriteLine( $"\n\n@@@@@@@@@ IDID:{++IDID}" );
					//	if( IDID == 8 )  WriteLine( $"\n\n### IDID:{IDID}" );

							if(debugPrint) WriteLine( $" **Stem:{Stem.ToStringRC()} \n LeafsCandidate:\n  {string.Join("\n  ",LeafsUCell)}" );

						// Generate candidate list for Stem. 
						SubsetMan.GenerateList_noBLis( Stem, debugPrint:debugPrint);

						// Apply Leafs to candidate list(noBList)
						SubsetMan.Apply_Link( LeafsUCell, debugPrint:debugPrint);
						
						// Evaluate the applied list and reflect the positive/negative digits in the analysis results.
						var solFound = SubsetMan.Check_ConfirmedDigits( );

						if( solFound ){
							SolCode=2;	
							SubsetExclusion_SolResultA( Stem, LeafsUCell );
							if( __SimpleAnalyzerB__ )  return true;
							if( !pAnMan.SnapSaveGP(pPZL) )  return true;
							
							////if(debugPrint)  Debug_StreamWriter( debug_Result );
							solFound=false;
						}
					}
				}
			}
			catch(Exception e){ WriteLine( $"{e.Message}\n{e.StackTrace}" ); }
			return false;
		}



		private IEnumerable<(USubset,List<UCell>)> IEGet_Stem_LeafA( List<UCell> LeafsCandidate, int StemSize, bool debugPrint=false ){
			List<USubset> StemLst = new();
			UInt128 B0 = pBOARD.Where(p=>p.FreeB!=0).Aggregate( UInt128.Zero, (p,q) => p | UInt128.One<<q.rc );

			// *--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*
			// basic policy : Determine the Stem from the Leaf candidates
			// *--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*
 			int nxt = 0;
			int szMax = Min( LeafsCandidate.Count, 8 );
			for( int LeafSize=2; LeafSize<=szMax; LeafSize++ ){
				Combination cmb = new(LeafsCandidate.Count, LeafSize );
				while( cmb.Successor() ){
					UInt128 Stem81BT = B0; 
					for( int k=0; k<LeafSize; k++ ){
						UCell P = LeafsCandidate[ cmb.Index[k] ];
						Stem81BT &= ConnectedCells81[P.rc];
					}
					if( Stem81BT.BitCount() < StemSize )  continue;
							if(debugPrint) WriteLine( $"Stem81BT:{Stem81BT.ToBitString81N()}" ); //@@@

					Combination_81B cmb81B = new ( Stem81BT.BitCount(), StemSize, Stem81BT );
					while( cmb81B.Successor() ){
						UInt128 Stem81B = cmb81B.value81B;
							if(debugPrint) WriteLine( $" Stem81B:{Stem81B.ToBitString81N()}" ); //@@@

						List<UCell> Stem_UCellLst = Stem81B.IEGet_UCell(pBOARD).ToList();

						int FreeB = Stem_UCellLst.Aggregate( 0, (p,q) => p| q.FreeB );
						USubset Stem = new( ID:0, FreeB, Stem_UCellLst );

						List<UCell> Leafs = new();
						for( int k=0; k<LeafSize; k++ ){
							UCell Pleaf = LeafsCandidate[cmb.Index[k]];
							if( Pleaf.FreeBC <= Stem.Size )  Leafs.Add(Pleaf);
						}
						if( Leafs.Count<2 )  continue;  // ### Needs consideration @@@@@
							if(debugPrint){	//@@@
								int FreeBStem = Stem.FreeB;
								UInt128 connected2Stem = Stem.bitExp.Aggregate_ConnectedAnd();		//3‚Ìê‡‚Í•Ê‚Ìˆ—‚ª‚¢‚é
								List<UCell> selLeafsCandidate = LeafsCandidate.FindAll(
											q => (connected2Stem.IsHit(q.rc) && (FreeBStem&q.FreeB).BitCount()>=2 ) );
								selLeafsCandidate.ForEach( p=> WriteLine(p) );
							}
						yield return (Stem,Leafs);
					}

				}
			}
			yield break;
		}

		private bool SubsetExclusion_SolResultA( USubset Stem, List<UCell> LeafsCandidate ){
			bool solFound = true;

			SolCode=2;
			Stem.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr);
			var LeafsCandidate81B = LeafsCandidate.Aggregate(UInt128.Zero, (p,q) => p | (UInt128.One<<q.rc) );
			Stem.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr);
			LeafsCandidate81B.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr2);

			string st_Stem  = $"{Stem.bitExp.ToRCStringComp(Stem.FreeB)}";

			string st_LeafsCandidate = "";
			foreach( var P in LeafsCandidate ){ st_LeafsCandidate += $" {P.rc.ToRCString()}"; };

			string st_LeafsCandidateL = "";
			foreach( var P in LeafsCandidate ){ st_LeafsCandidateL += $" {P.rc.ToRCString()}#{P.FreeB.ToBitStringN(9)}"; };

			Result     = $"SubsetExclusion Stem:{st_Stem} Leaf:{st_LeafsCandidate}";
			ResultLong = $"SubsetExclusion\n  Stem:{st_Stem}\n  Leaf:{st_LeafsCandidateL}";

			return solFound;
		}

	}
}
#endif