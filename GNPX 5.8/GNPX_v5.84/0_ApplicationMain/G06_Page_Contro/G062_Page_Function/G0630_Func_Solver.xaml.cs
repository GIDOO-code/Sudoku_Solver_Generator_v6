using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Linq;
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
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPX_space{
    /// <summary>
    /// Page1.xaml の相互作用ロジック
    /// </summary>
    public partial class Func_Solve: Page{
		static private	string		pageMode = "--";
		static public GNPX_App_Man 	pGNPX_App;
		static public GNPX_win		pGNP00win => pGNPX_App.pGNP00win;
        private GNPX_Engin			GNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine
		static private G6_Base		G6 => GNPX_App_Man.G6;
	
		public Page					objFunc_SolveSingle;	
		public Page					objFunc_SolveMulti;
		public Page					objFunc_SolveMethod;	 
		public Page					objFunc_SolveMethodOption;
		public Page					objFunc_SelectPuzzle;
		private List<Button>		pageControlList=null;


        public Func_Solve( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();
			
			objFunc_SolveSingle			 = new Func_SolveSingle( pGNPX_App );
			objFunc_SolveMulti			 = new Func_SolveMulti( pGNPX_App );	
			objFunc_SolveMethod			 = new Func_SolveMethod( pGNPX_App );
			objFunc_SolveMethodOption	 = new Func_SolveMethodOption( pGNPX_App );
			objFunc_SelectPuzzle = new Func_SelectPuzzle( pGNPX_App );

			btn_SolvePuzzleSelection.Opacity = 0.001;
        }
		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;

		  //When starting GNPX, be sure to open/close "Func_SolveMethod". This sets up the solver environment.
		    if( pageMode == "--" )  btn_Page_Clicked( btn_SolveMethod, e );
		}


		private void btn_Page_Clicked(object sender, RoutedEventArgs e ){
			if( pageControlList == null ){
				pageControlList = ( List<Button>)Extension_Utility.GetControlsCollection<Button>(this);
				pageControlList = pageControlList.FindAll( P=> P.Name.Contains("btn_Solve") );
			}

			Button btn = (Button)sender;
			if( btn.Opacity < 0.5 )  return;
			
			pageControlList.ForEach( P => P.Foreground=Brushes.White );
			btn.Foreground = Brushes.Gold;

			string btnName = btn.Name.Replace( "btn_","").Replace( "_Click","");
			switch(btnName){ 
				case "SolveSingle":		     frame_GNPX_Solve.Navigate( objFunc_SolveSingle );   break;
				case "SolveMulti":		     frame_GNPX_Solve.Navigate( objFunc_SolveMulti );    break;
				case "SolveMethod":		     frame_GNPX_Solve.Navigate( objFunc_SolveMethod );   break;
				case "SolveMethodOption":	 frame_GNPX_Solve.Navigate( objFunc_SolveMethodOption );    break;
				case "SolvePuzzleSelection": frame_GNPX_Solve.Navigate( objFunc_SelectPuzzle ); break;
			}
			pageMode = btnName;
        }



		private void btn_SolvePuzzleSelection_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
			btn_SolvePuzzleSelection.Opacity = 1.0;
		}

		private void btn_SolvePuzzleSelection_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			btn_SolvePuzzleSelection.Opacity = 1.0;
		}
	}
}
