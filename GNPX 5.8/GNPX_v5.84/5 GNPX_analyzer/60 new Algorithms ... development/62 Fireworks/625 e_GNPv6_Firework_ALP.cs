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

/*
	//The New Sudoku Players' Forum, ÅwFireworksÅx,
	// http://forum.enjoysudoku.com/fireworks-t39513.html
	//  http://forum.enjoysudoku.com/takabisha-t39322.html

+-------+-------+-------+
| . . . | 6 . 1 | . . . |
| . . 2 | . 3 . | 7 . . |
| . 8 . | . . . | . 4 . |
+-------+-------+-------+
| 1 . . | . . 4 | . . 5 |
| . 7 . | 1 . . | . 9 . |
| 3 . . | 9 6 . | . . 1 |
+-------+-------+-------+
| . 6 . | . . . | . 8 . |
| . . 9 | . 5 . | 2 . . |
| . . . | 7 . 6 | . . . |
+-------+-------+-------+
Takabisha - shye
skfr 8.3
...6.1.....2.3.7...8.....4.1....4..5.7.1...9.3..96...1.6.....8...9.5.2.....7.6...
437621958952438716681597342198374625276185493345962871563249187719853264824716539

.---------------------.------------------.--------------------.
| 4579   345    3457  | 6   x24789  1    | 3589   235  x28-39 |
| 4569   145    2     | 458  3      89   | 7      156   689   |
| 5679   8      13567 | 25   79-2   279  | 13569  4     2369  |
:---------------------+------------------+--------------------:
| 1      9      68    | 238  7-28   4    | 368    2367  5     |
|x24568  7      456-8 | 1   #28     35-28| 346-8  9    x23468 |
| 3      245    458   | 9    6      2578 | 48     27    1     |
:---------------------+------------------+--------------------:
| 2457   6      13457 | 234  149-2  239  | 13459  8     3479  |
| 478    134    9     | 348  5      38   | 2      136   3467  |
|x28-45  12345  13458 | 7   x12489  6    | 13459  135   349   |
'---------------------'------------------'--------------------'
dual firework ALP
original post

like the above this uses the (x|y)cell1 = (x|y)cell2 idea, but for something a bit fancier

fireworks on 2s and 8s in both r1c9b3 & r9c1b7
(2|8)r1c5 = (2|8)r5c9
(2|8)r5c1 = (2|8)r9c5
combined with r5c5 limited to only [28], at most one of 2 or 8 can be in r5c19 and at most one in r19c5. almost locked pairs in r5 and c5
stte

also worth mentioning is r1c9 and r9c1 become limited to [28], much like how in a regular almost locked pair you get limitations on a cell
here are some more resisitant puzzles which have the same setup as this

this next example is actually the first puzzle i made that had these ideas, back then i didnt view them the same way though, and its pretty complex
it features a firework with 5 positions (9s) being useful

 */


//.9.....4.6...2...8...1.4.....2..758..8.5...1...584.3.....4.5...1...3...6.7.....9. transform for GNPX
//591678243647923158238154967462317589783592614915846372829465731154739826376281495

    public partial class Firework_TechGen: AnalyzerBaseV2{


        public bool Firework_ALP( ){
			// ===== Prepare =====
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_Firework_TechGen();
			}
			debugPrint = false;	
			 
			// ===== Analize =====
			if( FireworkAgg_List==null || FireworkAgg_List.Count<=0 )  return false; 
			List<UFirework> Dual_Firework_List = FireworkAgg_List.FindAll(q=> q.FreeBC==2 && q.sw);
			if( Dual_Firework_List==null || Dual_Firework_List.Count==0 )  return false;
						if(debugPrint) Dual_Firework_List.ToList().ForEach( P=> WriteLine(P) );

			foreach( UFirework DF0 in Dual_Firework_List ){
				UInt128 rcIntersect81 = ConnectedCells81[DF0.rc1] & ConnectedCells81[DF0.rc2];
				rcIntersect81 = rcIntersect81.DifSet(DF0.rcStem_rc12B81);
				int FreeB = DF0.FreeB;
						if(debugPrint){
							WriteLine( $"\nDF0:{DF0}" );
							WriteLine( $"rcIntersect81:{rcIntersect81.ToRCStringComp()} {rcIntersect81.ToBitString81N()}" );
						}

				foreach( UCell UCintersect in rcIntersect81.IEGet_UCell_noB(pBOARD,FreeB) ){
					if( UCintersect.FreeBC!=2 || UCintersect.FreeB!=DF0.FreeB )  continue;
						if(debugPrint){
							WriteLine( $"UCintersect:{UCintersect.ToString()}" );
						}	

					bool solfound = Firework_ALP_SolResult( DF0, UCintersect );
					if( !solfound )  continue;

					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
				}
			}

			return false;
		}

		private bool Firework_ALP_SolResult( UFirework DF0, UCell UCIntersect ){	

			int   rc0=DF0.rcStem, FreeB=DF0.FreeB, rcIntersect=UCIntersect.rc;
			UCell UC0_Stem=pBOARD[DF0.rcStem];

			UC0_Stem.CancelB = UC0_Stem.FreeB. DifSet(FreeB);
					

			if( pBOARD.All(p=>p.CancelB==0) ){ pBOARD.ForEach(p=> p.ECrLst=null ); }
			else{
				SolCode = 2;
				UCell UC0_rc1=pBOARD[DF0.rc1], UC0_rc2=pBOARD[DF0.rc2];

				UC0_Stem.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr);
				UC0_rc1.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr2);
				UC0_rc2.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr2);

				UCIntersect.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr3);

				string st_DF0 = $"{DF0.ToStringResult()}";
				string st_IS  = $"Intersect_Cell:{rcIntersect.ToRCString()}";

				UInt128 exclude = pBOARD.FindAll(p=>p.CancelB>0).Aggregate( UInt128.Zero, (p,q) => p| UInt128.One<<q.rc);
				string  st_Exclude = exclude.ToRCStringComp();

				Result     = $"Firework_ALP FW:{st_DF0} / {rcIntersect.ToRCString()}";
				ResultLong = $"Firework_ALP\n  Forework : {st_DF0}\n rcIntersect : {st_IS}";
			}
			return (SolCode==2);
		}
	}
} 