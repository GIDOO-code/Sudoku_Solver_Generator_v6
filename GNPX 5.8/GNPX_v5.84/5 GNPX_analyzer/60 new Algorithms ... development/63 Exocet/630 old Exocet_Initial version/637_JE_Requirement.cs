using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.Windows.Shapes;

namespace GNPX_space{
    public partial class JExocet_TechGen: AnalyzerBaseV2{
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 

		private string		stType( bool typeB ) => typeB? "Diagonal": "Aligned";
		private int			_Digit_ToFreeB( int rc ) => (rc>=100)? (1<<(pBOARD[rc%100].No-1)): pBOARD[rc].FreeB;
		private int			IsFixed( int rc ) => (pBOARD[rc].No!=0)? rc+100: rc; //if Cell is Fixed, be a negative value	

	  #region Conditions for the Exocet to be established
			
		private IEnumerable<UExocet> IEGet_Exocet_Requirement_R1_BasicForm( bool debugPrint=false ){
			// dir, StemCell => basecells, FreeB, SLine0, block1, block2

			for( int dir=0; dir<2; dir++ ){
				for( int rcStem=0; rcStem<81; rcStem++ ){
		
					// ... Base cells
					int     block0 = rcStem.ToBlock(); 
					int     hBase = (dir,rcStem).DirRCtoHouse( );
					UInt128 base81 = (HouseCells81[hBase] & HouseCells81[block0+18] & BOARD_FreeCell81) .Reset(rcStem);
					if( base81.BitCount() != 2 )  continue;

					// ... The two Base-Cell have some digits in common.
						//	int FreeBAnd = baseCells.IEGet_UCell(pBOARD).Aggregate(0x1FF, (a,uc) => a & uc.FreeB);
						//	if( FreeBAnd.BitCount() == 0 ) continue;		// Cases with no common digits are excluded

					// ... Number of candidate digits in the base cell
					int FreeB0 = base81.IEGet_UCell(pBOARD).Aggregate( 0, (a,UC) => a | UC.FreeB );
					int FreeBC = FreeB0.BitCount();
			
					// [req1] The Base Cells together contain 3 or 4 Candidates, that we call Base Candidates
					//        (outside these Cells and the Target Cells, they are called Base Digits)
					// ... The upper limit of this condition is relaxed.
					//     Search time will increase, but it is questionable whether the possibility will increase.
    				//if( FreeBC<3 || FreeBC>4 )  continue;		// 2:LockedSet 5-:Hopeful predictions
					if( FreeBC<3 )  continue;		// 2:LockedSet 5-:Hopeful predictions

					// -------------------------------------------------------------------------------
					UExocet Exo_0 = new( dir, rcStem, base81, FreeB0 );
					//if( Exo.SLine0 == qZero )  continue;
					if( Exo_0.ExG0.SLine_Sx.IEGet_UCell(pBOARD).Any( P=> P.No==0 && P.FreeBC==0) ){
						
						var se = new Gidoo_EventHandler( eName:"SystemError", Message:"IEGet_Exocet_Requirement_R1_BasicForm" );
						Send_Command_to_GNPXwin( this, se );
						continue;
					}
					// -------------------------------------------------------------------------------
							if(debugPrint) WriteLine( $"{Exo_0}" );

					yield return Exo_0;
				}
			}
			yield break;
		}


