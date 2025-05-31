using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Reflection.Emit;
using System.Windows.Documents;
using System.Diagnostics;

namespace GNPX_space{
    //Advanced solving techniques
    // http://forum.enjoysudoku.com/advanced-solving-techniques-f6.html

    //Pattern Overlay Method
    // http://forum.enjoysudoku.com/pattern-overlay-method-t40180.html ...C

    //The tridagon rule
    // http://forum.enjoysudoku.com/the-tridagon-rule-t39859.html  ... C

    //Strong links within Fish Patterns:
    // http://forum.enjoysudoku.com/strong-links-within-fish-patterns-t30392.html ... A


    public class Research_trial: AnalyzerBaseV2{

        // This function is for developing new Sudoku algorithms. Not included in standard GNPX applications.
        // To change the puzzle's appearance, need to solve the puzzle.
        // There are puzzles that the implemented algorithm cannot solve.
        // So, i solve the puzzle by "trial and error".
		// This code is relatively fast. A typical Sudoku puzzle can be solved in 1-2 milliseconds.

		// To determine whether a puzzle has multiple solutions, set upperLimit=2.
		// If you specify a file name, multiple solutions will be output to the file.

        // The world's hardest sudoku
        //  8..........36......7..9.2...5...7.......457.....1...3...1....68..85...1..9....4..



		// <<< inner class >>>
			public class sUCell{ //Basic Cell Class
				public readonly int  rc;    //cell position(0-80)
				public readonly int  r;     //row
				public readonly int  c;     //column
				public readonly int  b;     //block

				public int      No;         //>0:Puzzle  =0:Open  <0:Solution
				public int      FreeB;      //Bit expression of candidate digits


				public sUCell( ){}
				public sUCell( int rc, int No=0, int FreeB=0 ){
					this.rc=rc; this.r=rc/9; this.c=rc%9; this.b=rc/27*3+(rc%9)/3;
					this.No=No; this.FreeB=FreeB;   
				}

				public override string ToString(){
					string st = $" sUCell rc:{rc}[r{r+1}c{c+1}]  no:{No}";
					st +=" FreeB:" + FreeB.ToBitString(9);
					return st;
				}
			}


		// <<< input, basic control variables >>>
        private int[]		  RowF=new int[9], ColF=new int[9], BlkF=new int[9];
        private List<int>[]   RowPosLst = new List<int>[9];    //Undetermined cells position in row
        private List<int>[]   RowDigLst = new List<int>[9];    //Undetermined numbers in row
		private List<int>     iBoard_inner;






		// <<< output >>>
        public  List<int[]>   RT_Get_SolList => solList;
		public  int[]		  RT_Get_Solution_iArray => solList[0];  // <-- If 'solList' has size zero, the index is out of range.
		public  int[]	      RT_Get_Solution_iAbs => RT_Get_Solution_iArray.ToList().ConvertAll(p=>Abs(p)).ToArray();
		// <<< Working variables >>>
		private List<sUCell>  sBoard = new();

        private  List<int[]>  solList = new();
        private int[]		  Sol = new int[81];

		// ... Operation control variables ...
		private bool		  filePutB=true;
		private int			  upperLimit = int.MaxValue;
		private bool		  devPrintB  = false;

		private string		  dirName = "Debug";
		private string		  TE_fileName => $"Multiple_Solutions {DateTime.Now.ToString("yyyyMMdd")}.txt";

		private bool		  _SearchInterrupted;
		private object		  obj = new object();
		private long		  hashVal_Memo=0;

							  
		public  List<UCell>   RT_Get_Board(){
			List<int>	solList = RT_Get_Solution_iArray.ToList();
			List<UCell> board = new();
			foreach( var (no,rc) in solList.WithIndex() ) board.Add( new UCell(rc,no) );
			return board;
		}
							  
        public Research_trial( GNPX_AnalyzerMan AnMan ): base(AnMan){ }
		
