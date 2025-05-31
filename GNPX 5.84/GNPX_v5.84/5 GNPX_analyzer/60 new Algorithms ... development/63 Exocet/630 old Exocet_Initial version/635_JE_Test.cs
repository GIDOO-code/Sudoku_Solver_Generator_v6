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
using static GNPX_space.Senior_Exocet_TechGen;
using static System.Formats.Asn1.AsnWriter;
using System.Windows.Media.Media3D;

namespace GNPX_space{
	// Reference to SudokuCoach.
	// https://www.taupierbw.be/SudokuCoach/SC_Exocet.shtml 


    public partial class JExocet_TechGen: AnalyzerBaseV2{

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
		private bool ElementElimination_Manager_rB( UExocet Exo, int rcE, int noB, string st="" ){
			if( st!="" && !extResultLst.Contains(st) )  extResultLst.Add(st);

			if( rcE == 999 )  return false;

			pBOARD[rcE%100].CancelB |= (pBOARD[rcE%100].FreeB)&noB;		
			return true;
		}
		private bool ElementElimination_Manager_Bn(  UExocet Exo, UInt128 ELM81, int no, string st="" ){
			if( st!="" && !extResultLst.Contains(st) )  extResultLst.Add(st);

			int noB = 1<<no;
			foreach( int rc in ELM81.IEGet_rc(81) ){
				if( pBOARD[rc].No != 0 )  continue;
				pBOARD[rc].CancelB |= (pBOARD[rc].FreeB)&noB;
			}
			if( st!="" && !extResultLst.Contains(st) )  extResultLst.Add(st);
			return true;
		}
		private bool ElementElimination_Manager_BB( UExocet Exo, UInt128 ELM81, int noB, string st="" ){
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

	#region Test

		private void Test_JE2_Rule0_2orMoreCoverLine( UExocet Exo, bool debugPrint=false ){ 							
			if( Exo.ExocetName != "JE2" )  return;

			int FreeB_valid = Exo.CoverLine_by_Size[2];
			if( FreeB_valid.BitCount() < 2 )  return;
		
			int elmNo = Exo.FreeB0 .DifSet(FreeB_valid);
			if( elmNo>0 ){
				ElementElimination_Manager_UT("@");

				UInt128 elm = Exo.Base81.IEGet_UCell_noB( pBOARD, elmNo).ToList().ToBiitExpression();
				string st = $"[Rule-0] BaseDigit {Exo.stBase}#{FreeB_valid.ToBitStringN(9)} have 2-CoverLine.";
					   st += $" So {Exo.stBase}#{(Exo.FreeB & elmNo).ToBitStringN(9)} can be excluded.";
				ElementElimination_Manager_BB( Exo, elm, noB:elmNo, st );
			}
			return;
		}






		private void Test_Rule01_only_one_S( UExocet Exo, bool debugPrint=false ){
			// Bird's definition: A base candidate that is restricted to only one 'S' cell cover house
			//	is invalid and is false in the base mini-line and target cells.

			if( Exo.ExocetName!="JE2" && Exo.ExocetName!="JE1" )  return;

			if( Exo.CoverLine_by_Size[1].BitCount() == 0 )  return;
			
			ElementElimination_Manager_UT("@");

			int      noBelm = Exo.CoverLine_by_Size[1];
			if( noBelm > 0 ){
				UInt128  elm81  = Exo.Base81.IEGet_UCell_noB( pBOARD, noBelm).ToList().ToBiitExpression();
				string st = $"[Rule-1] BaseDigit {Exo.stBase}#{noBelm.ToBitStringN(9)} have 1-CoverLine.";
					   st += $" So {Exo.stBase}#{(Exo.FreeB & noBelm).ToBitStringN(9)} can be excluded.";
				ElementElimination_Manager_BB( Exo, elm81, noB:noBelm, st );
			}
			return;
		}


		private void Test_Rule02_Base_Target_Mirror( UExocet Exo, bool debugPrint=false ){
			// Bird's definition:
			//	Any base candidate that isn't capable of being simultaneously true in at least one target cell and its mirror node is false.	
			
			// "JExocet Locked" holds. Looking at Base Digit #a, #a exists in at least one Target-Mirror.
			// E2 is the contrapositive of this. That is, "If #a does not exist in two Target-Mirrors, #a is not a Base Digit.

			ElementElimination_Manager_UT("@");

			int noB1 = _Test_Rule02_Contained_TandM( Exo.ExG1 );	// contained in both Target and Mirror
			int noB2 = _Test_Rule02_Contained_TandM( Exo.ExG2 );	// contained in both Target and Mirror

			int noB = Exo.FreeB0 .DifSet( noB1 | noB2 );		// At least one of them should be included
			if( noB > 0 ){
				string st = $"[Rule-2] #{noB.ToBitStringN(9)} is not contained in Target and Mirror.";
				ElementElimination_Manager_BB( Exo, Exo.Base81, noB:noB, st );
			}
			return;

				int _Test_Rule02_Contained_TandM( UExocet_elem ExGM ){
					int noBObj = Exo.FreeB0 & ExGM.Get_Target_FreeB_Updated_Or();  // contained in Target
					int noBMir = Exo.FreeB0 & ExGM.FreeB_Mirror81withFixed;		  // contained in Mirror
						//	WriteLine( $"noBObj:{noBObj.TBS()} noBMir:{noBMir.TBS()}  (noBObj|noBMir):{(noBObj|noBMir).TBS()}" );

					return  (noBObj | noBMir);
				}
		}



		// Rule-3 :
		private void Test_Rule03_non_Base_Candidate( UExocet Exo, bool debugPrint=false ){
			ElementElimination_Manager_UT("@");

			_Test_Rule03_non_Base_Candidate_sub( Exo, Exo.ExG1 );
			_Test_Rule03_non_Base_Candidate_sub( Exo, Exo.ExG2 );
			return;

				void  _Test_Rule03_non_Base_Candidate_sub( UExocet Exo, UExocet_elem ExGx ){
					if( ExGx.Object81.BitCount()>1 )  return;			// Object types have a dual state (either included or not included).
					int FreeBObjXOR  = ExGx.FreeB_Object81XOR;			// Object candidate 

					int FreeBObjNega = FreeBObjXOR. DifSet(Exo.FreeB0);
					if( FreeBObjNega>0 ){
						string st = $"[Rule-3] Target{ExGx.sq} {ExGx.Object81.TBScmp()}#{FreeBObjNega.ToBitStringN(9)}";
							   st += $" is a non-base candidate, then it is negative.";	
						ElementElimination_Manager_BB( Exo, ELM81:ExGx.Object81, noB:FreeBObjNega, st );
					}
				}
		}



		// Rule-4 :
		private void Test_Rule04_Target_to_Target( UExocet Exo, bool debugPrint=false ){
			//	E4.If the Base Digits is determined to be positive in one Target, it is negative in the other Target.
			//	Each Target has a different base number.
			//	If one Target is positive, the other Target will be negative.

			ElementElimination_Manager_UT("@");
			_Test_Rule04_Target_to_Target_sub( Exo, Exo.ExG1, Exo.ExG2 );
			_Test_Rule04_Target_to_Target_sub( Exo, Exo.ExG2, Exo.ExG1 );
			return;

				void _Test_Rule04_Target_to_Target_sub( UExocet Exo, UExocet_elem ExGA, UExocet_elem ExGB ){
					int FreeBObject = ExGA.Object81.IEGet_UCell(pBOARD).Aggregate(0, (a,uc) => a | (uc.FreeB.DifSet(uc.CancelB) ) );
					if( FreeBObject.BitCount()==1 ){
						int no = FreeBObject.BitToNum(9);
						if( !ExGB.FreeB_Object81 .IsHit(no) )  return;
						UInt128 elmAand = ExGA.Object81.IEGet_UCell_noB(pBOARD,1<<no).Aggregate(qMaxB81,(a,uc) => a & ConnectedCells81[uc.rc] );
						UInt128 elmObj = ExGB.Object81 & elmAand;	// ObjectA -> ObjectB
						string stTgObj = ((ExGA.Object81.BitCount()==1)? "Target": "Object") + $"{ExGA.sq}";

						string st = $"[Rule-4] {stTgObj} {ExGA.stObject81}#{no+1} is certin to be positive, then {ExGB.stObject81}#{no+1} is negative";
						ElementElimination_Manager_Bn( Exo, ELM81:elmObj, no:no, st );
					}
				}
		}


		// Rule-5 :	A base candidate that has a cross-line as an S cell cover house must be false in the target cell in that cross-line
		//          which may make other simple colouring eliminations available. 
		private void Test_Rule05_Candidates_with_CrossLines( UExocet Exo, bool debugPrint=false ){
			// Bird's definition: A base candidate that has a cross-line as an 'S' cell cover house must be false in the target cell in that cross-line
				// E5. The Base Digit with Cross Cover-Line are excluded from the corresponding Target.

			int		FreeB0 = Exo.FreeB0;
			foreach( var no in FreeB0.IEGet_BtoNo() ){
				var UCL = Exo.CoverStatusList[no];	// Cover Line
				if( UCL == null )  continue;
					if(debugPrint)  WriteLine( $"{sp5}Test_Rule5_  no:#{no+1}  CoverStatusList:{Exo.CoverStatusList[no]}" );

				int h0 = UCL.CL0idx;
				if( h0 >= 9 ){		// is Cross?  Specifications: Cross cover line is in CL0(not in CL1).
					var ExGx = (h0==9)? Exo.ExG1: Exo.ExG2;
					UInt128 Obj81 = ExGx.Object81;
					string stTyp = (Obj81.BitCount()==1)? $"Target{ExGx.sq}": $"Object{ExGx.sq}";
					string st = $"[Rule-5] {stTyp} {Obj81.TBScmp()}#{no+1} has a Cross_Line, then negative.";
					ElementElimination_Manager_Bn( Exo, ELM81:Obj81, no:no, st );
				}
			}
			return;
		}
				

		// Rule-6 :	Any base candidate that cant be true in the mirror node for a target cell is false in the target cell.
		//			  => Base candidates that are not included in the Mirror are false in the corresponding Target.
		private void Test_Rule06_Mirrored( UExocet Exo, bool debugPrint=false ){			// ... Je2/JE+ type supported	
			int FreeB0 = Exo.FreeB0;
//			if( FreeB0.BitCount() > 2 )  return;	// ... necessary !

			ElementElimination_Manager_UT("@");
			_Rule_06_Mirrored( FreeB0, Exo.ExG1 );
			_Rule_06_Mirrored( FreeB0, Exo.ExG2 );	
			return;

					void _Rule_06_Mirrored( int FreeB0, UExocet_elem ExGM ){
						int FreeBObject    = ExGM.FreeB_Object81 & FreeB0;					// Object positive candidate 
						int FreeBMirrorNeg = FreeBObject .DifSet(ExGM.FreeB_Mirror81withFixed);		// FreeB_Mirror81F: given, solved or candidate

						if( FreeBMirrorNeg > 0 ){
							// There are no confirmed(given or solved) candidate digits in the area connected to the Mirror.
							UInt128 Obj81 = ExGM.Object81;
							string stTyp = ((Obj81.BitCount()==1)? "Target": "Object") + $"{ExGM.sq}";
							string stMirr = $"#{FreeBMirrorNeg.ToBitStringN(9)}";
							string st = $"[Rule-6] Mirror-{ExGM.sq} cells {ExGM.stMirror81} is missing {stMirr}, then {stTyp} {Obj81.TBScmp()}{stMirr} is negative.";
							ElementElimination_Manager_BB( Exo, ELM81:Obj81, noB:FreeBMirrorNeg, st );
						}	
					}
		}


		// Rule-7 :	If one mirror node cell can only contain non-base digits, the second one will be restricted to the base digits in the opposite object cells.
		private void Test_Rule07_Missing_in_Mirror( UExocet Exo, bool debugPrint=false ){// ... Je2/JE+ type supported

			int FreeB0 = Exo.FreeB0;
			//if( FreeB0.BitCount() > 2 )  return;	// ... necessary. 

			ElementElimination_Manager_UT("@");
			_Rule_07_Missing_in_MirrorSub( FreeB0, Exo.ExG1 );
			_Rule_07_Missing_in_MirrorSub( FreeB0, Exo.ExG2 );			
			return;

					void XXX_Rule_07_Missing_in_MirrorSub( int FreeB_Fixed, UExocet_elem ExGM ){
						if( ExGM.Object81.BitCount() > 1 )  return;	// Target type

						int  FreeB0 = Exo.FreeB0;
						int  FreeB_Target = ExGM.FreeB_Object81.DifSet(FreeB0);	// not BaseDigits = Target - Base-Digits
						if( FreeB_Target == 0 )  return;

						foreach( int rcM in ExGM.Mirror81F.IEGet_rc() ){
							UCell ucM = pBOARD[rcM];
							if( (ucM.noB_FixedFreeB&FreeB_Target) != 0 )  continue;

							int rcM2 = ExGM.Mirror81.Reset(rcM).BitToNum(81);
							if( rcM2 < 0 )  continue;	
							UCell ucM2 = pBOARD[rcM2];

							int FreeB_Mirror_Positive = ExGM.FreeB_Mirror81&FreeB0;
							int noBelm = ucM2.FreeB .DifSet(FreeB_Mirror_Positive);
							if( noBelm.BitCount()!=1 ) continue;							

							FreeB_Mirror_Positive &= ucM2.FreeB;
							string st = $"[Rule-7] Mirror-{ExGM.sq} {rcM.ToRCString()}";
							st += (ucM.No!=0)?   $"#{ucM.No} is fixed.":   $" does not contain #{FreeB0.ToBitStringN(9)}";
							st += $" then {rcM2.ToRCString()}#{FreeB_Mirror_Positive.ToBitStringN(9)} is positive";
							st += $", {rcM2.ToRCString()}#{noBelm.ToBitStringN(9)} is negative,";

							ElementElimination_Manager_rB( Exo, rcM2, noB:noBelm, st );
						}
					}

					void _Rule_07_Missing_in_MirrorSub( int FreeB_Fixed, UExocet_elem ExGM ){
						if( ExGM.Object81.BitCount() > 1 )  return;	// Target type

						int  FreeB0 = Exo.FreeB0;
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

							ElementElimination_Manager_rB( Exo, rcM2, noB:noBelm, st );
						}
					}
		}



