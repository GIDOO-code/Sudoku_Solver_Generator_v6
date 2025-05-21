using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System;

namespace GNPX_space{
	using pApCrBr = App_ColorBrush;   

    //Sudoku's cell definition.
        [Serializable]
    public partial  class UCell{ //Basic Cell Class
        static private readonly Color _Black_ = Colors.Black; 
        static public GNPX_AnalyzerMan pAnMan;

        public readonly int    rc;    // cell position(0-80)
        public readonly int    r;     // row
        public readonly int    c;     // column
        public readonly int    b;     // block
	 	public readonly int    rcbFrame;	


        public int			ErrorState; // 0:-  1:Fixed 　8:Violation  9:No solution
        public int			No;         // >0:Puzzle  =0:Open  <0:Solution
        public int			FreeB{ get; set; }   // Bit expression of candidate digits

        public int			FixedNo;    // Fixed digit. Obtaine by an algorithm and reflecte to the board status by management processing
        public int			CancelB;    // Digits to eliminate(bit expression). Same process as above.

        public List<EColor> ECrLst; // Display color of cell digits. Obtaine by an algorithm.
        public Color		CellBgCr;   // background color. Obtaine by an algorithm
        
		private object		obj = new object();

        public		UCell( ){}
        public		UCell( int rc, int No=0, int FreeB=0 ){
            this.rc=rc; this.r=rc/9; this.c=rc%9; this.b=_ToBlock(rc);  //rc/27*3+(rc%9)/3;
			this.rcbFrame = _To_rcbFrame(rc);	// => (1<<(rc.ToBlock()+18)) | (1<<(rc%9+9)) | (1<<rc/9);rc.To_rcbFrame( );
            this.No=No; this.FreeB=FreeB;   
            this.ECrLst=null;
            this.FreeB_original = FreeB;
        }
        public override string ToString(){
            string st2 = $" UCell rc:{rc:pApCrBr}[r{r+1}c{c+1}]  No:#{No}";
            st2 += $" FreeB:{FreeB.ToBitString(9)}";
			st2 += $" FreeBwk:{FreeBwk.ToBitString(9)}";
            st2 += $" FixedNo:{FixedNo.ToString()}";
            st2 += $" CancelB:{CancelB.ToBitString(9)}";
            return st2;
        }
	}




	public partial  class UCell{ //Basic Cell Class
		//	public long Get_hashValue() => HashMan.hashBase[FreeB] ^ HashMan.hashBase[rc*73];
      
		
		
		public int      nx;         // (Working variable used in algorithm)	
		public int      FreeBwk;	// Working variable used in algorithm
        private readonly int		FreeB_original;


		public int      FreeB0	=> FreeB.DifSet(CancelB);
        public int      FreeBC{ get=>FreeB.BitCount(); }   //Number of candidate digits.
		public int		noB_FixedFreeB => (No!=0)? 1<<(Abs(No)-1): FreeB;
		public int      _SetFreeB_( int F ) => FreeB=F;
		public int		FreeB_ex => FreeB.DifSet(CancelB);	 
		private int		_ToBlock(int rc) => rc/27*3+(rc%9)/3;

		private int		_To_rcbFrame( int rc) => (1<<(rc.ToBlock()+18)) | (1<<(rc%9+9)) | (1<<rc/9);  //rc.To_rcbFrame( );
		


	    public int		Get_hashValue() => ((rc<<9) | FreeB).GetHashCode();

		public void		Set_FreeB( int f ) => FreeB = f;
		public void		Reset_FreeB( int f ) => FreeB &= (f^0x1FF);
		public void		Restore_FreeB() => this.FreeB = this.FreeB_original;

        public void		Reset_StepInfo(){
            ErrorState = 0;
            CancelB    = 0;
            FixedNo    = 0;

            this.ECrLst= null;
            CellBgCr   = _Black_;
        }

        public UCell Copy( ){
			lock(obj){
				UCell UCcpy = (UCell)this.MemberwiseClone();
				try{
					if( !(ECrLst is null) ){
						UCcpy.ECrLst = new List<EColor>();
						ECrLst.ForEach( p=> UCcpy.ECrLst.Add(new EColor(p) ));
					}
				}
				catch(Exception e){}
				return UCcpy;
			}
        }	

//        public void BackupFreeB() => this.FreeB_original = this.FreeB;
//        public void RepairFreeB() => this.FreeB = this.FreeB_original;

        public void Reset_result( bool resetAll=false ){
            CancelB=FixedNo=0; ECrLst=null;
            if( resetAll ) FreeB=0;
        }
        public void Reset_All(){
            CancelB=FixedNo=FreeB=0; ECrLst=null;
            if( No < 0 )  No = 0;
        }

        public void ECrLst_Add( EColor EC ){
            if( ECrLst==null) ECrLst=new List<EColor>();
            if( !ECrLst.Contains(EC) )  ECrLst.Add(EC);

            if( EC.ClrDigitBkg == Color.FromArgb(0xFF,0xFF,0xFF,0x00) )  WriteLine( $"EC:{EC}" );
        }

        //  EColor( Color ClrCellBkg, int noB, Color ClrDigit, Color ClrDigitBkg )
        public void Set_CellBKGColor( Color CellClrBkg ){ 
            ECrLst_Add( new EColor(CellClrBkg,0x1FF,_Black_,_Black_) );
        }

        public void Set_CellDigitsColor_noBit( int noB, Color clr ){
            if( (FreeB&noB) == 0 )  return;
            ECrLst_Add( new EColor( _Black_, noB, clr, _Black_ ) );
        }
        public void Set_CellDigitsColorRev_noBit( int noB, Color clrRev ){
            if( (FreeB&noB) == 0 )  return;
            ECrLst_Add( new EColor( _Black_, noB, _Black_, clrRev) );
        }

        public void Set_CellFrameColor( Color ClrCellFrame ){
            ECrLst_Add( new EColor( ClrCellFrame:ClrCellFrame) );
        }

        public void Set_CellColorBkgColor_noBit( int noB, Color clr, Color clrBkg ){
            ECrLst_Add( new EColor( clrBkg, noB, clr, _Black_) );
        }
        public void Set_CellDigitsBkgColorRev_noBit( int noB, Color clrRev, Color clrBkg ){
            if( (FreeB&noB) == 0 )  return;
            ECrLst_Add( new EColor( clrBkg, noB, _Black_, clrRev) );
        }

		public int FreeB_Updated( ) => (FreeB).DifSet(CancelB);
		public int FreeB_Updated_withFixed( ) => FreeB_Updated() | ((No!=0)? (1<<Abs(No)-1): 0);



      // public override int GetHashCode() => (int)( HashMan.hashBase[rc] ^ FreeB );

        public string ToStringN(){
            string st2 = $" UCell rc:{rc:pApCrBr}[r{r+1}c{c+1}]  no:#{No}";
            st2 += $" FreeB:{FreeB.ToBitString(9)}";
			st2 += $" FreeBwk:{FreeBwk.ToBitString(9)}";
            st2 += $" CancelB:{CancelB.ToBitString(9)}";
            return st2;
        }
        public string ToStringN2(){
            string st2 = $" UCell {rc.ToRCString()} #{No}";
            st2 += $" FreeB:{FreeB.ToBitString(9)}";
			st2 += $" FreeBwk:{FreeBwk.ToBitString(9)}";
            st2 += $" CancelB:{CancelB.ToBitString(9)}";
            return st2;
        }

        public void ResetAnalysisResult(){
            CancelB  = 0;
            FixedNo  = 0;
//            Selected = false;     ... Remove in next version
            this.ECrLst = null;
        }
    }
}