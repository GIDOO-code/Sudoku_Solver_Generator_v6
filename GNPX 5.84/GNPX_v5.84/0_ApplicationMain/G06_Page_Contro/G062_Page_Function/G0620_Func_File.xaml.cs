using GNPX_space;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Threading;



namespace GNPX_space{
    using pRes=Properties.Resources;
	using pGPGC = GNPX_Puzzle_Global_Control;
	using sysWin=System.Windows;

    public partial class Func_File: Page{
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 

		static public GNPX_App_Man 	pGNPX_App;
		static private G6_Base		G6 => GNPX_App_Man.G6;
		static public GNPX_win		pGNP00win => pGNPX_App.pGNP00win;
        private GNPX_Engin			GNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine
	    private PuzzleFile_IO		fPuzzleFile_IO => pGNPX_App.PuzzleFile_IO;
		public UPuzzle				ePZL{ get=>GNPX_Eng.ePZL; set=>GNPX_Eng.ePZL=value; } // Puzzle to analyze

		[DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]		// ... Required for SetCursorPos
        static private extern void SetCursorPos(int X,int Y);							//Move the mouse cursor to Control

		private DispatcherTimer    timerShortMessage;


		private int CountDownCC;
        public Func_File( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();

			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  

			timerShortMessage = new DispatcherTimer(DispatcherPriority.Normal);
			timerShortMessage.Interval = TimeSpan.FromMilliseconds(1000);
			timerShortMessage.Tick += new EventHandler(timerShortMessage_Tick);
        }
	    private void timerShortMessage_Tick( object sender, EventArgs e ){
			if( --CountDownCC > 0 ){
				lbl_ShortMessage.Visibility = Visibility.Visible;
				if( CountDownCC > 1 )lbl_ShortMessage.Content = $"Realy? ... {CountDownCC}";
				else lbl_ShortMessage.Content = $"Cancelled";
				return;
			}

			else{
				lbl_ShortMessage.Content = "";
				lbl_ShortMessage.Visibility = Visibility.Hidden;
				timerShortMessage.Stop();
				GradientStop2.Color = Colors.DarkBlue;
			}
        }

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;
         
		  // <<< Move the mouse cursor to Button:btn_OpenPuzzleFile >>>
			this.Cursor = Cursors.Arrow;
            var btnQ = btn_OpenPuzzleFile;                    
			var ptM  = new Point( btnQ.Margin.Left, btnQ.Margin.Top );  // Center coordinates
            var pt   = btn_OpenPuzzleFile.PointToScreen(ptM);			// Grid relative coordinates to screen coordinates.
            SetCursorPos((int)pt.X,(int)pt.Y);							// Move the mouse cursor
			
			txb_AppDefFilePath.Text = G6.Dir_SDK_Methods_XMLFileName;

			var PzlCC = pGPGC.GNPX_Puzzle_List_Count;
			if( PzlCC>0 )	btn_Initialize_PuzzleFile.Visibility = Visibility.Visible;
			else			btn_Initialize_PuzzleFile.Visibility = Visibility.Hidden;
        }

		private void btn_OpenPuzzleFile_Click(object sender, RoutedEventArgs e) {
			btn_OpenPuzzleFile.Width = 100;
			btn_OpenPuzzleFile.Height = 25;
			btn_OpenPuzzleFile.FontSize = 12;
			GradientStop00.Color = Colors.Black;	

		    var openFile = new OpenFileDialog{
				Multiselect = false,
				Title  = pRes.filePuzzleFile,
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
			};

            if( (bool)openFile.ShowDialog() ){
                string fName = openFile.FileName;
				List<UPuzzle> puzzleList = fPuzzleFile_IO.GNPX_PuzzleFile_Read( fName );
				txtFileName.Text = fName;

				if( puzzleList!=null && puzzleList.Count>0 ){
					pGPGC.Set_GNPX_Puzzle_List( puzzleList );

					if( puzzleList!=null && puzzleList.Count>0 ){
						// Notify the main that the puzzle file has been input and request preparation for analysis.
						Gidoo_EventHandler se = new Gidoo_EventHandler( eName:"PuzzleFile_Loaded" );  //Report
						Send_Command_to_GNPXwin( this, se );

						btn_Initialize_PuzzleFile.Visibility = Visibility.Visible;
						G6.GNPX_PuzzleFileName = fName;
					}
				}
			}
        }

		private void btn_Initialize_PuzzleFile_Click(object sender, RoutedEventArgs e) {
			if( pGPGC.IsEmpty  )  return;
			if( GradientStop2.Color==Colors.DarkBlue ){
				GradientStop2.Color = Colors.DarkRed;
				timerShortMessage.Start();
				CountDownCC = 6;
				return;
			}
			if( GradientStop2.Color==Colors.DarkRed ){
				lbl_ShortMessage.Content = "";
				lbl_ShortMessage.Visibility = Visibility.Hidden;
				timerShortMessage.Stop();
				CountDownCC = 0;

				pGPGC.Clear_GNPX_Puzzle_List( );
				txtFileName.Text = "";
				btn_Initialize_PuzzleFile.Visibility = Visibility.Hidden;
				Gidoo_EventHandler se = new Gidoo_EventHandler( eName:"PuzzleFile_Loaded" );  //Report
				Send_Command_to_GNPXwin( this, se );
			}
			GradientStop2.Color = Colors.DarkBlue;
        }



		private void btn_SavePuzzle_Click(object sender, RoutedEventArgs e) {
            if( pGPGC.GNPX_Puzzle_List.Count==0 )  return;
		    var saveFile = new SaveFileDialog{
				Title  = pRes.filePuzzleFile,
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
			};

            if( (bool)saveFile.ShowDialog() ){
                string fName = saveFile.FileName;

				bool append  = (bool)chb_Additional_Save.IsChecked;
				bool fType81 = (bool)chb_File81Nocsv.IsChecked;		// For ver.5.0-, the file format is only 81 digits.
				bool blank9  = (bool)chb_File81Plus.IsChecked;
				bool SolSort = (bool)chb_SolutionSort.IsChecked;
				bool SolSet  = (bool)cbx_ProbSolSetOutput.IsChecked;
				bool SolSet2 = (bool)chb_AddAlgorithmList.IsChecked;
				
				if( ePZL.BOARD.Any(p=>p.No!=0) ){
					ePZL.ID = pGPGC.GNPX_Puzzle_List.Count;

					pGPGC.GNPX_Puzzle_List_Add(ePZL);
//					pGPGC.current_Puzzle_No = 0;
				}

				fPuzzleFile_IO.GNPX_PuzzleFile_Write( fName, append, fType81, SolSort, SolSet, SolSet2, blank9 );
			}		
		}

		private void btn_SaveToFavorites_Click(object sender, RoutedEventArgs e) {
            bool SolSort = (bool)chb_SolutionSort.IsChecked;
            bool SolSet  = (bool)cbx_ProbSolSetOutput.IsChecked;    
            pGNPX_App.btn_FavoriteFileOutput(true,SolSet:SolSet,SolSet2:true);
		}


    }
}