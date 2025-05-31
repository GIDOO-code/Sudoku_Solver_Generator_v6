using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

using Windows.Foundation.Collections;

namespace GNPX_space{
    /*  for test    
        List<string>[] strLst = new List<string>[3];
        strLst[0] = new List<string>{"a","b","c"};
        strLst[1] = new List<string>{"X","Y"};
        strLst[2] = new List<string>{"1","2","3","4"};
        foreach( var P in IEGet_ListIndex(strLst) ){ 
            string st="P:";
            foreach( var x in P )  st += $" {x}";
            WriteLine( st );
        }
        Console.ReadKey();
    }
    */



    public class Combination{
        protected readonly int N;
        protected readonly int R;

        private bool  First=false;
        public int[] Index = new int[1];

        public Combination( int Nx, int Rx ){
            this.N = Nx;
            this.R = Math.Min(N,Rx);
            if(R>0 && R<=N){
                Index=new int[R];
                Index[0]=0;
                for(int m=1; m<R; m++) Index[m]=Index[m-1]+1;
                First=true;
			}
        }
        public bool Successor(){
            if(N<=0) return false;
            if(First){ First=false; }
            else{
                int m=R-1;
				if(Index!=null){
					while(m>=0 && Index[m]==(N-R+m)) m--;
					if(m<0){ Index=null; return false; }
					Index[m]++;
					for(int k=m+1; k<R; k++) Index[k]=Index[k-1]+1;
				}
            }
            return true;
        }
 
        public bool Successor(int skip=int.MaxValue){
            if(N<=0) return false;
            if(First){ First=false; return (Index!=null); }
 
            int k;
			if( Index==null ) return false;

			if(Index[0]==N-R) return false;
			if(skip<R-1){
				for(k=skip; k>=0; k--){ if(Index[k]<=N-R) break; }
				if(k<0)  return false;
			}
			else{
				for(k=R-1; k>0 && Index[k]==N-R+k; --k);
			}

			++Index[k]; 
			for(int j=k; j<R-1; ++j)  Index[j+1]=Index[j]+1;

            return true;
        }

        public int ToBitExpression( ){
            int bitR = 0;
            if( Index!=null )  foreach( var x in Index ) bitR |= (1<<x);
            return bitR;
        }
        public int ToIntBitExpression( List<int> BULst ){
            int bitR = 0;
			if( Index==null )  return 0;
            foreach( var x in Index.Select(p => BULst[p]))  bitR |= 1<<x;
            return bitR;
        }
        public UInt128 ToUInt128BitExpression( List<int> BULst ){
            UInt128 bitR = 0;
			if( Index==null )  return 0;
            foreach( var x in Index.Select(p => BULst[p]))  bitR |= (UInt128.One<<x);
            return bitR;
        }
        public IEnumerable<int> IEGetIndex(){
			if( Index==null)  yield break;
            for(int m=0; m<R; m++) yield return Index[m];
            yield break;
        }
        public IEnumerable<(int,int)> IEGetIndex2(){
			if( Index==null)  yield break;
            for(int m=0; m<R; m++) yield return (m,Index[m]);
            yield break;
        }
        public  List<T> GetCollection<T>( List<T> L ){
            List<T> Lst = new();
			if( Index==null)  return Lst;
            for(int m=0; m<R; m++) Lst.Add( L[Index[m]] );
            return Lst;
        }

        public override string ToString(){
            string st="";
            if( Index!=null )  Array.ForEach(Index, p=>{ st+=(" "+p);} );
            return st;
        }
    }





	public class Combination_9B: Combination{
        private int noBit;
        private List<int> _noBitLst;
        public int[] index2;
		public int   noB9;

        public Combination_9B( int N, int R, int noBit ): base(N,R){
            this.noBit=noBit;
            _noBitLst = _IEGet_BtoNo(noBit,9).ToList().ConvertAll(p=>1<<p);
            index2 = new int[R];
			return;

					IEnumerable<int> _IEGet_BtoNo( int noBin, int sz=9 ){
						for(int no=0; no<sz; no++ ){ if((noBin&(1<<no))>0)  yield return no; }
						yield break;
					}
        }

        public new bool Successor(int skip=int.MaxValue){
			int q=0;
            if(base.Successor(skip)){ 
				noB9 = 0;
                if( Index!=null ){
					for(int k=0; k<R; k++){ q= index2[k]=_noBitLst[base.Index[k]]; noB9|=q; }
				}
                return true;
            }
            return false;
        }
    }

	public class Combination_81B: Combination{
        private UInt128 U81B;
        private List<int> _rcList;
		public  UInt128 value81B;

        public Combination_81B( int N, int R, UInt128 U81B ): base(N,R){
            this.U81B=U81B;
            _rcList = _IEGetRC( U81B, 81).ToList();
			return; 

			IEnumerable<int> _IEGetRC( UInt128 U, int mx=81){
				UInt128  B = U;
				for(int m=0; m<mx; m++){
					if( (B&1)>0 ) yield return m;
					B = B>>1;
				}
			}
        }

        public new bool Successor(int skip=int.MaxValue){
            if(base.Successor(skip)){ 
				value81B = UInt128.Zero;
				if( Index!=null ){
					for(int k=0; k<R; k++){  value81B |= UInt128.One << _rcList[base.Index[k] ]; }
				}
                return true;
            }
            return false;
        }
    }

/*
	static public class jewelyBoxInner{
	    static public IEnumerable<int> IEGet_BtoNo( this int noBin, int sz=9 ){
            for(int no=0; no<sz; no++ ){ if((noBin&(1<<no))>0)  yield return no; }
            yield break;
        }
        static public IEnumerable<int> IEGet_rc( this UInt128 U, int mx=81){
            UInt128  B = U;
            for(int m=0; m<mx; m++){
                if( (B&1)>0 ) yield return m;
                B = B>>1;
            }
        }
	}
*/
}
