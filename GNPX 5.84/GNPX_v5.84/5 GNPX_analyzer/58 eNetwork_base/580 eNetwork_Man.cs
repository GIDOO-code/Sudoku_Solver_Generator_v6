using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Windows.Documents;
using System.Diagnostics;

namespace GNPX_space {

    public partial class eNetwork_Man: ALSLinkMan{   
	    static private readonly UInt128 qZero  = UInt128.Zero;
        static private readonly UInt128 qOne   = UInt128.One;
        static private readonly UInt128 _filter_b081 = (UInt128.One<<81)-1;  //b081    0 -  80 (81)

//        static public  UInt128[] Board_BitRep;
        static public List<eNetwork_Link> eNetwork_LinkList;
        static public List<eNetwork_Node> eNetwork_NodeList = new();

		static public eNetwork_Node Get_NodeRef( eNetwork_Node eN ) => Get_NodeRef(eN.ULGNode);
		static public eNetwork_Node Get_NodeRef( ULogical_Node ULG ) => eNetwork_NodeList.Find(p => p.matchKey2==ULG.matchKey2);


		static public int Get_NodeRef_Index( eNetwork_Node eN ) => Get_NodeRef_Index(eN.ULGNode);
		static public int Get_NodeRef_Index( ULogical_Node ULG ) => eNetwork_NodeList.FindIndex(p => p.matchKey2==ULG.matchKey2);


        private Queue<eNetwork_Node> QSearchQue = new();

        private Dictionary<UInt128,int> DefinedNodes = new();

        private int bpTNO(int no, int pmCnd) => 1<< (pmCnd+no*2);
        //==============================
        private bool debugPrintB=false;  //true;
        //------------------------------


		// ===== Constructor =====
        public eNetwork_Man( GNPX_AnalyzerMan pAnMan ): base(pAnMan){
            eNetwork_Link.pAnMan = pAnMan;
            eNetwork_Node.pAnMan = pAnMan;
        }

		


        public bool IsAnyUCellDuplicate( eNetwork_Node eN0, int rcBreak=-9 ){
			// rcnoBreak : Specify if duplicate origins are allowed. ...

			bool OLFlag=false;
			UInt128 eN0b081 = eN0.ULGNode.b081;

            var eNX = eN0;
			int loopC = 0;
            while( eNX!=null && (++loopC)<18 ){
                eNetwork_Link eL = eNX.eChaineNtwk_pre_Plus ?? eNX.eChaineNtwk_pre_Minus;
                if( eL == null ) return false;
                eNX = eL.eN_Org_nPM;
                if( eNX.rc==rcBreak )  return false;
                if( (eNX.ULGNode.b081 & eN0b081) > UInt128.Zero )  return true;
            }
            return false;
        } 


        private int IsConnected_withRCC( UAnLS UA, UAnLS UA2, int without_X=-1 ){
            int nRCC=0, RCC_BitExp=0;
					//WriteLine( $" UA:{UA}\nUA2:{UA2}" );

			int  FreeBX = (without_X>0)? without_X: (UA.FreeBwk & UA2.FreeBwk);
            foreach( var no in FreeBX.IEGet_BtoNo() ){  
				int rcbB = UA.rcbAnd9_wk(no) & UA2.rcbAnd9_wk(no);
				bool RCC1 = (rcbB&0x1FF).BitCount()==1 || ((rcbB>>9)&0x1FF).BitCount()==1 || ((rcbB>>18)&0x1FF).BitCount()==1;

                if( !RCC1 )  continue;
					//WriteLine( $"          rcbB:{rcbB.rcbToBitString27()}" );
                nRCC++;
                RCC_BitExp |= (1<<no); 
            }
            return RCC_BitExp;
        }
		
	// NWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNWNW

		public List<eNetwork_Link> Get_eNetwork_OrgToChain( eNetwork_Node eNX ){
			if( eNX.eChaineNtwk_pre_Minus!=null && eNX.eChaineNtwk_pre_Plus!=null ){
				var (_,eNListM) = Get_eNetwork_OrgToChain( eNX, pmCnd:0 );      // pmCnd:0   
				var (_,eNListP) = Get_eNetwork_OrgToChain( eNX, pmCnd:1 );      // pmCnd:1 

				var stLM=eNetwork_ToStringB(eNListM); stLM=eNetwork_ToStringB(eNListM);  // ..B..
				var stLP=eNetwork_ToStringA(eNListP); stLP=eNetwork_ToStringA(eNListP);  // ..A..
					//WriteLine( $"\n...stLM:{stLM}\n...stLP:{stLP}" );
					//WriteLine( $">>>... Get_eNetwork_OrgToChain usage error." );
			}
			else{
				var (_,eNetList) = Get_eNetwork_OrgToChain( eNX, pmCnd:1 );
				return eNetList;
			}
			return null;
		}

