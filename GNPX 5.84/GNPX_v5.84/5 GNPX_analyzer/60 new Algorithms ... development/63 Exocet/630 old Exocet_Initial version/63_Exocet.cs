using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using static System.Math;

using GIDOO_space;


namespace GNPX_space{
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//   under development (GNPXv6) ...  Not very elegant. Needs some code cleanup.
	//
	//   @@@@@ There are still discrepancies between the Sudoku-6 HP and GNPX-6 codes. @@@@@
	//
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

    public partial class JExocet_TechGen: AnalyzerBaseV2{
		// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
		//	"JuniorExocet" has a clear logic,
		//	so the algorithm and code are relatively simple!
		//	by:David P Bird	&nbsp;&nbsp;"JExocet Compendium"
		//	http://forum.enjoysudoku.com/jexocet-compendium-t32370.html
		//	*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

		static private readonly string  spX = new string(' ',10); 
		static private readonly string  sp5 = new string(' ',5); 
        static public  readonly UInt128 qZero   = UInt128.Zero;
        static public  readonly UInt128 qOne    = UInt128.One; 
		static public  readonly UInt128 qMaxB81 = (UInt128.One<<81)-qOne;

		static public int[]	CoverLineH = new int[]{
			0b000000001000000001000000001, 0b000000010000000010000000010, 0b000000100000000100000000100,	// Cover-Line across S0,S1,S2
			0b000001000000001000000001000, 0b000010000000010000000010000, 0b000100000000100000000100000,
			0b001000000001000000001000000, 0b010000000010000000010000000, 0b100000000100000000100000000,
			0b000000000111111111000000000, 0b111111111000000000000000000, 0b000000000000000000111111111 };	// Cross-Line. (S1,S2,S0)



		public List<UCell>  pBOARD => base.pBOARD;

		private static   string B81rcSt( UInt128 obj) => obj.ToRCStringComp();

		private static   int  To_rcReal( int rc ) => rc%100;
		private int			  stageNoPMemo = -9;

		private CellLinkMan   CeLKMan;
		internal bool		  debugPrint = false;


		private List<string> extResultLst = new();

		private UInt128		 BOARD_FreeCell81;

		// Adjust the search order, Cross-Lines is last.

		public  JExocet_TechGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

		public void Prepare_Exocet_TechGen(bool printB ){
			CeLKMan = new CellLinkMan(pAnMan);
			CeLKMan.Initialize();
            bool lkExtB = G6.UCellLinkExt;
            CeLKMan.PrepareCellLink( 1+2, lkExtB, printB:printB );    //StrongLink

			BOARD_FreeCell81 = pBOARD.Create_Free_BitExp128();	// FreeCell Bit Representation

			UExocet.qBOARD = base.pBOARD;
			UExocet_elem.qBOARD = base.pBOARD;
		//	USExocet.qBOARD = base.pBOARD;
		//	USExocet.BOARD_FreeCell81 = BOARD_FreeCell81;
		}

		public int Get_LockedHouse( UInt128 Cells81, int no, UInt128 Exclusion ){
			int noB = 1<<no;

			UInt128 Cells81_no = Cells81.IEGet_UCell_noB(pBOARD,noB).Aggregate(qZero, (a,uc)=> a| qOne<<uc.rc );
			int frame_Cells81_no = Cells81_no.Ceate_rcbFrameAnd();

			int h_locked = 0;
			foreach( int h in frame_Cells81_no.IEGet_BtoNo(27) ){	// 27:house size(row,column,block)
				UInt128 outer_Cells = (FreeCell81b9[no] & HouseCells81[h] ).DifSet(Cells81 | Exclusion);
				if( outer_Cells == qZero )  h_locked |= 1<<h;
			}

			return h_locked;
		}	
	}
}