		private void Test_Rule08_Mirror_nonBase( UExocet Exo, bool debugPrint=false ){	// ... Je2/JE+ type supported
			if( Exo.FreeB0.BitCount() > 2 ) return;

			ElementElimination_Manager_UT("@");
			_Rule_08_lockedSub( Exo, Exo.ExG1 );
			_Rule_08_lockedSub( Exo, Exo.ExG2 );
			return;

					void _Rule_08_lockedSub( UExocet Exo, UExocet_elem ExGM ){
						int nonBaseFreeB0 = ExGM.FreeB_Mirror81. DifSet(Exo.FreeB0);
						if( nonBaseFreeB0.BitCount() == 1 ){
							int no = nonBaseFreeB0.BitToNum(sz:9);
							UInt128 Mirror_no = ExGM.Mirror81.IEGet_UCell_noB(pBOARD,nonBaseFreeB0).Aggregate(qZero, (a,uc)=> a| qOne<<uc.rc );
							UInt128 Mirror_elm = Mirror_no.Aggregate_ConnectedAnd() & FreeCell81b9[no];
							Mirror_elm = Mirror_elm.DifSet( Exo.ExG1.Object81 | Exo.ExG2.Object81 );

							if( Mirror_elm != qZero ){
								string st = $"[Rule-8] Mirrar-{ExGM.sq} {Mirror_no.ToRCStringComp()}#{no+1} is only one possible non-Base Digit";
								st += $", then [{Mirror_elm.ToRCStringComp()}]#{no+1} is negative.";
								ElementElimination_Manager_Bn( Exo, Mirror_elm, no, st );
							}
						}
					}
		}	


