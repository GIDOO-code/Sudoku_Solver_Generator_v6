using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
using static System.Diagnostics.Debug;

using System.Security.Policy;
using System.Runtime.CompilerServices;
//using System.Drawing;
using GIDOO_space;
using System.Windows.Media;
using System.Windows.Annotations;

namespace GNPX_space{   
    using SWMColor = System.Windows.Media.Color;

    //========== Extended Function ==========
    static public class StaticSA{ 
		static char[] alpha = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
								'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 
								'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 
								'u', 'v', 'w', 'x', 'y', 'z'};
				

        static private UInt128[] pHouseCells81     => AnalyzerBaseV2.HouseCells81;
        static private UInt128[] pConnectedCells81 => AnalyzerBaseV2.ConnectedCells81;

        static private char[] _sep=new Char[]{ ' ', ',', '\t' };
        static private char[] _sepSharp=new Char[]{ '#' };
        static public bool rcbHitCheck( this int X, int Y ){
            if( X/9==Y/9 )  return true;    //row
            if( X%9==Y%9 )  return true;    //column
            if( (X/27*3+(X%9)/3) == (Y/27*3+(Y%9)/3) )  return true;  //block
            return false;
        }

		static public int dir_rc_toHouse( int dir, int rc ) => (dir==0)? rc/9: rc%9+9;
      #region ToString_SameHouseComp

/*
        static public void testc() {
            string[] test = {"r3 c5", "r4 r6", "c2 c4", "r1c1 r1c2", " r1c5 r2c5 ", "r1c5 r2c5 r9c9 r1c1 r1c2 r2c8",  "r1c1 r1c2 r1c5 r2c5", };
                
            foreach( var p in test ){
                WriteLine("");
                WriteLine( $"p:{p} ToString_SameHouseComp :{p.ToString_SameHouseComp()}" );
                WriteLine( $"p:{p} ToString_SameHouseComp2:{p.ToString_SameHouseCompNew()}" );
            }
        }
*/
        static public string ToString_SameHouseComp( this string stOrg, string insertString ){
            string st1 = stOrg.ToString_SameHouseComp();
            List<string> stLst = st1.Split(_sep,StringSplitOptions.RemoveEmptyEntries).ToList();
            string stInsert = insertString+" ";
            List<string> stLst2 = stLst.ConvertAll(p=> p+insertString);
            string st = string.Join( " ", stLst2 );

            if( insertString.Contains("#") )  st = st.ToString_SameHouseNoComp();
            return st.Trim();
        }

        static public string ToString_SameHouseComp( this string stOrg ){  // new version  
            List<string> stLst = stOrg.Split(_sep, StringSplitOptions.RemoveEmptyEntries).ToList();
            if( stLst.Count<=1 ) return stOrg.Trim();          

            var stLst2 = stLst.ConvertAll( P => new _C4Group(P, rcOpt:true) ) ;
            bool changed=false, changedT=false;
            if( stLst2.Count>=2 ){
                do{
                    changed=false;
                    var stGroup = stLst2.GroupBy( P => P.val1 );

                    foreach( var grp in stGroup ){ 
                        if( grp.Count() <= 1 )  continue;
                        string stW = grp.Aggregate( "", (a,b)=>a+b.val2 );
                        stLst2.RemoveAll( q=> q.val1==grp.Key );
                        stLst2.Add( new _C4Group(grp.Key, stW) );
                        changed = true;
                        changedT = true;
                    }
                }while(changed);
            }

            if( stLst2.Count>=2 ){
                do{
                    changed=false;
                    var stGroup = stLst2.GroupBy( P => P.val2 );

                    foreach( var grp in stGroup ){ 
                        if( grp.Count() <= 1 )  continue;
                        string stW = grp.Aggregate( "", (a,b)=>a+b.val1 );
                        stLst2.RemoveAll( q=> q.val2==grp.Key );
                        stLst2.Add( new _C4Group( stW, grp.Key) );
                        changed = true;
                        changedT = true;
                    }
                }while(changed);
            }

            string st = stOrg.Trim();
            if( changedT ){
                stLst = stLst2.ConvertAll( p => p.ToRCString() );
                stLst.Sort();
                st = string.Join( " ", stLst );
            }   

            if( st.Substring(0,2) == "rr" )  WriteLine( st );
            
                  //string st2 = stOrg.ToString_SameHouseComp_Old();
                  //if( st != st2 ) WriteLine( $"stOrg:{stOrg}\r   st:{st}\rstNew:{st2}" );
            return st;
        }

