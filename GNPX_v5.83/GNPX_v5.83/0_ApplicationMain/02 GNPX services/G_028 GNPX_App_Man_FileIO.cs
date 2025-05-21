using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using static System.Math;
using static System.Diagnostics.Debug;

using System.Threading;

using GIDOO_space;
using System.Diagnostics;
using Microsoft.Win32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GNPX_space{
	using pGPGC = GNPX_Puzzle_Global_Control;

    public partial class GNPX_App_Man {

        public int SDK_FileInput( string fName, bool puzzleIniFlag ){ 
            char[] sep=new Char[]{ ' ', ',', '\t' };        
            string line, puzzleName="";

            using( StreamReader sr=new StreamReader(fName) ){
                pGPGC.GNPX_Puzzle_List_Initialize();

                while( (line=sr.ReadLine()) !=null ){
                    if( line=="" ) continue;
					if( line[0]=='-' )  continue;
                    
                    // Supports the format "Contain a blank every 9 digits" and similar type.
                    int n81 = _Find_81Digits(line);
                    if( n81 > 81 ){
                        string st = line.Substring(0,n81).Replace(" ","").Replace(".","0");
                        if( st.Length==81 && st.All(char.IsDigit) ){ line = st + line.Substring(n81); }
                    }

                    string[] eLst = line.SplitEx(sep);
                    if( line[0]=='#' ){ puzzleName=line.Substring(1); continue; }		// comment line
                    if( eLst[0] == "sPos" ) continue;									// obsolete format
																						// 
                    int    nc = eLst.Length;
                    string name="";
                    int    difLvl=99;
                    if( eLst[0].Length>=81 ){
                        try{
							if( nc>=2 && eLst[1].IsNumeric() ) difLvl = eLst[1].ToInt();
							if( nc>=3 ){					   name   = eLst[2]; }  
                            string st = eLst[0].Replace(".", "0").Replace("-", "0").Replace(" ", "");
                            List<UCell> tmpBoard = _stringToBDL(st);
                            string TimeStamp =  _Find_TimeStamp( eLst );
                            int ID = pGPGC.GNPX_Puzzle_List.Count;
							UPuzzle Q = new(ID,tmpBoard,name,difficultyLevel:difLvl,TimeStamp);
                            pGPGC.GNPX_Puzzle_List_Add( Q );  
                        }
                        catch{ continue; }                   
                    }
                }

                pGPGC.current_Puzzle_No = 0;
                return  pGPGC.current_Puzzle_No;

                // ----------------- inner function -----------------
                int _Find_81Digits( string st ){
                    int n=0, m=0;
                    foreach( var p in st ){ m++; if( (char.IsDigit(p) || p=='.') && (++n)>= 81 )  break; }
                    return (n==81)? m: 0;
                }   

                string _Find_TimeStamp( string[] eList ){
                    string TimeStamp="";
                    foreach( var e in eList){ if( DateTime.TryParse(e,out _) ){ TimeStamp=e; break; } }
                    return TimeStamp;
                }
            }   
        }


        public UPuzzle SDK_ToUPuzzle( int[] SDK81, string name="", int difLvl=0, bool saveF=false ){
            if(SDK81==null) return null;
            string st="";
            foreach( var p in SDK81 ){ 
                if(p>9) st += "0";
                else st += (p<=9)? p.ToString(): ".";
            }
            var tmpPZL = SDK_ToUPuzzle( st, name, difLvl, saveF);
            return tmpPZL;
        }
        public UPuzzle SDK_ToUPuzzle( string st, string name="", int difficultyLevel=0, bool saveF=false ){
            List<UCell> B = _stringToBDL(st);
            if(B==null)  return null;
            var tmpPZL=new UPuzzle( int.MaxValue, B, name, difficultyLevel:difficultyLevel );
            if(saveF) pGPGC.GNPX_Puzzle_List_Add(tmpPZL);  
            return tmpPZL;
        }   

        public List<UCell> _stringToBDL( string stOrg ){
            string st = stOrg.Replace(".", "0").Replace("-", "0").Replace(" ", "");
            try{               
                List<UCell> B = new List<UCell>();
                int rc=0;
                for(int k=0; rc<81; ){
                    if(st[k]=='+'){ k++; B.Add(new UCell(rc++,-( st[k++].ToInt())) ); }
                    else{
                        while(!st[k].IsNumeric()) k++;
                        B.Add( new UCell(rc++, st[k++].ToInt() ) );
                    }
                }
                return B;
            }
            catch(Exception e){
                WriteLine($"_stringToBDL \rstOrg:{stOrg} \r   st:{st}");
                WriteLine( $"string error:{e.Message}\r{e.StackTrace}");
            }
            return null;
        }
                


        public void SDK_FileOutput( string fName, bool append, bool fType81, bool SolSort, bool SolSet, bool SolSet2, bool blank9 ){
            if( pGPGC.GNPX_Puzzle_List.Count==0 )  return;

//            GNPX_App_Ctrl.MultiPuzzle = 1;
            G6.Puzzle_LevelLow = 0;
            G6.Puzzle_LevelHigh = 999;

            string line, solMessage="";
            GNPX_App_Man .SlvMtdCList[0] = true;  //use all methods

            int m=0;
            pGPGC.GNPX_Puzzle_List.ForEach( p=>p.ID=(m++) );
            IEnumerable<UPuzzle> qry;
            if(SolSort) qry = from p in pGPGC.GNPX_Puzzle_List orderby p.difficultyLevel ascending select p;
            else qry = from p in pGPGC.GNPX_Puzzle_List select p;

            using( StreamWriter fpW=new StreamWriter(fName,append,Encoding.UTF8) ){
                foreach( var P in qry ){

                    //===== Preparation =====
                    solMessage = "";
                    if(SolSet) solMessage = SetSolution(aPZL:P,SolSet2:SolSet2,SolAll:SolSet);	//output Solution
                            
                    if(fType81){　//Solution(tytpe:line)
                        line = ""; 
                        P.BOARD.ForEach( q => {
                            line += Max(q.No,0).ToString();
                            if( blank9 && q.c==8 )  line += " ";        //Supports the format "Contain a blank every 9 digits"
                        } );
                        line = line.Replace("0",".");

                        line += $" {P.difficultyLevel} \"{P.Name}\"";
                        if(SolSet&&SolSet2) line += $" \"{SetSolution(P,SolSet2:true,SolAll:true)}\"";//解出力
                        if(ePZL.TimeStamp!=null) line += $" \"{ePZL.TimeStamp}\"";
                        fpW.WriteLine(line);
                    }
                }
            }
            GNPX_App_Man .SlvMtdCList[0] = false;             //restore method selection
        }
     
		public void btn_FavoriteFileOutput( bool fType81=true, bool SolSet=false, bool SolSet2=false ){
            if( SolSet ){
                var tokSrc = new CancellationTokenSource(); //procedures for suspension
                GNPX_App_Man .SlvMtdCList[0] = true;             //use all methods
                GNPX_Eng.AnMan.Update_CellsState( GNPX_Eng.ePZL.BOARD );
				GNPX_Eng.GNPX_Solver_SolveUp(tokSrc.Token);
            }

            var (difLvl,_) = GNPX_Eng.Get_difficultyLevel();          
            string line = "";  
            if( fType81 ){
				var eLS = ePZL.BOARD.ConvertAll( q => Max(q.No,0) );
				line = string.Join("", eLS).Replace("0",".");

                line += $" {ePZL.difficultyLevel} \"{ePZL.Name}\"";
                if( SolSet&&SolSet2 ) line += $" \"{SetSolution(ePZL,SolSet2:true,SolAll:true)}\"";//解を出力
                if( ePZL.TimeStamp!=null ) line += $" \"{ePZL.TimeStamp}\"";
            }
            
            string fNameFav = "SDK_Favorite.txt";
			Utility_Display.GNPX_StreamWriter( fNameFav, line, append:true ); 

            GNPX_App_Man .SlvMtdCList[0] = false;//use selected methods
        }




        public void Save_CreatedPuzzle_ToFile( UPuzzle aPZL ){
            if( G6.Save_CreatedPuzzle is false ) return;

            string fName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
			string dirStr = "AutoGen_Puzzles";
			if( !Directory.Exists(dirStr) ){ Directory.CreateDirectory(dirStr); }
			using( var fpW=new StreamWriter( dirStr+@"\"+fName, append:true, Encoding.UTF8) ){   
				var P = aPZL;
				string LRecord = "";
				P.BOARD.ForEach( q =>{ LRecord += Max(q.No,0).ToString(); } );
				LRecord = LRecord.Replace("0",".");
				LRecord += $" {P.difficultyLevel} \"{P.Name}\" \"{P.TimeStamp}\"";
				fpW.WriteLine(LRecord);
			}
		}


		public void GNPX_RandomizeAllPuzzle(){
			var openFolder = new OpenFolderDialog();
			if( (bool)openFolder.ShowDialog() ){
				string Line;
				var folder = openFolder.FolderName;
				

				// When reading the string and randomizing it, if any error occurs or even one line cannot be converted,
				// it will conclude that "this is not a Sudoku Puzzle file", stop the conversion, and delete the file.
				var rnd = new Random(10);
				foreach( string fx in Directory.GetFiles(folder) ){

					// Files that have already been converted will not be converted again.
					if( fx.Contains("_Random.") ) continue;

					using( var fr=new StreamReader(fx) ){
						int n = fx.LastIndexOf(".");
						string fx2 = fx.Insert(n,"_Random");						
						
						using( var fw=new StreamWriter(fx2, append:false, Encoding.UTF8) ){
							try{
								while( (Line=fr.ReadLine()) !=null ){
									string stR = _stringToRString( Line, rnd );
									string stRLine = stR + Line.Substring(81);
									if( stRLine.Length < 81 )  goto Lerror;
									fw.WriteLine(stRLine);
								}
							}
							catch( Exception e ){  goto Lerror; }
						}
						continue;

					  Lerror:
						File.Delete( fx2 );
					}
				}
			}

						string _stringToRString( string st, Random rnd ){
							// Reads a string and randomizes it. If unsuccessful, returns an empty string.
							string stR = "";
							try{
									var randomCV = new List<int>(9);
									for( int k=0; k<9; k++ )  randomCV.Add( (int)(rnd.Next(100)*10)+k+1); 
									randomCV.Sort();
									for( int k=0; k<9; k++ ) randomCV[k] %= 10;
							
								int[]  cvL=new int[81]; 

								for(int k=0; k<81; k++ ){
									char c = st[k];
									if( !c.IsNumeric()) stR += ".";
									else stR += randomCV[ c.ToInt()-1 ];
								}
								return stR;
							}
							catch( Exception e ){ }
							return "";
						}
		}


        public void Save_CreatedPuzzle_ToFile( List<LatinSquare_9x9> aLS_List, bool append=true, string fName="" ){
            if( G6.Save_CreatedPuzzle is false ) return;

			string fn = fName;
			if( fName=="" ) fn =  DateTime.Now.ToString("yyyyMMdd_HHmm") + ".txt";
			string dirStr = "AutoGen_Puzzles";
			if( !Directory.Exists(dirStr) ){ Directory.CreateDirectory(dirStr); }


			using( var fpW=new StreamWriter( dirStr+@"\"+fn, append:append, Encoding.UTF8) ){   
				aLS_List.ForEach( P => {
					string LRecord = string.Join( "", P.SolX ).Replace("0",".");
					fpW.WriteLine(LRecord);
				} );
			}
		}


		public string SetSolution( UPuzzle aPZL, bool SolSet2, bool SolAll=false ){
            string solMessage="";
            ePZL = aPZL;

            if( SolAll || ePZL.difficultyLevel<=0 || ePZL.Name=="" ){
                foreach( var p in aPZL.BOARD )  if( p.No<0 ) p.No=0;

                GNPX_Eng.AnMan.Update_CellsState( aPZL.BOARD );
                GNPX_Eng.AnalyzerCounterReset();

                var tokSrc = new CancellationTokenSource();　        //for suspension
				GNPX_Eng.GNPX_Solver_SolveUp(tokSrc.Token); 
                if( GNPX_Engin.eng_retCode<0 ){
                    ePZL.difficultyLevel = -999;
                    ePZL.Name = "unsolvable";
                }
                else{
                    if(ePZL.difficultyLevel<=1 || ePZL.Name==""){
                        var (difficult,pzlName) = GNPX_Eng.Get_difficultyLevel( );
                        if(ePZL.difficultyLevel<=1)  ePZL.difficultyLevel = difficult;
                        if(ePZL.Name=="")     ePZL.Name = pzlName;
                    }
                }
            }     
            solMessage = "";
            if(SolSet2) solMessage += GNPX_Eng.DGView_MethodCounterToString();　//適用手法を付加
            solMessage=solMessage.Trim();

            return solMessage;
        }

    }
}