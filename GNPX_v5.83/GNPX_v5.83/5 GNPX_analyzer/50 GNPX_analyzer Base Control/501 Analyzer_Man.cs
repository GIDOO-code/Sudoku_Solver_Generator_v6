using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Media;
using System.Threading;
using static System.Diagnostics.Debug;
using static System.Math;
using System.Security.Cryptography;
using System.Windows.Interop;
using System.IO;
using System.Text;
using GIDOO_space;

namespace GNPX_space{
    using pRes=Properties.Resources;

    // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
#if developv_5_1
	// Next development goal   ### �폜
	// (1) Exocet
	// (2) SK Loop
	// (3) Force Net
	// (4) Firework continuation
#endif


    // Analyzer Manager
    public class GNPX_AnalyzerMan{
		static private G6_Base  G6 => GNPX_App_Man.G6;
		static public  UInt128 b081_all  = (UInt128.One<<81)-1;
        private UInt128[]    pConnectedCells81 => AnalyzerBaseV2.ConnectedCells81;
		private UInt128[]    pHouseCells81 => AnalyzerBaseV2.HouseCells81; 

        static private Color _Black_=Colors.Black;

		private bool		 __SimpleAnalyzerB__ => AnalyzerBaseV2.__SimpleAnalyzerB__;
        private bool         DebugSolCheckMode = G6.DebugSolCheckMode;

		public GNPX_App_Man  pGNPX_App => pGNPX_Eng.pGNPX_App;    
		
		public bool          SolInfoB{ get=>GNPX_Engin.SolInfoB; }
        public GNPX_Engin    pGNPX_Eng;

        public  int          stageNoP	=> pGNPX_Eng.stageNoP;

		public UPuzzle		 ePZL		=> pGNPX_Eng.ePZL; 
        public bool          _Not_Puzzle_{ get=>ePZL._Not_Puzzle_; set=>ePZL._Not_Puzzle_=value; }   
		public int           stageNo	=> ePZL.stageNo;     
        public List<UCell>   pBOARD		=> ePZL.BOARD;
        public int           SolCode{	set =>ePZL.SolCode=value; }
        public string        Result{	set=>ePZL.Sol_Result=value; }
        public string        ResultLong{ set=>ePZL.Sol_ResultLong=value; }


        public bool         chbx_ConfirmMultipleCells{ get=>GNPX_App_Man .chbx_ConfirmMultipleCells; }

		private int			stageNoPMemo=-1;
        public TimeSpan     SdkExecTime;

//        public List<UAlgMethod>  _SolverLst0;
        private int[,]     Sol99sta{ get=> G6.Sol99sta; } //int[,]




        public  GNPX_AnalyzerMan( ){ }

