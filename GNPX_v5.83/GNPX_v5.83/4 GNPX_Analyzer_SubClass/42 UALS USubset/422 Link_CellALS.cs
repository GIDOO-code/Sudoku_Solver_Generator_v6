using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;
using System.Xml.Serialization;

using GIDOO_space;
using System.Windows.Documents;

namespace GNPX_space {
    public class Link_CellALS: IComparable{
//        static public UInt128[]    pConnectedCells81{ get{ return AnalyzerBaseV2.ConnectedCells81; } }
  
        public readonly UCell UC;
        public readonly UAnLS  ALS;
        public readonly int   nRCC=-1; //no:0..8 (not bit representation)

        public int            noBcand;

        public Link_CellALS( UCell UC, UAnLS ALS, int nRCC ){
            this.UC=UC; this.ALS=ALS; this.nRCC=nRCC;
        }
        public  override bool Equals( object obj ){
            var A = obj as Link_CellALS;
            return (this.ALS.ID==A.ALS.ID);
        }

        public int CompareTo( object obj ){
            Link_CellALS A = obj as Link_CellALS;
            return (this.ALS.ID-A.ALS.ID);
        }
        public int CompareTo( ){
            return (this.ALS.ID);
        }

        public override string ToString(){
            string st = $"Link_CellALS nRCC:#{nRCC+1}";
            st += " UCell:"+UC+"\rALS:"+ ALS;
            return st;
        }
        public override int GetHashCode(){ return base.GetHashCode(); }
    }
}
