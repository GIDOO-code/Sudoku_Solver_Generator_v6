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

#if false

// Development pending

//Exocet
// https://www.taupierbw.be/SudokuCoach/SC_Exocet.shtml


namespace GNPX_space{
// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
//    under development (GNPXv5.7)
// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

/*

	//The New Sudoku Players' Forum, ÅwFireworksÅx,
	// http://forum.enjoysudoku.com/fireworks-t39513.html
	// http://forum.enjoysudoku.com/fireworks-t39513.html#p312079

+-------+-------+-------+
| . 1 2 | . 7 . | 3 5 . |
| 3 . . | 2 . 1 | 4 . 7 |
| 4 . . | 5 . . | . 1 6 |
+-------+-------+-------+
| 2 . . | . . . | . 7 . |
| 1 6 . | . 8 . | . . 2 |
| . 4 8 | . . . | 6 3 . |
+-------+-------+-------+
| . . . | 8 . . | . . 4 |
| . . . | 1 5 . | . . 3 |
| . . . | . 4 3 | 1 2 . |
+-------+-------+-------+
Hanabi - shye
skfr 8.0
.12.7.35.3..2.14.74..5...162......7.16..8...2.48...63....8....4...15...3....4312.
.12.7.35.3..2.14.74..53.2162......7.16..8..42.48...63..318....4.2415...3....4312. <- step4

.---------------------.-----------------.----------------.
| x689     1     2    | 4-69  7    4689 | 3    5   B89   |
|  3       589   569  | 2     69   1    | 4    89   7    |
|  4       789   79   | 5     3    89   | 2    1    6    |
:---------------------+-----------------+----------------:
|  2       59    359  | 3469  169  4569 | 589  7    1589 |
|  1       6     3579 | 379   8    579  | 59   4    2    |
| x579     4     8    |B79    129  2579 | 6    3    1-59 |
:---------------------+-----------------+----------------:
|  5679    3     1    | 8     269  2679 | 579  69   4    |
|  6789    2     4    | 1     5    679  | 789  689  3    |
|xT789-56  5789  5679 |x679   4    3    | 1    2   x589  |
'---------------------'-----------------'----------------'

firework exocet
original post

fireworks on 56789 in r9c1b7
base set: r1c9 & r6c4 - target set: r9c1
non-base candidates removed from target cell
fireworks on 5 and 6 become two-string kites, giving elims in r1c4 & r6c9
compatibility testing, 9 must be limited to base cells, giving elims in r1c4 & r6c9 (resulting in naked singles)
solves with a turbot fish

this pattern is very complex and rare
here i wrote a less jargon-y way to view the deduction (as well as provide a easier example of it)

the last puzzle is probably the most interesting application

*/

//##### i still don't understand.

    public partial class Firework_TechGen: AnalyzerBaseV2{
        public bool Firework_exocet( ){
			// ===== Prepare =====
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_Firework_TechGen();
			}
			debugPrint = true;	
			 
			// ===== Analize =====
			List<UFirework> Dual_Firework_List = FireworkAgg_List.FindAll( p=> p.FreeBC==2 );
			if( Dual_Firework_List.Count<=1 )  return false;
					if(debugPrint) Dual_Firework_List.ToList().ForEach( P=> WriteLine(P) );

			Combination cmb = new( Dual_Firework_List.Count, 2 );
			while( cmb.Successor() ){
				UFirework DF0 = Dual_Firework_List[ cmb.Index[0] ];
				UFirework DF1 = Dual_Firework_List[ cmb.Index[1] ];
				if( DF0.FreeB==DF1.FreeB || DF0.rcStem!=DF1.rcStem )  continue;
				if( (DF0.rc12B81 & DF1.rc12B81).IsNotZero() )  continue;
				
						if(debugPrint)	WriteLine( $"\nDF0:{DF0}\nDF1:{DF1}" );





				UInt128 rcIntersect81 = ConnectedCells81[DF0.rc1] & ConnectedCells81[DF0.rc2]
									  &	ConnectedCells81[DF1.rc1] & ConnectedCells81[DF1.rc2];
									  
				int rcIntersect = rcIntersect81.FindFirst_rc();
						if(debugPrint){
							WriteLine( $"\nDF0:{DF0}\nDF1:{DF1}" );
							WriteLine( $"rcIntersecr81:{rcIntersect.ToRCString()}  {rcIntersect81.ToBitString81N()}" );
						}

				bool solfound = Firework_Exorcet_SolResult( DF0, DF1, rcIntersect );
				if( !solfound )  continue;
				if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
			}

			return false;
		}

		private bool Firework_Exorcet_SolResult( UFirework DF0, UFirework DF1, int rcIntersect ){	

			int   rc0=DF0.rcStem, rc1=DF1.rcStem, FreeB=DF0.FreeB;
			UCell UC0_Stem=pBOARD[DF0.rcStem], UC1_Stem=pBOARD[DF1.rcStem];

			UC0_Stem.CancelB = UC0_Stem.FreeB. DifSet(FreeB);
			UC1_Stem.CancelB = UC1_Stem.FreeB. DifSet(FreeB);

			UInt128 Hexclude = ConnectedCells81[rcIntersect]. DifSet(DF0.rcStem_rc12B81 | DF1.rcStem_rc12B81);
			Hexclude &= (ConnectedCells81[DF0.rc1] | ConnectedCells81[DF0.rc2] );

			foreach( var P in Hexclude.IEGet_UCell_noB(pBOARD,FreeB) ) P.CancelB = P.FreeB&FreeB;
						
			if( pBOARD.All(p=>p.CancelB==0) )  return false;
			SolCode = 2;

			UCell UC0_rc1=pBOARD[DF0.rc1], UC0_rc2=pBOARD[DF0.rc2];
			UCell UC1_rc1=pBOARD[DF1.rc1], UC1_rc2=pBOARD[DF1.rc2];

			UC0_Stem.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr);
			UC0_rc1.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr2);
			UC0_rc2.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr2);

			UC1_Stem.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr);
			UC1_rc1.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr2);
			UC1_rc2.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr2);

			UCell UC_intersect=pBOARD[rcIntersect];
			UC_intersect.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr3);

			string st_DF0 = $"{DF0.ToStringResult()}";
			string st_DF1 = $"{DF1.ToStringResult()}";
			string st_IS  = $"Intersect_Cell:{rcIntersect.ToRCString()}";

			UInt128 exclude = pBOARD.FindAll(p=>p.CancelB>0).Aggregate( UInt128.Zero, (p,q) => p| UInt128.One<<q.rc);
			string  st_Exclude = exclude.ToRCStringComp();

			Result     = $"Firework_ALP {DF0.ToStringResult()} /{DF0.ToStringResult()} / {rcIntersect.ToRCString()}";
			ResultLong = $"Firework_ALP\n  Forework1:\n{DF0.ToStringResultLong()}\n  Forework2:\n{DF0.ToStringResultLong()}\n\n rcIntersect:{rcIntersect.ToRCString()}";

			return (SolCode==2);
		}
	}
}

#endif