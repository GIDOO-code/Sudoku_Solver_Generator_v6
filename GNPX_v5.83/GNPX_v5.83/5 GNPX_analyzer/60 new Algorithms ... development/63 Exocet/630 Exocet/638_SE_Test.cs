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
using System.Net.Sockets;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Linq;

namespace GNPX_space{
	// Reference to SudokuCoach.
	// https://www.taupierbw.be/SudokuCoach/SC_Exocet.shtml 


    public partial class Senior_Exocet_TechGen : AnalyzerBaseV2{

		// ... Prefer clear code over efficiency ...


      #region ElementElimination_Manager

		private void ElementElimination_Manager_UT( string st ){
			switch(st){
				case "Initialize": 
					extResultLst.Clear(); 
					foreach( var P in pBOARD ){ P.CancelB=0; P.ECrLst=null; }
					break;

				case "@":
					extResultLst.Add("@");
					break;
					
				case "Adjust":
					break;
			}
		}
		private bool ElementElimination_Manager_rB( USExocet SExo, int rcE, int noB, string st="" ){
			if( st!="" && !extResultLst.Contains(st) )  extResultLst.Add(st);

			if( rcE == 999 )  return false;

			pBOARD[rcE%100].CancelB |= (pBOARD[rcE%100].FreeB)&noB;		
			return true;
		}
		private bool ElementElimination_Manager_Bn(  USExocet SExo, UInt128 ELM81, int no, string st="" ){
			if( st!="" && !extResultLst.Contains(st) )  extResultLst.Add(st);

			int noB = 1<<no;
			foreach( int rc in ELM81.IEGet_rc(81) ){
				if( pBOARD[rc].No != 0 )  continue;
				pBOARD[rc].CancelB |= (pBOARD[rc].FreeB)&noB;
			}
			if( st!="" && !extResultLst.Contains(st) )  extResultLst.Add(st);
			return true;
		}
		private bool ElementElimination_Manager_BB( USExocet SExo, UInt128 ELM81, int noB, string st="" ){
			if( st!="" && !extResultLst.Contains(st) )  extResultLst.Add(st);

			foreach( int rc in ELM81.IEGet_rc(81) ){
				if( pBOARD[rc].No != 0 )  continue;
				pBOARD[rc].CancelB |= (pBOARD[rc].FreeB)&noB;
			}
			if( st!="" && !extResultLst.Contains(st) )  extResultLst.Add(st);
			return true;
		}
		private bool ElementElimination_Manager_st( string st ){
			if( st!="" && !extResultLst.Contains(st) )  extResultLst.Add(st);
			return true;
		}
	  #endregion ElementElimination_Manager





