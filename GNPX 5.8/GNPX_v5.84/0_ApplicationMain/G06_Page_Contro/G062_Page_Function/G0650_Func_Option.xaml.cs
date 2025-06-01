using GIDOO_space;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Diagnostics.Debug;
using static System.Math;
using System.Threading;
//using System.Windows.Threading;

namespace GNPX_space{
	using pRes = Properties.Resources;
	using ioPath = System.IO.Path;

    public partial class Func_Option: Page{
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 

		public GNPX_App_Man 		pGNPX_App;

		private GNPX_Setting_Editor GNPX_Setting_Win;
		public GNPX_win				pGNP00win => pGNPX_App.pGNP00win;

		public GNPX_App_Ctrl        App_Ctrl => pGNPX_App.App_Ctrl;

		private G6_Base				G6 => GNPX_App_Man.G6;
        private GNPX_Engin			GNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine																				 
		private GNPX_AnalyzerMan	pAnMan => GNPX_Eng.AnMan;
		private GNPX_Graphics		gnpxGrp => pGNPX_App.gnpxGrp;
		private RenderTargetBitmap  bmpGZeroA;

		public UPuzzle				ePZL{ get=>GNPX_Eng.ePZL; set=>GNPX_Eng.ePZL=value; } // Puzzle to analyze

        public Func_Option( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();
			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  

			lbl_ProcessorCount.Content = $"ProcessorCount : {Environment.ProcessorCount}";
			bmpGZeroA = new ( 338, 338, 96,96, PixelFormats.Default );
        }

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;
			txb_RandomSeed_000.Text = G6.RandomSeedVal_000.ToString();
			int val = G6.RandomSeedVal_000;
			G6.RandomSeedVal_000 = val;
            App_Ctrl.Set_RandomSeed(val);

			btn_CopySolution.Opacity = G6.PowerUser? 1.0: 0.01;

			chbx_ShowCandidate.IsChecked = G6.sNoAssist;

			lbl_PowerUser.Visibility = Visibility.Hidden;
			btn_PowerUser.Opacity = 0.01;

		}

		private void Page_Unloaded(object sender, RoutedEventArgs e) {
//			G6.Save_CreatedPuzzle = (bool)chbx_FileOutputOnSuccess.IsChecked;
		}

		private void btn_LangageSelect_Click(object sender, RoutedEventArgs e) {
			string lng = (string)((Button)sender).Content;
			string culture = (lng=="Japanese")? "ja-JP": "en-US";
			ResourceService.Current.ChangeCulture(culture);
			App_Environment.culture = culture;
        
			WriteLine( $"Option The current culture is {culture}");

			{
				//    __bruMoveSub();			
				Gidoo_EventHandler se = new Gidoo_EventHandler( eName:"Vibrate!" );  //Report
				Send_Command_to_GNPXwin( this, se );
			}

		//	txtCopyrightDisclaimer.Text = CopyrightJP;		
        //    _MethodSelectionMan();

        }
		private void txb_randomSeed_000_TextChanged(object sender, TextChangedEventArgs e) {
			string st = txb_RandomSeed_000.Text;
			int val = 0;
			try{ val  = int.Parse( st ); }
			catch( Exception ex ){ 
				val = 0;
				txb_RandomSeed_000.TextChanged -= new TextChangedEventHandler( txb_randomSeed_000_TextChanged );
				txb_RandomSeed_000.Text = "0";
				txb_RandomSeed_000.TextChanged += new TextChangedEventHandler( txb_randomSeed_000_TextChanged );
			}
			G6.RandomSeedVal_000 =  int.Parse( txb_RandomSeed_000.Text );
            App_Ctrl.Set_RandomSeed(val);
		}
		
		private void btn_CopySolution_Click(object sender, RoutedEventArgs e ){
			try{
				List<UCell> board = Get_Solution();
				int nc = board.Count(p=>p.No>0);
				if( nc < 17 )  return;
				string st = board.ConvertAll(q=> Abs(q.No)).Connect("").Replace("0",".");
				Clipboard.Clear();
				Clipboard.SetData(DataFormats.Text, st);
				texb_bitmapPath.Text = $"Puzzle Solution\n{st}";
			}
			catch( Exception e2 ){ WriteLine( $"{e2.Message}\n{e2.StackTrace}" ); }
		}

