using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Windows.Shapes;
using System.Net;
using System.Runtime.CompilerServices;

namespace GNPX_space {

    public partial class eNetwork_Man: ALSLinkMan{         
        static public int[]  _hushBase = new int[8192]; 
				static eNetwork_Man(){
					// The initial value of the random number is fixed.
					// ->  The same random number is always used.
					// ->  Reproducibility is maintained even when debugging.
					Random _rnd_ = new Random(1);
					for( int k=0; k<_hushBase.Length; k++ )  _hushBase[k] = _rnd_.Next();
				}


        private int   debugCCX = 0;

        public int[]  eNetwork_pmCnd;


        public (bool,int) Set_UpstreamLinkInformation_OnNode( eNetwork_Link eLX ){
            int _Func_pmCndNo( int pmCnd, int no ) => 1<< (pmCnd+no*2);
            
			eNetwork_Node eN = eNetwork_Man.Get_NodeRef(eLX.eN_Des_nPM);
			int eN_Index = eNetwork_Man.Get_NodeRef_Index(eLX.eN_Des_nPM);

			if( eN==null || eN.ULGNode==null )  return (false,0);

			bool rcIsOne = eN.ULGNode.b081.BitCount()==1;
			int  rc = eN.rc;
			int  no = eN.no;
			int  pm = eLX.DesN.pmCnd;

            bool settedB = false;
			if( pm==0 && eN.eChaineNtwk_pre_Minus==null ){
				eN.eChaineNtwk_pre_Minus = eLX;		// Set Upstream Link Infomation
				if(rcIsOne) eNetwork_pmCnd[rc] |= _Func_pmCndNo(0,no);

					if( rc>=0 ){
						int noXX = (eNetwork_pmCnd[rc]>>(no*2)) & 0x3;
						string stX = $"=== {noXX} ===  {((noXX==3)? "*****": "")}" ;
								//WriteLine( $"{stX}  {eN.eChaineNtwk_pre_Minus}" );
					}
				settedB=true; 
			}

			if( pm==1 && (eN.eChaineNtwk_pre_Plus==null) ){


				eN.eChaineNtwk_pre_Plus = eLX;		// Set Upstream Link Infomation 
				if(rcIsOne) eNetwork_pmCnd[rc] |= _Func_pmCndNo(1,no);  

					if( rc>=0 ){
						int noXX = (eNetwork_pmCnd[rc]>>(no*2)) & 0x3;
						string stX = $"=== {noXX} ===  {((noXX==3)? "*****": "")}" ;
								//WriteLine( $"{stX}  {eN.eChaineNtwk_pre_Minus}" );
					}
				settedB=true; 
			}
			var rcNoF = 0;
			if( rcIsOne )  rcNoF = eNetwork_pmCnd[rc]>>(no*2) & 0b11;
            return  ( settedB, rcNoF );		
        }


        private eNetwork_Node IsContradiction( eNetwork_Link eL, bool debugPrintB=false ){        
            eNetwork_Node eNdes = eL.eN_Des_nPM;
            foreach( var rc in eNdes.ULGNode.b081.IEGet_rc(81) ){
                UCell  U = pBOARD[rc]; 
                int _tno = eNetwork_pmCnd[U.rc];
                if( _tno == 0 )  continue;

				// ... contradiction ...
				foreach( int no in U.FreeB.IEGet_BtoNo() ){ 
                    int tfFlag = (_tno>>(no*2))&3;
                    if(tfFlag==3){
								if( debugPrintB ){
									WriteLine( _tno.ToBitString(18) );
									for( int pm=0; pm<2; pm++ ){
										ULogical_Node ULG = new ULogical_Node( no:no, rc:rc, pmCnd:pm);	
									  //var eNX = eNetwork_Man.eNetwork_NodeList.Find(p => p.matchKey2==ULG.matchKey2);
										var eNX = eNetwork_Man.Get_NodeRef(ULG);
										var (hv,_eNList) = Get_eNetwork_OrgToChain( eNX, pmCnd:pm );                            
										if( _eNList==null || _eNList.Count<=0 )  continue;

										WriteLine( eNetwork_ToStringB(_eNList, debugPrintB:true) );
										DebugPrint_preLink( eNX );
									}		
								}
						return eNdes;
                    }
                }
            }
            return null;  // >=0 : Contradiction
                  
            void DebugPrint_preLink( eNetwork_Node eN2 ){
                string stX = $"-->>> CheckAndSet_Node/DebugPrint"; 
                stX += "   M:" + ((eN2.eChaineNtwk_pre_Minus!=null)? eN2.eChaineNtwk_pre_Minus.ToString(): "@@@@@@null");
                stX += "   P:" + ((eN2.eChaineNtwk_pre_Plus!=null)?  eN2.eChaineNtwk_pre_Plus.ToString():  "@@@@@@null");         
                WriteLine( stX ); 
            }
        }
  
		private string eNetwork_ToStringA( List<eNetwork_Link> eNetwork ){
            string st = "";
            if( eNetwork is null )  return st;
            bool ft=true; // Only at first true.
            eNetwork.ForEach( L => {
                string st2 = L.ToString2(ft); ft=false;
                st += "\r"+ st2;
            } ); 
            return  st;
        }

        public string eNetwork_ToStringB( List<eNetwork_Link> eNetworkList, bool debugPrintB=true ){
            string st = "";
            if( eNetworkList is null )  return st;
            bool ft=true; // Only at first true.
            eNetworkList.ForEach( L => {
                string st2 = L.ToString2B(ft); ft=false;
                st += st2;
            } ); 
            if( st=="" )  st = " (null)";
            else{
				var DesN = eNetworkList.Last().DesN;
				if( DesN == null ){ st += " ... ?????";  WriteLine(st); }
            }
            return  st;
        }


    }
}
                        