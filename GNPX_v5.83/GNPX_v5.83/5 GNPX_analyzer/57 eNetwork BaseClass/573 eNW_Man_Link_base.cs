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
    public partial class eNetwork_Link: IComparable{
        static public GNPX_AnalyzerMan	pAnMan;

        static public UInt128[]			pHouseCells81     => AnalyzerBaseV2.HouseCells81;
        static public UInt128[]			pConnectedCells81 => AnalyzerBaseV2.ConnectedCells81;
        static public List<UCell>		pBOARD => pAnMan.pBOARD;
        static private int				ID0=0;
		static public bool				DispID;

    //    static public  UInt128 keyDef( int no, UInt128 U, int pmCnd, int ID=0 ) => new ULogical_Node( no, U, pmCnd );
        static public void Initialize() => ID0=-1;
        static private protected bool _withID_=false;

        static public List<eNetwork_Node> peNetwork_NodeList => eNetwork_Man.eNetwork_NodeList;

        static public void Set_withID_( bool withID ) => _withID_ = withID;  // for DEBUG
        static public int qConnectTo_RC=-1;  

	  //static protected bool debugAT = false;
        // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*




        public int						ID;

		public int						lkCode=0;
		public int						genNo=0;		// branch order from stem	
        public int						Size => eObject.BitCount();
        public string					keySt;

        private int						_hv=int.MaxValue;
		public  int						hv{ get => (_hv!=int.MaxValue)? _hv: _hv=GetHashCode(); }


		/* *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
			A link has a source node and a destination node.
			eNetwork_Node is set as required from ULogical_Node information.
			Information on links connecting to nodes is not automatically set.
			Upstream link information for a node is set using dedicated routines.
			Nodes can maintain upstream link information.
			A node's upstream links include positively defined links and negatively defined links.
			and each is maintained separately.
			It is possible to detect that upstream link information is set in a node.
		   *--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*
		*/
		public ULogical_Node			OrgN;
		public ULogical_Node			DesN;

        public eNetwork_Node			eN_Org_nPM => Get_eNetwork_Node_withULG(OrgN);  //search with matchKey2  (with rc,no)
        public eNetwork_Node			eN_Des_nPM => Get_eNetwork_Node_withULG(DesN);//search with matchKey2  (with rc,no)
        public int						noOrg => OrgN.no;
        public int						noDes => DesN.no;
        public UInt128					eBody128 => eObject.DifSet(DesN.b081);

        public UInt128					eObject;					// Used to display results




        private eNetwork_Node Get_eNetwork_Node_withULG( ULogical_Node ULG, bool addOn=true ){
			// Find the same node regardless of whether the node is positive or negative.
			// The positive/negative information of the node is held by ULogical_Node.
            eNetwork_Node eN=null;
			UInt128 _matchKey2 = (ULG!=null)? ULG.matchKey2: UInt128.MaxValue;
            if( peNetwork_NodeList!=null && peNetwork_NodeList.Count>0 ) eN = peNetwork_NodeList.Find(p => p.matchKey2==_matchKey2);      
            if( (eN is null) && addOn )  peNetwork_NodeList.Add( eN=new(ULG) );
					//WriteLine(""); peNetwork_NodeList.ForEach( (p,mx) => WriteLine( $" peNetwork_NodeList  {mx} : {p}" ) ); 
            return eN;
        }

    //===============================================================================================================================











        public eNetwork_Link(){ ID = ++ID0;}


        public eNetwork_Link( ULogical_Node _OrgN, ULogical_Node _DesN ): this(){
            this.OrgN = _OrgN;
			this.DesN = _DesN;
        }


        public virtual string CreateKey() => "";
        public virtual int CompareTo( object xObj ){ return 0; }

        public int SearchOrder( eNetwork_Link A ){		
            int val = 0;
            if( (val = genNo - A.genNo) !=0 )  return val;			
            if( (val = OrgN.b081Size - A.OrgN.b081Size) !=0 )  return val;
            if( (val = DesN.b081Size - A.DesN.b081Size) != 0 )  return val;
            return (ID - A.ID);
        }
        public int SearchOrderSimple( eNetwork_Link A ){		
            int val = this.lkCode-A.lkCode;
			if( val != 0 )  return val;
            if( (val = genNo - A.genNo) !=0 )  return val;		// branch order from stem	
            if( (val = OrgN.b081Size - A.OrgN.b081Size) != 0 )  return val;
            if( (val = DesN.b081Size - A.DesN.b081Size) != 0 )  return val;
            return (ID - A.ID);
        }

        public override int GetHashCode(){ 
            int _hv_ = OrgN.matchKey3.GetHashCode();

            // For intraCell, both end nodes are the same. Therefore, hash value is 0. 
            if( !(this is eNW_Link_IntraCell) ) _hv_ ^= DesN.matchKey3.GetHashCode();
            return _hv_;
        }

		public override string ToString( ){
			return (DispID? $"ID:{ID} ": "");
		}


		#region Remove duplicate elements in link column display. 
		public string ToString2(bool firstTimet){
            string st = this.ToString();
            if(firstTimet) return st;
            else{
                int n = st.IndexOf( "=>");
				if( n<=0 )  return st;  
                string st2 = new string(' ',9) + st.Substring(n);
                return st2;
            }
        }

        public string ToString2B(bool firstTimet){
            string st = this.ToString();
            if(!firstTimet){
                int n = st.IndexOf( "=>");		
				if( n>=0 )  st = st.Substring(n);
			}
			int m = st.Length;
			st = st.Replace("=> [intra cell ->","=>").Replace("=> [inter cell ->","=>");
			if( st.Length != m )  st = st.Replace("]","");

            return st;
        }
#endregion
    }

}