		private IEnumerable< UExocet > JEeocet_Requirement_R1_BasicForm_Target_IEGet( UExocet Exo, bool debugPrint=false ){
			int dir	   = Exo.dir;
			int rcStem = Exo.rcStem;
			int FreeB0 = Exo.FreeB0;

			int		house_SLine0 = (dir==0)? (rcStem%9+9): (rcStem/9);
			UInt128 CrossLine0   = HouseCells81[house_SLine0];
			UInt128 SLine0	     =  CrossLine0 .DifSet( HouseCells81[rcStem.B()+18] );

			// <<< Target Cell >>>
			// [req2_1] The Target Cells belong to the same Band as the Base Cells but do not see the Base Cells
			foreach( var TgObj1 in IEGet_TargetObject_JE2( Exo,1,debugPrint:false) ){									

				var (house_SLine1,CrossLine1,SLine1) = _Get_SLine_withObject( dir, TgObj1, Exo.Block1 );	// Sline_1
				if( SLine1.IEGet_rc().All(rc=>pBOARD[rc].FreeBC==0) )  continue;			// Skip if all Sline cells are confirmed

				int FreeB_Tg1 = TgObj1.IEGet_UCell(pBOARD).Aggregate( 0, (a,uc) => a| uc.FreeB_Updated() );
				int rcCmp1    = _Get_rcCompanion( Exo, TgObj1, debugPrint:false );			 // Companion Cell_1
				if( (rcCmp1>=0 & rcCmp1<100)  &&  (_Digit_ToFreeB(rcCmp1) & FreeB_Tg1)>0 )   continue;		// [req4_1] C1 does not contain any Base Digit present in T1. 						
				// ----------------------------------------------------------------------------------------------------------------


				// [req2_2] The Target Cells belong to the same Band as the Base Cells but do not see the Base Cells
				foreach( var TgObj2 in IEGet_TargetObject_JE2( Exo,2, debugPrint:false) ){
					var (house_SLine2,CrossLine2,SLine2) = _Get_SLine_withObject( dir, TgObj2, Exo.Block2 );	// Sline_2
					if( SLine2.IEGet_rc().All(rc=>pBOARD[rc].FreeBC==0) )  continue;			// Skip if all Sline cells are confirmed

					int FreeB_Tg2 = TgObj2.IEGet_UCell(pBOARD).Aggregate( 0, (a,uc) => a| uc.FreeB_Updated() );
					int rcCmp2    = _Get_rcCompanion( Exo, TgObj2, debugPrint:false );			// Companion Cell_1
					if( (rcCmp2>=0 & rcCmp2<100) &&  (_Digit_ToFreeB(rcCmp2) & FreeB_Tg2)>0 )   continue;		// [req4_1] C1 does not contain any Base Digit present in T1. 								
					// ----------------------------------------------------------------------------------------------------------------


						{// Configure and verify the components( Companion, CrossLine, Sline).
							//[req3-0] Base digits are in at least one Target.
							if( FreeB0.DifSet(FreeB_Tg1|FreeB_Tg2) > 0 ) continue;	

							int FreeB_Tg12 = (FreeB_Tg1 | FreeB_Tg2) & FreeB0;
							int FreeB_Tg12_Count = FreeB_Tg12.BitCount();

							//[req3] The Target Cells together contain at least the same 3 or 4 Candidates as the Base Cells.
							if( FreeB_Tg12_Count<3 || FreeB_Tg12_Count>4 )   continue;		// Targets digits is 3 or 4
									if(debugPrint)  WriteLine( $"         FreeB_Tg12:{FreeB_Tg12.TBS()}" );

							// Registering Exocet_components
							Exo.Initialize();


							Exo.ExG0 = new( Exo, sq:0, Object81:qMaxB81, house_Sx:house_SLine0, Comp81:qMaxB81,CrossLine_Sx:CrossLine0, SLine_Sx:SLine0 );
							Exo.ExG1 = new( Exo, sq:1, Object81:TgObj1, house_Sx:house_SLine1, Comp81:qOne<<rcCmp1, CrossLine_Sx:qOne<<rcCmp1, SLine_Sx:SLine1 );
							Exo.ExG2 = new( Exo, sq:2, Object81:TgObj2, house_Sx:house_SLine2, Comp81:qOne<<rcCmp2, CrossLine_Sx:qOne<<rcCmp2, SLine_Sx:SLine2 );
							// -------------------------------------------------------------------------------------
						}

					// Testing the SLine covering
					// [req4] All instances of each Base Digit as a candidate or a given or a solved value in the S Cells
					//		  must be confined to no more than two Cover Houses

					bool IsCovered_2CoverLines = Exocet_Requirement_IsCovered( Exo, debugPrint:false );  // ===== Requirement ===== 
					if( !IsCovered_2CoverLines )	   continue;
					// -------------------------------------------------------------------------------------

					{// Configure Mirror Cells 	
						// Mirrors are determined from the Base and Target position. The Digits state of the Mirror candidate will not be evaluated.
						UInt128 Mirror1F = _Get_MirrorF_GivenSolvedCandidate( Exo, 1, debugPrint:false );
						UInt128 Mirror2F = _Get_MirrorF_GivenSolvedCandidate( Exo, 2, debugPrint:false );
						UInt128 Mirror1 = Mirror1F & BOARD_FreeCell81;
						UInt128 Mirror2 = Mirror2F & BOARD_FreeCell81;

						Exo.ExG1.Set_Mirror( Mirror1F, Mirror1 );
						Exo.ExG2.Set_Mirror( Mirror2F, Mirror2 );
					}


					if(debugPrint){
						string st = $"\n\n\n@@@ Exocet_Eements  type:{stType(Exo.diagonalB)}";
						st += $"\n  ExG1 : {Exo.ExG1.ToString(1)}\n  ExG2 : {Exo.ExG2.ToString(2)}"; 
						WriteLine( st ); 
					}

					yield return Exo;
				}

			}
			yield break;
		}



