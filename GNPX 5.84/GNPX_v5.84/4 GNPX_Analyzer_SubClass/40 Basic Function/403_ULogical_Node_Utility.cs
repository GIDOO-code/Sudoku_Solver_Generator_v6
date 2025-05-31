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
    static public class ULogical_Node_Utility{     
        static private UInt128[]     pHouseCells81     = AnalyzerBaseV2.HouseCells81;
        static private UInt128[]     pConnectedCells81 = AnalyzerBaseV2.ConnectedCells81;


		static private readonly int[]   _rcbFrame_Value;

        static public int To_rcbFrame( this int rc) => (1<<(rc.ToBlock()+18)) | (1<<(rc%9+9)) | (1<<rc/9);
		static public int rcbFrame_Value( this int rc) => _rcbFrame_Value[rc];
        static public int Ceate_rcbFrameAnd( this UInt128 b081 )  => b081.IEGet_rc().Aggregate( 0x7FFFFFF, (p,q) => p&rcbFrame_Value(q) );
        static public int Ceate_rcbFrameOr( this UInt128 b081 )   => b081.IEGet_rc().Aggregate( 0, (p,q) => p|rcbFrame_Value(q) );

		static public int Get_rcbFrame( this int rcbFrame, int type ) => (type==0)? rcbFrame&0x1FF: (type==1)? (rcbFrame>>9)&0x1FF: (rcbFrame>>18)&0x1FF;
        static public UInt128 Aggregate_ConnectedOr( this UInt128 B) => B.IEGet_rc().Aggregate( UInt128.Zero, (b,rc)=> b| pConnectedCells81[rc] );

		static public UInt128 Aggregate_ConnectedAnd( this UInt128 B) => B.IEGet_rc().Aggregate( (UInt128.One<<81)-1, (b,rc)=> b& pConnectedCells81[rc] );
        static public UInt128 Aggregate_ConnectedAnd( this ULogical_Node ULGN ) => ULGN.b081.Aggregate_ConnectedAnd();

		static ULogical_Node_Utility(){
			_rcbFrame_Value = new int[81];
			for( int rc=0; rc<81; rc++ )  _rcbFrame_Value[rc] = To_rcbFrame(rc);
		}
	}

}