using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPX_space{

    public class UCellLinkExt: IComparable{
        public int ID;
        public int type;        // 1:Strong 2:Weak
        public bool LoopFlag;   // Last Link


        public readonly int noH;
        public readonly int noT;
        public readonly UCell UCellHead1;
        public List<UCellLink> UCellLinkLst;
        public UCellLink LK0 => UCellLinkLst[0];

        public int      keyNRCRC;

        public int rc1 { get => UCellHead1.rc; }
//??    public readonly Bit81 B81;

        public UCellLinkExt() { }
        public UCellLinkExt( UCellLink ULK0, int noH, int noT ){
            UCellLinkLst = new List<UCellLink>();
            UCellLinkLst.Add(ULK0);
            
            this.type = ULK0.type;
            this.noH = noH;
            this.noT = noT;
            this.keyNRCRC = (type<<24) | ((noH<<8 | ULK0.rc1)<<12) | ((noT<<8 | ULK0.rc2));
        }

        public void UCellLinkExt_Add(UCellLink ULK0 ){
            // public eBit981( List<UCell> pBOARD, bool eSet=true ){
        }


        public int CompareTo( object obj ){
            UCellLink Q = obj as UCellLink;
            return this.LK0.CompareTo(Q);

            /*
                        UCellLinkExt Q = obj as UCellLinkExt;
                        if( this.type != Q.type ) return (this.type-Q.type);
                        if( this.noH != Q.noH )   return (this.noH-Q.noH);
                        if( this.noT != Q.noT )   return (this.noT-Q.noT);

                        if( this.LK0 != Q.LK0 )  return this.LK0.CompareTo(Q.LK0);
                        if( this.UCellLinkLst.Count != Q.UCellLinkLst.Count )  return this.UCellLinkLst.Count - Q.UCellLinkLst.Count;
                        if( this.UCellHead1 != Q.UCellHead1) return this.UCellLinkLst[0].CompareTo(Q.UCellLinkLst[0]);

                        for( int n=0; n<UCellLinkLst.Count; n++ ){
                            var x = this.UCellLinkLst[n].CompareTo(Q.UCellLinkLst[n]);
                            if( x != 0 )  return x;
                        }
                        return 0;
            */
        }

        public override bool Equals(object obj){
            UCellLinkExt Q = obj as UCellLinkExt;
            if( this.type != Q.type ) return false;
            if( this.noH  != Q.noH  ) return false;
            if( this.noT  != Q.noT  ) return false;

            if( this.LK0  != Q.LK0  ) return false;
            if( this.UCellLinkLst.Count != Q.UCellLinkLst.Count ) return false;
            if( this.UCellHead1 != Q.UCellHead1 ) return false;

            for (int n = 0; n<UCellLinkLst.Count; n++ ){
                var x = this.UCellLinkLst[n].CompareTo(Q.UCellLinkLst[n]);
                if( x != 0 ) return false;
            }
            return true;
        }

        public override string ToString( ){
            string st = $"noH:#{noH+1} ====>";
            UCellLinkLst.ForEach( LK => st += $" ¡ {LK}" );
            st += $" ¡ ====> #{noT+1}";
            return st;
        }
        public override int GetHashCode( ){ return base.GetHashCode(); }
    }

}