		 //Rule 9 : if a Mirror Node contains a locked digit, any other digits it contains of the same type(base or non-base) are false.
		private void Test_Rule09_Mirror_LockedDigits( UExocet Exo, bool debugPrint=false ){	// ... Je2/JE+ type supported
			// Rule-9 requires that another rule be applied to the object type.
			//if( Exo.FreeB0.BitCount() > 2 ) return;

			UExocet_elem ExG1=Exo.ExG1, ExG2=Exo.ExG2;

			ElementElimination_Manager_UT("@");

			_Rule_09_lockedSub( Exo, ExG1, ExG2.Object81 );
			_Rule_09_lockedSub( Exo, ExG2, ExG1.Object81 );
			return;

					void _Rule_09_lockedSub( UExocet Exo, UExocet_elem ExGM, UInt128 Exclusion ){
						if( ExGM.Object81.BitCount() > 1 )  return;  //What are the conditions for the extended type?

						int sq = ExGM.sq;
						int FreeB0 = Exo.FreeB0;

						foreach( int no in ExGM.FreeB_Mirror81.IEGet_BtoNo() ){
							int h_locked = Get_LockedHouse( ExGM.Mirror81, no, Exclusion );
							if( h_locked == 0 )  continue;	// unlocked

							int noB = 1<<no;
							if( (FreeB0&noB)>0 && FreeB0.BitCount()==2){
								int nFreeBF = FreeB0.DifSet(noB);
								UInt128 elm = ExGM.Mirror81.IEGet_UCell_noB(pBOARD,nFreeBF).Aggregate(qZero, (a,uc) => a | qOne<<uc.rc );

								string st = $"[Rule-9_Base] In Mirrar-{sq} {elm.TBScmp()}#{no+1} is locked BaseDigit.";
								st += $" Then {ExGM.stMirror81}#{nFreeBF.ToBitStringN(9)} is negative.";
								ElementElimination_Manager_Bn( Exo, ELM81:elm, no:no, st );
							}

							else if( (FreeB0&noB)==0 ){				// ... Locked digit(#no) is not Base Digits.
								int nFreeBF = ExGM.FreeB_Mirror81 .DifSet(FreeB0 | noB);	
								UInt128 elm = ExGM.Mirror81.IEGet_UCell_noB(pBOARD,nFreeBF).Aggregate(qZero, (a,uc) => a | qOne<<uc.rc );
								string st = $"[Rule-9_nonBase] In Mirrar-{sq} {elm.TBScmp()}#{no+1} is locked non-BaseDigit.";
								st += $" Then {ExGM.stMirror81}#{nFreeBF.ToBitStringN(9)} is negative.";
								ElementElimination_Manager_BB( Exo, ELM81:elm, noB:nFreeBF, st );
							}
						}
					}
		}	


