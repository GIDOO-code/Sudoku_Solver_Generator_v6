using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Media;
using System.Threading;
using static System.Diagnostics.Debug;
using static System.Math;
using System.Security.Cryptography;
using System.Windows.Interop;
using System.IO;
using System.Text;

namespace GNPX_space{
    using pRes=Properties.Resources;
    public delegate bool dSolver();

    // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
    public class UAlgMethod{

        static private  int     ID0=0;
		private G6_Base			G6        => GNPX_App_Man.G6;
        public int				ID{ get; set; }
		public string			MethodName{ get; set; }
		public bool				IsChecked{ get; set; }		// Algorithm validpublic string Signature{ get; set; }			
		public Brush			brsh{ get; set; }



		public readonly dSolver Method;
        public readonly string	MethodKey;
		public bool             Sel_Manual=false;

        // For algorithms with negative levels, there are simpler conjugate algorithms.
        // If you just solve Sudoku, you don't need it. For example, the 5D-LockedSet is conjugate with the 4D-LockedSet.
        public readonly int		difficultyLevel=-1;         // Level of difficulty
        public readonly int     recommendLevel =-1;
        public int				UsedCC=0;			// Counter applied to solve one puzzle.

		public bool			    method_valid = true;		// Valid method    ... for development

        public bool			    GenLogB;				// method "GeneralLogic"
		public bool				ID_caseGL;				// Low-level method to execute when GL is enabled.


	  // ===== <<<develop >>> =====
		static public int	   diffLevel;
		static public int      recmLevel;
		public string		   NameM{ get{
										string st = "";
										if( G6.Method_SelectionMode=="Difficulty"){ st = $"{Abs(difficultyLevel):00} {MethodName}"; }
										else{										st = $"{Abs(recommendLevel):00} {MethodName}";  }
										return st;
									}
								}
		public bool		       markA_dev = false;	// focused methods ... for development,
		public Brush		   brshM{ get{
											Brush br = Brushes.LightGray;
											if( markA_dev )  br = Brushes.Orange;

											if( G6.Method_SelectionMode=="Difficulty"){ if( Abs(difficultyLevel)<=diffLevel )  br = Brushes.Lime; }
											else{										if( recommendLevel<=recmLevel ) br = Brushes.Cyan; }

											if( Sel_Manual ) br = Brushes.White;
											return  br;
									}
								}
		public bool		       markA_de{ get; set; }
		public int			   sortKey => markA_dev? ID: (Abs(difficultyLevel)<=diffLevel? ID+100000: ID+200000); 
	  // ----- <<<develop >>> -----





        public UAlgMethod( ){ }

		public UAlgMethod( UAlgMethod P ){
			this.ID         = P.ID;
            this.recommendLevel       = P.recommendLevel;   

			this.Method     = P.Method;
            this.MethodName = P.MethodName;
            this.difficultyLevel = P.difficultyLevel;     //Level of difficulty
			this.MethodKey  = P.MethodKey;

            this.GenLogB    = P.GenLogB;

		}
        public UAlgMethod( int pid, int recommendLevel, string MethodName, int difficultyLevel, dSolver Method, bool GenLogB=false ){
			this.ID_caseGL	    = (pid<=4);
            this.ID             = pid*500+(ID0++); //System default order.
            this.recommendLevel = recommendLevel;   

			this.Method			= Method;
            this.MethodName		= MethodName;
            this.difficultyLevel = difficultyLevel;     //Level of difficulty
			this.MethodKey		= ID.ToString().PadLeft(7) +difficultyLevel.ToString().PadLeft(2) +MethodName;

            this.GenLogB    = GenLogB;
        }
        public override string ToString(){
			string stMName = $" ID:{ID} {MethodName} (dif:{difficultyLevel} cc:{UsedCC}) IsChecked:{IsChecked}";
            string st = stMName.PadRight(35);
            if( GenLogB ) st += " *** GeneralLogic:"+GenLogB.ToString();
            return st;
        }
    }

}