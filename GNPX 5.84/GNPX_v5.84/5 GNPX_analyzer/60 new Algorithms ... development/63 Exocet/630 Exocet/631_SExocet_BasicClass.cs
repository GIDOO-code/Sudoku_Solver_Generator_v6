using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

using GIDOO_space;
using static GNPX_space.Firework_TechGen;
using System.Windows.Documents;
using static GNPX_space.Senior_Exocet_TechGen;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Buffers.Text;
//using static GNPX_space.JExocet_TechGen;

namespace GNPX_space{
		using G6_SF = G6_staticFunctions;
    public partial class Senior_Exocet_TechGen: AnalyzerBaseV2{
		//static UInt128 _na_ = UInt128.MaxValue;
		static char[] sep=new Char[]{ ' ', ',', '\t' };  

	//	static public string  TBSP( UInt128 Q) => ((Q==_na_)? "n/a": Q.TBS());
	//	static private string rcSt( int rc ) => (rc>=0)? rc.ToRCString_NPM(): " n/a";


	// ===================================================
	// For future expansion, this is over-implemented.
	// ===================================================

	// ::: Code to support the Object extension of Target.
	// ::: If Target is a Cell, there is simpler code.



		public partial class  USExocet{
			static public List<UCell>   qBOARD;	
			static public UInt128		BOARD_FreeCell81;
			
			public string		ExocetControl = null;

			public string		ExocetName;
			public string		ExocetNamePlus;

			public int			dir;			// 0:row, 1:column
			public int			rcStem;

			public UInt128		Base81;
			public UInt128		Band81;
			public UInt128		EscapeCells;
			public UInt128		Companions = _na_;

			//  [TBD] ... Class UCoverLine0 will eventually be integrated.

			public UCoverLine0  CoverLine0;	// ... Base
			public UCoverLine0  CoverLine1;	// ... CrossLine1
			public UCoverLine0  CoverLine2;	// ... CrossLine2

			public UCoverLine	ExG0;	
			public UCoverLine	ExG1;			//
			public UCoverLine	ExG2;			//

			public List<UFireworkExo> fwList;

			public UCoverStatusA[] CoverStatusList;
			public int[]		   CoverLine_by_Size;
			public (int,int)[]	   CrossCoverLineB;

			public List<int> ProcessedTargetsList;

			public USExocet( string ExocetName, int dir, int rcStem, UInt128 Base81, int freeB0, UInt128 BOARD_FreeCell81 ){
				this.ExocetControl = ExocetName;
				__Set_SEName();

				this.dir	 = dir;
				this.rcStem	 = rcStem;
				this.Base81	 = Base81;
				this.Band81  = _Band81();
				this.EscapeCells = Base81.Aggregate_ConnectedAnd() & (HC81[rcStem/9] | HC81[rcStem%9+9]);

				int hCL0 = (1-dir,rcStem).DRCHf();
				UInt128 CrossLine0 = HouseCells81[hCL0] .DifSet(HouseCells81[rcStem.B()+18]);
				UInt128 SLine0  = CrossLine0;
				CoverLine0 = new( this, 0,hCL0,CrossLine0,SLine0);
				SLine012 = [SLine0]; 
				SLine012.Add(SLine0); SLine012.Add(SLine0); 

						//string DB_title = $"CrossLine0 SLine0_Base";
						//G6_SF.__MatrixPrint( Flag:Base81, CrossLine0, SLine0_Base, DB_title );

				ProcessedTargetsList = new();
			}

