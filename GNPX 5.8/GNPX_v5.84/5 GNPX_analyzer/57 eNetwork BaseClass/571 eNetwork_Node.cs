using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Xml.Linq;
using System.Net;

namespace GNPX_space {
    //ALS(Almost Locked Set) 
    // ALS is a state where there are "n+1 candidate digits" in "n cells" belonging to the same house. 
    // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page26.html

    [Serializable]
    public class eNetwork_Node: IComparable{
        static public GNPX_AnalyzerMan pAnMan;
        static public List<UCell> pBOARD => pAnMan.pBOARD;
        static public UInt128[]   pHouseCells81     => AnalyzerBaseV2.HouseCells81;
        static public UInt128[]   pConnectedCells81 => AnalyzerBaseV2.ConnectedCells81;

		static private int        _ID00=0;
        static private protected bool _withID_=false;

        // =============== property ===============
		public int		      ID;
        public UInt128        matchKey2 => ULGNode.matchKey2;  //b081, no     (81+4=85)
	  //public UInt128        matchKey3 => ULGNode.matchKey3;  //b081, no, mp (81+4+1=86)

        public ULogical_Node  ULGNode;

        public int            rc{    get=> ULGNode.rc;}
        public int            no{    get=> ULGNode.no;}
        public int            rcno{  get=> rc<<4 | no; }
     // public int            pmCnd{ get=> ULGNode.pmCnd; }		// Node have no positive or negative attributes

        public bool           selected=false;

        static public void Set_withID_( bool withID ) => _withID_ = withID;  // for DEBUG 

	 /* *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
			A link has a origin node and a destination node.
			Link information to connect to a node is not automatically set.
			Upstream link information for a node is set using a dedicated routine.
			(Node can hold upstream link information.)
			Upstream link of a node include links that are defined positively and negatively,
			and each is maintained separately.
			It is possible to detect that upstream link information has already been set in a node.
	    *--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*   */

        public eNetwork_Link  eChaineNtwk_pre_Plus  = null;	// positively defined link
        public eNetwork_Link  eChaineNtwk_pre_Minus = null;	// negatively defined link 
        // ----------------------------------------
		

        public eNetwork_Node( ULogical_Node nodeX ){
			this.ID      = _ID00++;
            this.ULGNode = nodeX;

		  //this.matchKey2 = (nodeX==null)? 0: this.matchKey2 = nodeX.matchKey2;  // b081. no         (81+4=85)
		  //this.matchKey3 = (nodeX==null)? 0: this.matchKey3 = nodeX.matchKey3;  // b081. no, mp     (81+4+1=86)

        }

        public override int GetHashCode(){ return ULGNode.GetHashCode(); }

/*
		public override string ToString(){
          //string st = $"{ULGNode}  matchKey2:{matchKey2}  matchKey3:{matchKey3}";
			string st = _withID_? $" ID:{ID} ": "";
            st += $"{ULGNode}";
            return st;
        }
        public string ToString_WithPre(){
            string st = $"{ULGNode}\r  T:{eChaineNtwk_pre_Plus}\r  F:{eChaineNtwk_pre_Minus}";
            return st;
        }
*/
        public int CompareTo( object xObj ){ return 0; }       
    }
}
