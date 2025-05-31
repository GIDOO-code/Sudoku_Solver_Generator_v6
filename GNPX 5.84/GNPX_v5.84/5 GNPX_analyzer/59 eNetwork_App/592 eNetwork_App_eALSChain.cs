using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Reflection.Metadata.Ecma335;
using System.Diagnostics;
using System.Windows;

namespace GNPX_space{
    public partial class ALSTechGen: AnalyzerBaseV2{

        //ALS Chain is an algorithm that connects in a loop using ALS-RCC.
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page52.html

        // ALS (Almost Locked Set)
        // https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page26.html
         
        //Paste the next 81 digits onto the grid and solve with /Solve/MultiSolve/      
        //6.14..5.7.3.7..4..9..35.....6..1.3.415.....688.4.3..1.....94..2..2..3.4.4.6..21.3
		//7....5..9.9.4.7.8...8.3.7..81.3.9.6...6...9...3.7.2.18..3.7.1...4.5.1.2.1..6....4
		//6..9....5.4.5.6.7...5.3.2...6.8.3.91..8...6..91.7.2.5...6.7.4...7.3.5.6.2....4..7

        private bool Break_ALSChain = false;
		private List<long>  eALS_Chain_Solution = new List<long>();
		private bool  szCtrl_enough;
        public bool eALS_Chain(){
            
		  // ========== Prepare ================================================== 
			debugPrint = false; ///[for debug]

			PrepareStage();
            if(ALSMan.ALSList==null || ALSMan.ALSList.Count<=3) return false;

            ALSMan.QSearch_ALS2ALS_Link( );		// Find RCC between ALSs
				//if(debugPrint) ALSMan.ALSList.ForEach( (P,kx) => WriteLine( $"sq:{kx} {P}" ) );    
		 // -----------------------------------------------------------------------			

            Break_ALSChain = false;
			eALS_Chain_Solution.Clear();

			for(int szCtrl=10; szCtrl<=25; szCtrl+=5 ){ 
				szCtrl_enough = true;
				foreach( var ULG_000 in __IE_ALSChain_Control__() ){
					if( pAnMan.Check_TimeLimit() ) return (SolCode>0);

					int noS = ULG_000.no;
					List<UAnLS> ALSsel = ALSMan.ALSList.FindAll( Q => (Q.connected81_9[noS] & ULG_000.b081)> UInt128.Zero );
					if( ALSsel==null || ALSsel.Count==0 ) continue;

					// *==*==* First Link *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
					foreach( var UAS in ALSsel ){	// First ALS of chain.
							if(debugPrint) WriteLine( $"\n szCtrl:{szCtrl}  ULG_000:{ULG_000} ===>\n UA{UAS.ToStringA()}" );

						if( UAS.FreeB.DifSet(1<<noS) == 0 ) continue;
						UAS.preALS_no = (null,noS);
						UInt128 usedCells = ULG_000.b081 | UAS.bitExp;
						Search_ALSChain( 0, ULG_000, (UAS,noS), usedCells, szCtrl-UAS.Size, FirstB:true  );

						if( Break_ALSChain )  return (SolCode>0);
					}
					// *--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*--*
				}
				if( szCtrl_enough )  break;
			}

            return (SolCode>0);

			// ===== inner function =====
			IEnumerable<ULogical_Node> __IE_ALSChain_Control__( ){
				foreach( UCell UC in pBOARD.Where(p=>p.FreeB>0) ){
					foreach( int no in UC.FreeB.IEGet_BtoNo() ){
						ULogical_Node ULG_000 = new( no:no, rc:UC.rc, pmCnd:1 ); 
							if(debugPrint) WriteLine( $"\n@ 1 @ __IE_ALSChain_Control__    ULG_000:{ULG_000}" );
						yield return ULG_000;
					}
				}
				yield break;
			}			
		}

