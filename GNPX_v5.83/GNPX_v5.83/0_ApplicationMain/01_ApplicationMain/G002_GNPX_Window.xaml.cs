using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Globalization;

using static System.Math;
using static System.Diagnostics.Debug;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using System.Windows.Threading;
using System.Runtime.InteropServices;

using GIDOO_space;
using System.IO;
using System.Windows.Shapes;

namespace GNPX_space{

    using pRes=Properties.Resources;
    using sysWin=System.Windows;

	using pGPGC = GNPX_Puzzle_Global_Control;

    public partial class GNPX_win: sysWin.Window{
		public event GNPX_EventHandler Send_Command_to_Func_Transform; 
		public event GNPX_EventHandler Send_Command_to_Func_SolveMulti; 
		public event GNPX_EventHandler Send_Command_to_Func_SolveSingle; 

		static public bool    EventSuppression=false;

		static private G6_Base G6 => GNPX_App_Man.G6;

		[DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]		// ... Required for SetCursorPos
        static private extern void SetCursorPos(int X,int Y);							//Move the mouse cursor to Control

        private sysWin.Point    WinPrePos;

    // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==
        public  GNPX_App_Man     GNPX_App;
		public  GNPX_App_Ctrl    App_Ctrl => GNPX_App.App_Ctrl;

		private Func_Option		 objFunc_Option => (Func_Option)GNPX_App.objFunc_Option;

        private GNPX_Engin       pGNPX_Eng => GNPX_App.GNPX_Eng;   
		private GNPX_AnalyzerMan pAnMan    => pGNPX_Eng.AnMan;		// pGNPX_App.pGNPX_Eng.AnMan;
        private UPuzzle          ePZL      => pGNPX_Eng.ePZL;		// current board
		
        public  CultureInfo      culture => pRes.Culture;

		public  GNPX_winSub		 GNPX_winSub;

        private int              WOpacityCC=0;
		private object obj = new();

		private DispatcherTimer  startingTimer;
        private DispatcherTimer  endingTimer;
        private DispatcherTimer  animationTimer;   
        private DispatcherTimer  bruMoveTimer;
        private DispatcherTimer  timerShortMessage;


    // ----- Extend -----
        private ExtendResultWin ExtResultWin;


    #region Application start/end
        public GNPX_win(){          

            try{
                InitializeComponent();  
				
				GNPX_App = new GNPX_App_Man(this);
				lbl_ShortMessage.Visibility = Visibility.Hidden;

				G60_PB.Set_Puzzle( ePZL, setImage:true );					// Blank Puzzle Display
				frame_GNPX_Func.Navigate( GNPX_App.objFunc_File );

              //var rdbLst = Extension_Utility.GetControlsCollection<RadioButton>(this);


			#region Timer
				timerShortMessage = new DispatcherTimer(DispatcherPriority.Normal);
				timerShortMessage.Interval = TimeSpan.FromMilliseconds(50);
				timerShortMessage.Tick += new EventHandler(timerShortMessage_Tick);

				startingTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
				startingTimer.Interval = TimeSpan.FromMilliseconds(70);
				startingTimer.Tick += new EventHandler(startingTimer_Tick);
				this.Opacity=0.0;
				startingTimer.Start();

				endingTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
				endingTimer.Interval = TimeSpan.FromMilliseconds(70);
				endingTimer.Tick += new EventHandler(endingTimer_Tick);

				animationTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
				animationTimer.Interval = TimeSpan.FromMilliseconds(200);//50
				animationTimer.Tick += new EventHandler(animationTimer_Tick);

				bruMoveTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
				bruMoveTimer.Interval = TimeSpan.FromMilliseconds(20);
				bruMoveTimer.Tick += new EventHandler(bruMoveTimer_Tick);

				FollowMove_timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher );
				FollowMove_timer.Interval = TimeSpan.FromMilliseconds(10);
				FollowMove_timer.Tick += new EventHandler(FollowMove_timer_Tick);
			#endregion Timer

				//   Frame-Navigation
				// frame_GNPX_Func.Navigate( GNPX_App.objFunc_Solve );
				animationTimer.Start();
			
            }
            catch( Exception ex ){ WriteLine( $"{ex.Message}\r{ex.StackTrace}"); }

        }

