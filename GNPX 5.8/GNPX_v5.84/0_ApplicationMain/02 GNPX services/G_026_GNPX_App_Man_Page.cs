using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;

using static System.Math;
using static System.Diagnostics.Debug;

using System.Windows.Media;
using System.Threading;


using GIDOO_space;
using System.Globalization;
using System.Text.RegularExpressions;
//using System.Drawing;
//using System.Drawing;


namespace GNPX_space{
    using pGPGC = GNPX_Puzzle_Global_Control;

    public partial class GNPX_App_Man{
		static public event		GNPX_EventHandler Send_Command_to_FuncPage; 

		static public G6_Base    G6 = new G6_Base();

        public GNPX_win         pGNP00win;
        public GNPX_App_Ctrl    App_Ctrl;                      //Puzzle Generator  
		public GNPX_Engin       GNPX_Eng;                      //Analysis Engine
		public PuzzleFile_IO    PuzzleFile_IO;
        public GNPX_Graphics    gnpxGrp;             //board bitmap


        public UPuzzle          ePZL{	 get=>GNPX_Eng.ePZL; set=>GNPX_Eng.ePZL=value; }  //current Puzzle is in pGNPX_Eng

        static public bool[]    SlvMtdCList = new bool[100];
        static public bool      chbx_ConfirmMultipleCells;				//#####
 
        static public DateTime  MultiSolve_StartTime = DateTime.Now;
        static public string    fNamePara;






        public string           SDK_Methods_XMLFileName = "SDK_Methods_XMLV6.xml"; // (Ver.6 was planned, but this was introduced first.)
        public string           _Develop_SuDoKuName = "_Develop_SuDoKu.txt";
        public List<UCell>      _Develop_Puzzle = null;


		public Page				objFunc_File;	
		public Page				objFunc_Solve;	
		public Page				objFunc_Create;	
		public Page				objFunc_Option;		
		public Page				objFunc_Transform;	
		public Page				objFunc_HomePage;
		public Page				objFunc_ExtensionsPage;
		private List<Page>		GNPX_Pages=null;

        public int[]            SDK81;

		public List<UAlgMethod> SolverList_Def;			// Initial definition of GNPX system
		public List<UAlgMethod> SolverList_Base;		// valid analysis routines List
			//public List<UAlgMethod> _SolverList_Base;		// valid analysis routines List
			//public List<UAlgMethod> SolverList_Base{ get=>_SolverList_Base; set=>_SolverList_Base=value; }// valid analysis routines List

		public List<UAlgMethod> SolverList_App = new(); // valid analysis routines List

		public List<string>     LanguageList;

        public GNPX_App_Man( GNPX_win pGNP00win ){

		    App_Environment.pixelsPerDip = VisualTreeHelper.GetDpi(pGNP00win).PixelsPerDip;

			{
				List<string> DirLst=Directory.EnumerateDirectories(".").ToList();
				LanguageList=new List<string>();
				LanguageList.Add("en");
				foreach( var P in DirLst ){
					var Q = P.Replace(".","").Replace("\\","");
					if( Q == "en" )  continue;
					if( Q.Length == 2 || (Q[2]=='-' && Q.Length==5) )  LanguageList.Add(Q);
				}

				LanguageList = LanguageList.FindAll(P=>(P.Length==2 ||(P[2]=='-' && P.Length==5)));
				LanguageList.Sort();
			}


            //=======================================================
			// Here,
			//   create functions(class object),
			//   link the object.
            //=======================================================
            this.pGNP00win = pGNP00win;

            GNPX_Eng  = new GNPX_Engin( this );       // unique in the system
            App_Ctrl  = new GNPX_App_Ctrl(this,0);
			gnpxGrp = new GNPX_Graphics(this);		
			UC_PB_GBoard.pGNPX_App = this;					//@@@@@@@@@@@@@@@@@@@

			GNPX_Puzzle_Global_Control.pGNPX_Eng = GNPX_Eng;
			PuzzleFile_IO = new( this );

            //-------------------------------------------------------
            SolverList_App = new ();


			objFunc_File      = new Func_File( this );	//To display at the start.
			objFunc_Solve     = new Func_Solve( this );
			objFunc_Create    = new Func_Create( this );
			objFunc_Option    = new Func_Option( this );	
			objFunc_Transform = new Func_Transform( this );
			objFunc_HomePage  = new Func_HomePage( this );
			objFunc_ExtensionsPage = new Func_Extensions( this );
		}


        public bool GNPX_App_Initialize(){	//""@@ no used?
            UPuzzle tPZL = pGPGC.GetCurrentPuzzle();
            if( tPZL is null )  return false;
            GNPX_Eng.Set_NewPuzzle(tPZL);

            GNPX_Eng.AnalyzerCounterReset( );
            GNPX_Eng.AnMan.ResetAnalysisResult(true);			//Return to initial state
            GNPX_Eng.AnMan.Update_CellsState( ePZL.BOARD );
			ePZL.extResult = "";
            return true;
        }

		public int Get_Difficulty_SolverList_App(){
			if( SolverList_App==null || SolverList_App.Count== 0 ) return 0;
			int diff = SolverList_App.Max( mth =>mth.difficultyLevel );
			return diff;
		}

    }

}
