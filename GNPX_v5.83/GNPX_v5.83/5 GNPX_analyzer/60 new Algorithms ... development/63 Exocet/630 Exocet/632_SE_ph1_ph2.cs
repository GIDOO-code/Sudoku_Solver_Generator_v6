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
using static GNPX_space.SubsetTechGen;
using System.Buffers.Text;


namespace GNPX_space{
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//   under development (GNPXv6)
	//   @@@@@ There are still discrepancies between the Sudoku-6 HP and GNPX-6 codes. @@@@@
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	using G6_SF = G6_staticFunctions;
	using TapleUCL = (UCoverLine,UCoverLine,UCoverLine);

    public partial class Senior_Exocet_TechGen : AnalyzerBaseV2{
		static private UInt128[] HC81 =>	HouseCells81;
		static private UInt128[] ConnC81 => ConnectedCells81;

		private IEnumerable<USExocet> IE_SE_ph1_Set_BasicForm1_Set_BasicForm( string ExoControl, bool debugPrint=false ){
			// dir, StemCell => basecells, FreeB, SLine0, block1, block2

			for( int dir=0; dir<2; dir++ ){	// direction  0:row 1:column
				for( int rcStem=0; rcStem<81; rcStem++ ){
					//if( dir!=1 || rcStem!=69 )  continue;			// ===== SE_Nxt Debug =====

					// ... Base cells
					int     hBase = (dir,rcStem).DirRCtoHouse( );
					UInt128 base81 = (HouseCells81[hBase] & HouseCells81[rcStem.B()+18] & BOARD_FreeCell81) .Reset(rcStem);
					
						
					// ... Single SE : Base is a single-cell and bivalue. 
					if( ExoControl.Contains("SingleBase") ){	
						foreach( var UC in base81.IEGet_UCell(pBOARD).Where(uc=>uc.FreeBC==2) ){
							USExocet SExo = _Create_SExocet( ExoControl, dir, rcStem, qOne<<UC.rc, UC.FreeB );
							if( SExo != null )  yield return SExo;
						}
					}

					else{ // ... Normal ........................................................
						if( base81.BitCount() != 2 )  continue;			// ... Number of bits in Base

						// ... Base candidates
						var (rcA,rcB) = base81.BitToTupple();			// (There are definitely two cells.)
						int  FreeB_UCA=pBOARD[rcA].FreeB, FreeB_UCB=pBOARD[rcB].FreeB;
						if( (FreeB_UCA & FreeB_UCB) == 0 )  continue;		// Base-cells are unfixed, and has common digits.

						// [req1] The Base Cells together contain 3 or 4 Candidates, that we call Base Candidates.
						int FreeB = FreeB_UCA | FreeB_UCB;		
						int FreeBC = FreeB.BitCount();
					
						if( !ExoControl.Contains("Extend") && (FreeBC<3 || FreeBC>4) )  continue;	
							// 2:LockedSet 5-:Hopeful predictions. ... Exocet_extend loosens restrictions.

						USExocet SExo = _Create_SExocet( ExoControl, dir, rcStem, base81, FreeB );
						if( SExo != null )  yield return SExo;
					}
				}
			}
			yield break;

						USExocet _Create_SExocet( string ExoControl, int dir, int rcStem, UInt128 Base81A,int FreeB ){
							UInt128 CrossLine0  =  HouseCells81[(1-dir,rcStem).DRCHf()] .DifSet( HouseCells81[rcStem.B()+18] );
							if( (CrossLine0 & BOARD_FreeCell81) == qZero )  return null;

							USExocet SExo = new( ExoControl, dir, rcStem, Base81A, FreeB, BOARD_FreeCell81 );
									if(debugPrint) WriteLine( $" CrossLine0:{CrossLine0.TBS()}" );

							return SExo;
						}
		}




