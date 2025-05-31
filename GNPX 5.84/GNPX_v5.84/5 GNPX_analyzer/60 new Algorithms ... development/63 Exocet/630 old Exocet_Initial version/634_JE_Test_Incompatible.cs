using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;

namespace GNPX_space{

	// Reference to SudokuCoach.
	// https://www.taupierbw.be/SudokuCoach/SC_Exocet.shtml 

    public partial class JExocet_TechGen: AnalyzerBaseV2{

		private int  Test_Incompatible( UExocet Exo, bool debugPrint=false ){
			
			int		FreeB0 = Exo.FreeB0;						// Updated with CancelB

			Exocet_Create_Firework( Exo, debugPrint:false );
			List<UFireworkExo> fwList0 = Exo.fwList;			// Complete FW(S-W link).
			if( Exo.ExocetName == "JE1" )  return FreeB0;

			List<(int,int)> AllPairs=new(), UnsuitablePair=new();

			UInt128 Object1=Exo.ExG1.Object81, Object2=Exo.ExG2.Object81;

			foreach( var (no1,no2,UR4B) in _IEGet_Permutation_no(Exo) ){
				int       no1B=1<<no1, no2B=1<<no2, URBlk ;
				(int,int) noT12 = (no1,no2);
				AllPairs.Add(noT12);													// AllPairs : All permutation patterns

				UFireworkExo fw1 = fwList0.Find( P => Object1.IsHit(P.rc1) && (P.FreeB&no1B)>0 );
				UFireworkExo fw2 = fwList0.Find( P => Object2.IsHit(P.rc1) && (P.FreeB&no2B)>0 );
				if( fw1==null || fw2==null )  continue;
						if(debugPrint) WriteLine( $"\nfw1:{fw1}\nfw2:{fw2}" );

				if( fw1.rc2 == fw2.rc2 )	UnsuitablePair.Add(noT12) ;					// fw1.rc2 and fw2.rc2 link to the same cell in S0
				else if( fw1.rc2.B()==fw2.rc2.B() && UR4B )	 UnsuitablePair.Add(noT12);	// occupy the same block & UR => both be eliminated.
			}

			List<(int,int)> ValidPairs = AllPairs.Except(UnsuitablePair).ToList();		// ValidPairs : Valid Pair patterns
					if( debugPrint ){
						if( ValidPairs!=null ) ValidPairs.ForEach( p=> WriteLine( $"ValidPairs (#{p.Item1+1},#{p.Item2+1})" ) );
						UnsuitablePair.ForEach( P=> WriteLine( $"UnsuitablePair (#{P.Item1+1},#{P.Item2+1})" ) );
					}
			var (rcB1,rcB2) = Exo.Base81.BitToTupple();		
			UCell UB1=pBOARD[rcB1], UB2=pBOARD[rcB2];
			if( ValidPairs!=null && ValidPairs.Count>0 ){ // Incompatibility exclusions in base digits
				int FreeB0_Valid = ValidPairs.Aggregate( 0, (a,p) => a |= (1<<p.Item1)|(1<<p.Item2) );
				UB1.CancelB |= UB1.FreeB.DifSet(FreeB0_Valid);
				UB2.CancelB |= UB2.FreeB.DifSet(FreeB0_Valid);

				//Exo.FreeB0 = 
				FreeB0 = Exo.FreeB0;							// @@ FreeB0 will be changed based on the analysis results so far.
			}

			// --- Compatible digit check result ---
			if( UnsuitablePair!=null && UnsuitablePair.Count()>0 && ValidPairs!=null ){
				string stU = UnsuitablePair.Aggregate( "#", (a,p)=> a+ $"({p.Item1+1},{p.Item2+1}) " );
				stU = stU.Trim().Replace(")(", "),(");
				string stV = ValidPairs.Aggregate( "#", (a,p)=> a+ $"({p.Item1+1},{p.Item2+1}) " );
				stV = stV.Trim().Replace(")(", "),(");

				ElementElimination_Manager_UT("@");

				string st = $"Compatible digit check ... (in development)" ;
				st += $"\n incompatible pair(T1,T2) : {stU}";
				st += $"\n        valid pair(T1,T2) : {stV}";
				ElementElimination_Manager_rB( Exo, 999, 0, st );

				if( UB1.CancelB>0 && UB1.CancelB.BitCount()==1 && UB1.CancelB==UB2.CancelB ){
					int no = UB1.CancelB.BitToNum(9);	
					st = $" {Exo.Base81.ToRCStringComp()} #{no+1} is negative.";
					ElementElimination_Manager_Bn( Exo, ELM81:Exo.Base81, no:no, st );
				}	
				else{
					if( UB1.CancelB>0 ){
						st = $" {UB1.rc.ToRCString()}#{UB1.CancelB.ToBitStringN(9)} is negative.";
						ElementElimination_Manager_rB( Exo, rcE:UB1.rc, noB:UB1.CancelB, st );
					}
					if( UB2.CancelB>0 ){
						st = $" {UB2.rc.ToRCString()}#{UB2.CancelB.ToBitStringN(9)} is negative.";
						ElementElimination_Manager_rB( Exo, rcE:UB2.rc, noB:UB2.CancelB, st );
					}
					if( (UB1.CancelB|UB2.CancelB)==0 ){
						st = $" No invalid pair(T1, T2) was found";
						ElementElimination_Manager_st( st );
					}
				}
			}
			return  FreeB0;


					// ===== inner function =====
					IEnumerable<(int,int,bool)> _IEGet_Permutation_no( UExocet Exo ){
						int		FreeB0 = Exo.FreeB0;
						int     FreeB_Obj1=Exo.ExG1.FreeB_Object81, FreeB_Obj2=Exo.ExG2.FreeB_Object81;
						int     digitsCommonToBase = FreeB_Obj1 & FreeB_Obj2;

						Permutation prm = new( FreeB0.BitCount(), 2 );
						List<int>  noList = FreeB0.BitToNumList().ConvertAll(p=>(p-1));
						int skip=2;
						while( prm.Successor(skip:skip) ){
							int no1=noList[prm.Index[0]];
							if( !FreeB_Obj1.IsHit(no1) ){ skip=0; continue; }

							skip = 1;
							int no2=noList[prm.Index[1]];
							if( !FreeB_Obj2.IsHit(no2) ) continue;
							 
							bool UR4B = digitsCommonToBase.IsHit(no1) & digitsCommonToBase.IsHit(no2);

							yield return (no1,no2,UR4B);
						}
						yield break;
					}

		}



