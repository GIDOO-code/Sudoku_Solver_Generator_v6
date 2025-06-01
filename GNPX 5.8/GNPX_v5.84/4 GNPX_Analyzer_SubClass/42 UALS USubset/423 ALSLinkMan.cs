using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Math;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Security.Cryptography.X509Certificates; 
using System.Security.Cryptography;
using System.Xml.Linq;


namespace GNPX_space {

    public class ALSLinkMan: AnalyzerBaseV2{
        private int                 pALSSizeMax => G6.ALSSizeMax;
        public List<UAnLS>           ALSList;
		static int					_PlsB_ = 0;
		static int					_maxSize_ = 0;

//        public List<ALSLink>        AlsInnerLink;   //innerLink
        public List<Link_CellALS>[] LinkCeAlsLst;   //Cell->ALS
        public bool                 ALS2ALS_Link;   //ALS ->ALS
		public int					stage_nPlus = 0;

        public ALSLinkMan( GNPX_AnalyzerMan pAnMan ){
            this.pAnMan = pAnMan;
			this.Initialize();
		}

		public void Initialize(){
            ALSList       = null;
            LinkCeAlsLst = null;
            ALS2ALS_Link = false;
			stage_nPlus  = 0;
			_PlsB_  = 0;
			_maxSize_ = 0;
        }

		private bool IsHit_stage_nPlus( int nPls ){
			if( stageNoP != (stage_nPlus>>4) ){ stage_nPlus = stageNoP<<4; return false; }
			if( (stage_nPlus & (1<<nPls)) == 0 )  return false;
			stage_nPlus = (stage_nPlus<<4) | (1<<nPls);
			return true;
		}

        public int Prepare_ALSLink_Man( int nPlsB, int maxSize=5, bool setCondInfo=false, bool debugPrintB=false ){
			{
				if( ALSList !=null && ALSList.Count>3 && _PlsB_>=nPlsB && _maxSize_>=maxSize )  return ALSList.Count;
				_PlsB_ = Max( _PlsB_, nPlsB);
				_maxSize_ = Max( _maxSize_, maxSize ); 
			}
		
			int plus = G6.AnLS? 3: 1;         
            plus = Max( plus, nPlsB );

			if( IsHit_stage_nPlus(plus) )  return ALSList.Count; 
				// WriteLine( $"@@@@@@@@@@ {++eNdebug} Prepare_ALSLink_Man" );
            ALSList = Create_ALSList( plus, minSize:1, maxSize );

            if(debugPrintB)  ALSList.ForEach( (P,kx) => WriteLine( $"{kx} {P}") );

            return ALSList.Count;  
		}
		private bool IsPureALS2( UAnLS UAn ){
			if( UAn.Size == 1 ) return  true;
			foreach( var P in UAn.UCellLst ){
				var Q = UAn.UCellLst.Where(q=>q.rc!=P.rc).Any(q=> (q.FreeB&P.FreeB)==0 );
				if( Q )	return false;
			}

			if( UAn.Size >= 3 ){
				for( int n=2; n<UAn.Size; n++ ){
					Combination cmb = new( UAn.Size, n );
					while(cmb.Successor()){
						int fb = 0;
						for( int k=0; k<n; k++ )  fb |= UAn.UCellLst[ cmb.Index[k] ].FreeB;
						if( fb.BitCount() == n ){
							//WriteLine( UAn );
							return false;
						}
					}
				}
			}
			return true;
		}


		private List<UAnLS> Create_ALSList( int nPls, int minSize, int maxSize ){
            int ALSSizeMax = G6.ALSSizeMax;
            List<UAnLS>  _tmpALSList = new() ;
            for(int h=0; h<27; h++ ){       // house 0-8(row) 9-17(column) 18-26(block)
                List<UCell> UCells = pBOARD.IEGetCellInHouse(h,0x1FF).ToList();
                if( UCells.Count<1 ) continue;

                int szMax = Min(UCells.Count,8-nPls);
                szMax = Min(szMax,pALSSizeMax);						// ALS size maximum value
                for( int sz=minSize; sz<=szMax; sz++ ){				// ALS size: Number of cells that make up ALS
                    Combination cmb = new Combination(UCells.Count,sz);
                    while( cmb.Successor() ){						// generate a combination
                        int FreeB = cmb.Index.Aggregate( 0, (p,q) => p| UCells[q].FreeB );
						int FreeBC = FreeB.BitCount();
                        if( FreeBC<=sz || FreeBC>(sz+nPls) ) continue;
               
                        List<UCell> UCellLst = cmb.Index.ToList().ConvertAll(q=> UCells[q]);

                        UAnLS UA = new UAnLS( 0, FreeB, UCellLst );
								 
						// Elements contained in only one cell (This is acceptable as ALS)
						// if( IsContainedInOnlyOneUCell(UA) ) continue;	
                        if( !UA.IsPureALS() ) continue;					// Pure ALS : not include LockedSet.
						if( !IsPureALS2(UA) ) continue;
							
						_tmpALSList.Add(UA);
                    }
                }
            }
                                                                                                                                                                                                                                                                                                         
            _tmpALSList = _tmpALSList.DistinctBy(p=>p.bitExp).ToList();
            _tmpALSList.Sort( (a,b) => a.CompareTo(b) );
            int ID=0;
            _tmpALSList.ForEach(P=> P.ID=ID++ );
		  //_tmpALSList.ForEach( p=> WriteLine(p));

            return _tmpALSList;  
        }




        public IEnumerable<UAnLS> IEGetCellInHouse( int lvl, int noB, UInt128 Area128 ){
            foreach( var P in ALSList.Where(p=>p.Level==lvl) ){
                if( (P.FreeB&noB)==0 )         continue;
                if( (P.bitExp.DifSet(Area128) ).IsNotZero() ) continue;
                yield return P;
            }
        }

