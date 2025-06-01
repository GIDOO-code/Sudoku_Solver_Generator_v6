using System;
using System.Linq;
using System.Collections.Generic;
using static System.Diagnostics.Debug;
using System.Globalization;

using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using System.Resources;
using System.Runtime.InteropServices;
using System.Diagnostics;
using GNPX_space;

namespace GIDOO_space{
    static public partial class Extension_Utility{
		static private readonly UInt128 _filter0081 = (UInt128.One<<81)-1;    // b081
        static private readonly UInt128 qZero   = UInt128.Zero;
        static private readonly UInt128 qOne    = UInt128.One;
	    static private readonly UInt128 qMaxB81 = (UInt128.One<<81)-qOne;
        static private readonly UInt128 _b9     = 0x1FF;

		static private UInt128[]    pConnectedCells81 => AnalyzerBaseV2.ConnectedCells81;

        //========================================================================
        // Note: UInt128 instance variables do not change the original variable.
        //========================================================================
        static public UInt128 DifSet( this UInt128 A, UInt128 B ) => A & ~B;							// (UInt128)(A&(B^UInt128.MaxValue));
        static public UInt128 Set( this UInt128 A, int rc )       => A | qOne<<rc;                      // A = A | qOne<<rc;
        static public UInt128 Set( this UInt128 A, UInt128 B )    => A | B;                             // A = A | B;
        static public UInt128 Reset( this UInt128 A, int rc )     => A & ((qOne<<rc)^UInt128.MaxValue); // A = A & (qOne<<rc)^UInt128.MaxValue;

        static public UInt128 Reset( this UInt128 A, UInt128 B )  => A & B^UInt128.MaxValue;            // A = A & B^UInt128.MaxValue;
        static public bool IsHit( this UInt128 A, int rc )        => (A & (qOne<<rc)) != qZero;
        static public bool IsHit( this UInt128 A, UInt128 B )     => (A & B) != qZero;

        static public bool IsZero( this UInt128 A )               => (A == qZero); 
    
        static public bool IsNotZero( this UInt128 A )            => (A != qZero);

		static public UInt128 ConnectedCells( this UInt128 A )	  => A.IEGet_rc().Aggregate( qZero, (a,rc) => a | pConnectedCells81[rc] );
		static public UInt128 DifSetCon( UInt128 A, UInt128 B )   => A .DifSet( B.ConnectedCells() );

        static public int FindFirst_rc( this UInt128 A ){
            UInt128 w = A & _filter0081;
            for( int rc=0; rc<81; rc++ ){
                if( (w&0x1FF) == 0 ){ w>>=9; rc+=8; continue;}
                if( (w&UInt128.One)>0 )  return rc;
                w >>= 1;
            }
            return -1;
        }

        static public int BitToNum( this UInt128 U128, int sz ){    
            if(U128.BitCount()!=1) return -1;
            for( int k=0; k<sz; k+=8 ){
                uint w = ((uint)(U128>>k)) & 0xFF;
                if( w == 0 )  continue;
                for( int n=0; n<8; n++ ){
                    if( (w & (1<<n)) > 0 ) return (k+n);
                }
            }
            return -1;
        } 
        static public List<int> BitToNumList( this UInt128 U128, int sz=81 ){
			int cnt= U128.BitCount(), cc=0;
            if( cnt <= 0 ) return   null;
			List<int> UList = new();   
            for( int k=0; k<sz; k+=8 ){
                uint w = ((uint)(U128>>k)) & 0xFF;
                if( w == 0 )  continue;
                for( int n=0; n<8; n++ ){
                    if( (w & (1<<n)) > 0 ){
						UList.Add( (k+n) );
						if( (++cc) >= cnt ) break;
					}
                }
            }
            return UList;
        } 
		static public (int,int) BitToTupple( this UInt128 U128 ){
			var L = U128.BitToNumList( 81 );
			switch (L.Count()){
				case 2: return (L[0], L[1]);
				case 1: return (L[0], -1);
				case 0: return (-1,-1);
				default: return (-9999,-9999);
			}
			throw new ArgumentException( $" BitToTupple Exception  U128:{U128.ToBitString81N()}" );
		}

        static public string ToBitStringWSP( this int num, int ncc, bool with_sp ){
            int numW = num;
            string st="";
            for(int k=0; k<ncc; k++ ){
                st += ((numW&1)!=0)? ((k%9)+1).ToString(): ".";
                if(k==8)  st += " ";
                numW >>= 1;
            }
            return st;
        }

        static public string ToBitString81( this UInt128 A ){
            string st="";
            for(int n=0; n<3; n++){
                int bp = (int)( A >> (n*27) );
                int tmp=1;
                for(int k=0; k<27; k++){
                    st += ((bp&tmp)>0)? "1": "."; //External expression
                    tmp = (tmp<<1);
                    if(k==26)         st += " /";
                    else if((k%9)==8) st += " ";
                }
            }
            return st;
        }