        private void Window_Loaded( object sender, RoutedEventArgs e ){
			G6.OperationalMode = "Initializing";		// Initializing / NormalOperation

            EventSuppression = true;

			//   Frame-Navigation
			frame_GNPX_Func.Navigate( GNPX_App.objFunc_Solve );

			frame_GNPX_Func.Navigate( GNPX_App.objFunc_File );		//@@ Show first

			GNPX_App.SolverList_App = GNPX_App.ReadXMLFile_AnalysisConditions_MethodList();
			G6.StartTime			= DateTime.Now;
			G6.sNoAssist			= false;
			G6.digitColoring		= false;
			G6.CellNumMax			= Max( G6.CellNumMax, 17 );
			nUD_MinDifficulty.Value = G6.G60_PuzzleMinDifficulty;

			
			// ..... for Power User .....
				G6.PowerUser			= true; //false	//GNPX_App_Ctrl.GNPX_Random.Next(10) != 0;
				btn_FinalState.Visibility = G6.PowerUser? Visibility.Visible: Visibility.Hidden;
			// ..........................


			G6.OperationalMode = "NormalOperation";		// Initializing / NormalOperation

			chbx_ShowCandidate.IsChecked = G6.sNoAssist;

			{
			//	#if DEBUG
			//		G6.RandomSeedVal = 1;
			//	#else
			//		G6.RandomSeedVal = 0;
			//	#endif
				App_Ctrl.Set_RandomSeed(G6.RandomSeedVal_000);
			}

			btn_Page_Clicked( btnP_File, new RoutedEventArgs() );
        }


		private void G60_PB_MouseEnter(object sender, MouseEventArgs e) {
			//G60_PB.Focus();
			//WriteLine( "@@@ G60_PB.Focus @@@" );
        }
        private void Window_Unloaded( object sender, RoutedEventArgs e ){
            Environment.Exit(0);
        }
	#endregion Application start/end 



	#region Operation services using timers
	  // <<< Start/end Timer >>>   
        private void appExit_Click( object sender, RoutedEventArgs e ){
            GNPX_App.WriteXMLFile_AnalysisConditions_MethodList();
            WOpacityCC=0;
            endingTimer.IsEnabled = true;
            endingTimer.Start();

		//	#if DEBUG
				// Collecting information on LS generation status
				List<string> stg4LS = new();
				foreach( var (k,n) in G6.g7_LSpattern.WithIndex() )  if(k>0)  stg4LS.Add( $"LS count {n}, {k}" );
				string stLS = string.Join("\n",stg4LS);

				string dirDevelop = "Algorithm_Development";
				if( !Directory.Exists(dirDevelop) )  Directory.CreateDirectory(dirDevelop);
				string fNameEmergency = dirDevelop+"/"+"g7_LSpattern.txt";
				Utility_Display.GNPX_StreamWriter( fNameEmergency, stLS, append:true ); 
		//	#endif
        }

        private void startingTimer_Tick( object sender, EventArgs e){
            WOpacityCC++;
            if( WOpacityCC >= 40 ){ this.Opacity=1.0; startingTimer.Stop(); }
            else{
				this.Opacity = WOpacityCC/40.0;
			}
        }
        private void endingTimer_Tick( object sender, EventArgs e){
            if( (++WOpacityCC)>20 )  Environment.Exit(0);   //Application.Exit();
            double dt = Max( 1.0-WOpacityCC/20.0, 0.0 );
            this.Opacity = dt = dt*dt;
			foreach( sysWin.Window w in Application.Current.Windows )  w.Opacity = dt;
        }
        
        private void __bruMoveSub(){ 
            Thickness X=G60_PB.Margin;
			G60_PB.Margin=new Thickness(X.Left+2,X.Top+2,X.Right,X.Bottom);
            bruMoveTimer.Start();
        }
		
        private void bruMoveTimer_Tick( object sender, EventArgs e){
			Thickness X=G60_PB.Margin;   //◆bruMoveTimer.Start();			
			G60_PB.Margin=new Thickness(X.Left-2,X.Top-2,X.Right,X.Bottom);
			Thread.Sleep(50);
            bruMoveTimer.Stop();
        }       