		// Rule 10 : known Base Digit is false in all cells in full sight of either both Base Cells, or both Target Cells.
		private void Test_Rule10_known_BaseDigit( UExocet Exo, bool debugPrint=false ){	// ... Je2/JE+ type supported

			if( Exo.FreeB0.BitCount() > 2 )  return;	// "if both Base Digits are determined"

			// Base Digit is the solution in one of the Base Cells, then it must also be the solution in one of the Target Cells.
			var (rcB1,rcB2) = Exo.Base81.BitToTupple();
			UInt128  fullSight = (ConnectedCells81[rcB1] & ConnectedCells81[rcB2]);	// Base1 & Base2
			fullSight = fullSight.DifSet( qOne<<rcB1 | qOne<<rcB2 );
			fullSight |= ( Exo.ExG1.Get_FullSight(withExclusion:true) & Exo.ExG2.Get_FullSight(withExclusion:true) );	// or Target1 & target2 ... Je2/JE+ type supported
					if(debugPrint)  WriteLine( fullSight.ToBitString81N() );

			ElementElimination_Manager_UT("@");
			string st_elm = "";
			foreach( int no in Exo.FreeB0.IEGet_BtoNo() ){
				UInt128 elm = FreeCell81b9[no] & fullSight; 
				if( elm != qZero ){
					st_elm += " "+elm.ToRCStringComp_AddNo($"#{no+1}");
					ElementElimination_Manager_Bn( Exo, elm, no );
				}
			}

			if( st_elm!="" ){
				st_elm = st_elm.Replace(" } { "," ");
				string st = $"[Rule-10] Base({Exo.stBase}) are Fixed, then {st_elm.Trim()} are negative.\n          (Both bases or both targets are in focus)";
				ElementElimination_Manager_st( st );
			}
		}


