using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using static System.Math;
using System.IO;

using GIDOO_space;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace GNPX_space{
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//   under development (GNPXv6)
	//   @@@@@ There are still discrepancies between the Sudoku-6 HP and GNPX-6 codes. @@@@@
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

	using G6_SF = G6_staticFunctions;

    public partial class Senior_Exocet_TechGen : AnalyzerBaseV2{

		public (int,int) SExocet_Valid_TargetPair_phase5( USExocet SExo, bool debugPrint=false ){
			UCoverLine  ExG1=SExo.ExG1, ExG2=SExo.ExG2;
			int		FreeB0 = SExo.FreeB0;
		
			int FreeB_Covered_2CL = SExo.CoverLine_by_Size[2];
			int noB_Tag1 = ExG1.FreeB_Object81 & FreeB_Covered_2CL;
			int noB_Tag2 = ExG2.FreeB_Object81 & FreeB_Covered_2CL;

			var  (noB1,noB2) = (0,0);
			if( noB_Tag1>0 && noB_Tag2>0 ){
				int skip=9;
				foreach( var (no1,no2) in G6_SF.PairGenerator(noB_Tag1,noB_Tag2,skip,uniqueB:true) ){
					noB1 |= 1<<no1;
					noB2 |= 1<<no2;
				}
			}
			else if( noB_Tag1>0 )  noB1 |= noB_Tag1;
			else if( noB_Tag2>0 )  noB2 |= noB_Tag2;

			return (noB1,noB2);
		}





		public int SE_ph4_IsCovered( USExocet SExo, bool debugPrint=false ){
			UCoverLine  ExG1=SExo.ExG1, ExG2=SExo.ExG2;
		int		FreeB0 = SExo.FreeB0;
			UInt128 Base_Object = SExo.Base81 | SExo.Object81;	
						if(debugPrint){
							_Information_4_Debugging(SExo);
							G6_SF.__MatrixPrint( qZero, SExo.SLine012_or, "SExo.SLine012_or" );
						}

			SExo.CoverStatusList = new UCoverStatusA[9];
			SExo.CoverLine_by_Size = new int[4];
			SExo.CrossCoverLineB   = new (int,int)[3];

					//debugPrint = true;

			int[] house_CL = _Gen_house_CoverLine_withCrossParallel(SExo);				// ---> house no. array of CoverLines
						// foreach( var (P,kx) in house_CL.WithIndex() )  WriteLine( $"house_CL[{kx,2}] : {P,3}" );
			
			foreach( var no in FreeB0.IEGet_BtoNo() ){
				int noB = 1<<no;
				UInt128 SLine_no = SExo.SLine012_or & FreeAndFixed81B9[no];
						if(debugPrint){ 
							string stV = __VectorPrint(SLine_no,house_CL);
							WriteLine( $"no:#{no+1} SLine_no:{SLine_no.TBS()}\n{stV}" );
							G6_SF.__MatrixPrint( Flag:Base_Object, SExo.SLine012_or, FreeAndFixed81B9[no], SLine_no,"SExo.SLine012_or, FreeAndFixed81B9[no], SLine_no" );
						}

				if( SLine_no==qZero ){
					SExo.CoverLine_by_Size[0] |= noB;
				}
				else{	// <<< _Get_CoverLine
					UCoverStatusA UCL = __Get_CoverLine( no, SLine_no, house_CL, debugPrint:true );
					if( UCL !=null ){
						int sz = UCL.sz;									// sz=3 ... Wildcard ...
						SExo.CoverStatusList[no] = UCL;
						SExo.CoverLine_by_Size[sz] |= noB;
					}
				}
			}

			foreach( var (SL,kx) in SExo.SLine012_List.WithIndex() )  SExo.CrossCoverLineB[kx] = SL.Get_SingleMore();
						/*
							foreach( var (SL,kx) in SExo.SLine012_List.WithIndex() ){
								var CCL = SL.Get_SingleMore();
								SExo.CrossCoverLineB[kx] = CCL;
								WriteLine( $"SExo.CrossCoverLineB{kx} oneF:{CCL.Item1.TBS()}  twoF:{CCL.Item2.TBS()} " );
							}
						*/
			
			int FreeB_Covered_2CL = SExo.CoverLine_by_Size[2];	//@[Att]  Bird@Doc. Rule-1
						if(debugPrint){ 
							string  stCoverLines = SExo.CoverStatusList.Where(p=>p!=null).Aggregate(" ",(a,ch)=> a+ $"\n {ch}");
							WriteLine( $"\nCoverLines\n{stCoverLines}" );

							WriteLine( $"FreeB_Covered_2CLtest2:{FreeB_Covered_2CL.TBS()}" );
							foreach( var (P,kx) in SExo.CoverLine_by_Size.WithIndex() ) WriteLine( $"  SExo.CoverLine_by_Size[{kx}] : {P.TBS()}" );
						} 

			return  FreeB_Covered_2CL;




					// ------------------------------------------------------------------------
					int[] _Gen_house_CoverLine_withCrossParallel( USExocet SExo ){
						int  dir = SExo.dir;
						int  hStart = (dir==0)? 0: 9;
						int  hStem  = (dir,SExo.rcStem).DRCHf();

						List<int> house_CL = Enumerable.Range(hStart,9).Where(n=> n!=hStem).ToList();
						List<int> hCross   = SExo.CL012.ToList().ConvertAll(p=>p.h+100);	// +100: CrossLine indicator
						house_CL.AddRange(hCross);
						return house_CL.ToArray();
					}


					// ------------------------------------------------------------------------
					void  _Information_4_Debugging( USExocet SExo ){
						string st = $" Exo_0 dir:{SExo.dir} rcStem:{SExo.rcStem}  baseCells:{SExo.Base81.TBScmp()}" ;
						st += $"  Object 1-2 : {ExG1.stObject} - {ExG2.stObject}";
						WriteLine( st );

							string[]  Marks = new string[81];
							var (rcB1,rcB2) = SExo.Base81. BitToTupple();
							if( rcB1>=0 ) Marks[rcB1] = "b"; 
							if( rcB2>=0 ) Marks[rcB2] = "b"; 
							foreach( var rc in ExG1.Object.IEGet_rc() )  Marks[rc] = "t1";
							foreach( var rc in ExG2.Object.IEGet_rc() )  Marks[rc] = "t2";

							//pBOARD.__CellsPrint_withFrame( Marks,  "SEBase,ExG1,ExG2" );
							pBOARD._Dynmic_CellsPrint_withFrame( Marks,  "***" );

						{
							WriteLine( $"\n dir:{SExo.dir}  Base:{SExo.stBase}  T1:{SExo.ExG1.stObject}  T2:{SExo.ExG2.stObject}" );
							UInt128 _Flag = SExo.Base81 | ExG1.Object | ExG2.Object;
							if(debugPrint)  G6_SF.__MatrixPrint( Flag:_Flag, SExo.CrossLine_012, SExo.SLine012_or, "CrossLine_012 SLine_012" );
						}
					}


					// ------------------------------------------------------------------------
					UCoverStatusA __Get_CoverLine( int no, UInt128 SLine_no, int[] house_CL, bool debugPrint=false  ){
						foreach( int h in house_CL ){
							UInt128 CLH0 = HC81[h%100];
							if( SLine_no.DifSet(CLH0) == qZero ) return  new UCoverStatusA( no, 1, h, -1, -1 );
						}

						foreach( int h1 in house_CL.Where(h=>h<100) ){
							UInt128 CLH1 = HC81[h1];
							if( (SLine_no&CLH1) == 0 )  continue;

							foreach( int h2 in house_CL){
								UInt128 CLH2 = HC81[h2%100];
								if( SLine_no.DifSet(CLH1|CLH2) == 0 ) return  new UCoverStatusA( no, 2, h1, h2, -1 );
							}
						}

						var ha= house_CL.ToList().Where(p=>p>=100).ToList();									
						// Compliant with requirements? : Are there any digits on all SLines?
						if( ha.All( h=> (SLine_no&HC81[h-100])>qZero ) )  return  new UCoverStatusA( no, 3, ha[0], ha[1], ha[2] );

						return  null;
					}


					// ------------------------------------------------------------------------
					string  __VectorPrint( UInt128 mat, int[] house_CL ){
						string st = "";
						foreach( int hh in house_CL.Where(p=>p>100) ){
							var (V,_) = mat.ToVectorH( hh%100);
							st += ((hh<9)? "r": "c") + ((hh%9)+1).ToString();
							st += ": " + V.ToBitString(9)+"@";
						}
						return st.Trim().Replace("@","\n");
					}

		}
	}
}