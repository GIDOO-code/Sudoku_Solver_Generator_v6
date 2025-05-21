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


		// ::: Searching all solutions to pattern :::
		private List<LatinSquare_9x9> GNPX_Creator_LS_Parallel_2( CancellationTokenSource cts, int LS_size ){
					//WriteLine( $"       --4 IE_Get_unique_LS_3" );


			// Set the selection order of 56*56 patterns of block12347 randomly.
			List<int>  kCode5656List = new();
			int _a_ = 1<<15; 
			for( int k=0; k<56*56; k++ ) kCode5656List.Add( GNPX_Random.Next(_a_)<<12 | k );
			kCode5656List.Sort();

			Stopwatch  SW = new();
			SW.Start();

			
			List<LatinSquare_9x9>[] XList = new List<LatinSquare_9x9>[LS_size];
			int nc=0, validCC=0;

			G6.Info_CreateMessage0 = "* Latin Square Preparation";
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
					G6.LS_P_P2 = 0;
					ParallelOptions options = new(){ CancellationToken=cts.Token, MaxDegreeOfParallelism=Environment.ProcessorCount	};
					try{
						Parallel.For( 0, LS_size, options, (k56) => {
							XList[k56] = Create_LatinSquareList_3_List( k56, ApplyPattern:true );

							if( ++nc>5 ){
								lock(obj){
									TimeSpan ts = SW.Elapsed;
									G6.Info_CreateMessage1 = $"stage process : 1 / 6 ... Create LS";
									G6.Info_CreateMessage2 = $"Lattin Square Parallel Search";
									G6.Info_CreateMessage3 = $"Created : {nc+1} / {LS_size} ... {ts.RemainingTimeToString(nc,LS_size)}";
								}
							}
						} );
					}
					catch( OperationCanceledException e ){ }
					finally{ /*cts.Dispose();*/ }


/*
					Parallel.For( 0, LS_size, (k56) => {
						XList[k56] = Create_LatinSquareList_3_List( k56, ApplyPattern:true );

						if( ++nc>5 ){
							lock(obj){
								TimeSpan ts = SW.Elapsed;
								G6.Info_CreateMessage1 = $"stage process : 1 / 6";
								G6.Info_CreateMessage2 = $"Lattin Square Parallel Search";
								G6.Info_CreateMessage3 = $"Created : {nc+1} / {LS_size} ... {ts.RemainingTimeToString(nc,LS_size)}";
							}
						}
					} );
*/

					G6.LS_P_P2 = LS_size;
					// --- @@@@@ parallel @@@@@ ---

			// --- Composite ---
			List<LatinSquare_9x9> aLS_ListM = new();
					G6.Info_CreateMessage1 = $"stage process : 2 / 6 ... Composite";
					G6.Info_CreateMessage2 = $"Merge LS ";
			foreach( var X in XList ) aLS_ListM.AddRange(X);

			// --- Sort ---
					G6.Info_CreateMessage1 = $"stage process : 3 / 6 ... Sort";
					G6.Info_CreateMessage2 = $"Sort LS : {aLS_ListM.Count}";
			aLS_ListM.Sort();

			// --- Unique solution selection by hash value ---
					G6.Info_CreateMessage1 = $"stage process : 4 / 6 ... Select Unique LS";
					G6.Info_CreateMessage2 = $"Uniqueness Check : {aLS_ListM.Count}";
			long key=0;
			LatinSquare_9x9 R=null;
			aLS_ListM.ForEach( Q=> {
				if( Q.hashVal==key ){ Q.validF=false; if(R!=null) R.validF=false; }
				else{ key=Q.hashVal; R=Q; }
			} );
			
					G6.Info_CreateMessage1 = $"stage process : 5 / 6 ... Select valid LS";
					G6.Info_CreateMessage2 = $"Select LS : {aLS_ListM.Count}";
			List<LatinSquare_9x9>  aLS_List = aLS_ListM.FindAll( Q => Q.validF );


			// --- Unique solution selection by TE ---
			int LS_Count = aLS_List.Count;
			if( LS_Count > 0 ){
					SW.Restart();
				nc=0;
				// <<< @@@@@ parallel @@@@@ >>>
				try{
					Parallel.ForEach( aLS_List, options, Q => {
						Research_trial RTrial = new( AnMan );
						bool ret = RTrial.TrialAndErrorApp( Q.SolX.ToList(), filePutB:false, upperLimit:2 );	
						Q.validF = ret;
						if( ret )  validCC++;

						if( ++nc>50 && (nc%100)==0  ){
							lock(obj){
								TimeSpan ts = SW.Elapsed;
								G6.Info_CreateMessage1 = $"stage process : 6 / 6 ... Select by TE";
								G6.Info_CreateMessage2 = $"Lap Time : {ts.TimespanToString()}";
								G6.Info_CreateMessage3 = $"Valid : {validCC} / {nc} / {LS_Count}";
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
			TimeSpan ts = SW.Elapsed;
			G6.Info_CreateMessage2 = $"Lap Time : {ts.TimespanToString()}";
			G6.Info_CreateMessage3 = $"Valid : {validCC} / {nc} / {LS_Count}";
		
			return aLS_List;
		}

	}
}
