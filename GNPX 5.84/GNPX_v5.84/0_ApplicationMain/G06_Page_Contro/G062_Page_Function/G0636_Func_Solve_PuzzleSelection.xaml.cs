using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Diagnostics.Debug;
using static System.Math;

using Microsoft.Win32;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Threading;
using System.Linq;
using System.Text;

using GIDOO_space;
using System.Net.Sockets;
using System.Xml.Linq;

namespace GNPX_space{
	using pRes = Properties.Resources;
	using ioPath = System.IO.Path;
	using pGPGC = GNPX_Puzzle_Global_Control;

    public partial class Func_SelectPuzzle: Page{  // only for Power User
		static public bool  EventSuppression=false;
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 

		public GNPX_App_Man 		pGNPX_App;
		public GNPX_win				pGNP00win => pGNPX_App.pGNP00win;

		public GNPX_App_Ctrl        App_Ctrl => pGNPX_App.App_Ctrl;
		private G6_Base				G6 => GNPX_App_Man.G6;
        private GNPX_Engin			pGNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine

		public UPuzzle				ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; } // Puzzle to analyze
		private int                 stageNo   => (ePZL is null)? 0: ePZL.stageNo;
		private GNPX_Graphics		gnpxGrp => pGNPX_App.gnpxGrp;

	    private PuzzleFile_IO		fPuzzleFile_IO => pGNPX_App.PuzzleFile_IO;
		private List<UPuzzle>		pGNPX_PUZZLE_List => pGPGC.GNPX_Puzzle_List;

	    private List<UAlgMethod>    SolverList_Dev;
		private List<UAlgMethod>    SolverList_App_Save;

		private DispatcherTimer     displayTimer;
		private object				obj = new();


		
		private string  sortMode = "ID";
		private string  dirStr   = "AutoGen_Puzzles";
		private string  fName_MethodList = "MethodList.txt"; 

        public  CancellationTokenSource cts;
		private Stopwatch			AnalyzerLap = new();
		private string				Solving_Message;




		public Func_SelectPuzzle( GNPX_App_Man pGNPX_App ){
			this.pGNPX_App = pGNPX_App;

            InitializeComponent();
			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  

			displayTimer = new DispatcherTimer( DispatcherPriority.Normal, this.Dispatcher );
            displayTimer.Interval = TimeSpan.FromMilliseconds(100);
            displayTimer.Tick += new EventHandler(displayTimer_Tick);
        }

        private void displayTimer_Tick( object sender, EventArgs e){
			lbl_analyzerLap.Content = $"Lap : {AnalyzerLap.Elapsed.TimespanToString()}";	// ToString(); .//AnalyzerLapElaped;
			txblk_AnalyzerResult.Text = Solving_Message;
			
			lock(obj){
				var MTHD = GNPX_Engin.AnalyzingMethod;
				if( MTHD != null )  Lbl_onAnalyzing.Content = $"step: {stageNo:2} {MTHD.MethodName}";
			}
        }




		// <<< Load / Uncload >>>		
		private void Page_Loaded(object sender, RoutedEventArgs e) {
			if( numUD_DifficultyLevel == null ) return;
			G6.PG6Mode = GetType().Name;
			numUD_DifficultyLevel.Value = G6.Method_DifficultyLevel;

			SolverList_App_Save = pGNPX_App.SolverList_App;
			SolverList_Dev = pGNPX_App.SolverList_Base.Copy(); 

			UAlgMethod QGenLog = SolverList_Dev.Find( P => P.MethodName.Contains("GeneralLogic") );
			if( QGenLog != null ) SolverList_Dev.Remove(QGenLog);	// GeneralLogic excluded

			btn_ResetMark_Click( sender, e );

			txt_FileName.Text = G6.GNPX_PuzzleFileName;
			btn_SolvePuzzle.IsEnabled =false;
        }

		private void Page_Unloaded(object sender, RoutedEventArgs e) {
			pGNPX_App.SolverList_App = SolverList_App_Save;
		}




		// <<< Puzzle File Read >>>
		private void btn_OpenPuzzleFile_Click(object sender, RoutedEventArgs e) {
            var OpenFDlog = new OpenFileDialog(){
				Multiselect = false,
				Title  = pRes.filePuzzleFile,
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
			};
			
			if( (bool)OpenFDlog.ShowDialog() ){
                string fName = OpenFDlog.FileName;
				var PZL_List = fPuzzleFile_IO.GNPX_PuzzleFile_Read( fName );
				pGPGC.Set_GNPX_Puzzle_List( PZL_List ); 

				if( pGPGC.GNPX_Puzzle_List!=null && pGPGC.GNPX_Puzzle_List.Count>0 ){
					Send_Command_to_GNPXwin( this, new Gidoo_EventHandler( eName:"PuzzleFile_Loaded" ) );
					txt_FileName.Text = fName;
					G6.GNPX_PuzzleFileName = fName;
				}
			}
		}

