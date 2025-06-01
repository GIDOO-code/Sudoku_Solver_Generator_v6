using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

using GIDOO_space;
using static GNPX_space.Firework_TechGen;
using System.Windows.Documents;
using static GNPX_space.Senior_Exocet_TechGen;
//using static GNPX_space.JExocet_TechGen;

namespace GNPX_space{
    public partial class JExocet_TechGen: AnalyzerBaseV2{

		//static private int	dir_rc_toHouse( int dir, int rc ) => (dir==0)? rc/9: rc%9+9;
		static private int	dirB81ToHouse_Func( int dir, UInt128 Obj ) => (dir,Obj.Get_rcFirst()).DirRCtoHouse();
		static private string rcSt( int rc ) => (rc>=0)? rc.ToRCString_NPM(): " n/a";


	// ===================================================
	// For future expansion, this is over-implemented.
	// ===================================================



	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
		public class UExocet{
			//public List<UCell>  qBOARD;
			static public List<UCell>  qBOARD;

			public string		ExocetName;
			public int			dir;			// 0:row, 1:column
			public int			rcStem;

			
			public UInt128		Base81;
			public UInt128		BaseConnectedAnd81;

			public int			FreeB_BOARD;	// InitialState

			public int			FreeB => Base81.IEGet_UCell(qBOARD).Aggregate(0, (a,uc) => a| uc.FreeB );

			public int			FreeB0 => Base81.IEGet_UCell(qBOARD).Aggregate(0, (a,uc) => a| uc.FreeB_Updated() );

			public string		stBase  => $"{Base81.ToRCStringComp()}";
			public string		stBaseFreeB0 => $"#{FreeB0.ToBitStringN(9)}";

			public int			Block1;
			public int			Block2;


			public UInt128		Band81 => HouseCells81[Block1+18] | HouseCells81[Block2+18];
			public UInt128		Band81andBaseBlock => Band81 | HouseCells81[rcStem.B()+18];

			private int			ToSymmetry(int B, int sz=3 ) => (B%sz)*sz + (B/sz);
			public UInt128		CrossBand81 => HouseCells81[ToSymmetry(Block1)+18] | HouseCells81[ToSymmetry(Block2)+18];

			public UInt128		EscapeCells81{ get => Get_EscapeCells81(); }		

			public UExocet_elem ExG0;	// ... Base
			public UExocet_elem ExG1;	// ... Target1
			public UExocet_elem ExG2;	// ... Target2

			
			public string		stCRL0 => ExG0.CrossLine_Sx.ToRCStringComp().Replace("r123456789","").Replace("c123456789","");
			public string		stCRL1 => ExG1.CrossLine_Sx.ToRCStringComp().Replace("r123456789","").Replace("c123456789","");
			public string		stCRL2 => ExG2.CrossLine_Sx.ToRCStringComp().Replace("r123456789","").Replace("c123456789","");

			public bool			diagonalB;

			public List<UFireworkExo> fwList;

			public UInt128		CrossLine81_012 => ExG0.CrossLine_Sx | ExG1.CrossLine_Sx | ExG2.CrossLine_Sx;

			public UInt128		SLine_012 => ExG0.SLine_Sx | ExG1.SLine_Sx | ExG2.SLine_Sx;

			public UCoverStatus[] CoverStatusList;
			public int[]		CoverLine_by_Size;	
			public int			noCovered=0;	// Usage is undecided. #################
			public string       stCoverLines => CoverStatusList.Where(p=>p!=null).Aggregate(" ",(a,ch)=> a+$"\n {ch.ToString()}");
			public int			WildCardB => CoverLine_by_Size[3] ;






