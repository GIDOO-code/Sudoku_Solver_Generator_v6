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
using GNPX_space.Properties;

namespace GNPX_space {
    //ALS(Almost Locked Set) 
    // ALS is a state where there are "n+1 candidate digits" in "n cells" belonging to the same house. 
    // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page26.html

	// *==*==*==*==*==*==*==*==*==*
	//		version 5.1 
	// *==*==*==*==*==*==*==*==*==*

    public class UAnLS: USubset{

		public UAnLS             preALS = null;
        public (UAnLS,int)       preALS_no = (null,-9);
		public List<(UAnLS,int)> ConnectedALS;


        public UAnLS(){ }

        public UAnLS( int ID, int FreeB, List<UCell> UCellLst ): base( ID, FreeB, UCellLst ){}

        public bool IsPureALS(){
            //Pure ALS : not include locked set of proper subset
            if( Size<=2 ) return true;
            for( int sz=2; sz<Size-1; sz++ ){
                var cmb=new Combination(Size,sz);
                while(cmb.Successor()){
                    int fb=0;
                    for(int k=0; k<sz; k++ )  fb |= UCellLst[cmb.Index[k]].FreeB;
                    if( fb.BitCount()==sz ) return false;
                }
            }
            return true;
        }


		public override string ToString(){
            string st = $"\nUAnLS << ID:{ID} >>  [ Size:{Size} Level:{Level} ] ";
            st += $"  :{FreeB.ToBitString(9)}  FreeBwk:{FreeBwk.ToBitString(9)}\n";
            st += $" rcbFrame:{(rcbFrame).ToBitString27rcb()}";
			st += $" IsInsideBlock:{(IsInsideBlock? "@":"-")}";
            st += $"         bitExp:{bitExp.ToBitString81()}\n";
			st += $"connectedB81:{connectedB81.ToBitString81()}";

            for(int k=0; k<UCellLst.Count; k++){
                st += "\n------";
				UCell P = UCellLst[k];
                st += $" rc:{P.rc.ToRCString()}";
                st += $" FreeB:{P.FreeB.ToBitString(9)}";
				st += $" FreeBwk:{P.FreeBwk.ToBitString(9)}";			               
				st += $" // rcbFrame: {P.rcbFrame.ToBitString27rcb(digitB:false)}";
            }
            return st;
        }

        public new string ToStringA(){
            string st = $"<> UAnLS ID:{ID} Size:{Size} Level:{Level}  FreeB:{FreeB.ToBitString(9)} rc:{ToStringRCN()}";
            return  st;
        }

		public new string ToString_FreeBwk(){
			string st = $"UAnLS << ID:{ID} >>  [ Size:{Size} Level:{Level} ]";
			st += $"  FreeB:{FreeB.ToBitString(9)}  FreeBwk:{FreeBwk.ToBitString(9)}\n";
			st += $"         bitExp {bitExp.ToBitString81N()}\n";
			for(int k=0; k<UCellLst.Count; k++){
				st += "------";
				int rcW = UCellLst[k].rc;
				st += $" rc:{rcW.ToRCString()}";
				st += $" FreeBwk:{UCellLst[k].FreeBwk.ToBitString(9)}";
				st += $" rcb:b{(rcbBlk).ToBitString(9)}";
				st += $" c{rcbCol.ToBitString(9)}";
				st += $" r{rcbRow.ToBitString(9)}";
				//st += $" connectedB81:{connectedB81.ToBitString81N()}";
			  	st += "\n";
			}
            return st;
        }
	}

}
