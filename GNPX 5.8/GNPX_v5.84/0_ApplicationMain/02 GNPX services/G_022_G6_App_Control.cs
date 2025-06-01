using System;
using System.Windows.Media;
using System.IO;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using static System.Diagnostics.Debug;

namespace GNPX_space{
	public class G6_Base{
	// ===== Ver.7 Service =====================
		[XmlIgnore]  public List<UCell>  g7FinalState_TE;
		
		[XmlIgnore]  public List<UCell>  g7CurrentState; 
			//string st1 = string.Join("",pBOARD.ConvertAll(p=> Abs(p.No))).Replace("0","."). AddSpace9_81();

		[XmlIgnore]  public int[]		 g7_LSpattern = new int[10000];

		[XmlIgnore]  public bool		 g7MarkA0;					// ver.5.2- for Develop
		[XmlIgnore]  public bool		 g7Error;					// ver.5.2- for Develop
		[XmlIgnore]  public List<string> g7MarkA_MLst0 = null;	// ver.5.2- for Develop


	// ===== Ver.6 Service =====================
		[XmlIgnore]  public string OperationalMode = "NormalOperation";		// Initializing / NormalOperation

		[XmlIgnore]  public RenderTargetBitmap bmpGZero;
		[XmlIgnore]  public string GNPX_PuzzleFileName = "";
		[XmlIgnore]  public string Dir_SDK_Methods_XMLFileName;
		[XmlIgnore]  public string PG6Mode = "--";

		[XmlIgnore]  public string taskCompInfo;
		[XmlIgnore]  public int    FreeBmask = 0;

		[XmlIgnore]  public string OnWork = "";		// "":free "on executing"

		//[XmlIgnore]  public int    _SolverState = 0;
		//[XmlIgnore]  public int    SolverState { get=>_SolverState;
		//										 set{ _SolverState=value; WriteLine( $"Statte:({_SolverState},{EnginState})" ); } }
		[XmlIgnore]  public int    SolverState = 0;
		[XmlIgnore]  public int    EnginState = 0;



		public DateTime StartTime				= DateTime.Now;
		public string stopped_StatusMessage;
		public bool   command_Analysis_Stop;
		public string windows_Animation			= "chase"; //""  //There is no setting to turn it off.
		[XmlIgnore]  public bool LinkedMove		= true;

		public string Canceled					= "";
		public bool   Found_level_Solution		= false;

		public bool	 slowPlayback				= true;
		[XmlIgnore]  public bool sNoAssist		= false;
		[XmlIgnore]  public bool digitColoring  = false;
		[XmlIgnore]  public bool sWhiteBack		= false;

		public int  G60_PuzzleMinDifficulty		= 1;

		public int  Method_DifficultyLevel		= 4;
		public int  Method_RecommendLevel		= 4;
		public string Method_SelectionMode		= "Difficulty";

		public int  MSlvr_MaxLevel				= 13;
		public int  MSlvr_MaxNoAlgorithm		= 50;
		public int  MSlvr_MaxNoAllAlgorithm		= 400;
		public int  MSlvr_MaxTime				= 10;		//sec
		public int  RecommendLevel				= 20;		// Restricted within the program. Cannot be changed at runtime


		public int  PreferSimpleLinks	= 1;
		public int  Use_eALS			= 1;

		public List<string>  st_methodList = null;

// ---------------------------------------------------
		// <<< Func_SolveMethodOption >>>
		public bool Cell				= true;
		public bool AIC					= true;
		public bool ALS					= true;
		public bool ALSXZ				= false;
		public bool AnLS				= false;
		public bool eALS 				= false;
		public string ForceLx			= "ForceL2";
		public bool UCellLinkExt		= true;
		public bool DebugSolCheckMode	= false;
		public bool ForceChain_on		= false;
		[XmlIgnore] public bool GeneralLogic_on	= false;     
		public int  nud_GenLogMaxSize		= 3;
		public int  nud_GenLogMaxRank		= 1;
		public int  NiceLoopMax         = 10;
		public int  QSearchMaxGen       = 5;
		public int  ALSSizeMax			= 5;
		public int  ALSChainSizeMax		= 8;

