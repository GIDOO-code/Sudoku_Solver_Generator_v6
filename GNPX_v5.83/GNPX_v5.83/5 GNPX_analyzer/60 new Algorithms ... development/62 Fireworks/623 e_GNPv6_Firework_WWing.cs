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
using System.Diagnostics;
using System.Xml.Linq;


namespace GNPX_space{

// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
//    under development (GNPXv5.1)
// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

	//The New Sudoku Players' Forum, ÅwFireworksÅx,
	// http://forum.enjoysudoku.com/fireworks-t39513.html
	// http://forum.enjoysudoku.com/fireworks-t39513-45.html

	//.2...783..47.2...13..1....7....38.15...5.4...58.79....6....2..82...8.57..793...6.
	//125647839947823651368159427496238715731564982582791346653472198214986573879315264 ... elapsed time:0ms

/*
+-------+-------+-------+
| . 8 5 | 7 . . | 6 . . |
| 3 . . | . 4 . | . 1 . |
| 2 . . | . . . | . . 8 |
+-------+-------+-------+
| 5 . 4 | 8 . . | . . . |
| 6 . . | . 2 . | . 5 . |
| . 9 . | . . 1 | . . 3 |
+-------+-------+-------+
| . . . | . . 9 | . . 4 |
| . . . | 1 . . | . . 7 |
| . . . | . 3 7 | 2 8 . |
+-------+-------+-------+
Pear and Rocket - shye
skfr 7.1
.857..6..3...4..1.2.......85.48.....6...2..5..9...1..3.....9..4...1....7....3728.
985713642376248915241965378534896721617324859892571463728659134463182597159437286 

.---------------------.------------------.-------------------.
|x149  8       5      | 7    #19    23   | 6      2349  2-9  |
| 3    67      679    | 2569  4     8    | 579    1     259  |
| 2    1467    1679   | 3569  1569  356  | 34579  3479  8    |
:---------------------+------------------+-------------------:
| 5    1237    4      | 8     679   36   | 179    2679  1269 |
| 6    137     1378   | 349   2     34   | 14789  5    #19   |
| 78   9       278    | 456   567   1    | 478    2467  3    |
:---------------------+------------------+-------------------:
| 178  123567  123678 | 256   568   9    | 135    36    4    |
| 489  23456   23689  | 1     568   2456 | 359    369   7    |
|x149  1456    169    | 456   3     7    | 2      8    x1569 |
'---------------------'------------------'-------------------'
*/

	//GNPX sample 47 step9
	//...8...4....21...7...7.5981315..9..8.8....4....41.83.5..1.82.646.8...1...236.18..
	//1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9
	//157893642849216537236745981315469278982537416764128395591382764678954123423671859 ... elapsed time:0ms



    public partial class Firework_TechGen: AnalyzerBaseV2{

		public bool Firework_WWing( ){
			// ===== Prepare =====
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_Firework_TechGen();
			}
			debugPrint = false;	
			 
			// ===== Analize =====
			if( FireworkAgg_List==null || FireworkAgg_List.Count<=0 )  return false; 
			var FWAgg2_List = FireworkAgg_List.FindAll(q=> q.FreeBC==2);	// Dual Firework
					//FWAgg2_List.ForEach( P=> WriteLine(P) );

