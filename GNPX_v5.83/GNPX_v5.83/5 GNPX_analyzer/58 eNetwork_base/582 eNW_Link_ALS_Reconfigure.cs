using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using static System.Math;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPX_space {
       
    [Serializable]
    public partial class eNetwork_Man: ALSLinkMan{       
		private int __dbX = 0;


		public void ALS_Reconfigure_initialSetting_house( UAnLS qALS, int no, UInt128 h81, bool debugPrintB=false ){
			int noB = 1<<no;
			foreach( var P in qALS.UCellLst ){
				P.FreeBwk = P.FreeB;
				if( h81.IsHit(P.rc) )  P.FreeBwk = P.FreeB.DifSet(noB);
			}		
			if(debugPrintB) WriteLine( $" ---- ALS_Reconfigure_initialSetting_no[{__dbX++}]: no:#{no+1}  {qALS}" );
		}

		public void ALS_Reconfigure_initialSetting( UAnLS qALS, int no, bool debugPrintB=false ){
			int noB=1<<no;
			qALS.UCellLst.ForEach( P => P.FreeBwk = P.FreeB.DifSet(noB) );
			if(debugPrintB) WriteLine( $" ---- ALS_Reconfigure_initialSetting_no[{__dbX++}]: no:#{no+1}  {qALS}" );
		}


        public bool ALS_Reconfigure( UAnLS qALS, int no, bool debugPrintB=false ){ 
			if(debugPrintB) WriteLine( $" +++++ before[{__dbX}]: no:#{no+1}  {qALS.ToString_FreeBwk()}" );

            int  sz=qALS.Size, noB=1<<no;
			int FreeBX = qALS.UCellLst.Aggregate(0, (p,q) => p |= q.FreeBwk);
			if( FreeBX.BitCount() != qALS.Size )  return false;	 // not lockedSet.

			// *==*==*==*==* small solver *==*==*==*==*==*==*==*==*==*==*==*==*==*==*  
			List<int> qRCFrB = qALS.UCellLst.ConvertAll( q=> q.FreeBwk );

			int  reconfigCtrl = 0b111;	//@@@@@
			int  noBenable = qALS.FreeB;
			do{
					if(debugPrintB) WriteLine( $"reconfigCtrl:{reconfigCtrl.ToBitString(5)}" );
				if( (reconfigCtrl&0b001)>0 ){ if( NakedSingle() )  continue; } 	
				if( (reconfigCtrl&0b010)>0 ){ if( LastDigit() )    continue; }
				if( (reconfigCtrl&0b100)>0 ){ if( LockedSet() )    continue; }
			}while(reconfigCtrl>0);
			// *--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*

			bool changedT = qALS.UCellLst.Any( q=> q.FreeBwk!=q.FreeB);
            if(changedT){
				int FreeBwk = 0;
                for( int k=0; k<sz; k++ ) qALS.UCellLst[k].FreeBwk = qRCFrB[k];
					if(debugPrintB) WriteLine( $" +++++ after[{__dbX}]: no:#{no+1}  {qALS.ToString_FreeBwk()}" );
            }

			qALS.FreeBwk = qALS.UCellLst.Aggregate(0, (p,q) => p |= q.FreeBwk);
            return changedT;

		
			
		// ============================== inner function ==============================
			// ---------- NakedSingle ----------
			// Confirm with "One element in Cell". This element is eliminated in other Cells.
			bool  NakedSingle( ){	
				reconfigCtrl &= 0b111111110;
						if(debugPrintB) qRCFrB.ForEach( (Q,kx) => WriteLine( $"NakedSingle --- {kx} {Q.ToBitString(9)}") );
								
				bool changed = false;
				for( int n=0; n<sz; n++ ){
					if( qRCFrB[n].BitCount()!=1 )  continue;
					int noX = qRCFrB[n].BitToNum(), noXB = 1<<noX;
					noBenable = noBenable.BitReset(noX);
						
					for( int k=0; k<sz; k++ ){
						if( k==n || (qRCFrB[k]&noXB)==0 )  continue;
						qRCFrB[k] = qRCFrB[k].DifSet(noXB);
						changed = true;
					}
					if(changed){
						reconfigCtrl |= 0b1; // set bit1
						if(debugPrintB) qRCFrB.ForEach( (Q,kx) => WriteLine( $"NakedSingle +++ {kx} {Q.ToBitString(9)}") );
					}
				}
				return changed;
			}

			// ---------- LastDigit ----------
			// Confirm with "elements existed only in one cell". Other elements are eliminated in this Cell
			bool LastDigit(){	
				reconfigCtrl &= 0b111111101;	// reset bit2
						if(debugPrintB)qRCFrB.ForEach( (Q,kx) => WriteLine( $"LastDigit --- {kx} {Q.ToBitString(9)}") );
				int oneF=0, twoF=0, X; 
				for( int n=0; n<sz; n++ ){
					X = qRCFrB[n];
					if( X.BitCount() <= 1 )  continue;
					twoF |= oneF & X;			// exist in more than one place
					oneF |= X;					// exist somewhere
				}
				int nonF = oneF.DifSet(twoF);   // elements existed only in one cell
				bool changed=false;
				if( nonF > 0 ){
						
					foreach( int noX in nonF.IEGet_BtoNo() ){
						int noXB = 1<<noX;
						for( int k=0; k<sz; k++ ){
							if( (qRCFrB[k]&noXB) == 0 )  continue;
							qRCFrB[k] = noXB;
							changed = true;
						}
					}
					if( changed ){
						reconfigCtrl |= 0b10; // set bit2
						if(debugPrintB) qRCFrB.ForEach( (Q,kx) => WriteLine( $"LastDigit +++ {kx} {Q.ToBitString(9)}") );
					}
				}
				return changed;
			}

			// ---------- LockedSet ----------
			bool LockedSet(){
				reconfigCtrl &= 0b111111011;	// reset bit3
				if( sz <= 2 )  return false;

				if(debugPrintB)qRCFrB.ForEach( (Q,kx) => WriteLine( $"LockedSet --- {kx} {Q.ToBitString(9)}") );
						
				bool changed = false;
				foreach( var cmbBit in __Func_Combination(sz,2,sz-1) ){ // Elements is 2 or more, and pure subset.
					int freeB9=0;
					for( int k=0; k<sz; k++ ){ if( ((1<<k) & cmbBit) > 0 ) freeB9|=qRCFrB[k]; }
					if( freeB9.BitCount() != cmbBit.BitCount() )  continue;// LockedSet (and pure subset).
                        
					for( int k=0; k<sz; k++ ){
						if( ((1<<k) & cmbBit ) > 0 )  continue;
						if( (qRCFrB[k] & freeB9) == 0 )  continue;
						qRCFrB[k] = qRCFrB[k].DifSet(freeB9);
						changed = true;
					}

					if( changed ){
						reconfigCtrl |= 0b100; // set bit3
						if(debugPrintB) qRCFrB.ForEach( (Q,kx) => WriteLine( $"LockedSet +++ {kx} {Q.ToBitString(9)}") );
						break;
					}
				}	
				return changed;
			}

			IEnumerable<int> __Func_Combination( int sz, int szStart, int szEnd ){  //...Can only be used with this function ...
				for( int nSZ=szStart; nSZ<=szEnd; nSZ++ ){
					Combination cmb = new(sz,nSZ);
					while( cmb.Successor() ){ yield return cmb.ToBitExpression(); }
				} 
				yield break;
			}
        }
	}
}
                    