using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;

using GIDOO_space;
using System.Data;


namespace GNPX_space{
    public partial class ALSTechGen: AnalyzerBaseV2{
		private int stageNoPMemo = -9;
        private ALSLinkMan  ALSMan;
		private bool		debugPrint=false;

        public ALSTechGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){
            this.pAnMan=pAnMan;
        }

		private void PrepareStage( int minSize=1 ){
            base.AnalyzerBaseV2_PrepareStage();

			if( stageNoP!=stageNoPMemo || ALSMan.ALSList== null ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();

                ALSMan = new ALSLinkMan( pAnMan );
				ALSMan.Initialize();
				ALSMan.Prepare_ALSLink_Man( nPlsB:1, setCondInfo:true);
			}      
		}

        //ALS-Wing is an analysis algorithm using three ALS. It is the case of the next ALS Chain 3ALS.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page44.html

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
		// ... Copying a digits column is fine with a rough range selection.
        //7.1..9..8.52...19..8...3.574.3.5.......2.1.......3.7.519.7...3..37...68.8..3..9.1
        //2.9..3..8.17...63..8...6.259.5.6.......3.2.......9.3.114.6...9..53...48.7..4..2.6
        //...8...4....21...7...7.5981315..9..8.8....4....41.83.5..1.82.646.8...1...236.18.. 
        //1..2.9.74.2....19.9.3..182.29..17.6.3.18.294....39.21...29..731.19..3682637128459

        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page44.html   - XYZ-WingALS
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page51.html   - ALS XY-Wing



    }
}
