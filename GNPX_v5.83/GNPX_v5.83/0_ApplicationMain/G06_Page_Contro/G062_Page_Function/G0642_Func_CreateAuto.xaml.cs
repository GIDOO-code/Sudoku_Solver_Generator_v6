using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;
using System.IO;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Diagnostics;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GNPX_space{
    using pRes   = Properties.Resources;
    using sysWin = System.Windows;
	using pApEnv = App_Environment;	

	using pGPGC = GNPX_Puzzle_Global_Control;

	using GIDOO_space;
    public partial class Func_CreateAuto: Page{
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 

		static public GNPX_App_Man 	pGNPX_App;	
		private GNPX_win			pGNP00win{ get=> pGNPX_App.pGNP00win; } 

		public GNPX_App_Ctrl        App_Ctrl => pGNPX_App.App_Ctrl;
        private GNPX_Engin			pGNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine
		private G6_Base				G6 => GNPX_App_Man.G6;

		private GNPX_Graphics		gnpxGrp => pGNPX_App.gnpxGrp;

		public UPuzzle				ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; } // Puzzle to analyze

//		private bool				InPreparation=true;
		private List<RadioButton>   PatSel_RBList = null;

        private RenderTargetBitmap  bmpPD = new RenderTargetBitmap(176,176, 96,96, PixelFormats.Default);//176=18*9+2*4+1*6        
		private CancellationTokenSource cts;

		private MethodDisplayWin	MthdWin;
        private Stopwatch		    AnalyzerLap;
		private DispatcherTimer     statusDisplayTimer;
        private DispatcherTimer		timerShortMessage;

		private bool				FirstB = true;

        public Func_CreateAuto( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;
            InitializeComponent();

			lbl_ShortMessage.Visibility = Visibility.Hidden;

			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );

			UC_SBoard.pGNPX_App = pGNPX_App;

		  #region Timer
			AnalyzerLap = new Stopwatch();
            statusDisplayTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
            statusDisplayTimer.Interval = TimeSpan.FromMilliseconds(100);//50
            statusDisplayTimer.Tick += new EventHandler(statusDisplayTimer_Tick);

			timerShortMessage = new DispatcherTimer(DispatcherPriority.Normal);
			timerShortMessage.Interval = TimeSpan.FromMilliseconds(50);
			timerShortMessage.Tick += new EventHandler(timerShortMessage_Tick);
		  #endregion

			G6.TLoopCC = 0;	//::::::::::::::::::::::::::::: Currently checking operation

//@@			btn_PatternClear_Click( this, null );	// Initial pattern settings
			txb_EpapsedTimeTS3.Text = "0";
        }
				

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;

			//　Pattern Clear(Run once at first)
			if( FirstB ){ FirstB=false; UC_SBoard.btn_PatternClear_Click( this, new RoutedEventArgs() ); }

		  #region PatSel
			if( PatSel_RBList == null ){
					PatSel_RBList = (List<RadioButton>)Extension_Utility.GetControlsCollection<RadioButton>(this); // Create list of "Pattern Buttons"
					PatSel_RBList = PatSel_RBList.FindAll( P=> P.Name.Contains("rdb_patSel") );
			}

			string patN = $"{G6.PatSel0X:00}";
			RadioButton rdb = PatSel_RBList.Find( P=> P.Name.Replace("rdb_patSel","")==patN);
			if( rdb != null )  rdb.IsChecked = true;
		  #endregion

			nud_Puzzle_LevelLow.Value  = G6.Puzzle_LevelLow;
			nud_Puzzle_LevelHigh.Value = G6.Puzzle_LevelHigh;

			elp_OnWork.Visibility = Visibility.Collapsed;			

			string culture = App_Environment.culture;
					//WriteLine( $"Page_IsVisibleChanged => culture is {culture}");

			Lbl_LS_P0cc.Content = "0";
			Lbl_LS_P2cc.Content = "0";

			txb_LS_Pattern_Cntrl_P0.Text = G6.LS_Pattern_Cntrl_P0.ToString();
			txb_LS_Pattern_Cntrl_P2.Text = G6.LS_Pattern_Cntrl_P2.ToString();

			chbx_RandomizingDigits.IsChecked = G6.Digits_Randomize;
			MthdWin_Control();

			int diff = pGNPX_App.Get_Difficulty_SolverList_App();
		}



		private void Page_Unloaded(object sender, RoutedEventArgs e ){
			MthdWin.Close();
			G6.Puzzle_LevelLow  = nud_Puzzle_LevelLow.Value;
			G6.Puzzle_LevelHigh = nud_Puzzle_LevelHigh.Value;

			G6.Save_CreatedPuzzle = (bool)chbx_FileOutputOnSuccess.IsChecked;
			G6.Digits_Randomize = (bool)chbx_RandomizingDigits.IsChecked;
;		}

		private void btn_ShowMethodList_Click(object sender, RoutedEventArgs e) {
			if( MthdWin==null )  MthdWin_Control();
			MthdWin.Visibility = (MthdWin.Visibility==Visibility.Hidden)? Visibility.Visible: Visibility.Collapsed;
        }

		private void MthdWin_Control(){
            MthdWin = new MethodDisplayWin(pGNP00win);
            MthdWin.Visibility = Visibility.Visible;
			MthdWin.Left  = pGNP00win.Left+pGNP00win.Width;
			MthdWin.Top   = pGNP00win.Top;
			MthdWin.Height = pGNP00win.Height;
            MthdWin.Show();
		}

		private void chb_ResultShow_Checked(object sender, RoutedEventArgs e) {
			G6.ResultShow = (bool)chb_ResultShow.IsChecked;
		}

		private void txb_LS_Pattern_Cntrl_P0_TextChanged(object sender, TextChangedEventArgs e) {
			string st = txb_LS_Pattern_Cntrl_P0.Text;
			int val = -1;
			try{ val  = int.Parse( st ); }
			catch( Exception ex ){
				val = 0;
				txb_LS_Pattern_Cntrl_P0.TextChanged -= new TextChangedEventHandler( txb_LS_Pattern_Cntrl_P0_TextChanged );
				txb_LS_Pattern_Cntrl_P0.Text = "0";
				txb_LS_Pattern_Cntrl_P0.TextChanged += new TextChangedEventHandler( txb_LS_Pattern_Cntrl_P0_TextChanged );		
			}
			G6.LS_Pattern_Cntrl_P0 = val;
		}
