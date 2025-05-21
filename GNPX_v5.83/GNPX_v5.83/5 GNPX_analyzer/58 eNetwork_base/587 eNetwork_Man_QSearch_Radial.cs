using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;
using static System.Math;
using GIDOO_space;
using System.Windows.Shapes;
using System.Net;

namespace GNPX_space {
    //using Tuple3 = (int,int,int);

	//Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/       
    //1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9   debug for eNW_DeathBlossom     
    //6.14..5.7.3.7..4..9..35.....6..1.3.415.....688.4.3..1.....94..2..2..3.4.4.6..21.3

    public partial class eNetwork_Man: ALSLinkMan{   
		static private readonly string _space10 = new string(' ',10);
		static private readonly string _space20 = new string(' ',20);
		static private readonly string _space30 = new string(' ',30);
		static private readonly string _space50 = new string(' ',50);
		

		
        public (int,eNetwork_Link) QSearch_Network_RadialType( ULogical_Node ULGstart000, int rcBreak, int extLink, UInt128 rcProhibit, bool debugPrintB=false ){ 
           
			debugPrintB = false; //@@@@@

            // ----- for develop/debug ----- 
            int next_LinksSize=0, debugIX=0, kx=0;
			eNetwork_Node.Set_withID_( withID:true );


            // ===== Prepare =====
			var eNetwork_LinkListX = eNetwork_Link_Switch( eLinksMode:extLink );
            if( (eNetwork_LinkListX is null) || eNetwork_LinkListX.Count<=0 )  return (-1,null);
														
								if(debugPrintB)  WriteLine( $"\r===#-1  -->  ULGstart:{ULGstart000}" );			 


            int solType=0; 
            eNetwork_NodeList = new();
            eNetwork_pmCnd    = new int[81];
            UInt128[] usedNode9 = new UInt128[9];
			// Set rc#no
			usedNode9[ULGstart000.no] = UInt128.One<<ULGstart000.rc;	// UInt128.Zero;  @@
            List<int> UsedLink_noDir = new();   




			// ===== [First Links]  Select the links connected to the origin node. =====
            var eN_Links = eNetwork_LinkListX.FindAll( q => !(q is eNW_Link_IntraCell) && (q.OrgN.matchKey3==ULGstart000.matchKey3) );
			eN_Links = eN_Links.DistinctBy( p=> p.hv ).ToList();
            if( eN_Links.Count<=0 )  return (-1,null); 
					if(debugPrintB)	eN_Links.ForEach( (P,kx) => WriteLine( $"+1 {kx} {P}") );


            eNetwork_Link eL_tail = null;
			eN_Links.ForEach( p => p.genNo=0 );
            List<eNetwork_Link> next_Links = new( eN_Links );

			int loop=0, nx=0;
            bool addedNextLinksF = true;



			// NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN
            while( next_Links.Count > 0 ){

			    //===== Select the next link to try.  ... Disallow used opposite direction.
				//     Assemble from simple links.  ... ex. interlink over ALS, ALS over ALSXZ)
				bool IsValidLink( eNetwork_Link L ) =>  (L.eObject & usedNode9[L.noDes])==UInt128.Zero  && (rcProhibit & L.eBody128)==UInt128.Zero;
				next_Links.RemoveAll( L => !IsValidLink(L) );	
				if( next_Links.Count == 0 )  break;

					// When added links, sort them into simple link order.
					if( addedNextLinksF ){
						addedNextLinksF = false;
										//	WriteLine( $"PreferSimpleLinks:{PreferSimpleLinks}" );
						next_Links = next_Links.DistinctBy(q=>q.keySt).ToList();
						if( PreferSimpleLinks ) next_Links.Sort( (a,b) => a.SearchOrderSimple(b) ); 
						else next_Links.Sort( (a,b) => a.SearchOrder(b) ); 
										if(debugPrintB){ WriteLine( "\n*P0 next_Links" ); next_Links.ForEach( (P,kx) => WriteLine( $"+1 {kx} {P}") ); }		
					}	




				// next Link / next node
                eNetwork_Link eLnext = next_Links.First();							// Select Link with minimum ID
                next_Links.RemoveAt(0);												// Exclude first link
									if(debugPrintB)  WriteLine( $"\nP1 debugIX:{debugIX++}  eLnext: @ {eLnext}" );

					//----- Excluding used Links(including reverse direction)
					int hv = eLnext.hv;													// link hash value, direction insensitive
					if( UsedLink_noDir.Contains(hv) )  continue;						// Exclude opposite links 
					UsedLink_noDir.Add(hv);
					next_LinksSize = next_Links.Count;

                //===== Cleared the connection conditions? Is it already registered? => if not, Interrupt.
				eNetwork_Node eNnext = eLnext.eN_Des_nPM;
				if( eLnext.eN_Des_nPM.rc == ULGstart000.rc )  continue;
//									if(debugPrintB){
//										WriteLine( $"  ====-3 --> {_space50} {eNnext} --- debugIX:{debugIX++}   {eNnext.GetType()} " );
//										eNWMan_DisplayNetworkPath( eLnext );
//									}


			  // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
                var (settedB,noF) = Set_UpstreamLinkInformation_OnNode( eLnext );
                if( noF==3 ) return (-1,eLnext);   // *** contradiction ****	
			  // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
      




				//===== Reached the target point.
				if( rcBreak>0 && eNnext.rcno==rcBreak )  return (1,eLnext);			//<<< Reached >>>


				if( eLnext.genNo>=G6.QSearchMaxGen )  continue;			// <<< Limit link length >>>


                //===== Add the links connecting to the node[eN2].
                var _ConnectedLinks = eNetwork_LinkListX.FindAll( q => __IsConnected(eLnext,q,usedNode9,ULGstart000) );
                if( _ConnectedLinks.Count==0 )  continue;
                usedNode9[eLnext.noDes] |= eLnext.eBody128;  





                //===== Consecutive intra_links is prohibited


                foreach( var eL2 in _ConnectedLinks.Where(p=>p.noDes>=0) ){ 
				//try{
					if( (eL2.eObject & usedNode9[eL2.noDes] ) > UInt128.Zero )  continue;
                    if( UsedLink_noDir.Contains(eL2.hv) )  continue;				// Excluding used Links(including reverse direction)
				//}catch(Exception e){  WriteLine( $"{e.Message}\n{e.StackTrace}" ); }
					
					eL2.genNo = eLnext.genNo+1;
                    next_Links.Add(eL2);    //Add next links to list. Exclude used links (reverse direction is also possible).
					addedNextLinksF = true;

					if(debugPrintB)  WriteLine( $"   P2 next_Links.Add debugIX:{debugIX++}  @+ {eL2}" );

					if( PreferSimpleLinks ){
						eNetwork_Link L0 = next_Links.First();
						addedNextLinksF = (L0.lkCode>21) || (L0.genNo!=eLnext.genNo);	//(interLink_size2) || different branch order
					}
				}
            }
            return (-1,null);


        // ---------- inner function ----------
            bool __IsConnected( eNetwork_Link eLpre, eNetwork_Link eL2, UInt128[] usedNode9, ULogical_Node ULGstart000 ){
				if( eL2.eN_Des_nPM.rc == ULGstart000.rc )  return false;    // exclude node (start_node)

                UInt128 matchKey3_eLtail = eLpre.DesN.matchKey3;    // rc,no,pmCnd
              //bool chk = eLpre.matchKey3_Des == eL2.matchKey3_Org;
				bool chk = eLpre.DesN.matchKey3 == eL2.OrgN.matchKey3;
                if( !chk )  return false;
                if( (eLpre is eNW_Link_IntraCell) && (eL2 is eNW_Link_IntraCell) )  return false;
        //        if( (eL2 is eNW_Link_ALS || eL2 is eNW_Link_ALSM ) && eL2.Size==1 ) return false;

                if( (usedNode9[eL2.noOrg]&eL2.eBody128).IsNotZero() )  return false;
                return true;
            }
        }
    }
}
                        