	#region JE1 exclusion rules
		private void Test_JE1_Rule( USExocet SExo, bool debugPrint=false ){
			// 1) One of the digits that satisfy the S criteria for BaseDigit(#abc) is present in Target, and the other (#bc) is not present in Target.
			//    It is not certain which digit is TargetDigit.
			// 2) BaseDigits (#bc) other than TargetDigit are in EscapeEscape.
			// 3) The wildcard number (#z) is in Base but does not exist in Target.
			// 4) here can be no BaseDigits in non-band, non-S regions.

			//if( SExo.ExocetName != "JE1" )  return;
			if( !SExo.ExocetName.Contains("JE1") && !SExo.ExocetNamePlus.Contains("Single") )  return;

			int	 FreeB0 = SExo.FreeB0;
			int  wildcardB = SExo.WildCardB;
			
			ElementElimination_Manager_UT("@");

			if( SExo.ExocetNamePlus.Contains("SingleBase") ){
				UInt128 elm = SExo.ExG2.Object;
				int elmNo = elm.Get_FreeB() .DifSet(FreeB0);
				if( elmNo > 0 ){
					string stSB = $"#{elmNo.TBScmp()}";
					string st = $"[Rule-1] {SExo.stBase}#{FreeB0.TBScmp()} is the \"SingleBase\", Then ({elm.TBScmp()}){stSB} are negative.";
					ElementElimination_Manager_BB( SExo, ELM81:elm, noB:elmNo, st );
				}
			}

			else if( wildcardB.BitCount()==1 ){
				int  noWC = wildcardB.BitToNum();

				string stWC = $"#{wildcardB.ToBitStringN(9)}";
				ElementElimination_Manager_UT("@");

				//foreach( var UC in SExo.ExG2.Object.IEGet_UCell_noB(pBOARD,wildcardB) )  UC.CancelB |= wildcardB;

				UInt128 elm = ( SExo.Base81.Aggregate_ConnectedAnd() & FreeCell81b9[noWC] ) .DifSet(SExo.Base81);

				var Base_withWildcard = SExo.Base81.IEGet_UCell_noB(pBOARD,wildcardB).ToList();
				if( Base_withWildcard.Count() == 1 ){
					elm |= ConnC81[ Base_withWildcard[0].rc] & FreeCell81b9[noWC];
				}

				if( elm != 0 ){
					string st = $"[Rule-1] {SExo.stBase}{stWC} is the \"Wildcard\", Then ({elm.TBScmp()}){stWC} are negative.";
					ElementElimination_Manager_BB( SExo, ELM81:elm, noB:wildcardB, st );

					UInt128 elmSx = SExo.ExG2.SLine_x.Get_UInt128_noB(pBOARD,wildcardB);
					if( elmSx.BitCount() == 1 ){
						int rcSx = elmSx.Get_rcFirst();
						int elmNo = pBOARD[rcSx].FreeB.DifSet(wildcardB);
						string stS1 = $"[Rule-1]   And {elmSx.ToRCStringComp()}{stWC} in Sline-2 is positive. Then {rcSx.ToRCString()}#{elmNo.TBScmp()} are negative.";
						ElementElimination_Manager_rB( SExo, rcE:rcSx, noB:elmNo, stS1 );
					}
				}
			}

			return;
		}
	#endregion JE1_Test


	
	#region SE_Test
		private void Test_SE_JE2_Rule0_2orMoreCoverLine( USExocet SExo, bool debugPrint=false ){ 							
			if( SExo.ExocetName != "JE2" )  return;

			int FreeB_valid = SExo.CoverLine_by_Size[2];
			if( FreeB_valid.BitCount() < 2 )  return;
		
			int elmNo = SExo.FreeB0 .DifSet(FreeB_valid);
			if( elmNo>0 ){
				ElementElimination_Manager_UT("@");

				UInt128 elm = SExo.Base81.IEGet_UCell_noB( pBOARD, elmNo).ToList().ToBiitExpression();
				string st = $"[Rule-0] BaseDigit {SExo.stBase}#{FreeB_valid.ToBitStringN(9)} have 2-CoverLine.";
					   st += $" So {SExo.stBase}#{(SExo.FreeB & elmNo).ToBitStringN(9)} can be excluded.";
				ElementElimination_Manager_BB( SExo, elm, noB:elmNo, st );
			}
			return;
		}

		private void Test_SE_Rule01_only_one_S( USExocet SExo, bool debugPrint=false ){
			// Bird's definition: A base candidate that is restricted to only one 'S' cell cover house
			//	is invalid and is false in the base mini-line and target cells.
					//if( SExo.ExocetName!="JE2" && SExo.ExocetName!="JE1" )  return;

			if( SExo.CoverLine_by_Size[1].BitCount() == 0 )  return;
			
			ElementElimination_Manager_UT("@");

			int      noBelm = SExo.CoverLine_by_Size[1];
			if( noBelm > 0 ){
				UInt128  elm81  = SExo.Base81.IEGet_UCell_noB( pBOARD, noBelm).ToList().ToBiitExpression();
				string st = $"[Rule-1] BaseDigit {SExo.stBase}#{noBelm.ToBitStringN(9)} have 1-CoverLine.";
					   st += $" So {SExo.stBase}#{(SExo.FreeB & noBelm).ToBitStringN(9)} can be excluded.";
				ElementElimination_Manager_BB( SExo, elm81, noB:noBelm, st );
			}
			return;
		}