		private bool Exocet_Create_Firework( UExocet Exo, bool debugPrint=false ){
			// FW was used as a structural expression (connection of S-W links).
 
			List<UFireworkExo> fwList  = _Get_UFireworkExo( Exo, Exo.ExG1, debugPrint:false ); // Find Fireworks from Target and the crossed direction of dir.)
			List<UFireworkExo> fwList2 = _Get_UFireworkExo( Exo, Exo.ExG2, debugPrint:false );
			fwList.AddRange(fwList2);
							if( debugPrint && fwList.Count()>0 ){ foreach( var (P,kx) in fwList.WithIndex() ) WriteLine($"    #{kx: 0} : {P}"); }

			Exo.fwList = fwList.FindAll( P=> P.sw && P.IsComplete );	//Select complete FW(S-W link).

			return true;

					// ----- inner functions -----
					List<UFireworkExo> _Get_UFireworkExo( UExocet Exo, UExocet_elem ExGM, bool debugPrint=false ){
						// Use the firework concept for the link that connects "Target_cell(Object) -> SLine_X -> SLIne0".
						//  This part operates relatively infrequently, so simplicity is prioritized over efficiency.

						UInt128 SLine0   = Exo.ExG0.SLine_Sx;
						int		FreeB0   = Exo.FreeB0;
						UInt128 SLine_Sx = ExGM.SLine_Sx;

						// ... Support for Object types ...
						List<(int,int,int)> rc_h_noList= new();		// (rc, h, no)

						// ... Support for Object types ... 
						// If Target is type Object, the starting point of FW is represented by the smaller "rc".
						UInt128 FWObject = ExGM.Object81;
						foreach( var uc in FWObject.IEGet_UCell(pBOARD) ){
							foreach( int no in (uc.FreeB & FreeB0).IEGet_BtoNo() ){
								var tap = (uc.rc,(1-Exo.dir,uc.rc).DirRCtoHouse(),no);
							    if( rc_h_noList.FindIndex(t=> t.Item2==tap.Item2 && t.Item3==tap.Item3  ) >= 0 ) continue; // Omit elements in the same direction and #no 
								rc_h_noList.Add( tap );		
							}
						}

							if(debugPrint) foreach( var (tap,kx) in rc_h_noList.WithIndex() )  WriteLine( $" rc_h_noList[{kx}] : {tap}" );

						// ... Search Firework ...
						List<UFireworkExo>  FW_List = new(); 
						foreach( var (rcTagX,h,noX) in rc_h_noList ){
							foreach( var LK1 in CeLKMan.CeLK81[rcTagX]. Where( q=> SLine_Sx.IsHit(q.rc2) && q.no==noX ) ){
								// LK1 : Target1 -> SlinkX
								int n = CeLKMan.CeLK81[LK1.rc2].Count;
								if( n > 0 ){
									n=0;
									foreach( var LK2 in CeLKMan.CeLK81[LK1.rc2]. Where( q=> SLine0.IsHit(q.rc2) && q.no==noX ) ){
										// LK2 : SlinkX -> SLink0
										bool swType = (FreeCell81b9[LK1.no] & SLine_Sx).BitCount() == 1;	//LK1.type==1

										UFireworkExo UFW = new( swType, rcStem:LK1.rc2, no:LK1.no, rc1:rcTagX, rc2:LK2.rc2, Alignment:false );
										if( !FW_List.Contains(UFW) ) FW_List.Add(UFW);	// ... Complete Firework
										n++;
									}
								}

								if( n == 0 ){
									UFireworkExo UFW = new( false, rcStem:LK1.rc2, no:LK1.no, rc1:rcTagX, rc2:-1, Alignment:false );
									if( !FW_List.Contains(UFW) ) FW_List.Add(UFW);		// ... Incomplete Firework
								}
							}

							UCell UCX = pBOARD[rcTagX];
							UInt128[] Fixed81bA = Fixed81B9;
							foreach( var no in (UCX.FreeB&FreeB0).IEGet_BtoNo() ){
								try{
									int     noB = 1<<no;
									UInt128 Fixed81b = Fixed81bA[no];
									UInt128 Fixed_SLine0B = Fixed81b & SLine0;

									if( Fixed_SLine0B == UInt128.Zero )  continue;
									int rc = Fixed_SLine0B.BitToNum(81);
									int rcStem = (ConnectedCells81[rc] & SLine_Sx).BitToNum(81);
									UFireworkExo UFW = new( rcStem, no, rcTagX, rc );
									if( !FW_List.Contains(UFW) ) FW_List.Add(UFW);			// ... FixedCell Firework
								}
								catch(Exception e){ WriteLine( $"{e.Message}\n{e.StackTrace}" ); }
							}
						}
								if( debugPrint && FW_List.Count()>0 )  foreach( var (p,kx) in  FW_List.WithIndex() ) WriteLine( $" #{kx} {p}" );
						return  (FW_List);
					}
		}


