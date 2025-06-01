using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPX_space{
    public partial class ALSTechGen: AnalyzerBaseV2{

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //8.9..3..4.24...61..7...6.297.2.4.......3.2.......5.1.225.1...4..47...83.1..7..2.5
        //..3..4...1.86539...5.7...83..6.37.9.7..4....54.....72.....9.51...9..6...8..3..26.

        // == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == ==
        // This XYZwingALS algorithm is extremly elegant and fast.
        //  ... Express the solution condition in bits, and search for an ALS that matches it.
        // [1] Set stem cell(P0) and digit(no).
        // [2] Create a bit representation of a cell with no as a candidate on the board
        // [3] Find the bit representation of the cell whose candidate is no in the block containing P0.
        // [4] Find the bit representation of the cell whose candidate is no in the row/column house containing P0.
        // [5] Find the bit representation of the cell in the row/column house containing P0 and outside the block whose candidate is no.
        // [6] Find two ALS that satisfy the bit representation.
        // [7] Check cells that meet the solution conditions.
        // == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == ==

        private bool break_XYZwingALS=false; //True if the number of solutions reaches the specified number.
        public bool XYZwingALS( ){
            break_XYZwingALS = false;
			PrepareStage();
            if( ALSMan.ALSList==null || ALSMan.ALSList.Count<=2 ) return false;
            ALSMan.QSearch_Cell2ALS_Link();     //prepare cell-ALS link

            for(int sz=2; sz<7; sz++ ){ //number of digits in the cell
				foreach( bool solFound in IE_XYZwingALS_sub(sz) ){
					if( !solFound )  continue;
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
				}
            }
            return false;
        }


        public IEnumerable<bool> IE_XYZwingALS_sub( int wsz ){ //simple UVWXYZwing
            List<UCell> FBCX = pBOARD.FindAll(p=>p.FreeBC<=wsz);            //Stemセル候補のセルを選ぶ
            if(FBCX.Count==0)  yield break;

            // ----- Stem cell (P0) -----
            foreach( UCell P0 in FBCX ){              // [1]... Stem cell           
                if( pAnMan.Check_TimeLimit() )  yield break;        

                // ----- digit (no) -----
                for( int no=0; no<9; no++){           // [1]... No relation between P0.FreeB and eliminated digit(no).
                    int noB=1<<no;

                    // [2]... Create a bit representation of a cell with no as a candidate on the board
                    UInt128 P0_Connected = FreeCell81b9[no] & ConnectedCells81[P0.rc];     //cells related to rc/no 　

                    // [3]... Find the bit representation of the cell whose candidate is no in the block containing P0.
                    UInt128 P0_block     = P0_Connected & HouseCells81[18+P0.b];       //rc-related cells in Stem block(=b0).

                    // ----- row / column -----
                    int[] _row_columnloop = {P0.r, 9+P0.c };
                    foreach( int h in _row_columnloop ){      //for( int h=P0.r, n=0; n<2; h=9+P0.c,n++ ){    
                        
                        // [4]... Find the bit representation of the cell whose candidate is no in the row/column house containing P0.
                        UInt128 Cand_inBlock = P0_block.DifSet( HouseCells81[h] );   //ALS candidate position inside the block

                        if( Cand_inBlock.IsZero() )  continue;

                        // [5]... Find the bit representation of the cell in the row/column house containing P0 and outside the block whose candidate is no.
                        //    ALSout:ALS out of Stem Block   ALSin:ALS in Stem Block
                        UInt128 Cand_outBlock = (P0_Connected & HouseCells81[h]).DifSet( HouseCells81[18+P0.b] ); //ALS candidate position outside the block
                        if( Cand_outBlock.IsZero() ) continue;


                        // [6].. Find two ALS(in/out) that satisfy the bit representation.
                        foreach( var ALSout in ALSMan.IEGetCellInHouse(1,noB,Cand_outBlock) ){   //ALS outside the stem block       
                            UInt128 B128_out= ALSout.ToBitExpression128( filter_FreeB:noB );//#no existence position(outer ALS)  

                            // ---- ALSin (ALS in Stem Block) -----
                            foreach( var ALSin in ALSMan.IEGetCellInHouse(1,noB,Cand_inBlock) ){ //ALS inside the stem block
                                
                                // [7]... Check cells that meet the solution conditions.
                                int FreeB_cover = (ALSout.FreeB | ALSin.FreeB).DifSet(noB);
                                if( P0.FreeB != FreeB_cover )  continue;

                                UInt128 elm128 = ALSout.ToBitExpression128(filter_FreeB:noB) | ALSin.ToBitExpression128(filter_FreeB:noB);

                                UInt128 cand128 = FreeCell81b9[no].DifSet( Cand_outBlock | Cand_inBlock );
                                bool  SolFound = false;  
                                foreach( int rc in cand128.IEGet_rc() ){
                                    if( elm128.DifSet(ConnectedCells81[rc]).IsNotZero() )  continue;
                                    pBOARD[rc].CancelB = noB;
                                    SolFound=true;
                                }

                                if(SolFound){
                                    SolCode=2;     
                                    string[] xyzWingName = { "XY-Wing","XYZ-Wing","WXYZ-Wing","VWXYZ-Wing","UVWXYZ-Wing"};
                                    string SolMsg = xyzWingName[wsz-2]+" (*ALS)";

                                    if( SolInfoB ){
                                        P0.Set_CellColorBkgColor_noBit(P0.FreeB,AttCr,SolBkCr2);

                                        ALSin.UCellLst.IE_SetNoBBgColor(  pBOARD, 0x1FF, AttCr3, SolBkCr);
                                        ALSout.UCellLst.IE_SetNoBBgColor( pBOARD, 0x1FF, AttCr3, SolBkCr);

                                        string msg0 = $" Pivot: {P0.rc.ToRCString()} #{P0.FreeB.ToBitStringN(9)}";
                                        string msg1 = $"    in: {ALSin.UCellLst.ToRCString()} #{ALSin.FreeB.ToBitStringN(9)}";
                                        string msg2 = $"   out: {ALSout.UCellLst.ToRCString()} #{ALSout.FreeB.ToBitStringN(9)}";
                                        string msg3 = $" Eliminated: {pBOARD.FindAll(p=>p.CancelB>0).ToRCString()} #{no+1}";   

                                        Result = SolMsg+msg0;  
                                        ResultLong = $"{SolMsg}\r{msg0}\r{msg1}\r{msg2}\r{msg3}";    
                                    }

                                    yield return true;
                                }
                            }
                        }
                    }
                }
            }
            yield break;
        }

    }
}