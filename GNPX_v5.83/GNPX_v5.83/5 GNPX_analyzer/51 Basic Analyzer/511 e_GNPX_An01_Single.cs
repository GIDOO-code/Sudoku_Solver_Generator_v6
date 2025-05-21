using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using GIDOO_space;
using static System.Math;
using System.Diagnostics;

namespace GNPX_space{

    // First, understand UCell, ConnectedCells81, HouseCells81, and IEGetCellInHouse.
    //  https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page23.html

    // Then the following algorithm("Single") is almost trivial.
    //  https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page31a.html
    //  https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page31b.html
    //  https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page31c.html


    public class SimpleSingleGen: AnalyzerBaseV2{
        public SimpleSingleGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

        //*==*==*==*==* Last Digit *==*==*==*==*==*==*==*==* 
        public bool LastDigit( ){
            bool  SolFound=false;
			UInt128 solBit = new();
            for(int h=0; h<27; h++ ){ //h:house (row:0-, column:9-, block:18-)
                if( pBOARD.IEGetCellInHouse(h,0x1FF).Count() == 1 ){   //// only one element(digit) in house
                   
                    var P = pBOARD.IEGetCellInHouse(h,0x1FF).First();
					if( P.FreeBC != 1 )  continue;

					//---------------------- found
					SolFound = true;
                    P.FixedNo = P.FreeB.BitToNum()+1;    
					solBit = solBit.Set(P.rc);
                    if( !chbx_ConfirmMultipleCells )  goto LFound;
                }
            }

          LFound:
            if(SolFound){
                SolCode=1;
                Result = "Last Digit  Fixed:" + solBit.ToRCStringComp();
				if( SolInfoB ) ResultLong = "Last Digit\n  Fixed:" + solBit.ToRCStringComp();
				if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
            }
            return false;
        }



        //*==*==*==*==* Naked Single *==*==*==*==*==*==*==*==* 
        public bool NakedSingle( ){
            bool  SolFound=false;
			UInt128 solBit = new();
            foreach( UCell P in pBOARD.Where(p=>p.FreeBC==1) ){   // only one element(digit) in cell

                //---------------------- found
                SolFound = true;
                P.FixedNo = P.FreeB.BitToNum()+1; 
				solBit = solBit.Set(P.rc);
                if( !chbx_ConfirmMultipleCells )  goto LFound;
            }

          LFound:
            if(SolFound){
                SolCode=1;
                Result = "Naked Single  Fixed:" + solBit.ToRCStringComp();
				if( SolInfoB ) ResultLong = "Naked Single\n  Fixed:" + solBit.ToRCStringComp();
				if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
            }
            return false;
        }



        //*==*==*==*==* Hidden Single *==*==*==*==*==*==*==*==*
        public bool HiddenSingle( ){
            bool  SolFound=false;
			UInt128 solBit = new();
            for(int no=0; no<9; no++ ){ //no:digit
                int noB=1<<no;
                for( int h=0; h<27; h++ ){
                    if(pBOARD.IEGetCellInHouse(h,noB).Count()==1){  //only one cell in house(h)
                        try{
                            var PLst=pBOARD.IEGetCellInHouse(h,noB).Where(Q=>Q.FreeBC>1);
                            if( PLst.Count()<=0 )  continue;
                                               
                            //---------------------- found

                            var P = PLst.First();
							if( P.FixedNo > 0 )  continue;
							SolFound = true;  
                            P.FixedNo = no+1;  
							solBit = solBit.Set(P.rc);
                            if( !chbx_ConfirmMultipleCells )  goto LFound;
                        }
                        catch(Exception e){ WriteLine($"{e.Message}\r{e.StackTrace}"); }
                    }
                }               
            }

          LFound:
            if(SolFound){
                SolCode=1;
                Result = "Hidden Single  Fixed:" + solBit.ToRCStringComp();
				if( SolInfoB ) ResultLong = "Hidden Single\n  Fixed:" + solBit.ToRCStringComp();
				if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
            }
            return false;
        }
    }
}