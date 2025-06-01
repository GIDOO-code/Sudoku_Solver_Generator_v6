using System;
using System.Linq;
using System.Collections.Generic;
using static System.Diagnostics.Debug;
using static System.Math;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Threading;

using GIDOO_space;
using System.Windows.Controls;

namespace GNPX_space{

    // ...  This class will change in the future.  ............
    //     Organize items into the following classes. ---> GNPX_AnalyzerMan, GNPX_App_Ctrl, GNPX_Engin, ...

    public partial class GNPX_App_Ctrl{    
        static public event GNPX_EventHandler Send_Command_to_GNPXwin; 
		static public event GNPX_EventHandler Send_Command_to_FCreateAuto; 

        static public Random	GNPX_Random;
        
		public GNPX_Engin		pGNPX_Eng;
		public GNPX_AnalyzerMan AnMan => pGNPX_Eng.AnMan; 

        private GNPX_App_Man	pGNPX_App;
        private GNPX_win		pGNP00win{ get=> pGNPX_App.pGNP00win; } 

        public PatternGenerator PatGen;         //Puzzle Pattern
												 
        public GNPX_App_Ctrl( GNPX_App_Man  pGNPX_App, int FirstCellNum ){
            this.pGNPX_App = pGNPX_App;
			pGNPX_Eng	= pGNPX_App.GNPX_Eng;
            Send_Command_to_GNPXwin     += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  

			Send_Command_to_FCreateAuto += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  
            
            G6.CellNumMax = FirstCellNum; 

            PatGen = new PatternGenerator( this );
            LSGen    = new LatinSquareGen( );
        }






        public Random Set_RandomSeed( int rs ){
			int rsVal = rs;     
            if(rs==0){
              //rsVal=Environment.TickCount&Int32.MaxValue;
                rsVal = (int)DateTime.Now.Ticks;
                WriteLine($"DateTime.Now.Ticks:{DateTime.Now.Ticks}  randomSeedVal:{rsVal}");
            }
			//G6.RandomSeedVal = Abs(rsVal);	// Debug with fixed conditions.

            GNPX_Random = new Random(rsVal);
				WriteLine( $"SetRandomSeed : {rsVal}" );
			return GNPX_Random;
        }


    #region Analizer
        public void task_Analyzer_SingleStage( CancellationToken cts, bool methodSet=true ){      //Analysis[step]
            try{
                G6.retNZ = -1;
                pGNPX_Eng.GNPX_Solver_SingleStage( cts:cts, SolInfoB:true );
				G6.EnginState = cts.IsCancellationRequested? 3: 2; 
            }
            catch(TaskCanceledException){ WriteLine("...Canceled by user."); }
            catch(Exception ex){ WriteLine( $"{ex.Message}\n{ex.StackTrace}"); }   
        }

/*
        public string task_Analyzer_SolveUp( CancellationToken cts ){
			string stRet = "";
            try{
                bool chbx_ConfirmMultipleCells = GNPX_App_Man .chbx_ConfirmMultipleCells;
                stRet = pGNPX_Eng.GNPX_Solver_SolveUp(cts);
            }
            catch(TaskCanceledException){ WriteLine("...Canceled by user."); stRet="canceled"; }
            catch(Exception ex){  stRet="exception"; WriteLine( $"{ex.Message}\n{ex.StackTrace}");}   
			
			return stRet;
        }
*/
    #endregion Analizer

    }
}
    