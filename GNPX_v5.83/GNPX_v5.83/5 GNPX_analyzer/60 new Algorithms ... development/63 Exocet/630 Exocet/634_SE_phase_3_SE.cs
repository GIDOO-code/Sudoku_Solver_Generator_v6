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
using System.Security.AccessControl;


namespace GNPX_space{
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//   under development (GNPXv6)
	//   @@@@@ There are still discrepancies between the Sudoku-6 HP and GNPX-6 codes. @@@@@
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	using G6_SF = G6_staticFunctions;
	using TapleUCL = (UCoverLine,UCoverLine,UCoverLine);

    public partial class Senior_Exocet_TechGen : AnalyzerBaseV2{


	// ===== IEGet_Targets  SE_standard =========================================================
		private IEnumerable<TapleUCL> IE_SE_ph3sub_Get_TargetObject_SLine_SE_Standard( USExocet SExo, (int,int,int) h012, bool debugPrint=false ){
			var		(h0,h1,h2) = h012;
			int     dir=SExo.dir, rcStem=SExo.rcStem, FreeB0= SExo.FreeB0;
			UInt128 Base_StemAnd = SExo.Base81 | SExo.Band81.Aggregate_ConnectedAnd();
			UInt128 ConStem		 = HC81[ (dir,rcStem).DRCHf() ];
			UInt128 PossibleArea = BOARD_FreeCell81 .DifSet(ConStem);
						//G6_SF.__MatrixPrint( Flag:Base_StemAnd, BOARD_FreeCell81, PossibleArea, "BOARD_FreeCell81 PossibleArea" );

			// ***** 1-2 *****
			if( _IsProcessedTargets_CC1( SExo, h1,h2 ) ){
				foreach( var ExG012 in _IEGet_ObjectX2_from_SLine(SExo, PossibleArea, SExo.CoverLine1, SExo.CoverLine2 ) ){
					yield return ExG012;
				}
			}
#if G6plus_Develop
			// ***** 0-1 *****
			if( _IsProcessedTargets_CC1( SExo, h0,h1) ){
				foreach( var ExG012 in _IEGet_ObjectX2_from_SLine(SExo, PossibleArea, SExo.CoverLine0, SExo.CoverLine1 ) ){
					yield return ExG012;
				}
			}

			// ***** 0-2 *****
			if( _IsProcessedTargets_CC1( SExo,h0,h2) ){
				foreach( var ExG012 in _IEGet_ObjectX2_from_SLine(SExo, PossibleArea, SExo.CoverLine0, SExo.CoverLine2 ) ){
					yield return ExG012;
				}
			}
#endif
			yield break;
				
				bool _IsProcessedTargets_CC1( USExocet SExo, int h1, int h2 ){
					//SExo.ProcessedTargetsList.ConvertAll(p=>p.TBS(18))
					int h12 = 1<<h1 | 1<<h2;
					if( SExo.ProcessedTargetsList.Contains( h12 ) )  return false;
					SExo.ProcessedTargetsList.Add( h12 );
					return true;
				}	

				IEnumerable<TapleUCL> _IEGet_ObjectX2_from_SLine( USExocet SExo, UInt128 PossibleArea, UCoverLine0 CLbA, UCoverLine0 CLbB, bool debugPrint=false ){
					foreach( var Object1a in _IEGet_TagObj_from_SLine(SExo, PossibleArea, CLbA) ){	
						//WriteLine( $"Object1a:{Object1a.TBScmp()}" );
						//if( (Object1a&qMaxB81)!=(qOne<<41) )  continue;	// ===== SE_Nxt Debug =====

						foreach( var Object2a in _IEGet_TagObj_from_SLine(SExo, PossibleArea, CLbB) ){
							//WriteLine( $"  Object2a:{Object2a.TBScmp()}" );
							//if( (Object2a&qMaxB81)!=(qOne<<16) )  continue;	// ===== SE_Nxt Debug =====
							//if( (Object1a&qMaxB81)!=(qOne<<12) || (Object2a&qMaxB81)!=(qOne<<24) )  continue; // ===== SE_Nxt Debug =====

							var (ExG0,ExG1,ExG2) = Phase3_Set_Target_SLine_SE_X( SExo, Object1a, Object2a, debugPrint:false );
									if(debugPrint)  Debug_SLIne_MatrixPrint(SExo, 1, 2, Object1a, Object2a, PossibleArea );					
							if( ExG0 != null )	yield return (ExG0,ExG1,ExG2);
						}
					}
				}
		}
			

