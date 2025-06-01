using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPX_space{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{
        //EmptyRectangle is an algorithm using cell-to-cell link and ConnectedCells.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page41.html
		// 4762391582391584671..4672397.....3253.2.7.6815613827946.789.51.9.....87.81.72.946

        public bool  EmptyRectangleEx( ){		// New algorithm using bit representation.
			PrepareStage();
            bool lkExtB = G6.UCellLinkExt;
            CeLKMan.PrepareCellLink(1, lkExtB);    //StrongLink

            for(int no=0; no<9; no++ ){                                     //Focused digit
				int noB = 1<<no;
				foreach( var UC0 in pBOARD.Where(p=>(p.FreeB&noB)>0) ){
					int blk0=UC0.b, row0=UC0.r, col0=UC0.c;

					UInt128 houseRow = HouseCells81[row0+0];
					UInt128 houseCol = HouseCells81[col0+9];
					UInt128 houseBlk = HouseCells81[blk0+18];
					UInt128 BlockBPno = FreeCell81b9[no] & houseBlk;

					if( (BlockBPno & houseRow).IsZero() )  continue;
					if( (BlockBPno & houseCol).IsZero() )  continue;

					UInt128 houseRowCol = houseRow | houseCol;
					if( BlockBPno.DifSet(houseRowCol).IsNotZero() )   continue;

					foreach( var rc1 in houseCol.DifSet(houseBlk).IEGet_rc() ){
						foreach( var LK1 in CeLKMan.IEGetRcNoBTypB(rc1,noB,1) ){
							int rcElm = row0*9 + LK1.UCe2.c;
							if( rcElm.ToBlock()==blk0 || !FreeCell81b9[no].IsHit(rcElm) )  continue;

							EmptyRectangle_SolResult( no, blk0, LK1, pBOARD[rcElm] );   //solution found

							if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
						}
					}

					foreach( var rc2 in houseRow.DifSet(houseBlk).IEGet_rc() ){
						foreach( var LK2 in CeLKMan.IEGetRcNoBTypB(rc2,noB,1) ){
							int rcElm = LK2.UCe2.r*9 + col0;
							if( rcElm.ToBlock()==blk0 || !FreeCell81b9[no].IsHit(rcElm) )  continue;

							EmptyRectangle_SolResult( no, blk0, LK2, pBOARD[rcElm] );   //solution found
							
							if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
						}
					}
                }
            }
            return false;
        }

        public bool  EmptyRectangle( ){ // old version
			PrepareStage();
            bool lkExtB = G6.UCellLinkExt;
            CeLKMan.PrepareCellLink(1, lkExtB);    //StrongLink

            for (int no=0; no<9; no++ ){                                     //Focused digit
                int noB = 1<<no;
                for(int bx=0; bx<9; bx++ ){                                 //Focused Block
                    int erB = pBOARD.IEGetCellInHouse(bx+18,noB).Aggregate(0,(Q,P)=>Q|(1<<P.nx));
                    if( erB==0 ) continue;	

                    for(int er=0; er<9; er++ ){                             //Focused Cell in the Focused Block
                        int Lr=er/3, Lc=er%3;                               //Block local Row and Column
                        int rxF = 7<<(Lr*3);                                //7=1+2+4   (Block local Row r1c123)
                        int cxF = 73<<Lc;                                   //73=1+8+64 (Block local Column r123c1)
          
                        if( (erB&rxF)==0 || erB.DifSet(rxF)==0 )  continue; //Row Lr(Row Cndition Check)
                        if( (erB&cxF)==0 || erB.DifSet(cxF)==0 )  continue; //Column Lc(Column Cndition Check)          
                        if( erB.DifSet(rxF|cxF) > 0 )             continue; //Row Lr and Column Lc(ER Condition Check)
                        
                        int r1 = bx/3*3+Lr;                                 //Convert to Absolute Row
                        int c1 = (bx%3)*3+Lc;                               //Convert to Absolute Column

                        foreach( var P in HouseCells81[9+c1].IEGet_UCell_noB(pBOARD,noB).Where(Q=>Q.b!=bx) ){
                                                                            //P:cell in house(column c1), P is outside bx
                            foreach( var LK in CeLKMan.IEGetRcNoBTypB(P.rc,noB,1) ){//rc:link end, noB:digit, 1:StrongLink
                                UCell Elm=pBOARD[r1*9+LK.UCe2.c];
                                if(Elm.b!=bx && (Elm.FreeB&noB)>0){                 //There is a Digit that can be eliminated
                                    EmptyRectangle_SolResult( no, bx, LK, Elm );    //solution found

									if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                                }
                            }
                        }

                        foreach( var P in HouseCells81[0+r1].IEGet_UCell_noB(pBOARD,noB).Where(Q=>Q.b!=bx) ){
                                                                            //P:cell in house(row0 r1), P is outside bx
                            foreach( var LK in CeLKMan.IEGetRcNoBTypB(P.rc,noB,1) ){//rc:link end, noB:digit, 1:StrongLink
                                UCell Elm=pBOARD[LK.UCe2.r*9+c1];
                                if(Elm.b!=bx && (Elm.FreeB&noB)>0){                 //There is a Digit that can be eliminated
                                    EmptyRectangle_SolResult( no, bx, LK, Elm );    //solution found
                                    
									if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        
        private void EmptyRectangle_SolResult( int no, int bx, UCellLink PLK, UCell PElm ){
            int noB=(1<<no);
            SolCode = 2;
            Result = $"EmptyRectangl #{(no+1)} in b{(bx+1)}";
            PElm.CancelB = noB;                                             //Cancellation Digit Setting
            if(!SolInfoB) return;
              
            ResultLong = $"EmptyRectangl #{(no+1)} in b{(bx+1)}";
            PLK.UCe1.Set_CellColorBkgColor_noBit( noB, AttCr, SolBkCr2 );                 //Mark Strong Links
            PLK.UCe2.Set_CellColorBkgColor_noBit( noB, AttCr, SolBkCr2 );                 //Mark Strong Links
               
            string st=""; 
            foreach( var Q in pBOARD.IEGetCellInHouse(bx+18,noB) ){
                Q.Set_CellColorBkgColor_noBit(noB,AttCr,SolBkCr);   //Empty Rectangle
                st += " "+Q.rc.ToRCString();
            }
            string msg=$"\r         digit: #{(no+1)}\r            ER: B{(bx+1)}({st.ToString_SameHouseComp()})";
            msg +=$"\r        S-Link: {PLK.rc1.ToRCString()}-{PLK.rc2.ToRCString()}";
            msg +=$"\rEliminatedCell: {PElm.rc.ToRCString()}";
            ResultLong = "EmptyRectangl"+msg;
        }
    }
}