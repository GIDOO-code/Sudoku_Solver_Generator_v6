using System;
using System.Linq;
using System.Collections.Generic;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;

namespace GNPX_space{

    //Copy below line and paste on 9x9 grid of GNPX. 
	// The selection range when copying can be rough.
    // 1..4....8.9.1...5.....63.....13.5.79..3...8..76.2.94.....75.....1...6.4.8....4..2  
	// 81.....32.2783149.......7182963481..478.1.369..1796824182.....3.4512.98.....8.241



	public partial class eGeneralLogicGen: eGeneralLogicGen_base{

		public  eGeneralLogicGen( GNPX_AnalyzerMan pAnMan ): base( pAnMan ){ }

		public bool eGeneralLogic( ){ 	
			// test_IE_GL_Condition();

			eGeneralLogic_Prepare( type:3 );

			GLMaxSize = G6.nud_GenLogMaxSize;
            GLMaxRank = G6.nud_GenLogMaxRank;

			GLMaxRank = 0; 

			for( int rank=0; rank<=GLMaxRank; rank++ ){

				// ============ Select BaseSet ============
				foreach( var basSet in IE_GL_BaseSet( GLMaxSize, rank, debugPrintB:debugPrintB ) ){

					// ============ Select CoverSet ============
					foreach( var bcvSet  in IE_GL_CoverSet( basSet, debugPrintB ) ){
						eGeneralLogicResult( bcvSet, debugPrintB );
						SolCode = 2;
						if( SolInfoB ) ResultLong = Result;
						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
					}
				}	
				//--------------------------------------------
			}
            return (SolCode>0);
        }


	#region BaseSet
		private IEnumerable<UBasCovSet> IE_GL_BaseSet( int GLMaxSize, int rank, bool finned=false, bool debugPrintB=false ){
			List<ULogical_Node> baseSet = new();
			int nxt=0;

			int noB9_BOARD = pBOARD.Aggregate(0,(p,q)=> p| q.FreeB );
					if(debugPrintB) WriteLine( $"noB9_BOARD:{noB9_BOARD.ToBitString(9)}" );

			// ========== [1] Setting element conditions ==========
			foreach( var (noB9sel,GLsize) in IE_GL_Condition( GLMaxSize, noB9_BOARD, debugPrint:true ) ){
						if(debugPrintB) WriteLine( $" GLsize:{GLsize} B:{noB9sel,3} {noB9sel.ToBitString(9)}" ) ;  
				// ========== [2] Selecting elements using the digit(s) axis ==========
					
				UGL_ListSel = UGL_List.FindAll(P=> __IsMatch(P,noB9sel) );
						bool __IsMatch(ULogical_Node P, int _noB9sel ){
							if(P.lkType==1)  return (P.noB9&_noB9sel)>0;
							if(P.lkType==2)  return (P.noB9&_noB9sel)==_noB9sel;
							return false;
						}

					if( UGL_ListSel==null || UGL_ListSel.Count<GLsize ) continue;
							if(debugPrintB){ WriteLine("\nUGL_ListSel"); UGL_ListSel.ForEach( p=> WriteLine(p) ); } 

				// ========== [3] Selecting elements using RCB axis ==========
				Combination cmbBas = new( UGL_ListSel.Count, GLsize );           // <<<<<
				nxt=9;
				while( cmbBas.Successor(nxt) ){
					baseSet.Clear();

					UInt128[] usedBas9 = new UInt128[9];
					long rcbnFrame=0;
					UInt128[] gridRCN=new UInt128[10];

					for( int k=0; k<GLsize; nxt=++k ){
						nxt=k;
						ULogical_Node P = UGL_ListSel[ cmbBas.Index[k] ];

						rcbnFrame |= P.rcbnFrame;

						var (sizeRCBN,sizeL) = func_sizeCheck4( rcbnFrame, printB:(k==GLsize) );
						if( sizeRCBN > GLsize )  goto LcmbBasNxt;				
								if(debugPrintB) WriteLine( $" --- ULogical_Node [{k}] {P}" );

						foreach( var no in P.noB9.IEGet_BtoNo() ){
							if( (usedBas9[no]&P.b081) > 0 )  goto LcmbBasNxt;
							usedBas9[no] |= P.b081;
						}

						baseSet.Add(P);
					}

					int noB9 = baseSet[0].noB9;
					UBasCovSet basCovSet = new UBasCovSet( GLsize, rank, noB9, baseSet, usedBas9 );

							 if(debugPrintB){
								baseSet.ForEach( P=>  WriteLine( $" ID:{P.ID,3}  {P.rcbFrame.ToBitString27rcb(digitB: false)}   {P}") );
							 }

					yield return basCovSet;

				 LcmbBasNxt:
					continue;
				}

			}
			yield break;
			
			// ===== inner function =====
				(int,List<int>) func_sizeCheck4( long RCBNframe, bool printB=false ){
					var sizeL = new List<int>();
					int noBL = ((int)(RCBNframe&0x1FF)).BitCount();
					int n = (int)(RCBNframe&0x1FF);
					int r = ((int)(RCBNframe>>9))&0x1FF;  sizeL.Add( r.BitCount() );
					int c = ((int)(RCBNframe>>18))&0x1FF; sizeL.Add( c.BitCount() );
					int b = ((int)(RCBNframe>>27))&0x1FF; sizeL.Add( b.BitCount() );

					string st = printB? "func_sizeCheck4: " + string.Join( " ", sizeL ): "";
					sizeL.Sort();
					sizeL.Add( n.BitCount() );
					int sizeRCBN = sizeL[0];// + n.BitCount();
						if(printB)  WriteLine( $"{st} {sizeL[3]} =>{sizeRCBN}" );

					return (sizeRCBN,sizeL);
				}
		}
	#endregion BaseSet



