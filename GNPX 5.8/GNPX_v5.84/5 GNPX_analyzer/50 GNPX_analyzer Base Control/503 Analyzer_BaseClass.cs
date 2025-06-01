using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Threading;
using System.Runtime.ConstrainedExecution;

using Simon_Squared;
using static GNPX_space.JExocet_TechGen;
using System.Security.Policy;

namespace GNPX_space {

    public partial class AnalyzerBaseV2{
        static private readonly UInt128 qZero   = UInt128.Zero;
        static private readonly UInt128 qOne    = UInt128.One;
	    static private readonly UInt128 qMaxB81 = (UInt128.One<<81)-qOne;
        static private readonly UInt128 _b9     = 0x1FF;
		static public  readonly UInt128 _na_    = UInt128.MaxValue;

        static public bool   __SimpleAnalyzerB__;
        static public int    __DebugBreak;
		static public G6_Base G6 => GNPX_App_Man.G6;

        private const int    S=1, W=2;

        static public UInt128[] qFreeCell81b9;

        static private GNPX_Engin pGNPX_Eng;
        public GNPX_AnalyzerMan pAnMan;

		public int	eNdebug=0;

      // public CancellationToken cts => pGNPX_Eng.cts;


		public string			AdditionalMessage{ set => GNPX_Engin.AdditionalMessage=value; }

        private UInt128[]       _FreeCell81b9;
        public UInt128[]        FreeCell81b9 => _FreeCell81b9?? (_FreeCell81b9=pBOARD.Create_bitExp81_9() );

        private UInt128[]       _FreeAndFixed81B9;	//  for Exocet(V6)
		public UInt128[]        FreeAndFixed81B9 => _FreeAndFixed81B9?? (_FreeAndFixed81B9=pBOARD.Create_bitExp81_9( withFixedB:true ) );

		public UInt128[]        Fixed81B9 => pBOARD.Create_FixedBitExp81_9();
        public  GNPX_App_Man    pGNPX_App => pGNPX_Eng.pGNPX_App;  
		public  UPuzzle         ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; }
        public List<UCell>      pBOARD{ get=>ePZL.BOARD; set=>ePZL.BOARD=value; } 

        private int             stageNoPMemo_ApBV2 = -1;
        public  int             stageNo => ePZL.stageNo;
        public  int             stageNoP => pGNPX_Eng.stageNoP;//pGNPX_Eng.stageNo100 + stageNo;

		protected string		TandE_st_Puzzle => pGNPX_Eng.TandE_st_Puzzle;
		protected int[]			TandE_sol_int81 => pGNPX_Eng.sol_int81;
		protected string		TandE_st_sol_int81 => pGNPX_Eng.TandE_st_sol_int81;

        public bool             SolInfoB{ get=>GNPX_Engin.SolInfoB; set=>GNPX_Engin.SolInfoB=value; }
        public int              SolCode{  get=>ePZL.SolCode; set=>ePZL.SolCode=value; }
        public string           Result{   get=>ePZL.Sol_Result; set=>ePZL.Sol_Result=value; }
        public string           ResultLong{ get=>ePZL.Sol_ResultLong; set=>ePZL.Sol_ResultLong=value; }
        public bool             chbx_ConfirmMultipleCells{ get=> pAnMan.chbx_ConfirmMultipleCells; } 
               

		
		public bool				PreferSimpleLinks = G6.PreferSimpleLinks > 0;
		public bool				Use_eALS=false;
     // public  bool            DebugSolCheck_Check => pGNPX_Eng.DebugSolCheck_Check();

		public string		    extResult{
									set{
										if( value=="" ) pBOARD.ForEach( P => P.Reset_result() ); //Clear all analysis results
										ePZL.extResult=value; 
									}
									get=> ePZL.extResult; }

        static  AnalyzerBaseV2( ){
          //Create_ConnectedCells();
            Create_ConnectedCells81();
            __SimpleAnalyzerB__ = false;

			// Needs improvement @@@
			List<Color> _colors = _ColorsLst1.ToList();

			RandomPastelColorGenerator RPCG = new RandomPastelColorGenerator();
			for( int n=0; n<20; n++ ) _colors.Add( RPCG.GetNext(160) );
/*
			foreach( var a in new float[]{ 0.7f, 0.3f } ){
				foreach( var crB in _ColorsLstB ) _colors.Add( crB*a );
				foreach( var crR in _ColorsLstR ) _colors.Add( crR*a );
			}
*/
			_ColorsLst = _colors.ToArray();
        }

        public  AnalyzerBaseV2( ){}
        public  AnalyzerBaseV2( GNPX_AnalyzerMan pAnMan ){ 
            this.pAnMan = pAnMan;
            UCell.pAnMan = pAnMan;
            pGNPX_Eng = pAnMan.pGNPX_Eng;
			UAnLS.qAnalyzer = this;
        }

        public void AnalyzerBaseV2_PrepareStage(){
            if( stageNoP!=stageNoPMemo_ApBV2 ){
				stageNoPMemo_ApBV2 = stageNoP;
                _FreeCell81b9 = null;
				_FreeAndFixed81B9 = null;		// for Exocet(V6)

				//if( pBOARD.Any( P => P.ECrLst!=null ) ){  WriteLine(" " ); }  ... for development
			}    
        }

		public UInt128 FreeCell81b9_FreeBAnd( int FreeB ){
			UInt128 b = UInt128.MaxValue;
			foreach( int no in FreeB.IEGet_BtoNo().Where(n =>(FreeB&(1<<n))>0) )  b &= FreeCell81b9[no];
			if( b==UInt128.MaxValue ) b=0;
			return b;
		}
        public (int,int) ToTuple1( int X ) => (X>>1,X&1);

