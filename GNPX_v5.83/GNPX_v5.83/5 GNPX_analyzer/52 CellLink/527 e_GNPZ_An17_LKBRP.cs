using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
//using System.Security.Cryptography.Xml;
using System.Windows.Media;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPX_space{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{

        //RemotePair is an algorithm that connects bivalue cells with a StrongLlink.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page47.html

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //.3..9.68...9.64..2..7..8.5.84.6.9....26...41....2.1.96.9.4..1..6..81.5...14.5..6.
        //2..8..1...8.4..6.3...2968...1..3.2.43.......69.5.8..3...1324...6.2..8.1...8..1..2

        public bool RemotePair( ){     //RemotePairs
			PrepareStage(); 
            if( BVCellLst==null )  BVCellLst = pBOARD.FindAll(p=>(p.FreeBC==2)); //BV:bivalue
            if( BVCellLst.Count<3 ) return false;  

            foreach( var (CRL,FreeB) in _RPColoring()){
                bool RPFound=false;
                foreach( var P in pBOARD.Where(p=>(p.FreeB&FreeB)>0) ){
                    if( (CRL[0] & ConnectedCells81[P.rc]).IsZero() )  continue;
                    if( (CRL[1] & ConnectedCells81[P.rc]).IsZero() )  continue;                  
                    P.CancelB = P.FreeB&FreeB; RPFound=true;
                }

                if( RPFound ){ //=== found ===
                    SolCode = 2;
                    string SolMsg="Remote Pair #"+FreeB.ToBitStringN(9);
                    Result=SolMsg;
                    if(!SolInfoB) return true;
                    ResultLong = SolMsg;

                    Color Cr  = _ColorsLst[0];
                    Color Cr1 = Color.FromArgb(255,Cr.R,Cr.G,Cr.B);   
                    Color Cr2 = Color.FromArgb(150,Cr.R,Cr.G,Cr.B);
                    foreach(var P in CRL[0].IEGet_rc().Select(p=>pBOARD[p]))  P.Set_CellColorBkgColor_noBit( FreeB, AttCr, Cr1 );
                    foreach(var P in CRL[1].IEGet_rc().Select(p=>pBOARD[p]))  P.Set_CellColorBkgColor_noBit( FreeB, AttCr, Cr2 );

					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                    RPFound = false;
                }
            }
            return false;
        }

        private IEnumerable<(UInt128[],int)> _RPColoring( ){
            if( BVCellLst.Count<4 )  yield break;
          
            // --- coloring with bivalue cells ---
            UInt128 BivalueB = BVCellLst.Aggregate( UInt128.Zero, (p,q)=> p| UInt128.One<<q.rc );
                       // WriteLine( $" BivalueB:{BivalueB.ToRCString()}" );      
            UInt128 usedB = UInt128.Zero;
            var QueTupl = new Queue<(int,int)>();

            UInt128[] CRL=new UInt128[2]; 
            CRL[0]=UInt128.Zero; CRL[1]=UInt128.Zero; 
            int  rc0;
            while( (rc0=BivalueB.FindFirst_rc())>=0 ){              //Start searching from rc0
                BivalueB = BivalueB.Reset(rc0);
                        // WriteLine( $" 000 BivalueB{BivalueB.ToRCString()}" );

                CRL[0]=UInt128.Zero; CRL[1]=UInt128.Zero;           //Clear chain
                
                QueTupl.Clear();                                    //Queue(QueTupl) initialization
                QueTupl.Enqueue( (rc0,0) );
                
                int FreeB = pBOARD[rc0].FreeB;         
                usedB = UInt128.Zero;
                while( QueTupl.Count>0 ){
                    var (rc1,color1) = QueTupl.Dequeue();           //Get Current Cell
                    usedB = usedB.Set(rc1);
                    CRL[color1] = CRL[color1].Set(rc1);
                    int color2 = 1-color1;                          //color inversion

                    UInt128 Chain = BivalueB & ConnectedCells81[rc1];
                    foreach( var rc2 in Chain.IEGet_rc().Where(rc=> !usedB.IsHit(rc)) ){
                        if( pBOARD[rc2].FreeB!=FreeB ) continue;
                        QueTupl.Enqueue( (rc2,color2) );
                        CRL[color2] = CRL[color2].Set(rc2);
                    }
                }
                
                if( CRL[1].BitCount()>0 ) yield return (CRL,FreeB);
                BivalueB = BivalueB.DifSet(CRL[0]|CRL[1]);
                         // WriteLine( $"{(CRL[0]|CRL[1]).ToRCString()}" );
                         // WriteLine( $" 111 BivalueB{BivalueB.ToRCString()}" );
            }
            yield break;
        }
    }
}