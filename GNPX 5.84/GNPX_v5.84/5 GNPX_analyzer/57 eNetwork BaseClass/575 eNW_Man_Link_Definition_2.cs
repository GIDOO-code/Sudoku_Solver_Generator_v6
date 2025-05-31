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

    // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
    //            eNW_Link_AnLS_with_2RCC
    // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
    //ALS(Almost Locked Set) 
    // ALS is a state where there are "n+1 candidate digits" in "n cells" belonging to the same house. 
    // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page26.html
    public class eNW_Link_AnLS_with_2RCC: eNetwork_Link, IComparable{
        public UAnLS     qALS;

        public eNW_Link_AnLS_with_2RCC( UAnLS qALS, ULogical_Node OrgN, ULogical_Node DesN ): base( OrgN, DesN ){
            this.qALS = qALS;
            this.eObject = qALS.bitExp;
			this.lkCode = 34;
            this.keySt = CreateKey();
        }

        public override string CreateKey() => this.ToString().Trim();
        public override int CompareTo( object xObj ){
            var x = xObj as eNW_Link_ALS;
            if( x is null )  return -1;

            int cmp = this.qALS.CompareTo(x.qALS );
            if( cmp != 0 )  return cmp;
            cmp = this.keySt.CompareTo(x.keySt);
            if( cmp != 0 )  return cmp;
            return base.CompareTo(xObj);
        }

        public override string ToString(){
			string  stOrg="--", stDes="--";

            if( OrgN!=null ) stOrg = $"{OrgN.ToString_SameHouseComp()}#{OrgN.no+1}";

            int fb0=qALS.FreeB, fb1=fb0.DifSet(1<<noOrg);
            string stObj = $"AnLS_with_2RCC:{qALS.bitExp.ToRCStringComp()} #{fb0.ToBitStringNZ(9)}";           
            stObj += $"->#{fb1.ToBitStringNZ(9)}";
            if( _withID_ )  stObj = $"ID:{ID} " + stObj;

            if( DesN!=null ){
				stDes = $"{DesN.ToString_SameHouseComp()}";
				if( qConnectTo_RC>=0 )  stDes=qConnectTo_RC.ToRCString();   //=== Refine the display of solutions ===
				stDes += $"#{DesN.no+1}{DesN.TFmark}";
			}

            string st = $"{base.ToString()}[{stOrg}+ => {stObj} ->{stDes}]";   

            return st;
        }

    }

}
