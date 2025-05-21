using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;

using GIDOO_space;
using System.Data;
//using Windows.System;
using System.Windows.Media.Media3D;

namespace GNPX_space{

    //1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9  for develop(1)


    public partial class eNetwork_App: AnalyzerBaseV2{
        static UInt128 Max_b081  = (UInt128.One<<81)-1;
        public bool eALS_Wing_2D( ) => eALS_Wing( szStart:2, szEnd:2 );
        public bool eALS_Wing_3D( ) => eALS_Wing( szStart:3, szEnd:3 );
        public bool eALS_Wing_4D( ) => eALS_Wing( szStart:4, szEnd:4 );


        public bool eALS_Wing( int szStart=2, int szEnd=4 ){  // ... exercise
			bool retPrepare = eNetwork_Dev5_Prepare(nPls:1);
            if( !retPrepare ) return false;

            if( ALSMan.ALSList==null || ALSMan.ALSList.Count<=2 ) return false;

            var pBOARD81 = pBOARD.Create_bitExp81_9();
            int ALSsize = ALSMan.ALSList.Count;
                ALSMan.ALSList.ForEach( P => WriteLine(P) );

            for( int sz=szStart; sz<=szEnd; sz++ ){    // sz:number of digits in the stem cell
                foreach( UCell UCstem in pBOARD.Where(p=>p.FreeBC==sz) ){                               // Select StemCell

                    Combination cmb = new( ALSsize, sz );
                    int nxt=int.MaxValue;
                    while( cmb.Successor(skip:nxt) ){                                                   // Combination
                        nxt = _IsConnected_roughCheck( UCstem, cmb );                                   // roughCheck
                        if( nxt<sz )  continue;

                        List<UAnLS> UBList = cmb.GetCollection( ALSMan.ALSList );                
                        UInt128    usedUB = UBList.Aggregate( UInt128.Zero, (a,b)=>a|b.bitExp );
                        int        FreeB_and = UBList.Aggregate( 0x1FF, (a,b)=>a&b.FreeB);

                        foreach( var noElm in FreeB_and.IEGet_BtoNo() ){

							// ----- All ALS cells contain noELM
							int noB = 1<<noElm;
							int ix = UBList.FindIndex(P => (P.FreeB&noB)==0 );
							if( ix >= 0 ){ nxt = ix; goto Lnxtcmb; }

							// ----- Find the intersection of cell influences. This is rcELM.
							UInt128 rcBR_Elimination = FreeCell81b9[noElm] .DifSet(usedUB);
							foreach( var qALS in UBList ){ 
								foreach( UCell P in qALS.UCellLst.Where( p => (p.FreeB&noB)>0 ) ){
									rcBR_Elimination &= ConnectedCells81[P.rc];
								}
							}

                            //rcBR_Elimination = rcBR_Elimination.DifSet(usedUB);

                            if( rcBR_Elimination == UInt128.Zero ) continue;     // => noElm candidates.          
                          
                            foreach( UCell UCelm in rcBR_Elimination.IEGet_UCell(pBOARD) ){ //rc -> UCell
                                UInt128[] Elim9 = new UInt128[9];

                                int stemFreeB = 0;
                                foreach( var UB in UBList ){
									eGLink_ManObj.ALS_Reconfigure_initialSetting( UB, noElm );
                                    eGLink_ManObj.ALS_Reconfigure( UB, noElm );          //ALS turns to a LockedSet by noElm
                                    int E = _Get_ExcludableStemDigits_FreeBwk( UCstem.rc, UB);
                                    stemFreeB |= E;
                                    // WriteLine( $"Stem_rc:{UCstem.rc} ID:{UB.ID:000} + E:{E.ToBitString(9)} = stemFreeB:{stemFreeB.ToBitString(9)} vs {UCstem.FreeB.ToBitString(9)}" );
                                }
                             
								//  UBList.ForEach( P => P.RestoreFreeB() );  //Undo the FreeB in ALS  (LockedSet -> ALS)
                                if( UCstem.FreeB.DifSet(stemFreeB) == 0 ){ 

                                    // ----- Solution found -----
                                    eALS_Wing_SolResult( UCstem, UBList, noElm, UCelm );
				
									if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                                }
                            }
                        }
					Lnxtcmb:
						continue;
                    }
                }                 
            }

            return false;

        // ----- inner functions -----
            int _IsConnected_roughCheck( UCell UStem, Combination cmb ){　　　　　　         // roughCheck
                // are connected with the same digits completely. ... roughCheck

                UInt128 UStem_rcBR = UInt128.One<<UStem.rc;                                  // Stem                                                       
                int sz=cmb.Index.Length;

                for( int k=0; k<sz; k++ ){
                    UAnLS UB = ALSMan.ALSList[ cmb.Index[k] ];
                    if( (UStem_rcBR & UB.bitExp) > 0 )  return k;                            // Overlap

                    int retFreeB = __IsConnected_sub( UStem, UB );                 
                    if( retFreeB==0 ) return k;
                  }
                return sz;
            
                int __IsConnected_sub( UCell UStem, UAnLS UB ){
                    int FreeBcommon = UStem.FreeB & UB.FreeB;
                    if( FreeBcommon == 0 ){ return 0; }                                      // no common digits

                    int hitFreeB=0, rc=UStem.rc;
                    foreach( var no in FreeBcommon.IEGet_BtoNo() ){                          // Common digits
            //A         foreach( int h in UB.rcbBitExp9And[no].IEGet_BtoHouse27() ){    
						foreach( int h in UB.rcbAnd9_wk(no).IEGet_BtoHouse27() ){    
                            if( HouseCells81[h].IsHit(rc) ){ hitFreeB |= 1<<no; break; }   // Common house
                        }
                    }
                    return hitFreeB;
                }
            }

            UInt128 _Get_rc_CandidateForElimination( UInt128[] pBOARD81, List<UAnLS> UBList, int no ){
                UInt128 rcBR_Elimination = pBOARD81[no];
                foreach( var UB in UBList ){
                    UInt128 UBrc = UInt128.Zero;

                    // h:house of common to all ALS,  no => UBrc:Cells in all house-h 
                    foreach( int h in UB.rcbAnd9_wk(no).IEGet_BtoHouse27() ){ UBrc |= HouseCells81[h]; }
                    // => rcBR_Elimination:Cells common to ALS-UBrc 
                    rcBR_Elimination &= UBrc;   
                }
                return rcBR_Elimination; // (In other words) Elimination candidate cells
            }
        
            int _Get_ExcludableStemDigits_FreeBwk( int rcStem, UAnLS UB ){
                int Eno=0;
                foreach( int no in UB.FreeB.IEGet_BtoNo() ){
                    int noB = 1<<no;
                    UInt128 Erc128 = Max_b081;    
                    bool hitB=false;
                    foreach( var UC in UB.UCellLst.Where(p=>(p.FreeBwk&noB)>0) ){
                        Erc128 &= ConnectedCells81[UC.rc]; 
                        hitB=true;
                    }
                    if( hitB && Erc128.IsHit(rcStem) )  Eno |= 1<<no;
                }
                return Eno;
            }
        }