        static public string ToString_SameHouseComp_Old( this string stOrg ){  // new version        
          // Preparation
            string stOrg2 = stOrg.Replace("c","s");     // Adjusting the order of r and c
            List<string> stLst = stOrg2.Split(_sep, StringSplitOptions.RemoveEmptyEntries).ToList();
            if( stLst.Count<=1 ) return stOrg.Trim();          
            stLst.Sort();
            var URCBCell = stLst.ConvertAll( p=>new _ClassNSS(p.Replace("s","c")) );

          // Convert to aggregate representation
            string st;
            bool cmpF=true;
            do{
                cmpF=false;
                foreach( var P in URCBCell ){
                    List<_ClassNSS> Qlst=URCBCell.FindAll(Q=>(Q.stP1==P.stP1 && Q.stP2[0]==P.stP2[0]));
                    if(Qlst.Count>=2 ){
                        st=Qlst[0].stP2[0].ToString();
                        Qlst.ForEach(Q=>st+=Q.stP2.Substring(1,Q.stP2.Length-1));
                        _ClassNSS R=URCBCell.Find(Q=>(Q.stP1==P.stP1));
                        R.stP2 = st;
                        foreach(var U in Qlst.Skip(1)) URCBCell.Remove(U);
                        cmpF=true;
                        break;
                    }

                    Qlst=URCBCell.FindAll(Q=>(Q.stP2==P.stP2 && Q.stP1[0]==P.stP1[0]));
                    if(Qlst.Count>=2 ){
                        st=Qlst[0].stP1[0].ToString();
                        Qlst.ForEach(Q=>st+=Q.stP1.Substring(1,Q.stP1.Length-1));
                        _ClassNSS R=URCBCell.Find(Q=>(Q.stP2==P.stP2));
                        R.stP1 = st;
                        foreach(var U in Qlst.Skip(1)) URCBCell.Remove(U);
                        cmpF=true;
                        break;
                    }
                }

            }while(cmpF);
            st="";
            URCBCell.ForEach(P=> st+=(P.stP1+P.stP2.Trim()+" ") );
            st = st.TrimEnd();

            return st;
        }

        static public string ToString_SameHouseNoComp( this string stOrg ){  // new version        
            if( !stOrg.Contains("#") )    return stOrg;

            List<string> stLst = stOrg.Split(_sep, StringSplitOptions.RemoveEmptyEntries).ToList();
            if( stLst.Count<=1 ) return stOrg.Trim();          

            var stLst2 = stLst.ConvertAll( P => new _C4Group(P) ) ;
            bool changed=false, changedT=false;
            do{
                changed=false;
                var stGroup = stLst2.GroupBy( P => P.val1 );

                foreach( var grp in stGroup ){ 
                    if( grp.Count() <= 1 )  continue;
                    string stW = grp.Aggregate("",(a,b)=>a+b.val2 );
                    stLst2.RemoveAll( q=> q.val1==grp.Key );
                    stLst2.Add( new _C4Group(grp.Key, stW) );
                    changed = true;
                    changedT = true;
                }
            }while(changed);
            if( !changedT )  return stOrg;

            stLst = stLst2.ConvertAll( p => p.ToString() );
            stLst.Sort();
            string st = string.Join( " ", stLst );
            return st;
        }
        private class _C4Group{
            public string val1{ get; set; }
            public string val2{ get; set; }
                
