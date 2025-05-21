using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.Windows;

namespace GNPX_space {
    //using Tuple3 = (int,int,int);

    public partial class eNetwork_Man: ALSLinkMan{   

        private List<eNetwork_Link> eNetwork_LinkList_Cell;

        public List<eNetwork_Link> eNetwork_Link_Switch( int eLinksMode=0 ){
            if( eLinksMode < 0 ){
                eNetwork_LinkList_Cell = eNetwork_LinkList.FindAll(P=> (P is eNW_Link_IntraCell) || (P is eNW_Link_InterCells) );
                return null;
            }
            if( eLinksMode==0 ) return  eNetwork_LinkList_Cell;
            return eNetwork_LinkList;
        }


        //* == * == * == * == * == * == * == * == * == * == * == * == * == * == * == * == *
        // Radial loop search algorithm:
        //   The search begins with a link.
        //   Leave a return path(link) to close the loop.
        //* == * == * == * == * == * == * == * == * == * == * == * == * == * == * == * == *
        //
/*
		private bool IsConnected( bool firstL, ULogical_Node ULG, eNetwork_Link LK ){
			if( firstL ){
				int no=ULG.no, rc=ULG.rc;
				 bool conB = !(LK is eNW_Link_IntraCell) && (LK.OrgN.matchKey3==ULG.matchKey3);
				 if( !conB )  return false;

				 if( (LK.OrgN.b081 & ConnectedCells81[rc]).IsNotZero() )  return false;
				 UInt128 b081b9 = FreeCell81b9[no] & LK.OrgN.b081;
				 if( b081b9.DifSet( ConnectedCells81[rc] ).IsNotZero() )  return false;
				 return true;
			}

		}
*/



