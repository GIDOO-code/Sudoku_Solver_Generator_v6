using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Policy;
using System.Data;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows.Documents;

namespace GNPX_space{
	// Reference to SudokuCoach.
	// https://www.taupierbw.be/SudokuCoach/SC_Exocet.shtml 


    public partial class JExocet_TechGen: AnalyzerBaseV2{
		private bool Exocet_RouleCheck( UExocet Exo ){
			UExocet_elem ExG1=Exo.ExG1, ExG2=Exo.ExG2;
			var			 (rcBase1,rcBase2) = Exo.Base81.Get_rc1_rc2();
			UCell		 UCBase1=pBOARD[rcBase1], UCBase2=pBOARD[rcBase2];	
			int			 FreeB0 = UCBase1.FreeB | UCBase2.FreeB;

					// ... for debug ..... <<< Check the exclusion rules that are causing the error >>>
						bool testOn = false;

						UCell UCt = pBOARD[11];
						void test(int rule) => WriteLine( $"  === rule:{rule}  UC:{UCt.rc.ToRCString()}({UCt.rc})  FreeB:{UCt.FreeB.TBS()}  CancelB:{UCt.CancelB.TBS()}" ); 
				
						if(testOn){
							int n = 0;
							string st = Exo.CoverLine_by_Size.Aggregate("  Exo.Covered:", (a,b) => a+$" [{n++}]"+b.TBS() );  
							WriteLine( $"\n{Exo}\n Exo.Covered {st}" );
							foreach( var p in Exo.CoverStatusList.Where(q => q!=null) )  WriteLine( $"{p}");
							if( UCt.CancelB > 0 )  WriteLine( "....." );

							foreach( var P in pBOARD.Where( uc => uc.CancelB>0) )  WriteLine( $"  ##TestOn: {P.rc.ToRCString_NPM()}.CancelB:{P.CancelB.TBS()}");
						}
					// ........................................

			// The following tests have dependencies during execution.
			// Since JExocet is rare, the cost of repetition is relatively small.
			int Retest=3, cancelT=0;
			try{

				// [0] This rule is invalid. JE2 requires two Cover-Lines in the S-Area.
				//Test_JE2_Rule0_2orMoreCoverLine( Exo, debugPrint:false );					// This exclusion rule is invalid
				//if(testOn) test(0);


				// ..... ver6.0 .....
				Test_Incompatible( Exo, debugPrint:false );	 // (Changed 'Exo.FreeB0' internally.)
				if(testOn) test(100);

			LRetest:



				// ============= JE1 Rule =============
				Test_JE1_Rule( Exo, debugPrint:false );				// ◆◆◆true
					if(testOn) test(101);


				// ============= JE2 Rule =============
				// [1] Base Digit that is restricted to only one S cover house is false.
				Test_Rule01_only_one_S( Exo, debugPrint:false );		// This rule is invalid. JE2 requires two Cover-Lines in the S-Area.
					if(testOn) test(1);

				// [2] Any base candidate that isnt capable of being simultaneously true in at least one target cell and its mirror node is false.
				Test_Rule02_Base_Target_Mirror( Exo, debugPrint:false );
					if(testOn) test(2);

				// [3] Any non-Base Candidate in a Target Cell is false.
				Test_Rule03_non_Base_Candidate( Exo, debugPrint:false );
					if(testOn) test(3);

				// [4] If one base digit is valid in the target cell, it is negative in the other target cell.
				Test_Rule04_Target_to_Target( Exo, debugPrint:false );
					if(testOn) test(4);

				// [5] A base candidate that has a cross-line must be negative.
				Test_Rule05_Candidates_with_CrossLines( Exo, debugPrint:false );
					if(testOn) test(5);

		//		// [13] Missing in the Mirror cells ... mirror
		//		Test_Rule13_missing_BaseDigit_is_negated( Exo, debugPrint:true );	// ◆◆◆true
					if(testOn) test(13);


				// ... in case "Base candidates may be true" ... ..........................
				if( Exo.FreeB0.BitCount() == 2 ){ 
	
					// [6] Missing in the Mirror cells ... mirror
					Test_Rule06_Mirrored( Exo, debugPrint:false );		// Rule 6 is applicable when 2-Base-Locked.  ... This decision is final.  // ◆◆◆true
						if(testOn) test(6);

					// [7] Missing in the Mror cells ... Target
					Test_Rule07_Missing_in_Mirror( Exo, debugPrint:false );
						if(testOn) test(7);

					// [8] Mirror contains only one possible non-Base Digit
					Test_Rule08_Mirror_nonBase( Exo, debugPrint:false );
						if(testOn) test(8);

					// [9] Mirror Node contains a locked digit.
					Test_Rule09_Mirror_LockedDigits( Exo, debugPrint:false );
						if(testOn) test(9);
	
					// [10] Known Base Digit is false in all cells in full sight of either both Base Cells, or both Target Cells.
					Test_Rule10_known_BaseDigit( Exo, debugPrint:false );
						if(testOn) test(10);

					// [11] Handling Escape Cells or non-S cells
					// Rule-11 is invalid because it does not meet the requirements of JE2.
					Test_Rule11_EscapeCells_nonSCells( Exo, debugPrint:false );  
						if(testOn) test(11);

					// [12] Prevent known Base from being True
					Test_Rule12_Prevent_knownBase_from_beingTrue( Exo, debugPrint:false );
						if(testOn) test(12);
				}


				// ... Rule for JEorcet+,++
				if( Exo.ExocetName == "JE2" ){
					// [20] In a pair of object cells containing a locked non-base digit, the other non-base digits are false.
					Test_JE2P01_Rule( Exo, debugPrint:false );
						if(testOn) test(21);

					// [21] In each object cell a base digit that is absent from the corresponding mirror node is false.
					Test_JE2P02_Rule( Exo, debugPrint:false );
						if(testOn) test(22);

					// [22] A base digit that must be true in one pair of object cells must be false in the other object cell pair.
					Test_JE2P03_Rule( Exo, debugPrint:false );　
						if(testOn) test(23);
				}

 			// =================================================================================
				// If any element is cancelled, parse again.
				// Even if repeat this command, the output of the same message will be suppressed.
				if( (--Retest) > 0 ){
					int cancelCC = pBOARD.Sum( uc=> uc.CancelB.BitCount() );
					if( cancelCC > cancelT ){ cancelT=cancelCC; goto LRetest; }
				}
			// ---------------------------------------------------------------------------------
 
			// ---------------------------------------------------------------------------------------------------------------------------
			//	if( pBOARD.All(p=>p.CancelB==0) ){ pBOARD.ForEach( p=> p.ECrLst=null ); return false; }		// ... no applicable rule 
				foreach( var P in pBOARD ){ if( P.CancelB>0 && (P.CancelB&P.FreeB)==0 ) P.CancelB=0; }				// This code is probably unnecessary
				if( pBOARD.All(P => P.FixedNo==0 && P.CancelB==0) ){ pBOARD.ForEach( p=> p.ECrLst=null ); return false;	} // ... code error
					// ... Not a solution. Clear the analysis results and　return false
			}
			catch( Exception e ){ WriteLine( $"{e.Message}\n{e.StackTrace}" ); }
	
			// --- Solution found ---
			return true;
		}

	}
}
