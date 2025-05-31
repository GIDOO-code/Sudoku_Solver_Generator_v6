using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Diagnostics.Debug;

namespace GNPX_space{
    using pRes=Properties.Resources;
	using pGPGC = GNPX_Puzzle_Global_Control;
    public partial class Func_Extensions: Page{
		static private	string		pageMode = "--";

		static public GNPX_App_Man 	pGNPX_App;
		static private G6_Base		G6 => GNPX_App_Man.G6;
        private GNPX_Engin			GNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine



		public Page					objFunc_DevelopWHS;
		public Page					objFunc_DevelopStatistics;	

		private List<Button>		pageControlList=null;

		public object Culture{ get=> pRes.Culture; }



        public Func_Extensions( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();

			objFunc_DevelopWHS = new Func_DevelopWHS( pGNPX_App );
        }

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;
		    if( pageMode == "--" )  btn_Page_Clicked( btn_DevelopWHS, e );
		}



		private void btn_Page_Clicked(object sender, RoutedEventArgs e) {
			if( pageControlList == null ){
				pageControlList = ( List<Button>)Extension_Utility.GetControlsCollection<Button>(this);
				pageControlList = pageControlList.FindAll( P=> P.Name.Contains("btn_Develop") );
			}

			Button btn = (Button)sender;
			if( btn.Opacity < 0.5 )  return;
			
			pageControlList.ForEach( P => P.Foreground=Brushes.White );
			btn.Foreground = Brushes.Gold;

			string btnName = btn.Name.Replace( "btn_","").Replace( "_Click","");
			switch(btnName){ 
				case "DevelopWHS":		     frame_GNPX_Solve.Navigate( objFunc_DevelopWHS );   break;
				case "DevelopStatictics":    frame_GNPX_Solve.Navigate( objFunc_DevelopStatistics );   break;
			}
			pageMode = btnName;
		}
	}
}
