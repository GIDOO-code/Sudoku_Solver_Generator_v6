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

namespace GNPX_space {
	// ==========================================================================
	//   ULogical_Node : he implementation of basic elements was changed and integrated.
	// ==========================================================================

    public partial class ULogical_Node: IComparable{   // Logical Unit Element
		public int     ID=0;

		public UInt128   b081;		// 81 bit
		public int       noB9;		//  9 bit
		public int       pmCnd;		//  1 bit

		public int       lkType;
	// -------------------------------------------------------------
        public int       rc			=> b081.BitToNum(sz:81);
		public int       no			=> noB9.BitToNum();
		public UInt128   b90		=> b081 | (UInt128)noB9<<81;		// (b081+noB9)
		public int	     rcbFrame	=> b081.Ceate_rcbFrameOr(); // (1<<(rc.ToBlock()+18)) | (1<<(rc%9+9)) | (1<<rc/9); }
		public long      rcbnFrame	=> (long)noB9 | ((long)rcbFrame>>9);
        public int       b081Size	=> b081.BitCount();

		public UInt128   matchKey2  => b081 | ((UInt128)no)<<81;				//b081, no
		public UInt128   matchKey3  => b081 | ((UInt128)(no | pmCnd<<9))<<81;	//b081, no, pmCnd
        public string    TFmark		=> (pmCnd==1)? "+": "-";
	// =============================================================

		static ULogical_Node(){ }  // static

		public ULogical_Node( int no, int rc, int pmCnd, int ID=0 ){ 
			this.noB9=1<<no;
			this.b081=UInt128.One<<rc;
			this.pmCnd=pmCnd;
			this.ID=ID;
        }

		// bit expressiom type
        public ULogical_Node( int noB9, UInt128 b081, int lkType=0, int pmCnd=0, int ID=0 ){  
 			this.noB9 = noB9;  	
			this.b081 = b081;

			this.lkType = lkType;
			this.pmCnd  = pmCnd;
			this.ID = ID;
		}

		// --------------------------------------------------------------------------


	  #region CompareTo HashCode
		public int CompareTo( object obj ){		// @@@ eNetwork で用いている。GeneralLogicでは使われていない
            var Xobj = obj as ULogical_Node;
            if( Xobj is null  )  return -1;
            var dif = this.matchKey3-Xobj.matchKey3;
            int ret = (dif<0)? -1: (dif>0)? 1: 0;
            return ret;
        }

		public int CompareToA( ULogical_Node B ){	// @@@ GeneralLogicで使われている。
            var Xobj = B as ULogical_Node;
            if( Xobj is null  )  return -1;
			if( this.noB9 !=B.noB9 )  return (this.noB9-B.noB9);
            if( this.b081 == B.b081 ) return 0;
            return (this.b081<B.b081)? -1: +1;
        }

        public override int GetHashCode(){
			UInt128  essentialULG4 = ((UInt128)pmCnd<<90) | ((UInt128)noB9<<81) | b081;
            int hashValue = essentialULG4.GetHashCode();  // <-- (b081,no,pmCnd)
            return hashValue;
        }
	  #endregion CompareTo HashCode

	  #region ToString()
		public override string ToString(){
			string st1 = $" ID:{ID:000} {ToString_SameHouseComp().PadRight(12)}#{noB9.ToBitString(9)} pmCnd:{pmCnd}" ;
			string st2 = $"  lkType:{lkType}";
			string st = $" {st1} // rc:{b081.ToBitString81N()} rcb:{rcbnFrame.ToBitString36rcbn(digitB:false)}" ;
			st += $" {st2}";
            return st;
        }

        public string ToString_SameHouseComp(){
            string st = $"{ToRCBString().ToString_SameHouseComp()}";
            return st.Trim();
        }

        public string ToRCBString(){
            string st="";
            for(int n=0; n<3; n++){
                int bp = (int)( b081 >> (n*27) );
                for(int k=0; k<27; k++){
                    if((bp&(1<<k))==0)  continue;
                    int rc=n*27+k;
                    st += $" {rc.ToRCString()}";
                }
            }
            return st.Trim();
        }

		public string HouseToString(){
			string st="";
			if( lkType==2 )	st = b081.FindFirst_rc().ToRCString();	//cell
			st += "#" + noB9.ToBitStringN(9);
			return st;
		}
	  #endregion ToString()
    }

}
