using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Windows.Devices.Spi;
using System.Diagnostics.Metrics;

namespace GNPX_space{
	using pRes = Properties.Resources;
	using ioPath = System.IO.Path;
	using pGPGC = GNPX_Puzzle_Global_Control; 

    public partial class Func_SolveMethod: Page{
		static public event GNPX_EventHandler Send_GNPX_Control; 

		public GNPX_App_Man 		pGNPX_App;
		public GNPX_win				pGNP00win => pGNPX_App.pGNP00win;

		private G6_Base				G6 => GNPX_App_Man.G6;
		private List<UAlgMethod>    _SolverList_ = null;		//For internal use. After processing, copy to the external "List".

		private int					RecommendLevel_Initial;


	//	private string				Method_SelectionMode{ get=>G6.Method_SelectionMode; set=>G6.Method_SelectionMode=value; }
		private int					selIX = -1;
		private CheckBox			selCheckBox = null;
		private string				key_MethodName;

        public Func_SolveMethod( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();
			Send_GNPX_Control += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man ); 
			
			nud_Difficulty.Value = G6.Method_DifficultyLevel;
			UAlgMethod.diffLevel = G6.Method_DifficultyLevel;
			nud_Recommend.Value  = 5;
        }

	  #region <<< Page Load / Unload >>>
		private void Page_Loaded( object sender, RoutedEventArgs e ){ 
			G6.PG6Mode = GetType().Name;

			G6.OperationalMode = "Initializing";

			if( _SolverList_ == null ){	 // Only once during execution
				RecommendLevel_Initial = G6.RecommendLevel;

				// :::::::::::::::::::::::::::::::::::::::::::::::::::
				_SolverList_ = pGNPX_App.SolverList_Base.Copy();
				// :::::::::::::::::::::::::::::::::::::::::::::::::::

				UAlgMethod.diffLevel = G6.Method_DifficultyLevel;
				nud_Difficulty.Value = G6.Method_DifficultyLevel;
				nud_Recommend.Value  = G6.Method_RecommendLevel;

				listBox_GMethod00A.ItemsSource = _SolverList_;

				SetChange_qSolverList_page( );	

				var currentDir = Directory.GetCurrentDirectory();
				txtB_SDK_Methods.Text = Path.Combine(currentDir, pGNPX_App.SDK_Methods_XMLFileName);

				bool B = G6.GeneralLogic_on;
				string cul = Thread.CurrentThread.CurrentCulture.Name;
				string st2;
				st2= (B? "":"not ") + "available";
				lbl_GeneralLogic.Content = "GeneralLogic : "+ st2;
				lbl_GeneralLogic.Foreground = (B)? Brushes.LightBlue: Brushes.Yellow;
			}
			G6.OperationalMode = "NormalOperation";
		}


		private void Page_Unloaded(object sender, RoutedEventArgs e) {
			G6.RecommendLevel = nud_Recommend.Value;
			pGNPX_App.SolverList_Base = _SolverList_.Copy();					 // After processing, copy to the external "List".
			pGNPX_App.SolverList_App  = _SolverList_.FindAll( P=> P.IsChecked ); // <<< Copy to "SolverList_App" >>>

			pGNPX_App.WriteXMLFile_AnalysisConditions_MethodList();
		}
	  #endregion ------------------------------------------


		private void SetChange_qSolverList_page( ){
			_SolverList_.ForEach( P=> P.IsChecked=false );
			UAlgMethod PG = _SolverList_.Find( P => P.GenLogB );

			if( G6.Method_SelectionMode=="" || G6.Method_SelectionMode=="Difficulty" ){
					// <<< Difficulty >>>
				var _diffLevel = nud_Difficulty.Value;
				foreach( var P in _SolverList_ ){
					int dif = P.difficultyLevel;

					P.IsChecked = dif>0 && (dif<=_diffLevel);
					if( _diffLevel>=10 ) P.IsChecked = (dif<=_diffLevel);		//If it's 10 or more, include the negatives.
					if( P==PG && !G6.GeneralLogic_on ) PG.IsChecked = false;
					P.brsh = P.IsChecked? Brushes.Lime: Brushes.LightGray;

					if( P.Sel_Manual && !P.IsChecked ){ P.IsChecked=true;; P.brsh=Brushes.White; }
				}
			}
			else{	// <<< Recommend >>>
				var _recommendLevel = nud_Recommend.Value;
					//WriteLine( $" recommendVal:{_recommendLevel}" );

				foreach( var P in _SolverList_ ){
					P.IsChecked = (P.recommendLevel<=_recommendLevel);
					if( P==PG && !G6.GeneralLogic_on ) PG.IsChecked = false;
					P.brsh = P.IsChecked? Brushes.Aqua: Brushes.LightGray;
					if( P.Sel_Manual && !P.IsChecked ){ P.IsChecked=true;; P.brsh=Brushes.White; }
				}
			}

			UAlgMethod QGenLog = _SolverList_.Find( P => P.MethodName.Contains("GeneralLogic") );
			if( QGenLog != null )  QGenLog.IsChecked  = G6.GeneralLogic_on;  // Enable/disable GeneralLogic

			listBox_GMethod00A.ItemsSource = null;
			listBox_GMethod00A.ItemsSource = _SolverList_;
		}



