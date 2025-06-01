using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;


namespace GNPX_space{

    public partial class eNetwork_App: AnalyzerBaseV2{      

/*
    Force-based algorithms use the current links.
    The chain is assembled using this links, and the pmCnd or false of the cell candidate is logically derived.

    The Force algorithm is based on the following logic.

     (1) Set X has one element true and the rest false. Which is true is undecided.
     (2) In a chain starting with true element, the value of the derived element is determined to be true or false.
     (3) In a chain starting with false element, the value of the derived element is uncertain (it can be true or false).
     (4) For each chain that starts assuming each element of set ‡] as true, 
         the authenticity of element A is determined when the true/false values ??of element A leading by all chains match.
     (5) In the chain that starts assuming that one element B of set X is true, 
         when the true/false values ??of the element C guided by multiple routes do not match,
         the starting element B is determined to be false. 

     https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/
     https://github.com/GIDOO-code/Sudoku_Solver_Generator/tree/master/docs/page56.html
 */ 


// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==
//   ForceChain ver.5.0   (ForceChain ver4.0 is abolished.)
// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==
        private (int,int,int,int,int) Func_ToRcNo(int V) => ( (V>>28)&0xF, (V>>20)&0xFF, (V>>16)&0xF, (V>>4)&0xFF, V&0xF );
		private int	Func_RcNoToV(int rc, int no ) => rc<<4 | no;
		private int	Func_RcNoToV(int OrgRc, int OrgNo, int DesRc, int DesNo) => Func_RcNoToV(OrgRc,OrgNo)<<16 | Func_RcNoToV(DesRc,DesNo);

     // private bool    ForceChain_on     => G6.ForceChain_on"];
     // private bool    showPrfMltPathsB  => G6.chbx_ShowProofMultiPaths"];
        private string  ForceChain_Option => G6.ForceLx;    // Exploration level

        private	List<string>  extStLst = new List<string>();

        private List<int> OrgDes_rcno = new();
        private int FCstageNoPMemo = -9;
       	private int stageNoForceMemo = -9;

        public bool eNetwork_ForceChain_prepare( ){

			if( stageNoP==stageNoForceMemo )  return true;			
            stageNoForceMemo = stageNoP;

          //==============================
            bool debugPrintB = false;
          //------------------------------
		    pAnMan.solutionC = 0;
            //extResult = "";
            extStLst.Clear();

			bool retPrepare = eNetwork_Dev5_Prepare( nPls:3, minSize:2 );
            if( !retPrepare ) return false;

            if( FCstageNoPMemo == stageNoP )  return false;
            FCstageNoPMemo = stageNoP;
			//base.AnalyzerBaseV2_PrepareStage();

            // ========== QSearch_CreateNetwork ==============================
            OrgDes_rcno.Clear();

			int searchCC = pBOARD.Sum(p=>p.FreeBC), serchKK=0;
            foreach ( var UCOrg in pBOARD.Where(p=>(p.FreeB>0)) ){	//----- origin rc
				foreach( var noOrg in UCOrg.FreeB.IEGet_BtoNo() ){	//----- origin no
					AdditionalMessage = $" ... {++serchKK} / {searchCC}";

                    ULogical_Node ULG_start = new( no:noOrg, rc:UCOrg.rc, pmCnd:1 );
                    eGLink_ManObj.QSearch_Network_RadialType( ULG_start, rcBreak:-1, extLink:1, rcProhibit:UInt128.Zero, debugPrintB:debugPrintB );
                    _Set_NnetworkEndpointValues( UCOrg.rc, noOrg );
                }
            }

            return true;

					// --------------- inner function ---------------
					List<int> _Set_NnetworkEndpointValues( int Orgrc, int noOrg ){
						int Org_rcno = ( (Orgrc<<4)|noOrg) <<16;
						foreach ( var UCDes in pBOARD.Where(p=>(p.FreeB>0)) ){
							int tfFlag = eGLink_ManObj.eNetwork_pmCnd[UCDes.rc];
							if( tfFlag == 0 )  continue;
							foreach ( var no in UCDes.FreeB.IEGet_BtoNo() ){
								int pm = (tfFlag>>(no*2))&0x3;
								if( pm <= 1 )  continue;    // pm=  0:-  1:minus  2:plus  3:(minus&plus) -> Contradiction
										// WriteLine( Get_eNetworkPath(no,UCDes.rc,pmCnd:1) );
								int _rcno = (pm<<28) | (Org_rcno) | ((UCDes.rc<<4)|no);
								if( OrgDes_rcno.Contains(_rcno) ) continue;
								OrgDes_rcno.Add( _rcno );
							}
						}
						return OrgDes_rcno;
					}
        }

        private string eNetwork_ToStringA( List<eNetwork_Link> eNetwork ){
            string st = "";
            if( eNetwork is null )  return st;
            bool ft=true; // Only at first true.
            eNetwork.ForEach( L => {
                string st2 = L.ToString2(ft); ft=false;
                st += "\n"+ st2;
            } ); 
            return  st;
        }

        private eNetwork_Node Get_eNetwork_Proof( int pmCnd, int OrgDesVal, UInt128 rcProhibit, bool debugPrintB=false ){
            var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(OrgDesVal);

            int rcnoBreak=OrgDesVal&0xFFFF; // <<< currently operating under ththis conditions.
				   // if( pmCnd==0 ){ rcnoBreakF=OrgDesVal&0xFFFF; }		
				   // if( pmCnd==1 ){ rcnoBreakT=OrgDesVal&0xFFFF; }		// <<< [ATT] Unimplemented >>>

            ULogical_Node ULG_start = new( no:Ono, rc:Orc, pmCnd:1 );            
						if( debugPrintB ){
							WriteLine( $" Origin:{Orc.ToRCString()}#{Ono+1}  =>  Destination:{Drc.ToRCString()}#{Dno+1}" );
							WriteLine( $"ULG_start:{ULG_start}" );
						}

            var (retCode,eL) = eGLink_ManObj.QSearch_Network_RadialType( ULG_start, rcnoBreak, extLink:1, rcProhibit:rcProhibit, debugPrintB:debugPrintB );
			if( eL == null )  return null;
						if(debugPrintB)  ToString_eNetwork_Proof( eL.eN_Des_nPM, writeB:true );
				/*
						if(debugPrintB){
							var _eNLst = eGLink_ManObj.Get_eNetwork_OrgToChain( eL.eN_Des_nPM );  // generate when needed <... for debug
							string stSol = eNetwork_ToStringA(_eNLst);
							WriteLine( stSol );
						}
				*/
            return  eL.eN_Des_nPM;
        }

		private string ToString_eNetwork_Proof( eNetwork_Node eNX, bool writeB ){
			var _eNLst = eGLink_ManObj.Get_eNetwork_OrgToChain( eNX );
			string stSol = eNetwork_ToStringA(_eNLst);
			if(writeB)  WriteLine( stSol );
			return stSol;
		}

    }
}