		void __Method_CheckMark( bool sel ){
			WriteLine( $"*SolverList_Dev : {SolverList_Dev.Count} methods" );
			foreach( var (mthd,sq) in SolverList_Dev.WithIndex() ){
				if( sel && (mthd.markA_dev is false) )  continue;
				WriteLine( $"sq:{sq,2} {mthd.NameM.PadRight(30)}  markA_dev:{(mthd.markA_dev? "on":"")}" );
			}
		}


		// <<< Solve and Marking : Marking Puzzles using a specified algorithm>>>
		private async void btn_SolvePuzzle_Click(object sender, RoutedEventArgs e) {
			cts = new CancellationTokenSource();

			if( (string)btn_SolvePuzzle.Content == "Suspend" ){
				cts.Cancel();
				btn_SolvePuzzle.Content = "Solve and Marking";
				return;
			}
				//	__Method_CheckMark( sel:false );


			if( pGNPX_PUZZLE_List==null || pGNPX_PUZZLE_List.Count<=0 ){
				Lbl_onAnalyzing.Content = "Puzzle file not entered";
				Lbl_onAnalyzing.Foreground = Brushes.Pink;
				displayTimer.Stop();
				AnalyzerLap.Stop();
				return;
			}

			if( SolverList_Dev.All(p => !p.markA_dev) ){
				txblk_AnalyzerResult.Text = "Any Algorithm is not marked.";
				txblk_AnalyzerResult.Foreground = Brushes.DarkOrange;
				return;
			}

			bool MSelPzlB = G6.PG6Mode=="Func_SelectPuzzle";

		    //region Save standard conditions. Set conditions for development
			int tmpMSlvrMaxAlgorithm = G6.MSlvr_MaxNoAlgorithm;
			int tmpMSlvrMaxTime		 = G6.MSlvr_MaxTime;

			G6.MSlvr_MaxNoAlgorithm  = 2;
			G6.MSlvr_MaxTime         = 2;
			btn_SolvePuzzle.Content  = "Suspend";

			btn_ClearMark.IsEnabled  = false;

			GNPX_Engin.MltAnsSearch  = true;
			GNPX_Engin.SolInfoB      = true;

			// ::::::::::::::::::::::::::::::::::::::::::::
			int difLvl = numUD_DifficultyLevel.Value;
			SolverList_Dev.RemoveAll( p => p.difficultyLevel>difLvl && !p.markA_dev);
			pGNPX_App.SolverList_App = SolverList_Dev;
			// ::::::::::::::::::::::::::::::::::::::::::::

			displayTimer.Start();
			AnalyzerLap.Start();

			int n=0, m=0, mE=0;
			foreach( var (aPZL,sqPZL) in pGNPX_PUZZLE_List.WithIndex() ){
				if( cts.IsCancellationRequested ){ break; }

				pGNPX_Eng.ePZL = aPZL;
				foreach( var p in aPZL.BOARD )  if( p.No<0 ) p.No=0;

				{
					// <1> Show target puzzle
					string st0 = string.Join("",aPZL.BOARD.ConvertAll(p=>Max(p.No,0))).Replace("0",".");
								//WriteLine( $"btn_SolvePuzzle_Click st0:{st0}" );

					// <2> Solve Puzzle. Verify that its Sudoku Puzzle.
					var ret = pGNPX_Eng.IsSudokuPuzzle_TE_Check( debugPrint:false );

					if( !ret ){  WriteLine( $" System Error. ... btn_SolvePuzzle_Click" ); continue; }

					// <3> Set Puzzle(aPZL) to Solver-Engin.
					pGNPX_Eng.Set_NewPuzzle( aPZL );

					// <4> Initialize Engin.
					pGNPX_Eng.AnMan.Update_CellsState( aPZL.BOARD );
					pGNPX_Eng.AnalyzerCounterReset();

					// <5> Initialize search flag.
					G6.g7MarkA0 = false;
					G6.g7MarkA_MLst0 = new();

					G6.MSlvr_MaxNoAlgorithm = 1;
					G6.MSlvr_MaxNoAllAlgorithm = 100;
					G6.MSlvr_MaxTime = 500;				 // Time allowed per algorithm (ms)

					pGNPX_Eng.ReturnToInitial();

				    // <6> Engine start
					await Task.Run(() => pGNPX_Eng.GNPX_Solver_SolveUp(cts.Token) );
				}

				aPZL.g7MarkA = G6.g7MarkA0;
				aPZL.g7MarkA_Msg = string.Join(" ",G6.g7MarkA_MLst0 );
				if( MSelPzlB && aPZL.g7MarkA ){
					if( aPZL.Sol_Result.Contains("error") ){ mE++; aPZL.g7Error=true; }
					else{ m++; }
				}

				this.Dispatcher.Invoke( ()=> { Solving_Message = $"\n Succeed : {m}\n  Failed : {mE}\n  Parsed : {++n} / {pGNPX_PUZZLE_List.Count}"; } );
				//Solving_Message = $"\n Succeed : {m}    Parsed : {++n} / {pGNPX_PUZZLE_List.Count}";
				btn_ClearMark.IsEnabled = true;
			}

			await Task.Delay(200);
			btn_SolvePuzzle.Content="Solve and Marking";

			AnalyzerLap.Stop();
			displayTimer.Stop();

			// Recover standard conditions.
            G6.MSlvr_MaxNoAlgorithm  = tmpMSlvrMaxAlgorithm;
            G6.MSlvr_MaxTime         = tmpMSlvrMaxTime;

			btn_SavePuzzle.IsEnabled =  GNPX_Puzzle_Global_Control.GNPX_Puzzle_List.Count( P=> P.g7MarkA ) > 0;  //　This operation is allowed only for PU
		
					void __Method_CheckMark( bool sel ){
						WriteLine( $"*SolverList_Dev : {SolverList_Dev.Count} methods" );
						foreach( var (mthd,sq) in SolverList_Dev.WithIndex() ){
							if( sel && (mthd.markA_dev is false) )  continue;
							WriteLine( $"sq:{sq,2} {mthd.NameM.PadRight(30)}  markA_dev:{(mthd.markA_dev? "on":"")}" );
						}
					}	
		}