		// ===== IEGet_Targets  SE_Single, SE_SingleBase ===============================================
		private IEnumerable<TapleUCL> IE_SE_ph3sub_Get_TargetObject_SLine_Single( USExocet SExo, (int,int,int) h012, bool debugPrint=false ){		

			foreach( var ObjectWD in _IEGet_TagObj_from_SLine_Single_Wildcard(SExo) ){
				UInt128 Object1 = (ObjectWD&qMaxB81);
							//if( Object1 != (qOne<<43 | qOne<<44) )  continue;	// ===== SE_Nxt Debug =====

				foreach( var ObjectX2 in _IEGet_TagObj_from_SLine_Another(SExo, ObjectWD ) ){
							//if( Object2 != (qOne<<16) )  continue;			// ===== SE_Nxt Debug =====
							if(debugPrint){
								UInt128 Base_StemAnd = SExo.Base81 | SExo.Band81.Aggregate_ConnectedAnd();
								G6_SF.__MatrixPrint( Flag:Base_StemAnd, Object1, (ObjectX2&qMaxB81), "Object1, Object2" );
							}
					var (ExG0,ExG1,ExG2) = Phase3_Set_Target_SLine_SE_X( SExo, ObjectWD, ObjectX2, debugPrint:false );
					if( ExG0 == null )  continue;

					ExG1.phantomObjectB = true;
					if( SExo.ExocetNamePlus!="SE_SingleBase" )  ExG1.wildcardB = true;			// ..... ExG1 is wildcard

							if(debugPrint){
								UInt128 Base_StemAnd = SExo.Base81 | SExo.Band81.Aggregate_ConnectedAnd();	
								G6_SF.__MatrixPrint( Flag:Base_StemAnd, ExG0.Object,  ExG1.Object, ExG2.Object, "ExG0.Object, ExG1.Object ExG2.Object" );
							}
					yield return (ExG0,ExG1,ExG2);	

				}
			}
			yield break;

					IEnumerable<UInt128> _IEGet_TagObj_from_SLine_Single_Wildcard( USExocet SExo ){	// ... Get Wildcard
						int B_stem = SExo.rcStem.B()+18;
						foreach( var bx in Enumerable.Range(18,9).Where(b=>b!=B_stem) ){
							UInt128 BlkFixed =  HC81[bx] & BOARD_Fixed81;
							int kx = SExo.SLine012.FindIndex( p=> (p& BlkFixed).BitCount() >=2);
							if( kx<0 )  continue;

							if( (HC81[bx] & SExo.SLine012[kx] & BOARD_FreeCell81) != qZero )  continue;
							UInt128 WildObject = BlkFixed & SExo.SLine012[kx];

							int FreeB_wildcard = WildObject.Get_FreeB();
							if( (FreeB_wildcard & SExo.FreeB) > 0 ) continue;
									//UInt128 Base_StemAnd = SExo.Base81 | SExo.Band81.Aggregate_ConnectedAnd();
									//G6_SF.__MatrixPrint( Flag:Base_StemAnd, SExo.SLine012[kx], BlkFixed, WildObject, "SExo.SLine012[kx], BlkFixed, WildObject" );
							
							yield return  ObjectSq(WildObject,kx);
						}
						yield break;
					}
					
					IEnumerable<UInt128> _IEGet_TagObj_from_SLine_Another( USExocet SExo, UInt128 ObjectWD ){
						int  sqWD = (int)(ObjectWD>>100);
						foreach( var (SLX,sq2) in SExo.SLine012.WithIndex() ){
							if( sq2 == sqWD ) continue;

							// <<< Target >>>
							foreach( int rc in SLX.IEGet_rc().Where( p=> (pBOARD[p].FreeB & SExo.FreeB0)>0 ) ){
								if( (SExo.Base81 & ConnC81[rc]) > qZero )  continue;
								yield return  ObjectSq(qOne<<rc,sq2);
							}

							// <<< Object >>>
							foreach( var bx in Enumerable.Range(18,9) ){
								UInt128 SLineBlock =  HC81[bx] & BOARD_FreeCell81 & SLX;

								int nc = SLineBlock.BitCount();
								if( nc <= 1 )  continue;

								if( nc >= 2 ){
									Combination_81B cmb81 = new( nc, 2, SLineBlock );
									while( cmb81.Successor() ){
										UInt128 obj2 = cmb81.value81B;
										yield return  ObjectSq(obj2,sq2);
									}
								}
							}
						}
						yield break;
					}
		}


		
		private	IEnumerable<UInt128> _IEGet_TagObj_from_SLine( USExocet SExo, UInt128 Selecter, UCoverLine0 CLb ){
			UInt128 SLineBand0 = Selecter & CLb.SLine0;
			int		FreeB0 = SExo.FreeB0;
			bool _IsContainedFreeB( UInt128 rcLL ) =>  (rcLL.Get_FreeB() & FreeB0) > 0;

			// <<< Target >>>
			foreach( int rc in SLineBand0.IEGet_rc().Where( p=> (pBOARD[p].FreeB & FreeB0)>0 ) ){
				if( (SExo.Base81 & ConnC81[rc]) > qZero )  continue;
				yield return  ObjectSq( (qOne<<rc), CLb.sq);
			}

			// <<< Object >>>
			UInt128 Obj = qZero;
			int Block_Object = SLineBand0.IEGet_rc().Aggregate(0, (a,rc) => a | 1<<rc.B() );
			foreach( var blk in Block_Object.IEGet_BtoNo() ){
				var rcL = SLineBand0.IEGet_rc().Where(rc=>rc.B()==blk).ToArray();
				int sz = rcL.Count();
				if( sz==2 ){
					Obj = qOne<<rcL[0] | qOne<<rcL[1]; if( _IsContainedFreeB(Obj) )  yield return  ObjectSq( Obj, CLb.sq );
				}
				if( sz==3 ){
					Obj = qOne<<rcL[0] | qOne<<rcL[1]; if( _IsContainedFreeB(Obj) )  yield return  ObjectSq( Obj, CLb.sq );
					Obj = qOne<<rcL[1] | qOne<<rcL[2]; if( _IsContainedFreeB(Obj) )  yield return  ObjectSq( Obj, CLb.sq );
					Obj = qOne<<rcL[2] | qOne<<rcL[0]; if( _IsContainedFreeB(Obj) )  yield return  ObjectSq( Obj, CLb.sq );
					Obj = qOne<<rcL[0] | qOne<<rcL[1] | qOne<<rcL[2] ; if( _IsContainedFreeB(Obj) )  yield return  ObjectSq( Obj, CLb.sq );
				}
			}
			yield break;

		}