			public UExocet( int dir, int rcStem, UInt128 Base81, int freeB0 ){

				this.dir			= dir;
				this.rcStem			= rcStem;
				this.Base81			= Base81;
				this.BaseConnectedAnd81 = Base81.Aggregate_ConnectedAnd();
				this.FreeB_BOARD    = Base81.IEGet_UCell(qBOARD).Aggregate(0, (a,uc) => a| uc.FreeB );	// InitialState

				int block0          = rcStem.ToBlock(), shiftV=block0/3*3;
			  	
				int  _house_Sx       = (1-dir,rcStem).DirRCtoHouse();	// Cross House => (1-dir)
				UInt128 _CrossLineBas81 = HouseCells81[_house_Sx];
				UInt128 _SLine_S0       = _CrossLineBas81. DifSet( HouseCells81[block0+18] );

				// Target Blocks				
				if( dir==0 ){ this.Block1=(block0+1)%3+shiftV; this.Block2=(block0+2)%3+shiftV; }
				else{		  this.Block1=(block0+3)%9;        this.Block2 = (block0+6)%9; }

				this.ExG0 = new( this, sq:0, int.MaxValue, CrossLine:_CrossLineBas81, Sline:_SLine_S0, Companion:qMaxB81 );

			}

/*
			public void Set_UExocet( UExocet Exo, USeniorExocet SExo, USE_ctr SEBase, USE_ctr SE1, USE_ctr SE2 ){
				this.SExo    = SExo;

				this.dir	 = Exo.dir;
				this.rcStem	 = Exo.rcStem;
				this.Base81	 = Exo.Base81;

				SExo.Set_UExocet_elemParameter( Exo, SEBase, SE1, SE2 );
			}
*/


			public void Initialize(){
				this.CoverStatusList = new UCoverStatus[9];
			}

			public (UCell,UCell)  Get_Base_UCells(){
				var (rcA,rcB) = Base81.Get_rc1_rc2(sz:81);
				return (qBOARD[rcA],qBOARD[rcB]);
			}


			private UInt128 Get_EscapeCells81( ){
				if( ExG1==null || ExG2==null )  return qZero;
				UInt128 escp0 = HouseCells81[ (1-dir,rcStem).DirRCtoHouse() ] & HouseCells81[rcStem.B()+18];
				UInt128 escp1 = this.ExG1.Escape81 | this.ExG2.Escape81;
				UInt128 EscapeCells81 =  escp0 | escp1;
					// WriteLine( $"      escp0 : {escp0.ToBitString81N()}\n      escp1 : {escp1.ToBitString81N()}" );
					// WriteLine( $"EscapeCells : {EscapeCells81.ToBitString81N()}" );
				return EscapeCells81;
			}


			public override string ToString( ){
				string st = $"\n dir:{dir}  <<rcStem:{rcStem}>>       block0:{rcStem.ToBlock()}\n  baseCells:{Base81.TBS()}";
			  	st += $"\n  SLine0:{ExG0.SLine_Sx.TBS()}";
				if( ExG1 != null )  st += $"\n  ExG1:{ExG1}";
				if( ExG2 != null )  st += $"\n  ExG2:{ExG2}";
				if( EscapeCells81!=qZero ) st += $"\n  EscapeCells : {EscapeCells81.TBS()}";

			//	string stH = "";
			//	foreach( var CH in CoverStatusList.Where(p=>p!=null) )  stH += $"\n{CH}";
			//	st += stH;
				return st;
			}

			public string  Get_FireworkList(){
				if( fwList == null )  return "";
				string st = string.Join( "\n", fwList.Select(fw=> "  "+fw.ToString_compact() ) );
				return st;
			}
		} 


	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
		public class UExocet_elem{
		//	static int	dir_rc_toHouse(int dir, int rc) => (dir==0)? rc/9: rc%9+9;
		  //private List<UCell> qBOARD;
			static public List<UCell>  qBOARD;

			public UExocet  Exo;
			public int		sq;

			public bool		PhantomObjectB = false;
			public UInt128  Object81;
			public int	    rcTarget = int.MinValue;

			private string  stPhB => PhantomObjectB? "-": ""; 
			public string	stObject81  => stPhB + Object81.ToRCStringComp();

			public UInt128	Companion81;
			public string   stCompanion => Companion81.ToRCStringComp();

			public UInt128  Mirror81F;						// given, solved and candidate
			public UInt128  Mirror81;						// candidate only
			public string	stMirror81  => (Mirror81==qZero)? "-": Mirror81.ToRCStringComp();
			public string	stMirror81F => (Mirror81F==qZero)? "-": Mirror81F.ToRCStringComp();