		private void Search_ALSChain( int GEN, ULogical_Node ULG_000, (UAnLS,int) UAfcs,  UInt128 usedCells, int szCtrl, bool FirstB=false ){
		   // GEN : Generation no. Mainly used for debugging.	   
		    if( Break_ALSChain )  return;
			if( szCtrl < 0 ){ szCtrl_enough=false; return; }
			
			var (UAF,noF) = UAfcs;  //... The second term is not a bit representation.
					if(debugPrint)  WriteLine( $" <<< {GEN} >>> {ALSChain_ToLinksString_Debug(ULG_000,UAfcs)}" );	

			{	// ===== solution found ====
				int noS = ULG_000.no;
				if( !FirstB && UAF.FreeB.DifSet(1<<noF).IsHit(noS) ){
					if( (UAF.connected81_9[noS] & ULG_000.b081) > UInt128.Zero ){ 
						if(debugPrint) WriteLine( $"\n  >>>>> {ALSChain_ToLinksString_Debug(ULG_000,UAfcs)} *** ALS_Chain" );	

						long hv = GetHashCode(ULG_000,UAfcs);
						if( eALS_Chain_Solution.Contains(hv) )  return;
						eALS_Chain_Solution.Add(hv);

						ALS_Chain_SolResult( ULG_000, UAfcs );

						SolCode = 2;
						if( !pAnMan.IsContinueAnalysis() ){ Break_ALSChain=true; return; } // @is Valid
						return;
					}
				}
			}

			// ===== Find the next ALS links ... ALS-A => RCC.Difset(1<<noF) => ALS-B
			int noFB = 1<<noF;
			if( UAF.ConnectedALS == null )  return;
			var UA_NextList = UAF.ConnectedALS.FindAll( QX => _IsConnected(QX,noFB,usedCells) );
					bool _IsConnected( (UAnLS,int) _QX, int _noFB, UInt128 _usedCells){
						var (_UA,_RCC) = _QX;
						bool chk = (_RCC&_noFB)>0 && _RCC.DifSet(_noFB)>0 && (_usedCells&_UA.bitExp)==UInt128.Zero;
						return chk;
					}
			if( UA_NextList.Count == 0 )  return;

			// ===== Set next ALS
			foreach( var UANconn in UA_NextList ){
				var (UAN,no2B) = UANconn;
				UAN.preSubset_no = UAfcs;
				UAN.preSubset_no = UAfcs;

				UInt128 usedCells2 = usedCells | UAN.bitExp;
				no2B = no2B.DifSet(1<<noF);
				foreach( int no2 in no2B.IEGet_BtoNo() ){ 
					
					(UAnLS,int) UANconn2 = ((UAnLS)UAN,no2);
					Search_ALSChain( GEN+1, ULG_000, UANconn2, usedCells2, szCtrl-UAN.Size );  //@<<< Recursive Search >>>
				}
			}
			return;
		
					long GetHashCode( ULogical_Node ULG, (UAnLS,int) UA ){
						long hv = (long)ULG.GetHashCode();
						int kx = 0;
						(UAnLS,int) Q = UA;
						while( Q.Item1!=null ){
							hv ^= ( Q.GetHashCode() ^(++kx).GetHashCode() );
							Q = Q.Item1.preALS_no;
						}
						return hv;
					}
		}

        private void ALS_Chain_SolResult( ULogical_Node ULG_000, (UAnLS,int) UAsol ){// { int solB, UInt128[] sol81, UAnLS UAnext ){    
            if( SolInfoB ){
                List<UAnLS> ALSChain = new();
                var P = UAsol;
                while(P.Item1!=null){ ALSChain.Add(P.Item1); P=P.Item1.preALS_no; } 
                ALSChain.Reverse();

                Color crBG, crElm=Colors.LightPink;
				int nx=0, solB=1<<ULG_000.no;
                foreach( var UA in ALSChain ){
                    crBG = _ColorsLst[ (nx++)%_ColorsLst.Length ]; 
                    UA.UCellLst.ForEach( UC => UC.Set_CellColorBkgColor_noBit( noB:solB, clr:AttCr, clrBkg:crBG) );
                }

                string stRCNO = ""; 
				int noSol = ULG_000.no;
				UCell UCstem = pBOARD[ULG_000.rc];
				UCstem.CancelB = UCstem.FreeB & (1<<noSol);

				string st = $"ALS_Chain\n Stem:{ULG_000.rc.ToRCString()}#{ULG_000.no+1}"; 
                string stChain = ALSChain_ToLinksString(ULG_000,UAsol);//ALSChain_ToLinksString(UAnext);

                Result = $"{st.Replace("\n","")}" ;
                ResultLong = $"{st}\n{stChain}";
				extResult  = ResultLong;
            }
        }

        private string ALSChain_ToLinksString_Debug( ULogical_Node ULG_000, (UAnLS,int) UA_no_tmp ){
            List<(UAnLS,int)> ALSChain = new();
            (UAnLS,int) Q = UA_no_tmp;
            while(Q.Item1!=null){ ALSChain.Add(Q); Q=Q.Item1.preALS_no; } 
            ALSChain.Reverse();

			string st = $"[{ULG_000}]";
            foreach( var (q,no2) in ALSChain ){
                st += $" -> #{no2+1} -> [{q.ID}@ {q.bitExp.ToRCStringComp()} #{q.FreeB.ToBitStringN(9)}]";
            }
            return st;
        }

        private string ALSChain_ToLinksString( ULogical_Node ULG_000, (UAnLS,int) UA_no_tmp ){
            List<(UAnLS,int)> ALSChain = new();
            (UAnLS,int) Q = UA_no_tmp;
            while(Q.Item1!=null){ ALSChain.Add(Q); Q=Q.Item1.preALS_no; } 
            ALSChain.Reverse();

			string st = "";// $" [{ULG_000}]";
			//string space = new string(' ',st.Length+5);
			bool first=true;
            foreach( var (q,no2) in ALSChain ){
				if(!first)  st += $" -> #{no2+1}";
                st += $"\n   -> [{q.bitExp.ToRCStringComp()} #{q.FreeB.ToBitStringN(9)}]";
				first = false;
            }
			st += $" -> #{ULG_000.no+1}\n   -> [{ULG_000.rc.ToRCString()}#{ULG_000.no+1}]";
			st = st.Replace("+","");
            return st;
        }
    }
}