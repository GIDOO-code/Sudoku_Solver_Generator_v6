using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPX_space{

    // Make a pattern of Sudoku Puzzles.
    // Supports symmetrical pattern creation.
    public class PatternGenerator{

        public GNPX_App_Ctrl    pApp_Cntrl;
		private G6_Base			G6 => GNPX_App_Man.G6;
        public int[,]			GPat;
        public int[]			PatB = new int[9];
		public int[]			GPat81;
		public int              pattenCount => GPat81.ToList().Count(p=>p>0);

        public PatternGenerator( GNPX_App_Ctrl SDKCx ){
            this.pApp_Cntrl = SDKCx;
            GPat = new int[9,9];
        }



        // Automatic Pattern Generation.
        // With symmetry and specified number, Generate a pattern.
        public int patternAutoMaker( int rdb_patSel ){
            int CellNumMax = G6.CellNumMax;
            int nc, rB=0, cB=0, bB=0;

            do{
                GPat = new int[9,9];
                for( nc=0; nc<CellNumMax; ){
                    int r, c;
                    do{
                        r = GNPX_App_Ctrl.GNPX_Random.Next(0,9);
                        c = GNPX_App_Ctrl.GNPX_Random.Next(0,9);
                    }while(GPat[r,c]!=0);
                    nc = symmetryPattern(rdb_patSel,r,c,true);
                    if(nc>CellNumMax) break;
                }

                for(int r=0; r<9; r++ ){
                    for(int c=0; c<9; c++ ){
                        if( GPat[r,c]==0 ) continue;
                        rB |= 1<<r;
                        cB |= 1<<c;
                        bB |= 1<<(r/3*3+c/3);
                    }
                }

                rB = rB.BitCount();
                cB = cB.BitCount();
                bB = bB.BitCount();
            }while( rB<8 || cB<8 || bB<8 );
            _PatternToBit( );
            return nc;
        }


        public int Count(){
            int nn=0;
            for(int rc=0; rc<81; rc++ ) if( GPat[rc/9,rc%9]!=0) nn++;
            return nn;
        }


        // Auxiliary routine for symmetric pattern generation.
        public int symmetryPattern( int rdb_patSel, int r, int c, bool setFlag ){
            int pat = setFlag? 1: 1-GPat[r,c];
            GPat[r,c] = pat;
            switch(rdb_patSel){
                case 0: break;
                case 1: GPat[8-c,r]  =GPat[8-r,8-c]=GPat[c,8-r]=pat; break;
                case 2: GPat[8-r,8-c]=pat; break;
                case 3: GPat[r,8-c]  =pat; break;
                case 4: GPat[8-r,c]  =pat; break;
                case 5: GPat[c,r]    =pat; break;
                case 6: GPat[8-c,8-r]=pat; break;
            }
            int nn = Count();
            _PatternToBit( );

            return nn;
        }




        public int patternImport( UPuzzle sPZL ){
            int nc=0;
            foreach( var P in sPZL.BOARD ){
                int n = P.No>0? 1: 0;
                GPat[P.r,P.c]= n;
                nc += n;
            }
            _PatternToBit( );   //Bit Representation of the Pattern
            return nc;
        }
  
        


        // Auxiliary routine when creating Latin Square
        private void _PatternToBit( ){
            for(int r=3; r<9; r++ ){
                int pb=0;
                for(int c=3; c<9 ; c++ ){
                    if(GPat[r,c]>0) pb |= (1<<c);
                }
                PatB[r] = pb;
            }
			GPat81 = GPat.Cast<int>().ToArray();
            return;
        }
 
    }
}