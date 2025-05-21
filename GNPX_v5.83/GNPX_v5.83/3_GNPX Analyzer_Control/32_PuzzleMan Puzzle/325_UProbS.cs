using System;
using System.Collections.Generic;
using static System.Math;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Linq;
using System.Windows.Documents;
using System.Xml.Linq;

namespace GNPX_space{

    // UPuzzleS : Class for multiple solution analysis.
    //   Elements are information about the analysis result.

    public class UPuzzleS{
        public int        __ID{ get; set; }
        public int        difficultyLevel{ get; set; }
        public string     Sol_Result{ get; set; }
        public string     Sol_ResultLong{ get; set; }
/*
        public UPuzzleS( int IDmp1, int difficultyLevel, string Sol_Result, string Sol_ResultLong ){
            this.__ID = IDmp1; this.difficultyLevel = difficultyLevel;
            this.Sol_Result     = Sol_Result;       //new string(Sol_Result);
            this.Sol_ResultLong = Sol_ResultLong;   //new string(Sol_ResultLong);
        }
*/
        public UPuzzleS( UPuzzle aPZL, int __ID ){
            this.__ID            = __ID;
            this.difficultyLevel = aPZL.pMethod.difficultyLevel;
            this.Sol_Result      = aPZL.Sol_Result;
            this.Sol_ResultLong  = aPZL.Sol_ResultLong;
        }

        public override string ToString(){
            string st = $"IDmp1:{__ID} difficultyLevel:{difficultyLevel}  Sol_Result:{Sol_Result}";
            return st;
        }
    }
}