            public _C4Group( string val1, string val2 ){ this.val1=val1; this.val2=val2; }

            public _C4Group( string Ast ){
                string[] A = Ast.Split('#'); 
                this.val1=A[0]; 
                this.val2=A[1];
            }
            public _C4Group( string Ast, bool rcOpt ){
                this.val1 = Ast.Substring(1,1);
                this.val2 = Ast.Substring(3,1);
            }
            public override string ToString( ){
                return $"{val1}#{val2}";
            }
            public string ToRCString( ){
                return $"r{val1}c{val2}";
            }
        }


        static public string ToRCString( this List<UCell> UCellLst ){
            string st="";
            UCellLst.ForEach( p =>{  st += $" {p.rc.ToRCString()}"; } );
            st = st.ToString_SameHouseComp();
            return st;
        }

        private class _ClassNSS{
            public int sz;
            public string stP1;
            public string stP2="  ";

            public _ClassNSS( int sz, string stP1, string stP2 ){
                this.sz=sz; this.stP1=stP1; this.stP2=stP2;
            }
            public _ClassNSS( string st ){
                try{
                    sz=1; 
                    if(st.Length>=2) stP1=st.Substring(0,2);
                    if(st.Length>=4) stP2=st.Substring(2,2);
                }
                catch(Exception ){ }
            }
        }
     




        static public int  sameHouseCheck( this int B, int C ){
            // 0:Completely Different
            // 1:Same Row　2:Same Column　4:Same Block --> Bit Representation
            int ret = 0;
            if(B/9==C/9) ret = 1;
            if(B%9==C%9) ret |= 2;
            if((B/27*3+(B%9)/3)==(C/27*3+(C%9)/3)) ret |= 4;
            return ret;
        }

      #endregion



        static public int _SortKey( this int num ){
            int numW=num, numR=0;
            for(int k=0; k<9; k++ ){
                if( (numW&1)>0 ){ numR=numR*10+(k+1); }
                numW>>=1;
            }
            return numR;
        }

        static public string ToBitString( this int num, int ncc=9, bool withDot=true )  => ToBitString( (uint)num, ncc, withDot:withDot );
		static public string ToBitStringMod( this int num, int ncc=9 )  => ToBitStringMod( (uint)num, ncc );
		static public string ToBitStringMod9( this int num, int ncc=9 )  => ToBitStringMod9( (uint)num, ncc );
       
		static public string TBSnD( this int num )  => num.ToBitString(9, withDot:false );			// 4 development
		
		static public string TBS( this int num )  => num.ToBitString(9);			// for development
		static public string TBScmp( this int num )  => num.ToBitStringN(9);		// for development
		static public string TBSM27( this int num )  => num.ToBitStringMod9(27);	// for development
        static public string rcbToBitString27( this int num )           => rcbToBitString27( (uint)num );
  //      static public string ToBitString27rcb( this int num, bool digitB=true ) => ToBitString27rcb( num, digitB:digitB );
  //      static public string ToBitString36rcbn( this long lnum, bool digitB=true ) => ToBitString36rcbn( lnum, digitB:digitB );
     
		static public string TBS( this int num, int ncc=9, string separator="" )   => ToBitStringN( (uint)num, ncc, separator );		// For debugging purposes only
		static public string ToBitStringN( this int num, int ncc=9, string separator="" )   => ToBitStringN( (uint)num, ncc, separator );
        static public string ToBitStringNor( this int num, int ncc ) => ToBitStringNor( (uint)num, ncc );
        static public string ToBitStringNZ( this int num, int ncc )  => ToBitStringNZ( (uint)num, ncc );
        static public string ToBitString( this long LNum, int n9=4 ) => ToBitString( (ulong)LNum, n9 );





