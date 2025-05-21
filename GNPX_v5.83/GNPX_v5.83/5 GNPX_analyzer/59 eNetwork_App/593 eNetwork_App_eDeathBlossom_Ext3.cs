using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Navigation;
using static System.Diagnostics.Debug;

namespace GNPX_space{

    public partial class eNetwork_App: AnalyzerBaseV2{
        //DeathBlossom is an algorithm based on the arrangement of ALS.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page53.html

        // DeathBlossom is an algorithm that uses ALS.
        // DeathBlossom using AIC is also possible.
        // We can also configure a DeathBlossom algorithm that combines both ALS and AIC.
        // We can also configure the DeathBlossom algorithm, which combines inter-cell links, intra-cell links, ALS, and AIC.

        // ***
        //  eNW_DeathBlossom is the DeathBlossom version with extended links.
        //  Concatenated/extended links are used for Force algorithms.
        //

		//.38.6...96....93..2..43...1..61..9355.3.8.1...4........8.65..13...8..5.63.59..827   @@@@@  r1c4#5, error occurred in step 8

        //1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9  for develop(1)
        //1.2956.485.6...29.4.9.7.56..538...1.248....56.17..582.761589432394...185825431679  for develop(2)
        //285..91.4743..1...169.24.7.6.4....8.8.2.4.6.7597..84.14...928.33.84....292..8.74.  for develop(3,4)
        //
    //  public bool eNetwork_FC_Contradiction( ) => eNW_DeathBlossom_Collective( solverCode:1 );      //===-1
        public bool eNW_DeathBlossom( )          => eNW_DeathBlossom_Collective( solverCode:2 );        //===-2 

      //public bool eNetworkSolver( )            => eNW_DeathBlossom_Collective( solverCode:3 );
            


        // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
        // (1) Sudoku problems always have a solution. There are no multiple solutions.
        // (2) The cell must have one digit true and the rest false.
        // (3) In a cell, If "everything except the digit X can be proved to be false", then "the digit X is true".
        // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
		
