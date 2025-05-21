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

namespace GNPX_space{
	using pGPGC = GNPX_Puzzle_Global_Control;
	using G6_SF = G6_staticFunctions;

    public partial class Func_CreateManual: Page{
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 

		public GNPX_App_Man 		pGNPX_App;
        private GNPX_win			pGNP00win{ get=> pGNPX_App.pGNP00win; } 
		public GNPX_App_Ctrl        App_Ctrl => pGNPX_App.App_Ctrl;

		private G6_Base				G6 => GNPX_App_Man.G6;

		private GNPX_Graphics		gnpxGrp => pGNPX_App.gnpxGrp;


        private GNPX_Engin			pGNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine
		public UPuzzle				ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; } // Puzzle to analyze

		private bool				InPreparation=true;
        public Func_CreateManual( GNPX_App_Man GNPX_App ){
			pGNPX_App = GNPX_App;
            InitializeComponent();

			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  

			InPreparation = false;
        }


		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;
        }

		private void Send_SetPuzzleOnBoard(){
			Gidoo_EventHandler se = new Gidoo_EventHandler( eName:"Show_AnalysisStatus" );  //Report
			Send_Command_to_GNPXwin( this, se );
		}


		private void chbAssist01_Checked(object sender, RoutedEventArgs e) {
			if( InPreparation )  return;
            G6.sNoAssist = (bool)chbx_ShowNoUsedDigits.IsChecked;
			Send_SetPuzzleOnBoard();
		}







		private void btnNewPuzzle_Click(object sender, RoutedEventArgs e) {
            if( ePZL.BOARD.Any(P=>P.No!=0) ){
				pGPGC.GNPX_Puzzle_List_Add_ifNotContain();
				pGPGC.CreateNewPuzzle();
			}
            Send_SetPuzzleOnBoard();
		}

		private void btn_CopyPuzzle_Click(object sender, RoutedEventArgs e) {
            UPuzzle tmpPZL = ePZL.Copy( );
            tmpPZL.Name="copy";
            pGPGC.CreateNewPuzzle(tmpPZL);
            Send_SetPuzzleOnBoard();
		}
		private void btn_ClearPuzzle_Click(object sender, RoutedEventArgs e) {
            for(int rc=0; rc<81; rc++ ){ ePZL.BOARD[rc] = new UCell(rc); }
            Send_SetPuzzleOnBoard();
		}

		private void btn_DeletePuzzle_Click(object sender, RoutedEventArgs e) {	
            pGPGC.GNPX_Remove();
            Send_SetPuzzleOnBoard();
		}

		private void btn_SavePuzzle_Click(object sender, RoutedEventArgs e) {
				//ePZL.BOARD.__CellsPrint( "btn_SavePuzzle_Click  ePZL.BOARD" );
            if( ePZL.BOARD.All(P=>P.No==0) ) return;
            pGPGC.GNPX_Puzzle_List_Add_ifNotContain();
            Send_SetPuzzleOnBoard();
		}

    }
}
