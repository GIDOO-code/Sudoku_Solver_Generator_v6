using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;
using System.Xml.Serialization;

using GIDOO_space;
using GNPX_space.Properties;

namespace GNPX_space {

	// *==*==*==*==*==*==*==*==*==*
	//		version 6 
	// *==*==*==*==*==*==*==*==*==*

    public class USubset: IComparable{
        static private UInt128 qZero  = UInt128.Zero;
        static private UInt128 qOne   = UInt128.One; 

		static public  AnalyzerBaseV2	qAnalyzer;
        static public  UInt128[]		pHouseCells81 => AnalyzerBaseV2.HouseCells81;
        static public  UInt128[]		pConnectedCells81 => AnalyzerBaseV2.ConnectedCells81;

        static private protected bool	_withID_=false;
        static public void Set_withID_( bool withID ) => _withID_ = withID;  // for DEBUG 



        public int               ID;
        public readonly int      Size;          //CellsSize 
        public readonly int      FreeB;         //Subset element digits

        public readonly int      Level;         //FreeDigits-CellsSize
        public readonly List<UCell> UCellLst = new List<UCell>();    //Subset Cells
		public readonly long     hashValue;
        protected readonly int   sortkey;       
     
		public int      FreeBC => FreeB.BitCount();
		private UInt128[]		_connectedCellsB81_9;
		public UInt128[]		connected81_9 => _connectedCellsB81_9?? (_connectedCellsB81_9 = Create_connectedCellsB81_9() );
        public UInt128          connectedB81;   


        public UInt128          bitExp;
        private UInt128[]       _BitExp9;           // <- B981in
        public UInt128[]        BitExp9 => _BitExp9?? (_BitExp9=UCellLst.Create_bitExp81_9() );

        public int              rcbFrame = 0x7FFFFFF;    //bit expression of Subset
      
        public int              rcbRow{ get=> rcbFrame&0x1FF; }      //row expression
        public int              rcbCol{ get=> (rcbFrame>>9)&0x1FF; } //column expression
        public int              rcbBlk{ get=> (rcbFrame>>18)&0x1FF; }//block expression

		public bool				IsInsideBlock => rcbBlk.BitCount()==1;

        public int              houseNo_Row{ get => (rcbRow.BitCount()==1)? rcbRow.BitToNum(): -1; }
        public int              houseNo_Col{ get => (rcbCol.BitCount()==1)? rcbCol.BitToNum()+9: -1; }
        public int              houseNo_Blk{ get => (rcbBlk.BitCount()==1)? rcbBlk.BitToNum()+18: -1; }

		// ------------------------------------------------
		private int[]		    _rcbAnd9;
		public int[]		    rcbAnd9 => _rcbAnd9?? (_rcbAnd9=Create_rcbAnd9());
		public int[]  Create_rcbAnd9(){ 
			int[] _rcbAnd9Tmp = new int[9]; 
			for( int no=0; no<9; no++ )  _rcbAnd9Tmp[no] = Create_rcbAnd9(no);
			return _rcbAnd9Tmp;
		}
		public int  Create_rcbAnd9( int no ){
			int noB=1<<no;
			int rcbAnd = UCellLst.Where(p=>(p.FreeB&noB)>0).						// Select cells with no as an element.
						 Aggregate(0x7FFFFFF, (a,b) => a & b.rc.rcbFrame_Value() );	// house(rcb) common to all elements.
			if( rcbAnd==0x7FFFFFF ) return 0;
			return rcbAnd;
		}
		// ------------------------------------------------

        public int              FreeBwk;         // changed during analysis
        public int[]            rcbBitExp9And; 
        public int[]            rcbBitExp9Or; 

//        public bool             SubsetEnabled=true;
//        public USubset          preSubset = null;   // Use when creating a chain network.
        public int              RCC=0;

        public List<(USubset,int)> ConnectedSubset;	//[ATT] ... The second term is a bit representation.  //@@@
        public (USubset,int)       preSubset_no = (null,-9);											  //@@@




        public USubset(){ }

