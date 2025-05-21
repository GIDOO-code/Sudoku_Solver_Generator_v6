using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using GNPX_space;
using System.Windows.Input;
using System.Security.Policy;

namespace GNPX_space{

    //Copy below line and paste on 9x9 grid of GNPX.
	// The selection range when copying does not have to be exact.
	// 81.....32.2783149.......7182963481..478.1.369..1796824182.....3.4512.98.....8.241 

	public partial class eGeneralLogicGen_base: AnalyzerBaseV2{
		static public  int          GLtrialCC;
		protected int				stageNoPMemo = -9;
		public List<ULogical_Node>	UGL_List;

		protected int				GLMaxSize;
		protected int				GLMaxRank;
		public List<ULogical_Node>	UGL_ListSel;

		protected bool				debugPrintB = false;

        static  eGeneralLogicGen_base( ){ }

        public  eGeneralLogicGen_base( GNPX_AnalyzerMan pAnMan ): base( pAnMan ){ 
			// analyzer3 = new(pAnMan);
			// test_IE_GL_Condition();
        }

		protected bool eGeneralLogic_Prepare( int type ){
			if( stageNoP!=stageNoPMemo || UGL_List==null ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();

			    GLMaxSize = G6.nud_GenLogMaxSize;
                GLMaxRank = G6.nud_GenLogMaxRank;

				UGL_List = new();
				if( (type&0x1) > 0 )  UGL_List.AddRange( Generate_links_betwine_cells( debugPrintB:false ) );
				if( (type&0x2) > 0 )  UGL_List.AddRange( Generate_links_inside_cell( szNMax:3, debugPrintB:false) );

				UGL_List = UGL_List.DistinctBy( p=> p.b90 ).ToList();	// Unify by matching rc, no																// 							
				UGL_List.Sort( (a,b)=>a.CompareToA(b) );
				int ID0=0;
				UGL_List.ForEach( p=> p.ID=ID0++);

				if(debugPrintB)  UGL_List.ForEach( P=> WriteLine(P) );
			}
			return true;


			// *==*==*==* Generate GN-Elements ==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
				List<ULogical_Node>	Generate_links_betwine_cells( bool debugPrintB=false ){					// ... Row/Column/Block link
					List<ULogical_Node>	 UGL_ListTmp = new();

					for( int no=0; no<9; no++ ){
						UInt128 qBOARD9 = FreeCell81b9[no];
						if( qBOARD9 == UInt128.Zero )  continue;

						for( int h=0; h<27; h++ ){	
							UInt128 h128 = qBOARD9 & HouseCells81[h];
							if( h128 == UInt128.Zero )  continue;
							int rcb = h128.Ceate_rcbFrameAnd();

							ULogical_Node ULG = new( noB9:1<<no, b081:h128, lkType:1 );
								if(debugPrintB) WriteLine( $"{rcb.HouseToString()} * {rcb.ToBitString27rcb( digitB:false)} " );
							UGL_ListTmp.Add(ULG);
						}
					}
					return UGL_ListTmp;
				}

				List<ULogical_Node>	Generate_links_inside_cell( int szNMax=3, bool debugPrintB=false ){	// ... inside cell
					List<ULogical_Node>	 UGL_ListTmp = new();

					foreach( var UC in pBOARD.Where(p=>p.FreeB>0) ){
							if(debugPrintB) WriteLine( $"UC:{UC}" );
						int houseB = UC.rc.rcbFrame_Value(); 

						ULogical_Node ULG = new( noB9:UC.FreeB, b081:UInt128.One<<UC.rc, lkType:2 );
							if(debugPrintB) WriteLine( $" ULG:{ULG}   noB:{UC.FreeB.ToBitString(9)}" );
						UGL_ListTmp.Add(ULG);
					}
					//if(debugPrintB)  UGL_ListTmp.ForEach( ULG => WriteLine( ULG.ToString() ) );
					return UGL_ListTmp;
				}

		}

		public class UBasCovSet{
			public int GLsize;
			public int rank;
			
