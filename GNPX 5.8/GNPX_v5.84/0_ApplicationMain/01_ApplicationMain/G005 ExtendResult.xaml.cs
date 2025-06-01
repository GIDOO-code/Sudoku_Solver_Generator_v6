using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GIDOO_space;

namespace GNPX_space{
    public partial class ExtendResultWin: Window{
		static private G6_Base G6 => GNPX_App_Man.G6;

        private GNPX_win pGNP00win;

        public ExtendResultWin( GNPX_win pGNP00win ){
			this.pGNP00win = pGNP00win;
            InitializeComponent();		
            devWin.Width = pGNP00win.Width;
            GNPXGNPX.Content = App_Environment.GNPXvX;
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();
        }

        private void btn_devWinClose_Click(object sender, RoutedEventArgs e){
            this.Hide();
        }

        public void SetText( string res ){
            ExtRes.Text=res;
        }

		private void chb_LinkedMove_Checked(object sender, RoutedEventArgs e) {
			if( chb_LinkedMove ==null )  return;
			G6.LinkedMove = (bool)chb_LinkedMove.IsChecked;
        }
    }
}
