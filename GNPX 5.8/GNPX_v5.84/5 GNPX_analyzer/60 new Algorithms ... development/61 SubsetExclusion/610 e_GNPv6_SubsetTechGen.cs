using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.IO;
using System.Text;

namespace GNPX_space{
    public partial class SubsetTechGen: AnalyzerBaseV2{
        static private UInt128 qZero  = UInt128.Zero;
        static private UInt128 qOne   = UInt128.One; 

		private int stageNoPMemo = -9;
		private ALSLinkMan  fALS;
		public  List<UAnLS> ALSList_Leaf=null;

		public SubsetTechGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){
			fALS = new ALSLinkMan(pAnMan);
        }


		public void Prepare_SubsetTec(){
			fALS.Prepare_ALSLink_Man( nPlsB:2, maxSize:5, debugPrintB:false );
			ALSList_Leaf = fALS.ALSList;	//Create_ALSList( nPls:1, minSize:1, maxSize:5 );	
		}

		internal bool		debugPrint = false;
		internal string     debug_Result = "";
		internal string		fName_debug = "SubsetTechGe.txtn";
		internal void Debug_StreamWriter( string LRecord ){
#if DEBUG
			if(!debugPrint)  return;
            using( var fpW=new StreamWriter(fName_debug,true,Encoding.UTF8) ){
                fpW.WriteLine(LRecord);
            }
#endif
		}	


		public class DigitsPattern{
			public  USubset Stem;
			public  int   Size=2;
			public  int   FreeB;
			public  int   FreeBX;
			public  int[] dLst;

			public  int d0{ get=>dLst[0]; set=>dLst[0]=value; }
			public  int d1{ get=>dLst[1]; set=>dLst[1]=value; }
			public  int d2{ get=>dLst[2]; set=>dLst[2]=value; }


			private bool _validB=true;
			public bool validB{ 
				set{ _validB=value; if(!value) FreeBX=0; } 
				get{ return _validB; }
			}

			public DigitsPattern( USubset Stem, int d0, int d1, int d2=-1 ){
				this.Stem = Stem;
				this.Size = Stem.Size;
				dLst = new int[3];
				this.dLst[0]=d0; this.dLst[1]=d1; this.dLst[2]=d2;

				this.FreeB = 1<<d0 | 1<<d1;
				if( d2 >= 0 ){ this.Size=3; this.FreeB |= 1<<d2; }
				this.FreeBX = this.FreeB;
			}

		    public override bool Equals( object obj ){
				DigitsPattern Q = obj as DigitsPattern;
				if( Q==null )  return true;
				if( this.Size!=Q.Size )   return false;
				if( this.d0!=Q.d0 || this.d1!=Q.d1 ) return false;
				if( this.Size==3 && this.d2!=Q.d2 )  return false;
				return true;
			}
		    public bool Equals( UCell Q ){
				if( Q==null )  return true;
				if( this.Size!=Q.FreeBC )   return false;
				if( this.FreeB!=Q.FreeB ) return false;
				return true;
			}

			public override string ToString( ){
				string st = Stem.UCellLst.Aggregate("", (p,q) => p+"  "+q.rc.ToRCString()+"#"+q.FreeB.ToBitString(9));
				st += $" | #{d0+1} #{d1+1}";
				if( Size == 3 )  st += $" #{d2+1}";
				st += $"  FreeB:{FreeB.ToBitString(9)}  FreeBX:{FreeBX.ToBitString(9)}";
				st += _validB? " .": " X";

				return st;
			}
		}

		public class UEval{
			public int FreeB0;
			public int FreeBX;
			public int[] noCnt=new int[9];

			public UEval(int freeB0) {
				this.FreeB0 = freeB0;
			}
 		}
	}
}