				  // <<< Animation >>>
					private int  opaCum = 0;
					private int  memoSec = 0;
					private void animationTimer_Tick( object sender, EventArgs e ){   //#@
						int sec = DateTime.Now.Second;
						if( memoSec != sec ){
							memoSec = sec;
							opaCum = (++opaCum) % 100;  
							double angle = 2.0 * Math.PI * (opaCum/100.0);
							GNPXv6_main.Opacity = 0.3 + Sin(angle) * 0.2;
						}
					}

      // <<< ShortMessage >>>
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
	#endregion Timer





	#region Location 
        private class Location_TimePos{
            public DateTime dt;
            public Point    pt;
            public Location_TimePos( DateTime d, Point p ){ dt=d; pt=p; }
			public override string ToString() => $"dt:{dt} pt:{pt}";
		}
		private DispatcherTimer  FollowMove_timer = new DispatcherTimer();

		private Queue<Location_TimePos> win_dtpt_List = new Queue<Location_TimePos>();

		private void Window_MouseDown( object sender, MouseButtonEventArgs e ){
            if(e.Inner(G60_PB))    return; //◆			
            this.DragMove();
        }

		private void GNPXwin_LocationChanged( object sender,EventArgs e ){ //Synchronously move open window
			if( G6.windows_Animation == "" ){
				Point shift = new Point(this.Left-WinPrePos.X, this.Top-WinPrePos.Y);
				foreach( sysWin.Window w in Application.Current.Windows )  _GNPXwin_LocationChanged_base(w,shift);

					void _GNPXwin_LocationChanged_base( sysWin.Window w, Point PtShift ){
						if( w==null || w.Owner==this || w==this )  return;
						if( w.Name=="devWin" && !G6.LinkedMove )   return;
						w.Left += PtShift.X;
						w.Top  += PtShift.Y;
					}	
				
			}	
	
			else if( G6.windows_Animation == "chase" ){	
				if( WinPrePos.X==0 && WinPrePos.Y==0 )  WinPrePos = new(this.Left,this.Top);
				
				Location_TimePos LTP;
				var   dt = DateTime.Now;
				Point PtShift = new(this.Left-WinPrePos.X,this.Top-WinPrePos.Y);

				if( win_dtpt_List.Count>0 ){
					LTP = win_dtpt_List.Peek();
					if( (dt-LTP.dt).TotalMilliseconds<10 )  return; // The time interval is short.            
					if( PtShift.X==0 && PtShift.Y==0 )		return;	// Close distance.
				}
				LTP = new(dt,PtShift);
				win_dtpt_List.Enqueue(LTP);
				if( win_dtpt_List.Count>0 )  FollowMove_timer.Start();
			}

			WinPrePos = new (this.Left,this.Top);
		}


		private void FollowMove_timer_Tick( object sender, EventArgs e ){
			if( win_dtpt_List.Count == 0 ){ FollowMove_timer.Stop(); return; } 
			
			Location_TimePos LTP = win_dtpt_List.Peek();
			int timeControl = (win_dtpt_List.Count>20)? 100: 100+50/win_dtpt_List.Count*5;
			if( (DateTime.Now-LTP.dt).TotalMilliseconds<timeControl ) return;

			LTP =  win_dtpt_List.Dequeue();
			foreach( sysWin.Window w in Application.Current.Windows ) GNPXwin_LocationChanged_chase( w, LTP.pt );				

			if( win_dtpt_List.Count <= 0 )  FollowMove_timer.Stop();

					void GNPXwin_LocationChanged_chase( sysWin.Window w, Point PtShift){
						if( w==null || w.Owner==this || w==this )  return;
						if( w.Name=="devWin" && !G6.LinkedMove )   return;

						w.Left += PtShift.X;
						w.Top  += PtShift.Y;
						//w.Topmost=true;
					}	
		}

    #endregion Location



		private void btnP_Extensions_MouseDoubleClick(object sender, MouseButtonEventArgs e ){
			if( btnP_Extensions.Opacity < 0.5 ){
				btnP_Extensions.Opacity = 1.0;
				btnP_Extensions.Width = 60;
				frame_GNPX_Func.Navigate( GNPX_App.objFunc_ExtensionsPage );
			}
			else{
				btnP_Extensions.Opacity = 0.001;
				btnP_Extensions.Width = 10;
				frame_GNPX_Func.GoBack();
			}
			e.Handled = true;
		}





