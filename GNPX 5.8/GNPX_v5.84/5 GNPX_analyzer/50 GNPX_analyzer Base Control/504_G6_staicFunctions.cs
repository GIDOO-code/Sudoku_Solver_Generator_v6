using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Policy;
using System.Data;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows.Documents;
using static GNPX_space.Research_trial;

namespace GNPX_space{
	// Reference to SudokuCoach.
	// https://www.taupierbw.be/SudokuCoach/SC_Exocet.shtml 


	static public partial class G6_staticFunctions{   

	    static private readonly UInt128 qZero   = UInt128.Zero;
        static private readonly UInt128 qOne    = UInt128.One;
	    static private readonly UInt128 qMaxB81 = (UInt128.One<<81)-qOne;
        static private readonly UInt128 _b9     = 0x1FF;
		static public  readonly UInt128 _na_    = UInt128.MaxValue;
		static private readonly int     _na9_   = (1<<9)-1;

		static private readonly char[]  _sep=new Char[]{ ' ', ',', '\t' };
		static private readonly char[]  _sep2=new Char[]{ ' ', '\t' };

		static public string  TBSP( this UInt128 Q) => ((Q==_na_)? "n/a": Q.TBS());

		static public int dir_rc_toHouse( int dir, int rc ) => (dir==0)? rc/9: rc%9+9;

		static public List<UCell> pBOARD;

		static public (UInt128,int) SplitInTo( this UInt128 Q ) => (Q&qMaxB81,(int)Q>>100);


		static public int DirRCtoHouse( this (int,int) Q ) => (Q.Item1==0)? (Q.Item2)/9: (Q.Item2)%9+9;
		static public int DRCHf( this (int,int) Q ) => (Q.Item1==0)? (Q.Item2)/9: (Q.Item2)%9+9;	// ... Abbreviations


		
		static public int Get_FreeB( this int rc, bool diffCan=false ){
			UCell U = pBOARD[rc];
			if( U.No != 0 ){ return  1<<(Abs(U.No)-1); }
			else{ 
				int FB = U.FreeB;
				if( diffCan ) FB = FB.DifSet(U.CancelB);
				return FB;
			}
		}
		static public int Get_FreeB( this UInt128 B, bool diffCan=false ){
			return  B.IEGet_rc().Aggregate( 0, (a,rc) => a| rc.Get_FreeB(diffCan) );
		}

		static public (int,int) Get_SingleMore( this UInt128 B ){
			int oneF=0, twoF=0;
			foreach( var U in B.IEGet_UCell(pBOARD) ){
				int F = U.FreeB;
				twoF |= oneF & F;			// exist in more than one place
				oneF |= F;					// exist somewhere
			}
			return (oneF.DifSet(twoF),twoF);		// only one
		}


		static 	public void __MatrixPrint( this UInt128 Flag, UInt128 BP0, string message ){
			int[] FlagArray = Flag.ToArray();
			int[] BPArray0 = BP0.ToArray();

			WriteLine( message.splitPad27( ) );
			for(int r0=0; r0<81; r0+=9){
				string st = $"   r{r0/9:##0}|{____MatrixToLine(r0,FlagArray,BPArray0)}";
				WriteLine(st);
			}
		}

		static 	public void __MatrixPrint( this UInt128 Flag, UInt128 BP0, UInt128 BP1, string message ){
			int[] FlagArray = Flag.ToArray();
			int[] BPArray0 = BP0.ToArray();
			int[] BPArray1 = BP1.ToArray();
			
			WriteLine( message.splitPad27( ) );
			for(int r0=0; r0<81; r0+=9){
				string st = $"   r{r0/9:##0}|{____MatrixToLine(r0,FlagArray,BPArray0)}";
				st += $"    {____MatrixToLine(r0,FlagArray,BPArray1)}";
				WriteLine(st);
			}
		}

		static 	public void __MatrixPrint( this UInt128 Flag, UInt128 BP0, UInt128 BP1, UInt128 BP2, string message ){
			int[] FlagArray = Flag.ToArray();
			int[] BPArray0 = BP0.ToArray();
			int[] BPArray1 = BP1.ToArray();
			int[] BPArray2 = BP2.ToArray();

			WriteLine( message.splitPad27( ) );
			for(int r0=0; r0<81; r0+=9){
				string st = $"   r{r0/9+1:##0}|{____MatrixToLine(r0,FlagArray,BPArray0)}";
				st += $"    {____MatrixToLine(r0,FlagArray,BPArray1)}";
				st += $"    {____MatrixToLine(r0,FlagArray,BPArray2)}";
				WriteLine(st);
			}
		}

		static 	public void __MatrixPrint( this UInt128 Flag, UInt128 BP0, UInt128 BP1, UInt128 BP2, UInt128 BP3, string message ){
			int[] FlagArray = Flag.ToArray();
			int[] BPArray0 = BP0.ToArray();
			int[] BPArray1 = BP1.ToArray();
			int[] BPArray2 = BP2.ToArray();
			int[] BPArray3 = BP3.ToArray();

			WriteLine( message.splitPad27( ) );
			for(int r0=0; r0<81; r0+=9){
				string st = $"   r{r0/9+1:##0}|{____MatrixToLine(r0,FlagArray,BPArray0)}";
				st += $"    {____MatrixToLine(r0,FlagArray,BPArray1)}";
				st += $"    {____MatrixToLine(r0,FlagArray,BPArray2)}";
				st += $"    {____MatrixToLine(r0,FlagArray,BPArray3)}";
				WriteLine(st);
			}
		}

