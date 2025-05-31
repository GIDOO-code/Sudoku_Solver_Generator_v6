using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Net;
using static System.Diagnostics.Debug;

using GIDOO_space;
using static System.Net.WebRequestMethods;

namespace GNPX_space{

        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/       
        //6.14..5.7.3.7..4..9..35.....6..1.3.415.....688.4.3..1.....94..2..2..3.4.4.6..21.3

		//47.21.539295.7361..13.95.27.4256..9.961384752.5..2..6.524.3.176.3.142985189.5.243
		//476218539295473618813695427342567891961384752758921364524839176637142985189756243

    public partial class eNetwork_App: AnalyzerBaseV2{
        public bool eNetwork_FC_Cells( ){

            eNetwork_ForceChain_prepare( );

		//================ contradiction ================================================
			int serchKK = 0;
			int searchCC = OrgDes_rcno.Count( p=> (p>>28)==3 );
			var desGroup = OrgDes_rcno.GroupBy( p=> p&0xFF00FFF );
			foreach( var desG in desGroup ){    // .... Force_Chain_Cell
				var (pm,OrcX,_,_,_) = Func_ToRcNo(desG.Key&0xFFFFFFF);
				if( pBOARD[OrcX].FreeBC == desG.Count() )  searchCC++;
			}

			List<int> contradiction_rcno = OrgDes_rcno.FindAll( p=> (p>>28)==3 );
			if( contradiction_rcno.Count > 0 ){
				foreach( int OrcnoDrcno in contradiction_rcno ){
					AdditionalMessage = $"  proofing: ... {++serchKK} / {searchCC}";				// <<< AdditionalMessage >>>
					int OrcnoDrcno2 = OrcnoDrcno & 0xFFFFFFF;
					var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(OrcnoDrcno2);

					var (stLine,stList) = eNetwork_Proof_Contradiction( OrcnoDrcno2 );
					eNetwork_FC_Cells_Contradiction_Result( OrcnoDrcno2, stLine, stList );
					                
					SolCode = 2;
					if( ForceChain_Option == "ForceL1" ){
						if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
						extResult = ""; extStLst.Clear();
					}
				}
				OrgDes_rcno.RemoveAll( p=> (p>>28)==3 );
			}
			// ------------------------------------------------------------
						// ==== ForceL2,3 =====

			if( extStLst.Count>0 && ForceChain_Option!="ForceL1" ){
				extResult = string.Join("\r",extStLst);

				if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
				extResult = ""; extStLst.Clear();
			}

		//================ ForceChain_Cells =========================================================
            List<int>  DeDuplicationList = new();
			OrgDes_rcno = OrgDes_rcno.ConvertAll( p => (p&0xFFFFFFF) );  // remove P/M code(31-28bit)
            OrgDes_rcno.Sort();

			int kx=0;
            foreach( var desG in desGroup ){    // .... Force_Chain_Cell
                // ===== Check the proof number conditions =========================
                var (pmX,OrcX,_,_,_) = Func_ToRcNo(desG.Key&0xFFFFFFF);
                if( pBOARD[OrcX].FreeBC != desG.Count() )  continue;

				AdditionalMessage = $"  proofing: ... {++serchKK} / {searchCC}";				// <<< AdditionalMessage >>>
                int _FreeB = desG.Aggregate(0, (a,b) => a| 1<<((b>>16)&0xF) ); 
                if( _FreeB != pBOARD[OrcX].FreeB )  continue;  // (maybe redundant)

                if( ForceChain_Option == "ForceL2" ){
                    int keyDrc = desG.Key&0xFFFF;
                    if( DeDuplicationList.Count>0 && DeDuplicationList.Contains(keyDrc) )  continue;
                    DeDuplicationList.Add(keyDrc);
                }

                // ==== solution found =====
				List< List<eNetwork_Link> >  eNtWk_LInkListList = new();
				List<string>	eNtWkSolStrList = new();

                foreach( var OrcnoDrcno in desG ){	
					var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(OrcnoDrcno);
                    Get_eNetwork_Proof( pmCnd:0, OrgDesVal:OrcnoDrcno, rcProhibit:UInt128.Zero, debugPrintB:false );

					ULogical_Node ULG = new( no:Dno, rc:Drc, pmCnd:1 );
				//	eNetwork_Node eN_Des = eNetwork_Man.eNetwork_NodeList.Find(p => p.matchKey2==ULG.matchKey2);
					eNetwork_Node eN_Des = eNetwork_Man.Get_NodeRef(ULG);
				  //var _eNList = eGLink_ManObj.Get_eNetwork_OrgToChain( eN_Des ); // generate when needed   <-- Reverse route from landing to starting point
					var _eNList = eGLink_ManObj.Get_eNetwork_OrgToChain( ULG ); // generate when needed   <-- Reverse route from landing to starting point
						string stL = eNetwork_ToStringA(_eNList);
						//WriteLine( stL );

					eNtWk_LInkListList.Add( _eNList );
					eNtWkSolStrList.Add( stL );
                }

                SolCode = 2;
				eNetwork_FC_Cells_SolResult( desG.Key, eNtWk_LInkListList, eNtWkSolStrList );

                // ==== ForceL1 =====
                if( ForceChain_Option == "ForceL1" ){
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                    extResult = ""; extStLst.Clear();
                }
            }

            // ==== ForceL2,3 =====
            if( extStLst.Count>0 && ForceChain_Option!="ForceL1" ){
                extResult = string.Join("\r",extStLst);
				if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
                extResult = ""; extStLst.Clear();
            }

            return (SolCode>0);
        }

