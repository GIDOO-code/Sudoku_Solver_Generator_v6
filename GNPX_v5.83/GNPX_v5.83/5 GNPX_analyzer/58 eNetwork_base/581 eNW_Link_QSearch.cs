using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.Threading.Channels;
using System.Windows.Documents;
using System.Xml.Linq;
using System.IO;
using System.Text;

namespace GNPX_space {

    public partial class eNetwork_Man: ALSLinkMan{       

			//1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9  for develop
			//6.14..5.7.3.7..4..9..35.....6..1.3.415.....688.4.3..1.....94..2..2..3.4.4.6..21.3

        public IEnumerable<UInt128> IEUInt128_H27_Combination( UInt128 basicU0, int startSZ, int endSZMax=6, bool debugPrintB=false ){ 
			// Generates combinations of only elements of the same House.

			if( basicU0.BitCount() == 1 ){ yield return basicU0; }
			else{
				for( int h=0; h<27; h++ ){
					UInt128 basicH = basicU0 & HouseCells81[h];
					if( basicH == UInt128.Zero ) continue;

					int sz = basicH.BitCount();
					List<int> BULst = basicH.IEGet_rc().ToList();
						if(debugPrintB)  BULst.ForEach( (p,mx) =>  WriteLine( $" BULst:{mx} {p}" ) );

					int endSZ = Min( basicH.BitCount(), endSZMax);
					for( int nSZ=startSZ; nSZ<=endSZ; nSZ++ ){
						Combination cmb = new(sz,nSZ);
						while( cmb.Successor() ){ 
							yield return cmb.ToUInt128BitExpression(BULst);
						}
					} 
				}
			}
			yield break;
        }  

		public IEnumerable<UInt128> IEUInt128_Combination( UInt128 basicH, int startSZ, int endSZMax=6, bool debugPrintB=false ){ 
			// Generates combinations of only elements of the same House.

			if( basicH.BitCount() == 1 ){ yield return basicH; }
			else{
				int sz = basicH.BitCount();
				List<int> BULst = basicH.IEGet_rc().ToList();
					if(debugPrintB)  BULst.ForEach( (p,mx) =>  WriteLine( $" BULst:{mx} {p}" ) );

				int endSZ = Min( basicH.BitCount(), endSZMax);
				for( int nSZ=startSZ; nSZ<=endSZ; nSZ++ ){
					Combination cmb = new(sz,nSZ);
					while( cmb.Successor() ){ 
						yield return cmb.ToUInt128BitExpression(BULst);
					}
				} 

			}
			yield break;
        }  


		public UInt128 Get_rc_BitExpression_FreeBwk( List<UCell> UCList, int no=-1 ){
            UInt128 cells128=0;
            int noB = (no>=0)? (1<<no): 0;
            foreach( var p in UCList ){
                if( p.No != 0 )  continue; 
                if( (no>=0 && no<=8) && (p.FreeBwk&noB)==0 )  continue;
                cells128 |= (UInt128)1<<p.rc;   
            }
            return cells128;
        }

		// *==*==*==*==*==*==*==*==*==*
		//  QSearch_Links_Generater
		// *==*==*==*==*==*==*==*==*==*

		public int QSearch_Links_Generater( bool checkB ){

			//WriteLine( $"========== {++eNdebug} QSearch_Links" );

            eNetwork_LinkList = new();
			//debugPrintB = true;

            if( G6.Cell ){
                QSearch_Cell_eLinks(  debugPrintB:debugPrintB );
                QSearch_Inter_eLinks( debugPrintB:debugPrintB );
            }
            if( G6.AIC ) QSearch_AIC_eLinks( debugPrintB:debugPrintB );

            if( G6.ALS ){
				
				QSearch_ALS_eLinks( debugPrintB:debugPrintB );   

				if( G6.ALSXZ ) QSearch_ALSXZ_eLinks( debugPrintB:debugPrintB );	
			  //if( G6.AnLS"] )  QSearch_AnLS_eLinks( debugPrintB:debugPrintB );	 //Next development goal

				if( G6.eALS )  QSearch_ReconfigureALS( debugPrintB:debugPrintB );

			}
			eNetwork_LinkList = eNetwork_LinkList.DistinctBy( p=> p.hv ).ToList();

/* @@@
			eNetwork_LinkList.ForEach( p=>WriteLine(p) );
			using(var fpW=new StreamWriter("ÅüeNetwork_LinkList_v6.txt",append:false,Encoding.UTF8)){
				eNetwork_LinkList.ForEach( P=> fpW.WriteLine(P));
			}
*/
            if(checkB){
                var query = eNetwork_LinkList.GroupBy(x => x.OrgN.matchKey3);
                foreach( var group in query ){
                    foreach( var P in group ) WriteLine( P.ToString() );
                }
            }
            return eNetwork_LinkList.Count;
                      //eNetwork_Link.Set_withID_(withID:false);   // ... for debug
        }