        //RCC
        public int  Get_AlsAlsRcc( UAnLS UA, UAnLS UB ){    
			int _ToB(int val, int ix) => (val>0)? 1<<ix:0; 
			int FreeB = UA.FreeB & UB.FreeB;
			int RCC = FreeB.IEGet_BtoNo().Aggregate(0, (p,n) => p|= _ToB( UA.rcbAnd9[n] & UB.rcbAnd9[n] , n) );
/*
            if( FreeB == 0 ) return 0;									// no common digit
            if( (UA.bitExp&UB.bitExp).IsNotZero() )          return 0;  // overlaps　
            if( (UA.connectedB81&UB.bitExp).IsZero() )  return 0;  // no connected

			int RCC = 0;
			foreach( int no in FreeB.IEGet_BtoNo() ){
				if( (UA.BitExp9[no] .DifSet(UB.connected81_9[no]) ) == UInt128.Zero )  RCC |= 1<<no;
				//WriteLine( $"#{no+1}  {UA.BitExp9[no].ToBitString81()}  {UB.connected81_9[no].ToBitString81()}" );
			}
			//if( RCC>0 ){ WriteLine( $"Get_AlsAlsRcc RCC:{RCC.ToBitString()}\n UA:{UA}\n UB:{UB}" ); }
*/
            return RCC;
        }


        // Link between Cell#n --> ALS
        //  ALS turns to LockedSet when Cell#n is true.
        public void QSearch_Cell2ALS_Link( ){
            Prepare_ALSLink_Man(1);
            if( LinkCeAlsLst!=null ) return ;
            if( ALSList==null || ALSList.Count<2 )  return;
            LinkCeAlsLst = new List<Link_CellALS>[81];


            foreach( var PA in ALSList ){
                //debug WriteLine( $"\r{PA}");
                foreach ( var no in PA.FreeB.IEGet_BtoNo() ){
                    int noB=(1<<no);
                    UInt128 H = UInt128.MaxValue;
                    foreach( var P in PA.UCellLst.Where(q=>(q.FreeB&noB)>0)){
                        H = H & ConnectedCells81[P.rc];
                    }
                    if( H.IsZero() ) continue;
                  
                    foreach ( var P in H.IEGet_UCell_noB(pBOARD,noB) ){
                        var Q = new Link_CellALS(P,PA,no);
                        if( LinkCeAlsLst[P.rc]==null )  LinkCeAlsLst[P.rc]=new List<Link_CellALS>();
                        LinkCeAlsLst[P.rc].Add(Q);

                        // if P#no is true, PA(ALS) turns to LockedSet.
                        //debug WriteLine( $"no:#{no+1} Q.UC:{Q.UC}" );
                    }
                }
            }
        }

        public void QSearch_ALS2ALS_Link( ){        // <-- eALS_Chain
            Prepare_ALSLink_Man( nPlsB:1, setCondInfo:true, debugPrintB:false);

            if( ALS2ALS_Link ) return;
            ALS2ALS_Link=true;

            var cmb = new Combination( ALSList.Count, 2 );
            while( cmb.Successor() ){

                UAnLS UA = ALSList[cmb.Index[0]];
                UAnLS UB = ALSList[cmb.Index[1]];
				//WriteLine( $"\n --UA{UA}\n --UB{UB}" );

                int RCC = Get_AlsAlsRccEx( UA, UB );    // improved! <- Get_AlsAlsRcc( UA, UB );
                if( RCC==0 ) continue;

                if( UA.ConnectedALS is null ) UA.ConnectedALS = new();
                UA.ConnectedALS.Add( (UB,RCC) );	//[ATT] ... The second term is a bit representation.

                if( UB.ConnectedALS is null ) UB.ConnectedALS = new();
                UB.ConnectedALS.Add( (UA,RCC) );	//[ATT] ... The second term is a bit representation.
            }
		}

            // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        public int Get_AlsAlsRccEx( UAnLS UA, UAnLS UB ){
            int FreeB_AB = UA.FreeB & UB.FreeB;
            if( FreeB_AB==0 )  return 0;  // no common digit

            int RCC_BitExp = 0;
            foreach( var no in FreeB_AB.IEGet_BtoNo() ){
				if( (UA.rcbAnd9[no] & UB.rcbAnd9[no]) == 0 )  continue;
                RCC_BitExp |= (1<<no); 
			}
				//WriteLine( $"RCC_BitExp:{RCC_BitExp.ToBitString(9)}");
			return RCC_BitExp;
        }


        public (UInt128,UInt128) Get_RCC_bitExp( int RCC, UAnLS UA, UAnLS UB, bool debugPrint=false ){
            if( RCC==0 )  return (UInt128.Zero,UInt128.Zero);  // no common digit

			UInt128 RCC_UA_BitExp=UInt128.Zero, RCC_UB_BitExp=UInt128.Zero;
            foreach( var no in RCC.IEGet_BtoNo() ){	
                RCC_UA_BitExp |= UA.BitExp9[no];
				RCC_UB_BitExp |= UB.BitExp9[no];
			}
			if(debugPrint){
				WriteLine( $" UA:{UA.bitExp.ToBitString81()}  UB:{UA.bitExp.ToBitString81()}" );
				for( int no=0; no<9; no++){
					WriteLine( $"  {no+1}:{UA.BitExp9[no].ToBitString81()}   {UA.BitExp9[no].ToBitString81()}" );
				}
			}
			return (RCC_UA_BitExp,RCC_UB_BitExp);
        }
    } 

}