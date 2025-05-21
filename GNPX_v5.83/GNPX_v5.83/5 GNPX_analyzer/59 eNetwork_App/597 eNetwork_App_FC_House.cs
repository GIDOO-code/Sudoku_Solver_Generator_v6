using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Net;
using static System.Math;
using static System.Diagnostics.Debug;

using GIDOO_space;
using static System.Net.WebRequestMethods;

namespace GNPX_space{

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/
        //6.14..5.7.3.7..4..9..35.....6..1.3.415.....688.4.3..1.....94..2..2..3.4.4.6..21.3
		//1..89..4.8..21...7...7.59813154.9..8.8.5..41...41.8395..1382.646.89..1...236.18.9  for develop

    public partial class eNetwork_App: AnalyzerBaseV2{
        public bool eNetwork_FC_House( ){
            eNetwork_ForceChain_prepare( );

			bool debugPrintB = false;	//true;

            List<int>  DeduplicationList = new(); 
            OrgDes_rcno.Sort();

				OrgDes_rcno.ForEach( (P,mx) => {
					var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(P);
					if(debugPrintB) WriteLine( $"OrgDes_rcno:{mx} {Orc.ToRCString()}#{Ono+1} -> {Drc.ToRCString()}#{Dno+1}" );
				} );

			int solCount = Get_solutionCount();
            foreach( var (h,nc,desGroup) in Get_desGroup() ){
				AdditionalMessage = $" Verifying: {pAnMan.solutionC} / {solCount}(max)";


              // .... Check by number of proofs ..... .... ..... .... 
                UInt128 houseRC = UInt128.Zero;
                foreach( var desG in desGroup ){    
                    var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(desG);
					if(debugPrintB) WriteLine( $"{Orc.ToRCString()}#{Ono+1} -> {Drc.ToRCString()}#{Dno+1}" );
                    houseRC = houseRC.Set(Orc);
                }
                if( houseRC.BitCount() != nc )  continue;  // houseRC:proven elements.
              // .... ..... .... ..... .... ..... .... ..... .... ..... 


              // ============== solution found ===============
                // Deduplication list
                int desG0 = desGroup[0];
                if( ForceChain_Option == "ForceL2" ){
                    int keyDrc = desG0&0xFFF;
                    if( DeduplicationList.Count>0 && DeduplicationList.Contains(keyDrc) )  continue;
                    DeduplicationList.Add(keyDrc);
                }

              // .... solution message.
                UInt128 selectedRC = UInt128.Zero;
                List< List<eNetwork_Link> >  solChain_List = new();
                foreach( var desG in desGroup ){
                    var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(desG);
                           if(debugPrintB)  WriteLine( $" --- Origin:{Orc.ToRCString()}#{Ono+1}  =>  Destination:{Drc.ToRCString()}#{Dno+1}" );
                    eNetwork_Node eN_Org = Get_eNetwork_Proof( pmCnd:0, OrgDesVal:desG, rcProhibit:houseRC, debugPrintB:false );
					if( eN_Org == null )  goto LNextGroup;
					var eNLst = eGLink_ManObj.Get_eNetwork_OrgToChain( eN_Org );  // generate when needed <... for debug
					if( eNLst == null )  goto LNextGroup;	// ### Probably a bug
						//string stSol = eNetwork_ToStringA(_eNLst);

                    solChain_List.Add( eNLst );
                    if( selectedRC.IsHit(Orc) ) eN_Org.selected = true;
                    selectedRC = selectedRC.Set(Orc);
								//WriteLine( eN_Org );
                }

                SolCode = 2;
                eNetwork_FC_House_SolResult( desGroup, solChain_List );

                // ==== ForceL1 =====
                if( ForceChain_Option == "ForceL1" ){
					extResult = $"eNetwork_FC_House\n" + string.Join("\n",extStLst);  
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                    extResult = ""; extStLst.Clear();
                }

			LNextGroup:
				continue;
            }

            // ==== ForceL2,3 =====
            if( ForceChain_Option != "ForceL1" ){
                extResult = $"eNetwork_FC_House\n" + string.Join("\n",extStLst);
				if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
				//extResult = "";
            }

            return (SolCode>0);

            // ----- inner function ----
            IEnumerable< (int,int,List<int>) > Get_desGroup(){
                for( int no=0; no<9; no++ ){
                    UInt128 BoardBPno = FreeCell81b9[no];//		eNetwork_Man.Board_BitRep[no];
                    for( int h=0; h<27; h++ ){
                        UInt128 pHB = HouseCells81[h] & BoardBPno;
                        if( pHB == UInt128.Zero )  continue;
                        List<int> OrgRCN = OrgDes_rcno.FindAll( p => ((p>>16)&0xF)==no && pHB.IsHit(p>>20) ); //Ono=(p>>16)&0xF Orc=(p>>20)
                        if( OrgRCN.Count == 0 )  continue;

                        var desGroup = OrgRCN.GroupBy( p=> p&0xFFF );
                        int nc = pHB.BitCount();
                        foreach( var desG in desGroup.Where(p=>p.Count()>=nc) )   yield return (h,nc,desG.ToList());
                    }
                }
                yield break;
            }

			int Get_solutionCount(){
				int cnt=0;
				foreach( var (h,nc,desGroup) in Get_desGroup() )  cnt += desGroup.Count();
				return cnt;
			}
        }

