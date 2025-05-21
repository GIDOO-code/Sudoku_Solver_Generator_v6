using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Navigation;
using static System.Diagnostics.Debug;

using GIDOO_space;


namespace GNPX_space{
    public partial class eNetwork_App: AnalyzerBaseV2{
		private int stageNoPMemo = -9;
//        public static DevelopWin devWin; //## development

        private eNetwork_Man eGLink_ManObj;

        private ALSLinkMan   ALSMan;
        public  int  NiceLoopMax{ get => G6.NiceLoopMax; }
        public  eNetwork_App( GNPX_AnalyzerMan pAnMan ): base(pAnMan){
            this.pAnMan=pAnMan;
        }

		private bool eNetwork_Dev5_Prepare( int nPls, int minSize=1 ){
			if( stageNoP!=stageNoPMemo ){
				stageNoPMemo = stageNoP;

				base.AnalyzerBaseV2_PrepareStage();

                eNetwork_Link.Initialize();
				
				ALSMan = new ALSLinkMan(pAnMan);			//Only definitions here. There is no substance
                eGLink_ManObj = new eNetwork_Man( pAnMan );
				eGLink_ManObj.QSearch_Links_Generater( checkB:false ); // Links : Cell, AIC, ALS, ALSXZ AnLS, eALS


        //        bool ALSChecked   = G6.ALS"];
        //        bool ALSXZChecked = G6.ALSXZ"];    
		//        ALSMan.Prepare_ALSLink_Man( nPlsB:nPls, minSize:minSize, setCondInfo:true, debugPrintB:false );



				//
				PreferSimpleLinks = G6.PreferSimpleLinks > 0;
				Use_eALS          = G6.Use_eALS > 1;                                                      //                                                                
			}  
            return true;
		}
    }
}