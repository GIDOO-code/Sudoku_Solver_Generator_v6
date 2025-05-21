using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;

using GIDOO_space;
using System.Data;
//using Windows.System;
using System.Windows.Media.Media3D;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace GNPX_space{
    public partial class eNetwork_App: AnalyzerBaseV2{
        private int stageNoP_eKrakenFish = -1;

    //Kraken Fish is an algorithm that connects ALS into a loop in RCC.
    // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page38.html

    //Kraken (Finned) Fish is an algorithm that combines Super Link with Fish. 
    //Kraken (Finned) Fish is an analysis algorithm that extends the effectiveness of link to multiple links connections.
    //For links, super links (inter-cell links, cell group links, ALS links) are used.

    //Duplicate solutions occur in Kraken Franken. Omitted the duplicate Solutions in the code below.
    //  pAnMan.SnapSaveGP(ePZL,OmitteDuplicateSolutionB:true)

    //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
    //.38.6...96....93..2..43...1..61..9355.3.8.1...4........8.65..13...8..5.63.59..827   @@@@@
    //  Kraken F/M Xwing #1 r12/b12 r1c8/+7 is false
    // [W r1c8/+7 -> r5c8/-7] => [W r5c8/+4 -> r5c9/-4] => [S r5c9/-4 -> r2c9/+4] => [W r2c9/+4 -> r2c3/-4] => [S r2c3/-4 -> r1c1/+4] => [W r1c1/+4 -> r1c1/-1]
    // [W r1c8/+7 -> r5c8/-7] => [S r5c8/-7 -> r5c4/+7] => [W r5c4/+7 -> r4c6/-7] => [W r4c6/+4 -> r9c6/-4] => [W r9c6/+1 -> r1c6/-1]

    // Kraken F/M Xwing #1 r12/b12 r2c4/+2 is false
    // [W r2c4/+2 -> r5c4/-2] => [W r5c4/+7 -> r5c8/-7] => [W r5c8/+4 -> r1c8/-4] => [S r1c8/-4 -> r1c1/+4] => [W r1c1/+4 -> r1c1/-1]
    // [W r2c4/+2 -> r5c4/-2] => [W r5c4/+7 -> r4c6/-7] => [W r4c6/+4 -> r9c6/-4] => [W r9c6/+1 -> r1c6/-1]

    //.2...783..47.2...13..1....7....38.15...5.4...58.79....6....2..82...8.57..793...6. 

        private int dbCC=0;
        private List<(int,UInt128)> eLinkSummaryList;
        private void eKrakenFish_Prepare(){
            eNetwork_ForceChain_prepare( );
            OrgDes_rcno.Sort();
            var orgGroup = OrgDes_rcno.GroupBy( p=> p&0x7FFF000F );
               
            eLinkSummaryList = new();
            foreach( var grpSel in orgGroup ){
                
                UInt128 Drc128 = grpSel.Aggregate(UInt128.Zero, (p,q)=> p = p| UInt128.One<<((q&0xFFF)>>4) );
                Drc128 = Drc128.Reset( (grpSel.Key>>20) );
                eLinkSummaryList.Add( (grpSel.Key,Drc128) );

					/*
						WriteLine( $"Drc128:{Drc128.ToBitString81()}" );
						foreach( var P2 in grpSel ){
							var (Orc,Ono,Drc,Dno) = Func_ToRcNo(P2);   
							WriteLine( $"Orc:{Orc.ToRCString()}#{Ono+1} => Drc:{Drc.ToRCString()}#{Dno+1}" );
						}
					*/
            }
        }


        public bool eKrakenFish( ){
			bool retPrepare = eNetwork_Dev5_Prepare(nPls:1);
            if( !retPrepare ) return false;

            if( stageNoP!=stageNoP_eKrakenFish ) eKrakenFish_Prepare();
            stageNoP_eKrakenFish = stageNoP;
			base.AnalyzerBaseV2_PrepareStage();

			for( int sz=2; sz<5; sz++ ){            
				for(int no=0; no<9; no++ ){
        		    foreach( var _ in eKrakenFishSub( sz, no, 27, FinnedFlag:false) ){
						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
						//extResult = "";
                    }
				}
            }
            return false;
        }


        public bool Finned_eKrakenFish( ){
			bool retPrepare = eNetwork_Dev5_Prepare(nPls:1);
            if( !retPrepare ) return false;

            for(int sz=2; sz<5; sz++ ){
				for(int no=0; no<9; no++ ){
        		    foreach( var FrkFish in eKrakenFishSub( sz, no, 27, FinnedFlag:true) ){
						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
						//extResult = "";
                    }
				}
            }
            return false;
        }




        public IEnumerable<bool> eKrakenFishSub( int sz, int no, int FMSize, bool FinnedFlag ){
			int baseSelecter=0x7FFFFFF, coverSelecter=0x7FFFFFF;
            int noB=(1<<no);
            UInt128 BDL_no81 = FreeCell81b9[no];

            //========================================================

			FishMan FMan = new FishMan(this,FMSize,no,sz,(sz>=3));
            foreach( var Bas in FMan.IEGet_BaseSet(baseSelecter,FinnedFlag:FinnedFlag) ){   //Generate BaseSet
                                         //if( Bas.HouseB.HouseToString() != "r78" )  continue;   //for debugging

                foreach( var Cov in FMan.IEGet_CoverSet(Bas,coverSelecter,FinnedFlag,debugPrint:false) ){    // debugPrint @@@@@
                                        //if( Cov.HouseC.HouseToString() != "b78" )  continue;   //for debugging

                     UInt128 FinB81 = Cov.FinB81;

                    if( FinnedFlag == FinB81.IsZero() )  continue;      // ""finned" designation?  "With fin"?
						/*
							WriteLine( $"dbCC:{dbCC++} \rbas:{Bas.BaseB81.ToBitString81()}\rCov:{Cov.CoverB81.ToBitString81()}" );  //for debugging
							WriteLine( $"Bas.HouseB:{Bas.HouseB.rcbToBitString27()}");               //for debugging
							WriteLine( $"Cov.HouseB:{Cov.HouseC.rcbToBitString27()}");               //for debugging
						*/	
                               
        
                    List<int> Sol_KFm = new List<int>();
                    foreach( var hc in Cov.HouseC.IEGet_BtoNo(27) ){
                        UInt128 cellsIsEclude = (Cov.CoverB81 & HouseCells81[hc]) | FinB81;
							//   WriteLine( $"cellsIsEclude:{cellsIsEclude.ToBitString81()}");          //for debugging

							//      var Es = eLinkSummaryList.FindAll( P => 
							//                      (P.Item1&0xF)==no && (cellsIsEclude.DifSet(P.Item2)).IsZero() );


                        var Es = eLinkSummaryList.FindAll( P => (P.Item1&0xF)==no );

                        if( Es.Count>0 ){
							/*
								Es.ForEach(P => {
									var (Orc,Ono,Drc,Dno) = Func_ToRcNo(P.Item1);
									WriteLine( $" Origin:{Orc.ToRCString()}#{Ono+1}  =>  Destination:{Drc.ToRCString()}#{Dno+1}" );
								} );
							*/
                            Es.ForEach(P => Sol_KFm.Add(P.Item1) );
                        }
                    }
                }
            }
            yield break;


            string _ToHouseName( int h ){
                string st="";
                switch(h/9){
                    case 0: st="   row "; break;
                    case 1: st="Column "; break;
                    case 2: st=" block "; break;
                }
                st += ((h%9)+1).ToString();
                return st;
            }
        }


        private string _KrFish_FishResultEx( int no, int sz, UFish Bas, UFish Cov, string msg0 ){
            int   HB=Bas.HouseB, HC=Cov.HouseC;
            UInt128 PB=Bas.BaseB81, PFin=Cov.FinB81; 
            UInt128 EndoFin=Bas.EndoFinB81, CnaaFin=Cov.CannFinB81;
            string[] FishNames = { "Xwing","SwordFish","JellyFish","Squirmbag","Whale", "Leviathan" };
    
            PFin-=EndoFin;
            try{
                int noB=(1<<no);    
                PB.IE_SetNoBBgColor( pBOARD, noB, AttCr,SolBkCr);
                PFin.IE_SetNoBBgColor( pBOARD, noB, AttCr,SolBkCr2);
                EndoFin.IE_SetNoBBgColor( pBOARD, noB, AttCr,SolBkCr3);
                CnaaFin.IE_SetNoBBgColor( pBOARD, noB, AttCr,SolBkCr3);

                string msg = "\r     Digit: " + (no+1);                 
                msg += "\r   BaseSet: " + HB.HouseToString();
                msg += "\r  CoverSet: " + HC.HouseToString();
                string msg2=$" #{(no+1)} BaseSet:{HB.HouseToString().Replace(" ","")} CoverSet:{HC.HouseToString().Replace(" ","")}";
 
                string FinmsgH="", FinmsgT="";
                if( PFin.BitCount() > 0 ){
                    FinmsgH = "Finned ";
                    string st="";
                    foreach( var rc in PFin.IEGet_rc() ) st += " "+rc.ToRCString();
                    msg += "\r    FinSet: "+st.ToString_SameHouseComp();
                
                }
                 
                if( EndoFin.IsNotZero() ){
                    FinmsgT = " with Endo Fin";
                    string st="";
                    foreach( var rc in EndoFin.IEGet_rc() ) st += " "+rc.ToRCString();
                    msg += "\r  Endo Fin: "+st.ToString_SameHouseComp();
                }

                if( CnaaFin.IsNotZero() ){
                    FinmsgH = "Cannibalistic ";
                    if( PFin.BitCount() > 0 ) FinmsgH = "Finned Cannibalistic ";
                    string st="";
                    foreach( var rc in CnaaFin.IEGet_rc() ) st += " "+rc.ToRCString();
                    msg += "\r  Cannibalistic: "+st.ToString_SameHouseComp();
                }

                string Fsh = FishNames[sz-2];
				int bf=0, cf=0;
				for(int k=0; k<3; k++ ){
					if( ((Bas.HouseB>>(k*9))&0x1FF)>0 ) bf |= (1<<k);
					if( ((Cov.HouseC>>(k*9))&0x1FF)>0 ) cf |= (1<<k);
				}
                if((bf+cf)>3) Fsh = "Franken/Mutant " + Fsh;
                Fsh = "Kraken "+FinmsgH+Fsh+FinmsgT;
                ResultLong = Fsh+msg+"\r"+msg0;  
                string _krfMsgEx = Fsh.Replace("Franken/Mutant","F/M")+msg2;
				Result = _krfMsgEx.Replace("BaseSet","Base").Replace("CoverSet","Cover");

                return _krfMsgEx;
            }
            catch( Exception ex ){ WriteLine( $"{ex.Message}\r{ex.StackTrace}"); }

            return "error in _KrFish_FishResultEx";
        }

        private void __Dev_PutDBLEx( string dir, string fName, bool append ){
            if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            using( var fp=new StreamWriter(dir+@"\"+fName,append:append,encoding:Encoding.UTF8) ){  
                string st=pBOARD.ConvertAll(P=>P.No).Connect("").Replace("-","+").Replace("0",".");
                fp.WriteLine(st);
            }
        }       


    }
}