        private void Set_eGLink( eNetwork_Link R, bool debugPrintB, List<eNetwork_Link> QList ){
            if( QList.Find(r=> r.keySt==R.keySt) is null )  QList.Add(R);
            //if( debugPrintB )  WriteLine( R );
        }

        private IEnumerable<UInt128> IE_MultiCombination( UInt128 U0, bool splitB=false  ){ 
            int nc = U0.BitCount();
            var UBlst = U0.IEGet_rc().ToList();
            int minus = splitB? 1: 0;
            for( int n=1; n<=nc-minus; n++ ){
                Combination cmb = new(nc,n);
                while( cmb.Successor() ){
                    UInt128 U = new();
                    for( int m=0; m<n; m++ )  U = U.Set( UBlst[cmb.Index[m]] );//(UInt128.One << UBlst[cmb.Index[m]]);
                        //WriteLine( $"U:{U.ToBitString81N()}" );
                    yield return U;
                }
            } 
            yield break;
        }



        // *==*==*==*==*==*==*==*
        //  eNW_Link_IntraCell
        // *==*==*==*==*==*==*==*
        private int QSearch_Cell_eLinks( bool debugPrintB=true ){

			// WriteLine( $"========== {++eNdebug} QSearch_Cell_eLinks" );

            List<eNetwork_Link> IntraCell_List = new(); 
            foreach( var UC in pBOARD.Where(p=> p.No==0 && p.FreeBC>=2) ){
                    if( debugPrintB )  WriteLine( $"\rUC:{UC}" );
                
                List<int> noList = UC.FreeB.IEGet_BtoNo().ToList();

                UInt128 b128_rc = UInt128.One<<UC.rc;
                Permutation prm = new(UC.FreeBC,2);
                while(prm.Successor()){
                    int no1 = noList[prm.Index[0]];
                    int no2 = noList[prm.Index[1]];
                    
                    ULogical_Node OrgN = new( noB9:1<<no1, b081:b128_rc, pmCnd:1 );
                    ULogical_Node DesN = new( noB9:1<<no2, b081:b128_rc, pmCnd:0 );

                    eNW_Link_IntraCell R = new( UC, OrgN, DesN );
                    Set_eGLink( R, debugPrintB, IntraCell_List );

                    if( UC.FreeBC==2 ){
                        ULogical_Node OrgN2 = new( noB9:1<<no1, b081:b128_rc, pmCnd:0 );
                        ULogical_Node DesN2 = new( noB9:1<<no2, b081:b128_rc, pmCnd:1 );

                        eNW_Link_IntraCell R2 = new( UC, OrgN2, DesN2 );
                        Set_eGLink( R2, debugPrintB, IntraCell_List );
                    }
                }
            }
            //IntraCell_List.ForEach( P => WriteLine(P) );

            eNetwork_LinkList.AddRange(IntraCell_List);
            return IntraCell_List.Count;
        }


