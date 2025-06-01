using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.IO;
using System.Text;
using System.Security.Policy;

namespace GNPX_space{

    public partial class Firework_TechGen: AnalyzerBaseV2{
        static private   UInt128 qZero  = UInt128.Zero;
        static private   UInt128 qOne   = UInt128.One; 
		private int		 stageNoPMemo = -9;

		public UInt128   BOARD_FreeCell81;

		public List<UFirework> Firework_List;	
		public List<UFirework> FireworkAgg_List;

	#region 4Debug
		internal bool		debugPrint = false;
		internal string     debug_Result = "";
		internal string		fName_debug = "Firework_Triple.txt";

		
		internal void Debug_StreamWriter( string LRecord ){
#if DEBUG
			if(!debugPrint)  return;
            using( var fpW=new StreamWriter(fName_debug,true,Encoding.UTF8) ){
                fpW.WriteLine(LRecord);
            }
#endif
	
		}	
	#endregion 4Debug


		public Firework_TechGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }


		public void  Prepare_Firework_TechGen(){
		
			Firework_List = IEGet_UFirework().ToList();						// Single Firework
			if( Firework_List==null || Firework_List.Count<=0 )  return ;
					//foreach( var (p,m) in Firework_List.WithIndex() ){ WriteLine( $"0-{m} {p}" ); };
					//WriteLine( "" );
			
			{// Aggregation of candidate digits <<< rcStem >>>
				FireworkAgg_List = new();
				var querySL = Firework_List.GroupBy(x => x.rcStem);
				foreach( var group in querySL.Where(p=>p.Count()>=2) ){
					var groupList = group.ToList();

					if( group.Count() >= 2 ){
						Combination cmb2 = new( groupList.Count, 2 );
						while( cmb2.Successor() ){
							UFirework UFW0 = groupList[ cmb2.Index[0] ];
							UFirework UFW1 = groupList[ cmb2.Index[1] ];

							UFirework UFWadd = new( UFW0, UFW1 );
							if( UFWadd.rcStem >= 0 )  FireworkAgg_List.Add(UFWadd);
						}
					}

					if( group.Count() >= 3 ){
						Combination cmb3 = new( groupList.Count, 3 );
						while( cmb3.Successor() ){
							UFirework UFW0 = groupList[ cmb3.Index[0] ];
							UFirework UFW1 = groupList[ cmb3.Index[1] ];
							UFirework UFW2 = groupList[ cmb3.Index[2] ];

							UFirework UFWadd2 = new( UFW0, UFW1 );
							if( UFWadd2.rcStem <= 0 ) continue;

							UFirework UFWadd3 = new( UFWadd2, UFW2 );
							if( UFWadd3.rcStem >= 0 )  FireworkAgg_List.Add(UFWadd3);
						}
					}
				}
					//foreach( var (p,m) in FireworkAgg_List.WithIndex() ){ WriteLine( $"  2-{m} {p}  FreeBC:{p.FreeBC}" ); };
					//WriteLine( "" );
			}	
		}

		private void __debug_sol_check( ) {
			foreach( var P in pBOARD.Where( p=> (p.FixedNo>0 && p.FixedNo!=sol_int81[p.rc]) ) ){
				WriteLine( $"\n   <<< error >>> :  P.FixedNo:{P.FixedNo}  sol:{sol_int81[P.rc]}\n         {P}" );
			}
			foreach( var P in pBOARD.Where( p=> (p.CancelB>0 && (p.CancelB & (1<<(sol_int81[p.rc]-1)))>0)) ){
				WriteLine( $"\n   <<< error >>> : P.FixedNo:{P.FixedNo}  sol:{sol_int81[P.rc]}\n         {P}" );
			}
		}


		public class UFirework{
			public bool sw;		// true : strong type (connected by strong link).
			public int  rcStem;
			public int  FreeB;
			public int  FreeBC;
			public int  rc1;
			public int  rc2;
			public UInt128 rc12B81;
			public UInt128 rcStem_rc12B81;
			public bool IsComplete => (rc1>=0 && rc2>=0);

			public UFirework( ){
				this.rcStem = -1;
			}
			public UFirework( bool s, int rcStem, int no, int rc1, int rc2 ){
				this.sw = s;
				this.rcStem = rcStem;

				this.rc1 = Min(rc1,rc2); this.rc2 = Max(rc1,rc2);

				this.FreeB = 1<<no;
				this.FreeBC = 1;
				this.rc12B81 = qOne<<rc1 | qOne<<rc2; 
				UInt128 rc12B81 = 0;
				if( rc1 >= 0 )  rc12B81 |= (qOne<<rc1);
				if( rc2 >= 0 )  rc12B81 |= (qOne<<rc2);
				this.rcStem_rc12B81 = (UInt128.One<<rcStem) | rc12B81;
			}

			public UFirework Copy( bool sw, int FreeB=-999 ){
				UFirework UFW = (UFirework)this.MemberwiseClone();
				UFW.sw = sw;
				if( FreeB != -999 ){ UFW.FreeB=FreeB; UFW.FreeBC=FreeB.BitCount(); }
				return UFW;
			}

