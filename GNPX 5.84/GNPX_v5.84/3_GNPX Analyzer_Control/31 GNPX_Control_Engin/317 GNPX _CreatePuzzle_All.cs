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
using System.Windows.Media;


namespace GNPX_space{
	using pGPGC = GNPX_Puzzle_Global_Control;
    using sysWin=System.Windows;

    public partial class GNPX_App_Ctrl{
			private Func_CreateAll FCreateAll;

			// :::::::::::::::::::::::::::::::::::::::::
			//
			//   Under consideration / development
			//
			// :::::::::::::::::::::::::::::::::::::::::

		public void task_GNPX_Creator_AllPuzzles_1( Func_CreateAll _fCreateAll, CancellationTokenSource cts, double partialSearch ){ //Creating Puzzles[Automatic]
						//WriteLine( $" --1 task_GNPX_Creator_AllPuzzles_1" );
			Gidoo_EventHandler se;

			if( FCreateAuto == null ){
				this.FCreateAll = _fCreateAll;
				Send_Command_to_FCreateAuto += new GNPX_EventHandler( FCreateAll.GNPX_Event_Handling_man );  
			}
			
            AnalyzerBaseV2.__SimpleAnalyzerB__ = true;
			Gidoo_EventHandler se2;

			// <<< Create All Puzzle ... Parallel >>>
			List<LatinSquare_9x9>  aLS_List = task_GNPX_Creator_AllPuzzles_2( cts, partialSearch );	//@@@@@@@@@@
			int SolutionCounter = aLS_List.Count;

			// <<< Up to pGPGC.GNPX_Puzzle_List >>>
			int aLS_List_Count = aLS_List.Count;

			G6.LS_P_P2 = aLS_List_Count;


			if( aLS_List==null || aLS_List.Count<=0 ){
				se = new Gidoo_EventHandler( eName:"PuzzleCreated", ePara0:0, 0 );
				Send_Command_to_FCreateAuto( this, se );
				return;
			}

			// <<< Save to GNPX internal puzzle list >>>
			int pzlCC_Max = Min( aLS_List_Count, 10000 );

			foreach( var (LSQ,kx) in aLS_List.WithIndex() ){
				int kxP = kx+1;
				if( kxP > pzlCC_Max )  break;

				List<UCell> tBOARD = new List<UCell>();
				for(int rc=0; rc<81; rc++ )  tBOARD.Add( new UCell(rc,LSQ.SolX[rc]) );
				UPuzzle qPZL = new UPuzzle( tBOARD );               // new UPuzzle
				pGPGC.GNPX_Puzzle_List_Add( qPZL );

				if( kxP>0 && kxP%1000==0 ){
					G6.Info_LS_Generator = $"Found : {kxP} / {aLS_List_Count}";
					
					se = new Gidoo_EventHandler( eName:"PuzzleCreated", ePara0:G6.NoOfPuzzlesToCreate, aLS_List_Count );
					Send_Command_to_FCreateAuto( this, se );
				}
			}
			se = new Gidoo_EventHandler( eName:"PuzzleCreated", ePara0:G6.NoOfPuzzlesToCreate, aLS_List_Count );
			Send_Command_to_FCreateAuto( this, se );


			// <<< Save to File All Puzzles >>>
			G6.Info_LS_Generator = $"Save File : {aLS_List_Count} Puzzles";

			string fName = String.Join("", aLS_List[0].SolX.ToList().GetRange(0,18) );
			pGNPX_App.Save_CreatedPuzzle_ToFile( aLS_List, append:false, fName:fName );	

		}