	// @@@@@ Event @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
	#region Event
        public void GNPX_Event_Handling_man( object sender, Gidoo_EventHandler e ){ 
			UPuzzle aPZL = pGPGC.GetCurrentPuzzle( );

			// ::::::::: In other thread control, "Dispatcher.Invoke" is need! ::::::::::

			switch( e.eName ){//			switch( e.eName.ToLower() ){
			
				case "Vibrate!": __bruMoveSub(); break;

				case "PuzzleFile_Clear":				
				case "PuzzleFile_Loaded": G60_Get_PreNxtPrg( 0, initialB:true ); break;


				case "SetPuzzleOnBoard": 
					aPZL = pGPGC.GetCurrentPuzzle( );
					G60_PB.Set_Puzzle( aPZL, setImage:true );	// @@@ v6
					
					this.Dispatcher.Invoke( () => Display_ExtResultWin( ) );
					break;


				case "PuzzleCreated":
					// Updates and displays of the main board (UC_PB_GBoard) 
					this.Dispatcher.Invoke(() => {
						aPZL = pGPGC.GetCurrentPuzzle( noX:int.MaxValue );
						G60_PB.Set_Puzzle( aPZL, setImage:true );
						G60_Get_PreNxtPrg( pm:0 );
						DIsplay_PuzzleList_Index( int.MaxValue );
					} );
					if( G6.ResultShow )  pGPGC.current_Puzzle_No = int.MaxValue;
					break;


				case "Solved":
					this.Dispatcher.Invoke( () => Display_ExtResultWin( ) );
					break;


				case "Show_AnalysisStatus":
					Clear_FinalState();

					G60_PB.Set_Puzzle( ePZL, setImage:true );
					this.Dispatcher.Invoke( () => {	
						int nn = pGPGC.current_Puzzle_No;
						DIsplay_PuzzleList_Index( nn );//   int.MaxValue );
						Display_ExtResultWin( );
					} );
				/*
					G6.g7CurrentState = ePZL.BOARD;
					if( GNPX_winSub!=null && GNPX_winSub.Visibility==Visibility.Visible ){
						btn_FinalState_Click( this, new RoutedEventArgs() );
						//btn_FinalState_Click( this, new RoutedEventArgs() );
					}
				*/
					break;

				case "G6_Changed":
					btn_FinalState.Visibility = G6.PowerUser? Visibility.Visible: Visibility.Hidden;	
					break;

				case "InvalidPuzzle":  /*_isValidPuzzle();*/ break;  //#@

				case "ShortMessage": shortMessage( e.gsm.Message, e.gsm.Pt, e.gsm.color, e.gsm.msec ); break;
				case "SystemError":  shortMessage( e.Message, new sysWin.Point(400,180), Colors.Orange, 3000 );	break;
//				default:			 shortMessage( "System error. Undefined event.", new sysWin.Point(100,200), Colors.DarkRed, 10000 ); break;
			}
			return;


		}

		private void Display_ExtResultWin( ){
            if( ePZL.extResult==null || ePZL.extResult.Length<2 ){
				if( ExtResultWin!=null && ExtResultWin.Visibility==Visibility.Visible ){
				    ExtResultWin.Visibility = Visibility.Collapsed;		//Close();    
                //    ExtResultWin = null;   
				}
				return;
			}
            else{        	
              //WriteLine( $"Display_ExtResultWin {DateTime.Now} {pPZL.extResult}" );    
                if( ExtResultWin != null ){
					ExtResultWin.Visibility = Visibility.Collapsed;//	Close();
					//ExtResultWin.Close(); ExtResultWin=null;
				}
                ExtResultWin = new ExtendResultWin(this);
                ExtResultWin.Visibility = Visibility.Visible;
				ExtResultWin.Width = this.Width;
				ExtResultWin.Left  = this.Left;
				ExtResultWin.Top   = this.Top+this.Height;

                ExtResultWin.Show();
			    ExtResultWin.SetText(ePZL.extResult);
            }
		
			G6.g7CurrentState = ePZL.BOARD;
		
			if( GNPX_winSub!=null && GNPX_winSub.Visibility==Visibility.Visible ){
				btn_FinalState_Click( this, new RoutedEventArgs() );
			}
		}	

	#endregion Event