		public void btn_CopyBitMap_Click(object sender, RoutedEventArgs e) {
            try{
			//	G6.sNoAssist  = (bool) chbx_ShowCandidate.IsChecked;
				G6.sWhiteBack = (bool)chbx_WhiteBack.IsChecked;

				List<UCell> BOARD = (bool)chbx_SetAnswer.IsChecked? Get_Solution(): ePZL.BOARD;
				var bmf = gnpxGrp.GBoardPaint( bmpGZeroA, BOARD,  sNoAssist:G6.sNoAssist, whiteBack:G6.sWhiteBack );

                Clipboard.SetData(DataFormats.Bitmap,bmf);
            }
            catch(System.Runtime.InteropServices.COMException){  }
		}


		public void btn_SaveBitMap_Click(object sender, RoutedEventArgs e ){
            BitmapEncoder enc = new PngBitmapEncoder(); // JpegBitmapEncoder(); BmpBitmapEncoder();
			G6.sNoAssist  = (bool) chbx_ShowCandidate.IsChecked;
			G6.sWhiteBack = (bool)chbx_WhiteBack.IsChecked;

			List<UCell> BOARD = (bool)chbx_SetAnswer.IsChecked? Get_Solution(): ePZL.BOARD;
			var bmp = gnpxGrp.GBoardPaint( bmpGZeroA, BOARD,  sNoAssist:G6.sNoAssist, whiteBack:G6.sWhiteBack );
            var bmf = BitmapFrame.Create(bmp);
            enc.Frames.Add(bmf);
            try {
                Clipboard.SetData(DataFormats.Bitmap,bmf);
            }
            catch(System.Runtime.InteropServices.COMException){  }

            if( !Directory.Exists( pRes.fldSuDoKuImages) ){ Directory.CreateDirectory(pRes.fldSuDoKuImages); }
            string fName = DateTime.Now.ToString("yyyyMMdd_HHmmss")+".png";
            using( Stream stream = File.Create(pRes.fldSuDoKuImages+"/"+fName) ){
                enc.Save(stream);
            }    
            texb_bitmapPath.Text = "Path : "+ioPath.GetFullPath( pRes.fldSuDoKuImages+"/"+fName);

		}


        private List<UCell> Get_Solution(){
			Research_trial RTrial = new( pAnMan );
			List<int> intBoard = ePZL.BOARD.ConvertAll( P=> P.No ); 
			bool ret = RTrial.TrialAndErrorApp( intBoard, filePutB:true, upperLimit:2 );

			List<UCell>  board = ret? RTrial.RT_Get_Board(): null;
			return board;
        }

		private void btn_PowerUser_Click(object sender, RoutedEventArgs e ){
			Thread.Sleep(200);

			lbl_PowerUser.Visibility = Visibility.Visible;

			if( btn_PowerUser.Opacity<0.5 ){
				G6.PowerUser = true;
				btn_PowerUser.Opacity = 1.0;
			}
			else{
				G6.PowerUser = false;
				btn_PowerUser.Opacity = 0.05;
			}
			btn_CopySolution.Opacity = G6.PowerUser? 1.0: 0.05;

			Gidoo_EventHandler se = new Gidoo_EventHandler( eName:"G6_Changed" );  //Report
			Send_Command_to_GNPXwin( this, se );
        }



		private void lbl_GNPX_Setting_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			lbl_GNPX_Setting.Opacity = 1.0;
			btn_GNPX_Setting.Opacity = 1.0;
        }
		private void btn_GNPX_Setting_Click(object sender, RoutedEventArgs e) {
			GNPX_Setting_Win = new GNPX_Setting_Editor( pGNP00win );
			GNPX_Setting_Win.Show();
		}

		private void chbx_ShowCandidate_Checked(object sender, RoutedEventArgs e) {
			G6.sNoAssist = (bool)chbx_ShowCandidate.IsChecked;
		}
	}
}
