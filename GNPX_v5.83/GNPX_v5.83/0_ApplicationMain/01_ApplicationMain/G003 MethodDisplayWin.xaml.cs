using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using GIDOO_space;

namespace GNPX_space{
    public partial class MethodDisplayWin: Window{

        private GNPX_win         pGNP00win;
		public  GNPX_App_Man     pGNPX_App => pGNP00win.GNPX_App;

		private List<UAlgMethod> SolverList_App{ get{ return pGNPX_App.SolverList_App; } }  // List of algorithms to be applied.

        public MethodDisplayWin( GNPX_win pGNP00win ){
			this.pGNP00win = pGNP00win;
            InitializeComponent();		
        }
		private void Window_Loaded(object sender, RoutedEventArgs e){
			var wSolverList_App = pGNPX_App.SolverList_Base.FindAll( P=> P.IsChecked );
			foreach( var P in wSolverList_App ){
				if( P.Sel_Manual )  P.brsh = Brushes.White;
				else{ P.brsh = P.IsChecked? Brushes.Lime: Brushes.LightGray; }
			}
			
			listBox_GMethod00A.ItemsSource = wSolverList_App;
		} 
		private void Window_Unloaded(object sender, RoutedEventArgs e){ }


		private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)=> this.DragMove();

		private void btn_MethosWinClose_Click( object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;

    }
}
