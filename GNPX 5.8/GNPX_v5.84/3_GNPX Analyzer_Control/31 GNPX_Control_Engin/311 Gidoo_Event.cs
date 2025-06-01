using System;
using System.Linq;
using System.Collections.Generic;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using System.Windows.Documents;

namespace GNPX_space{
	using sysWin = System.Windows;

    public delegate void GNPX_EventHandler( object sender, Gidoo_EventHandler args );

    public class Gidoo_EventHandler: EventArgs{
	    public string eName;
	    public int    ePara0;
        public int    ePara1;
        public bool   bPara;
        public int[]  SDK81;
		public List<UCell>  Board;
		public string Message;
		public GShortMessage gsm;
		public object obj;

	    public Gidoo_EventHandler( string eName=null, int ePara0=-1, int ePara1=-1, bool bPara=false, string Message="" ){
            try{
		        this.eName   = eName;
		        this.ePara0  = ePara0;
                this.ePara1  = ePara1;
                this.bPara   = bPara;
				this.Message = Message;
            }
            catch( Exception ex ){ WriteLine( $"{ex.Message}\r{ex.StackTrace}"); }
	    }

		// shortMessage("Insufficient number of cells in pattern.",new sysWin.Point(600,240),Colors.OrangeRed,3000);
		public Gidoo_EventHandler( string eName, GShortMessage gsm ){
			this.eName = eName;
		this.gsm   = gsm;
		}

		public Gidoo_EventHandler( string eName, List<UCell>  Board, bool bPara ){ this.Board=Board; this.bPara=bPara; }

		public Gidoo_EventHandler( int[] SDK81 ){
            this.SDK81=SDK81;
        }
    }



	public class GShortMessage{
		public string Message;
		public sysWin.Point Pt;
		public Color  color;
		public int    msec;
	
		public GShortMessage( string Message, sysWin.Point Pt, Color color, int msec ){
			this.Message = Message;
			this.Pt      = Pt;
			this.color   = color;
			this.msec    = msec;
		}
	}

}