		private void btn_ClearMark_Click(object sender, RoutedEventArgs e) {
			pGNPX_PUZZLE_List.ForEach( P=> {P.g7MarkA=false; P.g7Error=false;} );
			this.Dispatcher.Invoke( ()=> Solving_Message = "" );
		}



	#region <<< Algorithm Selection >>> 
		private void Set_Color(object sender, RoutedEventArgs e){
			//_restoration();
			__Develop_FileMan();
		}
		private void CheckBox_Checked(object sender, RoutedEventArgs e){
			var X = (CheckBox)sender;
			string stX = ((string)X.Content).Substring(3);

			//stX = stX.Substring(3);
			var Q = SolverList_Dev.Find(p=>p.MethodName==stX);
			if( Q is null ){ 
				Send_Command_to_GNPXwin( this, new Gidoo_EventHandler( eName:"Method is inactive" ) );
				Lbl_onAnalyzing.Content = "Method is inactive";
				Lbl_onAnalyzing.Foreground = Brushes.Pink;
				displayTimer.Stop();
				return;
			}
			int Method_DifficultyLevel = 999;
			if( X!=null && X is CheckBox ){
				Q.markA_dev = (bool)X.IsChecked;
				Set_Color( new object(), null );
			}

			if( SolverList_Dev.Any(p => p.markA_dev) ){
				txblk_AnalyzerResult.Text = "";
				txblk_AnalyzerResult.Foreground = Brushes.White;
			}

			int mcc = SolverList_Dev.Count(p => p.markA_dev);
			if( mcc > 0 ){
				lbl_Selected_Method.Content    = $"Selected : {mcc}";
				lbl_Selected_Method.Foreground = Brushes.SkyBlue;
			}
			else{
				lbl_Selected_Method.Content    = "Specify the Algorithm" ;
				lbl_Selected_Method.Foreground = Brushes.Orange;
			}
			btn_SolvePuzzle.IsEnabled = (mcc>0 && pGNPX_PUZZLE_List.Count>0);
		}

