using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using static System.Math;
using System.IO;

using GIDOO_space;
using static System.Net.Mime.MediaTypeNames;


namespace GNPX_space{

// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
//    under development (GNPXv5.1)
// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

	//GNPX sample 23
	//.1..7.69.4.6.9..1.5.9.2...87....9....9..3..8....8....41...6.8.9.8..4.7.5.67.8..4.
	//81..7.69.4.639851.5.9.2.4.87.8..9....9..3..8....8..9.41...6.8.9.8..4.765.67.8..4.  step5
	//813574692426398517579126438748619253295437186631852974154763829382941765967285341 ... elapsed time:5ms


	//756...3898235..641491386572587...2643648..197912...835639241758145738926278...413
	//756124389823579641491386572587913264364852197912467835639241758145738926278695413 ... elapsed time:1ms


    public partial class SubsetTechGen: AnalyzerBaseV2{


		public bool SubsetExclusion( ) => SubsetExclusionSub( enableALS:false );
		public bool SubsetExclusion_ALS( ) => SubsetExclusionSub( enableALS:true );

        public bool SubsetExclusionSub( bool enableALS ){
			try{
				if( stageNoP != stageNoPMemo ){
					stageNoPMemo = stageNoP;
					base.AnalyzerBaseV2_PrepareStage();
					
					// ===== Prepare =====
					fALS.Initialize();
					Prepare_SubsetTec();
						//ALSList_Leaf.ForEach( p=> WriteLine( p ));
				}
				debugPrint = false;	//######
				pBOARD[0].Set_CellColorBkgColor_noBit(0x1ff,AttCr,AttCr2 );

				if( ALSList_Leaf==null || ALSList_Leaf.Count<3 ) return false;
						//if(debugPrint)  Debug_StreamWriter( $"\n{DateTime.Now}\n{TandE_st_Puzzle}\n{TandE_st_sol_int81}" );	// @@@					


				// ===== Analize =====
				SubsetExcludeMan  SubsetMan = new( pAnMan );
				for( int StemSize=2; StemSize<=3; StemSize++ ){ // 2:Pair 3:Triple 	

					// Choose Stem and Leafs as a set. Analyze the selected Stem and Leafs.					
					foreach( var (Stem,Leafs) in IEGet_Stem_Leaf( enableALS, StemSize:StemSize, debugPrint:debugPrint ) ){
							if(debugPrint) WriteLine( $" **Stem:{Stem.ToStringRC()} \n LeafsCandidate:\n  {string.Join("\n  ",Leafs)}" );

						// Generate candidate list for Stem. 
						SubsetMan.GenerateList_noBLis( Stem, debugPrint:debugPrint);

						// Apply Leafs to candidate list(noBList). Exclude ALS with the same RCC (Leafs->Leafs2).
						var (appliedB,Leafs2) = SubsetMan.Apply_Link( Stem, Leafs, debugPrint:debugPrint);
						if( !appliedB )  continue;
						
						// Evaluate the applied list and reflect the positive/negative digits in the analysis results.
						var solFound = SubsetMan.Check_ConfirmedDigits( debugPrint:debugPrint );

						if( solFound ){
							SolCode=2;	
							SubsetExclusion_SolResult( Stem, Leafs2 );
						
							if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
						}
					}
				}

			}
			catch(Exception e){ WriteLine( $"{e.Message}\n{e.StackTrace}" ); }

			return false;
		}

		private IEnumerable<(USubset,List<UAnLS>)> IEGet_Stem_Leaf( bool enableALS, int StemSize, bool debugPrint=false ){
			// ===== Prepare =====
			// StemCandidate
			UInt128   StemCandidate = pBOARD.Create_Free_BitExp128();  //B0
			List<int> StemCandList  = StemCandidate.IEGet_rc().ToList();
				if(debugPrint){
					WriteLine( $"StemCandidate:{StemCandidate.ToBitString81N()}" );
					StemCandList.ForEach( rc=> WriteLine( $"{rc:00} {pBOARD[rc]}") );
				}

			// LeafsCandidate
			List<UAnLS> LeafsCandidate = fALS.ALSList.FindAll( p=> (p.Size==1 && p.UCellLst[0].FreeBC<=StemSize) ); //
			if( enableALS ){
				var LeafsCandidate_ALS =fALS.ALSList.FindAll( p=> (p.Size>=2 && p.Level==1) );
				LeafsCandidate.AddRange( LeafsCandidate_ALS );
					//LeafsCandidate_ALS.ForEach( LF=> WriteLine( $"{LF}") );
			}
				if(debugPrint) LeafsCandidate.ForEach( P=> WriteLine( $"{P}") );
				//LeafsCandidate.Sort( (a,b) => a.ID-b.ID ); 

			// ===================================
			List<USubset> StemLst = new();
			Combination cmb = new( StemCandidate.BitCount(), StemSize );

			while( cmb.Successor() ){
				
				UInt128		stem81B = cmb.Index.Aggregate( UInt128.Zero, (p,q) => p| UInt128.One<<StemCandList[q] );
				List<UCell> Stem_UCLST = (List<UCell>) stem81B.IEGet_UCell(pBOARD).ToList();
				USubset		Stem = new( 0, 0, Stem_UCLST );
				UInt128		stem81B_Connected = Stem_UCLST.Aggregate( UInt128.Zero, (p,q) => p| ConnectedCells81[q.rc] );
	
				List<UAnLS> Leafs = new();
				foreach( var LC in LeafsCandidate.Where(p => p.Size==1) ){
					if( LC.FreeBC > StemSize )     continue;
					if( LC.FreeB.DifSet(Stem.FreeB) > 0 )	 continue;
					if( stem81B.DifSet(LC.connectedB81) == qZero )  Leafs.Add(LC);
				}

				if( enableALS ){
					foreach( var LC in LeafsCandidate.Where(p => p.Size>=2) ){
						if( (stem81B & LC.bitExp) > qZero )  continue;		// Exclude elements that overlap with STem
						if( LC.bitExp.DifSet(stem81B_Connected) == qZero ) 	Leafs.Add(LC);
					}
				}
				if( Leafs.Count < 1 )  continue;
				yield return (Stem,Leafs);
			}

			
			yield break;

		}