        // *==*==*==*==*==*==*==*
        //  eNW_Link_InterCells
        // *==*==*==*==*==*==*==*
        private int QSearch_Inter_eLinks( bool debugPrintB=true ){ 

			//	WriteLine( $"========= {++eNdebug} QSearch_Inter_eLinks" );

            List<eNetwork_Link> InterCell_List = new(); 
            for( int no=0; no<9; no++ ){
				int noB = 1<<no;
                UInt128 B81no = FreeCell81b9[no];	//Board_BitRep[no];
						//WriteLine( $"#{no+1} B81no:{B81no.ToBitString81N()}" );//###

                int qALS_rcbFrame = B81no.Ceate_rcbFrameOr();
                foreach( int h in qALS_rcbFrame.IEGet_BtoHouse27() ){
						//WriteLine( $"--- h:{h}" );

                    UInt128 B81noH = B81no & HouseCells81[h];
                    if( B81noH.BitCount() < 2 )  continue;

                    foreach( var U1 in B81noH.IEGet_rc128(mx:81) ){
                        ULogical_Node OrgNp = new( noB9:noB, b081:U1, pmCnd:1 );
								//WriteLine( $"QSearch_Inter_eLinks OrgNp:{OrgNp}" );

                        UInt128 U2 = B81noH.DifSet(U1);
                        int szU2 = U2.BitCount();
                        foreach( var U2A in IEUInt128_Combination(U2,1,szU2) ){ 
                            ULogical_Node DesNp = new( noB9:noB, b081:U2A, pmCnd:0 );
								//WriteLine( $"QSearch_Inter_eLinks DesNp:{DesNp}" );

                            var Rp = new eNW_Link_InterCells( OrgNp, DesNp ); //R:+ -> -
								//WriteLine( $"QSearch_Inter_eLinks Rp:{Rp}" );

                            Set_eGLink( Rp, debugPrintB, InterCell_List  );
                        }

                        ULogical_Node OrgNm = new( noB9:noB, b081:U1, pmCnd:0 );
                        ULogical_Node DesNm = new( noB9:noB, b081:U2, pmCnd:1 );
                        var Rm = new eNW_Link_InterCells( OrgNm, DesNm ); //R:- -> +
							//WriteLine( $"QSearch_Inter_eLinks Rm:{Rm}" );
                        Set_eGLink( Rm, debugPrintB, InterCell_List  );
                    }
                }
            }

			//InterCell_List.Sort( (a,b) => ((a.OrgN.rc*10+a.OrgN.no) - (b.OrgN.rc*10+b.OrgN.no) ) );

            eNetwork_LinkList.AddRange(InterCell_List);
            return InterCell_List.Count; 
        }


        // *==*==*==*==*==*==*==*
        //  eNW_Link_ALS
        // *==*==*==*==*==*==*==*

      //1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9  for develop(1)
	  //6.14..5.7.3.7..4..9..35.....6..1.3.415.....688.4.3..1.....94..2..2..3.4.4.6..21.3