		private void DIsplay_PuzzleList_Index(int nn0=int.MaxValue ){		
			int nn = pGPGC.current_Puzzle_No;	//   Min(nn,nnLast);
			var aPZL = pGPGC.GNPX_Puzzle_List[nn];

			this.Dispatcher.Invoke(() => {
				txt_G60PuzzleNo_EventStop = true;
				txt_G60PuzzleNo.Text = (nn+1).ToString();
				txt_G60PuzzleNo_EventStop = false;
				
				int nnLast = pGPGC.GNPX_Puzzle_List.Count;
				lbl_G60PuzzlesCount.Content = $"/ {nnLast}";
				txt_G60PuzzleName.Text = aPZL.Name;
			
				btn_G60ProbPre.IsEnabled = (nn>0);
				btn_G60ProbNxt.IsEnabled = true;
			} );
		}





	// @@@@@ Grid Left @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
	#region Grid Left 
		private bool txt_G60PuzzleNo_EventStop = false;
		private void btn_G60ProbPre_Click(object sender,RoutedEventArgs e){ G60_Get_PreNxtPrg(-1); }
        private void btn_G60ProbNxt_Click(object sender,RoutedEventArgs e){ G60_Get_PreNxtPrg(+1); }
		private void G60_Get_PreNxtPrg( int pm, bool initialB=false ){
			Clear_FinalState();

			bool chkDif = (bool)chb_MinDifficulty.IsChecked;
            int nn = pGPGC.current_Puzzle_No;

			bool breakB=false;
		  LNext:
			nn += pm;
			if( initialB ){ nn=0; initialB=false; }
            if( nn<0 || nn>pGPGC.GNPX_Puzzle_List.Count-1 ){
				nn = (pm<=0)? 0: pGPGC.GNPX_Puzzle_List.Count-1;
				breakB = true;
			}

            pGPGC.current_Puzzle_No = nn;
			UPuzzle aPZL = pGPGC.GetCurrentPuzzle();
			if( aPZL == null ) return;
			int Diff = aPZL.difficultyLevel;
			if( !breakB && chkDif && Diff<G6.G60_PuzzleMinDifficulty )  goto LNext;

/*
				Point pt = Mouse.GetPosition(btn_G60ProbNxt);
				if( pt.X > btn_G60ProbNxt.Width*0.7 ){
					WriteLine( $"pt:{pt}" );
					pGPGC.current_Puzzle_No = 9999999;
					G60_Get_PreNxtPrg(0);
					//Send_InitialState__to_Func_Solvers();
					return;
				}
*/
			G6.g7FinalState_TE = null;
			G6.g7CurrentState  = null;

			GNPX_App.GNPX_Eng.Set_NewPuzzle( aPZL );
            GNPX_App.GNPX_Eng.AnMan.ResetAnalysisResult(false); //Clear analysis result only

			G60_PB.Set_Puzzle( aPZL, setImage:true );

            GNPX_App.GNPX_Eng.AnalyzerCounterReset();
			
			txt_G60PuzzleNo_EventStop = true;

			txt_G60PuzzleNo_EventStop = false;

			lbl_G60PuzzlesCount.Content = $"/ {pGPGC.GNPX_Puzzle_List.Count}";
			txt_G60PuzzleName.Text = aPZL.Name;
			
			btn_G60ProbPre.IsEnabled = (nn>0);
            btn_G60ProbNxt.IsEnabled = (nn<pGPGC.GNPX_Puzzle_List.Count-1);

			// BroadcastToPages()
			Send_InitialState__to_Func_Solvers();
        }



		private void txt_G60PuzzleName_TextChanged(object sender, TextChangedEventArgs e) {
			if( txt_G60PuzzleName == null )  return;
            if( txt_G60PuzzleName.IsFocused) ePZL.Name=txt_G60PuzzleName.Text;
		}


		private void txt_G60PuzzleNo_PreviewKeyDown(object sender, KeyEventArgs e) {
			if( e.Key != Key.Return )  return;

			int nn = int.Parse( txt_G60PuzzleNo.Text );
			pGPGC.current_Puzzle_No = nn-1;
			G60_Get_PreNxtPrg(0);
		}

		private void nUD_MinDifficulty_ValueChanged(object sender, GIDOOEventArgs args) {
			G6.G60_PuzzleMinDifficulty = nUD_MinDifficulty.Value;
		}

	#endregion Grid Left 




	// @@@@@ Grid Right @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
	#region Page Processing Function	