			UInt128 UC_Bival_81B = pBOARD.Create_bitExp_bivalue();
			foreach( var fwStem in FWAgg2_List ){
					//WriteLine( $"\n*** {fwStem.ToString()}" );

				int rcStem=fwStem.rcStem, FreeB=fwStem.FreeB;

				UInt128 Area_not_searched = ConnectedCells81[rcStem] | (qOne<<rcStem);
				UInt128 searchHouse1 = (ConnectedCells81[fwStem.rc1]. DifSet(Area_not_searched));
				UInt128 searchHouse2 = (ConnectedCells81[fwStem.rc2]. DifSet(Area_not_searched));

				// intersection cell
				UInt128 rcIntersec81 = searchHouse1 & searchHouse2;
				if( rcIntersec81.IsZero() )  continue;

				foreach( int rcIntersec in rcIntersec81.IEGet_rc(81) ){
					UInt128 searchHouse1_Connected = searchHouse1 & ConnectedCells81[rcIntersec];
					UInt128 searchHouse2_Connected = searchHouse2 & ConnectedCells81[rcIntersec];
					if( searchHouse1_Connected.IsZero() || searchHouse2_Connected.IsZero() )  continue; 

					int FreeBIntersec = pBOARD[rcIntersec].FreeB & fwStem.FreeB;
					if( FreeBIntersec == 0 )  continue;
						//WriteLine( $"fwStem:{fwStem}  rcIntersec:{rcIntersec.ToRCString()} FreeBIntersec:{FreeBIntersec.ToBitStringN(9)}" );
						//WriteLine( $"searchHouse1_Connected:{searchHouse1_Connected.ToBitString81N()}" );
						//WriteLine( $"searchHouse2_Connected:{searchHouse2_Connected.ToBitString81N()}" );

					// assist bivalue cell
					UInt128 FilterFeeB = UC_Bival_81B & FreeCell81b9_FreeBAnd(fwStem.FreeB);
					searchHouse1_Connected &= FilterFeeB;
					searchHouse2_Connected &= FilterFeeB;
						//WriteLine( $"fwStem.rc1:{fwStem.rc1.ToRCString()}  searchHouse1_Connected:{searchHouse1_Connected.ToBitString81N()}" );
						//WriteLine( $"fwStem.rc2:{fwStem.rc2.ToRCString()}  searchHouse2_Connected:{searchHouse2_Connected.ToBitString81N()}" );

					// (Isn't it true for multiple cells?)
					foreach( int rc1Assist in searchHouse1_Connected.IEGet_rc().Where(p=>p!=rcIntersec) ){
						foreach( int rc2Assist in searchHouse2_Connected.IEGet_rc().Where(p=>p!=rcIntersec) ){
						
							bool solfound = Firework_Dual_WWing_SolResult( fwStem, rc1Assist, rc2Assist, rcIntersec, FreeBIntersec );
							if( !solfound )  continue;

							if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
						}
					}
				}
			}

			return false;
		}

		private bool Firework_Dual_WWing_SolResult( UFirework fw, int rc1Assist, int rc2Assist, int rcIntersec, int FreeBIntersec ){	
			int     rcStem=fw.rcStem;

			UCell UCStem=pBOARD[rcStem],  UC1=pBOARD[fw.rc1], UC2=pBOARD[fw.rc2];
			UCell UCA1=pBOARD[rc1Assist], UCA2=pBOARD[rc2Assist], UCexclude=pBOARD[rcIntersec];

			UCexclude.CancelB = UCexclude.FreeB & FreeBIntersec;
			
			if( pBOARD.All(p=>p.CancelB==0) ){ pBOARD.ForEach(p=> p.ECrLst=null ); }
			else{
				UCStem.Set_CellBKGColor(SolBkCr);
				UC1.Set_CellBKGColor(SolBkCr);
				UC2.Set_CellBKGColor(SolBkCr);
				UCA1.Set_CellBKGColor(SolBkCr2);
				UCA2.Set_CellBKGColor(SolBkCr2);
				UCexclude.Set_CellBKGColor(SolBkCr3);

				SolCode = 2;
				string st_Assist = $"{UCA1.rc.ToRCString()}#{UCA1.FreeB.ToBitStringNZ(9)}  {UCA2.rc.ToRCString()}#{UCA2.FreeB.ToBitStringNZ(9)}";
				string st_Exclude = $"{UCexclude.rc.ToRCString()}#{(FreeBIntersec).ToBitStringNZ(9)}";

				Result     = $"Firework_WWing FW:{fw.ToStringResult()} Assist:{st_Assist} Exclude:{st_Exclude}";
				ResultLong = $"Firework_WWing\n  Firework : {fw.ToStringResult()}\n  Assist : {st_Assist}\n  Exclude : {st_Exclude}";
			}
			return (SolCode==2);
		}
	}
}