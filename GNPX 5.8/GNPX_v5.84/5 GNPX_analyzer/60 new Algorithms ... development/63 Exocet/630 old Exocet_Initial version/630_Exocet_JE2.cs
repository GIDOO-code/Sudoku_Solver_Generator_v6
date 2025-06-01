using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using static System.Math;

using GIDOO_space;


namespace GNPX_space{
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//   under development (GNPXv6)
	//   @@@@@ There are still discrepancies between the Sudoku-6 HP and GNPX-6 codes. @@@@@
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

    public partial class JExocet_TechGen: AnalyzerBaseV2{
		// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
		//	"JuniorExocet" has a clear logic,
		//	so the algorithm and code are relatively simple!
		//	by:David P Bird	&nbsp;&nbsp;"JExocet Compendium"
		//	http://forum.enjoysudoku.com/jexocet-compendium-t32370.html
		//	*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*


        public bool Junior_Exocet_JE2( ){	

			// <<<<< Prepare >>>>>
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_Exocet_TechGen( printB:false );	//debugPrint );
			}

			// <<<<< BasicForm ... dir,Stem => Base >>>>>
			ElementElimination_Manager_UT("Initialize");
			foreach( var Exo in IEGet_Exocet_Requirement_R1_BasicForm( debugPrint:false ) ){

				// UExocet_Eements
				Exo.ExocetName = "JE2";
				foreach( var UEx in JEeocet_Requirement_R1_BasicForm_Target_IEGet( Exo, debugPrint:false ) ){	
									
					// ............... JE2 Limited ...............
					if( Exo.CoverLine_by_Size[3]>0 ) goto LClear;		// Cases with wildcards are excluded.
					if( Exo.CoverLine_by_Size[0]>0 ) goto LClear;		// Cases with wildcards are excluded.
						

					// ...........................................
					bool solFound = Exocet_RouleCheck( Exo );			// JExocet_RouleCheck(Exo) does not check the number of Cover-Lines in S-Area.
					if( !solFound ) goto LClear;

					JExocet_Result( Exo, debugPrint:false );	//debugPrint );

					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
							
				LClear:	
					foreach( var UC in pBOARD.Where(p=>p.No==0) ){ UC.CancelB=0; UC.ECrLst=null; }
					ElementElimination_Manager_UT("Initialize");
				}
			}
			return false;	
		}

		private void JExocet_Result( UExocet Exo, bool debugPrint ){
			UExocet_elem ExG0=Exo.ExG0, ExG1=Exo.ExG1, ExG2=Exo.ExG2;
			var		 (rcBase1,rcBase2) = Exo.Base81.Get_rc1_rc2();
			
			int FreeB0 = Exo.FreeB0;
			UInt128	 SLine0=Exo.ExG0.SLine_Sx, SLine1=ExG1.SLine_Sx, SLine2=ExG2.SLine_Sx;

				// debugPrint = true;	


			// <<<<< Result ...  coloring, create message  >>>>>
			try{
				SolCode = 2;

			// <<< Base >>>
				Exo.Base81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr );	

			// <<< Object, Companion, Mirror >>>
				// IE_SetNoBBgColorNSel... "NSel": Set background color regardless of "digit condition"
						int plus = 0;
						string stObjL1 = "", stObjL2 = "";
						if( ExG1.Object81.BitCount()>1 ){ plus++; stObjL1 += $"\n  Object1:"; }
						else{ stObjL1 += $"\n  Target1:"; }

						if( ExG2.Object81.BitCount()>1 ){ plus++; stObjL2 += $"\n  Object2:"; }
						else{ stObjL2 += $"\n  Target2:"; }
			
						stObjL1 += $"{ExG1.stObject81}  Companion1:{ExG1.stCompanion}  Mirror1:{ExG1.stMirror81}";
						stObjL2 += $"{ExG2.stObject81}  Companion2:{ExG2.stCompanion}  Mirror2:{ExG2.stMirror81}";


						string stJE2Type = Exo.diagonalB? "diagonal type": "aligned type";
						if( plus==1 ) stJE2Type = "JE+ type"; else if( plus==2 ) stJE2Type = "JE++ type";
						stJE2Type += stObjL1 + stObjL2;

				ExG1.Object81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2G );
				ExG2.Object81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2G );

				if( ExG1.Companion81 != null ){
					ExG1.Companion81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2 );
					ExG2.Companion81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2 );	
					
					Exo.ExG1.Mirror81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr6 );
					Exo.ExG2.Mirror81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr6 );
				}

			// <<<SLine >>>
				List<UFireworkExo> fwList = Exo.fwList;
				UInt128 CrossLine_Sx = fwList.Aggregate( UInt128.Zero, (a,b) => a = a| qOne<<b.rcStem );

				string stCrossLine = $"\n CoverLines{Exo.stCoverLines}";

				SLine0.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr4 );
				SLine1.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr3 );
				SLine2.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr3 );

			// <<< Result >>>
						Result     = $"JE2 Base:{Exo.stBase}{Exo.stBaseFreeB0} Target:{Exo.ExG1.stObject81} {Exo.ExG2.stObject81}";
						ResultLong = $"Junior_Exocet_2\n  Stem : {Exo.rcStem.ToRCString()}\n  Base : {Exo.stBase}{Exo.stBaseFreeB0}  {stJE2Type}";

						extResult = ResultLong + "\n\n" + stCrossLine;
#if DEBUG
						extResult += "\n\n Firework\n" + Exo.Get_FireworkList();
#endif
						extResult += $"\n\n{new string('-',80)}\n Explanation of candidate digits exclusion\n";

						string stE = string.Join( "\n", extResultLst );
						int n1=stE.Length, n2;
						do{
							stE = stE.Replace("@\n@","@");
							if( n1==(n2=stE.Length) ) break;
							n1 = n2;
						}while(true);

						extResult += stE.Replace("+\n","+").Replace("@","\n").Replace("\n\n","\n").Replace("\n","\n  ");
							if(debugPrint)  WriteLine( "@@"+extResult );

			}
			catch( Exception e ){ WriteLine( $"{e.Message}\n{e.StackTrace}" ); }

			return;
		}
	}
}