        public  GNPX_AnalyzerMan( GNPX_Engin pGNPX_Eng ){
            this.pGNPX_Eng = pGNPX_Eng;

          //================================================================================================
          //  UAlgMethod( int pid, string MethodName, int difficultyLevel, dSolver Method, bool GenLogB=false )
          //================================================================================================
		    List<UAlgMethod> _SolverLst0 = new();

            var SSingle = new SimpleSingleGen(this);
            _SolverLst0.Add( new UAlgMethod( 1, 0, "LastDigit",				1, SSingle.LastDigit ) );
            _SolverLst0.Add( new UAlgMethod( 2, 0, "NakedSingle",			1, SSingle.NakedSingle ) );
            _SolverLst0.Add( new UAlgMethod( 3, 0, "HiddenSingle",			1, SSingle.HiddenSingle ) );

            var eGLTech = new eGeneralLogicGen(this);
            _SolverLst0.Add( new UAlgMethod( 4, 10, " GeneralLogic",			2, eGLTech.eGeneralLogic, true) );

            var LockedCand = new LockedCandidateGen(this);
            _SolverLst0.Add( new UAlgMethod( 5, 0, "LockedCandidate", 2, LockedCand.LockedCandidate ) );
            
			// Algorithm selection level : simple:0 3  recommend1:4 6  recommend1:7 9  all:10 
            var LockedSet = new LockedSetGen(this);
            _SolverLst0.Add( new UAlgMethod( 10,  3, "LockedSet(2D)",        3, LockedSet.LockedSet2 ) );
            _SolverLst0.Add( new UAlgMethod( 12,  4, "LockedSet(3D)",        4, LockedSet.LockedSet3 ) );
            _SolverLst0.Add( new UAlgMethod( 14,  5, "LockedSet(4D)",        5, LockedSet.LockedSet4 ) );
            _SolverLst0.Add( new UAlgMethod( 16, 10, "LockedSet(5D)",        6, LockedSet.LockedSet5 ) );//complementary to 4D
            _SolverLst0.Add( new UAlgMethod( 18, 11, "LockedSet(6D)",        6, LockedSet.LockedSet6 ) );//complementary to 3D
            _SolverLst0.Add( new UAlgMethod( 20, 12, "LockedSet(7D)",        6, LockedSet.LockedSet7 ) );//complementary to 2D           
            _SolverLst0.Add( new UAlgMethod( 11,  3, "LockedSet(2D)Hidden",  3, LockedSet.LockedSet2Hidden ) );           
            _SolverLst0.Add( new UAlgMethod( 13,  5, "LockedSet(3D)Hidden",  4, LockedSet.LockedSet3Hidden ) );          
            _SolverLst0.Add( new UAlgMethod( 15,  8, "LockedSet(4D)Hidden",  5, LockedSet.LockedSet4Hidden ) );
            _SolverLst0.Add( new UAlgMethod( 17, 10, "LockedSet(5D)Hidden",  6, LockedSet.LockedSet5Hidden ) );//complementary to 4D
            _SolverLst0.Add( new UAlgMethod( 19, 11, "LockedSet(6D)Hidden",  6, LockedSet.LockedSet6Hidden ) );//complementary to 3D        
            _SolverLst0.Add( new UAlgMethod( 21, 12, "LockedSet(7D)Hidden",  6, LockedSet.LockedSet7Hidden ) );//complementary to 2D

            var Fish = new FishGen(this);
            _SolverLst0.Add( new UAlgMethod( 30,  3, "XWing",             4, Fish.XWing ) );
            _SolverLst0.Add( new UAlgMethod( 31,  4, "SwordFish",         5, Fish.SwordFish ) );
            _SolverLst0.Add( new UAlgMethod( 32,  4, "JellyFish",         6, Fish.JellyFish ) );
            _SolverLst0.Add( new UAlgMethod( 33, 10, "Squirmbag",        10, Fish.Squirmbag ) );//complementary to 4D  
            _SolverLst0.Add( new UAlgMethod( 34, 11, "Whale",            10, Fish.Whale ) );    //complementary to 3D 
            _SolverLst0.Add( new UAlgMethod( 35, 12, "Leviathan",        10, Fish.Leviathan ) );//complementary to 2D 

            _SolverLst0.Add( new UAlgMethod( 40, 5, "Finned_XWing",       5, Fish.FinnedXWing ) );
            _SolverLst0.Add( new UAlgMethod( 41, 6, "Finned_SwordFish",   6, Fish.FinnedSwordFish ) );
            _SolverLst0.Add( new UAlgMethod( 42, 6, "Finned_JellyFish",   6, Fish.FinnedJellyFish ) );
            _SolverLst0.Add( new UAlgMethod( 43, 7, "Finned_Squirmbag",   7, Fish.FinnedSquirmbag ) );//not complementary with fin
            _SolverLst0.Add( new UAlgMethod( 44, 7, "Finned_whale",        7, Fish.FinnedWhale ) );    //not complementary with fin
            _SolverLst0.Add( new UAlgMethod( 45, 7, "Finned_Leviathan",   7, Fish.FinnedLeviathan ) );//not complementary with fin

            _SolverLst0.Add( new UAlgMethod( 46, 9, "Franken/Mutant_Fish",        8, Fish.FrankenMutantFish ) );
            _SolverLst0.Add( new UAlgMethod( 47, 9, "Finned Franken/Mutant_Fish", 9, Fish.FinnedFrankenMutantFish ) );

            _SolverLst0.Add( new UAlgMethod( 200, 13, "EndoFinned_F/M_Fish",       11, Fish.EndoFinnedFMFish ) );
            _SolverLst0.Add( new UAlgMethod( 200, 13, "Finned_EndoFinned_F/M_Fish",11, Fish.FinnedEndoFinnedFMFish ) );

            _SolverLst0.Add( new UAlgMethod( 201, 13, "CannibalisticF/M_Fish",        13, Fish.CannibalisticFMFish ) );
            _SolverLst0.Add( new UAlgMethod( 201, 13, "Finned_Cannibalistic_F/M_Fish",13, Fish.FinnedCannibalisticFMFish ) );

            var nxgCellLink = new NXGCellLinkGen(this);
            _SolverLst0.Add( new UAlgMethod( 50, 4, "Skyscraper",       4, nxgCellLink.Skyscraper ) );
		 // _SolverLst0.Add( new UAlgMethod( 51, 5, "EmptyRectangle",   5, nxgCellLink.EmptyRectangle ) );
            _SolverLst0.Add( new UAlgMethod( 51, 4, "EmptyRectangleEx", 4, nxgCellLink.EmptyRectangleEx ) );	//New algorithm using bit representation

            _SolverLst0.Add( new UAlgMethod( 52, 6, "XY_Wing",          5, nxgCellLink.XYwing ) );
            _SolverLst0.Add( new UAlgMethod( 53, 6, "W_Wing",           5, nxgCellLink.Wwing ) );

            _SolverLst0.Add( new UAlgMethod( 55, 6, "RemotePair",       4, nxgCellLink.RemotePair ) );    
            _SolverLst0.Add( new UAlgMethod( 56, 7, "XChain",           5, nxgCellLink.XChain ) );
            _SolverLst0.Add( new UAlgMethod( 57, 7, "XYChain",          7, nxgCellLink.XYChain ) ); 
        
            _SolverLst0.Add( new UAlgMethod( 70, 6, "Color_Trap",       5, nxgCellLink.Color_Trap ) );
            _SolverLst0.Add( new UAlgMethod( 71, 6, "Color_Wrap",       5, nxgCellLink.Color_Wrap ) );
            _SolverLst0.Add( new UAlgMethod( 72, 7, "MultiColor_Type1", 6, nxgCellLink.MultiColor_Type1 ) );
            _SolverLst0.Add( new UAlgMethod( 73, 7, "MultiColor_Type2", 6, nxgCellLink.MultiColor_Type2 ) );
        
            var AnLSTechP = new AnLSTechGen(this);
            _SolverLst0.Add( new UAlgMethod( 80, 7, "SueDeCoq",             7, AnLSTechP.SueDeCoq ) );		  // original code
			_SolverLst0.Add( new UAlgMethod( 81, 5, "SueDeCoqEx1",          5, AnLSTechP.SueDeCoqEx1 ) );		  //   > ver.5
			_SolverLst0.Add( new UAlgMethod( 82, 6, "SueDeCoqEx2",          6, AnLSTechP.SueDeCoqEx2 ) );
			_SolverLst0.Add( new UAlgMethod( 83, 7, "Franken_SDCEx3",       7, AnLSTechP.Franken_SDCEx3 ) );
			_SolverLst0.Add( new UAlgMethod( 84, 5, "Finned_SueDeCoqEx1",   5, AnLSTechP.Finned_SueDeCoqEx1 ) );
		 	_SolverLst0.Add( new UAlgMethod( 85, 6, "Finned_SueDeCoqEx2",   6, AnLSTechP.Finned_SueDeCoqEx2 ) );
		 	_SolverLst0.Add( new UAlgMethod( 86, 7, "Finned_Franken_SDCEx3",7, AnLSTechP.Finned_Franken_SDCEx3 ) );
/*
            var SimpleXYZ = new SimpleUVWXYZwingGen(this);                      //            > Replaced with ALS version(XYZ_WingALS)
            _SolverLst0.Add( new UAlgMethod( 90, 8, "XYZ_Wing",         5, SimpleXYZ.XYZwing ) );	// not recommended. Updated to XYZwingALS
            _SolverLst0.Add( new UAlgMethod( 91, 8, "WXYZ_Wing",        5, SimpleXYZ.WXYZwing ) );	// not recommended. Updated to XYZwingALS
            _SolverLst0.Add( new UAlgMethod( 92, 8, "VWXYZ_Wing",       6, SimpleXYZ.VWXYZwing ) );	// not recommended. Updated to XYZwingALS
            _SolverLst0.Add( new UAlgMethod( 93, 8, "UVWXYZ_Wing",      6, SimpleXYZ.UVWXYZwing ) );	// not recommended. Updated to XYZwingALS
*/
            var ALSTech = new ALSTechGen(this);									// ALS version
            _SolverLst0.Add( new UAlgMethod( 101, 7, "XYZ_WingALS",         5, ALSTech.XYZwingALS ) );	// recommended
            _SolverLst0.Add( new UAlgMethod( 102, 7, "ALS_XZ",              6, ALSTech.ALS_XZ ) );
            _SolverLst0.Add( new UAlgMethod( 103, 7, "ALS_XY Wing",         7, ALSTech.ALS_XY_Wing ) );

            _SolverLst0.Add( new UAlgMethod( 105, 8, "eALS_ChainA",         8, ALSTech.eALS_Chain ) );
            _SolverLst0.Add( new UAlgMethod( 106, 8, "ALS_DeathBlossom",    8, ALSTech.ALS_DeathBlossom ) );     
            _SolverLst0.Add( new UAlgMethod( 107, 9, "ALS_DeathBlossomEx", 10, ALSTech.ALS_DeathBlossomEx ) );   

            var eNetworkTech = new eNetwork_App(this);      
            _SolverLst0.Add( new UAlgMethod( 110,  9, "eNetwork_NiceLoop",    9, eNetworkTech.eNetwork_NiceLoop)); 
            _SolverLst0.Add( new UAlgMethod( 111,  13, "eNetwork_NiceLoopEx", 11, eNetworkTech.eNetwork_NiceLoopEx)); 

            _SolverLst0.Add( new UAlgMethod( 108, 10, "eNW_DeathBlossom",    11, eNetworkTech.eNW_DeathBlossom));
            _SolverLst0.Add( new UAlgMethod( 120, 13, "eNetwork_FC_Cells",   11, eNetworkTech.eNetwork_FC_Cells));
            _SolverLst0.Add( new UAlgMethod( 121, 13, "eNetwork_FC_House",   11, eNetworkTech.eNetwork_FC_House));
          //_SolverLst0.Add( new UAlgMethod( 122, 10, "eNetwork_FC_Contradiction", 10, eNetworkTech.eNetwork_FC_Contradiction));

            _SolverLst0.Add( new UAlgMethod( 130, 11, "eALS_Wing_2D",         9, eNetworkTech.eALS_Wing_2D ) );
            _SolverLst0.Add( new UAlgMethod( 131, 11, "eALS_Wing_3D",        10, eNetworkTech.eALS_Wing_3D ) );        
            _SolverLst0.Add( new UAlgMethod( 132, 11, "eALS_Wing_4D",        11, eNetworkTech.eALS_Wing_4D ) );
        
            _SolverLst0.Add( new UAlgMethod( 206, 14, "eKrakenFish",         13, eNetworkTech.eKrakenFish)); 
            _SolverLst0.Add( new UAlgMethod( 206, 15, "Finned_eKrakenFish",  13, eNetworkTech.Finned_eKrakenFish)); 


			var SubsetTechP = new SubsetTechGen(this);  // under development (GNPXv5.1)
			_SolverLst0.Add( new UAlgMethod( 140, 7, "SubsetExclusion",    7, SubsetTechP.SubsetExclusion ) );
			_SolverLst0.Add( new UAlgMethod( 141, 7, "SubsetExclusionALS", 8, SubsetTechP.SubsetExclusion_ALS ) );

			var Firework_TechP = new Firework_TechGen(this);  // under development (GNPXv5.1)
			_SolverLst0.Add( new UAlgMethod( 150, 8, "Firework_Triple",    8, Firework_TechP.Firework_Triple ) );
			_SolverLst0.Add( new UAlgMethod( 151, 8, "Firework_Quadruple", 8, Firework_TechP.Firework_Quadruple ) );
			_SolverLst0.Add( new UAlgMethod( 152, 8, "Firework_WWing",     8, Firework_TechP.Firework_WWing ) );
			_SolverLst0.Add( new UAlgMethod( 153, 8, "Firework_LWing",     8, Firework_TechP.Firework_LWing ) );
			_SolverLst0.Add( new UAlgMethod( 154, 8, "Firework_ALP",       8, Firework_TechP.Firework_ALP ) );
			_SolverLst0.Add( new UAlgMethod( 155, 8, "Firework_DualALP",   8, Firework_TechP.Firework_DualALP ) );


		// Exocet ... Initial version
		//	var JExocet_TechGen = new JExocet_TechGen(this);  // Exocet ... Initial version
		//	_SolverLst0.Add( new UAlgMethod( 163, 9, "Junior_Exocet_JE2", 8, JExocet_TechGen.Junior_Exocet_JE2) );
		//	_SolverLst0.Add( new UAlgMethod( 164, 9, "Junior_Exocet_JE1", 8, JExocet_TechGen.Junior_Exocet_JE1) );	

		// Exocet ... under development
			var SExocet_Nxg_TechGen = new Senior_Exocet_TechGen(this);
			_SolverLst0.Add( new UAlgMethod( 165, 9, "Senior_Exocet_JE2",      8, SExocet_Nxg_TechGen.Junior_Exocet_JE2) );	
			_SolverLst0.Add( new UAlgMethod( 165, 9, "Senior_Exocet_JE1",      8, SExocet_Nxg_TechGen.Junior_Exocet_JE1) );	

	//	    _SolverLst0.Add( new UAlgMethod( 165, 9, "Senior_Exocet_Basic",    8, SExocet_Nxg_TechGen.SExocet_Basic) );  
	//		_SolverLst0.Add( new UAlgMethod( 165, 9, "Senior_Exocet_standard", 8, SExocet_Nxg_TechGen.SExocet) );
	//		_SolverLst0.Add( new UAlgMethod( 165, 9, "Senior_Exocet_Single",   8, SExocet_Nxg_TechGen.SExocet_Single) );
	//		_SolverLst0.Add( new UAlgMethod( 165, 9, "Senior_Exocet_SingleBase", 8, SExocet_Nxg_TechGen.SExocet_SingleBase) );




            _SolverLst0.Sort( (a,b)=> (a.ID-b.ID) );

			pGNPX_App.SolverList_Def = _SolverLst0;				
			//_SolverLst0.ForEach( p=> WriteLine( p ) );

//			pGNPX_App.SolverLstDev = _SolverLst0.Copy(); //A copy of the List container, with the same contents.
        }

//==========================================================
        private (int,string)  stageNo_MethodName=(-1,""); 
        private int methodCC = 0;
		public int  solutionC;

