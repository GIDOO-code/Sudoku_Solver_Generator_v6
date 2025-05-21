using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Collections.Immutable;

namespace GNPX_space{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{

        //X-Chain is an algorithm using Locked which occurs when concatenating strong and weak links.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page48.html

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //..2..56.145..92....6.41.9.2..5.7....8.......7....2.3..1.3.46.2....95..735.92..8..
        //...4..296.79..2..3.42..351.4.......2.....1.5..21.54..92.8...9..9.4.3...116.9.5.4.

        // Code that prioritizes clarity over speed!


        public bool XChain(){
			PrepareStage();
			CeLKMan.PrepareCellLink(1+2);                              //Generate Link(S+W)
           
            Color Cr = _ColorsLst[0];
            Color Cr1=Cr; Cr1.A=255;
            Color Cr2=Cr; Cr2.A=128;
            Color Cre=_ColorsLst[1]; Cre.A = 128;

            for(int no=0; no<9; no++ ){
                int noB = (1<<no);   

                foreach( var (CRL,rcS) in _GetXChain(no) ){

                    // ===== X-Chain found =====
                    UInt128 ELM = CRL[2];
                    foreach( var P in ELM.IEGet_UCell_noB(pBOARD,noB) )  P.CancelB=noB; 
                    SolCode = 2;   
                    string SolMsg=$"X-Chain #{(no+1)} rcS:{rcS.ToRCString()}({rcS})";
                    Result=SolMsg;
                    if( SolInfoB ){  
                        (CRL[0].DifSet(ELM)).IE_SetNoBBgColor( pBOARD, noB, AttCr, Cr1 );     
                        (CRL[1].DifSet(ELM)).IE_SetNoBBgColor( pBOARD, noB, AttCr, Cr2 );  
                        ELM.IE_SetNoBBgColor( pBOARD, noB, AttCr, Cre );
                        ResultLong=SolMsg;
                    }

					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                }
            }
            return false;
        }
             
        private IEnumerable<(UInt128[],int)> _GetXChain( int no ){  //## Repeatedly coloring processing.
            UInt128 BPno = FreeCell81b9[no];           // Bit representation of the cells with #no as a candidate
            UInt128 BPstrong = CeLKMan.GetCellsWithStrongLink(no);
            
            UInt128[] CRL=new UInt128[3];                           // coloring 2 groups(CRL[0] and CRL[1]).
            var rcQue=new Queue<(int,int)>();
            int rcS;
            while( (rcS=BPstrong.FindFirst_rc()) >= 0 ){            // rcS:Set the origin cell.
                                //WriteLine( $"--1--  no:{no} BPstrong:{BPstrong.ToBitString81()}" );               
                BPstrong = BPstrong.Reset(rcS);                              // reset BPno to Processed.

                //--- initialize ---
                CRL[0] = UInt128.Zero; CRL[1] = UInt128.Zero; CRL[0] = CRL[0].Set(rcS);
                rcQue.Clear(); 
                rcQue.Enqueue( (rcS,1) );                           // start cell. 1:with Strong Link 

                //--- start ---
                while( rcQue.Count>0 ){
                    var (rc1,swF1) = rcQue.Dequeue( );              // origin (cell,color)
                    int swF2 = 3-swF1;   // 1 <--> 2                // change color
                                //WriteLine( $"  --2-- rc1:{rc1.ToRCString()}({rc1}) swF1:{swF1}" );

                    // Search with swF1=1 only targets Strong-link, with swF1=2 targets any link.
                    // Strong link is also Weak link. 
                    foreach( var LKx in CeLKMan.IEGetRcNoType(rc1,no,swF1) ){//LKx:link connected to cell rc1                                          
                        int rc2  = LKx.rc2;                         // anather cell of LKx                      
                        if( (CRL[0]|CRL[1]).IsHit(rc2) ) continue;  // already colored
                                //WriteLine( $"    --3-- rc2:{rc2.ToRCString()}({rc2}) swF1:{swF1}" );

                        CRL[swF2-1] = CRL[swF2-1].Set(rc2);         //(1,2) -> (0,1) Array index adjustment
                        rcQue.Enqueue( (rc2,swF2) );    //WriteLine($" {rc1} -> {rc2} {swF1}" );
                    }
                }               

                CRL[2] = (ConnectedCells81[rcS] & CRL[0]);    // CRL[1].Count>1 //always be satisfied,
                if( CRL[2].BitCount()>0 ){ 
                                //WriteLine( $"\n BPno    :{BPno}\n BPstrong:{BPstrong.ToBitString81()}" );
                                //WriteLine( $" CRL0    :{CRL[0].ToBitString81()}" );
                                //WriteLine( $" CRL1    :{CRL[1].ToBitString81()}" );
                                //WriteLine( $" CRL2    :{CRL[2].ToBitString81()}" );
                    yield return (CRL,rcS);
                }
                //--- end --- 
            }
            yield break;
        }

    }
}