        static public string ToBitString( this uint num, int ncc=9, bool withDot=true ){
            uint numW = num;
            string st="";
			string stDot = withDot? ".": "";
            for(int k=0; k<ncc; k++ ){
                st += ((numW&1)!=0)? (k+1).ToString(): stDot;
                numW >>= 1;
            }
            return st;
        }
        static public string ToBitStringMod( this uint num, int ncc=9 ){
            uint numW = num;
            string st="";
            for(int k=0; k<ncc; k++ ){
                st += ((numW&1)!=0)? ((k%10)+1).ToString(): ".";
                numW >>= 1;
            }
            return st;
        }
        static public string ToBitStringMod9( this uint num, int ncc=9 ){
            uint numW = num;
            string st="";
            for(int k=0; k<ncc; k++ ){
                st += ((numW&1)!=0)? ((k%9)+1).ToString(): ".";
				if( k%9 == 8 ) st += " ";
                numW >>= 1;
            }
            return st;
        }
		static public string ToBitStringAlpha( this uint num, int ncc=9 ){
            uint numW = num;
            string st="";
            for(int k=0; k<ncc; k++ ){
                st += ((numW&1)!=0)? alpha[k%36].ToString(): ".";
                numW >>= 1;
            }
            return st;
        }

        static public string ToBSt( this uint num ) => num.ToBitString(9);

        static public string rcbToBitString27( this uint num ){
            string st = (num&0x1FF).ToBitString(9)
                      + " "+((num>>9)&0x1FF).ToBitString(9)
                      + " "+((num>>18)&0x1FF).ToBitString(9) +" /";
            return st;
        }
        static public string ToBitString27rcb( this int num, bool digitB=true ){
            string st = "";
				if(digitB) st = $"r{(num&0x1FF).ToBitStringN(9)} c{((num>>9)&0x1FF).ToBitStringN(9)} b{(num>>18).ToBitStringN(9)}";
				else       st = $"r{(num&0x1FF).ToBitString(9)} c{((num>>9)&0x1FF).ToBitString(9)} b{(num>>18).ToBitString(9)}";
            return st;
        }
		static public string ToBitString36rcbn( this long Lnum, bool digitB=true ){
			string st = "";
				int noB9 = (int)(Lnum&0x1FF);
				if(digitB) st += $" n:#{noB9.ToBitStringN(9)}";
				else       st += $" n:#{noB9.ToBitString(9)}";
				st +=  $" {((int)((Lnum>>9)&0x7FFFFFF)).ToBitString27rcb(digitB)}";
			return st;
		}

        static public string ToBitStringN( this uint num, int ncc, string separator="" ){
            uint numW = num;
				List<int> Q=new();
				for(int k=0; k<ncc; k++ ){
					if((numW&1)!=0) Q.Add(k+1);
					numW >>= 1;
				}
				string st = "-";
				if(Q.Count>0)  st = string.Join(separator,Q);
            return st;
        }
/*
         static public string ToBitStringN( this uint num, int ncc ){
            uint numW = num;
			string Comma = withComma? ",": "";
				string st="";
				List<int> Q=new();
				for(int k=0; k<ncc; k++ ){
					if((numW&1)!=0) st += (k+1).ToString()+withComma;
					numW >>= 1;
				}
				if(st=="")  st = "-";
            return st;
        }
*/
        static public string ToBitStringNor( this uint num, int ncc ){
            uint numW = num;
				string st="";
				for(int k=0; k<ncc; k++ ){
					if((numW&1)!=0 ){
						if(st=="") st = (k+1).ToString();
						else st += " or "+(k+1).ToString();
					}
					numW >>= 1;
				}
				if(st=="")  st = "*";
            return st;
        }
        static public string ToBitStringNZ( this uint num, int ncc ){
            uint numW = num;
            string st="";
            for(int k=0; k<ncc; k++ ){
                if((numW&1)!=0) st += (k+1).ToString();
                numW >>= 1;
            }
            return st;
        } 
        static public string ToBitString( this ulong LNum, int n9=4 ){
            string st = "";
            ulong LNumW = LNum;
            for(int k=0; k<n9; k++ ){
                int LNumInt = (int)(LNumW&0x1FF);
                st += LNumInt.ToBitString(9)+" /";
                LNumW >>= 9;
            }
            return st;
        }