		// :::::::::::::::::::::::::::::::: Searching all solutions to pattern :::
		private List<LatinSquare_9x9> task_GNPX_Creator_AllPuzzles_2( CancellationTokenSource cts, double partialSearch ){
					//WriteLine( $"       --4 IE_Get_unique_LS_3" );

			int LS_LoopMax = (int)(3136 * partialSearch);	// ... Exhaustive search

			Stopwatch  SW = new();
			SW.Start();

			
			List<LatinSquare_9x9>[] XList = new List<LatinSquare_9x9>[LS_LoopMax];
			int nc=0, validCC=0;

							/*	// <<< not parallel ... for debug >>>
								for( int k56=0; k56<LS_LoopMax; k56++ ){		
									XList[k56] = Create_LatinSquareList_4_List( k56, ApplyPattern:true );	

									if( ++nc>5 ){
										TimeSpan ts = SW.Elapsed;
										G6.Info_LS_Generator = $"Create LS : {nc+1} / {LS_LoopMax} ... {ts.RemainingTimeToString(nc,LS_LoopMax)}";
									}
								}
							*/

					// <<< @@@@@ parallel @@@@@ >>>
					ParallelOptions options = new(){ CancellationToken=cts.Token, MaxDegreeOfParallelism=Environment.ProcessorCount	};
					try{
						Parallel.For( 0, LS_LoopMax, options, (k56) => {
							XList[k56] = Create_LatinSquareList_3_List( k56, ApplyPattern:true );

							if( ++nc>5 ){
								lock(obj){
									TimeSpan ts = SW.Elapsed;
									G6.Info_CreateMessage1 = $"stage process : 1 / 6";
									G6.Info_CreateMessage2 = $"Lattin Square Parallel Search";
									G6.Info_CreateMessage3 = $"Created : {nc+1} / {LS_LoopMax} ... {ts.RemainingTimeToString(nc,LS_LoopMax)}";
								}
							}
						} );
					}
					catch( OperationCanceledException e ){ }
					finally{ /*cts.Dispose();*/ }
					// --- @@@@@ parallel @@@@@ ---




			List<LatinSquare_9x9> aLS_ListM = new();
			G6.Info_CreateMessage1 = $"stage process : 2 / 6";
			G6.Info_CreateMessage2 = $"Merge LS ";
			foreach( var X in XList ) aLS_ListM.AddRange(X);

			G6.Info_CreateMessage1 = $"stage process : 3 / 6";
			G6.Info_CreateMessage2 = $"Sort LS : {aLS_ListM.Count}";
			aLS_ListM.Sort();

			//aLS_ListM = aLS_ListM.GetRange(0,1000);	//debug
			G6.Info_CreateMessage1 = $"stage process : 4 / 6";
			G6.Info_CreateMessage2 = $"Uniqueness Check : {aLS_ListM.Count}";
			long key=0;
			LatinSquare_9x9 R=null;
			aLS_ListM.ForEach( Q=> {
				if( Q.hashVal==key ){ Q.validF=false; if(R!=null) R.validF=false; }
				else{ key=Q.hashVal; R=Q; }
			} );
			
			G6.Info_CreateMessage1 = $"stage process : 5 / 6";
			G6.Info_CreateMessage2 = $"Select LS : {aLS_ListM.Count}";
			List<LatinSquare_9x9>  aLS_List = aLS_ListM.FindAll( Q => Q.validF );

			LS_LoopMax = aLS_List.Count;
			if( LS_LoopMax > 0 ){
				SW.Restart();
				nc=0;
		
					// <<< @@@@@ parallel @@@@@ >>>
					try{
						Parallel.ForEach( aLS_List, options, Q => {
							Research_trial RTrial = new( AnMan );
							bool ret = RTrial.TrialAndErrorApp( Q.SolX.ToList(), filePutB:false, upperLimit:2 );	
							Q.validF = ret;
							if( ret )  validCC++;

							if( ++nc>500 && (nc%2000)==0  ){
								lock(obj){
									TimeSpan ts = SW.Elapsed;
									G6.Info_CreateMessage1 = $"stage process : 6 / 6";
									G6.Info_CreateMessage2 = $"Lap Time + Remain: {ts.TimespanToString()} + {ts.RemainingTimeToString(nc,LS_LoopMax)}";
									G6.Info_CreateMessage3 = $"Valid : {validCC} / {nc} / {LS_LoopMax}";
								}
							}
						} );
					}
					catch( OperationCanceledException e ){ }
					finally{ /*cts.Dispose();*/ }
					// --- @@@@@ parallel @@@@@ ---
			}

			aLS_List = aLS_List.FindAll( Q => Q.validF );
			aLS_List.Sort( (a,b) => a.CompareTo2(b) );	// Sorting by actual pattern
			
			return aLS_List;
		}



		public List<LatinSquare_9x9> Create_LatinSquareList_3_List( int k5656, bool ApplyPattern ){	//@@@@@@@@@@

			List<uint> unique=new List<uint>();
			Permutation[] prmLstA=new Permutation[9];
			int[] URow=new int[9];
			int[] UCol=new int[9];

			List<LatinSquare_9x9> LS4List = new();

			int[,] Sol99 = LSGen.GeneratePara( k5656&0xFFFF );

			for(int r=0; r<3; r++){
				for(int c=3; c<9; c++){
					UCol[c] |= (1<<Sol99[r,c]);
					URow[c] |= (1<<Sol99[c,r]); //r,c
				}
			}
			for(int r=0; r<9; r++){
				for(int c=0; c<9; c++){
					if(r<3 || c<3) G6.Sol99sta[r,c] = Sol99[r,c];
				}
			}
			

			int RX=3; prmLstA[RX] = null;
			do{
			  
			LNxtLevel:
				Permutation prm=prmLstA[RX];
				if(prm==null) prmLstA[RX]=prm=new Permutation(9,6);
                
				int[] UCo2 = new int[9];
				int[] UBlk = new int[9];
				for(int c=3; c<9; c++) UCo2[c]=UCol[c];
				for(int r=3; r<RX; r++){                              // Mark used numbers
					for(int c=3; c<9; c++){
						int no=Sol99[r,c];
						UCo2[c] |= (1<<no);
						UBlk[r/3*3+c/3] |= (1<<no);
					}
				}

				int nxtX=9;
				while( prm.Successor(nxtX) ){                         //Fill blocks 5,6,8,9 to generate latinSol99(latin square)
					for(int cx=3; cx<9; cx++){
						nxtX=cx-3;
						int no=prm.Index[nxtX]+1;
						int noB = 1<<no;
						if( (UCo2[cx]&noB)>0 ) goto LNxtPrm;          //Eliminate used numbers in columns
						if( (URow[RX]&noB)>0 ) goto LNxtPrm;          //Eliminate used numbers in rows
						if( (UBlk[RX/3*3+cx/3]&noB)>0 ) goto LNxtPrm; //Eliminate used numbers in blocks
						Sol99[RX,cx] = no;
					}
					if( RX<8 ){
						prmLstA[++RX]=null;
						goto LNxtLevel;			//  ---> next Row setting
					}
					else{       
						//LatinSquare_9x9 LS99 = new( Sol99, ApplyPatternB:true, GPat81:PatGen.GPat81 );
						LatinSquare_9x9 LS99 = new( Sol99, ApplyPatternB:true, GPat81:PatGen.GPat81, debugB:false );

						// <<< List.Add >>>
						LS4List.Add(  LS99 );
					}

					LNxtPrm:
					continue;
				}
			}while( (--RX) >= 3 );

			LS4List.Sort();

			return LS4List;
		}
	}
}