		public void stageNo_MethodName_Reset() => stageNo_MethodName=(-1,"");


		public bool IsContinueAnalysis( ){
		    {//Analyzer Bug : Code style does not meet specifications
				if( ePZL is null )  return false;
				if( ePZL.Sol_ResultLong is null || ePZL.Sol_Result is null )  return false;   // ... interrupt
				if( ePZL.Sol_Result.Length < 5 ){ WriteLine( "... in Development ... solver incomplete " ); return true; }
			}

			bool errorB = pGNPX_Eng.IsSolutionValid();
			if( !errorB && __SimpleAnalyzerB__ )  return false;

			bool IsValid = SnapSaveGP(errorB);
			return IsValid;
		}

		public UPuzzle FirstpPZL(){
			//var pChild_PZLs = ePZL.Child_PZLs;
			if( ePZL.Child_PZLs==null || ePZL.Child_PZLs.Count==0 )  return null;
			return ePZL.Child_PZLs[0];
		}

        public bool SnapSaveGP( bool errorB, bool OmitteDuplicateSolutionB=true ){
            try{ 
                //var pChild_PZLs = ePZL.Child_PZLs;
                 ePZL.selectedIX = 0;
				 ePZL.pMethod = GNPX_Engin.AnalyzingMethod;	// pGNPX_Eng.


				// =====================================================================================================
				// Processing by solution type
				// <<< Single >>>
					if( GNPX_Engin.MltAnsSearch is false ){ //... Single Search
						ePZL.Child_PZLs.Add(ePZL); //In one-solution search, save the same Puzzle-object without copying
						G6.command_Analysis_Stop = true;
						return false;	//true; #####0827
					}


				// <<< Multiple >>>
					string __SolResultKey = $"{stageNoP}_{ePZL.Sol_ResultLong.Replace("\r"," ").Replace("\n"," ")}";
					var EAMethod = GNPX_Engin.AnalyzingMethod;				
					// Excluding the same solution
					if( OmitteDuplicateSolutionB && ePZL.Child_PZLs.Any(Q=>(Q.__SolResultKey==__SolResultKey)) ){					
						ePZL.ToPreStage( );
						#if DEBUG
								WriteLine( $"*** Redundant Solution Found in SnapSaveGP...  not unique solution : {__SolResultKey}" );
						#endif
					} 
				
					
					// Save the copy of the solution
					else{		
						UPuzzle aPZLcopy = ePZL.Copy( stageNo_Increments:0, IDm:ePZL.Child_PZLs.Count ); //Copy at the same stage
						aPZLcopy.difficultyLevel = EAMethod.difficultyLevel;
						aPZLcopy.__SolResultKey = __SolResultKey; 
						aPZLcopy.selectedIX = ePZL.Child_PZLs.Count;
						ePZL.Child_PZLs.Add(aPZLcopy);    // Save a copy.
						solutionC++;
						ePZL.ToPreStage( );           // Return the original to the state before analysis.
					}
				// =====================================================================================================



				{ // <<< Termination by number of solutions >>>
					if( ePZL.Child_PZLs.Count>=G6.MSlvr_MaxNoAllAlgorithm ){	// Reached the limit of multiple solutions searches.
						G6.stopped_StatusMessage = pRes.msgUpperLimitBreak; // Reached the limit of multiple solution searches.
						G6.command_Analysis_Stop = true;
						return false;   // ... Finish analysis
					}  

					// <<< Method-specific counter initialization >>>
					var Stage_Method = ( stageNo, EAMethod.MethodName ); // tuple:(stage,name)
					if( stageNo_MethodName!=Stage_Method || stageNoP!=stageNoPMemo ){
						stageNo_MethodName = Stage_Method;
						stageNoPMemo = stageNoP;
						methodCC = 0;       // Initialization of number of solutions per algorithm
					}
					int selExt = (G6.PG6Mode=="Func_SelectPuzzle" && EAMethod.markA_dev)? 50: 1;
					bool ContinueAnalysisB = (++methodCC < (G6.MSlvr_MaxNoAlgorithm*selExt) );
					return ContinueAnalysisB;  
				}



            }
            catch(Exception e){ WriteLine( $"{e.Message}\r{e.StackTrace}"); }
            return false;   // ... interrupt
        }


