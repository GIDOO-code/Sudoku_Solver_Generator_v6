using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPX_space {
        
	//Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/       
    //6.14..5.7.3.7..4..9..35.....6..1.3.415.....688.4.3..1.....94..2..2..3.4.4.6..21.3

    public partial class eNetwork_Man: ALSLinkMan{       

        private int QSearch_ReconfigureALS( bool debugPrintB=false ){
            Prepare_ALSLink_Man( nPlsB:1, setCondInfo:true, debugPrintB:false );         
            if( ALSList is null ) return 0;     //AALSList: Almost ALS

            List<eNetwork_Link> eALSrecong_List = new();

			foreach( var qALS in ALSList.Where(p=> p.Level==1 && p.Size>=4) ){

				foreach( var noOrg in qALS.FreeB.IEGet_BtoNo(9) ){
					int noOrgB = 1<<noOrg;
					UInt128 cand81 = FreeCell81b9[noOrg].DifSet(qALS.bitExp);	

					ALS_Reconfigure_initialSetting( qALS, noOrg );
					bool reconfiguredB =  ALS_Reconfigure( qALS, noOrg, debugPrintB:debugPrintB );
					if( !reconfiguredB )  continue;

					List<ULogical_Node> ULG_OrgList = _Generate__ULG_OrgList(qALS, noOrg, cand81, debugPrintB:debugPrintB);	
					if( ULG_OrgList==null || ULG_OrgList.Count==0 )  continue;
					
					List<ULogical_Node> ULG_DesList = _Generate_ULG_DesList(qALS);

					foreach( var ULGorg in ULG_OrgList ){
						foreach( var ULGdes in ULG_DesList.Where(p=>(ULGorg.b081&p.b081)==0) ){
							eNW_Link_refALS R = new( qALS, ULGorg, ULGdes );
							Set_eGLink( R, debugPrintB, eALSrecong_List );
								if(debugPrintB)  WriteLine( R );
						}
					}
				}
			}

			eALSrecong_List = eALSrecong_List.DistinctBy( p=> p.hv ).ToList();
			if(debugPrintB) eALSrecong_List.ForEach( (P,kx) => WriteLine( $"eNW_Link_refALS {kx} {P}" ) );

			eNetwork_LinkList.AddRange(eALSrecong_List);
			return eALSrecong_List.Count();

		// ==============================================================================================

		#region inner_function

			// ===== inner function =====
				List<ULogical_Node> _Generate__ULG_OrgList( UAnLS qALS, int noOrg, UInt128 cand81, bool debugPrintB=false ){
					List<ULogical_Node> ULG_OrgList = new();
					var qALS_rcbAnd9_Org = qALS.rcbAnd9[noOrg];
					if( qALS_rcbAnd9_Org == 0 )  return null;
							//WriteLine( $"qALS_rcbAnd9_Org:{qALS_rcbAnd9_Org.rcbToBitString27()}" );

					foreach( int hOrg in qALS_rcbAnd9_Org.IEGet_BtoHouse27() ){
						UInt128 rc81h = cand81 & HouseCells81[hOrg];
						foreach( var ULG in Generate_ULG(noOrg, rc81h, pmCnd:1) ){
							ULG_OrgList.Add( ULG );
								if(debugPrintB)  WriteLine( $" noOrg:#{noOrg+1}  hOrg:{hOrg:00}-- origin {ULG}" );
						}
					}
					ULG_OrgList =  ULG_OrgList.DistinctBy(p=> p.matchKey3).ToList();
								if(debugPrintB){
									WriteLine("** ULG_OrgList **");
									ULG_OrgList.ForEach( (p,mx) => WriteLine( $"{mx} {p}" ) );
								}
					return ULG_OrgList;
				}

				List<ULogical_Node> _Generate_ULG_DesList( UAnLS qALS ){
					List<ULogical_Node> ULG_DesList = new();

					// Targets within ALS-1
					foreach( UCell UC in qALS.UCellLst ){  //fixed							
						int noDes = UC.FreeBwk.BitToNum();
						if( noDes < 0 )  continue;
						ULogical_Node URGdes1 = new( no:noDes, rc:UC.rc, pmCnd:1 );
						ULG_DesList.Add( URGdes1 );
									//WriteLine( $"  -1- URGdes1:{UC}  URGdes1:{URGdes1}" );
					}
							
					// Targets within ALS-2
					foreach( UCell UC in qALS.UCellLst.Where(p=>(p.FreeB.DifSet(p.FreeBwk))>0) ){  //canceled
						int noDesB = UC.FreeB.DifSet(UC.FreeBwk);
						foreach( int noDes in noDesB.IEGet_BtoNo() ){
							ULogical_Node URGdes2 = new( no:noDes, rc:UC.rc, pmCnd:0 );
							ULG_DesList.Add( URGdes2 );
									//WriteLine( $"  -2- URGdes1:{UC}  URGdes2:{URGdes2}" );
						}

					}
							
					// Targets outside of ALS
					foreach( int noDes in qALS.FreeBwk.IEGet_BtoNo() ){
						var qALS_rcbAnd9wk_Des = qALS.rcbAnd9_wk(noDes);
						foreach( int hDes in qALS_rcbAnd9wk_Des.IEGet_BtoHouse27() ){
							UInt128 rc81DesOut = (FreeCell81b9[noDes] & HouseCells81[hDes]).DifSet(qALS.BitExp9[noDes]);
							if( rc81DesOut == 0 ) continue;
							foreach( var ULGdes in Generate_ULG( noDes, rc81DesOut, pmCnd:0 ) ){
								ULogical_Node URGdes3 = new( noB9:1<<noDes, b081:rc81DesOut, pmCnd:0 );
								ULG_DesList.Add( URGdes3 );
									//WriteLine( $"  -3- noDes:#{noDes+1}  hDes:{hDes:00}  URGdes3:{URGdes3}" );
							}
						}
					}
					ULG_DesList = ULG_DesList.DistinctBy(p=> p.matchKey3).ToList();
					if(debugPrintB){
						WriteLine("** ULG_DesList **");
						ULG_DesList.ForEach( (p,mx) => WriteLine( $"{mx} {p}" ) );
					}
					return ULG_DesList;
				}

				IEnumerable<ULogical_Node> Generate_ULG( int no, UInt128 ULGbase81, int pmCnd ){
					List<int> rcLst = ULGbase81.IEGet_rc().ToList();
					int sz =rcLst.Count;
				
					List<ULogical_Node> LogNDs = new();
					for( int n=1; n<=sz; n++ ){
						Combination cmb = new( sz, n );
						while( cmb.Successor() ){
							UInt128 rc81 = UInt128.Zero;
							for( int k=0; k<n; k++ )  rc81 = rc81.Set( rcLst[cmb.Index[k]] );
							ULogical_Node LogND = new( noB9:1<<no, b081:rc81, pmCnd:pmCnd, ID:0);
							LogNDs.Add(LogND);
						}
					}
					LogNDs = LogNDs.DistinctBy(p=>p.matchKey3).ToList();  //keys: no, b081, pmCnd (not include ID)

					foreach( ULogical_Node ULG in LogNDs )  yield return ULG;
					yield break;
				}
		#endregion inner_function

		}
    }
}
                        