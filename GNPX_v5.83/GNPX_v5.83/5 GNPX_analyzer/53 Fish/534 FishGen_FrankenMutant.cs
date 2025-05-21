using System;
using System.Linq;
using static System.Diagnostics.Debug;
using GIDOO_space;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;

namespace GNPX_space{
    public partial class FishGen: AnalyzerBaseV2{
        private readonly int _rcbSel = 0x7FFFFFF;

        // ========================================
        // Frankenn/MutantFish
        // ========================================

        //81.....32.2783149.......7182963481..478.1.369..1796824182.....3.4512.98.....8.241

        public bool FrankenMutantFish( ){       
            for( int sz=2; sz<=4; sz++ ){   //no fin: max size is 4
                for( int no=0; no<9; no++ ){
                    foreach( var _ in ExtFishSub( sz, no, FMSize:27, _rcbSel, _rcbSel, FinnedFlag:false) ){
                        if( pAnMan.Check_TimeLimit() ) return false;
						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                    }
                    if( pAnMan.Check_TimeLimit() ) return false;
                }
            }
            return false;
        }
        // Finned Frankenn/MutantFish
        public bool FinnedFrankenMutantFish( ){
            for( int sz=2; sz<=7; sz++ ){   //Finned: max size is 7 (5:Squirmbag 6:Whale 7:Leviathan)
                for( int no=0; no<9; no++ ){
                    foreach( var _ in ExtFishSub( sz, no, FMSize:27, _rcbSel, _rcbSel, FinnedFlag:true) ){
                        if( pAnMan.Check_TimeLimit() ) return false;
						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                    }
                    if( pAnMan.Check_TimeLimit() ) return false;
                }
            }
            return false;
        }



        //-----------------------------------------------------------------------
        
        public IEnumerable<bool> ExtFishSub( int sz, int no, int FMSize, int baseSelecter, int coverSelecter, bool FinnedFlag, bool _Fdef=true ){       
            int noB = 1<<no;
            bool extFlag = (sz>=3 && ((baseSelecter|coverSelecter).BitCount()>18));
            FishMan FMan = new FishMan( this, FMSize:FMSize, no, sz, extFlag );                  // ..... FMSize: 27  ( row, column, block )

            // ===== select BaseSet =====
            foreach( var Bas in FMan.IEGet_BaseSet( baseSelecter, FinnedFlag:FinnedFlag )){ 
                //if( pAnMan.Check_TimeLimit() )  yield break;

                // ===== select CoverSet =====
                foreach( var Cov in FMan.IEGet_CoverSet( Bas, coverSelecter, FinnedFlag ) ){
                    UInt128 FinB81 = Cov.FinB81;
                    UInt128 ELM = UInt128.Zero;
                    var FinZeroB = FinB81.IsZero();

                    //===== no Fin =====
                    if( !FinnedFlag && FinZeroB ){         
                        if( !FinnedFlag && (ELM=Cov.CoverB81.DifSet(Bas.BaseB81) ).BitCount()>0 ){                      
                            foreach( var P in ELM.IEGet_UCell_noB(pBOARD,noB) ){ P.CancelB=noB; SolCode=2; }
                            if(SolCode>0){              //solved!
                                if( SolInfoB ){
                                    _FishResult(no,sz,Bas,Cov,(FMSize==27), ELM ); //FMSize 18:regular 27:Franken/Mutant
                                }
                                yield return true;
                            }
                        }
                    }

                     //===== Finned ===== 
                    else if( FinnedFlag && !FinZeroB ){    //===== Finned ===== 
                        UInt128 Ecand=Cov.CoverB81.DifSet(Bas.BaseB81);
                        ELM = UInt128.Zero;
                        foreach( var P in Ecand.IEGet_UCell_noB(pBOARD,noB) ){
                            if( (FinB81.DifSet(ConnectedCells81[P.rc]) ).BitCount()==0 ) ELM = ELM.Set(P.rc);
                        }
                        if( ELM.BitCount() > 0 ){    //there are cells/digits can be eliminated                        
                            foreach( var P in ELM.IEGet_rc().Select(p=>pBOARD[p]) ){ P.CancelB=noB; SolCode=2; }   
                            if(SolCode>0){  //solved!
                                if( SolInfoB ){
                                    _FishResult(no,sz,Bas,Cov,(FMSize==27), ELM ); //FMSize 18:regular 27:Franken/Mutant
                                }
                                yield return true          ;
                            }
                        }
                    }
                    continue;
                }
            }
            yield break;       
        }



    }  
}