		private IEnumerable< UInt128 > IEGet_TargetObject_JE2( UExocet Exo, int sq, bool debugPrint=false ){
			int blockNo = (sq==1)? Exo.Block1: Exo.Block2;
			int FreeB0 = Exo.FreeB0;

			// <<< Target >>>
			UInt128 TargetBlock81B = ( HouseCells81[blockNo+18] .DifSet(ConnectedCells81[Exo.rcStem]) ) & BOARD_FreeCell81; //Candidate cells for Target  
			foreach( int rcTgX in TargetBlock81B.IEGet_rc().Where( rc=> (pBOARD[rc].FreeB&FreeB0)>0) )   yield return (qOne<<rcTgX); 
				



		// <<< Object >>>
			UInt128 Candidate4Object81 = ( HouseCells81[blockNo +18] & BOARD_FreeCell81 ) .DifSet(Exo.BaseConnectedAnd81);
				if(debugPrint)  WriteLine( $"BaseConnectedCells81:{Exo.BaseConnectedAnd81}\n   Candidate4Object81:{Candidate4Object81.TBS()}" );

			int h0 = (Exo.dir==0)? (blockNo%3)*3+9: (blockNo/3)*3;
			for( int h=h0; h<h0+3; h++ ){
				UInt128 Object81 = (Candidate4Object81 & HouseCells81[h]) & BOARD_FreeCell81;

				if( Object81.BitCount() == 1 )  continue;	
				int nFreeB = Object81.IEGet_UCell(pBOARD).Aggregate(0, (a,uc) => a|uc.FreeB) .DifSet(FreeB0);
				if( nFreeB > 0 ){
					foreach( int no in nFreeB.IEGet_BtoNo() ){
						int h_locked = Object81.Ceate_rcbFrameAnd();
						if( h_locked == 0 ) continue; 
						//non BaseDigit #no is locked



							if(debugPrint)  WriteLine( $"h0:{h,2} Object81:{Object81.TBS()}  Object81.Count;{Object81.BitCount()}" );
						yield return  Object81;
					}
				}
			}


			yield break;
		}														
		




		private int _Get_rcCompanion( UExocet Exo, UInt128 objX, bool debugPrint=false ){
			if( objX.BitCount() != 1 )  return -1;

			int rcTgX = objX.Get_rcFirst();
			int hh = (Exo.dir==0)? (rcTgX%9)+9: rcTgX/9;
			UInt128 Cmp81 = HouseCells81[ rcTgX.ToBlock()+18 ] .DifSet( ConnectedCells81[Exo.rcStem] );
			Cmp81 = (Cmp81 & HouseCells81[hh]). Reset(rcTgX);
			int rcCmpX = IsFixed( Cmp81.FindFirst_rc() );	// if CompanionCell is Fixed, be a negative value
					if(debugPrint) WriteLine($"  dir:{Exo.dir} rcTg:{rcTgX.ToRCString_NPM()} -> rcCmpX:{rcCmpX.ToRCString_NPM()}");
			return  rcCmpX;
		}

		private (int,UInt128,UInt128)   _Get_SLine_withObject( int dir, UInt128 objX, int blockX ){
			int rcTgX = objX.Get_rcFirst();
			int house_Sx = (dir==0)? (rcTgX%9+9): (rcTgX/9); 
			UInt128 CrossLine = HouseCells81[house_Sx];
			UInt128 SLineX = HouseCells81[house_Sx]. DifSet( HouseCells81[blockX+18] );
			return (house_Sx,CrossLine,SLineX);
		}
	