        private int QSearch_ALS_eLinks(  bool debugPrintB=true ){
			eNetwork_Link.Set_withID_( withID:debugPrintB );

            Prepare_ALSLink_Man( nPlsB:1, setCondInfo:true, debugPrintB:false );
            if( LinkCeAlsLst != null ) return 0;

            List<UAnLS> ALSLst_unique = ALSList.DistinctBy(p=>p.bitExp).ToList();
				//ALSLst_unique.ForEach( (P,kx) => WriteLine( $" +++{kx} {P}" ) );
				//debugPrintB = true;

            List<eNetwork_Link> ALS_List = new(); 
			//ALSLst_unique.ForEach( q => WriteLine(q) );

			int dbXX = 0;
            foreach ( var qALS in ALSLst_unique.Where(q=>q.Size>=2) ){                               // Select ALS
				//debugPrintB = true;
			//if(debugPrintB) WriteLine( $"qALS:{qALS}" );

                // ===================== ALS =====================
              //int qALS_freeB = qALS.FreeB;
                foreach( var noOrg in qALS.FreeB.IEGet_BtoNo(9) ){									// Select digit(no).	,,,1... no

					UInt128 qALS_B81no    = qALS.BitExp9[noOrg];									// no cells in qALS... 
                    int     qALS_rcbFrame = qALS_B81no.Ceate_rcbFrameAnd();							// rcb Frame
						if(debugPrintB) WriteLine( $"noOrg:#{noOrg+1}     B81no:{qALS_B81no.ToBitString81N()}  rcb:{qALS_rcbFrame.ToBitString27rcb(digitB:false)}" );

                    foreach( int hOrg in qALS_rcbFrame.IEGet_BtoNo(sz:27) ){    // === generate the House associated with the target element.
						UInt128 BOARDno_h   = FreeCell81b9[noOrg] & HouseCells81[hOrg];
                        UInt128 OrgCells128_no_h = BOARDno_h.DifSet(qALS_B81no);	// Org_candidate : Cells that convert ALS to LS in no(digit).
                        if( OrgCells128_no_h.IsZero() )  continue;
										if(debugPrintB){
											WriteLine( $"        BOARDno_h:{BOARDno_h.ToBitString81N()}" );
											WriteLine( $"       qALS_B81no:{qALS_B81no.ToBitString81N()}" );
											WriteLine( $" OrgCells128_no_h:{OrgCells128_no_h.ToBitString81N()}" );
										}

                        // ======= ALS turns to a LockedSet ================
						ALS_Reconfigure_initialSetting_house( qALS, noOrg, BOARDno_h, debugPrintB:debugPrintB );
                        bool changedT = ALS_Reconfigure( qALS, noOrg, debugPrintB:debugPrintB );  //@@@@@
						if(debugPrintB)  WriteLine( qALS );
                       
						foreach( UInt128  OrgUI in IEUInt128_H27_Combination(OrgCells128_no_h,1) ){  
							ULogical_Node OrgN = new( noB9:1<<noOrg, b081:OrgUI, pmCnd:1 );		//@@@@ ULogical_Node

							foreach( var UC in qALS.UCellLst ){
								int freeBX = UC.FreeB.DifSet(UC.FreeBwk);
								if( freeBX == 0 ) continue;
								foreach( var no in freeBX.IEGet_BtoNo() ){
									ULogical_Node DesN = new( no:no, rc:UC.rc, pmCnd:0 );	//@@@@ ULogical_Node
									eNW_Link_ALS R1 = new( qALS, OrgN, DesN );
									Set_eGLink( R1, debugPrintB, ALS_List );		//... Type-1
										  //if(debugPrintB) WriteLine( $" @@@ Type-1  {R1}" );
										  //WriteLine( $"R1:{R1}" );	//@@@@@
								}
							}

							foreach( var UC in qALS.UCellLst.Where(p=>p.FreeBwk.BitCount()==1) ){
								int noDes = UC.FreeBwk.BitToNum();
								ULogical_Node DesN = new( no:noDes, rc:UC.rc, pmCnd:1 );	//@@@@ ULogical_Node
								eNW_Link_ALS R2 = new( qALS, OrgN, DesN );

								Set_eGLink( R2, debugPrintB, ALS_List );			//... Type-2
										  //if(debugPrintB) WriteLine( $" @@@ Type-2  {R2}" );
										  //WriteLine( $"R2:{R2}" );	//@@@@@
							}
						}

						if(debugPrintB) WriteLine("\n");
                    }
                }
            }
			//var ALS_List2 = ALS_List.DistinctBy( q=>q.keySt ).ToList();

            if( ALS_List!=null && ALS_List.Count>0 )  eNetwork_LinkList.AddRange(ALS_List);
            return ALS_List.Count;

			bool CheckFor_PossiblePatterns( UInt128 rc128, int h, bool debugPrintB=false ){
				if( rc128.BitCount() <= 1 )  return true;
				int Ppat = rc128.Ceate_rcbFrameAnd();
					if(debugPrintB) WriteLine( $"**Ppat rc128:{rc128.ToBitString81()} h:{h}  Ppat:{Ppat.ToBitString27rcb(digitB:false)}" );
				return Ppat.DifSet(1<<h)>0;
			}
        }


        // *==*==*==*==*==*==*==*
        //  eNW_Link_AIC
        // *==*==*==*==*==*==*==*

