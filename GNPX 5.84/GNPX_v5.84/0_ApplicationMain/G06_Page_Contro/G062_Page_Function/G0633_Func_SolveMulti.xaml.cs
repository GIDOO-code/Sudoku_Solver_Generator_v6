using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Diagnostics.Debug;
using System.Diagnostics;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace GNPX_space{
	using pRes = Properties.Resources;
	using sysWin = System.Windows;
	using ioPath = System.IO.Path;
	
	
	public struct POINT{ public int x; public int y; }


    public partial class Func_SolveMulti: Page{
		[DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]		// ... Required for SetCursorPos
		static public extern void SetCursorPos(int X,int Y);

		[DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]		// ... Required for GetCursorPos
		static public extern void GetCursorPos(out POINT lpPoint);
				
					
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 
		static public event GNPX_EventHandler Send_Command_to_SolveMulti;


		static public bool			eventFire_Stop=false;

		public GNPX_App_Man 		pGNPX_App;
		public GNPX_win				pGNP00win => pGNPX_App.pGNP00win;



		public GNPX_App_Ctrl        App_Ctrl => pGNPX_App.App_Ctrl;
		private G6_Base				G6 => GNPX_App_Man.G6;
        private GNPX_Engin			pGNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine

		public UPuzzle				ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; } // Puzzle to analyze

		 private int                stageNo   => (ePZL is null)? 0: ePZL.stageNo;

		private Func_Option			objFunc_Option => (Func_Option)pGNPX_App.objFunc_Option;
		
		private CancellationTokenSource  cts;		
		private MethodDisplayWin	MthdWin;
		private Stopwatch			AnalyzerLap = new();
        
		private DispatcherTimer     displayTimer;	
		private object				obj = new();
		private bool				displayTimer_Stop;
		private int                 OnWorkCC = 0;

        private DispatcherTimer		timerShortMessage;



		#region <<< Constracter, Load, Unload >>>
        public Func_SolveMulti( GNPX_App_Man GNPX_App){
			eventFire_Stop = true;
			InitializeComponent();

			lbl_ShortMessage.Visibility = Visibility.Hidden;
			Lbl_onAnalyzingM.Content = "";

			pGNPX_App = GNPX_App;	
			cts = new CancellationTokenSource();　//for Cancellation

			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );
			pGNP00win.Send_Command_to_Func_SolveMulti += new GNPX_EventHandler( GNPX_Event_Handling_man );

			displayTimer = new DispatcherTimer(DispatcherPriority.Normal);
			displayTimer.Interval = TimeSpan.FromMilliseconds(50);
			displayTimer.Tick += new EventHandler(Show_RunningMethodName_Tick);

			timerShortMessage = new DispatcherTimer(DispatcherPriority.Normal);
			timerShortMessage.Interval = TimeSpan.FromMilliseconds(50);
			timerShortMessage.Tick += new EventHandler(timerShortMessage_Tick);
        }

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;
			G6.sNoAssist = true;

			elp_OnWorkMlt.Visibility      = Visibility.Hidden;
            MSlvr_MaxLevel.Value          = G6.MSlvr_MaxLevel;
			MSlvr_MaxNoAlgorithm.Value    = G6.MSlvr_MaxNoAlgorithm;
			MSlvr_MaxNoAllAlgorithm.Value = G6.MSlvr_MaxNoAllAlgorithm;
			MSlvr_MaxTime.Value			  = G6.MSlvr_MaxTime;
            MSlvr_MaxLevel.ToolTip = $"max:{G6.MSlvr_MaxLevel}";

#if DEBUG
			MthdWin_Control( winDisp:true );
#else
			MthdWin_Control( winDisp:false );