		private UInt128 _Get_MirrorF_GivenSolvedCandidate( UExocet Exo, int sq, bool debugPrint=false ){
			// Calculate only from the positional relationship. No numerical conditions are used.
		
			UInt128 MirrorF = qZero;	// MirrorF: given, solved and candidate
			if( sq == 1 ){
				UInt128  BlockHouse81 = HouseCells81[Exo.Block2+18] .DifSet( ConnectedCells81[Exo.rcStem] | Exo.ExG2.Object81) ;
				MirrorF = Exo.ExG1.Object81.IEGet_rc().Aggregate( qZero, (a,rc) => a| BlockHouse81.DifSet(ConnectedCells81[rc]) ); 
			}
			else{	//sq:2
				UInt128  BlockHouse81 = HouseCells81[Exo.Block1+18] .DifSet( ConnectedCells81[Exo.rcStem] | Exo.ExG1.Object81) ;
				MirrorF = Exo.ExG2.Object81.IEGet_rc().Aggregate( qZero, (a,rc) => a| BlockHouse81.DifSet(ConnectedCells81[rc]) ); 
			}
				if(debugPrint)  WriteLine($"  sq:{sq} MirrorF:{MirrorF.TBS()}");
			return MirrorF;
		}





		private bool Exocet_Requirement_IsCovered( UExocet Exo, bool debugPrint=false ){
		//	[req4] Check if SLine is covered.
			UExocet_elem ExG1=Exo.ExG1, ExG2=Exo.ExG2;
			int			 FreeB0 = Exo.FreeB0;

			Exo.CoverStatusList = new UCoverStatus[9];
			Exo.CoverLine_by_Size = new int[4];

			foreach( var no in FreeB0.IEGet_BtoNo() ){
				int noB = 1<<no;
				string st = $"### CoverLine #{no+1}";
				var  S012V = _Get_SAreaDigits( Exo, no, debugPrint:debugPrint );

				//If the Cross-Line has three or more fixed digits, it is case "no Cover-Line".
				UInt128 Fixed_on_CrossLine = Exo.CrossLine81_012 & Fixed81B9[no];	// fixed in S-Area
						if(debugPrint){
							WriteLine( $"   Exo.CrossLine_Sx:{Exo.CrossLine81_012.TBS()}" );
							WriteLine( $"      Fixed81B9[#{no+1}]:{Fixed81B9[no].TBS()}" );
							WriteLine( $" Fixed_on_CrossLine:{Fixed_on_CrossLine.TBS()}" );
						}


			    // CoverLine  *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
				if( S012V!=int.MinValue && Fixed_on_CrossLine.BitCount()<=2 ){

					// <<< _Get_CoverLine
					UCoverStatus UCL = _Get_CoverLine( Exo, S012V, no, debugPrint:true );
					int sz = 0;
					if( UCL != null ){
						Exo.CoverStatusList[no] = UCL;
						sz = Max( UCL.size, 0 );
					}
					Exo.CoverLine_by_Size[sz] |= noB;
				}
				else{
					if( Fixed_on_CrossLine.BitCount() >= 2 ) Exo.CoverLine_by_Size[3] |= noB;
					else   Exo.CoverLine_by_Size[0] |= noB;
				}
				// *--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*
			}

			var (valid_noBObj1,valid_noBObj2) = _Get_noBValid_Sline( Exo );		// if "JE1", valid_noBObj2 is not zero.
			if( valid_noBObj1==0 || valid_noBObj2==0 )  return false;	// Target has no BaseDigit


			// Check the SLine-Covering condition	*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
			if( Exo.ExocetName == "JE2" ){
				int ValidDigits_CL2 = Exo.CoverLine_by_Size[2];					// Candidate Base-Digits with two instances in Sline. 
				var (BaseUA,BaseUB) = Exo.Get_Base_UCells();					// Base-Cells 

				if( (BaseUA.FreeB & ValidDigits_CL2) == 0 )       return false;	// There are invalid candidate digits in the Base.
				if( (BaseUB.FreeB & ValidDigits_CL2) == 0 )       return false;

				if( (ExG1.FreeB_Object81&ValidDigits_CL2) == 0 )  return false;	// There are invalid candidate digits in the Object(Taget).
				if( (ExG2.FreeB_Object81&ValidDigits_CL2) == 0 )  return false;
			}
			// *--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*

			int FreeB_test2 = FreeB0 .DifSet(Exo.CoverLine_by_Size[0] | Exo.CoverLine_by_Size[1]);	//@[Att]  Bird@Doc. Rule-1
			return (FreeB_test2.BitCount()>=2);
		}

