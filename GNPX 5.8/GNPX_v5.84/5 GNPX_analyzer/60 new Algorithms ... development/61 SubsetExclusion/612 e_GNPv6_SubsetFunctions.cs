using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.IO;
using System.Text;
using static GNPX_space.SubsetTechGen;

namespace GNPX_space{

    public partial class SubsetExcludeMan{
        public GNPX_AnalyzerMan pAnMan;
        static private GNPX_Engin pGNPX_Eng;
		static private UInt128[]		pConnectedCells81{ get{ return AnalyzerBaseV2.ConnectedCells81; } }

        public  UPuzzle					ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; }
        public List<UCell>				pBOARD{ get=>ePZL.BOARD; set=>ePZL.BOARD=value; }


		private USubset Stem;
		private List<UAnLS> Leafs;
		private int			SizeStem;
		private List<DigitsPattern> noBList=null;

		public SubsetExcludeMan( GNPX_AnalyzerMan pAnMan ){
			this.pAnMan = pAnMan;
            pGNPX_Eng = pAnMan.pGNPX_Eng;
		}

		public void GenerateList_noBLis( USubset Stem, bool debugPrint=false ){ 
			this.Stem = Stem;
			this.Leafs = Leafs;		

			// Generate a list of stem candidate numbers. If concatenated within Stem, set invalid flag.
			int SizeStem = Stem.UCellLst.Count;
			switch(SizeStem){
				case 2: noBList = GenerateList_noBLis2( Stem );	break;
				case 3: noBList = GenerateList_noBLis3( Stem );	break;
			}

			noBList = noBList.FindAll(q => q.validB );		//Remove elements with the same value within the same House
				if(debugPrint){	int k=0; noBList.ForEach( q=> WriteLine( $"    {k++:00} {q.ToString()}" ) ); }

#region inner fuctions
		List<DigitsPattern> GenerateList_noBLis2( USubset Stem ){
				if( Stem.UCellLst.Count() != 2 ){ throw new ArgumentException( "size of noBLst is not 2."); }
								 
				List<int> FreeBs = Stem.UCellLst.ConvertAll( p=>p.FreeB );
				List<DigitsPattern> noBList = new();
				foreach( int no0 in FreeBs[0].IEGet_BtoNo() ){
					foreach( int no1 in FreeBs[1].IEGet_BtoNo() ){
						noBList.Add( new DigitsPattern( Stem, no0, no1 ) );
					}
				}
								
				//Set invalid flag to elements with same value in same House.
				UCell U0=Stem.UCellLst[0], U1=Stem.UCellLst[1];
				if( pConnectedCells81[U0.rc].IsHit(U1.rc) )	noBList.ForEach( q=> { if(q.d0==q.d1)  q.validB=false; });

				return noBList;
			}	
			List<DigitsPattern> GenerateList_noBLis3( USubset Stem ){
				if( Stem.UCellLst.Count() != 3 ){ throw new ArgumentException( "size of noBLst is not 3."); }

				List<int> FreeBs = Stem.UCellLst.ConvertAll( p=>p.FreeB );
				List<DigitsPattern> noBList = new();
				foreach( int no0 in FreeBs[0].IEGet_BtoNo() ){
					foreach( int no1 in FreeBs[1].IEGet_BtoNo() ){
						foreach( int no2 in FreeBs[2].IEGet_BtoNo() ){
							noBList.Add( new DigitsPattern( Stem, no0, no1, no2 ) );
						}
					}
				}

				//Set invalid flag to elements with same value in same House.
				UCell U0=Stem.UCellLst[0], U1=Stem.UCellLst[1], U2=Stem.UCellLst[2];
				if( pConnectedCells81[U0.rc].IsHit(U1.rc) )	noBList.ForEach( q=> { if(q.d0==q.d1)  q.validB=false; });
				if( pConnectedCells81[U1.rc].IsHit(U2.rc) )	noBList.ForEach( q=> { if(q.d1==q.d2)  q.validB=false; });
				if( pConnectedCells81[U2.rc].IsHit(U0.rc) )	noBList.ForEach( q=> { if(q.d2==q.d0)  q.validB=false; });

				return noBList;
			}
#endregion inner fuctions
		}

		public (bool,List<UAnLS>) Apply_Link( USubset Stem, List<UAnLS> Leafs, bool debugPrint=false ){
			bool appliedB = false;

			List<int>  RCCList = new();
			List<UAnLS> Leafs2 = new();
			foreach( var leaf in Leafs.Where(p=>p.Size==1) ){
				var Ps = noBList.FindAll(p=> (leaf.FreeB.DifSet(p.FreeBX)==0) );
				if( Ps==null || Ps.Count==0 )  continue;
				Ps.ForEach(q => q.validB=false );
				Leafs2.Add(leaf);
				appliedB = true; 
			}

			foreach( var Leaf in Leafs.Where(p => p.Size>=2) ){
				int RCC = Stem.Get_RCC_AnLS_AnLS( Leaf ); // |RCC|>=2
				if( RCC.BitCount() <= 1 )  continue;
				 var noBList_match = noBList.FindAll(p=> (RCC.DifSet(p.FreeBX)==0) );
				if( noBList_match==null || noBList_match.Count==0 )  continue;
				Leaf.RCC = RCC;
				Leafs2.Add(Leaf);
					//WriteLine( $" @Stem:{Stem} Leaf:{Leaf}\nRCC:{RCC.ToBitStringN(9)}" );
				RCCList.Add(RCC);
				noBList_match.ForEach(q => q.validB=false ); 
				appliedB = true; 
			}
			if(debugPrint){	int k=0; noBList.ForEach( q=> WriteLine( $"    {k++:00} {q.ToString()}" ) ); }
			return (appliedB,Leafs2);
		}

		public bool Check_ConfirmedDigits( bool debugPrint=false ){
			bool excludeB = false;
			int Size = noBList[0].Size;
			UEval[] UEvalList = new UEval[Size];
			for( int k=0; k<Size; k++ ){
				UEval pUEval = new UEval( noBList[0].FreeB );
				int  FreeBX=0;
				foreach( var Q in noBList.Where(p=>p.validB) ){
					int no = Q.dLst[k];
					pUEval.noCnt[no]++;
					FreeBX |= 1<<no;
				}
				pUEval.FreeBX = FreeBX;
				UEvalList[k] = pUEval;
			}

			for( int k=0; k<Size; k++ ){
				UCell UC = Stem.UCellLst[k];
				UEval pUEval = UEvalList[k];
				string st = $"Check_ConfirmedDigits:  Stem {UC.rc.ToRCString()} #{UC.FreeB.ToBitString(9)}";
				foreach( int no in Stem.UCellLst[k].FreeB.IEGet_BtoNo() ){
					int nc = pUEval.noCnt[no];
					st += $" #{no+1}-{nc}"; 
					if( nc==0 ){
						pBOARD[UC.rc].CancelB |= 1<<no;
						excludeB=true;
					}
				}
				if(debugPrint)  WriteLine( st );
			}
					if( debugPrint && excludeB){ int k=0; noBList.ForEach( q=> WriteLine( $"    {k++:00} {q.ToString()}" ) ); }

			bool SolFoundB = pBOARD.Any(p=>p.CancelB>0);
			if( SolFoundB is false ){
				//[ATT] Recovery process from "non-resolved cell coloring". ... A simple solution!
				ePZL.BOARD.ForEach( P=> P.ECrLst=null ); 
			}
			return SolFoundB;	// excludeB;
		}

	}
}