			public void Set_CrossLine_SLineBase( (int,int,int) h012 ){
				var (h0,h1,h2) = h012;

				int h1CL=(1-dir,h1).DRCHf(), h2CL=(1-dir,h1).DRCHf();
				UInt128 CrossLine1 = HouseCells81[h1];
				UInt128 CrossLine2 = HouseCells81[h2];
						//string DB_title = $"CL1, CL2";
						//G6_SF.__MatrixPrint( Flag:Base81, CrossLine1, CrossLine2, DB_title );

				UInt128 SLine1 = CrossLine1.DifSet(EscapeCells);
				UInt128 SLine2 = CrossLine2.DifSet(EscapeCells);
				CoverLine1 = new UCoverLine0( this, sq:1, h:h1, CrossLine:CrossLine1, SLine0:SLine1 );  
				CoverLine2 = new UCoverLine0( this, sq:2, h:h2, CrossLine:CrossLine2, SLine0:SLine2 );
				SLine012[1] = SLine1; SLine012[2] = SLine2;

						//string DB_title2 = $"CrossLine1, SLine1, CrossLine2, SLine2";
						//G6_SF.__MatrixPrint( Flag:Base81, CrossLine1, SLine1, CrossLine2, SLine2, DB_title2 );
			}
			public override string ToString( ){
				string st = $" USExocet dir:{dir} rcStem:{rcStem.ToRCString(),2} FreeB:{FreeB.TBS()}";
				st += stCLxBase() + stExGx();
				return st;
			}
		}


				public partial class  USExocet{

					public bool		BandTypeB	=> ExocetControl.Contains("basic");
			
					public int		FreeB		=> Base81.IEGet_UCell(qBOARD).Aggregate(0, (a,uc) => a| uc.FreeB );

					public int		FreeB0		=> Base81.IEGet_UCell(qBOARD).Aggregate(0, (a,uc) => a| uc.FreeB_Updated() );

					public UInt128  CrossLine_012 => CoverLine0.CrossLine | CoverLine1.CrossLine | CoverLine2.CrossLine;
					public UCoverLine0[] CL012	=> new []{ CoverLine0, CoverLine1, CoverLine2 };
					public List<UInt128> SLine012;

					public string   stBase		=> $"{Base81.ToRCStringComp()}";

					public UInt128	SLine012_or => ExG0.SLine_x | ExG1.SLine_x | ExG2.SLine_x ;

					public UCoverLine[] ExG012_List => new []{ ExG0, ExG1, ExG2 };
					public UInt128[] SLine012_List => new[]{ ExG0.SLine_x, ExG1.SLine_x, ExG2.SLine_x };

					public int[] CL_noB_Size => CoverLine_by_Size.ToList().ConvertAll(p => p.BitCount() ).ToArray();


					public string   stCoverLines => CoverStatusList.Where(p=>p!=null).Aggregate(" ",(a,ch)=> a+$"\n {ch.ToString()}");
					public string   stCrossCoverLineB => "CrossCoverLineB  one:" + CrossCoverLineB.Aggregate("", (a,b) => a+ " "+	b.Item1.TBS() ) +
														 "  two:"+ CrossCoverLineB.Aggregate("", (a,b) => a+ " "+	b.Item2.TBS() );
					
					public int		WildCardB	 => CoverLine_by_Size[3] ;
					

					// <<< ExocetControl --> ExocetName, ExocetNamePlus >>>
					public string Enquire( string que ){
						List<string> eLst = ExocetControl.Split(sep,StringSplitOptions.RemoveEmptyEntries).ToList();
						string?      q = eLst.FindLast(p=>p.Contains(que));
						if( q != null )  return q;
						return "";
					}

					private void __Set_SEName(){
						ExocetName = Enquire("name").Replace("name:","");

						string SE_type  = Enquire("type").Replace("type:","");
						if( SE_type != "" ) SE_type = "_"+SE_type;
						ExocetNamePlus = ExocetName + SE_type;
					}

					private UInt128  _Band81(){
						int block0=rcStem.B(), Block1=0, Block2=0, shiftV=block0/3*3;
						if( dir==0 ){ Block1=(block0+1)%3+shiftV; Block2=(block0+2)%3+shiftV; }
						else{		  Block1=(block0+3)%9;        Block2 = (block0+6)%9; }
						UInt128 Band81 = HouseCells81[Block1+18] | HouseCells81[Block2+18];
						return  Band81;
					}

					public UInt128 Object81 => ExG012_List.Aggregate( qZero, (a,UC)=> a | (UC.Object!=_na_? UC.Object: qZero) ); 

