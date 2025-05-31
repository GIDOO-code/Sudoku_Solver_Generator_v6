using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.IO;
using System.Text;

namespace GNPX_space{
    //  Sue de Coq
    //  http://hodoku.sourceforge.net/en/tech_misc.php#sdc
    //  A more formal definition of SDC is given in the original Two-Sector Disjoint Subsets thread:
    //  Consider the set of unfilled cells C that lies at the intersection of Box B and Row (or Column) R.
    //  Suppose |C|>=2. Let V be the set of candidate values to occur in C. Suppose |V|>=|C|+2.
    //  The pattern requires that we find |V|-|C|+n cells in B and R, with at least one cell in each, 
    //  with at least |V|-|C| candidates drawn from V and with n the number of candidates not drawn from V.
    //  Label the sets of cells CB and CR and their candidates VB and VR. Crucially,
    //  no candidate from V is allowed to appear in VB and VR. 
    //  Then C must contain V\(VB U VR) [possibly empty], |VB|-|CB| elements of VB and |VR|-|CR| elements of VR.
    //  The construction allows us to eliminate candidates VB U (V\VR) from B\(C U CB), 
    //  and candidates VR U (V\VB) from R\(C U CR).
    // (\:backslash)

    // SueDeCoq
    // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page45.html

    //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
    //.4...1.286..5....7..7.46...7.3..98..9.......2..12..3.9...95.1..1....2..559.1...7.

    //87........9.81.65....79...8.....67316..5.1..97124.....3...57....57.48.1........74
    //87.....9..9381.657...79...8.4.286731638571..97124398653.415798..57.48.1..8....574  //for develop

    //.2...3..4.4....25.6...243.8256..8....8..9..2....2..4868.463...2.63....4.9..7...6.
    //.2...36.434..6.25.69..243.82564.8.3.48.396.25.3925.48687463.5.2.63..2.4.912745863  //for develop

    //hodoku
    //3248615976579438219185273642.143....5.32.6...4..7.....1..6.....8..1746..7..39..1. //Q1 ok
    //641..8329873291645592..4187.8..2.4.1.....92.3...41.8.6......73..6.9..51...714.968 //Q2 another solution
    //82..6.....6.8...2...32..568641...37.53......4.87...6..4563.97..37...1.......5..3. //Q3 ok
    //329...5.........29.6.....3.9342..765716..5482258764..3.73....5.1954.3.7...25..3.. //Q4 ok

    public partial class AnLSTechGen: AnalyzerBaseV2{

		private int stageNoPMemo = -9;
		private ALSLinkMan fALS;

		public AnLSTechGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){
			fALS = new ALSLinkMan(pAnMan);
        }

		internal bool		debugPrint = false;
		internal string     debug_Result = "";
		internal string		fName_debug = "SueDeCoqEx2.txt";
		internal void Debug_StreamWriter( string LRecord ){
//#if DEBUG
			//if(!debugPrintFSDC)  return;
            using( var fpW=new StreamWriter(fName_debug,true,Encoding.UTF8) ){
                fpW.WriteLine(LRecord);
            }
//#endif
		}

	/* =============================================
		Classic version
	   ============================================= */

        public bool SueDeCoq( ){
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();

				fALS.Initialize(); 
			}
			                
			fALS.Prepare_ALSLink_Man(nPlsB:+2, maxSize:5 ); //Generate ALS(+1)   
            if( fALS.ALSList==null || fALS.ALSList.Count<=3 ) return false;
                 // fALS.ALSList.ForEach( P => WriteLine(P) );


