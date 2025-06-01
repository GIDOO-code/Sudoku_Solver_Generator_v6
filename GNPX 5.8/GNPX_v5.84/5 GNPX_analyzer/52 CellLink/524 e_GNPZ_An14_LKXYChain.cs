using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;

using GIDOO_space;
//using System.Windows.Input.Manipulations;
using static System.Diagnostics.Debug;

namespace GNPX_space{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{

        //XY-Chain is an algorithm using Locked which occurs in the concatenation of bivalues.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page49.html

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //.5....3...71.43...2..61...9..5....7.7.34..19.1...9.8..3.2.64.5........185.927.4..
        //...71...9.14.9.....9....3.72...4.5.186.1.72......8.7....6471..5.......689.25..17.
        private class Unit_ChainXY{
            public int   no;
            public UCell UCe;
            public Unit_ChainXY pre;

            public Unit_ChainXY( int no, UCell UCe, Unit_ChainXY pre){
                this.no  = no;
                this.UCe = UCe;
                this.pre  = pre;
            }

            public override string ToString(){
                string  rcX = (pre==null)? "-": (pre.UCe.rc).ToRCString();
                string st = $"sel:#{no+1} {UCe.ToStringN2()} pre:{rcX}";
                return st;
            }
        }  




        public bool XYChain(){
			PrepareStage();
			CeLKMan.PrepareCellLink(1);    //Generate StrongLink

            foreach( var (USol,noS,SolChain) in _GetXYChain_sub() ){

                //===== XY-Chain found =====
                SolCode=2;                    
                String SolMsg = $"XY Chain {USol.rc.ToRCString()} #{noS+1} is false";
                Result = SolMsg;

                int noB = (1<<noS);
                USol.CancelB = noB; 
                USol.Set_CellColorBkgColor_noBit(noB,AttCr,SolBkCr);  

                if( SolInfoB ){
                    string msg2 = "";
                    Color Cr = _ColorsLst[0];
                    foreach( var P in SolChain ){
                        P.UCe.Set_CellColorBkgColor_noBit(1<<P.no,AttCr,Cr);
						msg2 += $" - {(P.UCe.rc).ToRCString()}#{P.no+1}";
                    }
                    ResultLong = SolMsg + "\n  > " + msg2.Substring(2);
                }

				if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
            }
            return false;
        }
                
        private IEnumerable<(UCell,int,List<Unit_ChainXY>)> _GetXYChain_sub( ){
            UInt128 BP_bivalue = pBOARD.Create_bitExp_bivalue( );                   //bit representation of bivalue_cells
               
            Queue<Unit_ChainXY> Que;
            List<Unit_ChainXY> ChainXYLst;

            foreach( var PStart in BP_bivalue.IEGet_UCell_noB(pBOARD,0x1FF) ){       //Choose one BV_Cell(=>PStart)
                int rcS = PStart.rc;
                UInt128 CnctdCs = ConnectedCells81[rcS];   

                foreach( var noS in PStart.FreeB.IEGet_BtoNo() ){                   //Choose one digit(in PStart)
                    //--- prepare ---
                    int noB = (1<<noS);  
                    UInt128 Bpat_noS = FreeCell81b9[noS];
                    UInt128 Bpat_sol = CnctdCs & Bpat_noS;                          //here if there is a solution

                            //WriteLine($"selected noS: #{noS+1} Bpat_sol:{Bpat_sol}" );
                    int no0 = PStart.FreeB.BitReset(noS).BitToNum();                //The other digit of the starting cell
                    Unit_ChainXY P0 = new Unit_ChainXY(no0,PStart,null);

					Que = new Queue<Unit_ChainXY>();
					Que.Enqueue( P0 );
					ChainXYLst = new List<Unit_ChainXY>();
                    ChainXYLst.Add( P0 );
					UInt128 used = UInt128.Zero;   
                    used = used.Set(P0.UCe.rc);

                    //--- search ---    
                    while(Que.Count>0){                                             //Extend the chain step by step
                        var P1 = Que.Dequeue();

                        foreach( var LKnext in CeLKMan.IEGetRcNoType(P1.UCe.rc,P1.no,1) ){  //strongLink connected with P1, #no
                            UCell UCe2 = LKnext.UCe2;
                            if( !BP_bivalue.IsHit(UCe2.rc) )  continue;
                            if( used.IsHit(UCe2.rc) )  continue;
                            used = used.Set(UCe2.rc);

                            int no2 = (UCe2.FreeB.BitReset(P1.no)).BitToNum();      // no2:other digit
                            Unit_ChainXY P2 = new Unit_ChainXY(no2,UCe2,P1);                  // UCe2:next cell, P1:upstream cell
         
                            Que.Enqueue( P2 );
                            ChainXYLst.Add(P2);

                            if( no2==noS ){
                                foreach( var rc in Bpat_sol.IEGet_rc()){             // here if there is a solution
                                    if( ConnectedCells81[rc].IsHit(UCe2.rc) ){
                                        //--- found ---
                                        List<Unit_ChainXY> SolChain = new List<Unit_ChainXY>();
                                        SolChain.Add(P2);
                                        Unit_ChainXY PX = P2.pre;
                                        while(PX!=null){ SolChain.Add(PX); PX=PX.pre; } // follow the chain upstream.
                                        SolChain.Reverse();

                                        yield return ( pBOARD[rc], noS, SolChain );
                                        goto LBreak;
                                    }
                                }
                            }
                        }
                    }

				  LBreak:
					continue;
				}

            }
            yield break;
        }

    }
}