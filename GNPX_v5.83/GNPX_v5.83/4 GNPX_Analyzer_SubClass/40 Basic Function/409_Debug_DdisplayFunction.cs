using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static System.Diagnostics.Debug;
using static System.Math;
//using GIDOO_Lib;
//using OpenCvSharp;
using GNPX_space;

namespace GIDOO_space{
    public static class Utility_Display{
        static public char[] _sep={' ',',','\t'};
        static public char[] _sepC={','};
        static public string _backupDir="backupDir";

		static public void GNPX_StreamWriter( string fName, string line, bool append=true ){
		    using( var fpW=new StreamWriter( fName, append:true, Encoding.UTF8) ){
                fpW.WriteLine(line);
            }
		}

		static public void GNPX_StreamWriter_WithTime( string fName, string message, bool append=true ){
			using(var fpW=new StreamWriter(fName,append,Encoding.UTF8)){
                fpW.WriteLine($"{DateTime.Now}  {message}");
            }
		}




	#region __DBUG_Print2
		static public string  __DBUG_Print2( this List<UCell> UB, bool sqF, string st="" ){
		  //return __DBUG_Print2( UB.ConvertAll(p=>Abs(p.No)), sqF, st:st );
			return __DBUG_Print2( UB.ConvertAll(p=>p.No), sqF, st:st );
		}
		

		static public string __DBUG_Print2( this List<int> X, bool sqF, string st="" ){
			return __DBUG_Print2( X.ToArray(), sqF, st:st );
		}
        static public string __DBUG_Print2( this int[] X, bool sqF, string st="" ){
            string   stL="", stL2="  ", stL3="";
			if(sqF)  stL = $"{st}\n";

			for( int rc=0; rc<81; rc++ ){
				int no = X[rc];
				if( (rc%9) == 0 ){ stL += (rc/9+1).ToString("     ##0:"); stL3=""; }
				stL  += " "+ ToNumStringPM(no);
				stL3 += " "+ ToNumStringPM(Max(no,0));
				stL2 += ToNumStringP(no);

				if( (rc%9) == 8 ){ stL+= $"   {stL3}\n"; stL2 += " "; stL3=""; }
			}
			WriteLine( $"{stL}\n{stL2}" );
			return stL;

			string ToNumStringP( int no ) => (no>0)? no.ToString(): ".";
			string ToNumStringPM( int no ) => (no==0)? " .": (no>0)? no.ToString(" #" ): no.ToString("##" );
        }


        static public string __DBUG_Print2( int[,] pSol99, bool sqF, string st="" ){
            string   stL="", stL2="  ", stL3="";
			if(sqF)  stL = $"{st}\n";

			for( int rc=0; rc<81; rc++ ){
				int no = pSol99[rc/9,rc%9];
				if( (rc%9) == 0 ){ stL += (rc/9+1).ToString("     ##0:"); stL3=""; }
				stL  += " "+ ToNumStringPM(no);
				stL3 += " "+ ToNumStringPM(Max(no,0));
				stL2 += ToNumStringP(no);

				if( (rc%9) == 8 ){ stL+= $"   {stL3}\n"; stL2 += " "; stL3=""; }
			}
			WriteLine( $"{stL}\n{stL2}" );
			return stL;

			string ToNumStringP( int no ) => (no>0)? no.ToString(): ".";
			string ToNumStringPM( int no ) => (no==0)? " .": (no>0)? no.ToString(" #" ): no.ToString("##" );
		}

		static public string __DBUG_Print3( int no, UInt128 B81, bool sqF=false, string st="" ){
			string stL = $"{st}\n";
			for( int rc=0; rc<81; rc++ ){
				if( (rc%9) == 0 )  stL += $" row{(rc/9)+1}";
				int b = (int)((B81>>rc) & 0b1);
				stL += (b==0)? " .": $" {no+1}";
				if( (rc%9) == 8 )  stL += "\n";
			}
			WriteLine( stL );
			return stL;
		}
	#endregion



	}
}