            foreach( var ISPB in fALS.ALSList.Where(q=> q.Size>=3 && q.houseNo_Blk>=0) ){ //Selecte Block-type ALS
                int hb = ISPB.houseNo_Blk;  
                if( hb <= 1) continue;  //Block squares have multiple rows and columns

                    //WriteLine( $"ISPB:{ISPB}" );

                foreach( var ISPR in fALS.ALSList.Where(q=> q.Size>=3 && q.houseNo_Blk<0 ) ){　//Selecte Row-type/Column-type ALS
                  //  if( pAnMan.Check_TimeLimit() )  return false;

                    if( ISPR.houseNo_Row<0 && ISPR.houseNo_Col<0 )  continue;

                    //Are the cell configurations of the intersections the same?
                    int hs = (ISPR.houseNo_Row>=0)? ISPR.houseNo_Row:
                             (ISPR.houseNo_Col>=0)? ISPR.houseNo_Col:
                             -1;
                    if( hs<0 )  continue;
                    if( (ISPB.bitExp & HouseCells81[hs]) != (ISPR.bitExp & HouseCells81[ISPB.houseNo_Blk]) ) continue;
                        //WriteLine( $"ISPR:{ISPR}" );

                    // ***** the code follows HP -> https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page45.html *****
                    UInt128 IS = ISPB.bitExp & ISPR.bitExp;                               //Intersection
                    if( IS.BitCount()<2 ) continue; 　                               //At least 2 cells at the intersection
                    if( (ISPR.bitExp.DifSet(IS)).BitCount() == 0 ) continue;                      //There is a part other than the intersecting part in ISPR                    

                    UInt128 PB = ISPB.bitExp.DifSet(IS);                             //ISPB's outside IS
                    UInt128 PR = ISPR.bitExp.DifSet(IS);                             //ISPR's outside IS
                    int IS_FreeB = IS.UInt128AggregateFreeB(pBOARD);                 //Intersection number
                    int PB_FreeB = PB.UInt128AggregateFreeB(pBOARD);                 //ISPB's number outside the IS
                    int PR_FreeB = PR.UInt128AggregateFreeB(pBOARD);                 //ISPR's number outside the IS
                    
                    if( (IS_FreeB&PB_FreeB&PR_FreeB)>0 ) continue;

                    //A.DifSet(B): A-B = A&(B^0xFFFFFFFF)
                    int PB_FreeBn = PB_FreeB.DifSet(IS_FreeB);                      //Digits not at the intersection of PB
                    int PR_FreeBn = PR_FreeB.DifSet(IS_FreeB);                      //Numbers not in the intersection of PR

                    int sdqNC = PB_FreeBn.BitCount()+PR_FreeBn.BitCount();          //Number of confirmed numbers outside the intersection
                    if( IS_FreeB.BitCount()-IS.BitCount() != (PB.BitCount() + PR.BitCount() - sdqNC) ) continue;

                    int elmB = PB_FreeB | IS_FreeB.DifSet(PR_FreeB);                //Exclusion Number in PB 
                    int elmR = PR_FreeB | IS_FreeB.DifSet(PB_FreeB);                //Exclusion Number in PR                
                    if( elmB==0 && elmR==0 ) continue;

                    foreach( var P in _GetRestCells(ISPB,hb,elmB) ){ P.CancelB|=P.FreeB&elmB; SolCode=2; }
                    foreach( var P in _GetRestCells(ISPR,hs,elmR) ){ P.CancelB|=P.FreeB&elmR; SolCode=2; }

                    if(SolCode>0){      //--- SueDeCoq found -----
						//WriteLine( $"\n\nISPB:{ISPB}\nISPR:{ISPR}" );
                        SolCode=2;
                        SuDoQue_SolResult( ISPB, ISPR, hs );
                        if( ISPB.Level>=3 || ISPB.Level>=3 ) WriteLine("Level-3");

  						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                    }
                }
            }
            return false;
        }

        private IEnumerable<UCell> _GetRestCells( UAnLS ISP, int h, int selB ){
            return pBOARD.IEGetCellInHouse(h,selB).Where(P=> !ISP.bitExp.IsHit(P.rc) );
        }

        private void SuDoQue_SolResult( UAnLS ISPB, UAnLS ISPR, int hs ){
            int level = Math.Max( ISPB.Level, ISPR.Level );
            string stT = "SueDeCoq";
            if( level > 1 )  stT += $" (ALS Level-{level})";
            Result = stT;

            if( SolInfoB ){
                ISPB.UCellLst.IE_SetNoBBgColor( pBOARD, 0x1FF, AttCr, SolBkCr );    // FreeB_Clr, Digit_Clr, background_Clr
                ISPR.UCellLst.IE_SetNoBBgColor( pBOARD, 0x1FF, AttCr, SolBkCr );

                string st = "\r ALS";
                if( ISPB.Level==1 ) st += "(block)  ";
                else{ st += $"-{ISPB.Level}(block)"; }
                st += $": {ISPB.ToStringRCN()}";

                st += "\r ALS" + ((ISPR.Level==1)? "": "-2");
                st += ((hs<9)? "(row)":"(col)");
                st += ((ISPR.Level==1)? "    ": "  ");
                st += ": "+ISPR.ToStringRCN();
                ResultLong = stT+st;
            }
        }
    }
}