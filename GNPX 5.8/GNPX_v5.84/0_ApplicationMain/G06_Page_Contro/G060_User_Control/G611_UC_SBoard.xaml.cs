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


namespace GNPX_space{
//	
//	using static System.Net.Mime.MediaTypeNames;
	using sysWin = System.Windows;
	//	using pGPGC  = GNPX_Puzzle_Global_Control;
	using pApEnv = App_Environment;
	public partial class G611_UC_SBoard : UserControl {

		public GNPX_App_Man 		pGNPX_App;
		public GNPX_App_Ctrl        App_Ctrl => pGNPX_App.App_Ctrl;
			
        private GNPX_Engin			pGNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine
		public UPuzzle				ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; } // Puzzle to analyze
		private G6_Base				G6 => GNPX_App_Man.G6;

		private GNPX_Graphics		gnpxGrp => pGNPX_App.gnpxGrp;
        private RenderTargetBitmap  bmpPD = new RenderTargetBitmap(176,176, 96,96, PixelFormats.Default);//176=18*9+2*4+1*6     

		public G611_UC_SBoard( ) {
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {			
			CellNumMax.Value = G6.CellNumMax;
		}

	#region Pattern Management
		private void CellNumMax_ValueChanged(object sender, GIDOOEventArgs args) {
			G6.CellNumMax = CellNumMax.Value;
		}

		private void rdb_patSel0X_Checked(object sender, RoutedEventArgs e) {
			RadioButton　rdb = (RadioButton)sender;
			if( (bool)rdb.IsChecked ) G6.PatSel0X = int.Parse( rdb.Name.Replace("rdb_patSel","" ) );//  Substring(10,2) );
		}

		public void btn_PatternClear_Click(object sender, RoutedEventArgs e) {
            App_Ctrl.PatGen.GPat = new int[9,9];
            _SetBitmap_PB_pattern();  
		}

		private void PB_pattern_MouseDown(object sender, MouseButtonEventArgs e) {
            _GeneratePatternl(false);
		}

		private void btn_PatternAutoGen_Click(object sender, RoutedEventArgs e) {
            G6.CellNumMax = (int)CellNumMax.Value;
            _GeneratePatternl(true);
            GNPX_App_Ctrl.rxCTRL = -1;     //Initialize Puzzle candidate generater
		}

		private void btn_PatternCapture_Click(object sender, RoutedEventArgs e) {
            int nn = App_Ctrl.PatGen.patternImport( ePZL );
            _SetBitmap_PB_pattern();
			lbl_Pattern2.Content = nn.ToString();
		}

		private void chb_CreatePuzzleEx2_Checked( object sender, RoutedEventArgs e ){
			//this.Dispatcher.Invoke(() => Reflesh_PB_BasePatDig );
			Reflesh_PB_BasePatDig( );
		}

		public bool Reflesh_PB_BasePatDig_Prop{ set => Reflesh_PB_BasePatDig(); }
		public void Reflesh_PB_BasePatDig(){
			if( gnpxGrp == null )  return;
			if( (bool)chb_CreatePuzzleEx2.IsChecked ){
				PB_BasePatDig.Source = gnpxGrp.GBPatternDigit( bmpPD, G6.Sol99sta );
			}
			else{
				PB_BasePatDig.Source = null;
			}
		}
	
		public bool SetBitmap_PB_pattern{ set => _SetBitmap_PB_pattern(); }
		public bool GeneratePatternl{ set => _GeneratePatternl( ModeAuto:value ); }

        private void _SetBitmap_PB_pattern( ){
			if( gnpxGrp == null )  return;
            int nCell = gnpxGrp.GBPatternPaint( PB_pattern, App_Ctrl.PatGen.GPat );
			lbl_Pattern2.Content = nCell.ToString();	//App_Ctrl.PatGen.pattenCount;
        }

		private void _GeneratePatternl( bool ModeAuto ){   
			int rdb_patSel = G6.PatSel0X;
          /// int rdb_patSel = patSelLst.Find(p=>(bool)p.IsChecked).Name.Substring(10,2).ToInt(); //パターン形

            int nn = 0;
            if( ModeAuto )  nn = App_Ctrl.PatGen.patternAutoMaker(rdb_patSel);
            else{
                sysWin.Point pt = Mouse.GetPosition(PB_pattern);
				var (ret,row,col) = __GetRCPositionFromPattern( pt );
                if( ret ) nn = App_Ctrl.PatGen.symmetryPattern(rdb_patSel,row,col,false);
            }

            GNPX_App_Ctrl.rxCTRL = 0;     //Initialize Puzzle Generater
			G6.LS_P_P0 = 0;
            _SetBitmap_PB_pattern();
			lbl_Pattern2.Content = nn.ToString();

			return;

					(bool,int,int) __GetRCPositionFromPattern( sysWin.Point pt ){
						int selSizeHf = pApEnv.cellSizeP/2 + 1;

						int row=-1, col=-1;
						int rn = (int)(pt.Y - pApEnv.lineWidth);
						rn = rn-rn/(selSizeHf*3)*2;
						row = (rn/selSizeHf);

						int cn = (int)(pt.X - pApEnv.lineWidth);
						cn = cn-cn/(selSizeHf*3)*2;
						col = cn/selSizeHf;

						if( row<0 || row>=9 || col<0 || col>=9 ) return (false,-1,-1);
						return (true,row,col);
					}
        }

		#endregion Pattern Management


	}
}
