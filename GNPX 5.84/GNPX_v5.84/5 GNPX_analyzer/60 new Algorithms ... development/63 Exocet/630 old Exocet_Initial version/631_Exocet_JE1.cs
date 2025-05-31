using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using static System.Math;
using System.IO;

using GIDOO_space;
/*
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using System.Collections;
using static GNPX_space.Firework_TechGen;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Controls;
using Windows.Devices.Power;
using System.Data;
*/

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


        public bool Junior_Exocet_JE1( ){	

			// <<<<< Prepare >>>>>
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_Exocet_TechGen( printB:false );	//debugPrint );
			}

			// <<<<< BasicForm ... dir,Stem => Base >>>>>
			ElementElimination_Manager_UT("Initialize");
			foreach( var Exo in IEGet_Exocet_Requirement_R1_BasicForm( debugPrint:false ) ){			 // Requirement_1

				// UExocet_Eements
				Exo.ExocetName = "JE1";
				foreach( var UEx in IEGet_Target_JE1( Exo, debugPrint:false ) ){						// Requirement_23456
						if(debugPrint) WriteLine( $"\n\n\n Exocet_Elements\n   (ExG1,ExG2) : {Exo.ExG1}, {Exo.ExG2}\n EscapeCells:{Exo.EscapeCells81.TBS()}" ); 

					bool solFound = Exocet_RouleCheck( Exo );
					if( !solFound )  goto LClear;	

					// ..... JE1 Limited .....
					if( Exo.CoverLine_by_Size[2].BitCount() < 2 )  goto LClear;	

					JE1_Result( Exo, debugPrint:debugPrint );
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
																
							
				LClear:	
					foreach( var UC in pBOARD.Where(p=>p.No==0) ){ UC.CancelB=0; UC.ECrLst=null; }
					ElementElimination_Manager_UT("Initialize");
				}
			}
			return false;	
		}


		private void JE1_Result( UExocet Exo, bool debugPrint ){
			UExocet_elem ExG1=Exo.ExG1, ExG2=Exo.ExG2;
			var		 (rcBase1,rcBase2) = Exo.Base81.Get_rc1_rc2();
			
			int FreeB0 = Exo.FreeB0;
			UInt128	 SLine0=Exo.ExG0.SLine_Sx, SLine1=ExG1.SLine_Sx, SLine2=ExG2.SLine_Sx;

			List<UFireworkExo> fwList = Exo.fwList;

			// <<<<< Result ...  coloring, create message  >>>>>
			try{
				SolCode = 2;
				
			// IE_SetNoBBgColorNSel... "NSel": Set background color regardless of "digit condition"
			// <<< Base >>>
				Exo.Base81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr );	

			// <<< Object, Companion, Mirror >>>
				ExG1.Object81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2G );
				//ExG2.Object81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2G );
				ExG2.Object81.IE_SetNoBBgColor_All( pBOARD,  AttCr, SolBkCr2G );

				ExG1.Companion81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2 );
			  //ExG2.Companion81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2 );  // no Companions
						
				Exo.ExG1.Mirror81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr6 );
				Exo.ExG2.Mirror81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr6 );

				SLine0.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr4 );
				SLine1.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr3 );
				SLine2.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr3 );

			// =========================================================================================
				string stObjectL = Exo.diagonalB? "diagonal type": "aligned type";
				string stObjL1 = "", stObjL2 = "";
				int plus = 0;
				if( ExG1.Object81.BitCount()>1 ){ plus++; stObjL1 += $"\n  Object1:"; }
				else{ stObjL1 += $"\n  Target1:"; }

				if( ExG2.Object81.BitCount()>1 ){ plus++; stObjL2 += $"\n  Object2:"; }
				else{ stObjL2 += $"\n  Target2:"; }

				if( plus==1 ) stObjectL = "JE+ type"; else if( plus==2 ) stObjectL = "JE++ type";

				stObjL1 += $"{ExG1.stObject81}  Companion1:{ExG1.stCompanion}  Mirror1:{ExG1.stMirror81F}";
				stObjL2 += $"{ExG2.stObject81}  Companion2:{ExG2.stCompanion}  Mirror2:{ExG2.stMirror81F}";
				stObjectL += stObjL1 + stObjL2;

			// <<<SLine >>>
				UInt128 CrossLine_Sx = fwList.Aggregate( UInt128.Zero, (a,b) => a = a| qOne<<b.rcStem );
				string  stCRL1 = SLine1.ToRCStringComp();
				string  stCRL2 = SLine2.ToRCStringComp();
				string  stCrossLine = $"  CrossLine_ 0:{SLine0.ToRCStringComp()} / 1:{stCRL1} / 2:{stCRL2}";

			// <<< Result >>>
				string stWC = (Exo.WildCardB!=0)? $"Wildcard:#{Exo.WildCardB.ToBitStringN(9)}": "";
				Result     = $"JE1 Base:{Exo.stBase}{Exo.stBaseFreeB0} Target:{Exo.ExG1.stObject81} {Exo.ExG2.stObject81} {stWC}";
				ResultLong = $"Junior_Exocet_1 (Single Target)\n  Base : {Exo.stBase}{Exo.stBaseFreeB0}  {stObjectL}\n  {stWC}\n\n{stCrossLine}.";

				extResult = ResultLong;
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