		private void Test_SE_Rule02_Base_Target_Mirror( USExocet SExo, bool debugPrint=false ){
			// Bird's definition:
			//	Any base candidate that isn't capable of being simultaneously true in at least one target cell and its mirror node is false.	
			
			// "JExocet Locked" holds. Looking at Base Digit #a, #a exists in at least one Target-Mirror.
			// E2 is the contrapositive of this. That is, "If #a does not exist in two Target-Mirrors, #a is not a Base Digit.

			ElementElimination_Manager_UT("@");

			int noB1 = _Test_Rule02_Contained_TandM( SExo.ExG1 );	// contained in both Target and Mirror
			int noB2 = _Test_Rule02_Contained_TandM( SExo.ExG2 );	// contained in both Target and Mirror

			int noB = SExo.FreeB0 .DifSet( noB1 | noB2 );		// At least one of them should be included
			if( noB > 0 ){
				string st = $"[Rule-2] #{noB.ToBitStringN(9)} is not contained in Target and Mirror.";
				ElementElimination_Manager_BB( SExo, SExo.Base81, noB:noB, st );
			}
			return;

				int _Test_Rule02_Contained_TandM( UCoverLine ExGM ){
					int noBObj = SExo.FreeB0 & ExGM.Get_Target_FreeB_Updated_Or();  // contained in Target
					int noBMir = SExo.FreeB0 & ExGM.FreeB_Mirror81withFixed;		  // contained in Mirror
						//	WriteLine( $"noBObj:{noBObj.TBS()} noBMir:{noBMir.TBS()}  (noBObj|noBMir):{(noBObj|noBMir).TBS()}" );

					return  (noBObj | noBMir);
				}
		}



		// Rule-3 :
		private void Test_SE_Rule03_non_Base_Candidate( USExocet SExo, bool debugPrint=false ){
			ElementElimination_Manager_UT("@");

			_Test_Rule03_non_Base_Candidate_sub( SExo, SExo.ExG1 );
			_Test_Rule03_non_Base_Candidate_sub( SExo, SExo.ExG2 );
			return;

				void  _Test_Rule03_non_Base_Candidate_sub( USExocet SExo, UCoverLine ExGx ){
					if( ExGx.Object.BitCount()>1 )  return;			// Object types have a dual state (either included or not included).
					int FreeBObjXOR  = ExGx.FreeB_Object81XOR;			// Object candidate 

					int FreeBObjNega = FreeBObjXOR. DifSet(SExo.FreeB0);
					if( FreeBObjNega>0 ){
						string st = $"[Rule-3] Target{ExGx.sq} {ExGx.Object.TBScmp()}#{FreeBObjNega.ToBitStringN(9)}";
							   st += $" is a non-base candidate, then it is negative.";	
						ElementElimination_Manager_BB( SExo, ELM81:ExGx.Object, noB:FreeBObjNega, st );
					}
				}
		}