		private IEnumerable<USExocet> IE_SE_ph2_Get_TargetObject_SLine( USExocet SExo, bool debugPrint=false ){

			foreach( var h012 in _IE_SE_ph2sub_Get_CrossLine_HouseNo( SExo, debugPrint:false) ){
				//if( h012 != (03,07,02) )  continue; // ===== SE_Nxt Debug =====

				foreach( var (ExG0,ExG1,ExG2) in  IE_SE_ph3_Get_TargetObject_SLine( SExo, h012,debugPrint:false ) ){
					SExo.ExG0=ExG0; SExo.ExG1=ExG1; SExo.ExG2=ExG2;
					yield return SExo; 
				}
			}
			yield break;	

						IEnumerable< (int,int,int) >  _IE_SE_ph2sub_Get_CrossLine_HouseNo( USExocet SExo, bool debugPrint=false ){
							int dir=SExo.dir, rcStem=SExo.rcStem;
							int  h0 = 0;
							int[]  h1HxList, h2HxList;

							if( dir==0 ){
								h0 = rcStem %9 + 9;
								int SC = ((rcStem+3)%9)/3*3 + 9;		// +9 : column
								h1HxList = new[]{SC+0, SC+1, SC+2 };
				
								SC = ((rcStem+6)%9)/3*3 + 9;
								h2HxList = new[]{ SC+0, SC+1, SC+2 };
							}
							else{
								h0 = rcStem/9;
								int SR = (rcStem/27*3+3)%9;
								h1HxList = new[]{ SR+0, SR+1, SR+2 };
								SR = (rcStem/27*3+6)%9;
								h2HxList = new[]{ SR+0, SR+1, SR+2 };
							}
						
							foreach( int kx in Enumerable.Range(0,9) ){
								var h012 = (h0, h1HxList[kx/3], h2HxList[kx%3] );
								SExo.Set_CrossLine_SLineBase( h012 );
										if(debugPrint) WriteLine( $" h012 : {h012}" );

								yield return h012;
							}

							yield break;
						}
		}




		public IEnumerable<TapleUCL> IE_SE_ph3_Get_TargetObject_SLine( USExocet SExo, (int,int,int) h012, bool debugPrint=false ){
				// The target generation order is (h0-h1), (h0-h2), (h1-h2).
				// Eliminate duplicate target generation.

			switch(SExo.ExocetNamePlus){
				case "JE2":
					foreach( var (ExG0,ExG1,ExG2) in IE_SE_ph3sub_Get_TargetObject_SLine_JE2( SExo, h012, debugPrint:false) ){
						yield return (ExG0,ExG1,ExG2);
					}
					break;

				case "JE1":
					foreach( var (ExG0,ExG1,ExG2) in IE_SE_ph3sub_Get_TargetObject_SLine_JE1( SExo, h012, debugPrint:false)  ){
						// [ATT] ExG1 is the Base digits absent.
						yield return (ExG0,ExG1,ExG2);
					}
					break;

				case "SE_Basic":
					foreach( var (ExG0,ExG1,ExG2) in IE_SE_ph3sub_Get_TargetObject_SLine_SE_Basic( SExo, h012, debugPrint:false) ){
						yield return (ExG0,ExG1,ExG2);
					}
					break;

			// ====================================================================================================================

				case "SE_Standard":
					foreach( var (ExG0,ExG1,ExG2) in IE_SE_ph3sub_Get_TargetObject_SLine_SE_Standard( SExo, h012, debugPrint:false) ){
							//WriteLine( $"(ExG0,ExG1,ExG2):{ExG0.Object.TBScmp()}, {ExG0.Object.TBScmp()}, {ExG0.Object.TBScmp()}" );							
							//G6_SF.__MatrixPrint( Flag:SExo.Base81,  ExG0.Object, ExG1.Object, ExG2.Object, " ExG0.Object, ExG1.Object, ExG2.Object" );
						yield return (ExG0,ExG1,ExG2);
					}
					break;


				case "SE_Single":
				case "SE_SingleBase":
					foreach( var (ExG0,ExG1,ExG2) in IE_SE_ph3sub_Get_TargetObject_SLine_Single( SExo, h012, debugPrint:false)  ){
						// [ATT] ExG1 is the Base digits absent.
						yield return (ExG0,ExG1,ExG2);
					}
					break;


				default:
					WriteLine( $"\n\n ExocetNamePlus : {SExo.ExocetNamePlus} ... Next development. Next development. Next development. Next development.\n\n\n" );
					throw new Exception( $"Operation Error. SExo.ExocetNamePlus : {SExo.ExocetNamePlus}");

			}
			yield break;
		}

	}

}