			public int		house_Sx;
			public UInt128  CrossLine_Sx;
			public UInt128	SLine_Sx;
			public UInt128  Escape81  => ConnectedCells81[Exo.rcStem] & HouseCells81[ (1-Exo.dir,Object81.FindFirst_rc()).DirRCtoHouse() ];

			public int      FreeB_Object81 => Object81.IEGet_UCell(qBOARD) .Aggregate( 0, (a,uc)=> a| uc.FreeB_Updated() ); 
			public int      FreeB_Object81XOR => Object81.IEGet_UCell(qBOARD) .Aggregate( 0, (a,uc)=> a^ uc.FreeB_Updated() ); 
		
			public int      FreeB_Mirror81 => Mirror81.IEGet_UCell(qBOARD) .Aggregate( 0, (a,uc)=> a| uc.FreeB ); 
			public int      FreeB_Mirror81withFixed => Mirror81F.IEGet_UCell(qBOARD) .Aggregate( 0, (a,uc)=> a| uc.FreeB_Updated_withFixed() ); 

			public UInt128  Connected_rc( int rc) => (rc<100)? ConnectedCells81[rc]: qMaxB81;

			// <<< JE2D, JE2A / JE2+, JE2++ >>>
			public UExocet_elem( UExocet Exo, int sq, UInt128 Object81, UInt128 Comp81, int house_Sx, UInt128 CrossLine_Sx, UInt128 SLine_Sx, bool PhantomObjectB=false ){
				this.Exo		 = Exo;
				this.sq 		 = sq;
	
				this.Object81    = Object81;
				this.Companion81 = Comp81;

				this.house_Sx	  = house_Sx;
				this.CrossLine_Sx = CrossLine_Sx;
				this.SLine_Sx     = SLine_Sx;

				this.PhantomObjectB = PhantomObjectB;
			}

			public UExocet_elem( UExocet Exo, int sq, int rcTag, UInt128 CrossLine, UInt128 Sline, UInt128 Companion ){//, UInt128 Mirror81F, UInt128 Mirror81 ){
				this.Exo	  = Exo;
				this.sq 	  = sq;
				this.rcTarget = rcTag;
				this.Object81 = qOne<<rcTag;
				this.CrossLine_Sx = CrossLine;
				this.SLine_Sx     = Sline;
				this.Companion81  = Companion;
			//	this.Mirror81F    = Mirror81F;
			//	this.Mirror81     = Mirror81;
			}

			public void  Set_Mirror( UInt128 Mirror81F,  UInt128 Mirror81 ){
				this.Mirror81F   = Mirror81F;
				this.Mirror81    = Mirror81;
			}

			public int Get_Target_FreeB_Updated_And() => Object81.IEGet_rc().Aggregate( 0x1FF, (a,rc) => a & qBOARD[rc].FreeB_Updated() );
			public int Get_Target_FreeB_Updated_Or() => Object81.IEGet_rc().Aggregate( 0x1FF, (a,rc) => a | qBOARD[rc].FreeB_Updated() );
			public UInt128 Get_FullSight( bool withExclusion ){
				if( withExclusion ){ return Object81.IEGet_rc().Aggregate( qMaxB81, (a,rc) => a & ConnectedCells81[rc].Reset(rc) ); }
				else{				 return Object81.IEGet_rc().Aggregate( qMaxB81, (a,rc) => a & ConnectedCells81[rc] ); }
			}

			public override string ToString( ){
				string st = $"dir:{Exo.dir} rcStem:{Exo.rcStem.ToRCString()}";
				st += $" (B1,B2):{Exo.Base81.ToRCStringComp()} >>";
				st += $"T:{stPhB}{Object81.ToRCStringComp()} >>  C:{stCompanion}";
				st += $" >> M({stMirror81})";
				return st;
			}