		private List<Button>		pageControlList=null;
		private void btn_Page_Clicked(object sender, RoutedEventArgs e) {
			if( pageControlList == null ){
				pageControlList = ( List<Button>)Extension_Utility.GetControlsCollection<Button>(this);
				pageControlList = pageControlList.FindAll( P=> P.Name.Contains("btnP_") );
			}

			Button btn = (Button)sender;
			pageControlList.ForEach( P => P.Foreground=Brushes.White );
			btn.Foreground = Brushes.Gold;

			string btnName = btn.Name.Replace( "btnP_","");
			switch(btnName){ 
				case "File":       frame_GNPX_Func.Navigate( GNPX_App.objFunc_File ); break;
				case "Solve":      frame_GNPX_Func.Navigate( GNPX_App.objFunc_Solve ); break;
				case "Create":     frame_GNPX_Func.Navigate( GNPX_App.objFunc_Create ); break;
				case "Option":     frame_GNPX_Func.Navigate( GNPX_App.objFunc_Option ); break;
				case "Transform":  frame_GNPX_Func.Navigate( GNPX_App.objFunc_Transform ); break;
				case "HomePage":   frame_GNPX_Func.Navigate( GNPX_App.objFunc_HomePage ); break;
				case "Extensions": frame_GNPX_Func.Navigate( GNPX_App.objFunc_ExtensionsPage ); break;
			}
		}
		#endregion File Processing Function


	#region PreviewKeyDown
/*
		private void G60_PB_PreviewKeyDown(object sender, KeyEventArgs e) {
			WriteLine( "+++ G60_PB_PreviewKeyDown +++" );
			GNPXv6_PreviewKeyDown( sender, e );
		}
*/
		private void GNPXv6_PreviewKeyDown(object sender, KeyEventArgs e){
			//WriteLine( $"GNPXv6_PreviewKeyUp  G6.PG6Mode:{G6.PG6Mode}" );

            bool KeySft  = (Keyboard.Modifiers&ModifierKeys.Shift)>0;
            bool KeyCtrl = (Keyboard.Modifiers&ModifierKeys.Control)>0;

			Image imgPB_GBoard = G60_PB.imgPB_GBoard; 
            var (W,H) = (imgPB_GBoard.Width,imgPB_GBoard.Height);
			var Pt = Mouse.GetPosition(imgPB_GBoard);
            if( Pt.X<0 || Pt.Y<0 || Pt.X>=W || Pt.Y>=H )  return;



            if( e.Key==Key.C && KeyCtrl ){                // Ctrl+"c" => board -> Clipboard(81"n")
                string st = CopyToBuffer( KeySft:KeySft );
                try{
                    Clipboard.Clear();
                    Clipboard.SetData(DataFormats.Text, st);
					Thread.Sleep(100);
                }
                catch(System.Runtime.InteropServices.COMException){ /* NOP */ }
            }



            else if( e.Key==Key.F && KeyCtrl ){				// Ctrl+"f" => board -> Clipboard(9x9"n")
                string st = ToGridString(KeySft);   
                try{
                    Clipboard.Clear();
                    Clipboard.SetData(DataFormats.Text, st);
					Thread.Sleep(100);
                }
                catch(System.Runtime.InteropServices.COMException){ /* NOP */ }
            }



            else if( e.Key==Key.V && KeyCtrl ){				// Ctrl+"v" => Clipboard(any foramt) -> board
				//WriteLine( "@@@ e.Key==Key.V && KeyCtrl @@@" );

                string st=(string)Clipboard.GetData(DataFormats.Text);
                Clipboard.Clear();
                if( st==null || st.Length<81 ) return ;
                var aPZL= GNPX_App.SDK_ToUPuzzle( st, saveF:true ); 
                if( aPZL==null) return;
                pGPGC.current_Puzzle_No = int.MaxValue;			//pGPGC.GNPX_Puzzle_List.Count-1

				G60_PB.Set_Puzzle( aPZL, setImage:true );	//_SetPuzzleOnBoard();

				Send_InitialState__to_Func_Solvers();

				Thread.Sleep(100);
            }

			return;


						// ----------------------------------------------------------------------------------------------
						string CopyToBuffer( bool KeySft=false ){
							string st = "";
							if( KeySft ) st = ePZL.BOARD.ConvertAll(q=> Abs(q.No)).Connect("").Replace("0",".");
							else         st = ePZL.BOARD.ConvertAll(q=> Max(q.No,0)).Connect("").Replace("0",".");
							return st;
						}

						string ToGridString( bool SolSet ){
							string st="";
							ePZL.BOARD.ForEach( P =>{
								st+=(SolSet? P.No: Max(P.No,0));
								if( P.c==8 ) st+="\r";
								else if( P.rc!=80 ) st+=",";
							} );
							return st;
						}
		}
		#endregion PreviewKeyDown

