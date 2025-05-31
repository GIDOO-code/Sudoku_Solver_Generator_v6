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

namespace GNPX_space{
	using pRes = Properties.Resources;
	using sysWin = System.Windows;
	using ioPath = System.IO.Path;
	using pGPGC = GNPX_Puzzle_Global_Control;

    public partial class Func_SolveSingle: Page{
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 
		static public event GNPX_EventHandler Send_Command_to_SolveSingle;

		public GNPX_App_Man 		pGNPX_App;
		private G6_Base				G6        => GNPX_App_Man.G6;
		public GNPX_win				pGNP00win => pGNPX_App.pGNP00win;
		private GNPX_Graphics		gnpxGrp   => pGNPX_App.gnpxGrp;
        private GNPX_Engin			pGNPX_Eng => pGNPX_App.GNPX_Eng;   
		public GNPX_App_Ctrl        App_Ctrl  => pGNPX_App.App_Ctrl;
		
		private GNPX_AnalyzerMan    pAnMan    => pGNPX_Eng.AnMan;  // pGNPX_App.GNPX_Eng.AnMan;

		public  UPuzzle				ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; }  // Puzzle to analyze
		private int                 stageNo   => (ePZL is null)? -1: ePZL.stageNo;
	
		private DispatcherTimer     displayTimer;	
		private bool				displayTimer_Stop;
		private object				obj = new();

		private int                 OnWorkCC = 0;

		private MethodDisplayWin	MthdWin;

		private CancellationTokenSource  cts;
		private Stopwatch			AnalyzerLap = new();
        private DispatcherTimer		timerShortMessage;



		#region <<< Constracter, Load, Unload >>>
        public Func_SolveSingle( GNPX_App_Man GNPX_App ){
            InitializeComponent();

			lbl_ShortMessage.Visibility = Visibility.Hidden;

			pGNPX_App = GNPX_App;
			cts = new CancellationTokenSource();　//for Cancellation 

			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  
			pGNP00win.Send_Command_to_Func_SolveSingle += new GNPX_EventHandler( GNPX_Event_Handling_man );

			displayTimer = new DispatcherTimer(DispatcherPriority.Normal);
			displayTimer.Interval = TimeSpan.FromMilliseconds(50);
			displayTimer.Tick += new EventHandler(Show_RunningMethodName_Tick);

			timerShortMessage = new DispatcherTimer(DispatcherPriority.Normal);
			timerShortMessage.Interval = TimeSpan.FromMilliseconds(50);
			timerShortMessage.Tick += new EventHandler(timerShortMessage_Tick);
        }



		private void Page_Loaded( object sender, RoutedEventArgs e ){
			G6.PG6Mode = GetType().Name;
			if( chbx_ShowCandidate != null ){
				chbx_ShowCandidate.IsChecked = G6.sNoAssist;
				Lbl_onAnalyzing.Content = "";
				chbx_ShowCandidate.IsChecked = true;

				if( chb_DigitsColoring != null ){ 				
					chb_DigitsColoring.IsChecked = false; 
					grdColorPad9.Visibility=Visibility.Hidden;
				}
			}
			
			if( ePZL != null ) Send_SetPuzzleOnBoard();

            UCell _P0 = new UCell(rc: 0, FreeB: 0x1FF);
            _P0.Set_CellColorBkgColor_noBit(noB: 0x1FF, clr: Colors.Navy, clrBkg: Colors.White);
            imgDevelop.Source = gnpxGrp.Create_CellImageLight_withScale(_P0, 1.0);

			G6.sNoAssist = true;
			G6.digitColoring = false;
			if( stageNo == 0 ){
				txbxAnalyzerResult.Text = "";
				txbxAnalyzerResult.Foreground = Brushes.White;
				DGView_MethodCounter.ItemsSource = null;
			}

			elp_OnWork.Visibility = Visibility.Hidden;
			chbx_SlowPlayback.IsChecked = G6.slowPlayback;
#if DEBUG
			MthdWin_Control( winDisp:true );
#else
			MthdWin_Control( winDisp:false );
#endif
		}
		
		private void Page_Unloaded(object sender, RoutedEventArgs e) {
			MthdWin.Visibility = Visibility.Collapsed;//	Close();
		}
		#endregion

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
		#endregion

		#region Event

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

		private void Send_SetPuzzleOnBoard(){
			Send_Command_to_GNPXwin( this, new Gidoo_EventHandler( eName:"Show_AnalysisStatus" ) );
		}
		#endregion Event

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