		// <<< Func_Solver >>>
		[XmlIgnore] public bool   AnalysisResult = false;
		[XmlIgnore] public string stResult = "";

		// <<< Func_CreateAuto >>>
		[XmlIgnore] public int SolutionCounter = 0;
		public int  NoOfPuzzlesToCreate = 5;
        public int  Puzzle_LevelLow		= 1;
        public int  Puzzle_LevelHigh	= 5;	
		[XmlIgnore] public bool Search_AllSolutions = false;
		public bool Digits_Randomize    = true;
		public bool Save_CreatedPuzzle  = false;
		public bool CbxNextLSpattern	= false;    // Change Latin-Square Pattern on Success	
		public bool ResultShow			= true;
		[XmlIgnore] public string Info_LS_Generator = "";
		[XmlIgnore] public string Info_CreateMessage0 = "";
		[XmlIgnore] public string Info_CreateMessage1 = "";
		[XmlIgnore] public string Info_CreateMessage2 = "";
		[XmlIgnore] public string Info_CreateMessage3 = "";
		
        public int  retNZ;							// The original purpose is unclear and needs to be clarified and changed.

		public int  GenLStyp;
		public bool GenLS_turbo;
        public int  CellNumMax;
        [XmlIgnore] public int  LoopCC			= 0;
		[XmlIgnore] public int  TLoopCC			= 0;
        [XmlIgnore] public int  PatternCC		= 0;

		public int  RandomSeedVal_000;
//		public int  RandomSeedVal;
		public int	PatSel0X;

		[XmlIgnore] public int  LS_P_P0			= 0;
		[XmlIgnore] public int  LS_P_P2 		= 0;
		[XmlIgnore] public bool LS_PatternChange = false;	//... Redundant


		public int  LS_Pattern_Cntrl_P0			= 50;	// 50 - 100 - 1000 - 10000000
		public int  LS_Pattern_Cntrl_P2			= 40;	// 40 - 3136 (=56*56);

		public bool chbx_ShowProofMultiPaths  = true;
		public bool chbx_LinkExtension     = true;
		public bool chbx_DebugSolCheckMode = true;
		[XmlIgnore] public int[,] Sol99sta = new int[9,9];


		// <<< Func_CreateAllPuzzle >>>
		[XmlIgnore] public bool Develop_AllPuzzle = false;

		// <<< Func_Transform >>>
		[XmlIgnore] public bool DigitsChangeMode = false;
		[XmlIgnore] public int  Cell_rc;



		// <<< Power User / Power Mode >>>
		[XmlIgnore] public bool PowerUser = false;
		[XmlIgnore] public bool Solver_PowerMode = false;
		
		
		// <<< XmlSerializer >>>		
		[XmlIgnore] public XmlSerializer xmlSerializer = new XmlSerializer(typeof(G6_Base));


		public G6_Base(){}

	#region Solver I/O
		// <<< for output >>>
		public void Set_methodList( List<UAlgMethod> SolverList_Tmp ){
			st_methodList = new();
		    SolverList_Tmp.ForEach( P=>{ 
				string st = (P.IsChecked? "": "-") + P.MethodName.TrimStart(' ');
				st_methodList.Add(st);
            } );
		}

		// <<< for input >>>	
		public void Get_methodList( List<UAlgMethod> SolverList_Tmp ) {
		//	int IDx = 0;

			if( st_methodList == null )  return;
			st_methodList.ForEach( P=> {
				bool bChk = true;
			    if(P[0]=='-'){ bChk=false; P=P.Substring(1); }
				UAlgMethod Q = SolverList_Tmp.Find( x=> x.MethodName.Trim()==P);
                if( Q is UAlgMethod ){ Q.IsChecked=bChk; }
			} );
			UAlgMethod Q = SolverList_Tmp.Find( x=> x.MethodName.Trim()=="GeneralLogic");
			if( Q!=null ) Q.IsChecked = false;
		}
	#endregion Solver I/O


		public override string ToString( ){
			string st = "";
			//.....................
			return st;
		}
	}
}


