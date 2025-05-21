using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;

namespace GNPX_space{

    public partial class JExocet_TechGen: AnalyzerBaseV2{

		private IEnumerable< UExocet > IEGet_Target_JE1( UExocet Exo, bool debugPrint=false ){
			int dir	   = Exo.dir;
			int rcStem = Exo.rcStem;
			int FreeB0 = Exo.FreeB0;

			int		house_SLine0 = (dir==0)? (rcStem%9+9): (rcStem/9);
			UInt128 CrossLine0   = HouseCells81[house_SLine0];
			UInt128 SLine0	     =  CrossLine0 .DifSet( HouseCells81[rcStem.B()+18] );

			//	 debugPrint = true;
			// [req2_2] The Target Cells belong to the same Band as the Base Cells but do not see the Base Cells
			foreach( var (sq,TgObj2) in IEGet_TargetObject_JE1absent(Exo, debugPrint:false) ){	// ##### debugPrint=true
									if(debugPrint)    WriteLine( $"##### JE1 --- TgObj2:{TgObj2.TBScmp()}" );

				var (house_SLine1,  CrossLine2,SLine2) = _Get_SLine_withObject( Exo.dir, TgObj2, Exo.Block2 );	// Sline_2
				if( SLine2.IEGet_rc().All(rc=>pBOARD[rc].FreeBC==0) )  continue;
				int FreeB_Tg2 = TgObj2.IEGet_UCell(pBOARD).Aggregate( 0, (a,uc) => a| uc.FreeB_Updated() );
				int rcCmp2 = int.MinValue;
				// ----------------------------------------------------------------------------------------------------------------




				// <<< Target Cell >>>
				// [req2_1] The Target Cells belong to the same Band as the Base Cells but do not see the Base Cells
				foreach( var TgObj1 in IEGet_TargetObject_JE2(Exo,3-sq,debugPrint:false) ){	// ##### debugPrint=true										
									if(debugPrint)    WriteLine( $"##### JE1 --- TgObj1:{TgObj1.TBScmp()}  TgObj2:{TgObj2.TBScmp()}" );

					var (house_SLine2, CrossLine1,SLine1) = _Get_SLine_withObject( Exo.dir, TgObj1, Exo.Block1 );	// Sline_1
					if( SLine1.IEGet_rc().All(rc=>pBOARD[rc].FreeBC==0) )  continue;

					int FreeB_Tg1 = TgObj1.IEGet_UCell(pBOARD).Aggregate( 0, (a,uc) => a| uc.FreeB_Updated() );
					int rcCmp1    = _Get_rcCompanion( Exo, TgObj1, debugPrint:false );  // Companion Cell_1
					if( rcCmp1>=0 &&  (_Digit_ToFreeB(rcCmp1) & FreeB_Tg1)>0 )   continue;		// [req4_1] C1 does not contain any Base Digit present in T1. 						
					// ----------------------------------------------------------------------------------------------------------------


					// ===== Configure and verify the components. =====
					//[req3-0] Base digits are in at least one Target.
					if( Exo.FreeB0.DifSet(FreeB_Tg1|FreeB_Tg2) > 0 ) continue;	

					int FreeB_Tg12 = (FreeB_Tg1 | FreeB_Tg2) & Exo.FreeB0;
					int FreeB_Tg12_Count = FreeB_Tg12.BitCount();

					//[req3] The Target Cells together contain at least the same 3 or 4 Candidates as the Base Cells.
					if( FreeB_Tg12_Count<3 || FreeB_Tg12_Count>4 )   continue;		// Targets digits is 3 or 4
							if(debugPrint)  WriteLine( $"         FreeB_Tg12:{FreeB_Tg12.TBS()}" );
				

					// =====================================================================================
					Exo.Initialize();

					Exo.ExG0 = new( Exo, sq:0, Object81:qMaxB81, house_Sx:house_SLine0, Comp81:qMaxB81,CrossLine_Sx:CrossLine0, SLine_Sx:SLine0 );
					Exo.ExG1 = new( Exo, sq:1, TgObj1, qOne<<rcCmp1, house_SLine1, CrossLine1, SLine1 );
					
					Exo.ExG2 = new( Exo, sq:2, TgObj2, 0/* no Companions */, house_SLine2, CrossLine2, SLine2, PhantomObjectB:true );
					// -------------------------------------------------------------------------------------


					// [req6] All instances of each Base Digit as a candidate or a given or a solved value in the S Cells
					//		  must be confined to no more than two Cover Houses
/* SLine Cover */	bool ExocetLinkTermB = Exocet_Requirement_IsCovered( Exo, debugPrint:false );
					if( !ExocetLinkTermB )		continue;


					// <<< Mirror Cells >		
					// [req5] The Mx Mirror Node contain the Base Digits present in Tx
					// Mirror nodes hold  a "base-digits" and a "non-base-digits".

					// M1(M2) depends on the positions of T1 and T2. It is set after T1 and T2 are defined.
					UInt128 Mirror1F = _Get_MirrorF_GivenSolvedCandidate( Exo, 1, debugPrint:false );
					UInt128 Mirror2F = _Get_MirrorF_GivenSolvedCandidate( Exo, 2, debugPrint:false );
					UInt128 Mirror1 = Mirror1F & BOARD_FreeCell81;
					UInt128 Mirror2 = Mirror2F & BOARD_FreeCell81;

					Exo.ExG1.Set_Mirror( Mirror1F, Mirror1 );
					Exo.ExG2.Set_Mirror( Mirror2F, Mirror2 );


					if(debugPrint){
						string st = $"\n\n\n@@@ Exocet_Elements  type:{stType(Exo.diagonalB)}";
						st += $"\n  ExG1 : {Exo.ExG1.ToString(1)}\n  ExG2 : {Exo.ExG2.ToString(2)}"; 
						WriteLine( st ); 
					}

					yield return Exo;
				}

			}
			yield break;


			IEnumerable< (int,UInt128) > IEGet_TargetObject_JE1absent( UExocet Exo, bool debugPrint=false ){
				foreach( var Object81_1 in IEGet_TargetObject_JE1absentSub(Exo,1, debugPrint:debugPrint) ){ yield return (1,Object81_1); }
				foreach( var Object81_1 in IEGet_TargetObject_JE1absentSub(Exo,2, debugPrint:debugPrint) ){ yield return (2,Object81_1); }
			}
			IEnumerable< UInt128 > IEGet_TargetObject_JE1absentSub( UExocet Exo, int sq2, bool debugPrint=false ){
				int blockNo = (sq2==1)? Exo.Block1: Exo.Block2;
				int FreeB0 = Exo.FreeB0;

				UInt128 TargetBlock81B = ( HouseCells81[blockNo+18] .DifSet(ConnectedCells81[Exo.rcStem]) );



				UInt128 FixedObject81 = TargetBlock81B & (BOARD_FreeCell81^qMaxB81);   // <-- #####




					if(debugPrint)  WriteLine( $"BaseConnectedCells81:{Exo.BaseConnectedAnd81}\n   Candidate4Object81:{FixedObject81.TBS()}" );

				int h0 = (Exo.dir==0)? (blockNo%3)*3+9: (blockNo/3)*3;
				for( int h=h0; h<h0+3; h++ ){
					UInt128 Object81 = (FixedObject81 & HouseCells81[h]);
					if( Object81.BitCount() != 2 )  continue;	
						if(debugPrint)  WriteLine( $"h0:{h,2} Object81:{Object81.TBS()}  Object81.Count;{Object81.BitCount()}" );

					yield return  Object81;
				}
				yield break;
			}
			

		}
	}


}