			private  void TrialAndErrorApp_Initial_Setup( List<int> iBoard, bool parallelMode=true ){
				// Preparer_Research_trial
			
				if( !parallelMode ){
					long hv = iBoard.Get_hashValue_int81();
					if( hv == hashVal_Memo )  return;
					hashVal_Memo = hv;
				}

			  #region Initial_Setup
				// <<< Further speed improvements >>>
				// Identify repeated searches of the same pattern and do not set preconditions

				lock(obj){
					int rcX =0;
					sBoard = iBoard.ConvertAll( n => new sUCell( rcX++, n, 0 ) );
				}

				foreach( sUCell P in sBoard.Where(p=>p.No==0) ){
					int freeB9 = ConnectedCells81[P.rc].IEGet_rc().Aggregate(0,(p,q)=> p|= (1<<sBoard[q].No));
					P.FreeB = (freeB9>>1) ^ 0x1FF;
				}
				// -------------------------------------------------------------------------------------

				// <<< Prepare >>>
				for( int k=0; k<9 ; k++ ){ RowPosLst[k]=new();	RowDigLst[k]=new(); }
				RowF=new int[9]; ColF=new int[9]; BlkF=new int[9];
				Sol = new int[81];			//@@0 0614

				for( int rc=0; rc<81; rc++ ){
					sUCell P = sBoard[rc];
					if( P.No != 0 )  Sol[P.rc] = Abs(P.No);
					else{
						RowF[rc/9] |= P.FreeB;										// Unfixed digits in row (bit representation)
						ColF[rc%9] |= P.FreeB;										// Unfixed digits in column (bit representation)
						BlkF[rc.ToBlock()] |= P.FreeB;								// Unfixed digits in block (bit representation)
						RowPosLst[rc/9].Add(rc);									// Unfixed cell position within row
					}
				}

				for( int k=0; k<9; k++ ){
					foreach( var p in RowF[k].IEGet_BtoNo() )  RowDigLst[k].Add(p); // Unfixed digits in row
				}

						// <<< Further2 speed improvements >>>
						//  Efficient detection of invalid states in combination generation.
						for( int r=0; r<8; r++ ){
							int nc = RowF[r].BitCount();
							if( nc<=1 )  continue;
										if(devPrintB){
											WriteLine( "");
											_Check_RowPos_RowDig_Lst( "000000", r, nc );
										}

							for( int cX=0; cX<nc; cX++ ){
								int c = RowPosLst[r][cX] % 9;
								for( int r2=r+1; r2<9; r2++ ){
									if( iBoard[r2*9+c]>0 ){ RowPosLst[r][cX] += 100; RowDigLst[r][cX] += 10; }
								}
							}
										if(devPrintB) _Check_RowPos_RowDig_Lst( "set 10", r, nc );

							RowPosLst[r].Sort( (a,b) => b/100-a/100 );
							RowDigLst[r].Sort( (a,b) => b/10-a/10 );
										if(devPrintB) _Check_RowPos_RowDig_Lst( "Sort  ", r, nc );
										
							RowPosLst[r] = RowPosLst[r].ConvertAll(x=>x%100);
							RowDigLst[r] = RowDigLst[r].ConvertAll(x=>x%10);
										if(devPrintB) _Check_RowPos_RowDig_Lst( "Improv", r, nc );	

										void _Check_RowPos_RowDig_Lst( string stT, int r, int nc ){
											string st = $"*{stT} row:{r} nc:{nc} Pos:{string.Join(" ",RowPosLst[r])}  Dig:{string.Join(" ",RowDigLst[r])}" ;
											WriteLine( st);
										}
						}

			  #endregion Initial_Setup
			}