		// =================================================================================================================

		private  IEnumerable<UBasCovSet> IE_GL_CoverSet( UBasCovSet basCovSet, bool debugPrintB=false ){					
			int GLsize = basCovSet.GLsize;
			UInt128[] usedBas9 = basCovSet.usedBas9;
			List<ULogical_Node> BaseSetLst = basCovSet.baseSet;
			int noBact = basCovSet.noB;
			
			List<ULogical_Node>	UGL_List_4CoverSet = UGL_ListSel.FindAll( P=> IsNotBaseSet(P) && IsContactWith2Cell(P) );
						bool IsNotBaseSet( ULogical_Node P ) => BaseSetLst.Find(q => q.ID==P.ID) is null;
						bool IsContactWith2Cell( ULogical_Node P ){
							int nc = 0;
							foreach( var no in P.noB9.IEGet_BtoNo() ){ nc += (P.b081&usedBas9[no]).BitCount(); }
							return  nc>=2;
						}
			if( UGL_List_4CoverSet.Count < GLsize )  yield break;	
					if(debugPrintB){
						foreach( int no in noBact.IEGet_BtoNo() ){
							WriteLine( $"\n   IE_GL_CoverSet no:#{no+1}" );
							UGL_List_4CoverSet.ForEach( P=> WriteLine( $"   ID:{P.ID,3}  {P.rcbFrame.ToBitString27rcb(digitB:false)} {P}") );
						}
					}


			List<ULogical_Node> CovSet = new();
			int nxt=0;
			Combination cmbCov = new(UGL_List_4CoverSet.Count, GLsize );
			while( cmbCov.Successor(nxt) ){
				CovSet.Clear();

				UInt128[] usedCov9 = new UInt128[9];
				nxt = 0;
				for( int k=0; k<GLsize; nxt=++k ){
					ULogical_Node P = UGL_List_4CoverSet[ cmbCov.Index[k] ];
					foreach( int no in P.noB9.IEGet_BtoNo() ){
						if( (usedCov9[no]&P.b081) > 0 )  goto LcmbCovNxt;
						usedCov9[no] |= P.b081;
					}
					CovSet.Add(P);
				}


				if( !IsCovered() )  goto LcmbCovNxt;
						bool IsCovered(){
							for( int no=0; no<9; no++ ) if( usedBas9[no].DifSet( usedCov9[no] ) > UInt128.Zero )  return false;
							return true;	 
						}

				UInt128[] Elim9 = new UInt128[9];
				for( int no=0; no<9; no++ )	 Elim9[no] = usedCov9[no].DifSet( usedBas9[no] );

				if( IsElim_Blank() )  goto LcmbCovNxt;
						bool IsElim_Blank(){
							for( int no=0; no<9; no++ ) if( Elim9[no] > UInt128.Zero )  return false;
							return true;	 
						}

						if(debugPrintB){
							for( int no=0; no<9; no++ ){
								if( Elim9[no] > UInt128.Zero )   WriteLine( $"#Elim[#{no+1}]  {Elim9[no].ToBitString81()}" );
							}
						}

				basCovSet.SetCoverSet( CovSet, usedCov9, Elim9 );
				yield return basCovSet;

		  	  LcmbCovNxt:
				continue;
			}
			yield break;
		}

		private void eGeneralLogicResult( UBasCovSet bcSet, bool debugPrintB=false ){
            var  baseSet  = bcSet.baseSet; 
            var  coverSet = bcSet.coverSet;

			foreach( var P in pBOARD ){
				var (b9,c9,e9) = Get_noBEval(P.rc, bcSet );
				if( b9>0 )   P.Set_CellColorBkgColor_noBit( b9, clr:AttCr, clrBkg:SolBkCr2 );
				int c9C = c9.DifSet(b9);
				if( c9C>0 )  P.Set_CellColorBkgColor_noBit( c9, clr:AttCr, clrBkg:SolBkCr );
				if( e9>0  )  P.CancelB = P.FreeB&e9;
			}

			string st=$"eGeneralLogic size:{bcSet.GLsize} rank:{bcSet.rank}";
			string msgB = bcSet.baseSet.Aggregate("",  (a,b) => a+$" {b.HouseToString()}" ).TrimStart();
			string msgC = bcSet.coverSet.Aggregate("", (a,b) => a+$" {b.HouseToString()}" ).TrimStart();

			string msgE= "";

			for( int no=0; no<9; no++ ){
				if( bcSet.elim9[no] == UInt128.Zero ){ continue; }
				msgE += $"{bcSet.elim9[no].ToRCStringComp()} #{no+1}";
			}
			
            ResultLong = $"{st}\r  BaseSet: {msgB}\r CoverSet: {msgC}\r Eliminate: {msgE}";
            Result = $"eGL2 s:{bcSet.GLsize} r:{bcSet.rank} / {msgB} / {msgC} / {msgE}";
		}

		private (int,int,int)  Get_noBEval( int rc, UBasCovSet bcSet ){
			UInt128[] usedBas9 = bcSet.usedBas9, usedCov9=bcSet.usedCov9, elim9=bcSet.elim9;
			int  b9=0, c9=0, e9=0;
			for( int no=0; no<9; no++ ){
				int noB = 1<<no;
				if( usedBas9[no].IsHit(rc) )  b9 |= noB;
				if( usedCov9[no].IsHit(rc) )  c9 |= noB;
				if(    elim9[no].IsHit(rc) )  e9 |= noB;
			}
			return (b9,c9,e9);
		}
	}
}