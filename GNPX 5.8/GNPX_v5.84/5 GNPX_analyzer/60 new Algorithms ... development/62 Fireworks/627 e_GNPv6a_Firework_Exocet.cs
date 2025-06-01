#if false

using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using static System.Math;
using System.IO;

using GIDOO_space;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using System.Collections;


namespace GNPX_space{
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//    under development (GNPXv5.7)
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*



	//【解法】Exocet
	// https://www20.big.or.jp/~morm-e/puzzle/techniques/number_place/exocet/index.html  ##### 削除

	// The New Sudoku Players' Forum, 『Exotic patterns a resume』,
	// http://forum.enjoysudoku.com/exotic-patterns-a-resume-t30508.html
	// The New Sudoku Players' Forum, 『JExocet Pattern Definition』,
	// http://forum.enjoysudoku.com/jexocet-pattern-definition-t31133.html

	// ..5....38.3....4..42.3.........6..7929..5...1..38..2......1...6..45..9..9....7.5.
	//715429638638175492429386715841263579296754381573891264357912846164538927982647153 ... elapsed time:6ms

    public partial class Firework_TechGen: AnalyzerBaseV2{

        public bool Firework_Exocet_1( ){
			// ===== Prepare =====
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_Firework_TechGen();
			}



			// ===== Analize =====
			// @@@@ Base
						int LX=0;
			foreach( var (dir,rc0,baseCells,FreeB) in IEGet_BaseCells() ){
				int block0 = rc0.ToBlock(); 
						WriteLine( $"\n dir:{dir}  <<rc0:{rc0}>>   block0:{block0}  baseCells:{baseCells.ToBitString81N()}" );

				// @@@@@ Target Block
						if( (++LX)%3 == 1 )  WriteLine("");
				foreach( var (b1,b2) in IEGet_TargetBlocks(dir,rc0) ){
						WriteLine( $"   dir:{dir}  <<rc0:{rc0:00}>>   block0:{block0}   (b1,b1):({b1},{b2})" );

					int house_rc0 = (dir==0)? (rc0%9+9): (rc0/9); 
					UInt128 SLine = HouseCells81[house_rc0]. DifSet( HouseCells81[block0+18] );
						//WriteLine( $"   dir:{dir}  SLine:{SLine.ToBitString81N()}" );


					foreach( var (rc_t1,rc_t2) in IEGet_TargetCells(dir,rc0,b1,b2,FreeB) ){
						WriteLine( $"    (rc_t1,rc_t2) : {rc_t1.ToRCString()}, {rc_t2.ToRCString()}" ); 
				
						{ // Firework[ rc_t1 - SLine ]
							UInt128 SLine2 = SLine | (qOne<<rc_t1) | (qOne<<rc_t2);
							WriteLine( $"   dir:{dir}  SLine :{SLine.ToBitString81N()}" );
							WriteLine( $"   dir:{dir}  SLine2:{SLine2.ToBitString81N()}" );
							var fwList = Firework_List.FindAll( p=> (p.rc12B81&SLine2)>0 );
							fwList.ForEach( P=> WriteLine(P) );





						}

		/*
						bool solfound = Exocet_SolResult(  );
						if( !solfound ) continue;
						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
						break;
		*/
					}
				}

/*
				foreach( int chuteBlock in block0.IEGet_Chute_BlockToBlock() ){
					WriteLine( $" rc0:{rc0} rc1s:{baseCells.ToBitString81N()} chute:{chuteBlock.ToBitString(9)}" );

				}
		*/
			}





			return false;



			// ==============================================================================
			IEnumerable<(int,int,UInt128,int)> IEGet_BaseCells( ){
				for( int dir=0; dir<2; dir++ ){
					for( int rc0=0; rc0<81; rc0++ ){
						int block0 = rc0.ToBlock(); 

						UInt128 baseCells = (dir==0)? HouseCells81[rc0/9]: HouseCells81[rc0%9];
						baseCells = (HouseCells81[rc0/9] & HouseCells81[block0+18]) .Reset(rc0);
						baseCells &= BOARD_FreeCell81;
						var rcList = baseCells.BitToNumList(81);
						if( rcList==null || rcList.Count<=1 )  continue;
						UCell UC0=pBOARD[rcList[0]], UC1=pBOARD[rcList[1]];
						int FreeB = UC0.FreeB | UC1.FreeB;
						int FreeBC = FreeB. BitCount();
						if( FreeBC<3 || FreeBC>4 )  continue;
						yield return (dir, rc0, baseCells, FreeB); 
					}
				}
				yield break;
			}

			IEnumerable<(int,int)> IEGet_TargetBlocks( int dir, int rc0 ){
				int block0=rc0.ToBlock(),  shift=block0/3*3;
				if( dir==0 ){ yield return ( (block0+1)%3+shift, (block0+2)%3+shift); }
				else{         yield return ( (block0+3)%9, (block0+6)%9); }
				yield break;
			}

			IEnumerable<(int,int)> IEGet_TargetCells( int dir, int rc0, int b1, int b2,int FreeB ){
				UInt128 targetBlockt1_81b = HouseCells81[b1+18] .DifSet(ConnectedCells81[rc0]);
				UInt128 targetBlockt2_81b = HouseCells81[b2+18] .DifSet(ConnectedCells81[rc0]);

				foreach( int rc_targetCell1 in targetBlockt1_81b.IEGet_rc().Where(rc=> (pBOARD[rc].FreeB&FreeB)>0)  ){
					int rc_Companion1 = Get_rc_Companion(dir, rc0, rc_targetCell1 );
						//string st = $"dir:{dir}  rc_target->Companion : {rc_targetCell1.ToRCString()}";
						//st += $" -> {Abs(rc_Companion1).ToRCString()}";
						//if( rc_Companion1<0 )  st += " ---";
						//WriteLine(st);
					if( rc_Companion1<0 )  continue;


					foreach( int rc_targetCell2 in targetBlockt2_81b.IEGet_rc().Where(rc=> (pBOARD[rc].FreeB&FreeB)>0) ){
						int rc_Companion2 = Get_rc_Companion(dir, rc0, rc_targetCell2 );
							//string st2 = $"dir:{dir}  rc_target->Companion : {rc_targetCell2.ToRCString()}";
							//st2 += $" -> {Abs(rc_Companion2).ToRCString()}";
							//if( rc_Companion2<0 )  st2 += " ---";
							//WriteLine(st2);
						if( rc_Companion2<0 )  continue;

						yield return (rc_targetCell1, rc_targetCell2);
					}
				}
				yield break;



				int Get_rc_Companion( int dir, int rc0, int rc_target ){
					UInt128 com = HouseCells81[ rc_target.ToBlock()+18 ] .DifSet( ConnectedCells81[rc0]);
					int hh = (dir==0)? (rc_target%9)+9: rc_target/9;
					com = (com & HouseCells81[hh]).DifSet(UInt128.One<<rc_target);
					int rc_Companion = com.FindFirst_rc();

					UCell UC = pBOARD[rc_Companion];
					int testFreeB = (UC.No>0)? (1<<UC.No-1): FreeB;

					return  ((testFreeB & FreeB) == 0)? rc_Companion: -rc_Companion;
				}
			}
		}







		private bool Exocet_SolResult(  ){


			return false;
		}
	}
}
#endif