		public long		Get_HashValue() => UCellLst.Aggregate( (long)0, (a,b) => a^b.Get_hashValue() );
	  //private int		Func_rcbFrame(UCell P)  => 1<<(P.b+18) | 1<<(P.c+9) | (1<<P.r);
        public int CompareTo( object obj ){  // for IComparable. ... don't delete
            USubset UB = obj as USubset;
            if( this.Level!=UB.Level ) return (this.Level-UB.Level);
            if( this.Size!=UB.Size )   return (this.Size-UB.Size);

            if( this.FreeB!=UB.FreeB ) return ( this.sortkey-UB.sortkey );

            int bitW = this.bitExp.CompareTo(UB.bitExp);
            if( bitW != 0 ) return bitW;

            return  0;
        }
/*
		public int CompareTo( object obj ){
            UAnLS Q = obj as UAnLS;
			if( Q==null ) return 0;
			if( this.Level != Q.Level ) return (this.Level-Q.Level);
			if( this.Size  != Q.Size )  return (this.Size-Q.Size);
            if( this.FreeB !=Q.FreeB )  return (this.FreeB-Q.FreeB);
            return (this.ID-Q.ID);
        }
*/

        public USubset( int ID, int FreeB, List<UCell> UCellLst ){
            this.ID        = ID;
            this.Size      = UCellLst.Count;

			if( FreeB<=0 ){ FreeB = UCellLst.Aggregate(0, (p,q) => p|q.FreeB ); }
            this.FreeB     = FreeB;
            this.UCellLst  = UCellLst;
            this.Level     = FreeB.BitCount()-Size;
            this.sortkey   = this.FreeB._SortKey();
			this.hashValue = Get_HashValue();
			

            // --- initialSettings ---
            this.bitExp    = this.UCellLst.Aggregate( UInt128.Zero, (p,q) => p | qOne<<q.rc );
			this.connectedB81 = UCellLst.Aggregate( UInt128.Zero, (p,q)=> p| pConnectedCells81[q.rc] );
			
            _Set_BitExpression( );

			// --------------------------------------
			void _Set_BitExpression( ){
				FreeBwk = 0;
				rcbFrame = 0;
					//if( UCellLst.Count>2 ) WriteLine("");
				UCellLst.ForEach( P =>{
					FreeBwk  |= P.FreeB;
					rcbFrame |=	P.rcbFrame;		//Func_rcbFrame(P);
					foreach( int no in P.FreeB.IEGet_BtoNo() )  BitExp9[no] |= qOne<<P.rc;
						//if( UCellLst.Count>2 ) WriteLine( $"P{P}  rcbFrame:{rcbFrame.ToBitString27rcb()}" );
				} );

				rcbBitExp9And = new int[9];
				rcbBitExp9Or = new int[9];
				foreach( int no in FreeB.IEGet_BtoNo() ){
					rcbBitExp9And[no] = BitExp9[no].Ceate_rcbFrameAnd( );
					rcbBitExp9Or[no]  = BitExp9[no].Ceate_rcbFrameOr( );
				}
			}
		}
        public UInt128 ToBitExpression128( int filter_FreeB ){
            UInt128 BP128 = UInt128.Zero;
            foreach( var P in  UCellLst.Where( p=>(p.FreeB&filter_FreeB)>0 ) ) BP128 |= UInt128.One<<P.rc;
            return BP128;
        }
/*
        public USubset( int ID, int FreeB, List<UCell> UCellLst ): this(ID, FreeB, UCellLst ){
          //this.Level     = 1;
			this.LockedNoDir = null;
		   
			this.bitExp    = UCellLst.Aggregate( UInt128.Zero, (p,q)=> p| UInt128.One<<q.rc );
			this.connectedB81 = UCellLst.Aggregate( UInt128.Zero, (p,q)=> p| pConnectedCells81[q.rc] );
        }
*/

		public void RestoreFreeBwk( ) => UCellLst.ForEach( P => P.FreeBwk=P.FreeB ); 
		private UInt128[] Create_connectedCellsB81_9( ){			//..... FreeBwk
            UInt128[] B128 = new UInt128[9];
			foreach( int no in FreeB.IEGet_BtoNo() ){
				int noB = 1<<no;
				UInt128 Utmp = UInt128.MaxValue;
				foreach( var U in UCellLst ){
					if( (U.FreeB&noB) != 0 )  Utmp &= pConnectedCells81[U.rc];
				}
				B128[no] = Utmp;
			}
            return B128;
        }

		public int  rcbAnd9_wk( int no ){
			int noB=1<<no;
			int rcbAnd = UCellLst.Where(p=>(p.FreeBwk&noB)>0).						// Select cells with no as an element.
						 Aggregate(0x7FFFFFF, (a,b) => a & b.rc.rcbFrame_Value() );		// house(rcb) common to all elements.
			if( rcbAnd==0x7FFFFFF ) return 0;
				//UCellLst.ForEach( p=> WriteLine( $"b.rc:{p.rc.rcbFrame_Value().rcbToBitString27()}" ) );
				//WriteLine( $"rcbAnd:{rcbAnd.rcbToBitString27()}" );
			return rcbAnd;
		}

