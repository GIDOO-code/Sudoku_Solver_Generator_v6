using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using GIDOO_space;

namespace GNPX_space{
    public partial class GNPX_Setting_Editor: Window{
		static private G6_Base G6 => GNPX_App_Man.G6;

        private GNPX_win		pGNP00win;
		       
		private GNPX_App_Man    pGNPX_App;
		private string			pfName;

        public GNPX_Setting_Editor( GNPX_win pGNP00win ){
			this.pGNP00win = pGNP00win;
            pGNPX_App = pGNP00win.GNPX_App;
			InitializeComponent();		

            this.MouseLeftButtonDown += (sender, e) => this.DragMove();
        }

		private void devWin_Loaded(object sender, RoutedEventArgs e) {
			pfName = G6.Dir_SDK_Methods_XMLFileName;
			txb_Setting_FName.Text = pfName;
			// pGNPX_App.SolverList_App = pGNPX_App.ReadXMLFile_AnalysisConditions_MethodList();

			if( File.Exists( pfName ) ){
				string G6_test="", line;
				using( StreamReader rd = new StreamReader(pfName)){
					while( (line=rd.ReadLine()) != null ){ G6_test += line + "\n"; }
				}
				txbx_Setting.Text = G6_test;
			}
		}

		private void btn_Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

		private void btn_Set_Click(object sender, RoutedEventArgs e){
			string text = txbx_Setting.Text.Trim();
			var lines = text.Split("\n" ).ToList();
			using( StreamWriter wd = new StreamWriter(pfName)){
				foreach( var p in lines ){ wd.WriteLine(p); }
			}
			pGNPX_App.SolverList_App = pGNPX_App.ReadXMLFile_AnalysisConditions_MethodList();
		}

/*
		private void btn_SearchTerm_Click(object sender, RoutedEventArgs e) {
			string text = txbx_Setting.Text.Trim();
			var lines = text.Split("\n" ).ToList();
			string stSearch = txb_SearchTerm.Text;
			List<string> stEdit = lines.FindAll( p => p.Contains( stSearch ) ); 
			// ... under development ...

        }
*/
    }
}