		private TapleUCL  Phase3_Set_Target_SLine_SE_X( USExocet SExo, UInt128 ObjectAp, UInt128 ObjectBp, bool debugPrint=false ){
			int sx1 = (int)(ObjectAp>>100) & 3;	 // ... Get Target(Object) Cross-Line No.
			int sx2 = (int)(ObjectBp>>100) & 3;
			int sx0 = 3 - (sx1+sx2);		
			UInt128  Object1=ObjectAp&qMaxB81, Object2=ObjectBp&qMaxB81;

			UCoverLine0 CL0=SExo.CL012[sx0], CL1=SExo.CL012[sx1], CL2=SExo.CL012[sx2];
			int dir = SExo.dir;
			UInt128 BaseTagObj = SExo.Base81 | Object1 | Object2;

			//debugPrint = true;

		#region --------- Companions, SLine ---------
			UInt128 Object12 = Object1 | Object2;
			UInt128 parallrHouse_Object = Object12.IEGet_rc(). Aggregate( qZero, (a,rc) => a| HC81[ (dir,rc).DRCHf() ] );
			SExo.Companions  = (SExo.CrossLine_012 & parallrHouse_Object) .DifSet(Object12);
						if(debugPrint)	G6_SF.__MatrixPrint( Flag:BaseTagObj, parallrHouse_Object, Object12, SExo.Companions,  "parallrHouse_Object Object12 SExo.Companions" );

			//UInt128 BFC = BOARD_FreeCell81;
			UInt128 Exclude_cells = SExo.EscapeCells | Object12 | parallrHouse_Object;			
			UInt128 SLine0 = CL0.SLine0.DifSet(Exclude_cells);		// CrossLine -> SLine
			UInt128 SLine1 = CL1.SLine0.DifSet(Exclude_cells);
			UInt128 SLine2 = CL2.SLine0.DifSet(Exclude_cells);
						if(debugPrint)  G6_SF.__MatrixPrint( Flag:BaseTagObj, SLine0, SLine1, SLine2, "SLine0 SLine1 SLine2" );
		#endregion ------------------------------------


		#region --------- Mirror ---------
			int     rcTag1=Object1.FindFirst_rc(), rcTag2=Object2.FindFirst_rc(); 
			UInt128 Mirror1Fa = HC81[rcTag2.B()+18] .DifSet( SExo.EscapeCells | HC81[(1-dir,rcTag2).DRCHf()] );
			UInt128 Mirror2Fa = HC81[rcTag1.B()+18] .DifSet( SExo.EscapeCells | HC81[(1-dir,rcTag1).DRCHf()] ); 

			UInt128 Mirror1F = (Object1.BitCount()>1)? Mirror1Fa: Mirror1Fa.DifSet( HC81[(dir,rcTag1).DRCHf()] );	//If Target, exclude its own concatenation
			UInt128 Mirror2F = (Object2.BitCount()>1)? Mirror2Fa: Mirror2Fa.DifSet( HC81[(dir,rcTag2).DRCHf()] );
						if(debugPrint){
							G6_SF.__MatrixPrint( Flag:BaseTagObj, Mirror1Fa, Mirror1F, "Mirror1Fa, Mirror1F" );
							G6_SF.__MatrixPrint( Flag:BaseTagObj, Mirror2Fa, Mirror2F, "Mirror2Fa, Mirror2F" );
						}
		#endregion ------------------------------------

		#region --------- UCoverLine ---------
			UCoverLine ExG0 = new( SExo, sx0, CL0.h,  CrossLine:CL0.SLine0,  SLine:SLine0 );
			UCoverLine ExG1 = new( SExo, sx1, CL1.h,  CrossLine:CL1.SLine0,  SLine:SLine1, Object1, Mirror1F );
			UCoverLine ExG2 = new( SExo, sx2, CL2.h,  CrossLine:CL2.SLine0,  SLine:SLine2, Object2, Mirror2F );
						if(debugPrint){
							G6_SF.__MatrixPrint( Flag:BaseTagObj, ExG0.Object, ExG0.Mirror81, "ExG0.Object ExG0.Mirror81" );
							G6_SF.__MatrixPrint( Flag:BaseTagObj, ExG1.Object, ExG1.Mirror81, "ExG1.Object ExG1.Mirror81" );
							G6_SF.__MatrixPrint( Flag:BaseTagObj, ExG2.Object, ExG2.Mirror81, "ExG2.Object ExG2.Mirror81" );
						}

			// Candidate Digits in SLine
			ExG0.FreeB_SLine = SLine0.IEGet_UCell(pBOARD).Aggregate(0, (a,u) => a |= u.noB_FixedFreeB );
			ExG1.FreeB_SLine = SLine1.IEGet_UCell(pBOARD).Aggregate(0, (a,u) => a |= u.noB_FixedFreeB );
			ExG2.FreeB_SLine = SLine2.IEGet_UCell(pBOARD).Aggregate(0, (a,u) => a |= u.noB_FixedFreeB );
		#endregion ------------------------------------

		// ----------------------------------------------
			SExo.ExG0=ExG0; SExo.ExG1=ExG1; SExo.ExG2=ExG2; 
			// ----------------------------------------------
			return  (ExG0,ExG1,ExG2);
		}
	}
}