		public UInt128[] Get_ConnectedCellsB9( int noB9=0x1FF ){
			UInt128[] connectedCellsB9= new UInt128[9];
			foreach( int no in noB9.IEGet_BtoNo() ){
				connectedCellsB9[no] = BitExp9[no].IEGet_rc().Aggregate(UInt128.Zero, (p,q)=> p| pConnectedCells81[q] );
			}
			return connectedCellsB9;
		}
        
        public int Get_RCC_AnLS_AnLS( UAnLS UA ){
            int FreeB_AB = this.FreeB & UA.FreeB;
            if( FreeB_AB==0 )  return 0;  // no common digit

            int RCC_BitExp = 0;
            foreach( var no in FreeB_AB.IEGet_BtoNo() ){
				if( (this.rcbAnd9[no] & UA.rcbAnd9[no]) == 0 )  continue;
                RCC_BitExp |= (1<<no); 
			}
				//WriteLine( $"RCC_BitExp:{RCC_BitExp.ToBitString(9)}");
			return RCC_BitExp;
        }

		public void Copy_FreeBwk() => UCellLst.ForEach( p=> p.FreeBwk=p.FreeB );

#region ToStrng
		public override string ToString(){
            string st = $"\nUSubSet << ID:{ID} >>  [ Size:{Size} Level:{Level} ] ";
            st += $"  :{FreeB.ToBitString(9)}  FreeBwk:{FreeBwk.ToBitString(9)}\n";
            st += $" rcbFrame:{(rcbFrame).ToBitString27rcb()}";
			st += $" IsInsideBlock:{(IsInsideBlock? "@":"-")}";
            st += $"         bitExp:{bitExp.ToBitString81()}\n";
			st += $"connectedB81:{connectedB81.ToBitString81()}";

					//int rcbFX = 0;
            for(int k=0; k<UCellLst.Count; k++){
                st += "\n------";
				UCell P = UCellLst[k];
                st += $" rc:{P.rc.ToRCString()}";
                st += $" FreeB:{P.FreeB.ToBitString(9)}";
				st += $" FreeBwk:{P.FreeBwk.ToBitString(9)}";			               
				st += $" // rcbFrame: {P.rcbFrame.ToBitString27rcb(digitB:false)}";
            }
					//string stX1 = $"rcbFrame:{(rcbFrame).ToBitString27rcb()}";
					//string stX2 = $"rcbFrame:{(rcbFX).ToBitString27rcb()}";
					//WriteLine( $" stX1:{stX1}\n stX2:{stX2}" );

            return st;
        }

        public string ToStringA(){
            string st = $"<> USubset ID:{ID} Size:{Size} Level:{Level}  FreeB:{FreeB.ToBitString(9)} rc:{ToStringRCN()}";
            return  st;
        }

        public string ToStringRCN( ){
            string st="";
            UCellLst.ForEach( p =>{  st += $" {p.rc.ToRCString()}"; } );
            st = st.ToString_SameHouseComp()+" #"+FreeB.ToBitStringN(9);
            return st;
        }
        public string ToStringRC( ){
            string st="";
            UCellLst.ForEach( p =>{  st += $" {p.rc.ToRCString()}"; } );
            st = st.ToString_SameHouseComp();
            return st;
        }
		public  string ToString_FreeBwk(){
			string st = $"USubset << ID:{ID} >>  [ Size:{Size} Level:{Level} ]";
			st += $"  FreeB:{FreeB.ToBitString(9)}  FreeBwk:{FreeBwk.ToBitString(9)}\n";
			st += $"         bitExp {bitExp.ToBitString81N()}\r";
			for(int k=0; k<UCellLst.Count; k++){
				st += "------";
				int rcW = UCellLst[k].rc;
				st += $" rc:{rcW.ToRCString()}";
				st += $" FreeBwk:{UCellLst[k].FreeBwk.ToBitString(9)}";
				st += $" rcb:b{(rcbBlk).ToBitString(9)}";
				st += $" c{rcbCol.ToBitString(9)}";
				st += $" r{rcbRow.ToBitString(9)}";
				//st += $" connectedB81:{connectedB81.ToBitString81N()}";
			  	st += "\n";
			}
            return st;
        }
#endregion ToString

	}

}
