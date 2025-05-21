using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using GIDOO_space;

namespace GNPX_space{
    public class LockedSetGen: AnalyzerBaseV2{
        private UInt128 cells128;

        public LockedSetGen( GNPX_AnalyzerMan AnMan ): base(AnMan){
            UInt128 cells128=0;
            pBOARD.ForEach( p => { if(p.No!=0) cells128 |= (UInt128)1<<p.rc; } );        
        }

        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page33.html


        public bool LockedSet2() => LockedSet_Master(2);  //2D
        public bool LockedSet3() => LockedSet_Master(3);  //3D
        public bool LockedSet4() => LockedSet_Master(4);  //4D
        public bool LockedSet5() => LockedSet_Master(5);  //complementary to 4D Hidden
        public bool LockedSet6() => LockedSet_Master(6);  //complementary to 3D Hidden
        public bool LockedSet7() => LockedSet_Master(7);  //complementary to 2D Hidden

        public bool LockedSet2Hidden() => LockedSet_Master(2,HiddenFlag:true); //2D Hidden
        public bool LockedSet3Hidden() => LockedSet_Master(3,HiddenFlag:true);  //3D Hidden
        public bool LockedSet4Hidden() => LockedSet_Master(4,HiddenFlag:true);  //4D Hidden
        public bool LockedSet5Hidden() => LockedSet_Master(5,HiddenFlag:true);  //complementary to 4D 
        public bool LockedSet6Hidden() => LockedSet_Master(6,HiddenFlag:true);  //complementary to 3D 
        public bool LockedSet7Hidden() => LockedSet_Master(7,HiddenFlag:true);  //complementary to 2D 


        //   Extremely elegant code !!!

        public bool LockedSet_Master( int sz, bool HiddenFlag=false ){
            UInt128 cell128 = pBOARD.Create_Free_BitExp128(); //pBOARD.Get_rc_BitExpression( no:-1 );                       

            for( int h=0; h<27; h++ ){
                UInt128 cells_in_house = cell128 & HouseCells81[h];    // cells_in_house : unresolved cells in the house

                int nc = cells_in_house.BitCount();
                if( nc <= sz ) continue;

                List<UCell>  cells = cells_in_house.IEGet_UCell(pBOARD).ToList();
                
                Combination cmbG = new Combination(nc,sz);
                while( cmbG.Successor() ){
                    int cmbG_index_bit = cmbG.ToBitExpression();

                    int noB_selected=0, noB_unselected=0, selBlk=0;    
                    UInt128 cellA = 0;
                    foreach( var (P,selectedF) in cmbG_index_bit.IEGet_cell_withFlag(cells:cells) ){
                        if(selectedF){ noB_selected |= P.FreeB; selBlk |= 1<<P.b; cellA |= UInt128.One<<P.rc; }
                        else         noB_unselected |= P.FreeB;
                    }                        
                    if( (noB_selected&noB_unselected) == 0 ) continue;                  //any digits that can be eliminated?

                    if( !HiddenFlag ){
					  #region Naked Locked Set
                        if( noB_selected.BitCount() == sz ){                            //Number of selected cell's digits is sz
                            
                            // ----- solution found -----
                            if( h<18 && selBlk.BitCount()==1 ){ 
                                // When searching for a row or column and it is one block,
                                // there may be elements within the block that can be eliminated.
                                int h2 = selBlk.BitToNum()+18;              //bit expression -> House_No(18-26)
                                UInt128 cellB = (cell128 & HouseCells81[h2]) - cellA;  // -: difference set. bit operation. not subtraction.

                                foreach( var P in cellB.IEGet_UCell(pBOARD) ){ P.CancelB = P.FreeB&noB_selected; };
                            }

                            string resST = "";
                            foreach( var (P,selectedF) in cmbG_index_bit.IEGet_cell_withFlag(cells:cells) ){
                                if( selectedF ){
                                    P.Set_CellColorBkgColor_noBit(noB_selected,AttCr,SolBkCr);
                                    resST += " "+P.rc.ToRCString();
                                }
                                else P.CancelB = P.FreeB&noB_selected;
                            }
                            resST = resST.ToString_SameHouseComp()+" #"+noB_selected.ToBitStringN(9);
                            _LockedSetResult(sz,resST,HiddenFlag);

							//if( pBOARD.Any( P => P.No!=0 && P.ECrLst!=null ) ){  WriteLine(" " ); }

                            if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                        }
					  #endregion
                    }

                    else{
					  #region Hidden Locked Set
                        if( noB_unselected.BitCount() == (nc-sz) ){                    //Number of unselected cell's digits is (nc-sz)

                             // ----- solution found -----
                            string resST="";
                            foreach( var (P,selectedF) in cmbG_index_bit.IEGet_cell_withFlag(cells:cells) ){
                                if( !selectedF )  continue;
                                P.CancelB = P.FreeB&noB_unselected;
                                P.Set_CellColorBkgColor_noBit(noB_selected,AttCr,SolBkCr);
                                resST += " "+P.rc.ToRCString();
                            }
                            int nobR = noB_selected.DifSet(noB_unselected);
                            resST = resST.ToString_SameHouseComp()+" #"+nobR.ToBitStringN(9);
                            _LockedSetResult(sz,resST,HiddenFlag);

                            if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                        }
					  #endregion
                    }
                }
            }
            return false;
        }
       
		
        private void _LockedSetResult( int sz, string resST, bool HiddenFlag ){
            string[]  lockedSet_type = {"Pair[2D]", "Triple[3D]", "Quartet[4D]", "Set[5D]", "Set[6D]", "Set{7D}" };

            SolCode = 2;
            string SolMsg = "Locked" + lockedSet_type[sz-2];
            if( HiddenFlag ) SolMsg += " hidden";
            SolMsg += " "+resST;
            Result = SolMsg;
            ResultLong = SolMsg;
        }
    }
}