		// Rule 11 : Handling Escape Cells and non-S cells
	/*
		11.	A known base digit, or one that can only occur once in the escape cells in the cross-lines, is false in the non-S cells in its cover houses.
		既知のベース数字、またはクロスラインのエスケープセルに 1 回しか出現できない数字は、そのカバーハウスのS以外のセルでは偽です。
		The only cross-line cells a base digit can occupy in the JE band are one of the two target cells when its true or two of the escape cells when its false. When its can only occupy one of the escape cells then it too will be restricted to a single instance in the cross-line cells in the JE band and so must be true in two S cells.
		ベース数字が JEバンドで占有できるクロスラインセルは、それが真の場合は 2つのターゲットセルのうちの 1つ、それが偽の場合は 2つのエスケープセルです。エスケープセルのうちの 1つしか占有できない場合は、JEバンドのクロスラインセルでも 1つのインスタンスに制限されるため、2つのSセルで真である必要があります。
	*/

		private void Test_Rule11_EscapeCells_nonSCells( UExocet Exo, bool debugPrint=false ){




		// Rule-11 is invalid because it does not meet the requirements of JE2.
		}


		// Rule 12 :
		private void Test_Rule12_Prevent_knownBase_from_beingTrue( UExocet Exo, bool debugPrint=false ){
		if( Exo.FreeB0.BitCount() > 2 )  return;
		ElementElimination_Manager_UT("@");
			foreach( var no in Exo.FreeB0.IEGet_BtoNo() ){
				var UCL = Exo.CoverStatusList[no];	// CLB:CrossLine
				if( UCL != null ){
					int h0=UCL.CL0idx, h1=UCL.CL1idx;

					UInt128 elm81 = FreeCell81b9[no] .DifSet(Exo.Band81 | Exo.SLine_012);
					if(h1>=0)	elm81 &= HouseCells81[h0] | HouseCells81[h1];
					else		elm81 &= HouseCells81[h0];

					string st = $"[Rule-12] Base({Exo.stBase}) are Fixed";
					st += $", then {elm81.ToRCStringComp_AddNo(stAdd:$"#{no+1}")} are negative. They prevent the Base from becoming positive.";
					ElementElimination_Manager_Bn( Exo, elm81, no, st );
				}
			}
		}