        public (int,eNetwork_Link) QSearch_Network_LoopType( ULogical_Node ULGstart000, int rcBreak, int extLink, bool suspendOnB=false, bool debugPrintB=false ){ 

            var eNetwork_LinkListX = eNetwork_Link_Switch( eLinksMode:extLink );

            if( (eNetwork_LinkListX is null) || eNetwork_LinkListX.Count<=0 )  return (-1,null);
					if(debugPrintB)  WriteLine( $"\r====-1  -->  ULGstart:{ULGstart000}" );

            int solType=0; 

            // ----- for develop/debug -----
            int next_LinksSize=0, debugIX=0;
			UInt128 ULGstart000_matchKey3 = ULGstart000.matchKey3;

         //   var eN_Links = eNetwork_LinkListX.FindAll( L => !(L is eNW_Link_IntraCell) && (L.matchKey3_Org==ULGstart000_matchKey3) );
			var eN_Links = eNetwork_LinkListX.FindAll( L => (L is eNW_Link_InterCells) && (L.OrgN.matchKey3==ULGstart000_matchKey3) );


            if( eN_Links.Count<=0 )  return (-1,null); 
			if( ULGstart000.pmCnd==0 && eN_Links.Count>=2 )  return (-1,null);
					if(debugPrintB)  eN_Links.ForEach( (P,mx) => WriteLine( $"eN_Links[{mx}]: {P}" ) );



            eNetwork_Link eL_tail = null;
            foreach( var firstLink in eN_Links ){       // The first link is not eNW_Link_IntraCell.

                            if(debugPrintB)  WriteLine( $"\n=== ===@1 firstLink:{firstLink}  ID:{firstLink.ID}" );

                // ===== Prepare =====    
                solType=0; 
                eNetwork_NodeList.Clear();
                eNetwork_pmCnd = new int[81];
	            UInt128   usedNode = UInt128.Zero;	 // @ bit representation
                List<int> UsedLink_List = new();     // @ direction insensitive

/* @@ */
				usedNode |= ULGstart000.b081;
/* @@ */

                // ===== Set Initial condition =====
				firstLink.genNo = 0;
                Set_UpstreamLinkInformation_OnNode( firstLink );		// Initialization is required. @@@@@@@

                            if(debugPrintB){ eNWMan_DisplayNetworkPath( firstLink, debugBPM:true ); }


                int hv_firstLink = firstLink.hv;  // Link hash value, direction insensitive. is not eNW_Link_IntraCell.
                UsedLink_List.Add(hv_firstLink);            // Exclude the reverse direction of the first link.
					if(debugPrintB) WriteLine( $"firstLink.OrgN.b081:{firstLink.OrgN.b081} {firstLink.OrgN.b081.ToBitString81()}");
					//WriteLine( $"                 usedNode:{usedNode}");


                UInt128  matchKey3 = firstLink.DesN.matchKey3;
                List<eNetwork_Link> next_Links = eNetwork_LinkListX.FindAll( LX => 
						(LX.OrgN.matchKey3==matchKey3) & !UsedLink_List.Contains(LX.hv) & (ULGstart000.b081&LX.DesN.b081)==0 ); 

                if( next_Links.Count<=0 )  continue;
						if(debugPrintB)  next_Links.ForEach( (P,kx) => WriteLine( $"{kx}: {P}" ) );

                bool firstB = true;
				int loop=0;

				int dvX=0;
				bool addedNextLinksF = true;
                while( next_Links.Count > 0 ){
					++loop;
							if(debugPrintB) WriteLine( $"\n\n====== loop:{loop}  dvX:{dvX}   next_Links.Count:{next_Links.Count} ============" );

                    eNetwork_Link eL = null;
							if(debugPrintB)  next_Links.ForEach( (P,mx) => WriteLine( $" next_Links: sq:{mx} {P}" ) );
 
                    //Select the next link to try (disallow used opposite direction)
                    {  // Assemble from simple links (ex. interlink over ALS, ALS over ALSXZ)
						
                        if( addedNextLinksF ){
							if( PreferSimpleLinks ) next_Links.Sort( (a,b) => a.SearchOrderSimple(b) ); 
							else next_Links.Sort( (a,b) => a.SearchOrder(b) ); 
						}
		
                        eL = next_Links.First();                        // Select Link with minimum ID
                        next_Links.RemoveAt(0);                         // Exclude first link
                              if(debugPrintB)  WriteLine( $"   ==== @2  --> {eL} ---        debugIX:{debugIX++}    {eL.GetType()}" );

                        // Excluding used Links(including reverse direction)
                        int hv = eL.hv;                      // link hash value, direction insensitive
                        if( UsedLink_List.Contains(hv) )  continue;    // Exclude opposite links  ”½‘ÎŒü‚«‹ÖŽ~‚ª•sŠ®‘S
                        UsedLink_List.Add(hv);

                        next_LinksSize = next_Links.Count;

                        // Cleared the connection conditions? Is it already registered? => if not, Interrupt.
                        var (settedB,rcnoF) = Set_UpstreamLinkInformation_OnNode( eL );	// @@@@@@@@@
                        if( !settedB )  continue;
						if( debugPrintB && rcnoF==3 ){ WriteLine( $"\n\n\n@@@@ contradiction @@@@@\n\n\n" ); }
                    } 

                    // == == == == == == == == == == == == == == == == == == == == == == == ==
                    // eN -- eL --eN2 ( eNetwork_Node - eNetwork_Link - eNetwork_Node )
                    eNetwork_Node eN2 = eL.eN_Des_nPM;
							if(debugPrintB){
								WriteLine( $"         ==== @3   -->  {eN2}  ---  debugIX:{debugIX++}    {eN2.GetType()} " );
								eNWMan_DisplayNetworkPath( eL, debugBPM:true );
							}

                    // If there is a conflict (both True/False routes), proceed to the end process. This may also occur during the route.
                    eNetwork_Node eNcontra  = IsContradiction( eL, debugPrintB:debugPrintB );		//@@@@@                
                    if( eNcontra!=null ){		//contradiction.
                        solType=3; eL_tail=eL; 
                            if(debugPrintB){
								var (conP,conM) = ToString_ContradictionLinks( eNcontra );
					 			WriteLine( $"contradiction\n  {conP}\n {conM}" );
					 		}
                        goto LSearchBreak;
                    }
                    // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

                    if( rcBreak>=0 && eL.DesN.rc==rcBreak ){
					#region found looping 
							if(debugPrintB){ int nx=0; eNWMan_DisplayNetworkPath( eL, debugBPM:true ); }

                        eL_tail = eL;
                        solType = eL.DesN.pmCnd+1;
                        if( eL.DesN.no!=ULGstart000.no ){
                            var eL2X = TerminationProcessing( ULGstart000, eL, debugPrintB:debugPrintB );    //@@@@@@@@@@@
                            if( eL2X != null ){    
                                var (settedB,rcnoF) = Set_UpstreamLinkInformation_OnNode( eL2X );
														
										if( debugPrintB && rcnoF==3 ){ WriteLine( $"\n\n\n@@@@ contradiction @@@@@\n\n\n" ); }

                                solType = eL2X.DesN.pmCnd+1;
                                eL_tail = eL2X;
                                
                                if( ULGstart000.matchKey2==eL_tail.DesN.matchKey2 && ULGstart000.pmCnd!=eL_tail.DesN.pmCnd ){
                                    // An inconsistent state occurred due to the link added in the loop.
                                    solType=3; 
                                        if(debugPrintB)  WriteLine( $"\n\ncontradiction ULGstart000:{ULGstart000}  eL_tail:{eL_tail}" );
                                }
                            }
                        }
					#endregion
                        goto LSearchBreak;
                    } 
                    // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

					if( IsAnyUCellDuplicate( eN2, rcBreak ) )  continue; 

                    // Add the links connecting to the node[eN2].
	                var _ConnectedLinks = eNetwork_LinkListX.FindAll( q => __IsConnected(eL,q,usedNode) );
                    if( _ConnectedLinks.Count==0 )  continue;
	                usedNode |= eL.eBody128;  

                    // Consecutive intra_links is prohibited.									
					addedNextLinksF = false;
                    foreach( var eL2 in _ConnectedLinks ){ 
                        int hv2 = eL2.hv;                   // eNW_Link_IntraCell has a different hash value definition
                        if( UsedLink_List.Contains(hv2) )  continue;   // Excluding used Links(including reverse direction)
						if( !IsExistNextLink(eL) )  continue;      //@@## ___test___
						eL2.genNo = eL.genNo+1;
                        next_Links.Add(eL2);    //Add next links to list. Exclude used links (reverse direction is also possible).
						addedNextLinksF = true;
                    }
                }
            }

            return (-1,null);

          LSearchBreak:
            return (solType,eL_tail);
        //============================================================================================================================


        // ---------- inner function ----------
			bool IsExistNextLink( eNetwork_Link el ){
				int no = el.DesN.no;
				int rcb = el.DesN.b081.Ceate_rcbFrameAnd();
				if( rcb==0 )  return false;

				foreach( int h in rcb.IEGet_BtoHouse27() ){
					if( (FreeCell81b9[no] & HouseCells81[h]) > UInt128.Zero )  return true;
				}
				
				return  false;
			}
			bool IsExistNextLinkPlus( eNetwork_Link el, UInt128 usedNode ){
				int no = el.DesN.no;
				int rcb = el.DesN.b081.Ceate_rcbFrameAnd();
				if( rcb==0 )  return false;

				UInt128 freeBoard = FreeCell81b9[no].DifSet(usedNode);
				foreach( int h in rcb.IEGet_BtoHouse27() ){
					if( (freeBoard & HouseCells81[h]) > UInt128.Zero )  return true;
				}
				return  false;
			}

            eNetwork_Link TerminationProcessing( ULogical_Node ULGstart, eNetwork_Link eL, bool debugPrintB=false ){
                ULogical_Node  ULGend = eL.DesN;
                if( ULGend.pmCnd==0 || pBOARD[ULGend.rc].FreeBC>2 )  return null;
                    //CheckAndSet_Node( eL, debugPrintB:debugPrintB );
                eNetwork_Link eL2T = eNetwork_LinkListX.Find( q => (q is eNW_Link_IntraCell) && (q.OrgN.matchKey3==ULGend.matchKey3) );
                return eL2T;
            }

            bool __IsConnected( eNetwork_Link eLpre, eNetwork_Link eL2, UInt128 usedNode ){
                UInt128 matchKey3_eLtail = eLpre.DesN.matchKey3;    // rc,no,pmCnd
                bool chk = (matchKey3_eLtail == eL2.OrgN.matchKey3);
                if( !chk )  return false;
                if( (eLpre is eNW_Link_IntraCell) && (eL2 is eNW_Link_IntraCell) )  return false;
       //       if( (eL2 is eNW_Link_ALS || eL2 is eNW_Link_ALSM ) && eL2.Size==1 ) return false;

                if( (usedNode&eL2.eBody128).IsNotZero() )  return false;
                return true;
            }
		}

        public (string,string) ToString_ContradictionLinks( eNetwork_Node eNX ){//  int rc, int no ){
			string[] stL = new string[2];
            for( int pm=0; pm<=1; pm++ ){
				var ULG = new ULogical_Node( no:eNX.no, rc:eNX.rc, pmCnd:pm);
			  //var eNX0 = eNetwork_Man.eNetwork_NodeList.Find( p => p.matchKey2==ULG_matchKey2);
				var eNX0 = eNetwork_Man.Get_NodeRef(ULG);
                var (_,_eNList) = Get_eNetwork_OrgToChain( eNX0, pmCnd:pm );
                stL[pm] = eNetwork_ToStringA(_eNList); 
            }
			return (stL[0],stL[1]);
        }

    }
}
                        