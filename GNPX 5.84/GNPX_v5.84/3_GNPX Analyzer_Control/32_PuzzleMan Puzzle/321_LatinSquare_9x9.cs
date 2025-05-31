using System;
using System.Collections.Generic;
using static System.Math;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Linq;
using System.Windows.Documents;
using System.Xml.Linq;

namespace GNPX_space{

	public class LatinSquare_9x9: IComparable{
		public long   hashVal;
//		public ulong  keySol;
		public int[]  SolX;
		public bool   validF = true;

		public LatinSquare_9x9( List<int> SolXList ){
			this.SolX = SolXList.ToArray();
			this.hashVal = this.SolX.Get_hashValue_int81();
			
//			ulong ukey = 0;
//			for( int k=0; k<16; k++ )  ukey = (ukey<<4) | (ulong)SolXList[k];
//			this.keySol = ukey;
		}
									
		public LatinSquare_9x9( int[,] Sol99, bool ApplyPatternB=false, int[] GPat81=null, bool debugB=false ){

			SolX = Sol99.Cast<int>().ToArray();
			if( ApplyPatternB ){
				for( int rc=0; rc<81; rc++ ){ if( GPat81[rc]==0 )  SolX[rc] = 0; }
			}
			this.hashVal = SolX.Get_hashValue_int81();

			if(debugB)  SolX.__DBUG_Print2( sqF:true, $"LatinSquare_9x9  hv:{hashVal}" ); 
		}

		public int CompareTo( object obj) {
			LatinSquare_9x9 LS = (LatinSquare_9x9)obj;
			return (this.hashVal==LS.hashVal)? 0: (this.hashVal<LS.hashVal)? -1: 1;
		}

		public int CompareTo2( object obj ){
			LatinSquare_9x9 LS = (LatinSquare_9x9)obj;
//			if( this.keySol == LS.keySol ){
				for( int k=0; k<81; k++ ){
					if( this.SolX[k] == LS.SolX[k] )  continue;
					return  (this.SolX[k] - LS.SolX[k]);
				}
				return 0;
//			}
//			return (this.keySol<LS.keySol)? -1: 1;
		}

		public override string ToString(){
			string stL = string.Join("",SolX).Replace("0",".");
			//string st = $"hv:{hashVal} {stL.Substring(0,20)}";
			string st = $"hv:{hashVal} {stL}";
			return st;
		}
	}
}