	#region FC_Contradiction
		private (string,string) eNetwork_Proof_Contradiction( int OrcnoDrcno, bool debugPrintB=false ){
			var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(OrcnoDrcno);
				//WriteLine( $"{Orc.ToRCString()}#{Ono+1} -> {Drc.ToRCString()}#{Dno+1}" );
			ULogical_Node ULG_start = new( no:Ono, rc:Orc, pmCnd:1 );
			var (_,eLnext) = eGLink_ManObj.QSearch_Network_RadialType( ULG_start, rcBreak:-1, extLink:1, rcProhibit:UInt128.Zero, debugPrintB:debugPrintB );		
			string stLine = eGLink_ManObj.eNWMan_DisplayNetworkPath( eLnext.eN_Des_nPM, lineType:false );
			string stList = eGLink_ManObj.eNWMan_DisplayNetworkPath( eLnext.eN_Des_nPM, lineType:false ); 
			return (stLine,stList);
		}

		private void eNetwork_FC_Cells_Contradiction_Result( int OrcnoDrcno2, string stLine, string stList ){
			var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(OrcnoDrcno2);

			Result = $"eNW_FC_Cells Contradiction: {Orc.ToRCString()} #{Ono+1} is negative.";

			string st0 = $"eNetwork_FC_Cells Contradiction";
			string stStem = $"Stem Cell: {Orc.ToRCString()}#{Ono+1}";
			string stContra = $"Contradiction Cell : {Drc.ToRCString()}#{Dno+1}";
			ResultLong = $"eNetwork_FC_Cells Contradiction\n {stStem}\n {stContra}";
			ResultLong += $"is positive and negative.\n Then {Orc.ToRCString()}#{Ono+1} is negative.";
			ResultLong += $"\n{stList}";
			extResult += ResultLong;

			UCell StemCell=pBOARD[Orc], ContraCell=pBOARD[Drc];
			ContraCell.Set_CellColorBkgColor_noBit(ContraCell.FreeB,AttCr,SolBkCr);
			if(ForceChain_Option=="ForceL1")  ContraCell.Set_CellFrameColor(Colors.Blue);

			StemCell.CancelB = StemCell.FreeB & (1<<Ono);
			int Fb = StemCell.FreeB.DifSet(1<<Orc);
			StemCell.Set_CellColorBkgColor_noBit( Fb ,AttCr,SolBkCr2);	
		}
	#endregion FC_Contradiction

	#region Force_Chain
		private string eNetwork_FC_Cells_SolResult( int desG_Key, List<List<eNetwork_Link>> eNtWk_LInkListList, List<string> eNtWkSolStrList ){
            var (pm,Orc,Ono,Drc,Dno) = Func_ToRcNo(desG_Key);

            UCell  StemCell=pBOARD[Orc], ElimCell=pBOARD[Drc];
            StemCell.Set_CellColorBkgColor_noBit(StemCell.FreeB,AttCr,SolBkCr);
            if(ForceChain_Option=="ForceL1")  StemCell.Set_CellFrameColor(Colors.Blue);

            ElimCell.FixedNo = Dno+1;
			ElimCell.CancelB = ElimCell.FreeB.DifSet(1<<Dno);
            ElimCell.Set_CellColorBkgColor_noBit((1<<Drc),AttCr,SolBkCr2);
            
            // ----- Solution explanation message -----
            string stRes = "\nNo matter which digit in Stem_Cell is true, the target can be derived as true..";
            string st0 = $"eNetwork_FC_Cells";
            string stS = $"Stem:{StemCell.rc.ToRCString()} #{StemCell.FreeB.ToBitStringNZ(9)}";
            string stE = $"Target:{ElimCell.rc.ToRCString()} #{Dno+1}";
            Result = ($"{st0} {stS} {stE}").Replace("eNetwork","eNW").Replace("Eliminated","Elim");

            eNetwork_Link.qConnectTo_RC = StemCell.rc;     // ..... on: Control of explanation messages

			string stL = string.Join( "\n", eNtWkSolStrList );

            if( SolInfoB ){
				ResultLong = $"{st0}\n{stS}\n{stL}\n{stE}\n{stRes}";
				extResult = ResultLong;
			}

			return stL;
        }
	#endregion Force_Chain

    }
}