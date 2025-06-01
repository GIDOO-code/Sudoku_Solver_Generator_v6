using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Policy;
using System.Windows.Shapes;
using static GNPX_space.JExocet_TechGen;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace GNPX_space{
	// Reference to SudokuCoach.
	// https://www.taupierbw.be/SudokuCoach/SC_Exocet.shtml 


    public partial class JExocet_TechGen: AnalyzerBaseV2{

		// ... Prefer clear code over efficiency ...



	#region JE1 exclusion rules
		private void Test_JE1_Rule( UExocet Exo, bool debugPrint=false ){
			// 1) One of the digits that satisfy the S criteria for BaseDigit(#abc) is present in Target, and the other (#bc) is not present in Target.
			//    It is not certain which digit is TargetDigit.
			// 2) BaseDigits (#bc) other than TargetDigit are in EscapeEscape.
			// 3) The wildcard number (#z) is in Base but does not exist in Target.
			// 4) here can be no BaseDigits in non-band, non-S regions.

			// 1)BaseDigit(#abc) の S基準を満たす数字 の 1つ(#aとする)は Target に存在し、その他(#bc) は Target に存在しない。
			// どの数字が TargetDigit(#a) であるかは確定しない。
			// 2)TargetDigit以外のBaseDigits(#bc)はEscapeEscapeにある。
			// 3)Wildcard数字(#z)はBaseにあり、Targetには存在しない。
			// 4)非バンド、非S領域には BaseDigits はあり得ない。

			if( Exo.ExocetName != "JE1" )  return;

			int	 FreeB0 = Exo.FreeB0;
			int  wildcardB = Exo.WildCardB;
			int  noWC = wildcardB.BitToNum();
			if( wildcardB.BitCount()!=1 || wildcardB==0 )  return;
		
			ElementElimination_Manager_UT("@");

			string stWC = $"#{wildcardB.ToBitStringN(9)}";
			ElementElimination_Manager_UT("@");

			foreach( var UC in Exo.ExG1.Object81.IEGet_UCell_noB(pBOARD,wildcardB) )  UC.CancelB |= wildcardB;

			UInt128 elm = Exo.Base81.IEGet_UCell_noB( pBOARD,wildcardB) .Aggregate( qMaxB81, (a,uc)=> a& ConnectedCells81[uc.rc] );		
			elm = (elm.DifSet(Exo.Base81)) & FreeCell81b9[noWC];

			if( elm != 0 ){
				string st = $"[JE1_Rule-1] {Exo.stBase}{stWC} is the \"Wildcard\", Then ({elm.TBScmp()}){stWC} are negative.";
				ElementElimination_Manager_BB( Exo, ELM81:elm, noB:wildcardB, st );

				UInt128 elmS1 = Exo.ExG1.SLine_Sx.Get_UInt128_noB(pBOARD,wildcardB);
				if( elmS1.BitCount() == 1 ){
					int rcS1 = elmS1.Get_rcFirst();
					int elmNo = pBOARD[rcS1].FreeB.DifSet(wildcardB);
					string stS1 = $"[JE1_Rule-1]   And {elmS1.ToRCStringComp()}{stWC} in Sline-1 is positive. Then {rcS1.ToRCString()}#{elmNo.TBScmp()} are negative.";
					ElementElimination_Manager_rB( Exo, rcE:rcS1, noB:elmNo, stS1 );
				}
			}

			return;
		}
	#endregion JE1_Test





	#region JE2_Object exclusion rules
	/*
		1.	In a pair of object cells containing a locked non-base digit, the other non-base digits are false.
		2.	In each object cell a base digit that is absent from the corresponding mirror node is false.
		3.	A base digit that must be true in one pair of object cells must be false in the other object cell pair.  
		1. ロックされた非ベース数字を含むオブジェクト セルのペアでは、他の非ベース数字は偽です。 
		2. 各オブジェクト セルでは、対応するミラーノードに存在しないベース数字は偽です。 
		3. 1つのオブジェクト セル ペアで真である必要があるベース数字は、他のオブジェクト セル ペアでは偽である必要があります。
	*/

		// Rule-20 :
		private void Test_JE2P01_Rule( UExocet Exo, bool debugPrint=false ){　
			// 1.In a pair of object cells containing a locked non-base digit, the other non-base digits are false.
			//  ロックされた非ベース数字を含むオブジェクト セルのペアでは、他の非ベース数字は偽です。 

			if( Exo.ExocetName != "JE2" )  return;
			UExocet_elem ExG1=Exo.ExG1, ExG2=Exo.ExG2;			
			
			ElementElimination_Manager_UT("@");

			_Test_JE2P01_Rule_sub( Exo, ExG1, ExG2.Object81 );
			_Test_JE2P01_Rule_sub( Exo, ExG2, ExG1.Object81 );
			return;

				void  _Test_JE2P01_Rule_sub( UExocet Exo, UExocet_elem ExGx, UInt128 Exclusion ){
					if( ExGx.Object81.BitCount() <= 1 )  return;				// Object Type
					
					int nFreeB_Object = ExGx.FreeB_Object81.DifSet(Exo.FreeB0);	// non BaseDigit
					if( nFreeB_Object.BitCount() <= 1 )  return; 

					foreach( int no in nFreeB_Object.IEGet_BtoNo() ){
						int hLocked = Get_LockedHouse( ExGx.Object81, no, Exclusion );		
						if( hLocked == 0 )  continue;
						// "no" is a non-BaseDigit and locked with H-House. (The condition of this rule is that it is locked.)

						int noBElm = nFreeB_Object.BitReset(no);
						string st = $"[JE2+ Rule-P1] {ExGx.stObject81}#{no+1} is Locked non BaseDigit, Then {ExGx.stObject81}#{noBElm.TBScmp()} are negative";
						ElementElimination_Manager_BB( Exo, ELM81:ExGx.Object81, noB:noBElm, st );
					}
					return;
				}
		}
		


		private void Test_JE2P02_Rule( UExocet Exo, bool debugPrint=false ){　
			// 2. In each object cell a base digit that is absent from the corresponding mirror node is false.
			//  各オブジェクト セルでは、対応するミラーノードに存在しないベース数字は偽です。 

			if( Exo.ExocetName != "JE2" )  return;
			if( Exo.FreeB0.BitCount() > 2 ) return;

			ElementElimination_Manager_UT("@");
				
			_Test_JE2P02_Rule_sub( Exo, Exo.ExG1 );
			_Test_JE2P02_Rule_sub( Exo, Exo.ExG2 );
			return;

				void  _Test_JE2P02_Rule_sub( UExocet Exo, UExocet_elem ExGx ){
					if( ExGx.Object81.BitCount() <= 1 )  return;				// Object Type

					foreach( int no in (Exo.FreeB0 & ExGx.FreeB_Object81).IEGet_BtoNo() ){
						if( ExGx.FreeB_Mirror81withFixed.IsHit(no) )  continue;

						int     noBElm = 1<<no;
						UInt128 objElm = ExGx.Object81.IEGet_UCell_noB(pBOARD,noBElm).Aggregate(qZero, (a,uc) => a| qOne<<uc.rc );
						string st = $"[JE2+ Rule-P2] Mirror-{ExGx.sq} ({ExGx.stMirror81}) have no #{no+1},";
							   st += $" then Object-{ExGx.sq} ({ExGx.stObject81}#{noBElm.TBScmp()}) is negative.";
						ElementElimination_Manager_BB( Exo, ELM81:ExGx.Object81, noB:noBElm, st );
					}
					return;
				}
		}
		private void Test_JE2P03_Rule( UExocet Exo, bool debugPrint=false ){　
			// 3. A base digit that must be true in one pair of object cells must be false in the other object cell pair.  
			//  1つのオブジェクト セル ペアで真である必要があるベース数字は、他のオブジェクト セル ペアでは偽である必要があります。

			if( Exo.ExocetName != "JE2" )   return;
					
			_Test_JE2P03_Rule_sub( Exo, Exo.ExG1, Exo.ExG2 );
			_Test_JE2P03_Rule_sub( Exo, Exo.ExG2, Exo.ExG1 );
			return;

				void  _Test_JE2P03_Rule_sub( UExocet Exo, UExocet_elem ExGA, UExocet_elem ExGB ){
					if( ExGA.Object81.BitCount() == 1 )  return;

					foreach( int no in (Exo.FreeB0 & ExGA.FreeB_Object81).IEGet_BtoNo() ){
						int noB = 1<<no;
						int h_Lockedif =  Get_LockedHouse(ExGA.Object81,no, ExGB.Object81);
						if( h_Lockedif == 0 )  continue;

						UInt128 objElm = ExGB.Object81.IEGet_UCell_noB(pBOARD,noB).Aggregate(qZero, (a,uc) => a| qOne<<uc.rc );
						if( objElm.BitCount() == 0 )  continue;

						string st = $"[JE2+ Rule-P3] ({ExGA.Object81.TBScmp()})#{no+1} is fixed, Then ({ExGB.Object81.TBScmp()})#{noB.TBScmp()} are negative";
						ElementElimination_Manager_BB( Exo, ELM81:objElm, noB:noB, st );
					}
					return;
				}
		}
	#endregion JE2_Object_Test

	}
}