		private void _restoration(){
			object obj = new object();
			//btn_SortBy_Click( obj,null );

			switch(sortMode){
				case "ID":		btn_SortByID_Click( obj, null ); break;
				case "Name":	btn_SortByName_Click( obj, null ); break;
				case "Diff":	btn_SortByDifficulty_Click( obj, null ); break;
				case "Marked":	btn_SortByMarked_Click( obj, null); break;
			}
		}
		private void __Develop_FileMan(){
			int DifLevel = 999;	//numUD_DifficultyLevel.Value; #@

			if( !Directory.Exists(dirStr) ){ Directory.CreateDirectory(dirStr); }
			using( var fpW=new StreamWriter( dirStr+@"\"+fName_MethodList, append:false, Encoding.UTF8) ){   
				fpW.WriteLine(sortMode);
				fpW.WriteLine( $"Difficulty_Level {DifLevel}" );
				foreach( var P in SolverList_Dev.Where(p=> p.markA_dev) ){
					fpW.WriteLine(P.MethodName);
				}
			}
		}
	
	  #region <<< Check conditions >>>
		private void btn_SortByID_Click(object sender, RoutedEventArgs e) {	
			SolverList_Dev.Sort( (a,b)=> (a.ID - b.ID) );
			lsB_GMethod00B.ItemsSource = null;
			lsB_GMethod00B.ItemsSource = SolverList_Dev;
			sortMode="ID";
			//Set_Color( sender, e );
		}
		private void btn_SortByName_Click(object sender, RoutedEventArgs e) {
			SolverList_Dev.Sort( (a,b)=> (a.MethodName.CompareTo(b.MethodName)) );
			lsB_GMethod00B.ItemsSource = null;
			lsB_GMethod00B.ItemsSource = SolverList_Dev;
			sortMode="Name";
			//Set_Color( sender, e );
		}
		private void btn_SortByDifficulty_Click(object sender, RoutedEventArgs e) {
			SolverList_Dev.Sort( (a,b)=> (Abs(a.difficultyLevel) - Abs(b.difficultyLevel)) );
			lsB_GMethod00B.ItemsSource = null;
			lsB_GMethod00B.ItemsSource = SolverList_Dev;
			sortMode="Diff";
			//Set_Color( sender, e );
		}
		private void btn_SortByMarked_Click(object sender, RoutedEventArgs e) {
			SolverList_Dev.Sort( (a,b)=> (a.sortKey - b.sortKey) );
			lsB_GMethod00B.ItemsSource = null;
			lsB_GMethod00B.ItemsSource = SolverList_Dev;
			sortMode="Marked";
			//Set_Color( sender, e );
		}

		private void btn_ResetMark_Click(object sender, RoutedEventArgs e) {
			if( numUD_DifficultyLevel==null || SolverList_Dev==null )  return;

			int difLvl = numUD_DifficultyLevel.Value;
			UAlgMethod.diffLevel = difLvl;

			SolverList_Dev.ForEach( p => p.markA_dev=false );

			lsB_GMethod00B.ItemsSource = null;
			lsB_GMethod00B.ItemsSource = SolverList_Dev;
			sortMode="Reset";
			//Set_Color( sender, e );
		}
		private void numUD_DifficultyLevel_ValueChanged(object sender, GIDOOEventArgs args) {
			if( numUD_DifficultyLevel == null )  return;
			btn_ResetMark_Click( sender, new RoutedEventArgs() ); 
		}
	  #endregion << Check conditions >>>

	#endregion <<< Algorithm Selection >>> 


		private void btn_SavePuzzle_Click(object sender, RoutedEventArgs e) {
			var PZLs = GNPX_Puzzle_Global_Control.GNPX_Puzzle_List.FindAll( P=> P.g7MarkA );  //　This operation is allowed only for PU
			
			// Selected Puzzles
			string fName = txt_FileName.Text;
			if( fName != "File(Puzzle DB) path" )  fName = G6.GNPX_PuzzleFileName;
			int np = fName.LastIndexOf(".");
			string fNameS = fName.Substring(0,np) + "Sel.txt";
			GNPX_PuzzleFile_Write( fNameS, PZLs, append:true, blank9:false );

			// Puzzles with errors
			var errorPZLs = PZLs.FindAll( P=> P.g7Error );
			if( errorPZLs.Count > 0 ) {
				string  fNameE = fName.Substring(0,np) + "SelError.txt";
				GNPX_PuzzleFile_Write( fNameE, errorPZLs, append:true, blank9:false );
			}
		}

		private void GNPX_PuzzleFile_Write( string fName, List<UPuzzle> PZLs, bool append=true, bool blank9=false ){
            if( pGPGC.GNPX_Puzzle_List.Count==0 )  return;

            using( StreamWriter fpW=new StreamWriter(fName,append,Encoding.UTF8) ){
				int IDsq=0;
                foreach( var P in PZLs ){
                     string line = ""; 
                     P.BOARD.ForEach( q => {
                         line += Max(q.No,0).ToString();
                         if( blank9 && q.c==8 )  line += " ";        //Supports the format "Contain a blank every 9 digits"
						 } );
                     line = line.Replace("0",".");
					 line += $" {P.difficultyLevel} \"ID:{++IDsq,0000} {P.Name}\"";
                     fpW.WriteLine(line);
                }
            }
		}

	}	
}
