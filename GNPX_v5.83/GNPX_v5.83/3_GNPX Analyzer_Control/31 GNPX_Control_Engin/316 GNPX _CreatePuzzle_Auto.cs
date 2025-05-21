using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
using static System.Diagnostics.Debug;

using System.Threading.Tasks;
using System.Threading;

using Windows.Devices.Power;
using System.Collections;
using System.Diagnostics;

using GIDOO_space;
using static GNPX_space.GNPX_App_Ctrl;
using System.Diagnostics.Metrics;


namespace GNPX_space{
	using pGPGC = GNPX_Puzzle_Global_Control; 

    public partial class GNPX_App_Ctrl{

		static private G6_Base	G6 => GNPX_App_Man.G6;
        static public int		rxCTRL=-1;      //=0:
        static public int		solLevel_notFixedCells=0;
        static public string	MethodName;
		static public string	infoCount;
		
        public LatinSquareGen LSGen;        
		private bool			_DEBUGmode_= false; //false; //true;// 

		private object obj = new();
	
    // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*
    //   Latin Square Management
	//	 (uder consideration / development) 
    // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*

		private Research_trial RTrial;
		private Func_CreateAuto FCreateAuto;

        private List<LatinSquare_9x9> LatSqrList=null;

		// :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: 1
        public void task_GNPX_PuzzleCreaterAuto_1( Func_CreateAuto _fCreateAuto, CancellationTokenSource cts ){ //Creating Puzzles[Automatic]
						//WriteLine( $" --1 task_GNPX_PuzzleCreaterAuto_1" );

			if( FCreateAuto == null ){
				this.FCreateAuto = _fCreateAuto;
				Send_Command_to_FCreateAuto += new GNPX_EventHandler( FCreateAuto.GNPX_Event_Handling_man );  
			}
			

            AnalyzerBaseV2.__SimpleAnalyzerB__ = true;
            G6.SolutionCounter = 0;

            try{
				G6.LS_P_P0 = 0;

                do{
				  // <0> Prepare pool ====================
					// [ver.7-] Generate Latin-Square candidates in parallel threads.
                  
					G6.LoopCC++; G6.TLoopCC++;

				  // <1> Create Puzzle ====================
					// <<< Create one by one >>>	
					foreach( var tBOARD in IE_Get_GNPX_BOARD_2( G6.GenLStyp, cts ) ){ // Get List<UCell>
						if( cts.Token.IsCancellationRequested ){ return; }

						UPuzzle qPZL = new UPuzzle( tBOARD );               // Create new UPuzzle
						pGNPX_Eng.Set_NewPuzzle( qPZL );                    // Set Puzzle to Engin



						// <2> Solve Puzzle =======================
							pGNPX_Eng.AnalyzerCounterReset();			// (Not necessary)       

							string stRet = pGNPX_Eng.GNPX_Solver_SolveUp(cts.Token);	// Apply algorithms
							if( cts.Token.IsCancellationRequested ){ /*WriteLine( $"@@@ task_GNPX_PuzzleCreaterAuto_1");*/ return; }
							// ---------------------------------------------------    


									//  <3> Solved ======================= 
								if( stRet == "SolveUp" ){
										var PZL000 = pGNPX_Eng.PZL_Initial;

										// <<< The difficulty of the puzzle is acceptable? >>>
										int diffLvl = PZL000.difficultyLevel;
										if( diffLvl<G6.Puzzle_LevelLow || G6.Puzzle_LevelHigh<diffLvl ) 	continue;

										// randomize
										if( G6.Digits_Randomize ){
											List<int> rLS = _Randomize_PuzzleDigits( qPZL );
											int rc = 0;
											qPZL.BOARD = rLS.ConvertAll( q => new UCell(rc++,q) );
										}
							
										// <<< Save Puzzle >>>
										qPZL.pMethod = PZL000.pMethod;
										qPZL.difficultyLevel = diffLvl;
										qPZL.Name = qPZL.pMethod.MethodName;
										qPZL.TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
										qPZL.solMessage = pGNPX_Eng.DGView_MethodCounterToString();
										qPZL.BOARD.ForEach( p=>p.Reset_All() );
										pGPGC.GNPX_Puzzle_List_Add( qPZL );				
								
										if( G6.Save_CreatedPuzzle ) pGNPX_App.Save_CreatedPuzzle_ToFile(qPZL);

							
										G6.SolutionCounter++;
										Gidoo_EventHandler se = new Gidoo_EventHandler( eName:"PuzzleCreated", ePara0:G6.NoOfPuzzlesToCreate, G6.SolutionCounter );
										Send_Command_to_FCreateAuto( this, se );
								
										// ================================================
										if( !G6.Search_AllSolutions && G6.SolutionCounter>=G6.NoOfPuzzlesToCreate ) return; // The required No. of solutions has been found			
										if( G6.CbxNextLSpattern ) rxCTRL=0;      //Change LS pattern at next puzzle generation

										// ================================================ 
										if( ++G6.LS_P_P0 >= G6.LS_Pattern_Cntrl_P0 ){ G6.LS_PatternChange=true; break; }
								}		
							
					}

					// ================================================ @@@ Pattern Change
					if( G6.LS_PatternChange ){
						G6.LS_PatternChange = false;
						G6.LS_P_P0 = 0;
						PatGen.patternAutoMaker( G6.PatSel0X );		// <<< Change Pattern >>>				
						Gidoo_EventHandler se2 = new Gidoo_EventHandler( eName:"ChangedPattern" );  //pattern change command
						Send_Command_to_FCreateAuto( this, se2 );
					}

                }while(true); 
            }
            catch(TaskCanceledException){ WriteLine("...Canceled by user."); }
            catch(Exception ex){ WriteLine(ex.Message+"\r"+ex.StackTrace); }     


							List<int> _Randomize_PuzzleDigits( UPuzzle qPZL ){
								List<int> ranNum = new List<int>();
								for(int no=0; no<9; no++)  ranNum.Add( GNPX_Random.Next(0,9)*10+no );
								ranNum.Sort((x,y) => (x-y));
								for(int no=0; no<9; no++) ranNum[no] %= 10;

								int[] P = new int[81];
								for(int rc=0; rc<81; rc++){
									int no = qPZL.BOARD[rc].No;
									if( no>0 ) P[rc] = ranNum[no-1]+1;
								}
								return P.ToList();
							} 

        }



