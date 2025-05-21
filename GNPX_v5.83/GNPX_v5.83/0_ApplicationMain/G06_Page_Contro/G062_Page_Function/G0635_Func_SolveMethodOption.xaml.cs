using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Diagnostics.Debug;

namespace GNPX_space{
	using pRes = Properties.Resources;
	using ioPath = System.IO.Path;
	using pGPGC = GNPX_Puzzle_Global_Control;

		



    public partial class Func_SolveMethodOption: Page{
		static public bool    EventSuppression=true;
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 

		public GNPX_App_Man 		pGNPX_App;
		public GNPX_win				pGNP00win => pGNPX_App.pGNP00win;

		private G6_Base				G6 => GNPX_App_Man.G6;
		private bool				firstB = true;

        public Func_SolveMethodOption( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();
			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  

			//lbl_ProcessorCount.Content = $"ProcessorCount : {Environment.ProcessorCount}";
        }

		private void _MethodSelectionMan(){
            string st;
            bool B = G6.GeneralLogic_on;
            string cul=Thread.CurrentThread.CurrentCulture.Name;
            string st2;
            if(cul=="ja-JP") st2= B? "有効": "無効";
            else             st2= (B? "":"not ") + "available";


            B = G6.ForceChain_on;
            if(cul == "ja-JP") st2 = B ? "有効" : "無効";
            else st2 = (B? "" : "not ") + "available";

            var currentDir = Directory.GetCurrentDirectory();
		}


		private void Page_Loaded(object sender, RoutedEventArgs e) {
			EventSuppression = true;

			G6.PG6Mode = GetType().Name;

			nud_ALSSizeMax.Value		 = G6.ALSSizeMax;
			nud_NiceLoopMax.Value		 = G6.NiceLoopMax;
			chbx_method_NLCell.IsChecked  = G6.Cell;
			chbx_method_NLAIC.IsChecked   = G6.AIC;
			chbx_method_NLALS.IsChecked   = G6.ALS;
			chbx_method_NLALSXZ.IsChecked = G6.ALSXZ;
			chbx_method_NLeALS.IsChecked  = G6.eALS;
			chbx_method_NLAnLS.IsChecked  = G6.AnLS;

			switch(G6.ForceLx){
				case "rdb_ForceL1": rdb_ForceL1.IsChecked=true; break;
				case "rdb_ForceL2": rdb_ForceL2.IsChecked=true; break;
				case "rdb_ForceL3": rdb_ForceL3.IsChecked=true; break;
			}

			chbx_ShowProofMultiPaths.IsChecked = G6.chbx_ShowProofMultiPaths;

			chbx_GeneralLogicOnChbx.IsChecked = G6.GeneralLogic_on;
			nud_GenLogMaxSize.Value = G6.nud_GenLogMaxSize;
			nud_GenLogMaxRank.Value = G6.nud_GenLogMaxRank;

			chbx_LinkExtension.IsChecked = G6.chbx_LinkExtension;
			chbx_DebugSolCheckMode.IsChecked = G6.chbx_LinkExtension;

			firstB = false;

			chbx_GeneralLogicOnChbx.IsChecked = G6.Solver_PowerMode;
						
			EventSuppression = false;
		}

/*
		private void btn_Solver_PowerMode_Click(object sender, RoutedEventArgs e ){
			if( btn_Solver_PowerMode.Opacity<0.5 ){
				G6.Solver_PowerMode = true;
				btn_Solver_PowerMode.Opacity = 1.0;
				chbx_GeneralLogicOnChbx.IsEnabled = true;
			}
			else{
				G6.Solver_PowerMode = false;
				btn_Solver_PowerMode.Opacity = 0.01;
				chbx_GeneralLogicOnChbx.IsEnabled = false;

				G6.GeneralLogic_on = false;
				chbx_GeneralLogicOnChbx.IsChecked = false;
			}


        }		
*/