					public string stCLxBase(){
						string st = (CoverLine0==null)?   "  CL0Base:n/a": $"\n  CL0Base:{CoverLine0}";
							   st += (CoverLine1==null)?  "  CL1Base:n/a": $"\n  CL1Base:{CoverLine1}";
							   st += (CoverLine2==null)?  "  CL2Base:n/a": $"\n  CL2Base:{CoverLine2}";
						return st;
					}

					public string   stCompanions => Companions.TBScmp();
					public string stExGx(){
						string st = (ExG0==null)?  "  ExG0:n/a": $"\n  ExG0:{ExG0}";
							   st += (ExG1==null)?  "  ExG1:n/a": $"\n  ExG1:{ExG1}";
							   st += (ExG2==null)?  "  ExG2:n/a": $"\n  ExG2:{ExG2}";
						return st;
					}

					public string  Get_FireworkList(){
						if( fwList == null )  return "";
						string st = string.Join( "\n", fwList.Select(fw=> "  "+fw.ToString_compact() ) );
						return st;
					}
				}



		public partial class UCoverLine0{
			public  USExocet SExo;
			public  int		 sq;
			public  int		 h;
			public  UInt128  CrossLine	= _na_;
			public  UInt128  SLine0		= _na_;

			public UCoverLine0(USExocet SExo, int sq, int h, UInt128 CrossLine, UInt128 SLine0) {
				this.SExo		= SExo;
				this.sq			= sq;
				this.h			= h;
				this.CrossLine = CrossLine;
				this.SLine0		= SLine0 ;

				//string DB_title = $"ExG{sq}.CrossLine0 ExG{sq}.SLine0";
				//G6_SF.__MatrixPrint( Flag:SExo.Base81, CrossLine, SLine0, DB_title );
			}
		}
/*
				public partial class UCoverLine0{ }
*/


		public partial class UCoverLine{
			static public List<UCell>	qBOARD;
			static public UInt128		BOARD_FreeCell81;

			public USExocet SExo;
			public int		sq;
			public int		h;
			public int		rcTag	   = int.MaxValue;
			public bool		wildcardB  = false;
			public UInt128  Object	   = _na_;
			public UInt128  Object_sq  = _na_;
			public bool		phantomObjectB = false;
			public UInt128  CrossLine  = _na_;
			public UInt128  SLine_x    = _na_;
			public UInt128  Mirror81F  = _na_;
			public UInt128  Mirror81   = _na_;
			public int		FreeB_SLine = 0;

			public UCoverLine(){
				this.sq = -9999;
			}

			public UCoverLine( USExocet SExo, int sq, int h, UInt128 CrossLine, UInt128 SLine ){
				this.SExo		= SExo;
				this.sq			= sq;
				this.h			= h;
				this.CrossLine  = CrossLine;
				this.SLine_x    = SLine;
			}
			public UCoverLine( USExocet SExo, int sq, int h, UInt128 CrossLine, UInt128 SLine, int rcTag ): 
								this( SExo, sq, h, CrossLine, SLine ){
				this.rcTag	    = rcTag;
				this.Object_sq	= Object | ((UInt128)sq)<<100 ;
				this.Object		= qOne<<rcTag;

			}
			public UCoverLine( USExocet SExo, int sq, int h, UInt128 CrossLine, UInt128 SLine, UInt128 Object ):
								this( SExo, sq, h, CrossLine, SLine ){
				if( Object.BitCount()==1 )  this.rcTag = Object.FindFirst_rc();
				this.Object_sq	= Object | ((UInt128)sq)<<100 ;
				this.Object		= Object & qMaxB81;
			}

			public UCoverLine( USExocet SExo, int sq, int h, UInt128 CrossLine, UInt128 SLine, UInt128 Object, UInt128 MirrorF  ):
								this( SExo, sq, h, CrossLine, SLine, Object ){
				this.Mirror81F = MirrorF;
				this.Mirror81  = MirrorF & BOARD_FreeCell81;
			}
			public override string ToString(){
				string st = $"h:{h,2} rcTag:{(rcTag<0? " n/a": rcTag.ToRCString())}  CrossLine:{CrossLine.TBSP()}  SLine_x:{SLine_x.TBSP()}";
				return st;
			}
		}