        public bool Check_TimeLimit(){
			if( G6.PG6Mode.Contains("Func_Develop") )  return false;	// No time limit during development

            if( GNPX_Engin.MltAnsSearch is false )  return false;
            TimeSpan ts =  DateTime.Now - GNPX_App_Man .MultiSolve_StartTime;
            bool timeLimit = ts.TotalSeconds >= G6.MSlvr_MaxTime;
            if(timeLimit)  G6.stopped_StatusMessage = pRes.msgUpperLimitTimeBreak;
            return timeLimit;
        }


        //==========================================================
        public (bool,int,int,int) Aggregate_CellsPZM( List<UCell> aBOARD ){
            int P=0, Z=0, M=0;
            if( aBOARD==null )  return (false,P,Z,M);
            aBOARD.ForEach( q =>{
                if(q.No>0)      P++;
                else if(q.No<0) M++;
                else            Z++;
            } );

            return  (Z==0,P,Z,M);
        }



        public (int,int[]) Execute_Fix_Eliminate( List<UCell> aBOARD ){//Confirmation process
            // return code = 0 : Complete. Go to the next stage.
            //               1 : Solved. 
            //              -1 : Error. Conditions are broken.

            if( aBOARD.All(p=> p.No!=0) )  return (1,null);     //1: Solved. 

            if( aBOARD.Any(p=>p.CancelB>0) ){                         // ..... CancelB .....
                foreach( var P in aBOARD.Where(p=>p.CancelB>0) ){
					P.Reset_FreeB( P.CancelB ); 
					P.CancelB=0;       
                    P.CellBgCr=_Black_;
                }
            }





            if( aBOARD.Any(p=>p.FixedNo>0) ){                         // ..... FixedNo .....
                foreach( var P in aBOARD.Where(p=>p.FixedNo>0) ){
                    int No = P.FixedNo;
                    if( No<1 || No>9 ) continue;
                    P.No=-No; P.FixedNo=0;
					P.Set_FreeB( 0 );	// P.FreeB=0;
                    P.CellBgCr = _Black_;
                }
            }

            {   // ..... Check Error .....
                Update_CellsState( aBOARD, false );
                foreach( var P in aBOARD.Where(p=>(p.No==0 && p.FreeBC==0)) )  P.ErrorState=9; // ..... Error .....

                int[] NChk=new int[27];
                for(int h=0; h<27; h++ ) NChk[h]=0;
                foreach( var P in aBOARD ){
                    int no = (P.No<0)? -P.No: P.No;
                    int B = (no>0)? (1<<(no-1)): P.FreeB;
                    NChk[P.r]|=B; NChk[P.c+9]|=B; NChk[P.b+18]|=B;
                }
                for(int h=0; h<27; h++ ){
                    if(NChk[h]!=0x1FF){ SolCode=-9119; return (-1,NChk); }  // -1: Error. Conditions are broken.
                }
            }
            SolCode=-1;
            return (0,null);    // 0: Complete. Go to next stage.
        }

