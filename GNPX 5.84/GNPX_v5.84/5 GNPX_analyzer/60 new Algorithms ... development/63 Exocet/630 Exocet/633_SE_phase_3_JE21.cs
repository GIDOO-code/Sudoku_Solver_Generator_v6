using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using static System.Math;
using System.IO;

using GIDOO_space;
using System.Windows.Documents;
using GNPX_space;
using static GNPX_space.Senior_Exocet_TechGen;
using System.Security.Policy;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Windows.System;


namespace GNPX_space{
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//   under development (GNPXv6)
	//   @@@@@ There are still discrepancies between the Sudoku-6 HP and GNPX-6 codes. @@@@@
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	using G6_SF = G6_staticFunctions;
	using TapleUCL = (UCoverLine,UCoverLine,UCoverLine);


    public partial class Senior_Exocet_TechGen : AnalyzerBaseV2{

		// ===== IEGet_Targets  JE2 =========================================================
		private IEnumerable<TapleUCL> IE_SE_ph3sub_Get_TargetObject_SLine_JE2( USExocet SExo, (int,int,int) h012, bool debugPrint=false ){
			UInt128 ConStem = ConnectedCells81[SExo.rcStem];
			UInt128 SLineBand = BOARD_FreeCell81 & SExo.Band81.DifSet(ConStem);

			foreach( var Object1 in _IEGet_TagObj_from_SLine( SExo, SLineBand, SExo.CoverLine1 ) ){	//1-2																								//				
				foreach( var Object2 in _IEGet_TagObj_from_SLine( SExo, SLineBand, SExo.CoverLine2 ) ){																							// 				
					var (ExG0,ExG1,ExG2) = Phase3_Set_Target_SLine_SE_X( SExo, Object1, Object2, debugPrint:false );
					if( ExG0 == null )  continue;
					yield return (ExG0,ExG1,ExG2);
				}
			}
			yield break;
		}
		


		// ===== IEGet_Targets  JE1 =========================================================
		private IEnumerable<TapleUCL> IE_SE_ph3sub_Get_TargetObject_SLine_JE1( USExocet SExo, (int,int,int) h012, bool debugPrint=false ){
			UInt128 ConStem			= ConnectedCells81[SExo.rcStem];
			UInt128 SLineBand		= BOARD_FreeCell81 & SExo.Band81.DifSet(ConStem);
			UInt128 SLineBandFixed  = BOARD_Fixed81 & SExo.Band81.DifSet(ConStem);
			UCoverLine0 UCLb_A=SExo.CoverLine1, UCLb_B=SExo.CoverLine2;

						//G6_SF.__MatrixPrint( Flag:qZero, SLineBandFixed, SExo.Band81.DifSet(ConStem), SLineBandFixed, "SLineBandFixed SExo.Band81.DifSet(ConStem)" );
			foreach( var (ObjectX1,sqWD) in _IEGet_TagObj_from_SLine_SE1_Wildcard(SExo) ){	//1-2																				//				
				UCoverLine0 UCLb = (sqWD==1)? UCLb_B: UCLb_A;
				foreach( var ObjectX2 in _IEGet_TagObj_from_SLine(SExo, SLineBand, UCLb) ){
					if( ObjectX2.BitCount() != 1 )  continue;	// [TBD] @@@ In JE1, is an Object a one-cell type? @@@

						if(debugPrint)  G6_SF.__MatrixPrint( Flag:qZero, ObjectX1, ObjectX2, "ObjectX1 ObjectX2" );
					var (ExG0,ExG1,ExG2) = Phase3_Set_Target_SLine_SE_X( SExo, ObjectSq(ObjectX1,sqWD), ObjectSq(ObjectX2,3-sqWD), debugPrint:false );
					if( ExG0 == null )  continue;

					ExG1.wildcardB = true;			// ..... ExG1 is wildcard
						//G6_SF.__MatrixPrint( Flag:qZero, ExG1.Object, ExG2.Object, "ExG1.Object ExG2.Object" );
					yield return (ExG0,ExG1,ExG2);	

				}
			}
			yield break;

					IEnumerable<(UInt128,int)> _IEGet_TagObj_from_SLine_SE1_Wildcard( USExocet SExo ){	
						UInt128 SLineBand1 = SLineBandFixed & UCLb_A.SLine0;
								//G6_SF.__MatrixPrint( Flag:qZero, SLineBandFixed, CrsLn1, SLineBand1, "SLineBandFixed CrsLn1 SLineBand1" );
						if( SLineBand1.BitCount() == 2 ) yield return (SLineBand1,1);	// ... Two FixedCells = wildcard

						UInt128 SLineBand2 = SLineBandFixed & UCLb_B.SLine0;
								//G6_SF.__MatrixPrint( Flag:qZero, SLineBandFixed, CrsLn2, SLineBand2, "SLineBandFixed CrsLn2 SLineBand2" );
						if( SLineBand2.BitCount() == 2 ) yield return (SLineBand2,2);  // ... Two FixedCells = wildcard
											
						yield break;
					}
		}


		// ===== IEGet_Targets  SE_base =========================================================
		private IEnumerable<TapleUCL> IE_SE_ph3sub_Get_TargetObject_SLine_SE_Basic( USExocet SExo, (int,int,int) h012, bool debugPrint=false ){
					
			//under development
			var (h0,h1,h2)	  = h012;
			UInt128 ConStem	  = ConnectedCells81[SExo.rcStem];
			UInt128 SLineBand = BOARD_FreeCell81 & SExo.Band81.DifSet(ConStem);  // *** & SExo.Band81

			// ***** 1-2 *****
			foreach( var Object1 in _IEGet_TagObj_from_SLine(SExo, SLineBand, SExo.CoverLine1) ){			
				foreach( var Object2 in _IEGet_TagObj_from_SLine( SExo, SLineBand, SExo.CoverLine2) ){	
					
					if( (SExo.Band81 & (Object1 | Object2)) == qZero )  continue; 
					// *** At least one of the Targets is in Band.

					var (ExG0,ExG1,ExG2) = Phase3_Set_Target_SLine_SE_X( SExo, Object1, Object2, debugPrint:false );
							if(debugPrint)  Debug_SLIne_MatrixPrint(SExo, 1, 2, Object1, Object2, SLineBand );					
					if( ExG0 != null )  yield return (ExG0,ExG1,ExG2);
				}
			}
			yield break;

					void Debug_SLIne_MatrixPrint( USExocet SExo, int saX, int sbX, UInt128 obj1, UInt128 obj2, UInt128 SLineBand ){
						UCoverLine0 CL_A=SExo.CL012[saX], CL_B=SExo.CL012[sbX];			// ... CreossLine
						UInt128 _SLine01 = (CL_A.SLine0 | CL_B.SLine0) & SLineBand;
						string DB_title = $"_SLine{saX}_SLine{sbX} Object1:{obj1.TBScmp()} Object2:{obj2.TBScmp()}";
						G6_SF.__MatrixPrint( Flag:qZero,  _SLine01, obj1, obj2, DB_title );
						WriteLine( $"(Object1,Object2):{(obj1.TBScmp(), obj2.TBScmp())}" );
					}
		}



	}
}