        private void nud_ALSSizeMax_ValueChanged(Object sender,GIDOOEventArgs args){
            if( EventSuppression )  return;
            if( nud_ALSSizeMax==null )  return;
            G6.ALSSizeMax = nud_ALSSizeMax.Value;
            _MethodSelectionMan();
        }
        private void nud_ALSChainSizeMax_ValueChanged(Object sender,GIDOOEventArgs args){
            if( EventSuppression )  return;
            if( nud_ALSSizeMax==null )  return;
            G6.ALSChainSizeMax = nud_ALSSizeMax.Value;
            if(!firstB) _MethodSelectionMan();
        }
        private void nud_NiceLoopMax_ValueChanged(Object sender,GIDOOEventArgs args){
            if( EventSuppression )  return;
            if( nud_NiceLoopMax==null )  return;
            G6.NiceLoopMax = nud_NiceLoopMax.Value;
            if(!firstB) _MethodSelectionMan();
        }
		private void nud_QSearchMaxGen_ValueChanged(object sender, GIDOOEventArgs args) {
            if( EventSuppression )  return;
            if( nud_NiceLoopMax==null )  return;
            G6.QSearchMaxGen = nud_NiceLoopMax.Value;
            if(!firstB) _MethodSelectionMan();
        }
        private void chbX_GdNiceLoop_CGA_Checked(Object sender,RoutedEventArgs e){
            if( EventSuppression )  return;
            if( chbx_method_NLCell==null || chbx_method_NLAIC==null || chbx_method_NLALS==null || 
				chbx_method_NLALSXZ==null || chbx_method_NLAnLS==null || chbx_method_NLeALS==null )  return;

            G6.Cell   = (bool)chbx_method_NLCell.IsChecked;
            G6.AIC    = (bool)chbx_method_NLAIC.IsChecked;
            G6.ALS    = (bool)chbx_method_NLALS.IsChecked;
            G6.ALSXZ  = (bool)chbx_method_NLALSXZ.IsChecked;
            G6.AnLS   = (bool)chbx_method_NLAnLS.IsChecked;
            G6.eALS   = (bool)chbx_method_NLeALS.IsChecked;
			
			bool ALSb = (bool)chbx_method_NLALS.IsChecked;
				chbx_method_NLALSXZ.IsEnabled = ALSb;
				chbx_method_NLAnLS.IsEnabled = ALSb;
				chbx_method_NLeALS.IsEnabled = ALSb;
			Brush br = ALSb? Brushes.White: Brushes.LightPink; 
				chbx_method_NLALSXZ.Foreground = br;
				chbx_method_NLAnLS.Foreground = br;
				chbx_method_NLeALS.Foreground = br;

            if(!firstB) _MethodSelectionMan();
        }
   		private void rdb_ForceL1L2L3_Checked( object sender,RoutedEventArgs e ){
            if( EventSuppression )  return;
            if( rdb_ForceL1==null || rdb_ForceL2==null )  return;
			G6.ForceLx = ((RadioButton)sender).Name;
            if(!firstB) _MethodSelectionMan();
		}
		private void ShowProofMultiPaths_Checked(object sender, RoutedEventArgs e) {
            if( EventSuppression )  return;
            if( chbx_ShowProofMultiPaths==null )  return;
			G6.chbx_ShowProofMultiPaths = (bool)chbx_ShowProofMultiPaths.IsChecked;
            if(!firstB) _MethodSelectionMan();
        }
        private void GeneralLogicOnChbx_Checked(Object sender,RoutedEventArgs e){
            if( EventSuppression )  return;
            if( chbx_GeneralLogicOnChbx==null )  return;
            G6.GeneralLogic_on = (bool)chbx_GeneralLogicOnChbx.IsChecked;
            if(!firstB) _MethodSelectionMan();
        }
		private void nud_GenLogMaxSize_ValueChanged(Object sender,GIDOOEventArgs args){
            if( EventSuppression )  return;
            if( nud_GenLogMaxSize==null )  return;
            G6.nud_GenLogMaxSize = nud_GenLogMaxSize.Value;
            if(!firstB) _MethodSelectionMan();
        }
        private void nud_GenLogMaxRank_ValueChanged(Object sender,GIDOOEventArgs args){
            if( EventSuppression )  return;
            if( nud_GenLogMaxRank==null )  return;
            G6.nud_GenLogMaxRank = nud_GenLogMaxRank.Value;
            if(!firstB) _MethodSelectionMan();
        }

        private void chbx_LinkExtension_Checked(object sender, RoutedEventArgs e){
            if (EventSuppression) return;
            if (chbx_GeneralLogicOnChbx == null) return;
            G6.UCellLinkExt = (bool)chbx_LinkExtension.IsChecked;
            if(!firstB) _MethodSelectionMan();
        }
        private void chbx_DebugSolCheckMode_Checked(object sender, RoutedEventArgs e){
            if (EventSuppression) return;
            if (chbx_GeneralLogicOnChbx == null) return;
            G6.DebugSolCheckMode = (bool)chbx_DebugSolCheckMode.IsChecked;
            if(!firstB) _MethodSelectionMan();
        }


    }	
}