				public partial class UCoverLine{
					public int		Get_Target_FreeB_Updated_Or() => Object.IEGet_rc().Aggregate( 0x1FF, (a,rc) => a | qBOARD[rc].FreeB_Updated() );
					//public UInt128  Object81 => UInt128.One<<rcTag;		// For compatibility. Will sort it out eventually.
						
					public UInt128  Escape81  => ConnectedCells81[SExo.rcStem] & HouseCells81[ (1-SExo.dir,Object.FindFirst_rc()).DirRCtoHouse() ];

					public int      FreeB_Object81    => Object.IEGet_UCell(qBOARD) .Aggregate( 0, (a,uc)=> a| uc.FreeB_Updated() ); 
					public int      FreeB_Object81XOR => Object.IEGet_UCell(qBOARD) .Aggregate( 0, (a,uc)=> a^ uc.FreeB_Updated() ); 	
					public int      FreeB_Mirror81    => Mirror81.IEGet_UCell(qBOARD) .Aggregate( 0, (a,uc)=> a| uc.FreeB ); 
					public int      FreeB_Mirror81withFixed => Mirror81F.IEGet_UCell(qBOARD) .Aggregate( 0, (a,uc)=> a| uc.FreeB_Updated_withFixed() ); 

					public string   stSLine_x       => SLine_x.TBS();
					public string   stObject		=> Object.TBScmp();
					public string   stMirror81		=> Mirror81.TBScmp();

					public UInt128 Get_FullSight( bool withExclusion ){
						if( withExclusion ){ return Object.IEGet_rc().Aggregate( qMaxB81, (a,rc) => a & ConnectedCells81[rc].Reset(rc) ); }
						else{				 return Object.IEGet_rc().Aggregate( qMaxB81, (a,rc) => a & ConnectedCells81[rc] ); }
					}

				}




		// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
		public class UCoverStatusA{
			public int	 no;
			public int   sz;
			public int	 CLH_0 = -1;
			public int	 CLH_1 = -1;
			public int	 CLH_2 = -1;
			public int[] CLH_012 => new[]{ CLH_0, CLH_1, CLH_2 };

			public UCoverStatusA( int no, int sz, int CLH_0, int CLH_1, int CLH_2 ){
				this.no = no;
				this.sz = sz;
				this.CLH_0=CLH_0; this.CLH_1=CLH_1;	this.CLH_2=CLH_2;
			}

			public override string ToString( ){
				string st = $" no:#{no+1} size:{sz}";
				       st += $"  CoverLine: {_ToStHouse(CLH_0)} {_ToStHouse(CLH_1)} {_ToStHouse(CLH_2)}";
				return st;
			
				string _ToStHouse(int hh){
					string st="";
					if( hh<0 )  st = "----";
					else{
						int h = hh%100;			
						st = ((h<9)? "r": "c") + ((h%9)+1).ToString() + ((hh<100)? "_p": "_x");
					}
					return st.PadRight(5);
				}
			}
		}


		public class UFireworkExo: UFirework{
			public int    typeExocet;

			public UFireworkExo( bool s, int rcStem, int no, int rc1, int rc2, bool Alignment=true ): base( s, rcStem, no, rc1, rc2 ){
				this.typeExocet = s? 1: 2;
				this.rc1=rc1; this.rc2=rc2;
			}

			public UFireworkExo( int rcStem, int no, int rc1, int rc2 ): base( true, rcStem, no, rc1, rc2 ){
				this.typeExocet = 3;
			}

			public override string ToString( ){
				string stTF() => sw? "T": "F";
				string st = $"UFirework Stem;{rcStem.ToRCString()} sw:{stTF()} typeExocet:{typeExocet}";
				st += $"  no:#{FreeB.BitToNum()+1}  (rc1,rc2){rc12B81.ToBitString81N()}";
				st += $" IsComplete:{IsComplete}";
				return st;
			}

			public string ToString_compact(){
				string stTF = sw? "T": "F";
				string st = $"FW sw:{stTF}  no:#{FreeB.BitToNum()+1}  FW-Link: {rc1.ToRCString()} - {rcStem.ToRCString()} - {rc2.ToRCString()}";
				return st;
			}

		}
	} 


}