		// Rule-4 :
		private void Test_SE_Rule04_Target_to_Target( USExocet SExo, bool debugPrint=false ){
			//	E4.If the Base Digits is determined to be positive in one Target, it is negative in the other Target.
			//	Each Target has a different base number.
			//	If one Target is positive, the other Target will be negative.

			ElementElimination_Manager_UT("@");
			_Test_Rule04_Target_to_Target_sub( SExo, SExo.ExG1, SExo.ExG2 );
			_Test_Rule04_Target_to_Target_sub( SExo, SExo.ExG2, SExo.ExG1 );
			return;

				void _Test_Rule04_Target_to_Target_sub( USExocet SExo, UCoverLine ExGA, UCoverLine ExGB ){
					int FreeBObject = ExGA.Object.IEGet_UCell(pBOARD).Aggregate(0, (a,uc) => a | (uc.FreeB.DifSet(uc.CancelB) ) );
					if( FreeBObject.BitCount()==1 ){
						int no = FreeBObject.BitToNum(9);
						if( !ExGB.FreeB_Object81 .IsHit(no) )  return;
						UInt128 elmAand = ExGA.Object.IEGet_UCell_noB(pBOARD,1<<no).Aggregate(qMaxB81,(a,uc) => a & ConnectedCells81[uc.rc] );
						UInt128 elmObj = ExGB.Object & elmAand;	// ObjectA -> ObjectB
						string stTgObj = ((ExGA.Object.BitCount()==1)? "Target": "Object") + $"{ExGA.sq}";

						string st = $"[Rule-4] {stTgObj} {ExGA.stObject}#{no+1} is certin to be positive, then {ExGB.stObject}#{no+1} is negative";
						ElementElimination_Manager_Bn( SExo, ELM81:elmObj, no:no, st );
					}
				}
		}

			
		// Rule-5 :	A base candidate that has a cross-line as an S cell cover house must be false in the target cell in that cross-line
		//          which may make other simple colouring eliminations available. 
		private void Test_SE_Rule05_Candidates_with_CrossLines( USExocet SExo, bool debugPrint=false ){
			// Bird's definition: A base candidate that has a cross-line as an 'S' cell cover house must be false in the target cell in that cross-line
				// E5. The Base Digit with Cross Cover-Line are excluded from the corresponding Target.

			int     FreeB0a = SExo.FreeB0;
			if( SExo.ExocetName.Contains("JE1") || SExo.ExocetName.Contains("Single") )  FreeB0a = FreeB0a.DifSet(SExo.WildCardB);
			foreach( var no in FreeB0a.IEGet_BtoNo() ){
				if( !SExo.CoverLine_by_Size[2].IsHit(no) )  continue;
				var UCL = SExo.CoverStatusList[no];	// Cover Line
				if( UCL == null )  continue;
					if(debugPrint)  WriteLine( $"{sp5}Test_Rule5_  no:#{no+1}  CoverStatusList:{SExo.CoverStatusList[no]}" );


				if( UCL.CLH_1 >= 100 ){
					int hh = UCL.CLH_1%100;
					if( hh == SExo.ExG1.h ) _Test_SE_Rule05_sub( no, SExo.ExG1 );
					if( hh == SExo.ExG2.h ) _Test_SE_Rule05_sub( no, SExo.ExG2 );
				}
				if( UCL.CLH_2 >= 100 ){
					int hh = UCL.CLH_2%100;
					if( hh == SExo.ExG1.h ) _Test_SE_Rule05_sub( no, SExo.ExG1 );
					if( hh == SExo.ExG2.h ) _Test_SE_Rule05_sub( no, SExo.ExG2 );
				}
			}
			return;

					void _Test_SE_Rule05_sub( int no, UCoverLine ExGx ){
						UInt128 Obj81 = ExGx.Object;
						string stTyp = (Obj81.BitCount()==1)? $"Target{ExGx.sq}": $"Object{ExGx.sq}";
						string st = $"[Rule-5] {stTyp} {Obj81.TBScmp()}#{no+1} has a Cross_Line, then negative.";
						ElementElimination_Manager_Bn( SExo, ELM81:Obj81, no:no, st );
					}
		}
				