			public string ToString( int n ){
				string st = $"dir:{Exo.dir} rcStem:{Exo.rcStem.ToRCString()}";
				st += $" (B1,B2):{Exo.Base81.ToRCStringComp()} >> T{n}:{stPhB}{Object81.ToRCStringComp()} >> C{n}:{stCompanion}";
				st += $" >> M{n}({stMirror81})";
				st += $" >> Sline{n} {SLine_Sx.ToBitString81N()}";
				return st;
			}
		}

	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
		public class UCoverStatus{
			static public int[] CoverLineH => JExocet_TechGen.CoverLineH ;
			public int	 no;
			private UExocet Exo;
			private int dir =>Exo.dir;
			public int   size;
			public int   S012V;
			public int	 CL0idx = -1;
			public int	 CL1idx = -1;
			public int	 CL2idx = -1;

			public int	 CL0house = -1;
			public int	 CL1house = -1;
			public int	 CL2house = -1;
			public int[] SL_Cnt;
			
			public UCoverStatus( UExocet Exo, int no ){
				this.Exo = Exo;
				this.no = no;
				this.size = 0;
				this.SL_Cnt = new int[4];
			}
			public UCoverStatus( UExocet Exo, int no, int S012V, int CL0idx, int cL0house ){
				this.Exo = Exo;
				this.no = no;
				this.size = 1;
				this.S012V  = S012V;
				this.CL0idx = CL0idx;
				this.CL0house = cL0house;
				this.SL_Cnt = Set_SL_Cnt( S012V );
			}

			public UCoverStatus( UExocet Exo, int no, int S012V, int CL0idx, int CL1idx, int cL0house, int CL1house ){
				this.Exo = Exo;
				this.no = no;
				this.size = 2;
				this.S012V  = S012V;
				this.CL0idx = CL0idx;
				this.CL1idx = CL1idx;
				this.CL0house = cL0house;
				this.CL1house = CL1house;
				this.SL_Cnt = Set_SL_Cnt( S012V );
			}

			public void Set_wildcard_para( int no, int CL2idx, int CL2house ){
				this.no = no;
				this.size = 3;
				this.CL2idx = CL2idx;
				this.CL2house =	CL2house;
				this.SL_Cnt = Set_SL_Cnt( S012V );
			}

			private int[]  Set_SL_Cnt( int S012V ){
				int[] _SL_Cnt = new int[4];
				_SL_Cnt[0] = (S012V & CoverLineH[11]) .BitCount();
				_SL_Cnt[1] = (S012V & CoverLineH[09]) .BitCount();
				_SL_Cnt[2] = (S012V & CoverLineH[10]) .BitCount();
				return _SL_Cnt;
			}

			private string ToStrc(int _dir) => (_dir==0)? "r": "c"; 
			private string ToStgRC( int h ) => (h<0)? "--": (h<9)? $"{ToStrc(dir)}{h+1}":  (h==9)? Exo.stCRL1: Exo.stCRL2; 

			private string ToBitString(int A){
				if( A <= 0 )  return "n/a";
				string st = "";
				for( int k=0; k<27; k++){ 
					st += ((A>>k)&0b1)==0? ".": $"{(k%9)+1}"; 
					if( (k%9)==8 ) st += " ";
				}
				return st.Trim();
			}

			public override string ToString( ){
				string st = $"  no:#{no+1} size:{size}";
				       st += $"  CoverLine:{ToStgRC(CL0idx)},{ToStgRC(CL1idx)},{ToStgRC(CL2idx)} (CLXidx:{CL0idx,2},{CL1idx:00}";
					   st += ((CL2idx>=0)? $",{CL2idx,2}": ",--") + ")";
					   st += $"  S012V:{S012V.ToBitStringMod9(27)}";
					   st += $"  CLxhouse 0:{ToBitString(CL0house)}  1:{ToBitString(CL1house)}  2:{ToBitString(CL2house)}";
				return st;
			}
		}

	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*


		public class UFireworkExo: UFirework{
			public int    typeExocet;

			public UFireworkExo( bool s, int rcStem, int no, int rc1, int rc2, bool Alignment=true ): base( s, rcStem, no, rc1, rc2 ){
				this.typeExocet = s? 1: 2;
				this.rc1 = rc1; this.rc2 = rc2;
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