        static public string ToBitStringN( this long LNum, int n9=4 ){
            string st = "";
            long LNumW = LNum;
            for(int k=0; k<n9; k++ ){
                int LNumInt = (int)(LNumW&0x1FF);
                st += LNumInt.ToBitStringN(9)+" /";
                LNumW >>= 9;
            }
            return st;
        }
        static public string ToRCString( this int rc ){
			int rcAbs = Abs(rc); 
			string st = $"r{rcAbs/9+1}c{(rcAbs%9)+1}";
			if( rc<0 )  st = "-" + st;
            return st;
        }
        static public string ToRCString_NPM( this int rc ){
			int rcReal = rc%100; 
			string st = (rc>=100 )? "-": "";  // "-":Fixed-cell
            st += $"r{rcReal/9+1}c{(rcReal%9)+1}";
			if( rc<0 )  st = " n/a";
            return st;
        }

        static public string ToRCNCLString( this int rc ){
            return  rc.ToRCString().ToString_SameHouseComp();
        }



        static public string HouseToString( this int HH ){
            string st="";
            if((HH&0x1FF)>0) st += "r"+(HH&0x1FF).ToBitStringN(9)+" ";
            if(((HH>>=9)&0x1FF)>0) st += "c"+(HH&0x1FF).ToBitStringN(9)+" ";
            if(((HH>>=9)&0x1FF)>0) st += "b"+(HH&0x1FF).ToBitStringN(9)+" ";
            return st.Trim();
        }
        static public string hToString( this int h, string noSt=" " ){
            string st="";
            if(h<0)            st += "---";
            if(h>=0 && h<9)    st += "r"+(h+1)+noSt;
            if(h>=9 && h<18)   st += "c"+(h-9+1)+noSt;
            if(h>=18)          st += "b"+(h-18+1)+noSt;
            return st.Trim();
        }



        static public int ToRCB_BitRep( this int rc ){
            int r=rc/9, c=rc%9, b=r/3*3+c/3;
            int rcbBP = (1<<(b+18)) | (1<<(c+9)) | (1<<r);
            return rcbBP;
        }    
        

        static public string Row3Col3ToString( this int rcX3 ){
            string st="";
            for(int k=0; k<27; k++ ){
                if((rcX3&(1<<k))>0)  st += (k%3+1).ToString();
                else st+=".";
                if((k%9)==8)  st += "<>";
                else if((k%3)==2)  st += " ";
            }
            return st;
        }
        //========== bit expression→Number  ==========   
        static public int BitToNum( this int val, int sz=9 ){
			int mask = (sz==9)? 0x1FF: ((sz>0 && sz<31)? (1<<sz)-1: 0); 
			int valMask = val & mask;
            if( valMask.BitCount()!=1) return -1;
            for(int k=0; k<sz; k++ ){ if( valMask==(1<<k)) return k; }
            return -1;
        }

        static public int No_indexRefer( this int FreeB, int ix ){    
            int sz = FreeB.BitCount();
            if( sz<=0 ) return -1;
            int x=FreeB, kx=0;

            for( int n=0; n<sz; n++ ){ 
                if( (x&1)>0 ){ 
                    kx++;
                    if( kx==ix )  return n;
                }
                x >>= 1;
            }
            return -1;
        }
        static public List<int> BitToNumList( this int FreeB ){    
            int sz = FreeB.BitCount();
            List<int> Lst = new List<int>();
            int x=FreeB, n=0;
            while(x>0){
                n++;
                if( (x&1)>0 ) Lst.Add(n);
                x >>= 1;
            }
            return Lst;
        }