		// Rule-6 :	Any base candidate that cant be true in the mirror node for a target cell is false in the target cell.
		//			  => Base candidates that are not included in the Mirror are false in the corresponding Target.
		private void Test_SE_Rule06_Mirrored( USExocet SExo, bool debugPrint=false ){			// ... Je2/JE+ type supported	
			int FreeB0 = SExo.FreeB0;
//			if( FreeB0.BitCount() > 2 )  return;	// ... necessary !

			ElementElimination_Manager_UT("@");
			_Rule_06_Mirrored( FreeB0, SExo.ExG1 );
			_Rule_06_Mirrored( FreeB0, SExo.ExG2 );	
			return;

					void _Rule_06_Mirrored( int FreeB0, UCoverLine ExGM ){
						int FreeBObject    = ExGM.FreeB_Object81 & FreeB0;					// Object positive candidate 
						int FreeBMirrorNeg = FreeBObject .DifSet(ExGM.FreeB_Mirror81withFixed);		// FreeB_Mirror81F: given, solved or candidate

						if( FreeBMirrorNeg > 0 ){
							// There are no confirmed(given or solved) candidate digits in the area connected to the Mirror.
							UInt128 Obj81 = ExGM.Object;
							string stTyp = ((Obj81.BitCount()==1)? "Target": "Object") + $"{ExGM.sq}";
							string stMirr = $"#{FreeBMirrorNeg.ToBitStringN(9)}";
							string st = $"[Rule-6] Mirror-{ExGM.sq} cells {ExGM.stMirror81} is missing {stMirr}, then {stTyp} {Obj81.TBScmp()}{stMirr} is negative.";
							ElementElimination_Manager_BB( SExo, ELM81:Obj81, noB:FreeBMirrorNeg, st );
						}	
					}
		}


		// Rule-7 :	If one mirror node cell can only contain non-base digits, the second one will be restricted to the base digits in the opposite object cells.
		private void Test_SE_Rule07_Missing_in_Mirror( USExocet SExo, bool debugPrint=false ){// ... Je2/JE+ type supported

			int FreeB0 = SExo.FreeB0;

			ElementElimination_Manager_UT("@");
			_Rule_07_Missing_in_MirrorSub( FreeB0, SExo.ExG1 );
			_Rule_07_Missing_in_MirrorSub( FreeB0, SExo.ExG2 );			
			return;

					void _Rule_07_Missing_in_MirrorSub( int FreeB_Fixed, UCoverLine ExGM ){
						if( ExGM.Object.BitCount() > 1 )  return;	// Target type

						int  FreeB0 = SExo.FreeB0;
						int  FreeB_Target = ExGM.FreeB_Object81.DifSet(FreeB0);			// not BaseDigits = Target - Base-Digits

						foreach( int rcM in ExGM.Mirror81F.IEGet_rc() ){
							UCell ucM = pBOARD[rcM];
							if( (ucM.noB_FixedFreeB & FreeB0) > 0 )  continue;			// contain Base digits

							int rcM2 = ExGM.Mirror81.Reset(rcM).BitToNum(81);			// second Mirror
							if( rcM2 < 0 )  continue;	
							UCell ucM2 = pBOARD[rcM2];

							int FreeB_Mirror_Positive = ExGM.FreeB_Object81 & FreeB0;	// Base digits in the Object cells
							int noBelm = ucM2.FreeB .DifSet(FreeB_Mirror_Positive);
							if( noBelm.BitCount()!=1 ) continue;							

							FreeB_Mirror_Positive &= ucM2.FreeB;
							string st = $"[Rule-7] Mirror-{ExGM.sq} {rcM.ToRCString()}";
							st += (ucM.No!=0)?   $"#{ucM.No} is fixed.":   $" does not contain #{FreeB0.ToBitStringN(9)}";
							st += $" then {rcM2.ToRCString()}#{FreeB_Mirror_Positive.ToBitStringN(9)} is positive";
							st += $", {rcM2.ToRCString()}#{noBelm.ToBitStringN(9)} is negative,";

							ElementElimination_Manager_rB( SExo, rcM2, noB:noBelm, st );
						}
					}
		}