		// Rule 13 :
		private void Test_Rule13_missing_BaseDigit_is_negated( UExocet Exo, bool debugPrint=false ){
			// If the target is confirmed, the missing base digit is negated in the mirrored target cell.

			ElementElimination_Manager_UT("@");

			_Test_Rule13_missing_BaseDigit_is_negated( Exo, Exo.ExG1 );
			_Test_Rule13_missing_BaseDigit_is_negated( Exo, Exo.ExG2 );




			void _Test_Rule13_missing_BaseDigit_is_negated( UExocet Exo, UExocet_elem ExGx ){
				int FreeBObject = ExGx.Object81.IEGet_UCell(pBOARD).Aggregate(0, (a,uc) => a | (uc.FreeB.DifSet(uc.CancelB) ) );
				if( FreeBObject.BitCount()==1 ){
					int no = FreeBObject.BitToNum();
					int noB = 1<<no;
					var MirrorElmList = ExGx.Mirror81.IEGet_UCell_noB(pBOARD,noB).ToList();
					if( MirrorElmList.Count() != 1 )  return;

					UCell UCB = MirrorElmList[0];
					int   noBelm = UCB.FreeB.BitReset(no);
					if( noBelm == 0 )  return;

					string st = $"[Rule-13] {ExGx.stObject81}#{no+1} is confirmed to be positive, then {UCB.rc.ToRCString()}#{noBelm.TBScmp()} is negative";
					ElementElimination_Manager_rB( Exo, rcE:UCB.rc, noB:noBelm, st );
				}
			}
		}

	#endregion _Test

	}


}
