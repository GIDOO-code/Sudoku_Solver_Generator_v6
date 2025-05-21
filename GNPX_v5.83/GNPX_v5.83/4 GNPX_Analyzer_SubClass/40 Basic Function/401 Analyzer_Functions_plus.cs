using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Windows.Media.Animation;
//using Accessibility;
using System.Security.Policy;
using System.Windows.Documents;

namespace GNPX_space{

    // UInt128 is available in .net7.0.
    // Since int128 is implemented as a struct, Bit81A is not updated.
    //  ... Under development. Has bugs.

    static public class _Static_Function_for_Int128{            //UInt128 ver.
		static private readonly UInt128 _filter0081 = (UInt128.One<<81)-1;    // b081
        static private readonly UInt128 qZero  = UInt128.Zero;
        static private readonly UInt128 qOne   = UInt128.One;
	  //static private readonly UInt128 qMaxB81   = (UInt128.One<<81)-qOne;
        static private readonly UInt128 _b9    = 0x1FF;

        static public int  UInt128AggregateFreeB( this UInt128 U, List<UCell> XLst ) => U.IEGet_rc().Aggregate(0,(Q,q)=>Q|XLst[q].FreeB);

		static public UInt128 Get_rc_BitExpression( this List<UCell> aBOARD, int no=-1 ){
            UInt128 cells128=0;
            int noB = (no>=0)? (1<<no): 0;
            foreach( var p in aBOARD ){
                if( p.No != 0 )  continue; 
                if( (no>=0 && no<=8) && (p.FreeB&noB)==0 )  continue;
                cells128 |= (UInt128)1<<p.rc;   
            }
            return cells128;
        }

		static public UInt128 Get_BitExpression( this UInt128 UC81,  List<UCell> aBOARD, int noB ){
            UInt128 cells128=0;
			foreach( var p in UC81.IEGet_UCell_noB(aBOARD,noB).Where(q=> (q.FreeB&noB)>0) ){
                cells128 |= (UInt128)1<<p.rc;   
            }
            return cells128;
        }

		static public UInt128 ToBiitExpression( this List<UCell> UCellList ){
			var B = UCellList.Aggregate(qZero, (a,b) => a| qOne<<b.rc );
			return B;
        }

        static public IEnumerable<(UCell,bool)> IEGet_cell_withFlag( this int bitRep, List<UCell> cells){
            int length = cells.Count;
            int w = bitRep;
            for( int k=0; k<length; k++ ){
                yield return (cells[k],(w&1)>0);
                w >>= 1;
            }
            yield break;

        }

        static public IEnumerable<UCell> IEGet_UCell( this UInt128 bitRep, List<UCell> aBOARD ){
            int rc=0;
            UInt128 w=bitRep;
            do{
                if( (w&1) > 0 )  yield return aBOARD[rc]; 
                w >>= 1;
            }while(w>0 && ++rc<81 );
            yield break;
        }

		static public string ToString( this (UAnLS,int) UA_no ){
			var (UA,no) = UA_no;
			string st = $"no:{no.ToBitString()}\nUA:{UA}";
			return st;		
		}
        static public IEnumerable<UCell> IEGet_UCell_noB( this UInt128 bp, List<UCell> pBOARD, int noBX ){ //nx=0...8        
            for(int k=0; k<81; k++ ){
                if( ((bp>>k)&1)==0 ) continue;
                UCell P = pBOARD[k];
                if( (P.FreeB&noBX) > 0 )  yield return P;
            }
        }

		static public UInt128 Get_UInt128_noB( this UInt128 bp, List<UCell> pBOARD, int noBX ){
			return  bp.IEGet_UCell_noB(pBOARD,noBX).ToList().ToBiitExpression();
		/*
			UInt128 B = qZero;
			for(int k=0; k<81; k++ ){
                if( ((bp>>k)&1)==0 ) continue;
                UCell P = pBOARD[k];
                if( (P.FreeB&noBX) > 0 ) B |= qOne<<P.rc;
            }
			return B;
		*/
		}


		static public string TBScmpRC( this UInt128 U, string msg="", int mx=81 ) => ToRCStringComp( U, msg, mx, rcPlus:true );
		static public string TBScmp( this UInt128 U, string msg="", int mx=81 ) => ToRCStringComp( U, msg, mx );
		static public string TBScmp( this UInt128 U, int freeB, int mx=81 )     => ToRCStringComp( U, freeB, mx );
		static public string TBScmp_No( this UInt128 U, string stAdd="", string msg="", int mx=81) => ToRCStringComp_AddNo( U, stAdd, msg, mx );


        static public string ToRCStringComp( this UInt128 U, string msg="", int mx=81, bool rcPlus=false ){
			if( U==UInt128.Zero || U==UInt128.MaxValue ) return "_n/a_";
            var stRCList = U.IEGet_rc(mx:81).ToList().ConvertAll( p=>p.ToRCString() );
            string st = string.Join(' ',stRCList);
			st = st.ToString_SameHouseComp();
			if(rcPlus)  st += "(" + string.Join(",",U.BitToNumList()) + ")";
            return st+msg;
        }
		static public string ToRCStringComp( this UInt128 U, int freeB, int mx=81 ){
			return ToRCStringComp( U, " #"+freeB.ToBitStringN(9) );
		}

        static public string ToRCStringComp_AddNo( this UInt128 U, string stAdd="", string msg="", int mx=81){
            var stRCList = U.IEGet_rc(mx:81).ToList().ConvertAll( p=>p.ToRCString() );
            string st = string.Join(' ',stRCList);
			st = st.ToString_SameHouseComp();

			if( stAdd != null ){
				List<string> eLst = st.Split(new char[]{' '}).ToList().ConvertAll( p=>p+stAdd);
				st = string.Join(' ',eLst);
				st = "{ "+st+" }";
			}
            return st+msg;
        }

    }  
}