        public bool TrialAndErrorApp( List<int> iBoard, bool filePutB, int upperLimit=2 ){//=int.MaxValue ){
			if( (iBoard is null) || iBoard.Count!=81 )  return false; 
					//iBoard.__DBUG_Print2( true, "iBoard");		
					
				TrialAndErrorApp_Initial_Setup( iBoard );


			this.filePutB      = filePutB;
			this.upperLimit    = upperLimit;

			_SearchInterrupted = false;

						

							
			solList.Clear();
			// <<< TrialAndError Solve >>>
			Set_RowLine( 0, RowF, ColF, BlkF );			//	>> first entry

			


			// <<< Result >>>
			if( solList.Count == 1 ){		// ... There is only one solution. This is a Sudoku Puzzle. 
				SolCode = 1;
                Sol = solList[0];
                ResultLong = Result = "Trial And Error";
                pBOARD.ForEach( P => { if( Sol[P.rc]<0 )  P.FixedNo=-Sol[P.rc]; } );		
				
				int rc=0;
				G6.g7FinalState_TE = Sol.ToList().ConvertAll(p => new UCell(rc++,p));
				return true;
            }
            else if( solList.Count >= 2 ){	// ... There are multiple solutions. It's not a Sudoku Puzzle. 
				SolCode = -1; 
				ResultLong = Result = "There are multiple solutions."; 			
				if(filePutB) fileOutput_TE_multisolution();  // In parallel processing, it is prohibited.
            }

            return false;


							// ----------------------------------------------------------
							void fileOutput_TE_multisolution( ) {
								if( !filePutB || TE_fileName=="" || solList.Count<=1 )  return;

								if( !Directory.Exists(dirName) )  Directory.CreateDirectory(dirName);

								using(var fpW = new StreamWriter( $"{dirName}/{TE_fileName}", append: true, Encoding.UTF8)) {
									fpW.WriteLine($"\n{DateTime.Now}  {solList.Count} solutions");
									fpW.WriteLine($"{string.Join("", iBoard).Replace("0", ".")}  puzzle");

									string unavoidable = __Find_unavoidable(solList);
									fpW.WriteLine($"{unavoidable}  @:unavoidable cell");

									int mx = 0;
									solList.ForEach(Q => fpW.WriteLine($"{string.Join("", Q).Replace("-", "")}  solution:{++mx:00}"));
								}
							}	

							string __Find_unavoidable( List<int[]> solList ){
								int[] FreebList = new int[81];
								solList.ForEach( P => {
									for( int rc=0; rc<81; rc++ ) FreebList[rc] |= (1<<(Abs(P[rc])));
								} );
								var Q = FreebList.ToList().ConvertAll(p=> p.BitCount()==1? ".": "@" );			
								return string.Join("",Q);
							}
		}