        public int QSearch_AIC_eLinks( bool debugPrintB=true ){

			//	WriteLine( $"========== {++eNdebug} QSearch_AIC_eLinks" );

            List<eNetwork_Link> AIC_List = new(); 

            for( int no1=0; no1<9; no1++ ){                                         // select no1

                int noB = 1<<no1;              
                UInt128 B81P = FreeCell81b9[no1];										// cells with digit no1
                if( B81P.IsZero() || B81P.BitCount() <=1 )  continue;

                foreach( int selH in B81P.Ceate_rcbFrameOr().IEGet_BtoNo(sz:27)){   // select house no1.

                    UInt128 B81selH = B81P & HouseCells81[selH];                   // selected house cells
                    if( B81selH.IsZero() || B81P.BitCount()<=1)  continue;
                        if( debugPrintB )  WriteLine( $"\r selH:{selH}  AIC:{B81selH.ToBitString81N()}" );

                    foreach( int axis1 in B81selH.Ceate_rcbFrameOr().IEGet_BtoNo(sz:27).Where(h=>h!=selH) ){ // 1st axis

                        var B81ax1 = B81selH & HouseCells81[axis1];
                        if( B81ax1.IsZero() )  continue;

                        var B81selHrem = B81selH - B81ax1;  // (B81selH:Elements of AIC axis of focused.B81selHrem:emaining elements of AIC.)
                        if( B81selHrem.IsNotZero() )  continue;Å@//(Fails if there are remaining elements.)

                        foreach( int axis2 in B81selHrem.Ceate_rcbFrameAnd().IEGet_BtoNo(sz:27) ){          // 2nd axis
                            if( axis2 == axis1 ) continue;
                            var B81org = (B81P & HouseCells81[axis1]).DifSet(B81selH);
                            if( B81org.IsZero() )  continue;

                            var B81des = (B81P & HouseCells81[axis2]).DifSet(B81selH);
                            if( B81des.IsZero() )  continue;

                            for( int pm=0; pm<2; pm++ ){
                                ULogical_Node  aAIC = new( noB9:1<<no1, b081:B81selH, pmCnd:3 );
                                ULogical_Node  B81OrgN_2 = new( noB9:1<<no1, b081:B81ax1, pmCnd:1-pm );  // B81org.BP128 error
                                ULogical_Node  B81DesN_3 = new( noB9:1<<no1, b081:B81selHrem, pmCnd:pm );
                                ULogical_Node  B81DesN_4 = new( noB9:1<<no1, b081:B81des, pmCnd:1-pm );

                                foreach( var org_1 in B81org.IEGet_rc128() ){
                                    ULogical_Node B81OrgN_1 = new( noB9:1<<no1, b081:org_1, pmCnd:pm ); 
                                    if( (B81OrgN_1.b081&B81DesN_4.b081) > 0 ) continue;

                                    eNW_Link_AIC R = new( aAIC, B81OrgN_1, B81OrgN_2, B81DesN_3);							//@@@@@ AIC
                                    Set_eGLink( R, debugPrintB, AIC_List  );
                                }

                                if( B81org.BitCount() >= 2 ){                              
                                    ULogical_Node B81OrgN_1 = new( noB9:1<<no1, b081:B81org, pmCnd:pm ); 
                                    if( (B81OrgN_1.b081&B81DesN_4.b081) > 0 ) continue;
                                    eNW_Link_AIC R = new( aAIC, B81OrgN_1, B81OrgN_2, B81DesN_3);
                                    Set_eGLink( R, debugPrintB, AIC_List  );
                                }
                            }

						}
                    }
                }

            }

            eNetwork_LinkList.AddRange(AIC_List);
            return AIC_List.Count; 
        }





		//1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9  for develop