		private int __dispCC=0;
		private void Show_RunningMethodName_Tick( object sender, EventArgs e ){
			UAlgMethod MTHD = null;	
			lock(obj){ MTHD = GNPX_Engin.AnalyzingMethod; }
			if( MTHD == null )  return;
			
			Lbl_onAnalyzing.Content = MTHD.MethodName;
			txbStepCC.Text = ePZL.stageNo.ToString();

			Lbl_onAnalyzingTS.Content = AnalyzerLap.Elapsed.TimespanToString();
			lbl_CurrentndifficultyLevel.Content = $"difficulty : {MTHD.difficultyLevel}";
			
			elp_OnWork.Visibility = (((++OnWorkCC)%5)<2)? Visibility.Visible: Visibility.Hidden;
			if( displayTimer_Stop ){
				displayTimer.Stop();
			
				Lbl_onAnalyzing.Content = "";
				elp_OnWork.Visibility = Visibility.Hidden;
			}

			Send_SetPuzzleOnBoard();
		}
		private bool IsValidPuzzle_pageUI(){
			var (singlePuzzleB,_) = pGNPX_Eng.IsValidPuzzle();
			if( singlePuzzleB is false ){ 
				shortMessage( "Invalid Puzzle", new sysWin.Point(140,50), Colors.Crimson, tm:3000);
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



		// ===== UI Functions ================================================
				private void btn_Solve_Click( object sender, RoutedEventArgs e ){
					if( (G6.SolverState&1) != 0 )  return;
					G6.SolverState |= 1;
					G6.EnginState  = 0;


					try{
						AnalyzerBaseV2.__SimpleAnalyzerB__ = true;
						txbxAnalyzerResult.Foreground = Brushes.White;
						//G6.command_Analysis_Stop = false;  ... not required
						__GNPX_Solver( solveup:false );
						TransitionToInitialState();
					}
					catch( Exception ex ){ WriteLine( $"{ex.Message}\n{ex.StackTrace}" ); }
				}
				private void btn_Solve_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
					G6.SolverState |= 2;
					TransitionToInitialState();
				}



				private void btn_SolveUp_Click(object sender, RoutedEventArgs e) {
					//if( G6.SolverState != 0 )  return;	//Runs in any condition!
					G6.SolverState = 1;
					G6.EnginState  = 0;

					if( !IsValidPuzzle_pageUI() ) return;
					//G6.command_Analysis_Stop = false;  ... not required
					__GNPX_Solver( solveup:true );
					TransitionToInitialState();
				}


				private void TransitionToInitialState(){
					if( G6.SolverState==3 && G6.EnginState>=2 )  G6.SolverState = 0;
				}


				private void btn_Undo_Click( object sender, RoutedEventArgs e ){
					Lbl_onAnalyzing.Content = "";
					if( stageNo == 0 )  return;
					else{
						ePZL.Clear();
						ePZL = ePZL.pre_PZL;
							//WriteLine( $"   --- ePZL.stageNo:{ePZL.stageNo} {ePZL.pMethod}  SolCode:{ePZL.SolCode} >>> {G6.stResult}" );

						txbStepCC.Text = ePZL.stageNo.ToString();
						Lbl_onAnalyzing.Content = (ePZL.pMethod!=null)? ePZL.pMethod.MethodName: "";				
						txbxAnalyzerResult.Text = ePZL.Sol_Result;
						txbxAnalyzerResult.Foreground = Brushes.White;
						_Set_DGView_MethodCounter( SetDiffB:false );
					}
					if( stageNo == 0 )  btn_AnalyzerResetAll_Click(sender, e);
					Send_SetPuzzleOnBoard();
				}

				private void chbx_SlowPlayback_Checked(object sender, RoutedEventArgs e) {
					G6.slowPlayback = (bool)chbx_SlowPlayback.IsChecked;
				}
				private async void btn_AnalyzerResetAll_Click(object sender, RoutedEventArgs e) {
					if( stageNo == 0 )  return;
					while(ePZL.stageNo>0){
						ePZL = ePZL.pre_PZL;
						txbStepCC.Text = ePZL.stageNo.ToString();	
						if(G6.slowPlayback)  await Task.Delay(50);
						Send_SetPuzzleOnBoard();
					}

					Puzzle_ResetAnalyzer();

					// Send_Command_to_GNPXwin( sender, new Gidoo_EventHandler( eName:"Vibrate!" ) );
					Send_SetPuzzleOnBoard();

					// Update display information:Clear
					Send_Command_to_GNPXwin( this, new Gidoo_EventHandler(eName:"Solved", Message:"SolveSingle") );
				}

				private void Puzzle_ResetAnalyzer(){
					Lbl_onAnalyzing.Content = "";
					txbxAnalyzerResult.Text = "";
					lbl_CurrentndifficultyLevel.Visibility = Visibility.Hidden;
					DGView_MethodCounter.ItemsSource = null;
					
					G6.SolverState = 0;
					G6.EnginState  = 0;
				}



		private async void __GNPX_Solver( bool solveup ){
			G6.EnginState = 1;

			if( stageNo < 0 ){ 
				var se = new Gidoo_EventHandler( eName:"SystemError", Message:"Func_SolveSingle" );
				Send_Command_to_GNPXwin( this, se ); goto LSolved;
			}
			if( stageNo==0 ){ 
				if( !IsValidPuzzle_pageUI() ) goto LSolved;
				ePZL.BOARD.ForEach( p=>p.Reset_All() );
			}				
			ePZL.BOARD.ForEach( P => P.ECrLst = null );		// ... Clear color information from previous analysis results.

			if( G6.OnWork!="" || GNPX_Engin.SolverBusy )   goto LSolved;


			if( pGNPX_Eng.IsSolved() ){ txbxAnalyzerResult.Text = "\r solved."; goto LSolved; }






			cts.TryReset();
			// <<< suspend >>>
			if( (string)btn_Solve.Content == pRes.msgSuspend ){    
				btn_Solve.Content = pRes.btn_Solve;
				Lbl_onAnalyzing.Foreground = Brushes.Red;
				Lbl_onAnalyzing.Content = "Canceled";
				cts.Cancel();

				displayTimer_Stop = true;
				goto LSolved;
			}




			// <<< Solve >>>
			lbl_CurrentndifficultyLevel.Visibility = Visibility.Visible;
			__GNPX_Solver_PreProcessing();

				displayTimer_Stop = false;
				displayTimer.Start();
				do{	
					try{			
						string retInfo = pGNPX_Eng.Set_NextStage( skip_valid_check:true ); 
						if( retInfo == "solved" ){ Send_SetPuzzleOnBoard(); goto LSolved; }
						
						if( stageNo > 1 ){
							bool ErrorStopB = !pGNPX_Eng.Adjust_Candidates();  // Confirm element
							if( ePZL.BOARD.All(p=>p.No!=0) ){ G6.stResult="Solved"; goto LSolved; }
							//if( ErrorStopB || ePZL.BOARD.All(p=>p.No!=0) ) goto LSolved;	// --- All digits are confirmed. --- #####
							if( ErrorStopB ) goto LSolved;	// --- All digits are confirmed. ---
						}
							
						ePZL.SolCode=0;
						GNPX_Engin.SolverBusy = true;
						// Solve the Puzzle (solver_task start) 
						await Task.Run( () => App_Ctrl.task_Analyzer_SingleStage(cts.Token,methodSet:true) );

						// ------------------
						if( !pGNPX_Eng.AnalysisResult )  goto LSolved;

					}
					catch( Exception ex ){ WriteLine( $"{ex.Message}\r{ex.StackTrace}" ); }	
				}while(solveup);


		LSolved:
			__GNPX_Solver_PostProcessing();

			displayTimer_Stop = true;
			GNPX_Engin.SolverBusy = false;
					//WriteLine( $" +++ ePZL.stageNo:{ePZL.stageNo} {ePZL.pMethod}  SolCode:{ePZL.SolCode} >>> {G6.stResult}" );
			if( G6.stResult == "Unsolved" ){
				//shortMessage( "Unsolved", new sysWin.Point(150,50), Colors.Crimson, tm:1500);
				txbxAnalyzerResult.Foreground = Brushes.LightPink;
			}
			else{
				if( ePZL.BOARD.All(p=>p.No!=0) )  ePZL.BOARD.ForEach(p=>p.ECrLst=null);
				Send_SetPuzzleOnBoard();
			}
			return;

			// ==========================================================================================

				// ----- inner function -----
					void __GNPX_Solver_PreProcessing( ){
						btn_Solve.Content = pRes.msgSuspend;
						txbStepCC.Text   = stageNo.ToString();

						Lbl_onAnalyzing.Content    = pRes.Lbl_onAnalyzing;	
						Lbl_onAnalyzing.Foreground = Brushes.Orange;
						Lbl_onAnalyzing.Content = "";
						this.Cursor = Cursors.Wait;

						GNPX_App_Man .chbx_ConfirmMultipleCells = (bool)chbx_ConfirmMultipleCells.IsChecked;
						GNPX_Engin.SolInfoB = true;

						G6.taskCompInfo="";  
						G6.OnWork = "Solver_Single";
						
						AnalyzerLap.Restart();
					}

					void __GNPX_Solver_PostProcessing( ){
						AnalyzerLap.Stop(); 
//						Lbl_onAnalyzingTS.Content = AnalyzerLap.Elapsed.TimespanToString();

						btn_Solve.Content = pRes.btn_Solve;
						txbStepCC.Text = ePZL.stageNo.ToString();	

						Lbl_onAnalyzing.Content = pRes.Lbl_onAnalyzing;
						
						Lbl_onAnalyzing.Foreground = Brushes.LightGreen;
						Lbl_onAnalyzing.Visibility = Visibility.Visible;

						txbxAnalyzerResult.Text = ePZL.Sol_Result;
//						lbl_CurrentndifficultyLevel.Content = $"difficulty:{ePZL.pMethod.difficultyLevel}";

						if( ePZL.extResult != null ){
							Send_Command_to_GNPXwin( this, new Gidoo_EventHandler(eName:"Solved", Message:"SolveSingle") );
						}

						bool SetDiffB = (bool)chbx_SetDifficulty.IsChecked;
						_Set_DGView_MethodCounter( SetDiffB );

						G6.taskCompInfo="";  
						G6.EnginState = 2;


						G6.OnWork = "";
						this.Cursor = Cursors.Arrow;
					}
		}









	//===== Analysis Support ==================================================================================================
	#region Analysis Support	
		private void chbx_ShowCandidate_Checked(object sender, RoutedEventArgs e) {
			if( chb_DigitsColoring == null) return;

			G6.sNoAssist = (bool)chbx_ShowCandidate.IsChecked;
            G6.digitColoring = G6.sNoAssist && (bool)chbx_ShowCandidate.IsChecked; //

			chb_DigitsColoring.IsEnabled = G6.digitColoring;

			bool ColorPadB = G6.digitColoring && (bool)chb_DigitsColoring.IsChecked;
			grdColorPad9.Visibility = ColorPadB? Visibility.Visible : Visibility.Hidden;

            Send_SetPuzzleOnBoard();
		}     
		
		private void chb_DigitsColoring_Checked(object sender, RoutedEventArgs e){
            G6.digitColoring = G6.sNoAssist && (bool)chb_DigitsColoring.IsChecked;
            if( grdColorPad9 == null ) return;
            grdColorPad9.Visibility = G6.digitColoring ? Visibility.Visible : Visibility.Hidden;
            Send_SetPuzzleOnBoard();
        }	


        private void CreateDisplay_imgDevelop(){
            UCell _P0 = new UCell(rc: 0, FreeB: 0x1FF);
            _P0.Set_CellColorBkgColor_noBit(noB: 0x1FF, clr: Colors.Navy, clrBkg: Colors.White);
            imgDevelop.Source = gnpxGrp.Create_CellImageLight_withScale(_P0, 1.0);

            Send_SetPuzzleOnBoard();
        }

        private void develop_Initialize(){
            CreateDisplay_imgDevelop();
        }

		private void Develp_Close_Click(object sender, RoutedEventArgs e) {
			grdColorPad9.Visibility = Visibility.Hidden;
		}

        private void Develp_Set_Click(object sender, RoutedEventArgs e){
            int fMask = G6.FreeBmask;
            fMask = (fMask <<1) & 0x1FF;
            if( fMask==0 ) fMask = 1;
            G6.FreeBmask = fMask;
            Send_SetPuzzleOnBoard();
            CreateDisplay_imgDevelop();
            __imageSave();
        }

        private void Develp_Reset_Click(object sender, RoutedEventArgs e){
            G6.FreeBmask = 0;
            Send_SetPuzzleOnBoard();
            CreateDisplay_imgDevelop();
        }

        private void Develop_multi_Checked(object sender, RoutedEventArgs e){
          //f(multiB) return;
            int fMask = G6.FreeBmask;
            if( (bool)Develop_multi.IsChecked && fMask==0 ) fMask=0x1FF;
            G6.FreeBmask = fMask;
            Send_SetPuzzleOnBoard();
            CreateDisplay_imgDevelop();
        }


        private void imgDevelop_MouseUp(object sender, MouseButtonEventArgs e){
            if (!imgDevelop.IsMouseOver) return;
            int nn = _Get_imgDevelop_Position();

            int freeB9 = G6.FreeBmask;
			bool multiB = (bool)Develop_multi.IsChecked;
            if (multiB) freeB9 ^= (1<<nn);
            else freeB9 = 1<<nn;
            G6.FreeBmask = freeB9;

            CreateDisplay_imgDevelop();
            //WriteLine($" imgDevelop_MouseUp n:{nn} FreeBmask:{G6.FreeBmask.ToBSt()}");

            // ----------------- inner function -----------------
            int _Get_imgDevelop_Position(){
                Point pt = Mouse.GetPosition(imgDevelop);
                int rn = (int)(pt.Y/12), cn = (int)(pt.X/12);
                return (rn * 3 + cn);
            }
        }

        private void __imageSave(){
            UCell P0 = new UCell(rc: 0, FreeB: 0x1FF);
            BitmapEncoder enc = new PngBitmapEncoder();
            var bmp = gnpxGrp.Create_CellImageLight_withScale(P0, 1.0);
            var bmf = BitmapFrame.Create(bmp);
            enc.Frames.Add(bmf);
            try{
                Clipboard.SetData(DataFormats.Bitmap, bmf);
            }
            catch( System.Runtime.InteropServices.COMException ){ /* NOP */ }

            if (!Directory.Exists(pRes.fldSuDoKuImages)) { Directory.CreateDirectory(pRes.fldSuDoKuImages); }
            string fName = DateTime.Now.ToString("yyyyMMdd HHmmss") + ".png";
            using( Stream stream = File.Create(pRes.fldSuDoKuImages + "/" + fName) ){
                enc.Save(stream);
            }
            bitmapPath.Content = "Path : " + Path.GetFullPath(pRes.fldSuDoKuImages + "/" + fName);
		}

	#endregion Analysis Support

	#region MethodCounter	
		private class _MethodCounter{
			public int    ID; 
			public string methodName{ get; set; }
			public string difficultyLevel{  get; set; }
			public string count{ get; set; }

			public _MethodCounter( string nm, int cc ){
				ID = nm.Substring(0,7).ToInt();
				methodName = " "+nm.Substring(9);//.PadRight(30);
				difficultyLevel = nm.Substring(7,2)+"  ";//.PadRight(30);
				count      = cc.ToString()+"  ";
			}
		}	
						
		private void _Set_DGView_MethodCounter( bool SetDiffB ){  // Aggregation of methods used
            List<_MethodCounter> methodCounters;

            var _MethodCounter = new Dictionary<string,int>();
            var Q = pGNPX_Eng.ePZL;
            if( Q.stageNo == 0 ){ DGView_MethodCounter.ItemsSource=null; return; }            


			int difficultyLevelMax = 0;
			UAlgMethod Method_DiffMax = null;
            while( Q!=null && Q.stageNo>0 ){
                if( Q.pMethod == null )  continue;
                string keyString = Q.pMethod.MethodKey;     //"ID"+MethodName
                if( !_MethodCounter.ContainsKey(keyString) )  _MethodCounter[keyString] = 0;
                _MethodCounter[keyString] += 1;

				// Maximum difficulty and MethodName
				if( Q.pMethod.difficultyLevel > difficultyLevelMax ){
					Method_DiffMax = Q.pMethod;
					difficultyLevelMax = Method_DiffMax.difficultyLevel;
				}
				Q = Q.pre_PZL;
            }

            if( _MethodCounter.Count>0 ){
                methodCounters = new List<_MethodCounter>();
                foreach( var q in _MethodCounter )  methodCounters.Add( new _MethodCounter(q.Key, q.Value) );

                methodCounters.Sort( (a,b) => (a.ID-b.ID) );

				if( SetDiffB ){
					// There is an issue with puzzle data management.
					// As a temporary measure, values ​​will be set in two places (P0, ePZL)
					// This will be fixed in the future version.

					UPuzzle P0 = pGPGC.GetCurrentPuzzle( );
					P0.difficultyLevel = difficultyLevelMax;
					if( P0.Name == "" ) P0.Name = Method_DiffMax.MethodName;
					
					ePZL.difficultyLevel = difficultyLevelMax;
					if( ePZL.Name == "" ) ePZL.Name = Method_DiffMax.MethodName;
				}

                DGView_MethodCounter.ItemsSource = methodCounters;
                if( methodCounters.Count>0)  DGView_MethodCounter.SelectedIndex = -1;


			/*	Written in XML
				if( methodCounters.Count>0 && DGView_MethodCounter.Columns.Count>1 ){
                    Style style = new Style(typeof(DataGridCell));
                    style.Setters.Add( new Setter(DataGrid.HorizontalAlignmentProperty, HorizontalAlignment.Right) );
                  //  DGView_MethodCounter.Columns[1].CellStyle = style;
                    DGView_MethodCounter.Columns[2].CellStyle = style;
                }
			*/
            }
			return;	
		}


	}
	#endregion MethodCounter	
}
