using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using static System.Math;
using static System.Diagnostics.Debug;
using GIDOO_space;

namespace GNPX_space{
    public partial class FishGen: AnalyzerBaseV2{
        //Autocannibalism
        //http://www.dailysudoku.com/sudoku/forums/viewtopic.php?p=26306&sid=13490447f6255f8d78a75b647a9096b9

        //http://forum.enjoysudoku.com/als-chains-with-overlap-cannibalism-t6580-30.html
        //http://www.dailysudoku.com/sudoku/forums/viewtopic.php?t=219&sid=dae2c2133114ee9513a6a37124374e7c
        //http://www.dailysudoku.co.uk/sudoku/forums/viewtopic.php?p=1180&highlight=#1180

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //.6...52..4..1..65.....6..3....3...65.5........7.5.....681457.......2.517.2.9..846
        //....2.6..7.41.9....3.847.1..7.9.1.541.3...7.9.9.2.8.36.4.683.7.3.75.4.......1.4..

        //....9.6..4.61.8....8.462.5..6.2.1.373.5...2.8.4.3.6.95.3.914.7.6.18.5.......2.8..
        //12.59.6844561.8.2..8.462.51.6.251437315749268.4.386195.3.9145766.18.5.425.462.81.  for develop

        public bool CannibalisticFMFish( ){
            CannibalisticFMFish_Ex( FinnedFlag:false, CannFlag:true );
            return  false;
        }

        public bool FinnedCannibalisticFMFish( ){
            CannibalisticFMFish_Ex( FinnedFlag:true, CannFlag:true );
            return  false;
        }


        private bool CannibalisticFMFish_Ex( bool FinnedFlag=false, bool CannFlag=true ){
            for(int sz=2; sz<=7; sz++ ){
                for(int no=0; no<9; no++ ){
					foreach( bool solFound in IE_CannibalisticFMFish_sub(sz,no,FMSize:27,FinnedFlag,EndoFlag:false,CannFlag:true) ){
						if( solFound ){
							if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
						}
					}
                }
            }
            return false;
        }

        public IEnumerable<bool> IE_CannibalisticFMFish_sub( int sz, int no, int FMSize, bool FinnedFlag, bool EndoFlag=false, bool CannFlag=false ){
            int noB=(1<<no);
            int baseSelecter=0x7FFFFFF, coverSelecter=0x7FFFFFF;
            FishMan FMan=new FishMan(this,FMSize,no,sz,extFlag:(sz>=3));
            
            foreach( var Bas in FMan.IEGet_BaseSet(baseSelecter, FinnedFlag:FinnedFlag, EndoFlag:EndoFlag) ){    

                foreach( var Cov in FMan.IEGet_CoverSet(Bas, coverSelecter, FinnedFlag:FinnedFlag, CannFlag:CannFlag) ){                  //CoverSet
                    UInt128 FinB81 = Bas.BaseB81 .DifSet(Cov.CoverB81);

                    if( FinB81.BitCount() == 0 ){
                        foreach( var P in Cov.CannFinB81.IEGet_UCell_noB(pBOARD,noB) ){ P.CancelB=noB; SolCode=2; }
                        if(SolCode>0){
                            if( SolInfoB ) _FishResult(no,sz,Bas,Cov,(FMSize==27), Cov.CannFinB81 );
                            yield return true; // @is Valid
                        }
                    }

                    else{
                        FinB81 |= Cov.CannFinB81;
                        UInt128 ELM = UInt128.Zero;
                        UInt128 E=(Cov.CoverB81.DifSet(Bas.BaseB81)) | Cov.CannFinB81;
                        ELM = UInt128.Zero;
                        foreach( var rc in E.IEGet_rc() ){
                            if( (FinB81.DifSet( ConnectedCells81[rc]) ).BitCount() == 0 )  ELM = ELM.Set(rc);
                        }
                        if( ELM.BitCount() > 0 ){
                            foreach( var P in ELM.IEGet_UCell_noB(pBOARD,noB) ){ P.CancelB=noB; SolCode=2; }
                            if( SolCode>0 ){
                                if( SolInfoB ) _FishResult(no,sz,Bas,Cov,(FMSize==27), ELM );
                                yield return true; // @is Valid
                            }
                        }
                    }
                }
            }
            yield break;
        }

        private void ___Debug_CannFish(string MName){
            using( var fpX=new StreamWriter(" ##DebugP.txt",append:true,encoding:Encoding.UTF8) ){
                string st="";
                pBOARD.ForEach(q =>{ st += (Max(q.No,0)).ToString(); } );
                st=st.Replace("0",".");
                fpX.WriteLine(st+" "+MName);
            }
        }
    }  
}