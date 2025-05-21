using GIDOO_space;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    public partial class Func_CreateAll: Page{
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
		private CancellationTokenSource cts = new();

		private string				stJP,stEN;

		private MethodDisplayWin	MthdWin;
        private Stopwatch		    AnalyzerLap;
		private DispatcherTimer     statusDisplayTimer;
        private DispatcherTimer		timerShortMessage;

		private List<string>		partialSearchList = new List<string>(){"1%", "2%", "5%", "10%", "20%", "50%", "100%" };
		private bool				FirstB = true;

        public Func_CreateAll( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;
            InitializeComponent();

			lbl_ShortMessage.Visibility = Visibility.Hidden;

			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );

			UC_SBoard.pGNPX_App = pGNPX_App;

			AnalyzerLap = new Stopwatch();
            statusDisplayTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
            statusDisplayTimer.Interval = TimeSpan.FromMilliseconds(50);//50
            statusDisplayTimer.Tick += new EventHandler(statusDisplayTimer_Tick);

			timerShortMessage = new DispatcherTimer(DispatcherPriority.Normal);
			timerShortMessage.Interval = TimeSpan.FromMilliseconds(50);
			timerShortMessage.Tick += new EventHandler(timerShortMessage_Tick);

			G6.TLoopCC = 0;	//::::::::::::::::::::::::::::: Currently checking operation

			txb_NoOfPuzzles.Text = "0";

			comb_partialsearch.ItemsSource = partialSearchList;
			comb_partialsearch.SelectedIndex = 1;

			string endl = "\r";
            stJP =  "パターンの全解生成" + endl;
			stJP += "・問題パターンの全パズルを生成する。" + endl;
			stJP += "・解析アルゴリズムとは独立。" + endl;
			stJP += "・乱数化は本質ではなく、行わない。" + endl;
			stJP += "・必要資源(時間,CPU)はパターンよる。" + endl;
			stJP += "・生成は数10分-数時間を要する。" + endl;
			stJP += "・中断はできない。" + endl;
			stJP += "・最大10000パズルはシステム保存。" + endl;
			stJP += "・全パズルはファイル保存。" + endl;
            
			stEN =  "Create complete solution" + endl;
			stEN += "  Complete solution of patterns." + endl;
			stEN += "  Independent of analysis algorithm." + endl;
			stEN += "  Randomization is not essential." + endl;
			stEN += "  Time,CPU vary depend on the pattern." + endl;
			stEN += "  Creation takes several minitus/hours." + endl;
			stEN += "  Cannot be interrupted." + endl;
			stJP += "  -10,000 puzzles be stored in the system." + endl;
			stJP += "  All puzzles are saved in files." + endl;
        }
				

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;

			if( FirstB ){ FirstB=false; UC_SBoard.btn_PatternClear_Click( this, new RoutedEventArgs() ); }

			if( PatSel_RBList == null ){
				PatSel_RBList = (List<RadioButton>)Extension_Utility.GetControlsCollection<RadioButton>(this);
				PatSel_RBList = PatSel_RBList.FindAll( P=> P.Name.Contains("rdb_patSel") );
			}

			string patN = $"{G6.PatSel0X:00}";
			RadioButton rdb = PatSel_RBList.Find( P=> P.Name.Replace("rdb_patSel","")==patN);
			if( rdb != null )  rdb.IsChecked = true;

			string culture = App_Environment.culture;
            WriteLine( $"Page_IsVisibleChanged => culture is {culture}");
            string urlHP="";
            txbk_CreateAllPuzzle.Text = (culture=="ja-JP")? stJP: stEN;

		}



		private void Page_Unloaded(object sender, RoutedEventArgs e ){
			//MthdWin.Close();
		}
		private void btn_ShowMethodList_Click(object sender, RoutedEventArgs e) {
			MthdWin.Visibility = (MthdWin.Visibility==Visibility.Hidden)? Visibility.Visible: Visibility.Collapsed;
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

            lbl_ResourceMemory.Content = "Memory : " + GC.GetTotalMemory(true).ToString("N0");   
//			lblSolutionLevel.Content = $"Solvable Probability: {solLvl} / {81-nP}";
//			lbl_LSCandidate.Content = G6.Info_LS_Generator;
			lbl_CreateAll_Message2.Content = G6.Info_CreateMessage2;
			lbl_CreateAll_Message3.Content = G6.Info_CreateMessage3;

			int lapMSec = ((int)AnalyzerLap.ElapsedMilliseconds) /50;
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
					if( G6.Search_AllSolutions ) shortMessage( $"{e.ePara1} found  ", new sysWin.Point(90,5), Colors.Blue, tm:500 );
					else shortMessage( $"{e.ePara1} / {e.ePara0} found  ", new sysWin.Point(90,5), Colors.Blue, tm:500 );
					// Updates and displays of the main board (UC_PB_GBoard) are done by the main window.
					break;

				case "ChangedPattern":
					shortMessage( $"Changed Pattern.", new sysWin.Point(300,140), Colors.DarkBlue, 500 );
					this.Dispatcher.Invoke(() => UC_SBoard.SetBitmap_PB_pattern=true );
					break;

				case "ChangedFrame12347":
					this.Dispatcher.Invoke(() => UC_SBoard.Reflesh_PB_BasePatDig );
					break;


				case "ShortMessage": shortMessage( e.gsm.Message, e.gsm.Pt, e.gsm.color, e.gsm.msec ); break;
				case "SystemError":  shortMessage( e.Message, new sysWin.Point(400,180), Colors.Orange, 3000 );	break;
				default:			 shortMessage( "System error. Undefined event.", new sysWin.Point(100,200), Colors.DarkRed, 10000 ); break;
											// ( In other thread control, "Dispatcher.Invoke" is need! )
			}


			
		}
	#endregion Event Processing

	#region Create All Puzzle
		private async void btn_CreateAllPuzzle_Click(object sender, RoutedEventArgs e) {
            int nn = App_Ctrl.PatGen.Count();

			// If the hint number for the Puzzle is less than 17, it is not a valid Sudoku Puzzle.
			if( nn<17 ){
				shortMessage( "Not enough patterns.(n<17)", new sysWin.Point(250,200), Colors.Crimson, 1500 );      
				return;		//<<< process end >>>
			}


			// <<< suspend >>>
			if( (string)btn_CreateAllPuzzle.Content != pRes.btn_CreatePuzzleAuto ){ //Suspend
				cts.Cancel();
				goto LPostProcessing; //
			}



		// ============================================================================================
			// <<< Prepare >>>
				G6.taskCompInfo =　null;
				G6.LoopCC		= 0;					
				G6.PatternCC	= 0;

				G6.Search_AllSolutions = true;

				btn_CreateAllPuzzle.Content = pRes.msgSuspend;
				txb_NoOfPuzzles.Text   = "0";

				GNPX_Engin.SolInfoB        = false;
				GNPX_Engin.AnalyzingMethod = null;		// (Hide the running algorithm display.)
				AnalyzerBaseV2.__SimpleAnalyzerB__ = true;
				//==============================================================================================
				AnalyzerLap.Restart();
				statusDisplayTimer.Start();

				string stPaSe = (string)comb_partialsearch.SelectedItem;
				double partialSearch = double.Parse(stPaSe.Replace("%",""))/100.0;



			// <<<Generate puzzles in a separate thread >>>	
				cts = new CancellationTokenSource();			
				await Task.Run( ()=> App_Ctrl.task_GNPX_Creator_AllPuzzles_1( this, cts, partialSearch ) );

				//cts.TryReset();

			// --- task end ---
			if( cts.IsCancellationRequested ){
				G6.taskCompInfo="Canceled";

				AnalyzerBaseV2.__SimpleAnalyzerB__=false;
				btn_CreateAllPuzzle.Content = pRes.btn_CreatePuzzleAuto;
				shortMessage( "cancellation accepted", new sysWin.Point(40,5), Colors.DarkOrange, 1000); 
			}
			else G6.taskCompInfo="Complated"; 
			//----------------------------------------------------------------------------------------------

			if( G6.LS_P_P2 == 0 ){
				shortMessage( "no Puzzle in this pattern", new sysWin.Point(90,5), Colors.Crimson, 3000 );    
			}

			// <<< Post Processing >>>
			LPostProcessing:
				//cts.Dispose();
				pGPGC.current_Puzzle_No = int.MaxValue;  // Set to last Puzzle in list.			
				Send_SetPuzzleOnBoard();
				await Task.Delay(100);
			
				AnalyzerLap.Stop();
				statusDisplayTimer.Stop();
				btn_CreateAllPuzzle.Content = pRes.btn_CreatePuzzleAuto;
            return;
		}


		#endregion Create All Puzzle

		private void btn_RandomizeAllPuzzle_Click(object sender, RoutedEventArgs e) {
			pGNPX_App.GNPX_RandomizeAllPuzzle();
		}


	}
}