        //========== bit expression→2 Numbers ==========   
        
        static public (int,int) BitTo2Nums( this int noB ){
            int na=-1,nb=-1;
            if(noB.BitCount()!=2) return (-1,-1);
            for(int k=0; k<9; k++ ){
                if((noB&1)>0) nb=k;
                if(na<0) na=nb;
                noB >>= 1;
            }
            return (na,nb);
        }



        // for UInt128 version
        static public List<int> GetRowList( this UInt128 X81, int r ){
            List<int> RowLst = new List<int>();
            UInt128 Bit81_BP = X81;                     //.Get_BitExpression();
            int _BPX = (int)( Bit81_BP >> (r*9) );
            for(int c=0; c<9; c++ ){
                int p = (_BPX>>c)&1;
                RowLst.Add(p);
            }
            return RowLst;
        }


        static public int Get_tfx_rc( this int h, int nx ){
            int r=0, c=0;
            switch(h/9 ){
                case 0: r=h; c=nx; break; //row   
                case 1: r=nx; c=(h-9); break; //Column
                case 2: int b=(h-18); r=(b/3)*3+nx/3; c=(b%3)*3+nx%3; break; //block
            }
            return (r*9+c);
        }
		static public int B( this int rcx ){ return (rcx/27*3+(rcx%9)/3); }
		static public int ToBlock( this int rcx ){ return (rcx/27*3+(rcx%9)/3); }

        static public long BPReset( this long A, long B ){
            A &= (B^0x7FFFFFFFFFFFFFFF);
            return A;
        }


		static public int ToCompress( this UInt128 B, int dir ){
			int BC = 0;
			if( dir == 0 ){
				for( int k=0; k<81; k+=9 )	BC |= (int)((B>>k)&0x1FF);
			}
			else{
				for( int k=0; k<81; k++ ){
					int b = ((int)(B>>k)) & 1;
					if( b > 0 ) BC |= 1<<(k/9);
				}
			}
			return BC;
		}

		static public int ToVector( this UInt128 B, int H ){
			int BC = 0;
			if( H < 9 )  BC = (int)(B>>(H*9));
			else if( H < 18 ){
				for( int k=0; k<9; k++ ){
					int b = ((int)(B>>(k*9+H-9))) & 1;
					if( b > 0 ) BC |= 1<<k;
				}
			}
			return BC;
		}
		static public (int,int) ToVectorH( this UInt128 B, int H ){ /* house No -> Vector(9bit) */
			int BC = 0;
			if( H < 9 )  BC = (int)(B>>(H*9));
			else if( H < 18 ){
				for( int k=0; k<9; k++ ){
					int b = ((int)(B>>(k*9+H-9))) & 1;
					if( b > 0 ) BC |= 1<<k;
				}
			}
			return (BC,H);
		}