		// ..... under development .....
		private bool Proof_of_UR( UInt128 Block_nUR, int rc0, int no ){
			UInt128 B81 = pBOARD.Create_Current_bitExp81_9()[no]; 
				WriteLine( $"B81      {B81.ToBitString81N()}" );
			B81 = B81.DifSet( Block_nUR );
				WriteLine( $"B81.Diff {B81.ToBitString81N()}" );

													//	int pack(int rc, int no, int PN ) => PN<<27 | no<<18 | rc;
													//	(int,int,int) unpack(int P) => (P>>27,(P>>18)&0xF,P&0xFF);

					WriteLine( $"### Proof_of_UR            B81:{B81.ToBitString81N()}" );

			UInt128 usedB = UInt128.Zero;
            var QueTupl = new Queue< (int,int,int) >();
			var FwdReference = new (int,int,int)[81];
			for( int k=0; k<81; k++ )  FwdReference[k].Item1 = -9;

			int noP = no+1;
            QueTupl.Clear();								//Queue(QueTupl) initialization
            QueTupl.Enqueue( (rc0,noP,1) );
       
            usedB = UInt128.Zero;
            while( QueTupl.Count>0 ){
                var (rc1,no1P,pn1) = QueTupl.Dequeue();		//Get Current Cell
                usedB = usedB.Set(rc1);
                int pn2 = 1-pn1;							//color inversion

                UInt128 Chain = _Get_ConnectedLink_withSW( B81, rc1, pn1 );
				if( Chain == qZero )  continue;
								WriteLine( $"### Proof_of_UR rc1:{rc1.ToRCString()} Chain:{Chain.ToBitString81N()}" );

				foreach( var rc2 in Chain.IEGet_rc().Where(rc=> rc==rc0 || !usedB.IsHit(rc)) ){ 
							WriteLine( $" {(rc1,noP,pn1)} rc2:{rc2.ToRCString()} .. {_stPath( FwdReference, rc2,noP,pn2)}" );
					if( rc2 == rc0 )  return (pn2==1);		// true: UR is established
					QueTupl.Enqueue( (rc2,noP,pn2) );
					FwdReference[rc2] = (rc1,noP,pn1);
					usedB = usedB.Set(rc2);							
				}
            }
                
			return false;

					UInt128 _Get_ConnectedLink_withSW( UInt128 B, int rc, int PosNeg/* 1,0 */ ){
						UInt128 BX = B & ConnectedCells81[rc], BXw;
						if( PosNeg == 0 ){
							BX = qZero;
							if( (BXw=B & HouseCells81[rc/9]).BitCount() == 2 )      BX |= BXw;
							if( (BXw=B & HouseCells81[(rc%9)+9]).BitCount() == 2 )  BX |= BXw;
							if( (BXw=B & HouseCells81[rc.B()+18]).BitCount() == 2 ) BX |= BXw;
						}
						return BX;
					}

					string _stPath( (int,int,int)[] FwdReference, int rcP, int noP, int pnP ){
						List< (int,int,int) > L = new();
						L.Add( (rcP,noP,pnP) );

						int rcX=rcP, loopC=0;
						do{
							var Q = FwdReference[rcX];
							if( Q.Item1 < 0 )  break;
							rcX = Q.Item1;
							L.Add( Q );
						}while(++loopC<20);
						L.Reverse();
						var stL = L.ConvertAll( Q => $"{Q.Item1.ToRCString()}#{Q.Item2}{(Q.Item3>0 ?" - ":" = ")}" );

						string st = string.Join("",stL);
						st = st.Substring(0,st.Length-2);
						return st;
					}
		}	


	}


}