	//	public void		ePZL_FreeB_Save(){ foreach( var P in ePZL.BOARD ) P.FreeBwk = P.FreeB; }		// for debug
	//	public void		ePZL_FreeB_Restore(){ foreach( var P in ePZL.BOARD ) P.FreeB = P.FreeBwk; }


    #region Display Control
        static public readonly string[]    rcbStr=new string[]{ "r", "c", "b" };

		static public readonly Color[] _ColorsLst;

        static private readonly Color[] _ColorsLst1=new Color[]{ Colors.Yellow, Colors.LightGreen, Colors.Aqua };
		static private readonly Color[] _ColorsLstB=new Color[]{ Colors.MediumSpringGreen, Colors.SkyBlue, Colors.Lime, Colors.Plum, Colors.SkyBlue };
		static private readonly Color[] _ColorsLstR=new Color[]{ Colors.Magenta, Colors.HotPink, Colors.OrangeRed, Colors.Lime, Colors.Tomato, Colors.Magenta, Colors.Purple };

        static public readonly Color AttCr    = Colors.Red;
        static public readonly Color AttCr2   = Colors.SkyBlue;
        static public readonly Color AttCr3   = Colors.LightCoral;
        static public readonly Color AttCr4   = Colors.Gold;
		static public readonly Color AttCr5   = Colors.Blue;
		static public readonly Color AttCr6   = Colors.BlueViolet;

        static public readonly Color SolBkCr  = Colors.Yellow;
        static public readonly Color SolBkCr2 = Colors.LightGreen;	//Aqua;//SpringGreen//Colors.CornflowerBlue;  //FIn
		static public readonly Color SolBkCr2G = Colors.DodgerBlue;	//RoyalBlue; //.DeepSkyBlue; //LightPink; //Aqua;//SpringGreen//Colors.CornflowerBlue;  //FIn
		static public readonly Color SolBkCr3 = Colors.Aqua;　　	//Colors.CornflowerBlue;
        static public readonly Color SolBkCr4 = Colors.LawnGreen;   //SpringGreen B81P0Hk PaleVioletRed LightSalmon Orchid PaleGreen
		static public readonly Color SolBkCr5 = Colors.BlueViolet; 
		static public readonly Color SolBkCr6 = Colors.LemonChiffon; 
		
    #endregion Display Control      

    #region Connected Cells
        static public UInt128[]  ConnectedCells81;    //Connected Cells
        static public UInt128[]  HouseCells81;        //Row(0-8) Collumn(9-17) Block(18-26)
        static public UInt128    bit81_1;

		static public int[]		 Connected33;

        static private void Create_ConnectedCells81(){
            if( ConnectedCells81 != null )  return;
            bit81_1 = ((UInt128)1<<81)-1;
            ConnectedCells81    = new UInt128[81];
          //ConnectedCellsRev81 = new UInt128[81];

            for( int rc=0; rc<81; rc++ ){
                UInt128 BS128 = 0;
                foreach( var q in __IEGetCellsConnectedRC(rc).Where(q=>q!=rc)) BS128 |= (UInt128)1<<q;
                ConnectedCells81[rc]    = BS128;       // {ATT} Does not include stem cell!
              //ConnectedCellsRev81[rc] = BS128 ^ bit81_1;
					//WriteLine( $" BS128 rc:{rc} {BS128.ToString81()}");
            }

			Connected33 = new int[9];
            for( int rc=0; rc<9; rc++ ){
                int  B33 = 0;
                foreach( var q in __IEGet_Connected33(rc).Where(q=>q!=rc)) B33 |= 1<<q;
                Connected33[rc] = B33;       // {ATT} Does not include stem cell!
					//WriteLine( $" BS128 rc:{rc} {B33.ToBitString(9)}");
            }

            HouseCells81 = new UInt128[27];
            for( int h=0; h<27; h++ ){
                UInt128 tmp = 0;
                foreach( var q in __IEGetCellInHouse(h) ) tmp |= UInt128.One<<q;
                HouseCells81[h] = tmp;
	                //WriteLine( $" HouseCells81:{h} {tmp.ToString81()}");
            }
        }
        static private IEnumerable<int> __IEGetCellsConnectedRC( int rc ){ 
            int r=0, c=0;
            for(int kx=0; kx<27; kx++ ){
                switch(kx/9){
                    case 0: r=rc/9; c=kx%9; break; //row 
                    case 1: r=kx%9; c=rc%9; break; //collumn
                    case 2: int b=rc/27*3+(rc%9)/3; r=(b/3)*3+(kx%9)/3; c=(b%3)*3+kx%3; break;//block
                }
                yield return r*9+c;
            }
        }
        static private IEnumerable<int> __IEGet_Connected33( int rc ){ 
            int r, c;
			for( int k=0; k<3; k++ ){ r=rc/3; c=k; yield return r*3+c; }
			for( int k=0; k<3; k++ ){ r=k;    c=rc%3; yield return r*3+c; }
        }

        static private IEnumerable<int> __IEGetCellInHouse( int h ){ //nx=0...8
            int r=0, c=0, tp=h/9, fx=h%9;
            for(int nx=0; nx<9; nx++ ){
                switch(tp){
                    case 0: r=fx; c=nx; break;  //row
                    case 1: r=nx; c=fx; break;  //collumn
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;  //block
                }
                yield return (r*9+c);
            }
        }
	#endregion Connected Cells
	}
}