		private bool debugPrintB = false;	//#####
        public bool eNW_DeathBlossom_Collective( int solverCode ){
			
            //==============================
			if( debugPrintB )  eNetwork_Link.DispID = true;		//#####
            //------------------------------



			bool retPrepare = eNetwork_Dev5_Prepare( nPls:3, minSize:2 );
            if( !retPrepare ) return false;

            // ========== Search ==========
            //extResult = "";
            foreach ( var UCOrg in pBOARD.Where(p=>(p.FreeB>0)) ){                  //----- origin rc [Cell with exclusion candidates]
			
				//if( pAnMan.Check_TimeLimit() ) return (SolCode>0);				// --- If the speed is not improved, the algorithm will not work. #####@@@@@

				WriteLine( $" + eNW_DeathBlossom_Collective   UCOrg:{UCOrg.rc.ToRCString()}" );

				//if( UCOrg.rc != 14 )  continue;					//  ##### GNPX_1_SAMPLE2_v6.txt Q.44

                int  freeB0=UCOrg.FreeB, freebF=0, Soltype=-999;
				foreach( var noOrg in UCOrg.FreeB.IEGet_BtoNo() ){                  //----- origin no [digit for possible exclusion]
                    // if(debugPrintB)WriteLine( $" UCOrg.rc:{UCOrg.rc} noOrg:#{noOrg+1}" );
					WriteLine( $" +++ eNW_DeathBlossom_Collective   UCOrg:{UCOrg.rc.ToRCString()} noOrg:#{noOrg+1}" );
				
					//if( noOrg != 1 )  continue;						//  ##### GNPX_1_SAMPLE2_v6.txt Q.44
					//debugPrintB = (UCOrg.rc==14) && (noOrg==1 );	//  ##### GNPX_1_SAMPLE2_v6.txt Q.44
					//eNetwork_Link.DispID = true;					//  ##### GNPX_1_SAMPLE2_v6.txt Q.44

                    //========== QSearch_CreateNetwork =================================================

                    ULogical_Node ULGstart = new( no:noOrg, rc:UCOrg.rc, pmCnd:1);    // no,rc,pmCnd
                    eGLink_ManObj.QSearch_Network_RadialType( ULGstart, rcBreak:-1, extLink:1, rcProhibit:UInt128.Zero, debugPrintB:debugPrintB );
                    //----------------------------------------------------------------------------------


                    ULogical_Node ULogNodeOrg = new( no:noOrg, rc:UCOrg.rc, pmCnd:1 );

                    if(debugPrintB) WriteLine( $" =============== UCOrg.rc:{UCOrg.rc} noOrg:#{noOrg+1}" );
                    foreach ( var UCDes in pBOARD.Where(p=>(p.FreeB>0)) ){			//----- destination Cell[Stem Cell]	
                        var (noBitF,noContra) = _DetermineType(UCDes);

                        bool solutionFound=false;
                        if( noContra>0 ){              // -----[1] Contradiction type -----
                            ULogical_Node ULG_des = new( no:noContra, rc:UCDes.rc, pmCnd:0 );
							var eN = eNetwork_Man.Get_NodeRef( ULG_des );

                            eDeathBlossom_Contradiction_SolResult( ULGstart, ULG_des, noContra );
                            solutionFound = true;
                        }

                        else if( noBitF==UCDes.FreeB ){       // -----[2] eDeathBlossom type -----
                          //ULogical_Node ULG_des = new( no:0, rc:UCDes.rc, pmCnd:0);    // (no is free)
							ULogical_Node ULG_des = new( noB9:noBitF, b081:UInt128.One<<UCDes.rc, pmCnd:0);    // (no is free)
                            eDeathBlossom_SolResult( ULGstart, ULG_des );
                            solutionFound = true;
                        }
                
                        // ----- Solved -----
                        if( solutionFound ){
                            SolCode = 2;
							if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                        }              
                    }
                }

            }
            return (SolCode>0);



            // ----- inner function -----
            (int,int) _DetermineType( UCell UC ){ 
                int noBitF=0, noContra=-9;
                int tfFlag = eGLink_ManObj.eNetwork_pmCnd[UC.rc];

                if( tfFlag == 0 )   return (noBitF,noContra);

                foreach( int no in UC.FreeB.IEGet_BtoNo() ){ 
                    int tfX = (tfFlag>>(no*2))&3;   
                    if( tfX == 3 ){ noContra=no; break; }          // T/F derived. Contradiction #no
                    if( tfX == 1 ){       
                        ULogical_Node ULG_no = new ULogical_Node( no:no, rc:UC.rc, pmCnd:0 );
                      //var eN = eNetwork_Man.eNetwork_NodeList.Find(p => p.matchKey2==ULG_no.matchKey2);
						var eN = eNetwork_Man.Get_NodeRef(ULG_no);
                        if( eN==null || eN.eChaineNtwk_pre_Minus == null )  break;
                        noBitF |= 1<<no;  
                    }
                }
                 
				return (noBitF,noContra);              // true(10) : derived false in all elements
            }
        }


        private void eDeathBlossom_Contradiction_SolResult( ULogical_Node ULGstart,  ULogical_Node ULG_des, int noContra ){
          //var eN = eNetwork_Man.eNetwork_NodeList.Find(p => p.matchKey2==ULG_des.matchKey2);   
			var eN = eNetwork_Man.Get_NodeRef(ULG_des);
            UCell  UCstart=pBOARD[ULGstart.rc], UC_des=pBOARD[ULG_des.rc];

            string st = $"eDeathBlossom_Contradiction\n Stem: {ULG_des.rc.ToRCString()}#{noContra+1}";
            st += $"\n  Eliminate: {ULGstart.rc.ToRCString()}#{ULGstart.no+1}";
            string stL="", stLex="";

            bool IsMatch2 = ULGstart.matchKey2==ULG_des.matchKey2;
            for( int pm=0; pm<=1; pm++ ){
                if( pm==0 && IsMatch2 )continue;

                var (_,eNList) = eGLink_ManObj.Get_eNetwork_OrgToChain( eN, pmCnd:1-pm );// Reverse path of "pmCnd" value
                stL += "\n" + eGLink_ManObj.eNetwork_ToStringB(eNList).eLinkFormatter(55,4);
                stLex += "\n" +  eNetwork_ToStringA(eNList);

                Color  crA;
                int crKX=0, crKX2, size_ColorsLst=_ColorsLst.Length-1;
                foreach( var eL in eNList.Where(p=> !(p is eNW_Link_IntraCell)) ){ // Omit coloring of inter-cell links.
                    int  noB2=1<<eL.eN_Org_nPM.no;
                    crKX2 = 0;
                    if( !(eL is eNW_Link_IntraCell || eL is eNW_Link_InterCells) )   crKX2 = crKX = (crKX%size_ColorsLst)+1;
                    crA = _ColorsLst[crKX2];
                    foreach( var P in eL.eObject.IEGet_UCell(pBOARD) ){
                        if( P.rc == ULGstart.rc )  continue;  
                        P.Set_CellColorBkgColor_noBit( noB:noB2, clr:AttCr, clrBkg:crA );
                    }
                }
            }
   
            Color  cr, crStem=Colors.LightPink, crElim=Colors.DeepSkyBlue;
            UC_des.Set_CellColorBkgColor_noBit( noB:UC_des.FreeB, clr:AttCr, clrBkg:crStem);  
            UC_des.Set_CellFrameColor(Colors.Blue);
            if( !IsMatch2 ){
                UCstart.Set_CellColorBkgColor_noBit( noB:1<<ULGstart.no, clr:AttCr, clrBkg:crElim);  
                UCstart.Set_CellFrameColor(Colors.Orange);
            }

            Result = st.Replace("eDeathBlossom","eDB").Replace("\n","");
            ResultLong = $"{st}\n{stL}";
            extResult  = $"{st}{stLex}";   

            UCstart.CancelB = 1<<ULGstart.no;
        }