        public void SetBG_OnError(int h){
            foreach(var P in pBOARD.IEGetCellInHouse(h)) P.Set_CellBKGColor(Colors.Violet);
        }

        public void Update_CellsState( List<UCell> aBOARD, bool setAllCandidates=true ){
			//Set all candidates : Clear all analysis results.
			List<int> To_ListInt(List<UCell> aBOARD) => aBOARD.ConvertAll(P=>P.No);

			UInt128 B81Free = aBOARD.Create_Free_BitExp128();
			UInt128 B81Fixed = B81Free ^ b081_all;

				//Utility_Display.__DBUG_Print2( To_ListInt(aBOARD), sqF:true,"Update_CellsState");

            _Not_Puzzle_ = false;
			foreach( var P in B81Free.IEGet_UCell(aBOARD) ){
				P.Reset_StepInfo();

				{//There are candidates remaining. No two digits will be the same.
					UInt128 Q = pConnectedCells81[P.rc] & B81Fixed;
					int freeB9 = Q.IEGet_rc().Aggregate(0, (p,q)=> p|1<<(Abs(aBOARD[q].No)) );
					freeB9 = (freeB9>>=1) ^ 0x1FF; 
					if( !setAllCandidates ) freeB9 &= P.FreeB; 
					if( freeB9==0 ){ _Not_Puzzle_=true; P.ErrorState=1; }//No solution

					P.Set_FreeB( freeB9 );
				}
			}
        }