/*
		private void txb_LS_Pattern_Cntrl_P1_TextChanged(object sender, TextChangedEventArgs e) {
			string st = txb_LS_Pattern_Cntrl_P1.Text;
			int val = -1;
			try{ val  = int.Parse( st ); }
			catch( Exception ex ){
				val = 0;
				txb_LS_Pattern_Cntrl_P1.TextChanged -= new TextChangedEventHandler( txb_LS_Pattern_Cntrl_P1_TextChanged );
				txb_LS_Pattern_Cntrl_P1.Text = "0";
				txb_LS_Pattern_Cntrl_P1.TextChanged += new TextChangedEventHandler( txb_LS_Pattern_Cntrl_P1_TextChanged );		
			}
			G6.LS_Pattern_Cntrl_P1 = val;
		}
*/
		private void txb_LS_Pattern_Cntrl_P2_TextChanged(object sender, TextChangedEventArgs e) {
			string st = txb_LS_Pattern_Cntrl_P2.Text;
			int val = -1;
			try{ 
				val  = int.Parse( st ); 
				if( val<0 || val>=3136 )  val = 3036;
			}
			catch( Exception ex ){
				val = 0;
				txb_LS_Pattern_Cntrl_P2.TextChanged -= new TextChangedEventHandler( txb_LS_Pattern_Cntrl_P2_TextChanged );
				txb_LS_Pattern_Cntrl_P2.Text = "0";
				txb_LS_Pattern_Cntrl_P2.TextChanged += new TextChangedEventHandler( txb_LS_Pattern_Cntrl_P2_TextChanged );		
			}
			G6.LS_Pattern_Cntrl_P2 = val;
		}


	#region Timer (shortMessage,statusDisplay)
		// <<< ShortMessage >>>
		private void timerShortMessage_Tick( object sender, EventArgs e ){
            lbl_ShortMessage.Visibility = Visibility.Hidden;
            timerShortMessage.Stop();
        }	
		private void shortMessage( string st, sysWin.Point pt, Color cr, int tm ){
			if( tm==9999 ) timerShortMessage.Interval = TimeSpan.FromSeconds(5);
			else           timerShortMessage.Interval = TimeSpan.FromMilliseconds(tm);   

			this.Dispatcher.Invoke( ()=>{
				lbl_ShortMessage.Content = st;
				lbl_ShortMessage.Foreground = new SolidColorBrush(Colors.White);
				lbl_ShortMessage.Background = new SolidColorBrush(cr);
				lbl_ShortMessage.Margin = new Thickness(pt.X,pt.Y,0,0);
				lbl_ShortMessage.Visibility = Visibility.Visible;
			} );
			
			Thread.Sleep(tm);
			timerShortMessage.Start();
        }

		private void statusDisplayTimer_Tick( object sender, EventArgs e ){
			txb_EpapsedTimeTS3.Text = AnalyzerLap.Elapsed.TimespanToString();

			Lbl_LS_P0cc.Content = G6.LS_P_P0;
			Lbl_LS_P2cc.Content = G6.LS_P_P2;

			string stLS =  $"{G6.Info_CreateMessage0}\n {G6.Info_CreateMessage1}\n {G6.Info_CreateMessage2}\n {G6.Info_CreateMessage3}";
			lbl_LS_Parallel.Content = stLS;

            lbl_ResourceMemory.Content = "Memory : " + GC.GetTotalMemory(true).ToString("N0");   
//			lblSolutionLevel.Content = $"Solvable Probability: {solLvl} / {81-nP}";
//			lbl_LSCandidate.Content = G6.Info_LS_Generator;

			int lapMSec = ((int)AnalyzerLap.ElapsedMilliseconds) /50;

			elp_OnWork.Visibility = ((lapMSec%10)<3)? Visibility.Visible: Visibility.Hidden;

			if( GNPX_Engin.AnalyzingMethod!=null ) Lbl_onAnalyzingMethod.Content = "Running Method : "+GNPX_Engin.AnalyzingMethod.MethodName;
		}
	#endregion Timer (shortMessage,statusDisplay)



	#region Event Processing
		private void Send_SetPuzzleOnBoard(){
			Gidoo_EventHandler se = new Gidoo_EventHandler( eName:"Show_AnalysisStatus" );  //Report
			Send_Command_to_GNPXwin( this, se );
		}

		// <<< Event Receiver >>>
		public void GNPX_Event_Handling_man( object sender, Gidoo_EventHandler e ){ 
			
			switch( e.eName ){

				case "PuzzleCreated":
					Dispatcher.Invoke( () => txb_NoOfPuzzles.Text = e.ePara1.ToString() );
					shortMessage( $" {e.ePara1} / {e.ePara0} found  ", new sysWin.Point(320,5), Colors.Blue, tm:500 );
					// Updates and displays of the main board (UC_PB_GBoard) are done by the main window.
					break;

				case "ChangedPattern":
					shortMessage( $"Changed Pattern.", new sysWin.Point(30,170), Colors.DarkBlue, 500 );
					this.Dispatcher.Invoke(() => UC_SBoard.SetBitmap_PB_pattern=true );
					break;

				case "ChangedFrame12347":
					this.Dispatcher.Invoke(() => UC_SBoard.Reflesh_PB_BasePatDig_Prop=true );
					break;


				case "ShortMessage": shortMessage( e.gsm.Message, e.gsm.Pt, e.gsm.color, e.gsm.msec ); break;
				case "SystemError":  shortMessage( e.Message, new sysWin.Point(400,180), Colors.Orange, 3000 );	break;
				default:			 shortMessage( "System error. Undefined event.", new sysWin.Point(100,200), Colors.DarkRed, 10000 ); break;
											// ( In other thread control, "Dispatcher.Invoke" is need! )
			}
		}
	#endregion Event Processing



	#region Conditions for Puzzle Creation
		private void nud_Puzzle_LevelLow_ValueChanged(object sender, GIDOOEventArgs args) {
			if( nud_Puzzle_LevelLow==null )  return;
			int Lval=(int)nud_Puzzle_LevelLow.Value, Uval=(int)nud_Puzzle_LevelHigh.Value;
			if( Lval>Uval ) nud_Puzzle_LevelHigh.Value = Lval;
		}
		private void nud_Puzzle_LevelHigh_ValueChanged(object sender, GIDOOEventArgs args) {
			if( nud_Puzzle_LevelHigh==null )  return;
			int Lval = (int)nud_Puzzle_LevelLow.Value, Uval=(int)nud_Puzzle_LevelHigh.Value;
			if( Uval < Lval ) nud_Puzzle_LevelLow.Value = Uval;
		}
		
		private void chbx_RandomizingDigits_Checked(object sender, RoutedEventArgs e) {
			G6.Digits_Randomize = (bool)chbx_RandomizingDigits.IsChecked;
			// pGPGC.Randumize_All( );
		}
	#endregion Conditions for Puzzle Creation


		
	#region Puzzle Creation

		private async void btn_CreatePuzzleAuto_Click( object sender, RoutedEventArgs e ){
			Button btn = (Button)sender;

            int nn = App_Ctrl.PatGen.Count();

				// If the hint number for the Puzzle is less than 17, it is not a valid Sudoku Puzzle.
				if( nn<17 ){
					shortMessage( "Not enough patterns.(n<17)", new sysWin.Point(5,230), Colors.Orange, 3000 );    
							//this.Dispatcher.Invoke(() => UC_SBoard.SetBitmap_PB_pattern=true );
					if( cts != null )  cts.Cancel();
					return;		//<<< process end >>>
				}


				// <<< suspend >>>
				if( (string)btn_CreatePuzzleAuto.Content != pRes.btn_CreatePuzzleAuto ){ //Suspend
					cts.Cancel();
					_LPostProcessing(); //
					return;
				}
				// ============================================================================================


			// <<< Prepare >>>
			G6.taskCompInfo =　null;
			G6.LoopCC		= 0;					
			G6.PatternCC	= 0;

			G6.SolutionCounter    = 0;
			G6.NoOfPuzzlesToCreate = int.Parse(No_PuzzlesToCreate.Text);
			G6.Search_AllSolutions = false;
			G6.Digits_Randomize   = (bool)chbx_RandomizingDigits.IsChecked;
			G6.Puzzle_LevelLow    = (int)nud_Puzzle_LevelLow.Value;
			G6.Puzzle_LevelHigh   = (int)nud_Puzzle_LevelHigh.Value;

			btn_CreatePuzzleAuto.Content = pRes.msgSuspend;
			txb_NoOfPuzzles.Text   = "0";

			GNPX_Engin.SolInfoB        = false;
			GNPX_Engin.AnalyzingMethod = null;		// (Hide the running algorithm display.)
			AnalyzerBaseV2.__SimpleAnalyzerB__ = true;



			//==============================================================================================
			AnalyzerLap.Restart();
			statusDisplayTimer.Start();

			// <<<Generate puzzles in a separate thread >>>	
			cts = new CancellationTokenSource();			
			await Task.Run( ()=> App_Ctrl.task_GNPX_PuzzleCreaterAuto_1( this, cts ) );
			//==============================================================================================



			//cts.TryReset();

							// --- task end ---  @@pattern
							if( cts.IsCancellationRequested ){
								G6.taskCompInfo="Canceled";

								AnalyzerBaseV2.__SimpleAnalyzerB__=false;
								btn_CreatePuzzleAuto.Content = pRes.btn_CreatePuzzleAuto;
								shortMessage( "cancellation accepted", new sysWin.Point(40,5), Colors.DarkOrange, 1000); 
							}
							else G6.taskCompInfo="Complated"; 
							//----------------------------------------------------------------------------------------------

							if( G6.SolutionCounter>=1 ){
								string stCreated = $"Create {G6.SolutionCounter} Puzzles";
								shortMessage( stCreated, new sysWin.Point(90,5), Colors.Crimson, 8000 );  
							}
							else if( G6.LS_P_P2 >= G6.LS_Pattern_Cntrl_P2 ){
								shortMessage( "Pattern change", new sysWin.Point(90,5), Colors.Crimson, 1000 );    
							}


			// <<< Post Processing >>>
			_LPostProcessing();

            return;

					void _LPostProcessing(){
						pGPGC.current_Puzzle_No = int.MaxValue;  // Set to last Puzzle in list.			
						Send_SetPuzzleOnBoard();
						//await Task.Delay(100);
			
						AnalyzerLap.Stop();
						statusDisplayTimer.Stop();
						btn_CreatePuzzleAuto.Content = pRes.btn_CreatePuzzleAuto;
						elp_OnWork.Visibility = Visibility.Hidden;
					}
        }


		#endregion Running P generation

		private void btn_EmergencySave_Click(object sender, RoutedEventArgs e) {
			var eLS = ePZL.BOARD.ConvertAll( q => Max(q.No,0)+((q.c==8)? " ":"") );
            string line = string.Join("", eLS).Replace("0",".");
			line += DateTime.Now.ToString("yyyyMMdd_hhMM");

			string dirError = "Algorithm_Error";
			if( !Directory.Exists(dirError) )  Directory.CreateDirectory(dirError);

			string fNameEmergency = dirError+"/"+"Emergency.txt";
			Utility_Display.GNPX_StreamWriter( fNameEmergency, line, append:true ); 

			shortMessage( "_emargency save done ", new sysWin.Point(40,5), Colors.DarkOrange, 2000); 
        }
    }
}
