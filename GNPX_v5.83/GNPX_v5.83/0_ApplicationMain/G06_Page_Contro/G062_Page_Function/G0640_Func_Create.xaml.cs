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
    /// <summary>
    /// Page1.xaml の相互作用ロジック
    /// </summary>
    public partial class Func_Create: Page{
		static private	string		pageMode = "--";
		static public GNPX_App_Man 	pGNPX_App;
		static private G6_Base		G6 => GNPX_App_Man.G6;
	
		public Page					objFunc_CreateManual;
		public Page					objFunc_CreateAuto;	
		public Page					objFunc_CreateAll;	

		private List<Button>		pageControlList=null;

        public Func_Create( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();

			objFunc_CreateManual = new Func_CreateManual( pGNPX_App );	
			objFunc_CreateAuto   = new Func_CreateAuto( pGNPX_App );
			objFunc_CreateAll    = new Func_CreateAll( pGNPX_App );
        }
		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;

			if( pageMode == "--" )  btn_Page_Clicked( btn_CreateAuto, e );

		}

		private void btn_Page_Clicked(object sender, RoutedEventArgs e ){
			if( pageControlList == null ){
				pageControlList = ( List<Button>)Extension_Utility.GetControlsCollection<Button>(this);
				pageControlList = pageControlList.FindAll( P=> P.Name.Contains("btn_Create") );
			}

			Button btn = (Button)sender;
			
			pageControlList.ForEach( P => P.Foreground=Brushes.White );
			btn.Foreground = Brushes.Gold;

			string btnName = btn.Name.Replace( "btn_","").Replace( "_Click","");
			switch(btnName){ 
				case "CreateManual":  frame_GNPX_Create.Navigate( objFunc_CreateManual ); break;
				case "CreateAuto":    frame_GNPX_Create.Navigate( objFunc_CreateAuto );   break;
				case "CreateAll":     frame_GNPX_Create.Navigate( objFunc_CreateAll );    break;
			}
			pageMode = btnName;
        }

    }

}