        public bool Verify_Puzzle_Roule( ){
            bool    ret=true;
            if( _Not_Puzzle_ ){ SolCode=9; return false; }  // Anomaly has already been detected

			UInt128 B81Free = pBOARD.Create_Free_BitExp128();
			UInt128 B81Fixed = B81Free ^ b081_all;

            for(int h=0; h<27; h++){
                int usedB=0, errB=0;
				foreach( var P in (B81Fixed&pHouseCells81[h]).IEGet_UCell(pBOARD) ){
                    int no = Abs(P.No);
                    if(( usedB&(1<<no)) !=0 )   errB |= (1<<no); // have the same digit.
                    usedB |= (1<<no);
				}

                if(errB!=0){
                    foreach( var P in (B81Fixed&pHouseCells81[h]).IEGet_UCell(pBOARD) ){
                        int no = Abs(P.No);
                        if( (errB&(1<<no)) !=0 ){ P.ErrorState=8; ret=false; }  //mark cells with the same digit
                    }
                }
            }
/*
           for(int h=0; h<27; h++){
                int usedB=0, errB=0;
                foreach(var P in pBOARD.IEGetCellInHouse(h).Where(Q=> Q.No!=0)){
                    int no = Abs(P.No);
                    if(( usedB&(1<<no)) !=0 )   errB |= (1<<no); // have the same digit.
                    usedB |= (1<<no);
                }

                if(errB!=0){
                    foreach(var P in pBOARD.IEGetCellInHouse(h).Where(Q=> Q.No!=0)){
                        int no = Abs(P.No);
                        if( (errB&(1<<no)) !=0 ){ P.ErrorState=8; ret=false; }  //mark cells with the same digit
                    }
                }
            }
*/
			//if( ret ){ ePZL.Sol_Result = "..."; ePZL.Sol_ResultLong = "@@@"; }
            return ret;
        }

        public void ResetAnalysisResult( bool clear0 ){
            if(clear0){   // true:Initial State
              //foreach(var P in pBOARD.Where(Q=>Q.No<=0)){ P.Reset_StepInfo(); P.FreeB=0x1FF; P.No=0; }
			    foreach(var P in pBOARD.Where(Q=>Q.No<=0)){ P.Reset_StepInfo(); P.Set_FreeB(0x1FF); P.No=0; }
            }
            else{
              //foreach(var P in pBOARD.Where(Q=>Q.No==0)){ P.Reset_StepInfo(); P.FreeB=0x1FF; }
				foreach(var P in pBOARD.Where(Q=>Q.No==0)){ P.Reset_StepInfo(); P.Set_FreeB(0x1FF); }
            }
            Update_CellsState( aBOARD:pBOARD );
			ePZL.extResult = "";
        }
    }
}