		private void Test_SE_Rule08_Mirror_nonBase( USExocet SExo, bool debugPrint=false ){	// ... Je2/JE+ type supported
			if( SExo.FreeB0.BitCount() > 2 ) return;

			ElementElimination_Manager_UT("@");
			_Rule_08_lockedSub( SExo, SExo.ExG1 );
			_Rule_08_lockedSub( SExo, SExo.ExG2 );
			return;

					void _Rule_08_lockedSub( USExocet SExo, UCoverLine ExGM ){
						if( ExGM.Mirror81.BitCount() <= 1 )  return;

						int nonBaseFreeB0 = ExGM.FreeB_Mirror81. DifSet(SExo.FreeB0);
						if( nonBaseFreeB0.BitCount() == 1 ){
							int no = nonBaseFreeB0.BitToNum(sz:9);
							UInt128 Mirror_no = ExGM.Mirror81.IEGet_UCell_noB(pBOARD,nonBaseFreeB0).Aggregate(qZero, (a,uc)=> a| qOne<<uc.rc );
							UInt128 Mirror_elm = Mirror_no.Aggregate_ConnectedAnd() & FreeCell81b9[no];
							Mirror_elm = Mirror_elm.DifSet( SExo.ExG1.Object | SExo.ExG2.Object );

							if( Mirror_elm != qZero ){
								string st = $"[Rule-8] Mirrar-{ExGM.sq} {Mirror_no.ToRCStringComp()}#{no+1} is only one possible non-Base Digit";
								st += $", then [{Mirror_elm.ToRCStringComp()}]#{no+1} is negative.";
								ElementElimination_Manager_Bn( SExo, Mirror_elm, no, st );
							}
						}
					}
		}	


		 //Rule 9 : if a Mirror Node contains a locked digit, any other digits it contains of the same type(base or non-base) are false.
		private void Test_SE_Rule09_Mirror_LockedDigits( USExocet SExo, bool debugPrint=false ){	// ... Je2/JE+ type supported
			// Rule-9 requires that another rule be applied to the object type.
			
			if( SExo.FreeB0.BitCount() > 2 ) return;

			UCoverLine ExG1=SExo.ExG1, ExG2=SExo.ExG2;

			ElementElimination_Manager_UT("@");

			_Rule_09_lockedSub( SExo, ExG1, ExG2.Object );
			_Rule_09_lockedSub( SExo, ExG2, ExG1.Object );
			return;

					void _Rule_09_lockedSub( USExocet SExo, UCoverLine ExGM, UInt128 Exclusion ){
						if( ExGM.Object.BitCount() > 1 )  return;  //What are the conditions for the extended type?

						int sq = ExGM.sq;
						int FreeB0 = SExo.FreeB0;

						foreach( int no in ExGM.FreeB_Mirror81.IEGet_BtoNo() ){
							int h_locked = Get_LockedHouse( ExGM.Mirror81, no, Exclusion );
							if( h_locked == 0 )  continue;	// unlocked

							int noB = 1<<no;

							if( (FreeB0&noB)==0 ){				// ... Case : Locked digit(#no) in Mirror is not <BaseDigits>.
								int nFreeBF = ExGM.FreeB_Mirror81 .DifSet(FreeB0 | noB);	
								UInt128 elm = ExGM.Mirror81.IEGet_UCell_noB(pBOARD,nFreeBF).Aggregate(qZero, (a,uc) => a | qOne<<uc.rc );
								string st = $"[Rule-9_nonBase] In Mirrar-{sq} {elm.TBScmp()}#{no+1} is locked non-BaseDigit.";
								st += $" Then {ExGM.stMirror81}#{nFreeBF.ToBitStringN(9)} is negative.";
								ElementElimination_Manager_BB( SExo, ELM81:elm, noB:nFreeBF, st );
							}

							else if( FreeB0.BitCount()==2 ){ //  (FreeB0&noB)>0 ) ...  <mpn_BaseDigits> & Locked
								// Case :  Locked digit(#no) in Mirror is BaseDigits. Locked BaseDigits is a necessary condition.
								int nFreeBF = FreeB0.DifSet(noB);
								UInt128 elm = ExGM.Mirror81.IEGet_UCell_noB(pBOARD,nFreeBF).Aggregate(qZero, (a,uc) => a | qOne<<uc.rc );

								string st = $"[Rule-9_Base] In Mirrar-{sq} {elm.TBScmp()}#{no+1} is locked BaseDigit.";
								st += $" Then {ExGM.stMirror81}#{nFreeBF.ToBitStringN(9)} is negative.";
								ElementElimination_Manager_Bn( SExo, ELM81:elm, no:no, st );
							}
						}
					}
		}	