		static public string TBS( this UInt128 A )   => A.ToBitString81N( );		// For debugging purposes only
		static public string TBSComp( this UInt128 A )   => A.ToBitString81( );		// For debugging purposes only

        static public string ToBitString81N( this UInt128 A){
            string st="";
            for(int n=0; n<3; n++){
                int bp = (int)( A >> (n*27) );
                int tmp=1;
                for(int k=0; k<27; k++){
                  //st += ((bp&tmp)>0)? ((k%9)+0).ToString(): "."; //Internal expression
                    st += ((bp&tmp)>0)? ((k%9)+1).ToString(): "."; //External expression
                    tmp = (tmp<<1);
                    if(k==26)         st += " /";
                    else if((k%9)==8) st += " ";
                }
            }
            return st;
        }
        static public string ToBitString128( this UInt128 A, int size=128 ){
            string st="";
              
            UInt128 w = A;
            for(int k=0; k<size; k++){
                st += (w&1)>0? $"1": "."; 
                if( k%8==7 ) st += " ";
                w >>=1 ;
            }
            return st;
        }
        static public string ToStringN( this UInt128 A ){
            string st="";
              
            UInt128 w = A;
            for(int k=0; k<128; k++){
                st += (w&1)>0? $"{k%8+1}": "."; 
                if( k%8==7 ) st += " ";
                w >>=1 ;
            }
            return st;
        }



        static public string ToBitString128(this UInt128? A, int size=128){
            string st = "";

            UInt128? w = A;
            for( int k=0; k<size; k++ ){
                st += (w&1)>0? $"1" : ".";
                if( k%8==7 ) st += " ";
                w >>= 1;
            }
            return st;
        }



       static public int BitCount( this UInt128 arg ){
            int N = 0;
            UInt128 w = arg;
            for( int k=0; k<128; k+=32 ){
                        // WriteLine( $"w:{w.ToString81()}" );
                if( w != 0 )  N += ((uint)w).BitCount();
                w >>= 32;
            }
            return N;
        }

        static public string ToString81( this UInt128 arg81 ){
            string st="";
            for(int n=0; n<9; n++){
                int w = (int)arg81&0x1FF;
                st += w.ToBitString(9);
                if( n%3 == 2 )  st += " ";       
                if( n<8 ) st += " ";
                arg81 >>= 9;
            }
            return st;
        }



        static public IEnumerable<(int,bool)> IEGet_index_withFlag( this int bitRep, int length){
            int w = bitRep;
            for( int k=0; k<length; k++ ){
                yield return (k,(w&1)>0);
                w >>= 1;
            }
            yield break;

        }
		
        static public int Get_rcFirst( this UInt128 arg, int sz=128 ){
            for( int k16=0; k16<sz; k16+=16 ){
                int w = (int)(arg>>k16) & 0xFFFF;
                for( int k=0; k<16; k++ ){
                    if( (w&(1<<k)) != 0 )  return (k+k16);
                }
            }
            return -1;
        }

		static public int Get_rc_ifUnique( this UInt128 arg, int sz=128, int notUnique=-1 ){
			if( arg.BitCount() != 1 )  return notUnique;
			return arg.Get_rcFirst();
		}

		static public (int,int) Get_rc1_rc2( this UInt128 arg, int sz=128 ){
			if( arg.BitCount() != 2 )  return (-1,-1);
			var rcList = arg.IEGet_rc().ToList();
			return (rcList[0],rcList[1]);
		}
/*
		static public (UCell,UCell) Get_UC1_UC2( this UInt128 arg, List<UCell> UCList, int sz=81 ){
			if( arg.BitCount() != 2 )  return (null,null);
			var rcList = arg.IEGet_rc().ToList();
			return  (UCList[rcList[0]], UCList[rcList[1]] );
		}
*/

		static public int[] ToArray( this UInt128 A, int sz=81 ){
			int[] AL = new int[sz];
			for(int rc=0; rc<sz; rc++) AL[rc] = (A&(UInt128.One<<rc))>0 ? 1 : 0;
			return AL;
		}

        static public IEnumerable<int> IEGet_rc( this UInt128 A, int mx=81){
            for(int rc=0; rc<mx; rc++){
                if( (A&(UInt128.One<<rc)) > 0  ) yield return rc;
            }
            yield break;
        }
        static public IEnumerable<UInt128> IEGet_rc128( this UInt128 A, int mx=81){
            for(int rc=0; rc<mx; rc++){
                if( (A&(UInt128.One<<rc)) > 0  ) yield return UInt128.One<<rc;
            }
            yield break;
        }

		static public UInt128 Get_CellsWithNo81B( this UInt128 A, List<UCell> board, int noB ){
			var B = A.IEGet_UCell_noB(board,noB).Aggregate( qZero, (a,b) => a| qOne<<b.rc );
		//	var B = A.IEGet_UCell_noB(board,noB).ToList().ToBiitExpression();
			return B;
		}
    }
}