		// ----- <<< Up / Down >> -----
        private void rbtn_GMethod01U_Click( object sender, RoutedEventArgs e ){
			selIX = listBox_GMethod00A.SelectedIndex;
            if( selIX<=3 || selIX==0 )  return;
			
			var Q = (UAlgMethod)listBox_GMethod00A.SelectedItem;
			_ChangeOrder_MethodList(selIX,-1);

			SetChange_qSolverList_page( );
			selIX = _SolverList_.FindIndex( q=> q.NameM==Q.NameM );
			listBox_GMethod00A.SelectedIndex = selIX;
        }
        private void rbtn_GMethod01D_Click( object sender, RoutedEventArgs e ){
			selIX = listBox_GMethod00A.SelectedIndex;
            if( selIX<=3 || selIX==listBox_GMethod00A.Items.Count-1 )  return;

			var Q = (UAlgMethod)listBox_GMethod00A.SelectedItem;
			_ChangeOrder_MethodList(selIX,+1);

			SetChange_qSolverList_page( );
			selIX = _SolverList_.FindIndex( q=> q.NameM==Q.NameM );
			listBox_GMethod00A.SelectedIndex = selIX;
        }

        public  void _ChangeOrder_MethodList( int nx, int UD ){
            UAlgMethod MA=_SolverList_[nx], MB;
            if(UD<0){ MB=_SolverList_[nx-1]; _SolverList_[nx-1]=MA; _SolverList_[nx]=MB; }   
            if(UD>0){ MB=_SolverList_[nx+1]; _SolverList_[nx+1]=MA; _SolverList_[nx]=MB; }
        }




		// ----- <<< DifficultyUp / Recommend >> -----
		private void nud_Difficulty_ValueChanged(object sender, GIDOOEventArgs args) {
			if( nud_Difficulty==null || _SolverList_==null )  return;
			G6.Method_DifficultyLevel = nud_Difficulty.Value;
			if( G6.OperationalMode == "NormalOperation" )  G6.Method_SelectionMode = "Difficulty";
			SetChange_qSolverList_page( );
		}
		private void nud_Recommend_ValueChanged(object sender, GIDOOEventArgs args) {
			if( nud_Recommend==null || _SolverList_==null )  return;
			G6.Method_RecommendLevel = nud_Recommend.Value;
			if( G6.OperationalMode == "NormalOperation" )  G6.Method_SelectionMode = "Recommend";
			SetChange_qSolverList_page( );
        }



		// ----- <<< Reverting to the start of this page. >> -----
		private void btn_Initial_Click(object sender, RoutedEventArgs e) {
			nud_Recommend.Value = RecommendLevel_Initial;

			// <<< Copy form SolverList_Def >>>
			_SolverList_  = pGNPX_App.SolverList_Def.Copy();	
			_SolverList_.ForEach( P=> P.Sel_Manual=false );
			
			if( (bool)chb_SortByDifficulty.IsChecked ){
				_SolverList_.Sort( (a,b)=> (a.difficultyLevel-b.difficultyLevel) );
			}

			G6.Method_SelectionMode = "Difficulty";
			UAlgMethod.diffLevel = G6.Method_DifficultyLevel;
			nud_Difficulty.Value = G6.Method_DifficultyLevel = 5;
			nud_Recommend.Value  = G6.Method_RecommendLevel =  5;
			listBox_GMethod00A.ItemsSource = null;
			listBox_GMethod00A.ItemsSource = _SolverList_;
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e) {
			var P = (CheckBox)sender;
			var PContent = (String)P.Content;

			var Q = _SolverList_.Find( q=> q.NameM==PContent ); 
			if( Q != null )  Q.IsChecked = (bool)P.IsChecked;		//If it's 10 or more, include the negatives.
		}
	}
}