				// =======================================================================================================================
		private int _Get_SAreaDigits( UExocet Exo, int no, bool debugPrint=false ){
			int     noB = 1<<no;
			UInt128 nonBand81andBaseBlock = qMaxB81 ^ Exo.Band81andBaseBlock;	// or ~Exo.Band81andBaseBlock & qMaxB81;

			UInt128 FreeAndFixed81b = FreeAndFixed81B9[no] & nonBand81andBaseBlock;	// Distribution of confirmed(given or solved) or candidates on the BOARD.
					if( debugPrint ) WriteLine( $"@@@  no:#{no+1}\n   FrFix81b:{FreeAndFixed81b.ToBitString81N()}" );

			UExocet_elem  ExG0=Exo.ExG0, ExG1=Exo.ExG1, ExG2=Exo.ExG2;
			// Distribution of #n in SLine(81bit representation)
			UInt128 S0no = FreeAndFixed81b & ExG0.SLine_Sx;
			UInt128 S1no = FreeAndFixed81b & ExG1.SLine_Sx;
			UInt128 S2no = FreeAndFixed81b & ExG2.SLine_Sx;

			// vector_Sline. Transformation using its characteristics(UInt128 -> int).
			int  dirR=1-Exo.dir;
			int	dirB81ToHouse_Func( int dir, UInt128 Obj ) => ( dir, Obj.Get_rcFirst()).DirRCtoHouse();
			var (S0Vec,S0HouseNo) = S0no.ToVectorH( (dirR,Exo.rcStem).DirRCtoHouse() );			// House No.
			var (S1Vec,S1HouseNo) = S1no.ToVectorH( dirB81ToHouse_Func(dirR,ExG1.Object81) );
			var (S2Vec,S2HouseNo) = S2no.ToVectorH( dirB81ToHouse_Func(dirR,ExG2.Object81) );
			//Exo.SLineHouseNo = new int[]{ S1HouseNo, S2HouseNo, S0HouseNo};
					if( debugPrint ){ 
						WriteLine( $"  no:#{no+1}\n{sp5}S0Vec:{S0Vec.TBS()}  H0:{S0HouseNo}" );
						WriteLine( $"{sp5}S1Vec:{S1Vec.TBS()}  H0:{S1HouseNo}\n{sp5}S2Vec:{S2Vec.TBS()}  H2:{S2HouseNo}" );
					}

			// <<< Connectivity >>>
			int SLineHitCount = Max((S1Vec&S2Vec).BitCount(),1) + Max((S2Vec&S0Vec).BitCount(),1);
			if( SLineHitCount==0 ){	    return int.MinValue; }	// -> If S's are not covered by two CoverLines, it may fail.
			if( S1Vec==0 && S2Vec==0 ){	return int.MinValue; }	// Base diigits that are only in S0 are not suitable.
		
			// <<< Is Covered? >>
			int S012V = (S2Vec<<18) | (S1Vec<<9) | S0Vec;
						if( debugPrint ) WriteLine( $"{spX}    S012V      :{S012V.ToBitStringMod9(27)}" );
			return  S012V;
		}

				
		private UCoverStatus _Get_CoverLine( UExocet Exo, int S012V, int no,  bool debugPrint=false ) {
			if( S012V.BitCount() == 0 )  return null;
					
			// ===========================================================
			// ... Singly Paralell-CoverLink ...
			var UCL1 = __Get_CoverLink_1( no, S012V );
			if( UCL1 !=null ) return UCL1;

  			// ... Two CoverLink ...
			//   2-Parallel CoverLink
			var UCL2 = __Get_CoverLink_2PP( no, S012V );
			if( UCL2 !=null )  return UCL2;

			// ... 1-Cross, 1-Parallel CoverLink ...
			var UCLx = __Get_CoverLink_2CP( no, S012V );
			if( UCLx !=null )  return UCLx;

			// ... Wildcard ...
			for( int WCidx=9; WCidx<=10; WCidx++ ){
				var CLHdif = CoverLineH[WCidx];		// {S1,S2}
				if( (S012V&CLHdif) > 0 ){
					var S012Vdif = S012V .DifSet(CLHdif);
					var UCLw = __Get_CoverLink_2PP( no, S012Vdif );		// (S012-S1) is covered by PP, (S012-S2) is covered by PP
					if( UCLw !=null ){ 
						UCLw.Set_wildcard_para( no, WCidx, CLHdif );
						return UCLw;
					}
				}
			}
			// -------------------------------------------------------------
			return null;

					// -----------------------------
					UCoverStatus __Get_CoverLink_1( int no, int pS012V ){
						for( int cl0=0; cl0<=8; cl0++ ){
							int CLH0 = CoverLineH[cl0];
							if( pS012V.DifSet(CLH0) == 0 ) return  new UCoverStatus( Exo, no, pS012V, cl0, CLH0 );
						}
						return null;
					}

					UCoverStatus __Get_CoverLink_2PP( int no,int pS012V ){
						for( int cl0=0; cl0<=8; cl0++ ){
							int CLH0 = CoverLineH[cl0];
							if( (pS012V&CLH0) == 0 )  continue;
							for( int cl1=cl0+1; cl1<9; cl1++ ) {
								int CLH1 = CoverLineH[cl1];
								if( pS012V.DifSet(CLH0|CLH1) == 0 ) return  new UCoverStatus( Exo, no, pS012V, cl0, cl1, CLH0, CLH1 );
							}
						}
						return null;
					}
					
					UCoverStatus __Get_CoverLink_2CP( int no,int pS012V ){
						// Specifications: Cross cover line is in CL0.
						for( int cl0=9; cl0<=10; cl0++ ){
							int CLH0 = CoverLineH[cl0];
							if( (pS012V&CLH0) == 0 )  continue;
							if( pS012V.DifSet(CLH0) == 0 )  continue;

							for( int cl1=0; cl1<9; cl1++ ) {
								int CLH1 = CoverLineH[cl1];
								if( pS012V.DifSet(CLH0|CLH1) == 0 ) return  new UCoverStatus( Exo, no, pS012V, cl0, cl1, CLH0, CLH1 );
							}
						}
						return null;
					}
		}