        private void eDeathBlossom_SolResult( ULogical_Node ULG_elim, ULogical_Node ULG_stem ){
            UCell UCstem=pBOARD[ULG_stem.rc], UCelim=pBOARD[ULG_elim.rc];

            Color  cr, crStem=Colors.LightPink, crElim=Colors.DeepSkyBlue;
            UCstem.Set_CellColorBkgColor_noBit( noB:UCstem.FreeB, clr:AttCr, clrBkg:crStem);  
            UCstem.Set_CellFrameColor(Colors.Blue);
            UCelim.Set_CellColorBkgColor_noBit( noB:1<<ULG_elim.no, clr:AttCr, clrBkg:crElim);  
            UCelim.Set_CellFrameColor(Colors.Orange);

            int crKX=0, crKX2, size_ColorsLst=_ColorsLst.Length-1;
            string stL="", stLex="";
            foreach( int noS in UCstem.FreeB.IEGet_BtoNo() ){
                ULogical_Node ULG_no = new ULogical_Node( no:noS, rc:ULG_stem.rc, pmCnd:0 );
              //  var eN = eNetwork_Man.eNetwork_NodeList.Find(p => p.matchKey2==ULG_no.matchKey2);      
				var eN = eNetwork_Man.Get_NodeRef(ULG_no);
                var (_,eNListF) = eGLink_ManObj.Get_eNetwork_OrgToChain( eN, pmCnd:0 );      // --- pmCnd:0   
                if( eNListF==null || eNListF.Count()==0 )   WriteLine( $"eNListF error:{eN}" );

                string stLF =  "\n" + eGLink_ManObj.eNetwork_ToStringB(eNListF).eLinkFormatter(55,4);  //WriteLine( stLF );
                string stLFex = eNetwork_ToStringA(eNListF);  //WriteLine( stLF );   
                    // WriteLine( $"result eN:{eN} {stLF}" );
                stL += $"\n{stLF}";
                stLex += $"\n{stLFex}";

                foreach( var eL in eNListF.Where(p=> !(p is eNW_Link_IntraCell)) ){ // Omit coloring of inter-cell links.
                    int  noB2=1<<eL.eN_Org_nPM.no;
                    
                    crKX2 = 0;
                    if( !(eL is eNW_Link_IntraCell || eL is eNW_Link_InterCells) )   crKX2 = crKX = (crKX%size_ColorsLst)+1;
                    cr = _ColorsLst[crKX2];
                    foreach( var P in eL.eObject.IEGet_UCell(pBOARD) ){
                        if( P.rc == ULG_elim.rc )  continue;  
                        P.Set_CellColorBkgColor_noBit( noB:noB2, clr:AttCr, clrBkg:cr );
                    }
                }
            }

            int rc = ULG_elim.rc, no=ULG_elim.no, noB=1<<no;
            string st = $"eDeathBlossom\n Stem: {UCstem.rc.ToRCString()}#{UCstem.FreeB.ToBitStringN(9)}";
            st += $"\n Eliminate: {rc.ToRCString()}#{no+1}";
       
            UCelim.CancelB = noB;

            Result = st.Replace("\n","");
            ResultLong = st + stL;         
            extResult = $"{st}{stLex}";   
        }
    }
}