		// :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: 2
        public IEnumerable< List<UCell> > IE_Get_GNPX_BOARD_2( int GenLSTyp, CancellationTokenSource cts  ){  //for Parallel

			// --- Parallel ---
			List<LatinSquare_9x9> aLS_List = GNPX_Creator_LS_Parallel_2( cts, G6.LS_Pattern_Cntrl_P2 );
			G6.Info_CreateMessage0 = "* Solve with GNPX_Algorithm";

			foreach( var LSQ in aLS_List ){
				if( cts.Token.IsCancellationRequested ) yield break;

					// Convert the unique_LS to BOARD(List<Ucell>)
					int[] SolX = LSQ.SolX;
					List<UCell> BDLa = new List<UCell>();
					for( int rc=0; rc<81; rc++ )  BDLa.Add( new UCell(rc,SolX[rc]) );
					if( _DEBUGmode_ )   Utility_Display.__DBUG_Print2( BDLa, sqF:true, "          " );
					yield return BDLa;
			}
			G6.LS_PatternChange = true;	// ##### Change Pattern #####
			
			yield break;
        }

	                          
      #region Latin Squares ID code generation for Standadization
        public string Get_SDKNumPattern(int[] TrPara, int[] AnsNum){
        //Standadization(Number)
            int[] ChgNum=new int[10];
            for(int k=0; k<9; k++){
                int n=Abs(AnsNum[(k/3*9)+(k%3)]);
                ChgNum[n]=k+1;
            }
            int[,] AnsN2=new int[9,9];
            for(int rc=0; rc<81; rc++){
                int n=Abs(AnsNum[rc]);
                AnsN2[rc/9,rc%9]=ChgNum[n];
            }
#if DEBUG
					//  Utility_Display.__DBUG_Print2(AnsNum,true,"Before");
                    //  Utility_Display.__DBUG_Print2(AnsN2,"After");
#endif

        //Block 2347
            int[] PTop=new int[8];
            int[] PLft=new int[8];
            
            for(int s=0; s<8; s++) PTop[s]=-1;
            LSGen._LatinSquareSub_01R( AnsN2, PTop);

            for(int s=0; s<8; s++) PLft[s]=-1;
            LSGen._LatinSquareSub_11R( AnsN2, PLft);
/*
                      string st2="PTop";
                      Array.ForEach( PTop, P=>st2+=" "+P);
                      WriteLine(st2);
                      st2="PLft";
                      Array.ForEach(PLft,P=>st2+=" "+P);
                      WriteLine(st2);
*/
        //Block 5689
            int ID = LSGen.GetLatSqrID(AnsN2);

        //ID
            int N=PTop[0]*10+PTop[1];
            for(int n=2; n<8; n++) N=(N*10)+PTop[n];
            TrPara[12]=N;   //14:Block 23

            N=PLft[0]*10+PLft[1];
            for(int n=2; n<8; n++) N=(N*10)+PLft[n];
            TrPara[13]=N;   //15:Block 47

            TrPara[14]=ID;  //16:Block 5689

            N=0;
            for(int n=0; n<10; n++) N=(N*10)+ChgNum[n];           
            TrPara[15]=N;   //13:Exchange
/*
                      st2="ID";
                      Array.ForEach(TrPara,P=>st2+=" "+P);
                      WriteLine(st2);
*/
        //SuDoKu Standadization Code
            string st="===== Standadization Code=====";
            st +="\rPattern Code:\r";
            for(int k=9; k<12; k++) st+=" "+TrPara[k];

            st+="\r\rLatin Square Code\r";
            for(int k=12; k<15; k++) st+=" "+TrPara[k];

            st+="\r\r===== Transformation Parameter=====\rPattern:";
            st+=" "; for(int k=0; k<4; k++) st+=TrPara[k];
            st+=" "; for(int k=4; k<8; k++) st+=TrPara[k];
            st+=" "+TrPara[8];

            st+="\rNumber:";
            st+=TrPara[16].ToString()+" -> 123456789";

            return st;
        }
      #endregion
    }
}