        private int solcc = 0;      
        private bool Set_RowLine( int rowNo, int[] RowF0, int[] ColF0, int[] BlkF0 ){
            // Check_sol( rowNo, RowF0, ColF0, BlkF0 );

						if( rowNo==9 ){   // ... found 
							if( devPrintB ) Check_sol( rowNo, RowF0, ColF0, BlkF0);//, Sol0 );

							SolCode = 1;
							int[] SolCpy = new int[81];
							for( int k=0; k<81; k++ )  SolCpy[k] = Sol[k];
							//SolCpy.__DBUG_Print2( true, $"SolCpy solList[{solList.Count}]");
							solList.Add(SolCpy); 
				
							if( solList.Count >= upperLimit )  _SearchInterrupted = true; //More than the specified number of solutions were found.
							return true;
						}



			// <<< Processing the (rowNo)-th row >>

            int nc = RowF[rowNo].BitCount();										// digit of blanks in row

            if( nc==0 ){	// All the digits in this row are fixed.
                bool ret = Set_RowLine( rowNo+1, RowF0, ColF0, BlkF0 );				// ---> recursiv call 
				if( _SearchInterrupted )  return false;  
            }


            else{
                List<int> RowPosLst_rowNo = RowPosLst[rowNo];						// Unfixed cell position within row.
                List<int> RowDigLst_rowNo = RowDigLst[rowNo];						// Unfixed digits in row (bit representation)

                int[]   ColF1=new int[9], BlkF1=new int[9];
                Permutation prmX = new Permutation(nc);
                int nxt=9;
                while( prmX.Successor(skip:nxt) ){

					// Copy the process up to this point
                    for( int k=0; k<9 ; k++ ){ ColF1[k]=ColF0[k]; BlkF1[k]=BlkF0[k]; }	// Unfixed digits in the k-th column / k-th block
																						// (Copying blocks(BlkF1) is redundant. But simpler is faster.)
					nxt = 999;     
					for( int k=0; k<nc; k++ ){

							try{  //================================
								int rc=RowPosLst_rowNo[k], c=rc%9,  b=rc.ToBlock();
										//int mm = prmX.Index[k];
										//if( mm<0 || mm>=RowDigLst_rowNo.Count )  WriteLine( $"prmX.Index[k]:{prmX.Index[k]}" );

								int No  = RowDigLst_rowNo[ prmX.Index[k] ];		// No: k-th digit in (rowNo)-th row,
								int noB = 1<<No;								// bit representation of No

								if( (ColF1[c]&noB) == 0 ){ nxt=k; break; }		// Is No used in c-th column?
								if( (BlkF1[b]&noB) == 0 ){ nxt=k; break; }		// Is No used in b-th block?
								ColF1[c] = ColF1[c].DifSet(noB);				// if not used, remove from unused column=list (difference opperation)
								BlkF1[b] = BlkF1[b].DifSet(noB);				// if not used, remove from unused block-list (difference opperation)
								Sol[rc] = -(No+1);
							}
							catch(Exception e){ 				
										// An incomprehensible phenomenon is occurring. ?????
										// his occurs when the input iBoard is changed externally.
										// This can cause problems when using parallel processing.

								WriteLine( $"{e.Message}\n{e.StackTrace}" );
								WriteLine( $" *** RowDigLst_rowNo : {string.Join(" ",RowDigLst_rowNo)}" );
								WriteLine( $" *** prmX.Index : {string.Join(" ",prmX.Index.ToList())}" );
								__DBUG_Print2( Sol, sqF:true, "Set_RowLine" );
								nxt=k; break;
							}	//----------------------------------

					}
					if( nxt<999 )  continue;	//goto L_next_prmX;


					bool ret1 = Set_RowLine( rowNo+1, RowF0, ColF1, BlkF1 );	// >> recursiv call 
					if( _SearchInterrupted )  return false;
				
                }
                return false;
            }

            return (rowNo==0 && SolCode>0);

			// -------------------------------------------------
					void Check_sol( int rowNo, int[] RowF0, int[] ColF0, int[] BlkF0 ){
						solcc++;
						if( rowNo <= 7 )  return;
						string st = $"** rowNo:{rowNo} solcc:{solcc}";

					  //for( int k=0; k<9; k++ )  st += $"\rfree k:{k}  r:{RowF0[k].ToBSt()}  c:{ColF0[k].ToBSt()}  b:{BlkF0[k].ToBSt()} "; 
						for( int rc=0; rc<81; rc++ ){
							if( rc%9 == 0 )  st += $"\r  {rc/9}: ";
							int s = Sol[rc];
							st += (s>0)? $"  {s}": (s<0)? $" {s}": "  .";
						}
						WriteLine( st );
					}
		}





		private void __DBUG_Print2( List<int> X, bool sqF, string st="" ){
			__DBUG_Print2( X.ToArray(), sqF, st="" );
		}
        private void __DBUG_Print2( int[] X, bool sqF, string st="" ){
            string st2, p2="", stZero="";
            if(sqF) WriteLine("\r");
            for(int r=0; r<9; r++ ){
                st2 = "";
                for(int c=0; c<9; c++ ){
                    int wk = X[r*9+c];
                    if(wk==0) st2 += "  .";
                    else st2 += wk.ToString().PadLeft(3);
					stZero += Max(wk,0).ToString();
                }
                if(sqF) WriteLine(st+r.ToString("##0:")+st2);
                p2 += " "+st2.Replace(" ","");
            }
            WriteLine( $"{st} {p2}\n{st} {stZero.Replace("0",".")}" );
        }
        private void __DBUG_Print2Abs( int[] X, bool sqF, string st="" ){
            string st2, p2="";
            if(sqF) WriteLine("\r");
            for(int r=0; r<9; r++ ){
                st2 = "";
                for(int c=0; c<9; c++ ){
                    int wk=Abs(X[r*9+c]);
                    if(wk==0) st2 += " .";
                    else st2 += wk.ToString(" #");
                }
                if(sqF) WriteLine(st+r.ToString("##0:")+st2);
                p2 += " "+st2.Replace(" ","");
            }
            WriteLine(st+" "+p2);
        }
    }
}