		static private string splitPad27( this string st, int spN=6 ){
			List<string> eList = st.Split(_sep,StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(p=>p.PadRight(27) ); // sep: include comma
		  //List<string> eList = st.Split(_sep2,StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(p=>p.PadRight(27) ); 
			string stTmp = "\n" + (new string(' ',spN)) + string.Join("    ",eList);
			return  stTmp;
		}

		static string  ____MatrixToLine( int r0, int[] FlagArray, int[] BPArray ){
			string st="";
			for(int c=0; c<9; c++){
				int p = BPArray[r0+c];
				st += (FlagArray[r0+c]!=0)? " *": "  ";
				st += (p==0)? ".": (c+1).ToString("#");
			}
			return st;
		}



		
		static 	public void __CellsPrint( this List<UCell> Cells, string message ){
			WriteLine(message);
			for(int r0=0; r0<81; r0+=9){
				string st = $"   r{r0/9:##0} | {____CellToLine(r0,Cells)}";
				WriteLine(st);
			}
			return;

				string  ____CellToLine( int r0, List<UCell> Cells, int width=10 ){
					string st="", stCell="";
					for(int c=0; c<9; c++){
						UCell P = Cells[r0+c];
						if( P.No!=0 ){
							int no = P.No;
							stCell = (no<0)? $"-<{Abs(no)}>-": $"<{Abs(no)}>";  
						}
						else if( P.FreeB == 0 )  stCell = "_0_";
						else if( P.FreeB == _na9_ )  stCell = "_n/a_";
						else{ stCell = P.FreeB.TBSnD(); }
				
						st += stCell.PadRight(width);
					}
					return st;
				}
		}



		static 	public void __CellsPrint_withFrame( this List<UCell> Cells, string[] Marks, string message="", int width=9 ){
			WriteLine(message);
			string stLine3 = new string('-', (width+3)*3-1 );
			string stLine = $"*{stLine3}*{stLine3}*{stLine3}*";

			for(int r0=0; r0<81; r0+=9){
				if( (r0%27)==0 ) WriteLine(stLine);
				string st = $"{____CellToLinet_wF(r0,Cells, Marks,  width:width)}";
				WriteLine(st);
			}
			WriteLine(stLine);
			return;

				string  ____CellToLinet_wF( int r0, List<UCell> Cells, string[] Marks, int width=9 ){
					string st="";
					for(int c=0; c<9; c++){
						string stCell = ((c%3)==0)? " | ": "   ";
						UCell P = Cells[r0+c];
						if( P.No!=0 ){
							int no = P.No;
							stCell += (no<0)? $"-<{Abs(no)}>-": $"<{Abs(no)}>";  
						}
						else{ stCell += P.FreeB.TBSnD(); }
						if( Marks!=null && Marks.Length==81 ){
							string stMark = Marks[r0+c];
							if( stMark!=null && stMark!="" )  stCell += " "+stMark;
						}
						st += stCell.PadRight(width+3);
					}
					st += " |";
					return st.TrimStart();
				}
		}

		static 	public void _Dynmic_CellsPrint_withFrame( this List<UCell> Cells, string[] Marks, string message="", int width=9 ){
			WriteLine(message);
			//	string stLine3 = new string('-', (width+3)*3-1 );
			//string stLine = $"*{stLine3}*{stLine3}*{stLine3}*";

			string[][] stDD = new string[9][];
			int[]	   colSize = new int[9]; 
			for( int r=0; r<9; r++ )  stDD[r] = ___D_CellToLinet_wF(r*9,Cells, Marks,  width:width);
			
			string  stLine = "*";
			for( int c=0; c<9; c++ ){
				int sz=0;
				for( int r=0; r<9; r++ )  sz = Max( sz, stDD[r][c].Length );
				colSize[c] = sz;
				stLine += new string('-',sz+4);
				stLine += (c%3==2)? "*": "-";
			}


			for(int r=0; r<9; r++){
				if( (r%3)==0 ) WriteLine(stLine);
				string st = "| ";
				for( int c=0; c<9; c++ ){
					int n = colSize[c]+1;
					st += " " + stDD[r][c].PadRight(n);
					st += (c%3==2)? " | ": "   ";
				}
				WriteLine(st);
			}
			WriteLine(stLine);
			return;

				string[]  ___D_CellToLinet_wF( int r0, List<UCell> Cells, string[] Marks, int width=9 ){
					string[] stD = new string[9];
					string st="";
					for(int c=0; c<9; c++){
						string stCell = "";
						UCell P = Cells[r0+c];
						if( P.No!=0 ){
							int no = P.No;
							stCell += (no<0)? $"-<{Abs(no)}>-": $"<{Abs(no)}>";  
						}
						else{ stCell += P.FreeB.TBSnD(); }
						if( Marks!=null && Marks.Length==81 ){
							string stMark = Marks[r0+c];
							if( stMark!=null && stMark!="" )  stCell += " "+stMark;
						}
						stD[c] = stCell;
					}
					st += " |";
					return stD;
				}
		}


		static public IEnumerable<(int,int)> PairGenerator( int PBitA, int PBitB, int skip, int size=9, bool uniqueB=false ){
			if( PBitA==0 || PBitB==0 )  yield break;

			int[] AList = PBitA.IEGet_BtoNo(size).ToArray();
			int[] BList = PBitB.IEGet_BtoNo(size).ToArray();

			foreach( int na in AList ){
				foreach( int nb in BList ){
					if(uniqueB && na==nb )  continue;
					yield return (na,nb);
					if( skip==0 )  break;
				}
			}
			yield break;
		}
	}
}