		private void G60_PB_MouseUp(object sender, MouseButtonEventArgs e) {
			if( G6.PG6Mode != "Func_Transform" )  return;
			int rc = G6.Cell_rc;
			if( rc<0 || rc>80 ) return;
			Send_Command_to_Func_Transform( this, new Gidoo_EventHandler( "Cell_Clicked", ePara0:rc ) );
		}

		private void Send_InitialState__to_Func_Solvers(){
			Send_Command_to_Func_SolveMulti( this, new Gidoo_EventHandler( "ToInitialState" ) );
			Send_Command_to_Func_SolveSingle( this, new Gidoo_EventHandler( "ToInitialState" ) );
		}

		private void btn_CopyBitMap_Click(object sender, RoutedEventArgs e) {
			objFunc_Option.btn_CopyBitMap_Click(sender, e );
			objFunc_Option.btn_SaveBitMap_Click(sender, e );
        }


		private void chbx_ShowCandidate_Checked(object sender, RoutedEventArgs e ){
			if( chbx_ShowCandidate ==null ) return;
			G6.sNoAssist = (bool)chbx_ShowCandidate.IsChecked;
		}


		private bool disp_Solution_TE = false;

		public void Clear_FinalState(){
			//if( !G6.PowerUser )  return;
			disp_Solution_TE = false;
			if( GNPX_winSub != null )  GNPX_winSub.Visibility = Visibility.Collapsed;		//Close();   
		}
		private void btn_FinalState_Click(object sender, RoutedEventArgs e) {
			bool B = G6.PowerUser;
			if( !B )  return;

			if( ePZL.BOARD.All( uc => uc.No==0 ) )  return;

			B = disp_Solution_TE = !disp_Solution_TE;
			if( B ){
				List<UCell> BOARD_final = _Get_Solution();

				if( GNPX_winSub== null ){			
					GNPX_winSub = new( GNPX_App );
					GNPX_winSub.Visibility = Visibility.Visible;
					GNPX_winSub.Left  = this.Left-GNPX_winSub.Width-1;
					GNPX_winSub.Top   = this.Top+116;
				}

				GNPX_winSub.Set_imgPB_GBoard( BOARD_final, whiteBack:true );

				GNPX_winSub.Show();
			}
			else{
				GNPX_winSub.Visibility = Visibility.Collapsed;		//Close();    
			}

			
		//	GNPX_Event_Handling_man( this, new Gidoo_EventHandler( eName:"Show_AnalysisStatus" ) );
			return;
		
						// <<< Solve using "Research_trial". >>>
						List<UCell> _Get_Solution(){
						//	var Q = pGNPX_Eng.sol_int81;
							List<UCell>  board = null;
							if( G6.g7FinalState_TE != null )  board = G6.g7FinalState_TE;
							else{
								Research_trial RTrial = new( pAnMan );
								List<int> intBoard = ePZL.BOARD.ConvertAll( P=> P.No ); 
								bool ret = RTrial.TrialAndErrorApp( intBoard, filePutB:true, upperLimit:2 );	
								board = ret? RTrial.RT_Get_Board(): null;	
							}

				// ... G7 Development Plan ... Display of error location
							if( board!=null && G6.g7CurrentState!=null ){
								var BD = G6.g7FinalState_TE;
								board.ForEach( uc => board[uc.rc].ECrLst=null );
								foreach( var uc in G6.g7CurrentState.Where(uc=>uc.No<=0) ){
									UCell U = board[uc.rc];
									int   no0 = Abs(U.No);
									if( uc.No!=0 && Abs(uc.No)!=no0 ) U.Set_CellBKGColor( Colors.Bisque );
									if( uc.CancelB.IsHit(no0-1)  )    U.Set_CellBKGColor( Colors.PeachPuff );
								}
							}
							return board;
						}

		}

	}

}	