		private bool SubsetExclusion_SolResult( USubset Stem, List<UAnLS> LeafsCandidate ){
			bool solFound = false;
				//LeafsCandidate.ForEach( P=> WriteLine( $"  {P}") );
			
			// ------------------------
			Stem.bitExp.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr);
			string st_Stem  = $" {Stem.bitExp.ToRCStringComp(Stem.FreeB)}";
			string st_Leaf = "", st_LeafS="";
			// ------------------------

			// size=1 leaf
			List<UAnLS> Leaf_size1 = LeafsCandidate.FindAll(p => p.Size==1);
			UInt128 Leaf_size1_81B = Leaf_size1.Aggregate(UInt128.Zero, (p,q) => p | (UInt128.One<<q.UCellLst[0].rc) );
			Leaf_size1_81B.IE_SetNoBBgColor(pBOARD, 0x1FF, AttCr, SolBkCr2);
			int nx = 0;
			foreach( var P in Leaf_size1.Select(p=>p.UCellLst[0]) ){ 
				if( (nx++) >= 1 )  st_Leaf += "\n       ";
				st_Leaf += $" Cell {P.rc.ToRCString()} #{P.FreeB.ToBitStringNZ(9)}";
				st_LeafS += $" {P.rc.ToRCString()} #{P.FreeB.ToBitStringNZ(9)}";
			};

			// ------------------------
			// size>=2(ALS) leaf
			List<UAnLS> Leaf_size2over = LeafsCandidate.FindAll(p => p.Size>=2);
			nx = (st_Leaf=="")? 1: 2;
            foreach( var UA in Leaf_size2over ){
                Color crBG = _ColorsLst[ (nx++)%_ColorsLst.Length ]; 
                UA.UCellLst.ForEach( UC => UC.Set_CellColorBkgColor_noBit( noB:0x1FF, clr:AttCr, clrBkg:crBG) );  //@@@ pBOARD[UC.rc]

				if( nx >= 3 )  st_Leaf += "\n       ";
				st_Leaf += $"  ALS {UA.bitExp.ToRCStringComp()} #{UA.FreeB.ToBitStringNZ(9)} RCC#{UA.RCC.ToBitStringNZ(9)}";
				st_LeafS += $" ALS:{UA.bitExp.ToRCStringComp()}";
            }

			string methodName = "SubsetExclusion" + ((Leaf_size2over.Count>0)? "_with_ALS": "");

			var  overlap_Cells = pBOARD.FindAll(q=> q.ECrLst!=null && q.ECrLst.Count()>1);
			string st_overlap = (overlap_Cells.Count()>0)? "(overlap)": "";
			string st_overlapRC = overlap_Cells.ToRCString();


			string st_Exclude = "";

			if( pBOARD.Any(p=>p.CancelB>0) ){
				foreach( var P in pBOARD.Where(p=>p.CancelB>0) ){
					st_Exclude += $" {P.rc.ToRCString()}#{Abs(P.No)+1}"; 
					P.Set_CellDigitsColor_noBit(P.CancelB,AttCr);			//######################
				}
				SolCode=2;
				solFound = true;
			}
			else{ ePZL.BOARD.ForEach( P=> P.ECrLst=null ); }


			Result     = $"{methodName} Stem:{st_Stem} Leaf:{st_LeafS}";
			ResultLong = $"{methodName}{st_overlap}\n  Stem:{st_Stem}\n  Leaf:{st_Leaf}\n  Exclude:{st_Exclude}";
			if( st_overlap!="")  ResultLong += $"\n  Overlap : {st_overlapRC}";
			//extResult = "";	// This is a prohibited operation! Clear all analysis results.

			return solFound;

			bool IsOverlap() => pBOARD.Any( q=> q.ECrLst!=null && q.ECrLst.Count()>1 );
			
		}
	}
}