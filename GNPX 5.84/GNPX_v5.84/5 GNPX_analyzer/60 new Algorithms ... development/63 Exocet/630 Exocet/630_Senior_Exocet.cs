using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using static System.Math;
using System.IO;

using GIDOO_space;
using System.Windows.Documents;
using System.Xml.Linq;
using static GNPX_space.Senior_Exocet_TechGen;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace GNPX_space{
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//   under development (GNPXv6)
	//   @@@@@ There are still discrepancies between the Sudoku-6 HP and GNPX-6 codes. @@@@@
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

	using G6_SF = G6_staticFunctions;
	using TapleUCL = (UCoverLine,UCoverLine,UCoverLine);

    public partial class Senior_Exocet_TechGen: AnalyzerBaseV2{

		private UInt128 ObjectSq(UInt128 obj, int sq ) => obj | ((UInt128)sq)<<100;

		public bool Junior_Exocet_JE2( ) => Exocet_General( ExoControl:"name:JE2" );
		public bool Junior_Exocet_JE1( ) => Exocet_General( ExoControl:"name:JE1" );

				// Type:base : Starting point for Senior Exocet development. This version is to ensure that "Junior Exocet is included."
				//	public bool SExocet_Basic()		 => Exocet_General( ExoControl:"name:SE type:Basic"  );

		public bool SExocet()			 => Exocet_General( ExoControl:"name:SE type:Standard"  );
		public bool SExocet_Single()	 => Exocet_General( ExoControl:"name:SE type:Single"  );
		public bool SExocet_SingleBase() => Exocet_General( ExoControl:"name:SE type:SingleBase"  );



		// [TBD]  Mirror behavior in Senior Exocet

        public bool Exocet_General( string ExoControl ){
			USExocet.qBOARD = pBOARD;
			//bool debugPrint = true;

			// ::: Prepare :::
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_SExocet_TechGen( printB:false );	//debugPrint );
			}
			ElementElimination_Manager_UT("Initialize");

					


			// ::: Phase1 :::  ... An instance for determining the shape of the Base.
			foreach( var SExo in IE_SE_ph1_Set_BasicForm1_Set_BasicForm( ExoControl:ExoControl, debugPrint:false) ){			
				//if( SExo.dir!=0 || SExo.rcStem!=02 )  continue;			// ===== SE_Nxt Debug =====
				

				foreach( var _ in IE_SE_ph2_Get_TargetObject_SLine( SExo, debugPrint:false) ){
					UCoverLine ExG0=SExo.ExG0, ExG1=SExo.ExG1, ExG2=SExo.ExG2;		// SExo.Companions=SExo.Companions;
					//if( ExG1.Object!=(qOne<<70) || ExG2.Object!=((qOne<<21) | (qOne<<22)) )  goto LClear; // ===== SE_Nxt Debug =====

					int FreeB_Object = ExG1.FreeB_Object81 | ExG2.FreeB_Object81;
					if( (SExo.FreeB.DifSet(FreeB_Object)) > 0 )  goto LClear;
					int FreeB_Companions = SExo.Companions.Get_FreeB();

					if( ( SExo.FreeB & FreeB_Companions ) > 0 )  goto LClear;

								if(debugPrint){
									UInt128 ConStem = HC81[ (SExo.dir,SExo.rcStem).DRCHf() ] | HC81[SExo.rcStem.B()+18];
									UInt128 SLineBand  = BOARD_FreeCell81 .DifSet(ConStem);		//

									UInt128 Base_StemAnd = SExo.Base81 | SExo.Band81.Aggregate_ConnectedAnd();
									G6_SF.__MatrixPrint( Flag:Base_StemAnd, ExG0.SLine_x, ExG1.SLine_x, ExG2.SLine_x, "ExG0.SLine_x ExG1.SLine_x ExG2.SLine_x" );
									WriteLine( $"IE_SE_ph3_Get_TargetObject_SLine  ****  ExG1.Object: {ExG1.Object.TBScmpRC()}    ExG2.Object: {ExG2.Object.TBScmpRC()}" );
								}



					// <<< phase4 >>>  ... Search and validate CoverLines.
					int FreeB_Covered_2CL = SE_ph4_IsCovered( SExo, debugPrint:false );
					int[] Covered_noB = SExo.CoverLine_by_Size;
					int[] CL_noB_Size = Covered_noB.ToList().ConvertAll(p => p.BitCount() ).ToArray();

					// ... validation
					if( SExo.ExocetName.Contains("JE1") || SExo.ExocetNamePlus=="SE_Single" ){ // ... JE1
						if( CL_noB_Size[3]!=1 || SExo.FreeB .DifSet(Covered_noB[2] | Covered_noB[3]) > 0 )  goto LClear;
					}

					else{	// ... JE2 / SE
						if( CL_noB_Size[2]<2 || CL_noB_Size[0]>0 || CL_noB_Size[3]>0 )  goto LClear;

				/*
						if( SExo.ExocetName.Contains("JE2") ){}
						else{	// ... SE
							if( Check_CoverLine(SExo, Covered_noB) is false  ) goto LClear;
						}
				*/
					}



					//  <<< phase5 >>>  ... Check Roule
					bool solFound = SE_ph5_RouleCheck( SExo );


					// --------------------------------------------------------------------------
					// Preventive defense. This is not desirable. 
					if( !solFound || pBOARD.Any(uc=> uc.No==0 && uc.FreeB0==0) ) goto LClear; 
					// --------------------------------------------------------------------------


					//  <<< phase6 >>>  ... Reporting the results
					SE_ph6_Result( SExo, debugPrint:false );	//debugPrint );
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid


					// =================================================================================
					// <<< Final Processing >>>
				LClear:	
					foreach( var UC in pBOARD.Where(p=>p.No==0) ) UC.CancelB=0;
					pBOARD.ForEach( UC => UC.ECrLst=null );
					ElementElimination_Manager_UT("Initialize");

				}
			}
			return false;	


				/*
						bool Check_CoverLine( USExocet SExo, int[] Covered_noB ){

							UCoverLine ExG1=SExo.ExG1, ExG2=SExo.ExG2;
								//WriteLine( $"**CoverLines (p:Parallel x:Cross){SExo.stCoverLines}" );
							int Covered2B = Covered_noB[2];		// Candidate digits covered by 2-CoverLine
							// @@@ 2-CoverLineÇÃóvëf

							//Fail if there are any base digits that are not covered by 2-CL
							if( SExo.Base81.IEGet_UCell(pBOARD).Any(u=>(u.FreeB&Covered2B)==0) )  return false;	


						// @@@ à»â∫ÇÕçƒåüì¢Ç™ïKóv

							// The candidate digits of the Target is the Base-Digits and is included in the SLIne.
							int  h_CL1=ExG1.h, h_CL2=ExG2.h, exclude1=Covered2B, exclude2=Covered2B;

							foreach( var ucs in SExo.CoverStatusList.Where(p=>p!=null) ){	//excludeX : Digit of XCL is excluded from Object-X.
								int noB = 1<<ucs.no;
								if( ucs.CLH_012.Any( ch => (ch%100)==h_CL1) )  exclude1 |= noB;		
								if( ucs.CLH_012.Any( ch => (ch%100)==h_CL2) )  exclude2 |= noB;
									//WriteLine( $" no:#{ucs.no+1}  exclude1:{exclude1.TBS()}  exclude2:{exclude2.TBS()}" );  
							}
							
							int fb1 = (ExG1.FreeB_Object81 & SExo.FreeB0) .DifSet(exclude1);
							int fb2 = (ExG2.FreeB_Object81 & SExo.FreeB0) .DifSet(exclude2);

							int FreeBTest = SExo.FreeB0 .DifSet(fb1|fb2);
							bool valid = FreeBTest.BitCount()>=2;



							return valid;
						}
			*/

		}




		private void SE_ph6_Result( USExocet SExo, bool debugPrint ){
			string  ExocetName=SExo.ExocetName, ExocetNamePlus=SExo.ExocetNamePlus;

			UCoverLine ExG1=SExo.ExG1, ExG2=SExo.ExG2;
			var		 (rcBase1,rcBase2) = SExo.Base81.Get_rc1_rc2();
			
			int FreeB0 = SExo.FreeB0;
			UInt128	 SLine1=ExG1.SLine_x, SLine2=ExG2.SLine_x;

			// <<<<< Result ...  coloring, create message  >>>>>
			try{
				SolCode = 2;

			  // ::: Base
				SExo.Base81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr );	

			  // ::: Object, Companion, Mirror
				Color cr = SolBkCr2G;
				int FB0 = FreeB0;
				if( (ExocetName.Contains("JE1") || SExo.ExocetNamePlus.Contains("_Single")) && SExo.ExG1.wildcardB && ExG1.Object!=_na_){
					SExo.ExG1.Object.IE_SetNoBBgColor_All( pBOARD, AttCr, SolBkCr2G ); 
				}

				if( ExocetNamePlus.Contains("SE_SingleBase") ){
					if( SExo.ExG1.phantomObjectB ) SExo.ExG1.Object.IE_SetNoBBgColor_All( pBOARD, AttCr, SolBkCr2G ); 
					if( SExo.ExG2.phantomObjectB ) SExo.ExG2.Object.IE_SetNoBBgColor_All( pBOARD, AttCr, SolBkCr2G ); 
				}
				else{
					SExo.ExG1.Object.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2G );
					if(ExG2.Object!=_na_)  SExo.ExG2.Object.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2G ); 			
				}

				SExo.ExG1.Object.IE_SetNoBBgColor_All( pBOARD, AttCr, SolBkCr2G ); 
				SExo.ExG2.Object.IE_SetNoBBgColor_All( pBOARD, AttCr, SolBkCr2G ); 

				if(SExo.Companions!=_na_) SExo.Companions.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr2 );
						
				ExG1.Mirror81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr6 );
				ExG2.Mirror81.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr6 );

			// ::: SLine
				SExo.ExG0.SLine_x.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr4 );
				SExo.ExG1.SLine_x.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr3 );
				SExo.ExG2.SLine_x.IE_SetNoBBgColorNSel( pBOARD, FreeB0, AttCr, SolBkCr3 );



			// ::: Report
				string stBase  = $"B:{SExo.Base81.TBScmp()}#{SExo.FreeB0.ToBitStringN(9)} Companion:{SExo.stCompanions}";
				string stObjL1 = $"T1:{ExG1.stObject} M1:{ExG1.stMirror81}";
				string stObjL2 = $"T2:{ExG2.stObject} M2:{ExG2.stMirror81}";
				
				string stBaseL  = $"   Base: {SExo.Base81.TBScmp()}#{SExo.FreeB0.ToBitStringN(9)}";
				string stObjL1L = $"Object1: {ExG1.stObject}  Mirror1:{ExG1.stMirror81}";
				string stObjL2L = $"Object2: {ExG2.stObject}  Mirror2:{ExG2.stMirror81}";
				string stWildcard = (SExo.WildCardB!=0)? $"Wildcard: #{SExo.WildCardB.ToBitStringN(9)}": "";

				Result     = $"Åü{ExocetNamePlus} {stBase} T1:{ExG1.stObject} T2:{ExG2.stObject}";
				
				string stL = $"{stBaseL}\n   {stObjL1L}\n   {stObjL2L}\n   Companions:{SExo.stCompanions}";
				ResultLong = $"Senior Exocet_NXG_{ExocetNamePlus}\n   {stL}";
				extResult = $"Senior Exocet_NXG_{ExocetNamePlus}\n  @dir:{SExo.dir}  @Stem:{SExo.rcStem.ToRCString()}\n\n   {stL}";
				
				if( SExo.ExocetName.Contains("JE1") || SExo.ExocetNamePlus.Contains("_Single") ){
					ResultLong += $"\n   {stWildcard}";
					extResult  += $"\n   {stWildcard}";
				}

				extResult += $"\n\n CoverLines (p:Parallel x:Cross){SExo.stCoverLines}";
				extResult += $"\n {SExo.stCrossCoverLineB}";
				extResult += "\n\n Firework\n" + SExo.Get_FireworkList();

				extResult += $"\n\n{new string('-',80)}\n Explanation of candidate digits exclusion\n";
					string stE = string.Join( "\n", extResultLst );
					int n1=stE.Length, n2;
					do{
						stE = stE.Replace("@\n@","@");
						if( n1==(n2=stE.Length) ) break;
						n1 = n2;
					}while(true);
				extResult += stE.Replace("+\n","+").Replace("@","\n").Replace("\n\n","\n").Replace("\n","\n  ");
					if(debugPrint)  WriteLine( "@@"+extResult );
			}
			catch( Exception e ){ WriteLine( $"{e.Message}\n{e.StackTrace}" ); }

			return;
					
		}
	}
}