		private (int,int) _Get_noBValid_Sline( UExocet Exo ){
			int noB_valid_1 = __Get_noBValid_Sline_sub( Exo, Exo.ExG1 );
			int noB_valid_2 = 0x1FF;
			if( Exo.ExocetName=="JE2" ) noB_valid_2 = __Get_noBValid_Sline_sub( Exo, Exo.ExG2 );
			return  (noB_valid_1,noB_valid_2);
					
					// ----------------------------------------------------------------------------
					int __Get_noBValid_Sline_sub( UExocet Exo, UExocet_elem ExGX ){ 
						int noB_invalid=0;
						foreach( int no in (ExGX.FreeB_Object81&Exo.FreeB0).IEGet_BtoNo() ){
							UInt128 F81no = FreeCell81b9[no];
							//UInt128 CL81 = (F81no & ExGX.SLine_Sx);	// .DifSet( Exo.SLine_012.ConnectedCells() );		
							UInt128 DifSetConnected = F81no & (Exo.ExG0.SLine_Sx&F81no).ConnectedCells() & ExGX.CrossLine_Sx;

							UInt128 CL81 = (F81no & ExGX.SLine_Sx) .DifSet( DifSetConnected );		
							if( CL81.BitCount() >= 2 )	noB_invalid |= (1<<no);	// Only valid if there is one candidate
						}
						int noB_Valid = (ExGX.FreeB_Object81 & Exo.FreeB0) .DifSet(noB_invalid);		// & Base-Digits

						return noB_Valid;
					}
		}


	  #endregion Conditions for the Exocet to be established

	}


}
