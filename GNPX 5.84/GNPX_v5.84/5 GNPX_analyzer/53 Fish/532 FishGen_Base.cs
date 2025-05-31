using System;
using System.Linq;
using static System.Diagnostics.Debug;
using GIDOO_space;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace GNPX_space{
    public partial class FishGen: AnalyzerBaseV2{

//        public int[] rcbCntrl = { 0x1FF, 0x1ff<<9, 0x3FFFF, 0x7FFFFFF };
        //=======================================================================
        //Fish:
        // Understand this algorithm, you need to know BaseSet and CoverSet.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page34.html
        //
        //81........27.3149.......718.9.34.....7.....6.....96.2.182.......4512.98........41
        //81.....32.2783149.......7182963481..478.1.369..1796824182.....3.4512.98.....8.241
      //-----------------------------------------------------------------------

        private FishMan FMan;

        public FishGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }


        private void _FishResult( int no, int sz, UFish Bas, UFish Cov, bool FraMut, UInt128 ELM ){
            int funcFM(int rcbX, int mask ) => ((rcbX&mask)>0 && (rcbX&(mask^0X7FFFFFF))==0)? 1: 0;

            int   HB=Bas.HouseB, HC=Cov.HouseC;
            UInt128 PB=Bas.BaseB81, PFin=Cov.FinB81; 
            UInt128 EndoFin=Bas.EndoFinB81, CnaaFin=Cov.CannFinB81;
            string[] FishNames = { "Xwing","SwordFish","JellyFish","Squirmbag","Whale", "Leviathan" };
    
            PFin-=EndoFin;
            try{
                int noB=(1<<no);  
				PB.IE_SetNoBBgColor(		pBOARD, noB, AttCr, AttCr3 );
                (PB-PFin).IE_SetNoBBgColor( pBOARD, noB, AttCr, SolBkCr );
                PFin.IE_SetNoBBgColor(      pBOARD, noB, AttCr, SolBkCr2 );
                EndoFin.IE_SetNoBBgColor(   pBOARD, noB, AttCr, SolBkCr3 );
                CnaaFin.IE_SetNoBBgColor(   pBOARD, noB, AttCr, SolBkCr3 );

                string msg = $"\r     Digit: #{no+1}";                 
                msg += $"\r   BaseSet: {HB.HouseToString()}";  //+"#"+(no+1);
                msg += $"\r  CoverSet: {HC.HouseToString()}";  //+"#"+(no+1);

                string FinmsgH="", FinmsgT="";
                if( PFin.BitCount()>0 ){
                    FinmsgH = "Finned ";
                    msg += $"\r    FinSet: {PFin.ToRCStringComp()}";                
                }
                 
                if( EndoFin.IsNotZero() ){
                    FinmsgT = " with Endo Fin";
                    msg += $"\r  Endo Fin: {EndoFin.ToRCStringComp()}";
                }

                if( CnaaFin.IsNotZero() ){
                    FinmsgH = "Cannibalistic ";
                    if( PFin.BitCount() > 0 ) FinmsgH = "Finned Cannibalistic ";
                    msg += $"\r  Cannibalistic: {CnaaFin.ToRCStringComp()}";
                }

                msg += $"\rEliminated: {ELM.ToRCStringComp()} #{no+1}";
                string msg2 = $" #{no+1} {HB.HouseToString().Trim()}/{HC.HouseToString().Trim()}";

                string Fsh = FishNames[sz-2];

                if(FraMut) Fsh = "Franken/Mutant "+Fsh;
                    //int FM1 = funcFM(HB,0x1FF)*1 + funcFM(HC,0X1FF<<9)*2;
                    //int FM2 = funcFM(HB,0x1FF<<9)*1 + funcFM(HC,0X1FF)*2;
                    //if( FM1!=3 && FM2!=3 ) Fsh = "Franken/Mutant "+Fsh;

                Fsh = FinmsgH+Fsh+FinmsgT;
                ResultLong = Fsh+msg;  
                Result = Fsh.Replace("Franken/Mutant","F/M")+msg2;
            }
            catch( Exception ex ){ WriteLine( $"{ex.Message}\r{ex.StackTrace}"); }
        }

    }

}