        // *==*==*==*==*==*==*==*
        //  eNW_Link_ALSXZ
        // *==*==*==*==*==*==*==*
        private int QSearch_ALSXZ_eLinks(  bool debugPrintB=false ){
			int ALSXZxx=0;
			//	WriteLine( $"@@@@@@@@@@ {++eNdebug} QSearch_ALSXZ_eLinks" );

            Prepare_ALSLink_Man( nPlsB:2, setCondInfo:true, debugPrintB:false );         
            if( ALSList is null || ALSList.Count<=1)  return 0;     //AALSList: Almost ALS
					//ALSList.ForEach( (p,mx) => WriteLine( $"{mx}: {p.ToStringA()}" ) );

            List<eNetwork_Link> DL_ALSXZ_List = new();

            foreach( var UA in ALSList.Where(p=>p.Size>=2) ){


                foreach( var UA2 in ALSList.Where( p=> p.ID>UA.ID && p.Size>=3 && p.Level==2)){	//At least one is Level 2 ALS
					ALSXZxx++;

                    if( (UA.bitExp & UA2.bitExp).IsNotZero() )          continue;  // overlapsÅ@
                    if( (UA.connectedB81 & UA2.bitExp).IsZero() )	continue;  // no contact		


					int FreeB = UA.FreeB | UA2.FreeB;
					foreach( var noOrg in FreeB.IEGet_BtoNo() ){
								// ... RestoreFreeB ... <<< important procedures >>>
						UA. RestoreFreeBwk();
						UA2.RestoreFreeBwk();
								if(debugPrintB) WriteLine( $"\n\n<<<after Restore: >>>\nUA:{UA}\nUA2:{UA2}" );


						// ----- rough check -----
						int noB = 1<<noOrg;
						if( (UA.FreeB.DifSet(noB)).BitCount() != UA.Size+1 )   continue;
						if( (UA2.FreeB.DifSet(noB)).BitCount() != UA2.Size+1 ) continue;
								if(debugPrintB) WriteLine( $" UA:{UA}\n{UA2}" );

						int FreeB_without_no = (UA.FreeB&UA2.FreeB) .DifSet(noB);
						int RCC = IsConnected_withRCC( UA, UA2, FreeB_without_no);
						if( RCC.BitCount() != 2 ) continue;				//UA, UA2 have two digits in common.

						// ----- rcbFrameOr -----
						// (An ALS with two RCCs becomes an expanded LockedSet by OriginNode.)
						int UArcbOr  = UA. BitExp9[noOrg].Ceate_rcbFrameOr();
						int UA2rcbOr = UA2.BitExp9[noOrg].Ceate_rcbFrameOr();
						int UAUA2rcbOr = UArcbOr | UA2rcbOr;
								//WriteLine( $" UArcbOr:{UArcbOr.rcbToBitString27()}" );
								//WriteLine( $"UA2rcbOr:{UA2rcbOr.rcbToBitString27()}" );
								//WriteLine( $"UAUA2rcbOr:{UAUA2rcbOr.rcbToBitString27()}" );

						// ----- Get OriginNode -----
						UInt128 UAUA2_BitExp = UA.bitExp | UA2.bitExp;	
								//WriteLine( $"UAUA2_BitExp:{UAUA2_BitExp.ToBitString81()}");
						List<UInt128> OrgND128List = new();
						for( int hh=0; hh<27; hh+=9 ){
							int UAUA2rcb = ((UAUA2rcbOr>>hh)&0x1FF);
							if( UAUA2rcb.BitCount() != 1 ) continue;
							int h = UAUA2rcb.BitToNum()+hh;
							UInt128 OrgND128 = (FreeCell81b9[noOrg] & HouseCells81[h]).DifSet(UAUA2_BitExp);
							if( OrgND128 == UInt128.Zero ) continue;
								//WriteLine( $"FreeCell81b9[noOrg]:{FreeCell81b9[noOrg].ToBitString81()}" );
								//WriteLine( $"       OrgND128:{OrgND128.ToBitString81()}" );
							if( OrgND128List.Contains(OrgND128) )  continue;
							OrgND128List.Add(OrgND128);
						}
						if( OrgND128List.Count == 0 ) continue;
						List<ULogical_Node> LogNDs = Create_OriginNode( noOrg, OrgND128List, pmCond:1 );
								//OrgND128List.ForEach( (P,mx) => WriteLine( $"--< LogNDs[{mx}] >-- {P.ToBitString81()}" ) );

						foreach( var LNG_O1 in LogNDs ){    // There is a UA2 origin node.
								//	WriteLine( $"LNG_01:{LNG_O1}" );
								// --------------------------------------------------------------
								// "FreeBwk" : Exclude no and reconfigure FreeB.
							ALS_Reconfigure_initialSetting( UA, noOrg );
							ALS_Reconfigure( UA, noOrg, debugPrintB:false );	//... FreeB
							if( !IsALS_byFreeBwk(UA) )  continue;

							ALS_Reconfigure_initialSetting( UA2, noOrg );
							ALS_Reconfigure( UA2, noOrg, debugPrintB:false );
							if( !IsALS_byFreeBwk(UA2) )  continue;
								if(debugPrintB){
									WriteLine( $"@ALS_Reconfigure UA:{UA}" );
									WriteLine( $"@ALS_Reconfigure UA2:{UA2}" );
								}
									int RCC_BitExp = IsConnected_withRCC(UA,UA2);  // ... Confirm ALSXZ holds
									if( RCC_BitExp.BitCount() < 2 )  continue;
										if(debugPrintB)	WriteLine( $" RCC_BitExp:{RCC_BitExp.ToBitString(9)}" );

							int UA_FreeBX = UA.UCellLst.Aggregate( 0, (p,q)=> p|q.FreeBwk );
							int UA2_FreeBX = UA2.UCellLst.Aggregate( 0, (p,q)=> p|q.FreeBwk );

							if( (UA_FreeBX|UA2_FreeBX).BitCount() != (UA.Size+UA2.Size) ) continue;
							// <<< Attention!!! >>> UA and UA2 are connected by two RCCs, and a new LockedSet is generated.
					
							foreach( var (ALSXZtype,LNG_D4) in IEGrt_ALSXZ_Sol(UA,UA2,LNG_O1,RCC_BitExp) ){
								   if(debugPrintB){
										WriteLine( $"noOrg:#{LNG_O1.no+1} h:{LNG_O1.ID:00} cand:{LNG_O1.b081.ToBitString81N()}" );
										WriteLine( $"====...ALSXZtype:{ALSXZtype}  LNG_D4:{LNG_D4}" );
								   }
				
								int sz1 = LNG_O1.b081Size;
								foreach( var OrgN_1 in IEUInt128_H27_Combination(LNG_O1.b081,1,sz1,debugPrintB:debugPrintB) ){ 
									
									ULogical_Node B81OrgN_1 = new( noB9:1<<LNG_O1.no, b081:OrgN_1, pmCnd:1 ); 
										if(debugPrintB)  WriteLine( $" ++ B81OrgN_1:{B81OrgN_1}" );
									int sz4=LNG_D4.b081Size;
									foreach( var DesN_4 in IEUInt128_H27_Combination(LNG_D4.b081,1,sz4,debugPrintB:debugPrintB) ){ 

										ULogical_Node B81DesN_4 = new( noB9:1<<LNG_D4.no, b081:DesN_4, pmCnd:LNG_D4.pmCnd ); 
										eNW_Link_ALSXZ R = new( UA, UA2, RCC_BitExp, B81OrgN_1, B81DesN_4 );
											if(debugPrintB)  WriteLine( R );
										Set_eGLink( R, debugPrintB, DL_ALSXZ_List );
									}
								}
							}
					
						}
					}
                }
            }    
			//DL_ALSXZ_List.ForEach( (P,mx) => WriteLine( $"{mx}:{P}" ) );

            eNetwork_LinkList.AddRange(DL_ALSXZ_List);
            return DL_ALSXZ_List.Count;
        
     
            // =========== inner functions =================================
			bool IsALS_byFreeBwk( UAnLS UA ){
				int FreeBwk = UA.UCellLst.Aggregate(0, (p,q)=> p|q.FreeBwk );
				var ret = FreeBwk.BitCount() == UA.UCellLst.Count+1;
				return ret;
			}

			 List<ULogical_Node> Create_OriginNode( int no, List<UInt128> OrgND128List, int pmCond ){
				List<ULogical_Node> LogNDs = new();

				foreach( var LNDorg in OrgND128List ){
					List<int> orgLst = LNDorg.IEGet_rc().ToList();
					int sz = orgLst.Count;
					for( int n=1; n<=sz; n++ ){
						Combination cmb = new( sz, n );
						while( cmb.Successor() ){
							UInt128 Org = UInt128.Zero;
							for( int k=0; k<n; k++ )  Org = Org.Set( orgLst[cmb.Index[k]] );
							ULogical_Node LogND = new( noB9:1<<no, b081:Org, pmCnd:pmCond, ID:0);
							LogNDs.Add(LogND);
						}
					}
				}
                LogNDs = LogNDs.DistinctBy(p=>p.matchKey3).ToList();  //keys: no, b081, pmCnd (not include ID)

					//LogNDs.ForEach( (P,mx) => WriteLine( $"@@LogNDs {P}" ) );
                return LogNDs;
            }



			// =========================================================================================================
            IEnumerable<(int,ULogical_Node)> IEGrt_ALSXZ_Sol( UAnLS UA, UAnLS UA2, ULogical_Node LNG, int RCC_BitExp ){
                int no1 = LNG.no;						
				UInt128 UAUA2_BitExp = UA.bitExp | UA2.bitExp;

				{  // inner exclusion candidates
					foreach( UCell P in UA.UCellLst ){
						int elmB = P.FreeB.DifSet(P.FreeBwk).DifSet(RCC_BitExp);
						if( elmB == 0 )  continue;
						foreach( int no in elmB.IEGet_BtoNo(9) ){
							ULogical_Node LogND_Des4 = new( no:no, rc:P.rc, pmCnd:0 );
								//WriteLine( $"inner exclusion candidates UA : {LogND_Des4}" );
							yield return (1,LogND_Des4);
						}
					}

					foreach( UCell P in UA2.UCellLst ){
						int elmB = P.FreeB.DifSet(P.FreeBwk).DifSet(RCC_BitExp);
						if( elmB == 0 )  continue;
						foreach( int no in elmB.IEGet_BtoNo(9) ){
							ULogical_Node LogND_Des4 = new( no:no, rc:P.rc, pmCnd:0 );
								//WriteLine( $"inner exclusion candidates UA2 : {LogND_Des4}" );
							yield return (1,LogND_Des4);
						}
					}
				}
				
				{	// inner fixed candidates
					foreach( UCell P in UA.UCellLst.Where(p=>p.FreeBwk.BitCount()==1) ){
						int no = P.FreeBwk.BitToNum();
						ULogical_Node LogND_Des4 = new( no:no, rc:P.rc, pmCnd:1 );
						yield return (2,LogND_Des4);
					}

					foreach( UCell P in UA2.UCellLst.Where(p=>p.FreeBwk.BitCount()==1) ){
						int no = P.FreeBwk.BitToNum();
						ULogical_Node LogND_Des4 = new( no:no, rc:P.rc, pmCnd:1 );
						yield return (2,LogND_Des4);
					}
				}

                // Exclusion of external elements with RCC
                foreach( int no4 in RCC_BitExp.IEGet_BtoNo()){
					UInt128 rcBitRCC = UA.BitExp9[no4] | UA2.BitExp9[no4];
					int rcbRCC = rcBitRCC.Ceate_rcbFrameAnd( );
					for( int hh=0; hh<27; hh+=9 ){
						int rcb = ((rcbRCC>>hh)&0x1FF);
						if( rcb.BitCount() != 1 ) continue;
						int h = rcb.BitToNum()+hh;
						UInt128 cand_outer = (FreeCell81b9[no4] & HouseCells81[h]).DifSet(UAUA2_BitExp);
						if( cand_outer == UInt128.Zero ) continue;
                        ULogical_Node LogND_Des4 = new( noB9:1<<no4, b081:cand_outer, pmCnd:0 );
                            //WriteLine( $"           type-1(RCC) LogND_Des4 no:#{no4+1}  h:{h}  LogND_Des4:{LogND_Des4}" );
                        yield return (3,LogND_Des4);
                    }
                }

				// Exclusion of external elements with LockedSet
				List<UCell> UCellList_wk = new List<UCell>();
				UCellList_wk.AddRange(UA.UCellLst);
				UCellList_wk.AddRange(UA2.UCellLst);
					//UCellList_wk.ForEach( (P,mx) => WriteLine( $"UCellList_wk {mx}: {P}" ) );		//...

				int FreeBwk = UCellList_wk.Aggregate(0, (p,q)=> p|q.FreeBwk );
					//WriteLine( $"FreeBwk:{FreeBwk.ToBitString(9)}" );						//...
				if( FreeBwk.BitCount() == UCellList_wk.Count ){		// -> is LockedSet
					foreach( int no in FreeBwk.IEGet_BtoNo() ){
						//if( no==3 )  WriteLine( $" ... Exclusion of external elements with LockedSet #{no+1}" );	//...
						int noB = 1<<no;
						UInt128 rc81 = Get_rc_BitExpression_FreeBwk( UCellList_wk, no );
								//WriteLine( $"no:{no+1} rc81:{rc81.ToBitString81()}" );	//...
						int rcbRCC = rc81.Ceate_rcbFrameAnd( );	
						for( int hh=0; hh<27; hh+=9 ){
							int rcb = ((rcbRCC>>hh)&0x1FF);
							if( rcb.BitCount() != 1 ) continue;
							int h = rcb.BitToNum()+hh;
							UInt128 cand_outer = (FreeCell81b9[no] & HouseCells81[h]).DifSet(UAUA2_BitExp);
								//WriteLine( $"h:{h}  cand_outer:{cand_outer.ToBitString81()}" ); //...
							if( cand_outer == UInt128.Zero ) continue;
							ULogical_Node LogND_Des5 = new( noB9:1<<no, b081:cand_outer, pmCnd:0 );
								//WriteLine( $"           type-1(RCC) LogND_Des4 no:#{no+1}  h:{h}  LogND_Des4:{LogND_Des5}" ); //...
							yield return (4,LogND_Des5);
						}
					}
				}

            }
        }
	
    }
}
                        