    #region Enumerators
        static public IEnumerable<UCell> IEGetCellInHouse( this List<UCell> pBOARD, int h, int FreeB=0x1FF ){
            //Find the cells with digits X(FreeB) in house.
            int r=0, c=0, tp=h/9, fx=h%9;
            for(int nx=0; nx<9; nx++ ){
                switch(tp ){
                    case 0: r=fx; c=nx; break; //row(h:0-8)
                    case 1: r=nx; c=fx; break; //column(h:9-17)
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break; //block(h:18-26)
                }
                UCell P = pBOARD[r*9+c];
                P.nx = nx;
                if((P.FreeB&FreeB)>0) yield return P;
            }
        }
        static public IEnumerable<UCell> IEGetCellInHouse2( this List<UCell> pBOARD, int h, int FreeB=0x1FF ){
            //Find the cells with digits X(FreeB) in house.
            int r=0, c=0, tp=h/9, fx=h%9;
            for(int nx=0; nx<9; nx++ ){
                switch(tp ){
                    case 0: r=fx; c=nx; break; //row(h:0-8)
                    case 1: r=nx; c=fx; break; //column(h:9-17)
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break; //block(h:18-26)
                }
                UCell P = pBOARD[r*9+c];
                P.nx = nx;
                if((P.FreeB&FreeB)>0) yield return P;
            }
        }
/*
        static public void IE_SetNoBBgColor( this Bit81 BX, List<UCell> pBOARD, int noBX, SWMColor AttCr, SWMColor Cr ){ 
            foreach( var p in BX.IEGet_UCell_noB(pBOARD,noBX) ) pBOARD[p.rc].Set_CellColorBkgColor_noBit(noBX,AttCr,Cr); 
        }
*/
        static public void IE_SetNoBBgColor( this UInt128 BX, List<UCell> pBOARD, int noBX, SWMColor AttCr, SWMColor Cr ){ 
			if( BX == UInt128.Zero ) return; 
            foreach( var p in BX.IEGet_UCell_noB(pBOARD,noBX) ) pBOARD[p.rc].Set_CellColorBkgColor_noBit(noBX,AttCr,Cr); 
        }

        static public void IE_SetNoBBgColorNSel( this UInt128 BX, List<UCell> pBOARD, int noBX, SWMColor AttCr, SWMColor Cr ){ 
			if( BX == UInt128.Zero ) return; 
            foreach( var p in BX.IEGet_UCell_noB(pBOARD,0x1FF) ) pBOARD[p.rc].Set_CellColorBkgColor_noBit(noBX,AttCr,Cr); 
        }

		static public void IE_SetNoBBgColor_All( this UInt128 BX, List<UCell> pBOARD, SWMColor AttCr, SWMColor Cr ){ 
			if( BX == UInt128.Zero ) return; 
            foreach( var p in BX.IEGet_UCell(pBOARD) ) pBOARD[p.rc].Set_CellColorBkgColor_noBit(0,AttCr,Cr); 
        }
        static public void IE_SetNoBBgColor( this List<UCell> LstUCell, List<UCell> pBOARD, int noBX, SWMColor AttCr, SWMColor Cr ){ 
			if( LstUCell==null || LstUCell.Count==0 ) return;
            LstUCell.ForEach( p => pBOARD[p.rc].Set_CellColorBkgColor_noBit( noBX, AttCr, Cr) );
        }

        // for UInt128 version
/*
        static public IEnumerable<UCell> IEGet_UCell_noB( this Bit81 BX, List<UCell> pBOARD, int noBX ){ //nx=0...8        
            UInt128 bp = BX.Get_BitExpression();
            for(int k=0; k<81; k++ ){
                if( ((bp>>k)&1)==0 ) continue;
                UCell P = pBOARD[k];
                if( (P.FreeB&noBX) > 0 )  yield return P;
            }
        }
        static public IEnumerable<int> IEGet_rc( this Bit81 X81 ){
            UInt128 bp = X81.Get_BitExpression();
            for(int k=0; k<81; k++ ){
                if( ((bp>>k)&1) > 0 ) yield return k;
            }
            yield break;
        }
*/
        static public IEnumerable<int> IEGet_BtoNo( this int noBin, int sz=9 ){
            for(int no=0; no<sz; no++ ){ if((noBin&(1<<no))>0)  yield return no; }
            yield break;
        }


        static public IEnumerable<int> IEGet_BtoHouse27( this int tfbContainer ){
            int P = tfbContainer;
            for(int m=0; m<27; m++ ){ if( (P&(1<<m)) > 0 )  yield return m; }
            yield break;
        }
        static public string Connect<T>( this IEnumerable<T> list, string separator ){
    	    return string.Join(separator,list);
        }

		static public IEnumerable<int> IEGet_Chute_BlockToBlock( this int b ){
			yield return 7 - (b%3)+1 + b/3*3;
			yield return 73- (b%3)*3 + 1;
			yield break;
		}
    #endregion

    }

}