        private string eNetwork_FC_House_SolResult( List<int> desGroup, List< List<eNetwork_Link> > solChain_List ){
            var (pm0,Orc0,Ono0,Drc0,Dno0) = Func_ToRcNo(desGroup[0]);

          // Set color & cancel
            Color  crAtt=Colors.Navy, crElm=Colors.LightPink;
            foreach( var desG in desGroup ){ 
                var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(desG);
                    
					if( Orc<0 || Orc>80 )	WriteLine( $" --- Origin:{Orc.ToRCString()}#{Ono+1}  =>  Destination:{Drc.ToRCString()}#{Dno+1}" );
                UCell  stm=pBOARD[Orc];
                stm.Set_CellColorBkgColor_noBit((1<<Ono),crAtt,SolBkCr);
            }

			List<eNetwork_Link> eNList0 = solChain_List[0];
			ULogical_Node ULGfixed = eNList0.Last().DesN;
            UCell UCfixed = pBOARD[ULGfixed.rc];
            
			SolCode = 1;
            UCfixed.FixedNo = Dno0+1;
            UCfixed.Set_CellColorBkgColor_noBit(1<<Dno0,AttCr,SolBkCr);
            

          // ----- Solution explanation message -----
            string stRes = $"\nNo matter which #{Ono0+1} of House is true, #{Dno0+1} of the target cell is false.";
            string st0 = $"eNetwork_FC_House ";

            string stL="";
            foreach( var eNList in solChain_List ){//.Where(p=>p.selected) ){
                stL += eNetwork_ToStringA(eNList);
            }     
            int noS = (desGroup[0]>>16)&0xF;
            string stL2 = desGroup.Aggregate("",(a,b)=>a+$" {(b>>20).ToRCString()}").ToString_SameHouseComp();
            
            string stE = $"Eliminated:{UCfixed.rc.ToRCString()} #{Dno0+1}";
            Result = ($"{st0}  Stem:{stL2} #{noS+1}  {stE}").Replace("eNetwork","eNW").Replace("Eliminated","Elim");

            if( SolInfoB ) ResultLong = $"{st0}\n{stL}\n{stE}\n{stRes}";
            string stS = "";


            // ----- all links in the solution -----
            eNetwork_Link.qConnectTo_RC = -1;     // ..... off: Control of explanation messages
            string stExt = stS;
                  
            string stLast="";
            foreach( var eNList in solChain_List ){
                string _st2 = eNetwork_ToStringA(eNList);
				int lng = Max(0, _st2.Length-1);
                string _st3 = _st2.Substring(lng);
                if( _st3 != stLast ){ _st2 += "\n"; stLast=_st3; }
                stExt += _st2;
            }

            stExt += $"\nfixed : {ULGfixed.rc.ToRCString()}#{ULGfixed.no+1}";

            string extResult00 = stExt+stRes;
            extStLst.Add(extResult00);

            return extResult00;
        }
    }
}