		  // ----- BaseSet -----
			public int			 noB;
			public List<ULogical_Node> baseSet;
			public UInt128[]	 usedBas9;
			public UInt128		 BasBComp;
//			public int           houseBas => baseSet.Aggregate(0,(p,q)=>p|(q.houseB));
			
		  // ----- coverSet -----
		  	public List<ULogical_Node> coverSet;
			public UInt128[]	 usedCov9;
			public UInt128[]	 elim9;
//			public int           houseCov => coverSet.Aggregate(0,(p,q)=>p|(q.houseB));

			public UBasCovSet( int GLsize, int rank, int noB, List<ULogical_Node> baseSet, UInt128[] usedBas9 ){
				this.GLsize=GLsize; this.rank=rank;
				this.noB=noB; this.baseSet=baseSet; this.usedBas9=usedBas9;
			
				this.BasBComp = UInt128.Zero; //+++++
				for( int no=0; no<9; no++ ) BasBComp |= usedBas9[no];

				//foreach( var P in baseSet )  WriteLine( P );

			}

			public void SetCoverSet( List<ULogical_Node> coverSet, UInt128[] usedCov9, UInt128[] elim9 ){
				this.coverSet=coverSet; this.usedCov9=usedCov9; this.elim9=elim9;
			}

			public override string ToString( ){
				string st = $"\nbaseSet:  no:#{noB.ToBitStringN(9)}";
				foreach( int no in noB.IEGet_BtoNo() )  st += $"\n   usedBas9[#{no+1}]:{usedBas9[no].ToBitString81N()}";
				int kx=0; baseSet.ForEach( P => st += $"  {kx} {P.b081.ToString81()}" );

				st = $"coverSet:  no:#{noB.ToBitStringN(9)}";
				foreach( int no in noB.IEGet_BtoNo() )  st += $"\n   usedCov9[#{no+1}]:{usedCov9[no].ToBitString81N()}";
				kx=0; coverSet.ForEach( P => st += $"  {kx} {P.b081.ToString81()}" );
				return  st;
			}
		}



		// ======================================================================================
		public void test_IE_GL_Condition( ){ 
			GLMaxSize = 6;
			GLMaxRank = 0;
			// ***** test IEnumerable *****
			foreach( var (noB9,GLsz) in IE_GL_Condition( 5, 0x35, debugPrint:true ) ){
				WriteLine( $" GLsz:{GLsz} B:{noB9:00} {noB9.ToBitString(9)}" ) ;
			}
		}

		public IEnumerable<(int,int)> IE_GL_Condition( int GLMaxSize, int noB9, bool finned=false, bool debugPrint=false ){

			// type-1: One digit and multiple RCB axis
			for( int GLsz=2; GLsz<=GLMaxSize; GLsz++ ){
				foreach( int no in noB9.IEGet_BtoNo() ){
					yield return ( 1<<no, GLsz );
				}
			}

			// typeof-2: 
			for( int cmbsz=2; cmbsz<=GLMaxSize; cmbsz++ ){
				Combination_9B cmb9 = new( noB9.BitCount(), cmbsz, noB9 );
				int nxt=9;
				while( cmb9.Successor(nxt) ){				
					yield return ( cmb9.noB9, cmbsz );
				}

			}

/*
			// One RCB axis and multiple digits
			// if(debugPrint)  WriteLine( " ----- Combination_9B -----" );
			for( int GLsz=1; GLsz<=GLMaxSize-1; GLsz++ ){
				int cmbszMax = GLMaxSize-GLsz;
				for( int cmbsz=2; cmbsz<=cmbszMax; cmbsz++ ){
					if( GLsz+cmbsz>GLMaxSize )  continue;
					Combination_9B cmb9 = new( noB9.BitCount(), cmbsz, noB9 );
					int nxt=9;
					while( cmb9.Successor(nxt) ){
						 yield return ( cmb9.noB9, GLsz );
					}
				}
			}
*/
			yield break;
		}
		// --------------------------------------------------------------------------------------

	}

}
