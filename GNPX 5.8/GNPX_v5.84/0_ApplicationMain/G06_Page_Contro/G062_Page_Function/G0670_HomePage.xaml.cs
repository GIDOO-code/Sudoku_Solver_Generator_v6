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
using System.Windows.Shapes;
using static System.Diagnostics.Debug;

namespace GNPX_space{
    using pRes=Properties.Resources;
	using pGPGC = GNPX_Puzzle_Global_Control;
    public partial class Func_HomePage: Page{

		static public GNPX_App_Man 	pGNPX_App;
		static private G6_Base		G6 => GNPX_App_Man.G6;
        private GNPX_Engin			GNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine

        string CopyrightJP, CopyrightEN;

        public Func_HomePage( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();

              #region Copyright
                string endl = "\r";
                string st = "【著作権】" + endl;
                st += "本ソフトウエアと付属文書に関する著作権は、作者GNPX に帰属します。" + endl;
                st += "本ソフトウエアは著作権法及び国際著作権条約により保護されています。" + endl;
                st += "使用ユーザは本ソフトウエアに付された権利表示を除去、改変してはいけません" + endl + endl;

                st += "【配布】" + endl;
                st += "インターネット上での二次配布、紹介等は事前の承諾なしで行ってかまいません。";
                st += "バージョンアップした場合等には、情報の更新をお願いします。" + endl;
                st += "雑誌・書籍等に収録・頒布する場合には、事前に作者の承諾が必要です。" + endl + endl;
                   
                st += "【禁止事項】" + endl;
                st += "以下のことは禁止します。" + endl;
                st += "・オリジナル以外の形で、他の人に配布すること" + endl;
                st += "・第三者に対して本ソフトウエアを販売すること" + endl;
                st += "・販売を目的とした宣伝・営業・複製を行うこと" + endl;
                st += "・第三者に対して本ソフトウエアの使用権を譲渡・再承諾すること" + endl;
                st += "・本ソフトウエアに対してリバースエンジニアリングを行うこと" + endl;
                st += "・本承諾書、付属文書、本ソフトウエアの一部または全部を改変・除去すること" + endl + endl;

                st += "【免責事項】" + endl;
                st += "作者は、本ソフトウエアの使用または使用不能から生じるコンピュータの故障、情報の喪失、";
                st += "その他あらゆる直接的及び間接的被害に対して一切の責任を負いません。" + endl;
                CopyrightJP=st;

                st="===== CopyrightDisclaimer =====" + endl;
                st += "Copyright" + endl;
                st += "The copyright on this software and attached document belongs to the author GNPX" + endl;
                st += "This software is protected by copyright law and international copyright treaty." + endl;
                st += "Users should not remove or alter the rights indication attached to this software." + endl + endl;

                st += "distribution" + endl;
                st += "Secondary distribution on the Internet, introduction etc. can be done without prior consent.";
                st += "Please update the information when upgrading etc etc." + endl;
                st += "In the case of recording / distributing in magazines · books, etc., consent of the author is necessary beforehand." + endl + endl;
                   
                st += "Prohibited matter" + endl;
                st += "The following things are forbidden." + endl;
                st += "Distribute it to other people in forms other than the original." + endl;
                st += "Selling this software to a third party." + endl;
                st += "Promotion, sales and reproduction for sale." + endl;
                st += "Transfer and re-accept the right to use this software to a third party." + endl;
                st += "Modification / removal of this consent form and attached document" + endl + endl;

                st += "Disclaimer" + endl;
                st += "The author assumes no responsibility for damage to computers, loss of information or any other direct or indirect damage resulting from the use or inability of the Software." + endl;
                
				CopyrightEN = st;
                txtCopyrightDisclaimer.Text = CopyrightEN;
              #endregion Copyright

        }

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;

			if( this.Visibility != Visibility.Visible )  return;
		    string culture = App_Environment.culture;
				//WriteLine( $"Page_IsVisibleChanged => culture is {culture}");
            string urlHP="";
            if(  culture == "ja-JP" ){
				txtCopyrightDisclaimer.Text = CopyrightJP;
				urlHP = "https://gidoo-code.github.io/Sudoku_Solver_Generator_v6_jp/";
			}
            else{
				txtCopyrightDisclaimer.Text = CopyrightEN; 
				urlHP = "https://gidoo-code.github.io/Sudoku_Solver_Generator_v6/"; 
			}
			HP_address.Text = urlHP;
		}

		private void btnHomePageGitHub_Click(object sender, RoutedEventArgs e) {
		    string culture = App_Environment.culture;
				//WriteLine( $"Page_IsVisibleChanged => culture is {culture}");
            string urlHP="";
            if( culture=="ja-JP" ) urlHP = "https://gidoo-code.github.io/Sudoku_Solver_Generator_v6_jp/";
            else                   urlHP = "https://gidoo-code.github.io/Sudoku_Solver_Generator_v6/"; 

            HP_address.Text = urlHP;
            Clipboard.SetData(DataFormats.Text, urlHP);
            CopiedHP.Visibility = Visibility.Visible;
            Extension_Utility.ProcessExe(urlHP);
		}
	}
}