        private bool eALS_Wing_SolResult( UCell UCstem, List<UAnLS> UBList, int no, UCell UCelm ){
            int noB = 1<<no;

            SolCode = 2;
            UCelm.CancelB = noB;

            Color crStem=Colors.Gold, crElm=Colors.Pink, crGreen=Colors.Green;
            UCstem.Set_CellColorBkgColor_noBit(UCstem.FreeB,crGreen,crStem);
            UCstem.Set_CellFrameColor(Colors.Blue);

            UCelm.Set_CellColorBkgColor_noBit(noB,AttCr,crElm);

            int kx = 0;
            string msg = "";
            foreach( var UB in UBList){
                Color Cr = _ColorsLst[++kx];
                foreach( var uc in UB.UCellLst){
                    uc.Set_CellColorBkgColor_noBit(uc.FreeB.DifSet(noB), crGreen, Cr);
                    uc.Set_CellColorBkgColor_noBit(noB, AttCr, Cr);
                }

                string stT = "";
                foreach( var P in UB.UCellLst) stT += " " + P.rc.ToRCString();
                msg += $"\n   ALS_{kx}: {stT.ToString_SameHouseComp()} #{UB.FreeB.ToBitStringNZ(9)}";
            }

            UInt128 overlapB = UInt128.Zero;
            foreach( var P in pBOARD.Where(p=>p.FreeB>0 && p.ECrLst!=null) ){
                if( P.ECrLst.Count<=1 )  continue;
                var G = P.ECrLst.GroupBy(p=>p.ClrCellBkg).ToList();
                if( G.Count(q=> ((Color)q.Key)!=Colors.Black) >= 2 )  overlapB |= UInt128.One<<P.rc;
            }
            if( overlapB.IsNotZero() ){
                string stT = "";
                foreach( var rc in overlapB.IEGet_rc() ) stT += " " + rc.ToRCString();
                msg += $"\n   (ALS overlap cells: {stT.ToString_SameHouseComp()})";
            }

            int sz = UCstem.FreeBC;
            string SolMsg = $"eALS_Wing_{sz}D  Stem: {UCstem.rc.ToRCString()}  Eliminated: {UCelm.rc.ToRCString()}#{no+1}";
            Result = SolMsg;
            SolMsg = $"eALS_Wing_{sz}D\n  Stem Cell: {UCstem.rc.ToRCString()}";
            ResultLong = SolMsg + $"   {msg}\r  Eliminated: {UCelm.rc.ToRCString()} #{no+1}";

            return true;
        }

    }
}