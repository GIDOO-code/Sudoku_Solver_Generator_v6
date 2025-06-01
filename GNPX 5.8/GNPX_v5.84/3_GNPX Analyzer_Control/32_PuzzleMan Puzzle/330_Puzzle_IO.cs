using System;
using System.Collections.Generic;
using static System.Math;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Linq;
using System.Windows.Documents;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace GNPX_space{
   using pRes=Properties.Resources;
	using pGPGC = GNPX_Puzzle_Global_Control;


    public class PuzzleFile_IO{
		static private G6_Base		G6 => GNPX_App_Man.G6;
        private GNPX_Engin			pGNPX_Eng;                      //Analysis Engine

		private UPuzzle  ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; } // Puzzle to analyze

		public PuzzleFile_IO( GNPX_Engin pNPX_Eng ){
			pGNPX_Eng = pNPX_Eng;
		}

		public PuzzleFile_IO( GNPX_App_Man  pNPX00 ){
			pGNPX_Eng = pNPX00.GNPX_Eng;
		}

		// ===== Read_Puzzle ============================================================
        public List<UPuzzle> GNPX_PuzzleFile_Read( string fName ){
            char[] sep=new Char[]{ ' ', ',', '\t' };        
            string line, puzzleName="";

			List<UPuzzle> puzzleList = new();
            using( StreamReader fRead=new StreamReader(fName) ){

				int ID=0;
                while( (line=fRead.ReadLine()) !=null ){
                    if( line=="" ) continue;
					if( line[0]=='-' )  continue;
					if( line[0]=='/' )  continue;
					//if( line[0]=='\t' )  continue;
                    
                    // Supports the format "Contain a blank every 9 digits" and similar type.
                    int n81 = _Find_81Digits(line);
                    if( n81 > 81 ){
                        string st = line.Substring(0,n81).Replace(" ","").Replace(".","0");
                        if( st.Length==81 && st.All(char.IsDigit) ){ line = string.Concat(st, line.AsSpan(n81)); }
                    }

                    string[] eLst = line.SplitEx(sep);
                    if( line[0]=='#' || line[0]=='@' || line[0]=='!' ) continue;		// comment line
					if( eLst.Length==0 )  continue;

                    int    nc = eLst.Length;
                    string name="";
                    int    difLvl=99;
                    if( eLst[0].Length>=81 ){
                        try{
							if( nc>=2 && eLst[1].IsNumeric() ) difLvl = eLst[1].ToInt();
							if( nc>=3 ){					   name   = eLst[2]; }  
                            string st = eLst[0].Replace(".", "0").Replace("-", "0").Replace(" ", "");
                            List<UCell> _board = _stringToBDL(st);
                            string      _timeStamp =  _Find_TimeStamp( eLst );
							UPuzzle     _puzzle = new UPuzzle( ID++, _board, name, difficultyLevel:difLvl, _timeStamp);
                            puzzleList.Add( _puzzle );  
                        }
                        catch{ continue; }                   
                    }
                }
			}
			return puzzleList;

		#region  Inner function(GNPX_PuzzleFile_Read)
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

			List<UCell> _stringToBDL( string stOrg ){
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
		#endregion  Inner function(GNPX_PuzzleFile_Read)
        }

		// ===== Write_Puzzle ============================================================
        public void GNPX_PuzzleFile_Write( string fName, bool append, bool fType81, bool SolSort, bool SolSet, bool SolSet2, bool blank9 ){
            if( pGPGC.GNPX_Puzzle_List.Count==0 )  return;

            G6.NoOfPuzzlesToCreate = 1;
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

		public string SetSolution( UPuzzle aPZL, bool SolSet2, bool SolAll=false ){
            string solMessage="";
            ePZL = aPZL;

            if( SolAll || ePZL.difficultyLevel<=0 || ePZL.Name=="" ){
                foreach( var p in aPZL.BOARD )  if( p.No<0 ) p.No=0;

                pGNPX_Eng.AnMan.Update_CellsState( aPZL.BOARD );
                pGNPX_Eng.AnalyzerCounterReset();

                var tokSrc = new CancellationTokenSource();　        //for suspension
				pGNPX_Eng.GNPX_Solver_SolveUp(tokSrc.Token); 
                if( GNPX_Engin.eng_retCode<0 ){
                    ePZL.difficultyLevel = -999;
                    ePZL.Name = "unsolvable";
                }
                else{
                    if(ePZL.difficultyLevel<=1 || ePZL.Name==""){
                        var (difficult,pzlName) = pGNPX_Eng.Get_difficultyLevel( );
                        if(ePZL.difficultyLevel<=1)  ePZL.difficultyLevel = difficult;
                        if(ePZL.Name=="")     ePZL.Name = pzlName;
                    }
                }
            }     
            solMessage = "";
            if(SolSet2) solMessage += pGNPX_Eng.DGView_MethodCounterToString();　//適用手法を付加
            solMessage=solMessage.Trim();

            return solMessage;
        }

	}
}