		// Rule 10 : known Base Digit is false in all cells in full sight of either both Base Cells, or both Target Cells.
		private void Test_SE_Rule10_known_BaseDigit( USExocet SExo, bool debugPrint=false ){	// ... Je2/JE+ type supported	
		
			//if( SExo.ExocetNamePlus.Contains("SE_single") )  return;	 // <<===== SExocet Debug =====

			// Base Digit is the solution in one of the Base Cells, then it must also be the solution in one of the Target Cells.
			var (rcB1,rcB2) = SExo.Base81.BitToTupple();
			int szBase = 2;
			if( rcB2 < 0 ){ szBase=1; rcB2=rcB1; }	// for SE_SingleBase.(When one is negative.)

			if( SExo.FreeB0.BitCount() > szBase )  return;	// "if both Base Digits are determined"

			UInt128  fullSight = (ConnectedCells81[rcB1] & ConnectedCells81[rcB2]);	// Base1 & Base2
			fullSight = fullSight.DifSet( qOne<<rcB1 | qOne<<rcB2 );


			fullSight |= ( SExo.ExG1.Get_FullSight(withExclusion:true) & SExo.ExG2.Get_FullSight(withExclusion:true) );	// or Target1 & target2 ... Je2/JE+ type supported
					if(debugPrint)  WriteLine( fullSight.ToBitString81N() );

			ElementElimination_Manager_UT("@");
			string st_elm = "";
			foreach( int no in SExo.FreeB0.IEGet_BtoNo() ){
				UInt128 elm = FreeCell81b9[no] & fullSight; 
				if( elm != qZero ){
					st_elm += " "+elm.ToRCStringComp_AddNo($"#{no+1}");
					ElementElimination_Manager_Bn( SExo, elm, no );
				}
			}

			if( st_elm!="" ){
				st_elm = st_elm.Replace(" } { "," ");
				string st = $"[Rule-10] Base({SExo.stBase}) are Fixed, then {st_elm.Trim()} are negative.\n          (Both bases or both targets are in focus)";
				ElementElimination_Manager_st( st );
			}

		}


		// Rule 11 : Handling Escape Cells and non-S cells
		//	A known base digit, or one that can only occur once in the escape cells in the cross-lines, is false in the non-S cells in its cover houses.
		//  The only cross-line cells a base digit can occupy in the JE band are one of the two target cells when its true or two of the escape cells when its false. When its can only occupy one of the escape cells then it too will be restricted to a single instance in the cross-line cells in the JE band and so must be true in two S cells.
		private void Test_SE_Rule11_EscapeCells_nonSCells( USExocet SExo, bool debugPrint=false ){
			int FreeBnon = SExo.FreeB.DifSet(SExo.FreeB0);
			FreeBnon &= SExo.CoverLine_by_Size[2];	// Satisfy Cover-Line condition.
			if( FreeBnon == 0 )  return;
			
			int dir = SExo.dir;
			ElementElimination_Manager_UT("@");
			UInt128 EscapeCrossLine =  SExo.EscapeCells & SExo.CrossLine_012;
			UCoverLine ExG1=SExo.ExG1, ExG2=SExo.ExG2;
			foreach( int no in FreeBnon.IEGet_BtoNo() ){
				UInt128 CLwithno = EscapeCrossLine& FreeCell81b9[no];
				if( CLwithno==qZero || CLwithno.BitCount()!=1 ) continue;

				_Test_SE_Rule11_sub( ExG1, no );
				_Test_SE_Rule11_sub( ExG2, no );
			}

						void  _Test_SE_Rule11_sub( UCoverLine ExGx, int no ){
							UInt128 Swithno = ExGx.SLine_x & FreeCell81b9[no];
							if( Swithno==qZero || Swithno.BitCount()!=1 ) return;
							int rc = Swithno.FindFirst_rc();
							UInt128 elm = (HC81[(dir,rc).DRCHf()] & FreeCell81b9[no]) .DifSet(SExo.CrossLine_012);
							if( elm == qZero )  return;
					
							string st = $"[Rule-11] Base({SExo.stBase}) are Fixed";
							st += $", then {elm.ToRCStringComp_AddNo(stAdd:$"#{no+1}")} are negative. They prevent the Base from becoming positive.";
							ElementElimination_Manager_Bn( SExo, elm, no, st );
						}
		}