#endif
			if( stageNo == 0 ){
				txbxAnalyzerResultM.Text = "";
				txbxAnalyzerResultM.Foreground = Brushes.White;
			}
			eventFire_Stop = false;
			
			Send_SetPuzzleOnBoard();		// Display the puzzle in standard conditions
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e) {
			MthdWin.Visibility = Visibility.Collapsed;//	Close();
		}
		#endregion <<< Constracter, Load, Unload >>>


		public void GNPX_Event_Handling_man( object sender, Gidoo_EventHandler e ){ 
			switch( e.eName ){
				case "ClearAll":
				case "ToInitialState":
					Dispatcher.Invoke( () => Puzzle_ResetAnalyzer() );
					break;

				case "ShortMessage": shortMessage( e.gsm.Message, e.gsm.Pt, e.gsm.color, e.gsm.msec ); break;
				case "SystemError":  shortMessage( e.Message, new sysWin.Point(400,180), Colors.Orange, 3000 );	break;
				default:			 shortMessage( "System error. Undefined event.", new sysWin.Point(100,200), Colors.DarkRed, 10000 ); break;
											// ( In other thread control, "Dispatcher.Invoke" is need! )
			}
		}



		#region <<< ShortMessage >>>
	    private void timerShortMessage_Tick( object sender, EventArgs e ){
            lbl_ShortMessage.Visibility = Visibility.Hidden;
            timerShortMessage.Stop();
        }
		public void shortMessage( string st, sysWin.Point pt, Color cr, int tm ){
			if( tm==9999 ) timerShortMessage.Interval = TimeSpan.FromSeconds(5);
			else           timerShortMessage.Interval = TimeSpan.FromMilliseconds(tm);   

			this.Dispatcher.Invoke( ()=>{
				lbl_ShortMessage.Content = st;
				lbl_ShortMessage.Foreground = new SolidColorBrush(Colors.White);
				lbl_ShortMessage.Background = new SolidColorBrush(cr);
				lbl_ShortMessage.Margin = new Thickness(pt.X,pt.Y,0,0);
				lbl_ShortMessage.Visibility = Visibility.Visible;
			} );
			
			timerShortMessage.Start();
        }
		#endregion <<< ShortMessage >>>


		#region Event
 		private void Send_SetPuzzleOnBoard(){
			Gidoo_EventHandler se = new Gidoo_EventHandler( eName:"Show_AnalysisStatus" );  //Report
			Send_Command_to_GNPXwin( this, se );
		}
		#endregion


		#region Show analysis information
		private void btn_ShowMethodList_Click(object sender, RoutedEventArgs e) {
			if( MthdWin==null )  MthdWin_Control();
			MthdWin.Visibility = (MthdWin.Visibility==Visibility.Hidden)? Visibility.Visible: Visibility.Hidden;
        }
		private void MthdWin_Control( bool winDisp=true ){
            MthdWin = new MethodDisplayWin(pGNP00win);
            MthdWin.Visibility = Visibility.Visible;
			MthdWin.Left  = pGNP00win.Left+pGNP00win.Width;
			MthdWin.Top   = pGNP00win.Top;
			MthdWin.Height = pGNP00win.Height;
            if( winDisp ) MthdWin.Show();
		 }

		 private void Show_RunningMethodName_Tick( object sender, EventArgs e ){
			lock(obj){
				var MTHD = GNPX_Engin.AnalyzingMethod;
				if( MTHD != null )  Lbl_onAnalyzingM.Content = MTHD.MethodName;
			}

			Lbl_onAnalyzingTSM.Content = AnalyzerLap.Elapsed.TimespanToString();
			elp_OnWorkMlt.Visibility = (((++OnWorkCC)%10)<4)? Visibility.Visible: Visibility.Hidden;

			if( displayTimer_Stop ){
				displayTimer.Stop();  
			  //Lbl_onAnalyzingM.Content = "";
				elp_OnWorkMlt.Visibility = Visibility.Hidden;
			}
		 }

		private bool IsValidPuzzle_pageUI(){
			var (singlePuzzleB,_) = pGNPX_Eng.IsValidPuzzle();
			if( singlePuzzleB is false ){ 
				shortMessage( "Invalid Puzzle", new sysWin.Point(20,50), Colors.Crimson, tm:3000);
			}
			return singlePuzzleB;
		}
		#endregion Show analysis information



		  /* state transition
			[0] "Initial state"
				- button"Solve" click then -> [1]

			[1] "Solver start(mouse down)"
				- Solver_Engin start  -> [E0->E1]
				- button"Solve" up    -> [2]

					    [E1] "Engin working"
							- Engin Complate  -> [E2]
							- Engin Suspend   -> [E3]
							- [E2 or E3] & [2] -> [0]

			[2] "Solver Command received(button up)"
			    - "Engin Complate/Suspend" [2] & [E2 or E3] -> [0]

		  */



		private void Move_MousePosition( Button btnQ, int sftX, int sftY ){
			POINT ptX = new POINT();
			GetCursorPos( out ptX );
            SetCursorPos((int)ptX.x+sftX, (int)ptX.y+sftY);							// Move the mouse cursor
		}


		private int shift_MousePos = 50;
		private async void btn_MultiSolve_Click( object sender, RoutedEventArgs e ) {
			//if( G6.OnWork!="" || GNPX_Engin.SolverBusy)  goto LProcessEnd;
			if( (G6.SolverState&1) != 0 )  return;

			Move_MousePosition( btn_MultiSolve, 0, -shift_MousePos ); // Move mouse position(up). Prevent inappropriate clicks.

			txbxAnalyzerResultM.Foreground = Brushes.White;
			btn_MultiSolve.Visibility = Visibility.Hidden;


			G6.SolverState |= 1;
			G6.EnginState  = 0;

			#region <<< Preprocessing >>>
				if( stageNo < 0 ){
					var se = new Gidoo_EventHandler(eName: "SystemError", Message: "Func_SolveSingle");
					Send_Command_to_GNPXwin(this, se);
					goto LProcessEnd;;
				}
				if( stageNo == 0 ){
					if(!IsValidPuzzle_pageUI() )  goto LProcessEnd;
					ePZL.BOARD.ForEach( p=>p.Reset_All() );
				}

				ePZL.BOARD.ForEach( P => P.ECrLst = null );		// ... Clear color information from previous analysis results.

				if( G6.OnWork!="" || GNPX_Engin.SolverBusy)  goto LProcessEnd;

				if( pGNPX_Eng.IsSolved() ){
					txbxAnalyzerResultM.Text = "\r solved.";
					goto LProcessEnd;;
				}

				//-----------------------------------------------------------------------------
				Lbl_onAnalyzingM.Content = "";
				Lbl_onAnalyzingM.Foreground = Brushes.LightGreen;
				Lbl_onAnalyzingM.Visibility = Visibility.Visible;
				txb_StepMCC.Text = stageNo.ToString();

				OnWorkCC = 0;
				GNPX_Engin.MltAnsSearch = true;
				GNPX_Engin.SolInfoB = true;


				// Measure the search time for each stage of multiple solution search.
				// Incorporating an "upper time check" function into algorithms that require a lot of time
				GNPX_App_Man.MultiSolve_StartTime = DateTime.Now;  

				int MSlvr_MaxLevel = G6.MSlvr_MaxLevel;
				int MSlvr_MaxNoAlgorithm = G6.MSlvr_MaxNoAlgorithm;
				int MSlvr_MaxNoAllAlgorithm = G6.MSlvr_MaxNoAllAlgorithm;
				int MSlvr_MaxTime = G6.MSlvr_MaxTime;
				G6.stopped_StatusMessage = "";
				G6.command_Analysis_Stop = false;
				cts = new CancellationTokenSource();

			#endregion <<< Preprocessing >>>

			{ // <<< Solving >>>
				AnalyzerLap.Restart();
				bool solveup = false;

				displayTimer_Stop = false;
				displayTimer.Start();
				GNPX_Engin.SolverBusy = true;
				bool methodSet = true;
				do{
					try{
						string retInfo = pGNPX_Eng.Set_NextStage();
						if( retInfo == "solved" ){ Send_SetPuzzleOnBoard(); goto LSolved; }

						bool ErrorStopB = !pGNPX_Eng.Adjust_Candidates();  // Confirm element
						if(ErrorStopB || ePZL.BOARD.All(p => p.No!=0) )  goto LSolved;   // --- All digits are confirmed. ---

						ePZL.SolCode = 0;
						// <<< solver_task start >>>
						await Task.Run(( ) => App_Ctrl.task_Analyzer_SingleStage( cts.Token, methodSet:methodSet )  );

						if( G6.command_Analysis_Stop )  break;
					}
					catch(Exception ex) { WriteLine($"{ex.Message}\r{ex.StackTrace}"); }
					methodSet = false;
				}while(solveup);

		      LSolved:
				GNPX_Engin.SolverBusy = false;
				AnalyzerLap.Stop();
				Lbl_onAnalyzingTSM.Content = AnalyzerLap.Elapsed.TimespanToString();

				if( G6.stResult == "Unsolved" ){
					//shortMessage( "Unsolved", new sysWin.Point(150,50), Colors.Crimson, tm:1500);
					txbxAnalyzerResultM.Foreground = Brushes.LightPink;
				}
				//else{
				//	Send_SetPuzzleOnBoard();
				//}

			}

		#region <<< PostProcessing >>>
			txbxAnalyzerResultM.Text = ePZL.Sol_Result;
			txb_StepMCC.Text = ePZL.stageNo.ToString();

			G6.MSlvr_MaxLevel = MSlvr_MaxLevel;
			G6.MSlvr_MaxNoAlgorithm = MSlvr_MaxNoAlgorithm;
			G6.MSlvr_MaxNoAllAlgorithm = MSlvr_MaxNoAllAlgorithm;
			G6.MSlvr_MaxTime = MSlvr_MaxTime;
			G6.stopped_StatusMessage = "";

			Set_USolLst2( selIndx:0 );

		//	Thread.Sleep(20);
			Move_MousePosition( btn_MultiSolve, 0, shift_MousePos ); // Restore mouse position.

		#endregion PostProcessing

		LProcessEnd:
			displayTimer_Stop = true;
			displayTimer.Start();
			Send_SetPuzzleOnBoard();
			btn_MultiUndo.IsEnabled = true;
			btn_MultiSolve.Visibility = Visibility.Visible;

			return;
		}

		private void btn_MultiSolve_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			G6.SolverState |= 2;
			TransitionToInitialState();
		}

		private void btn_MultiStop_Click(object sender, RoutedEventArgs e) {
			// <<< Suspend >>>
			btn_MultiStop.Visibility = Visibility.Hidden;

			btn_MultiSolve.Content = pRes.btn_Solve;
			Lbl_onAnalyzingM.Foreground = Brushes.Red;
			Lbl_onAnalyzingM.Content = "Canceled";
			GNPX_Engin.SolverBusy = false;
			cts.Cancel();
			
			displayTimer_Stop = true;
        }

		private void TransitionToInitialState(){
			if( G6.SolverState==3 && G6.EnginState>=2 )  G6.SolverState = 0;
		}


		private void btn_MultiUndo_Click(object sender, RoutedEventArgs e) {
			if( ePZL.stageNo <= 1 ){
				btn_AnalyzerResetAll_Click(sender, e);
				btn_MultiUndo.IsEnabled = false;
			}

			else{
				pGNPX_Eng.Add_stageNo();

				ePZL.Clear();
				ePZL = ePZL.pre_PZL;

				txb_StepMCC.Text = ePZL.stageNo.ToString();	
				txbxAnalyzerResultM.Text = ePZL.Sol_Result;
				Set_USolLst2( selIndx:ePZL.selectedIX );

				G6.taskCompInfo = "Complated";
				btn_MultiSolve.Content   = pRes.btn_MultiSolve;
				GNPX_Engin.SolverBusy = false;     
			}
			Send_SetPuzzleOnBoard();

			G6.SolverState = 0;		// under Test ... Okay! Maybe
		}
		
		
		private void Set_USolLst2( int selIndx ){
			int mpCC = 0;
			List<UPuzzleS> USolLst2 = ePZL.Child_PZLs.ConvertAll(G => new UPuzzleS(G, ++mpCC));
			if( USolLst2!=null && USolLst2.Count>0) {
				LstBx_MltSolutions.ItemsSource = USolLst2;
				LstBx_MltSolutions.ScrollIntoView(USolLst2.First());
				LstBx_MltSolutions.SelectedIndex = selIndx;
				
				GNPX_Engin.SolverBusy = false;

				List<int> USolLst2Error = USolLst2.FindAll( q => q.Sol_Result.Contains("error") ).ConvertAll(q=>q.__ID);
				if( USolLst2Error.Count == 0 )  txbAlgorithmError.Visibility = Visibility.Collapsed;
				else{
					txbAlgorithmError.Text = $"GNPX Algorithm error\n {string.Join(" ,", USolLst2Error)}";
					txbAlgorithmError.Visibility = Visibility.Visible;
				}
			}
			else{
				LstBx_MltSolutions.ItemsSource = null;
			}
		}		
		private void LstBx_MltSolutions_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if( GNPX_Engin.SolverBusy )  return;
            
            try{
                var Q = (UPuzzleS)LstBx_MltSolutions.SelectedItem;
                if( Q is null )  return;
                txbxAnalyzerResultM.Text= $"[{Q.__ID}] {Q.Sol_ResultLong}"; 

                int selX = LstBx_MltSolutions.SelectedIndex;
                if( selX>=0 )  pGNPX_Eng.Set_selectedChild( selX );

				Send_SetPuzzleOnBoard();	// <<< Display selected analysis results on the board >>>

				// ..... solver algorith error .....
				SolidColorBrush br = new(Color.FromArgb(0xFF, 0x0A, 0x0a,0x24) );
				if( ePZL.Sol_ResultLong.Contains(" ... error ...") ){
					br = new(Color.FromArgb(0xFF, 0x3A, 0x0a,0x24) );
				}

				txbxAnalyzerResultM.Background = br;
            }
            catch(Exception e2){ WriteLine($"{e2.Message}\r{e2.StackTrace}"); }
            finally{ }
		}


		private async void btn_AnalyzerResetAll_Click(object sender, RoutedEventArgs e) {
			//Send_Command_to_GNPXwin( this, new Gidoo_EventHandler( eName:"Vibrate" ) );  //__bruMoveSub();

			while(ePZL.stageNo>0){
				ePZL = ePZL.pre_PZL;
				txb_StepMCC.Text = ePZL.stageNo.ToString();	
				await Task.Delay(50);
				Send_SetPuzzleOnBoard();
			}
             
            pGNPX_Eng.MethodLst_Run__Reset_UsedCC();
			ePZL.Reset_ToInitial();

            Puzzle_ResetAnalyzer(true);
            btn_MultiUndo.IsEnabled = false;

            pGNPX_Eng.ReturnToInitial();

			txbxAnalyzerResultM.Foreground = Brushes.White;
			SolidColorBrush br = new(Color.FromArgb(0xFF, 0x0A, 0x0a,0x24) );
			txbxAnalyzerResultM.Background = br;
					
			G6.SolverState = 0;
			G6.EnginState  = 0;
			return;
		}

			// ------------------------------------------
		private void Puzzle_ResetAnalyzer( bool AllF=true ){
			if( G6.OnWork != "" ) return;
                       
			pGNPX_Eng.Add_stageNo();
			pGNPX_Eng.Clear_0();

			txbxAnalyzerResultM.Text   = "";
			txbxAnalyzerResultM.Foreground = Brushes.White;
			SolidColorBrush br = new(Color.FromArgb(0xFF, 0x0A, 0x0a,0x24) );
			txbxAnalyzerResultM.Background = br;

			LstBx_MltSolutions.ItemsSource   = null;

			btn_MultiSolve.IsEnabled   = true;
			txb_StepMCC.Text = "0";

			Lbl_onAnalyzingM.Content = "";

			pGNPX_Eng.AnalyzerCounterReset();
			pGNPX_Eng.ePZL.pMethod = null;

			eGeneralLogicGen_base.GLtrialCC=0;

			Send_SetPuzzleOnBoard();
		}

	  #region Conditions for multiple solution search
		private void Save_XMLFile_AnalysisConditions(){
			if( pGNPX_App != null )  pGNPX_App.WriteXMLFile_AnalysisConditions_MethodList();
		}

		private void MSlvr_MaxLevel_ValueChanged(object sender, GIDOOEventArgs args) {
            if( eventFire_Stop )  return;
			G6.MSlvr_MaxLevel = MSlvr_MaxLevel.Value;
			Save_XMLFile_AnalysisConditions();
		}

		private void MSlvr_MaxNoAlgorithm_ValueChanged(object sender, GIDOOEventArgs args) {
			if( eventFire_Stop )  return;
            G6.MSlvr_MaxNoAlgorithm = MSlvr_MaxNoAlgorithm.Value;
			Save_XMLFile_AnalysisConditions();
		}

		private void MSlvr_MaxNoAllAlgorithm_ValueChanged(object sender, GIDOOEventArgs args) {
            if( eventFire_Stop )  return;
            G6.MSlvr_MaxNoAllAlgorithm = MSlvr_MaxNoAllAlgorithm.Value;
			Save_XMLFile_AnalysisConditions();
		}

		private void MSlvr_MaxTime_ValueChanged(object sender, GIDOOEventArgs args) {
            if( eventFire_Stop )  return;
            G6.MSlvr_MaxTime = MSlvr_MaxTime.Value;
			Save_XMLFile_AnalysisConditions();
		}

		private void chkPreferSimpleLinks_Checked(object sender, RoutedEventArgs e) {
			G6.PreferSimpleLinks = (bool)chkPreferSimpleLinks.IsChecked? 1: 0;
			Save_XMLFile_AnalysisConditions();
		}




		#endregion Conditions for multiple solution search

    }
}
	