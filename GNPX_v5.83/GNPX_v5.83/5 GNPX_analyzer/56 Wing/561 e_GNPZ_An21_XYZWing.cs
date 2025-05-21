using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPX_space{
    public partial class SimpleUVWXYZwingGen: AnalyzerBaseV2{
        public List<UCell> FBCX;
		private int stageNoPMemo = -9;

		// .. .. .. .. .. .. .. .. .. .. .. .. .. .. .. .. .. .. ..
        //     YZwing has migrated to ALS version XYZwing.
		// .. .. .. .. .. .. .. .. .. .. .. .. .. .. .. .. .. .. ..

        public SimpleUVWXYZwingGen( GNPX_AnalyzerMan AnMan ): base(AnMan){
			stageNoPMemo = -9;
        }

        public bool XYZwing( ){    return _UVWXYZwing(3); }  //XYZ-wing
        public bool WXYZwing( ){   return _UVWXYZwing(4); }  //WXYZ-wing
        public bool VWXYZwing( ){  return _UVWXYZwing(5); }  //VWXYZ-wing
        public bool UVWXYZwing( ){ return _UVWXYZwing(6); }  //UVWXYZ-wing


        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //8.9..3..4.24...61..7...6.297.2.4.......3.2.......5.1.225.1...4..47...83.1..7..2.5

        private bool _UVWXYZwing( int wsz ){     //simple UVWXYZwing
            if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();

				FBCX = pBOARD.FindAll(p=>p.FreeBC==wsz);
			}
            if( FBCX.Count==0 ) return false;

            bool wingF=false;
            foreach( var P0 in FBCX ){  //focused Cell
                int b0=P0.b;            //focused Block

                foreach( int no in P0.FreeB.IEGet_BtoNo() ){ //focused Digit
                    if( pAnMan.Check_TimeLimit() )  return false;

                    int noB=1<<no;
                    UInt128 BP_bivalue = pBOARD.Create_bitExp_bivalue( ) & FreeCell81b9[no] ;
                    UInt128 B81_P0_conn = BP_bivalue & ConnectedCells81[P0.rc];
                    UInt128 B81P0H   = B81_P0_conn & HouseCells81[18+P0.b];
                              //  WriteLine( $"no:#{no+1}" );
                              //  WriteLine( $"  BP_bivalue:{BP_bivalue.ToBitString81()}" );
                              //  WriteLine( $" B81_P0_conn:{B81_P0_conn.ToBitString81()}" );
                              //  WriteLine( $"      B81P0H:{B81P0H.ToBitString81()}" );     //in r4c7

                    for(int dir=0; dir<2; dir++ ){ //dir 0:row 1:col
                        int h = (dir==0)? P0.r: (9+P0.c);

                        UInt128 B81P0H2 = B81P0H.DifSet(HouseCells81[h]);
                              //  WriteLine( $"     B81P0H2:{B81P0H2.ToBitString81()}" );

                        if( B81P0H2.IsZero() ) continue;
                        UInt128 Pout = (B81_P0_conn & HouseCells81[h] ).DifSet(HouseCells81[18+P0.b]);
                        if( B81P0H2.BitCount()+Pout.BitCount() != (wsz-1) ) continue;
                              //  WriteLine( $" in B81P0H2:{B81P0H2.ToBitString81()}" );
                              //  WriteLine( $" Pout      :{Pout.ToBitString81()}" );

                        int FreeBin  = B81P0H2.UInt128AggregateFreeB(pBOARD); 
                        int FreeBout = Pout.UInt128AggregateFreeB(pBOARD);
                        if((FreeBin|FreeBout)!=P0.FreeB) continue;
                        UInt128 ELst   = HouseCells81[h] & HouseCells81[18+P0.b];
                        ELst = ELst.Reset(P0.rc);
                              //  WriteLine( $" ELst      :{ELst.ToBitString81()}" );

                        string msg3="";
                        foreach( var E in ELst.IEGet_rc().Select(p=>pBOARD[p]) ){
                            if( (E.FreeB&noB)>0 ){
                                E.CancelB=noB; wingF=true; 
                                if( SolInfoB ) msg3 += " "+E.rc.ToRCString();
                            }
                        }

                        if(!wingF)  continue;
                        
                        //--- ...wing found -------------
                        SolCode=2;     
                        string[] xyzWingName = { "XYZ-Wing","WXYZ-Wing","VWXYZ-Wing","UVWXYZ-Wing"};
                        string SolMsg = xyzWingName[wsz-3];

                        if( SolInfoB ){
                            P0.Set_CellColorBkgColor_noBit(P0.FreeB,AttCr,SolBkCr2);
                            foreach( var P in B81P0H2.IEGet_rc().Select(p=>pBOARD[p]) ) P.Set_CellColorBkgColor_noBit(P.FreeB,AttCr,SolBkCr);
                            foreach( var P in Pout.IEGet_rc().Select(p=>pBOARD[p]) ) P.Set_CellColorBkgColor_noBit(P.FreeB,AttCr,SolBkCr);

                            string msg0=" Pivot: "+P0.rc.ToRCString();
                            string msg1 = $" in: {B81P0H2.ToRCStringComp()}";
                            string msg2 = $" out: {Pout.ToRCStringComp()}";
                            ResultLong = SolMsg+"\r"+msg0+ "\r   "+msg1+ "\r  "+msg2+ "\r Eliminated: "+msg3.ToString_SameHouseComp();
                            Result = SolMsg+msg0+msg1+msg2;      
                        }
							if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                        wingF=false;
                    }
                }
            }
            return false;
        }
    }
}