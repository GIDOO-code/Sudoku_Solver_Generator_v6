using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static System.Diagnostics.Debug;
using System.Threading;

using GIDOO_space;
using System.Windows.Input;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace GNPX_space{
    using pRes=Properties.Resources;
    using  tuple3 = Tuple<int,int,int>;
    public partial class eNetwork_App: AnalyzerBaseV2{

        // Along with expanding the types of links, the link expression was changed from "strong/weak_link" to "cell, candidate digits, true/false".
        // Link connection is also determined by matching "cell, candidate digits, true/false".
        // Also, AALSXZ (Almost ALSXZ) was added as a new link. This is the first step in expanding link types.
        // (Introduced a difficult type of link, intentionally.)

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //47....3.99...4728...8.9...7...81..4..167.48...8..6..2.85..7...4.6.4.5..81.3...57.
        //47....5.92...7361...3.9...7...56..9..613.47...5..2..6.52..3...6.3.1.2..51.9...24.
        //......3...385.1.4.5..37..6..76..389...9...7...157..42..9..15..4.8.6.753...2......
       
        //6.14..5.7.3.7..4..9..35.....6..1.3.415.....688.4.3..1.....94..2..2..3.4.4.6..21.3
		//1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9  for develop


		//..1.37......218.577.....81.1.9365....6..74.3.....82.....87....44.....78..378415..
		//281537946946218357753496812129365478865974231374182695598723164412659783637841529 ... elapsed time:10ms

		//47....3.99...4728...8.9...7...81..4..167.48...8..6..2.85..7...4.6.4.5..81.3...57.
		//47...83.993..4728.628.9.4.7.9.81..4..167.489..84.6..2.85..7...4.6.4.5..8143986572
		//471258369935647281628391457592813746316724895784569123859172634267435918143986572

		//756...3898235..641491386572587...2643648..197912...835639241758145738926278...413
		//756124389823579641491386572587913264364852197912467835639241758145738926278695413 ... elapsed time:1ms

		public bool eNetwork_NiceLoop() => eNetwork_NiceLoop_base( complexLink:false );
		public bool eNetwork_NiceLoopEx() => eNetwork_NiceLoop_base( complexLink:true );



		// Processing is too complicated. I would like to organize it into an easy-to-understand structure. (---> v6 ?) 


        public bool eNetwork_NiceLoop_base( bool complexLink ){

            // ==================== for Develop ====================
            bool  debugPrintB = false;
			
			eNetwork_Link.Set_withID_( withID:debugPrintB ); // Display the element_ID in the output only when debugging.
            // -----------------------------------------------------

            // ========== Prepare ==========
            eNetwork_Dev5_Prepare( nPls:3, minSize:2 );
            eGLink_ManObj.eNetwork_Link_Switch(-1);

            int   dbgSolCC=0;
			int searchCC = pBOARD.Sum(p=>p.FreeBC), serchKK=0;
            foreach( var UCOrg in pBOARD.Where(p=>(p.FreeB>0)) ){                                   //----- origin rc 

				foreach( var noOrg in UCOrg.FreeB.IEGet_BtoNo() ){                                  //----- origin no				
					if(debugPrintB) AdditionalMessage = $" ... {++serchKK} / {searchCC}";

					//----- Control of search conditions 
                    foreach( var (pmCnd0,rcBreakX,extLink) in IE_NiceLoop_Control(UCOrg.rc, complexLink:complexLink) ){
                        ULogical_Node ULGstart = new( no:noOrg, rc:UCOrg.rc, pmCnd:pmCnd0);    // no,rc,pmCnd
                            if(debugPrintB) WriteLine( $"\n\n@@@@@ IE_NiceLoop_Control  ULGstart:{ULGstart}  pmCnd0:{pmCnd0} rcBreakX:{rcBreakX} extLink:{extLink}" );
                       
					// ========== ========== ========== ========== ========== ========== ========== ==========
                        var (solType,eL_tail) = eGLink_ManObj.QSearch_Network_LoopType( ULGstart, rcBreak:rcBreakX, extLink, debugPrintB:debugPrintB, suspendOnB:true );
                        if( solType<0 || eL_tail is null )  continue;
                    // ---------- ---------- ---------- ---------- ---------- ---------- ---------- ----------  

                        if( solType == 3 ){ // ... Contradiction!   the path was found that led to both Plus/Minus(true/false).
										if(debugPrintB)  WriteLine( $"start ULGstart:{ULGstart}   rcBreakX:{rcBreakX}" );
                            NiceLoopEx_SolResult_Contradiction( ULGstart, eL_tail, complexLink ); 
                            SolCode = 2;
                        }

                        else{   // solType>=0 && !(eL_tail is null)
							if(debugPrintB) eGLink_ManObj.eNWMan_DisplayNetworkPath( eL_tail, debugBPM:true ); 

                            eNetwork_Node eN_Des = eL_tail.eN_Des_nPM;
                            int pm = eL_tail.DesN.pmCnd;
                            var (hv,_eNList) = eGLink_ManObj.Get_eNetwork_OrgToChain( eN_Des, pmCnd:pm );                            

                            if( _eNList==null || _eNList.Count<=0 )  continue;  // <<-- This if statement is not needed.
										if(debugPrintB)  WriteLine( eGLink_ManObj.eNetwork_ToStringB(_eNList, debugPrintB:debugPrintB) );

                            ULogical_Node _endULG = new( 1<<eN_Des.no, b081:eN_Des.ULGNode.b081, lkType:solType-1, pmCnd:pm );  // variable for determining continuity	 
                            var NL_cond = Is_NiceLoop_Connected(ULGstart, _endULG );
										if(debugPrintB) WriteLine( $"ULGstart:{ULGstart}  eN_Des:{eN_Des} ... (iSCon,noX):{NL_cond}" ); 
                            if( NL_cond.Item1=="" || NL_cond.Item1=="--" )     continue; // ...try next

                            bool _solFlag = IsSolution( NL_cond, ULGstart, _eNList );
                            if( !_solFlag )  continue; 

                            // ===== Solution found =====
                            NiceLoopEx_SolResult( NL_cond, ULGstart, _eNList, complexLink );
                            SolCode = 2;
                        }
 
                        // ----- Solution save
						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid

                        break;
                    }
                }
            }

            return (SolCode>0);


          // ====================== Inner function ========================================================================================
			IEnumerable<(int,int,int)> IE_NiceLoop_Control( int rcBreak, bool complexLink ){
				// Explore with just a simple link
                yield return ( 1, rcBreak, 0);  // 1) P, rcBreak, cell-link
                yield return ( 0, rcBreak, 0);  // 2) M, rcBreak, cell-link

                //-- Explore including complex links
				if( complexLink ){
					bool extLink = G6.AIC || G6.ALS || 
								   G6.ALSXZ || G6.AnLS;
					if( !extLink )  yield break;
					yield return ( 1, rcBreak, 1 ); // 4) P, rcBreak
					yield return ( 0, rcBreak, 1 ); // 5) M, rcBreak
				}
                yield break;
            }

            (string,ULogical_Node) Is_NiceLoop_Connected( ULogical_Node sNode, ULogical_Node eNode ){ 
                if(debugPrintB) WriteLine( $"   ----- IsNLConnected? {sNode} ... {eNode}" );

                (string,ULogical_Node) NL_cond=("",null);

                if( sNode.rc != eNode.rc ){ NL_cond = ("contradiction",eNode); goto LsubEnd; }                   // rcS!=rcE ... contradiction
                if( sNode.rc<0 )  goto LsubEnd;   

                // -------------------------------------------------------------
                UCell UC = pBOARD[sNode.rc];
                bool  IsMatchno = sNode.no==eNode.no;
                bool  IsMatchTF = sNode.pmCnd==eNode.pmCnd;
                bool  UC_bivalue = UC.FreeBC==2;

                if( sNode.pmCnd==0  && eNode.pmCnd==1 ){     // F - T  (1)
                    if( !IsMatchno ){ NL_cond = ("continue",eNode); goto LsubEnd; }               
                    else{             NL_cond = ("discontinue",eNode); goto LsubEnd; }
                }

                if( sNode.pmCnd==1 && eNode.pmCnd==0 ){     // T - F  (2)
                    if( !IsMatchno && UC_bivalue ){ NL_cond = ("continue",eNode); goto LsubEnd; }  
                    if( IsMatchno ){                NL_cond = ("discontinue",eNode); goto LsubEnd; }  
                    NL_cond = ("--",eNode); goto LsubEnd;
                }

                if( sNode.pmCnd==1 && eNode.pmCnd==1 ){
                    if( IsMatchno ){  NL_cond = ("continue",eNode); goto LsubEnd; }    // T - T  (3)
                    else{             NL_cond = ("discontinue",eNode); goto LsubEnd; }
                }

                if( sNode.pmCnd==0  && eNode.pmCnd==0 ){
                    if( IsMatchno ){  NL_cond = ("continue",eNode); goto LsubEnd; }    // F - F  (4)
                    else{             NL_cond = ("discontinue",eNode); goto LsubEnd; }
                }


              LsubEnd:
                if(debugPrintB) WriteLine( $"-- -- -- -- -- {sNode} ... {eNode}   NL_cond:{NL_cond} -- -- -- -- --" );

                return NL_cond;
            }

            bool IsSolution( (string,ULogical_Node) NL_cond, ULogical_Node ULGstart, List<eNetwork_Link> eNetList ){
                var  (NLtype,eN)= NL_cond;
                bool  SolFlag = false;
                UCell UCstart = pBOARD[ULGstart.rc];

                UInt128 NL128 = eNetList.Aggregate(UInt128.Zero, (a,b)=>a|b.eObject );
                ULogical_Node ULGend = eNetList.Last().DesN;

                if( NLtype=="continue" ){
                    int kcr=0;

                    // Evaluation of Origin and Destination (not internal link connection)
                    //  ULGstart:Origin   ULGend:Destination
                    if( ULGstart.pmCnd==0 && ULGend.pmCnd==1 ){  // (rc,no,pmCnd) 
                        int noBcan = (1<<ULGstart.no) | (1<<ULGend.no);
                        int cancelB = UCstart.FreeB.DifSet(noBcan);
                        if( cancelB>0 ){ UCstart.CancelB=cancelB; SolFlag=true; }
                    }

                    eNetwork_Link eL1 = null;
                    foreach( var eL2 in eNetList ){      
                        if( eL1 != null  ){              // ... Node
                            ULogical_Node U1 = eL1.DesN;
                            ULogical_Node U2 = eL1.OrgN;
                            if( U1.matchKey3 == U2.matchKey3 ){  // (rc,no,pmCnd) 
                                if( U1.pmCnd==1 ){
                                    UCell UC = pBOARD[U1.rc];
                                    int cancelB = UC.FreeB.DifSet(1<<U1.no);
                                    if( cancelB>0 ){ UC.CancelB=cancelB; SolFlag=true; }
                                }
                            }
                        }

                        if( eL2 is eNW_Link_InterCells ){  // ... Link
                            int no2 = eL2.noOrg;
                            UInt128 bpCom = eL2.OrgN.Aggregate_ConnectedAnd() & eL2.DesN.Aggregate_ConnectedAnd();
                            bpCom &= FreeCell81b9[no2];
                            bpCom = bpCom.DifSet(NL128).DifSet(eL2.eObject);
                            if( bpCom.IsNotZero() ){
                                foreach( var P in bpCom.IEGet_UCell(pBOARD) ) P.CancelB = 1<<no2;
                                SolFlag = true;
                                    if(debugPrintB) WriteLine( $" eL2:{eL2}  cancel no2:{no2+1}  {bpCom.ToBitString81()}");
                            }
                        }
                        eL1 = eL2;
                    }
                }

				else if( NLtype =="discontinue" ){
                    bool  UC_bivalue = UCstart.FreeBC==2;
                    string conectionType = (ULGstart.pmCnd==1? "P":"M") + (ULGend.pmCnd==1? "P":"M");
                    int noBX = 1<<ULGstart.no | 1<<ULGend.no;
                    switch(conectionType){
                        case "MP": UCstart.FixedNo = ULGstart.no+1; SolFlag=true;  break;

                        case "PM": UCstart.CancelB = 1<<ULGstart.no; SolFlag=true; break;
                            
                        case "PP":                           
                            UCstart.CancelB = 1<<ULGstart.no;
                            if( UCstart.CancelB>0 ) SolFlag = true;
                            break;    

                        case "MM":
							if( UC_bivalue ){
								UCstart.CancelB = UCstart.FreeB.DifSet(noBX);
								if( UCstart.CancelB>0 ) SolFlag = true;
							}
                            break;    
                    }
                }

                else if( NLtype =="contradiction" ){  //discontinuous
                    if( ULGstart.pmCnd==1 ){
                        UCstart.CancelB = 1<<ULGstart.no;
                        SolFlag = true;
                    }
                }
                
                else{ } //--
                return SolFlag;
            }
        }


        // ==========================================================================================================================
        private void NiceLoopEx_SolResult((string ,ULogical_Node) NL_cond, ULogical_Node ULGstart, List<eNetwork_Link> eNetList, bool complexLink ){
            string stL = eGLink_ManObj.eNetwork_ToStringB(eNetList,debugPrintB:false); //WriteLine( stL );
            stL = stL.eLinkFormatter(55,4);
            // .. .. .. .. .. .. .. result report.. .. .. .. .. .. ..
            int rc = ULGstart.rc, no=ULGstart.no, noB=1<<no;
            string st = "eNiceLoop" + (complexLink? "Ex": "");
            st += $" {NL_cond.Item1} Origin:{rc.ToRCString()}#{no+1}";     //In Continuous, the starting point has no meaning, but is used for identification.
			Result     = st;
            ResultLong = $"{st}\n\n{stL}";
			extResult  = ResultLong;
        
            // .. .. .. .. .. .. .. coloring .. .. .. .. .. .. ..
            Color  cr, crStart=Colors.LightPink, crStem=Colors.Navy;
            UCell UCstart = pBOARD[ULGstart.rc];
            UCstart.Set_CellColorBkgColor_noBit(noB,AttCr,AttCr2);
            /*if( !iSCon )*/ UCstart.Set_CellFrameColor(Colors.Blue);

            int kColor=2, kColor2=1, kColorX;
            int crLstSize = _ColorsLst.Length;
            bool simpleNL = eNetList.All(p=>(p is eNW_Link_IntraCell) || (p is eNW_Link_InterCells) ); 

            foreach( var eL in eNetList.Skip(1) ){
                if( eL is eNW_Link_IntraCell ) continue;  // Omit coloring of inter-cell links.

                ULogical_Node eLGN = eL.OrgN;
                int  noB2=1<<eLGN.no;
                kColor %= crLstSize;
                kColorX = simpleNL? kColor2: kColor++;
                cr = _ColorsLst[kColorX];
                foreach( var P in eLGN.b081.IEGet_UCell(pBOARD) ){
                    P.Set_CellColorBkgColor_noBit(noB2,AttCr,cr);
                }

                if( (eL is eNW_Link_ALS) || (eL is eNW_Link_AIC) ){
                    kColorX = simpleNL? kColor2: kColor++;
                    cr = _ColorsLst[kColorX];
                    foreach( var P in eL.eObject.IEGet_UCell(pBOARD) ){
                        P.Set_CellColorBkgColor_noBit(P.FreeB,AttCr,cr);
                    }
                }

            }
        }

        private void NiceLoopEx_SolResult_Contradiction( ULogical_Node ULGstart, eNetwork_Link eL_rail, bool complexLink ){
            eNetwork_Node eN_Des = eL_rail.eN_Des_nPM;
            var (stLM,stLP) = eGLink_ManObj.ToString_ContradictionLinks(eN_Des);  

            ULogical_Node contraURG = eN_Des.ULGNode; 
            int rc=ULGstart.rc, no=ULGstart.no, noB=1<<no, rcTo=contraURG.rc, noTo=contraURG.no;
            UCell UCstart=pBOARD[ULGstart.rc], Pcontra=pBOARD[contraURG.rc] ;

            if( ULGstart.pmCnd==1 ) UCstart.CancelB = noB;
            else  UCstart.CancelB = UCstart.FreeB.DifSet(noB);

            // ===================== result report ===========================
            string st = "eNiceLoop" + (complexLink? "Ex": "") + " contradiction" ;
			string st2 = $" Org:{_ToString_eN(ULGstart)} to {rcTo.ToRCString()}#{noTo+1}";
            Result = st+st2;
            
            string stCan = $" {ULGstart.rc.ToRCString()}#{UCstart.CancelB.ToBitStringN(9)} is excluded.";
            ResultLong = $"{st}\n{st2}\n{stCan}\n{stLP}\n{stLM}";
 			extResult  = ResultLong;

            // ===================== result coloring ===========================
            Color  cr, crStart=Colors.LightPink, crStem=Colors.Navy;
            UCstart.Set_CellColorBkgColor_noBit(noB,AttCr,AttCr2);
            UCstart.Set_CellFrameColor(Colors.Blue);

            Pcontra.Set_CellColorBkgColor_noBit(noB,AttCr,AttCr3);


			// ----- inner function -----			// @@@
			string _ToString_eN( ULogical_Node ULG){					// @@@ 594 eNetwork_App_NiceLoopEx.cs L316
				string st = $"{ULG.ToString_SameHouseComp()}#{no+1}{ToString_pmCnd()}";
				if( ULG.ID>0 )  st += $" {ULG.ID}";
				return st;
            
				string ToString_pmCnd() => ( (ULG.pmCnd==1)? "+": (ULG.pmCnd==0)? "-": "*");
			}
        }
    }  
}