			public UFirework ( UFirework UF1, UFirework UF2 ): this(){
				UInt128 rc12B81 = UF1.rc12B81 | UF2.rc12B81;
				if( UF1.rcStem==UF2.rcStem && (UF1.FreeB&UF2.FreeB)==0 && rc12B81.BitCount()<=2 ){
					this.sw		= UF1.sw & UF2.sw;
					this.FreeB  = UF1.FreeB | UF2.FreeB; 
					this.FreeBC = UF1.FreeBC + UF2.FreeBC;
					this.rc12B81 = rc12B81;
					this.rcStem_rc12B81 = UF1.rcStem_rc12B81 | UF2.rcStem_rc12B81;

					this.rcStem = UF1.rcStem;
					List<int> rcList = rc12B81.IEGet_rc(81).ToList();
					this.rc1 = rcList[0];
					this.rc2 = (rcList.Count>=2)? rcList[1]: -1;
				}
			}





			public override string ToString( ){
				string stTF() => sw? "T": "F";
				string st = $"UFirework Stem;{rcStem.ToRCString()} sw:{stTF()}";
				st += $"  FreeB:{FreeB.ToBitString(9)}  (rc1,rc2){rc12B81.ToBitString81N()}";
				st += $" IsComplete:{IsComplete}";
				return st;
			}

			public string ToStringResult( ){
				string st = $"{rcStem.ToRCString()} <#{FreeB.ToBitStringN(9)}> / {rc12B81.ToRCStringComp()}";
				return st;
			}
			public string ToStringResultLong( ){
				string st = $"    Stem;{rcStem.ToRCString()} sw:{sw}";
				st += $" FreeB:{FreeB.ToBitStringN(9)}\n    (rc1,rc2):{rc12B81.ToRCStringComp()}";
				return st;
			}
			public override bool Equals( object obj ){
				UFirework ufw = obj as UFirework;
				if( this.FreeB != ufw.FreeB ) return false;
				return (this.rcStem_rc12B81 == ufw.rcStem_rc12B81); 
			}

			public int CompareTo( object obj ){  // for IComparable. ... don't delete
				UFirework ufw = obj as UFirework;

				if( this.rcStem != ufw.rcStem ) return (this.rcStem-ufw.rcStem);
				if( this.FreeB  != ufw.FreeB )  return (this.FreeB-ufw.FreeB);

				if( this.rc1 != ufw.rc1 )       return ( this.rc1-ufw.rc1 );
				return  (this.rc2-ufw.rc2);
			}

			public int CompareToRC( object obj ){  // for IComparable. ... don't delete
				UFirework ufw = obj as UFirework;

				if( this.rc1 != ufw.rc1 )       return ( this.rc1-ufw.rc1 );
				if( this.rc2 != ufw.rc2 )       return ( this.rc2-ufw.rc2 );

				if( this.FreeB  != ufw.FreeB )  return (this.FreeB-ufw.FreeB);

				return (this.rcStem-ufw.rcStem);
			}
		}




		private IEnumerable<UFirework> IEGet_UFirework( bool debugPrint=false ){
			BOARD_FreeCell81 = pBOARD.Create_Free_BitExp128();

			foreach( UCell Stem in pBOARD.Where(p=> p.FreeBC>=2) ){
				if( Stem.FreeBC < 2 )  continue;
				int  rowH=Stem.r, colH=Stem.c+9, blkH=Stem.b+18;
				UInt128 row81 = HouseCells81[rowH];
				UInt128 col81 = HouseCells81[colH];
				UInt128 blk81 = HouseCells81[blkH];
				UInt128 fw_StemS = row81 | col81;
				UInt128 fw_Stem  = fw_StemS. DifSet(blk81);	// Target House : (Hrou|HColumn)-Block

					//WriteLine( $"Stem:{Stem}\n fw_Stem:{fw_Stem.ToBitString81N()}" );

				foreach( int no in Stem.FreeB.IEGet_BtoNo(9) ){
					UInt128 fw_Stem_no = FreeCell81b9[no] & fw_Stem;
					UInt128 fw_Stem_noN = BOARD_FreeCell81 & fw_Stem;

					UInt128 col81Sel = row81 & fw_Stem_no;
					int rc1 = col81Sel.Get_rc_ifUnique(81);						// Target row unique cell

					UInt128 row81Sel = col81 & fw_Stem_no;
					int rc2 = row81Sel.Get_rc_ifUnique(81);						// Target clumn unique cell

					// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
					if( rc1<0 && rc2<0 )  continue;								// * missing in both directions

					int swCount = (FreeCell81b9[no]&fw_StemS).BitCount();
					if( rc1>=0 && rc2>=0 ){										// * both row and column
						bool sw = (swCount==3);
						UFirework UFW = new( sw, Stem.rc, no, rc1, rc2 );
						yield return UFW;
					}

					else if( rc1<0 && rc2>=0 && col81Sel==0 ){					// * row direction is missing
						UInt128 row81SelN = row81 & fw_Stem_noN;
						bool sw = (swCount==3);
						foreach( var rc1N in row81SelN.IEGet_rc(81) ){
							UFirework UFW = new( sw, Stem.rc, no, rc1N, rc2 );
							yield return UFW;
						}
					}

					else if( rc1>=0 && rc2<0 && row81Sel==0 ){					// * column direction is missing
						UInt128 col81SelN = col81 & fw_Stem_noN;
						bool sw = (swCount==3);
						foreach( var rc2N in col81SelN.IEGet_rc(81) ){
							UFirework UFW = new( sw, Stem.rc, no, rc1, rc2N );
							yield return UFW;
						}
					}
				}
			}
		}
	}

}