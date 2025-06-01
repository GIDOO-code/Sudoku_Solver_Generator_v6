using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Diagnostics.Debug;
using static System.Math;

namespace GNPX_space{
    using pRes=Properties.Resources;
	using pGPGC = GNPX_Puzzle_Global_Control;
    public partial class Func_DevelopStatistics: Page{
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 


        private GNPX_AnalyzerMan    pAnMan => GNPX_Eng.AnMan;

		static public GNPX_App_Man 	pGNPX_App;
		static private G6_Base		G6 => GNPX_App_Man.G6;
		static public GNPX_win		pGNP00win => pGNPX_App.pGNP00win;
        private GNPX_Engin			GNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine
		public object Culture{ get=> pRes.Culture; }


        public Func_DevelopStatistics( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();

			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  
        }

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;
		}

		private void btn_devWinClose_Click(object sender, RoutedEventArgs e) {

        }
    }
}