		// Rule 12 :
		private void Test_SE_Rule12_Prevent_knownBase_from_beingTrue( USExocet SExo, bool debugPrint=false ){

			int szBase = SExo.Base81.BitCount();
			if( SExo.FreeB0.BitCount() > szBase )  return;// for SE_SingleBase.(When one is negative.)

			ElementElimination_Manager_UT("@");
			foreach( var no in SExo.FreeB0.IEGet_BtoNo() ){
				try{
					var UCL = SExo.CoverStatusList[no];	// CLB:CrossLine
					if( UCL != null ){
						//int h0=UCL.CL0idx, h1=UCL.CL1idx;
						int h0=UCL.CLH_0%100, h1=UCL.CLH_1%100;

						UInt128 elm81 = SExo.Base81.Aggregate_ConnectedAnd() & FreeCell81b9[no];
						string st = $"[Rule-12] Base({SExo.stBase}) are Fixed";
						st += $", then ({elm81.TBScmp()})#{no+1} are negative. They prevent the Base from becoming positive.";
						ElementElimination_Manager_Bn( SExo, elm81, no, st );
					}
				}
				catch(Exception e ){ WriteLine( $"{e.Message}\n{e.StackTrace}" ); }
			}
		}

		// Rule 13 :
		private void Test_SE_Rule13_missing_BaseDigit_is_negated( USExocet SExo, bool debugPrint=false ){
			// If the target is confirmed, the missing base digit is negated in the mirrored target cell.

			ElementElimination_Manager_UT("@");

			_Test_Rule13_missing_BaseDigit_is_negated( SExo, SExo.ExG1 );
			_Test_Rule13_missing_BaseDigit_is_negated( SExo, SExo.ExG2 );


			void _Test_Rule13_missing_BaseDigit_is_negated( USExocet SExo, UCoverLine ExGx ){
				int FreeBObject = ExGx.Object.IEGet_UCell(pBOARD).Aggregate(0, (a,uc) => a | (uc.FreeB.DifSet(uc.CancelB) ) );
				if( FreeBObject.BitCount()==1 ){
					int no = FreeBObject.BitToNum();
					int noB = 1<<no;
					var MirrorElmList = ExGx.Mirror81.IEGet_UCell_noB(pBOARD,noB).ToList();
					if( MirrorElmList.Count() != 1 )  return;

					UCell UCB = MirrorElmList[0];
					int   noBelm = UCB.FreeB.BitReset(no);
					if( noBelm == 0 )  return;

					string st = $"[Rule-13] {ExGx.stObject}#{no+1} is confirmed to be positive, then {UCB.rc.ToRCString()}#{noBelm.TBScmp()} is negative";
					ElementElimination_Manager_rB( SExo, rcE:UCB.rc, noB:noBelm, st );
				}
			}
		}

	#endregion SE_Test

	}


}
