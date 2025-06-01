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
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*


	using G6_SF = G6_staticFunctions;

    public partial class Senior_Exocet_TechGen: AnalyzerBaseV2{

		static private readonly string  spX = new string(' ',10); 
		static private readonly string  sp5 = new string(' ',5); 
        static public  readonly UInt128 qZero   = UInt128.Zero;
        static public  readonly UInt128 qOne    = UInt128.One; 
		static public  readonly UInt128 qMaxB81 = (UInt128.One<<81)-qOne;

		private int FreeB_UInt128( UInt128 X)   => (X==qZero)? 0: X.IEGet_UCell(pBOARD) .Aggregate( 0, (a,uc)=> a| uc.FreeB ); 
		private int noB_FixedFreeB_UInt128( UInt128 X)   => (X==qZero)? 0: X.IEGet_UCell(pBOARD) .Aggregate( 0, (a,uc)=> a| uc.noB_FixedFreeB ); 
//
		private UInt128		  BOARD_FreeCell81;		// Bit representation of Free  cells on the board
		private UInt128		  BOARD_Fixed81;		// Bit representation of Fixed cells on the board

		private int			  stageNoPMemo = -9;

		private CellLinkMan   CeLKMan;
		private List<string>  extResultLst = new();

		internal bool		  debugPrint = false;

		public  Senior_Exocet_TechGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

		public void Prepare_SExocet_TechGen(bool printB ){
			CeLKMan = new CellLinkMan(pAnMan);
			CeLKMan.Initialize();
            bool lkExtB = G6.UCellLinkExt;
            CeLKMan.PrepareCellLink( 1+2, lkExtB, printB:printB );    //StrongLink

			BOARD_FreeCell81 = pBOARD.Create_Free_BitExp128();	// FreeCell Bit Representation
			BOARD_Fixed81    = pBOARD.Where(p=>p.No!=0).Select(p=>p.rc).Aggregate(qZero, (a,b) => a| qOne<<b);

			G6_staticFunctions.pBOARD = base.pBOARD;
			USExocet.qBOARD			  = base.pBOARD;
			UCoverLine.qBOARD		  = base.pBOARD;
			USExocet.BOARD_FreeCell81 = BOARD_FreeCell81;
			UCoverLine.BOARD_FreeCell81 = BOARD_FreeCell81;
		}





		private void Debug_SLIne_MatrixPrint( USExocet SExo, int saX, int sbX, UInt128 obj1, UInt128 obj2, UInt128 SLineBand ){
			UCoverLine0 CL_A=SExo.CL012[saX], CL_B=SExo.CL012[sbX];			// ... CreossLine
			UInt128 _SLine01 = (CL_A.SLine0 | CL_B.SLine0) & SLineBand;
			string DB_title = $"_SLine{saX}_SLine{sbX} Object1:{obj1.TBScmp()} Object2:{obj2.TBScmp()}";
			G6_SF.__MatrixPrint( Flag:qZero,  _SLine01, obj1, obj2, DB_title );
			WriteLine( $"(Object1,Object2):{(obj1.TBScmp(), obj2.TBScmp())}" );
		}

		private int Get_LockedHouse( UInt128 Cells81, int no, UInt128 Exclusions81 ){
			// Find House where #n in cells "Cells81" is locked.
			// Output is the bit representation of the House number.
			// (Exclude Exclusions81 from the inspection target. =0:No Exclusions)
			
			int noB = 1<<no;
			UInt128 Cells81_no = Cells81.IEGet_UCell_noB(pBOARD,noB).Aggregate(qZero, (a,uc)=> a| qOne<<uc.rc );
			int frame_Cells81_no = Cells81_no.Ceate_rcbFrameAnd();	// House number(0-26 0-8:row 9-17:column 18-26:block)

			int h_locked = 0;
			foreach( int h in frame_Cells81_no.IEGet_BtoNo(27) ){	// 27:house size(row,column,block)
				UInt128 outer_Cells = (FreeCell81b9[no] & HouseCells81[h] ).DifSet(Cells81 | Exclusions81);
				if( outer_Cells == qZero )  h_locked |= 1<<h;
			}

			return h_locked;
		}	


	}
}