		public List<eNetwork_Link> Get_eNetwork_OrgToChain( ULogical_Node LG0 ){
            List<eNetwork_Link> eNetList=null;  //@@@@@
            ULogical_Node LGX = LG0;

		  //eNetwork_Node eNX = eNetwork_Man.eNetwork_NodeList.Find(p => p.matchKey2==LG0.matchKey2);
			eNetwork_Node eNX = eNetwork_Man.Get_NodeRef(LG0);
            int loopC=0;
            while( LGX!=null && (++loopC)<18 ){
				eNetwork_Link eL = (LGX.pmCnd==0)? eNX.eChaineNtwk_pre_Minus: eNX.eChaineNtwk_pre_Plus;
//              eNetwork_Link eL = LGX.eChaineNtwk_pre_Plus ?? LGX.eChaineNtwk_pre_Minus;
                if( eL == null ) break;
                if( eNetList is null ) eNetList = new();
                eNetList.Add(eL);
                LGX = eL.eN_Org_nPM.ULGNode;

//                if( (LGX is null) || (eN0 is null) ){ WriteLine( $"LGX:{LGX}  eN0:{eN0}" ); break; }
                if( LGX.matchKey3 == LG0.matchKey3 )  break;
            }
            if( eNetList!=null ) eNetList.Reverse();
            return eNetList;
        }


        public (int,List<eNetwork_Link>) Get_eNetwork_OrgToChain( eNetwork_Node eN0, int pmCnd ){
            List<eNetwork_Link> eNetList = new();
            eNetwork_Node  eNX = eN0;

            int  hashValue=0, loopC=0, pm=pmCnd, badX=0;
            while( eNX!=null & (++loopC)<20 ){   
                eNetwork_Link eLX = (pm==0)? eNX.eChaineNtwk_pre_Minus: eNX.eChaineNtwk_pre_Plus;        
                if( eLX is null )  break;
                hashValue |= (badX=eLX.hv ^ _hushBase[eLX.ID%8191]);//2-1]);
                eNetList.Add(eLX);
                if( badX==-1 )  WriteLine( $"badX:{badX}" );

                eNX = eLX.eN_Org_nPM;
                pm  = eLX.OrgN.pmCnd;
                if( eNX==null || eNX.matchKey2==eN0.matchKey2)  break;
            }
            if( eNetList!=null ) eNetList.Reverse();
            return (hashValue,eNetList);
        } 


// ---------------------------------------------------------------------------------------------
/*
     000 ---> --M-- : r1c2#2+ =>  r1c5#2- =>  r1c5#8+ => [ALSM:r23c6 #168->#16 ->r23c6#6+] =>r6c6#6-
     000 ---> ++P++ : r1c2#2+ =>  r1c5#2- =>  r2c5#2+ =>  r2c1#2- =>  r4c1#2+ =>  r4c4#2- =>  r4c4#8+ =>  r4c6#8- =>  r4c6#5+ =>  r6c6#5- =>  r6c6#6+
*/
        public string  eNWMan_DisplayNetworkPath( eNetwork_Link eLX, bool lineType=true, bool debugBPM=false ){
            eNetwork_Node eNX = eLX.eN_Des_nPM;
			string st = eNWMan_DisplayNetworkPath( eNX, lineType, debugBPM );
			return  st;
		}

        public string  eNWMan_DisplayNetworkPath( eNetwork_Node eNX, bool lineType=true, bool debugBPM=false ){
			if( eNX.eChaineNtwk_pre_Minus==null && eNX.eChaineNtwk_pre_Plus==null ){
				ULogical_Node ULG = new ULogical_Node( no:eNX.no, rc:eNX.rc, pmCnd:0);	
			  //eNX = eNetwork_Man.eNetwork_NodeList.Find(p => p.matchKey2==ULG.matchKey2);
				eNX = eNetwork_Man.Get_NodeRef(ULG);
			}

			var (_,eNListM) = Get_eNetwork_OrgToChain( eNX, pmCnd:0 );      // pmCnd:0   
            var (_,eNListP) = Get_eNetwork_OrgToChain( eNX, pmCnd:1 );      // pmCnd:1 

			string st="", stLM="", stLP="";
			if(lineType){ stLM=eNetwork_ToStringB(eNListM); stLP=eNetwork_ToStringB(eNListP); }  // ..B..
			else{         stLM=eNetwork_ToStringA(eNListM); stLP=eNetwork_ToStringA(eNListP); }  // ..A..

			 int spN=5;
			if(debugBPM){ st=$"{new string(' ',spN)}@ ---> --M-- : {stLM}\n{new string(' ',spN)}@ ---> ++P++ : {stLP}"; }
			else{ 		  st = $"{stLM}\n{stLP}";	}

            if(debugBPM) WriteLine( st );
			return st;
		}
    }
}
                        