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
using static System.Math;

using GNPX_space;

namespace GIDOO_space{
    static public partial class PuzzleExtender{
		static public UInt128 b081_all = (UInt128.One<<81)-1;      

        static public UInt128 Create_Free_BitExp128( this List<UCell> Us ){			//..... FreeBwk		
            UInt128 B128 = new();
			foreach( var U in Us.Where(u=>u.No==0) ){ B128 |= UInt128.One<<U.rc; }
            return B128;
        }

		static public UInt128 Create_Solved_no_BitExp128( this List<UCell> Us, int noSolved ){			//..... FreeBwk		
            UInt128 B128 = new();
			foreach( var U in Us.Where(u=> Abs(u.No)==noSolved) ){ B128 |= UInt128.One<<U.rc; }
            return B128;
        }

/*
        static public UInt128 Create_bitExp128Rev( this List<UCell> Us ){			//..... FreeBwk		
            UInt128 B128 = Create_Free_BitExp128(Us);
            return B128^b081_all;
        }
*/
		static public UInt128 Select_bitExp128( this UInt128[] B128, int bitF ){
			UInt128 andB128 = UInt128.MaxValue;
			foreach( var no in bitF.IEGet_BtoNo() ){ andB128 = andB128 & B128[no]; }
			return andB128;
		}

        static public UInt128[] Create_bitExp81_9( this List<UCell> Us, bool withFixedB=false ){			//..... FreeBwk
            UInt128[] B128 = new UInt128[9];
            foreach( var U in Us.Where(P=>P.FreeB>0) ){
                UInt128 Urc128 = UInt128.One<<U.rc;
				foreach( int no in U.FreeB.IEGet_BtoNo() )  B128[no] |= Urc128;
            }
			if( withFixedB ){
				foreach( var U in Us.Where(u=>u.No!=0) ) B128[Abs(U.No)-1] |= UInt128.One<<U.rc;
			}

				// for( int no=0; no<9; no++ ){
				//	 WriteLine( $" Create_bitExp81_9 no:#{no+1} B128:{B128[no].ToBitString81N()}" );
				// }
            return B128;
        }

        static public UInt128[] Create_Current_bitExp81_9( this List<UCell> Us, bool withFixedB=false ){			//..... FreeBwk
            UInt128[] B128 = new UInt128[9];
            foreach( var U in Us.Where(P=>P.FreeB>0) ){
                UInt128 Urc128 = UInt128.One<<U.rc;
				int F_C = U.FreeB.DifSet(U.CancelB);
				foreach( int no in F_C.IEGet_BtoNo() )  B128[no] |= Urc128;
            }
			if( withFixedB ){
				foreach( var U in Us.Where(u=>u.No>0) ) B128[U.No-1] |= UInt128.One<<U.rc;
			}

            return B128;
        }


        static public UInt128[] Create_FixedBitExp81_9( this List<UCell> Us ){			//..... FreeBwk
            UInt128[] B128 = new UInt128[9];
			foreach( var U in Us.Where(u=>u.No>0) ) B128[U.No-1] |= UInt128.One<<U.rc;
            return B128;
        }
        static public UInt128[] Create_bitExp81b_9_by_FreeBwk( this List<UCell> Us ){		//..... FreeBwk
            UInt128[] B128 = new UInt128[9];
            foreach( var U in Us ){
                UInt128 Urc128 = UInt128.One<<U.rc;
				foreach( int no in U.FreeBwk.IEGet_BtoNo() )  B128[no] |= Urc128;
            }
            return B128;
        }

        static public UInt128 Create_bitExp_bivalue( this List<UCell> Us ){
            UInt128 B128 = UInt128.Zero;
            foreach( var U in Us.Where(p=>p.FreeBC==2) )  B128 |= UInt128.One<<U.rc;
            return  B128;
        }

    }
}