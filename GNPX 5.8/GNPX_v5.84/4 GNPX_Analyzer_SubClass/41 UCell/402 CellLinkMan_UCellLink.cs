using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;
//using ABI.System;

namespace GNPX_space{
    public class CellLinkMan: AnalyzerBaseV2{
        // Cell-to-cell link
        //  Strong links are true and false in both directions. Link is 2 cells.
        //  Weak links propagate from true to false. Link is 3 or more cells.
        //  https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page25.html

        // HP:
        //  https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/


        public  int           SWCtrl;
        public  List<UCellLink>[] CeLK81;//cell Link
        public  List<UCellLinkExt>[] LKExt81;
        private bool          UCellLinkExt_IsActive => G6.UCellLinkExt;


        public CellLinkMan( GNPX_AnalyzerMan pAnMan ){
            this.pAnMan = pAnMan;
            SWCtrl=0;
        }

		public void  Initialize(){ SWCtrl=0; }
    
      #region Create Strong/WeakLink List            
        public void  PrepareCellLink( int swSW, bool lkExtB=false, bool printB=false ){
            int _chk = swSW.DifSet(SWCtrl);
            if( _chk==0 )  return;                 // already explored

            if( SWCtrl==0 ){
                CeLK81=new List<UCellLink>[81];
                if(lkExtB) LKExt81 = new List<UCellLinkExt>[81];
            }
            if( (_chk&1) > 0 ) Search_StrongLinks(true); //strong link
            if( (_chk&2) > 0 ) Search_WeakLinks( );      //weak link
            SWCtrl |= swSW; 

            int IDX=0;
            foreach( var P in CeLK81 ){
                if(P!=null){ P.Sort(); P.ForEach(Q=> Q.ID=(++IDX) ); }
            }

            if( UCellLinkExt_IsActive ){
                Search_UCellLinkExt1(); 

                for( int rc=0; rc<81; rc++ ){
                    var Q = LKExt81[rc];
                    if( Q != null )  Q = Q.DistinctBy(q=>q.keyNRCRC).ToList(); 
                }

				if( printB ) __NLPrint( CeLK81 );
            }  
        }

        private (int,int) Search_StrongLinks( bool weakOn ){
            for(int h=0; h<27; h++ ){
                for(int no=0; no<9; no++){
                    int noB = 1<<no;
                    List<UCell> PLst = pBOARD.IEGetCellInHouse(h,noB).ToList();
                    if(PLst.Count!=2)  continue;
                    UCell UC1=PLst[0], UC2=PLst[1];

                    //The algorithm is simplified with a list that includes forward and reverse directions.
                    //For example, skyscraper.
                    SetLinkList(h,1,no,UC1,UC2);
                    SetLinkList(h,1,no,UC2,UC1);  //Generate the opposite direction

                    if (weakOn){ //Strong links are also weak links.
                        SetLinkList(h,2,no,UC1,UC2);
                        SetLinkList(h,2,no,UC2,UC1);
                    }
                }  
            }               
        #region Debug Print
            // foreach( var P81 in CeLK81.Where(p=>p!=null) ) P81.Sort();
            // __NLPrint( CeLK81 );
        #endregion Debug Print
            var scwc = Count();
            return scwc;
        }

        private (int,int) Search_WeakLinks( ){
            for (int h=0; h<27; h++ ){
                    for(int no=0; no<9; no++){
                    int noB = 1<<no;
                    List<UCell> PLst = pBOARD.IEGetCellInHouse(h,noB).ToList();
                    List<UCell> PLst2 = pBOARD.IEGetCellInHouse2(h,noB).ToList();

                        string st0 = PLst.Aggregate( " ",(a,b) => a+$"b.ID.ToString() ");
                        string st2 = PLst2.Aggregate(" ",(a,b) => a+$"b.ID.ToString() ");
                        if( st0!=st2 )  WriteLine( $"st0:{st0}  st2:{st2}" );

                    if( PLst.Count<=2 ) continue;

                    bool SFlag=(PLst.Count==2);
                    for(int n=0; n<PLst.Count-1; n++){
                        UCell UC1=PLst[n];
                        for(int m=n+1; m<PLst.Count; m++ ){
                            UCell UC2=PLst[m];
                            SetLinkList(h,2,no,UC1,UC2);
                            SetLinkList(h,2,no,UC2,UC1);
                        }
                    }
                }  
            }     
        #region Debug Print
            // foreach( var P81 in CeLK81.Where(p=>p!=null) ) P81.Sort();
            // __NLPrint( CeLK81 );
        #endregion Debug Print
            var scwc = Count();
            return scwc;
        }

        private void __NLPrint( List<UCellLink>[] CeLkLst ){
            WriteLine("\r");
            int nc=0;
            foreach( var P81 in CeLkLst.Where(p=>p!=null) ){
                foreach( var P in P81 ){
                    int type = P.type;
                    int no   =  P.no;
                    int rc1  =  P.rc1;
                    int rc2  = P.rc2;

                    string st = "  No:" + (nc++).ToString().PadLeft(3);
                    st += "  type:" + type + "  no:" + (no+1);
                    if( type <= 2 ){
                        st += "  rc[" + rc1.ToString("00") + "]r" + ((rc1/9)+1);
                        st +=  "c" + ((rc1%9)+1) + "-b" + (rc1.ToBlock()+1);
                        st += " --> rc[" + rc2.ToString("00") + "]r" + ((rc2/9)+1);
                        st += "c" + ((rc2%9)+1) + "-b" + (rc2.ToBlock()+1);
                    }
                    else{
                        st += " " + ((rc1<10)? "r"+rc1: "c"+(rc1-10));
                        st += ((rc2<10)? "r"+rc2: "c"+(rc2-10));
                    }
                    WriteLine(st);
                }
            }
        }   

        public  void SetLinkList( int h, int type, int no, UCell UC1, UCell UC2 ){
            var LK = new UCellLink(h,type,no,UC1,UC2);
            int rc1=UC1.rc;
            if( CeLK81[rc1]==null ) CeLK81[rc1]=new List<UCellLink>();
            if( !CeLK81[rc1].Contains(LK) )  CeLK81[rc1].Add(LK);
        }
        public (int,int) Count(){ 
            int sc=0, wc=0;
            foreach( var P81 in CeLK81.Where(p=>p!=null) ){
                foreach( var P in P81 ){
                    int type = P.type;
                    if( type==1 ) sc++;
                    else wc++;
                }
            }
            return  (sc,wc);
        }
      #endregion Create Strong/WeakLink List

        public UInt128 GetCellsWithStrongLink( int no ){
            int noB = 1<<no;
            UInt128 BPstrong = UInt128.Zero;
            foreach( var P in CeLK81.Where(p=>p!=null) ){
                foreach( var Q in P.Where(q=>((q.no==no)&&(q.type&1)>0)) ){ 
                    BPstrong |= (UInt128.One<<Q.rc1) | (UInt128.One<<Q.rc2);
                }
            }
            return BPstrong;
        }

        public IEnumerable<UCellLink> IEGetCellInHouse( int typB ){
            foreach(var P in CeLK81.Where(p=>p!=null) ){
                foreach( var Q in P.Where(q=>((q.type&typB)>0)) ) yield return Q;
            }
        }
        public IEnumerable<UCellLink> IEGetNoType( int no, int typB ){
            foreach( var P in CeLK81.Where(p=>p!=null) ){
                foreach( var Q in P.Where(q=>((q.no==no)&&(q.type&typB)>0)) ) yield return Q;
            }
        }

        public IEnumerable<UCellLink> IEGetRcNoType( int rc, int no, int typB ){
            if( CeLK81[rc] == null ) yield break;
            foreach( var LK in CeLK81[rc].Where(p=> ((p.no==no)&&(p.type&typB)>0)) ){
                yield return LK;
            }
            yield break;
        }

        public IEnumerable<UCellLink> IEGetRcNoBTypB( int rc, int noB, int typB ){
            // typB=1:strong  link =2:weak link  =3:strong & weak link
            if( CeLK81[rc] == null ) yield break;
            foreach( var LK in CeLK81[rc] ){
                if( ((1<<LK.no)&noB)>0 && ((LK.type&typB)>0) ) yield return LK;
            }
            yield break;
        }

        private void Search_UCellLinkExt1(){

            foreach( var P in pBOARD.Where(p=> p.No==0 && CeLK81[p.rc]!=null ) ){ 

                int type = (P.FreeBC==2)? 1: 2;
                foreach( var LK in CeLK81[P.rc].Where(q=>q.type==1) ){
                    int no = LK.no;
                                //WriteLine( $" {P} #{LK}" ); 
                    foreach( int noH in LK.UCe1.FreeB.IEGet_BtoNo().Where(q=>q!=no) ){ 

                        foreach( int noT in LK.UCe2.FreeB.IEGet_BtoNo().Where(q=>q!=no) ){ 
                            var LKE = new UCellLinkExt(LK, noH, noT);
                            if( LKExt81[P.rc] is null ) LKExt81[P.rc] = new List<UCellLinkExt>();
                            LKExt81[P.rc].Add(LKE);
                                //WriteLine($" -- type1 -- {LKE}");
                        }
                    }

                }
            }
        }
    }


    public class UCellLink: IComparable{
        public int               ID;
        public int               h;
        public int               type;      // 1:Strong 2:Weak
        public bool              BVFlag;    // bivalued Link
        public bool              LoopFlag;  // Last Link

        public readonly int      no;
        public readonly UCell    UCe1;
        public readonly UCell    UCe2;
        public int               rc1{ get=>UCe1.rc; }
        public int               rc2{ get=>UCe2.rc; }
        public readonly UInt128  B81;

        public UCellLink(){}
        public UCellLink( int h, int type, int no, UCell UCe1, UCell UCe2 ){
            this.h=h; this.type=type; this.no=no;
            this.UCe1=UCe1; this.UCe2=UCe2; this.ID=h;
            BVFlag = UCe1.FreeBC==2 && UCe2.FreeBC==2;
            B81 = (UInt128.One<<rc1) | (UInt128.One<<rc2);
        }

        public UCellLink( int no, UCell UCe1, UCell UCe2 ): this( h:-99, type:2, no: no, UCe1, UCe2) { }
      
        public int CompareTo( object obj ){
            UCellLink Q = obj as UCellLink;
            if( this.type!=Q.type ) return (this.type-Q.type);
            if( this.no  !=Q.no   ) return (this.no-Q.no);
            if( this.rc1 !=Q.rc1  ) return (this.rc1-Q.rc1);
            if( this.rc2 !=Q.rc2  ) return (this.rc2-Q.rc2);
            return (this.ID-Q.ID);
        }
        public override bool Equals( object obj ){
            UCellLink Q = obj as UCellLink;
            if( Q==null )  return true;
            if( this.type!=Q.type || this.no!=Q.no )   return false;
            if( this.rc1!=Q.rc1   || this.rc2!=Q.rc2 ) return false;
            return true;
        }
        public override string ToString(){
            string st= $"ID:{ID.ToString().PadLeft(2)}  type:{type} no:#{no+1}";
            st +=  $" rc1:{rc1.ToRCString()}({rc1:00}) rc2:{rc2.ToRCString()}({rc2:00})"; 
            return st;
        }

        public override int GetHashCode(){ return base.GetHashCode(); }
    }


    public class UCellLink128: IComparable{
        public int               ID;
        public int               h;
        public int               type;      // 1:Strong 2:Weak
        public bool              BVFlag;    // bivalued Link
        public bool              LoopFlag;  // Last Link

        public readonly int      no;
        public readonly UCell    UCe1;
        public readonly UCell    UCe2;
        public int               rc1{ get=>UCe1.rc; }
        public int               rc2{ get=>UCe2.rc; }
        public readonly UInt128  b081;

        public UCellLink128(){}
        public UCellLink128( int h, int type, int no, UCell UCe1, UCell UCe2 ){
            this.h=h; this.type=type; this.no=no;
            this.UCe1=UCe1; this.UCe2=UCe2; this.ID=h;
            BVFlag = UCe1.FreeBC==2 && UCe2.FreeBC==2;
            b081 = (UInt128.One<<rc1 | UInt128.One<<rc2);
        }

        public UCellLink128( int no, UCell UCe1, UCell UCe2 ): this( h:-99, type:2, no: no, UCe1, UCe2) { }
      
        public int CompareTo( object obj ){
            UCellLink Q = obj as UCellLink;
            if( this.type!=Q.type ) return (this.type-Q.type);
            if( this.no  !=Q.no   ) return (this.no-Q.no);
            if( this.rc1 !=Q.rc1  ) return (this.rc1-Q.rc1);
            if( this.rc2 !=Q.rc2  ) return (this.rc2-Q.rc2);
            return (this.ID-Q.ID);
        }
        public override bool Equals( object obj ){
            UCellLink Q = obj as UCellLink;
            if( Q==null )  return true;
            if( this.type!=Q.type || this.no!=Q.no )   return false;
            if( this.rc1!=Q.rc1   || this.rc2!=Q.rc2 ) return false;
            return true;
        }
        public override string ToString(){
            string st= $"ID:{ID.ToString().PadLeft(2)}  type:{type} no:#{no+1}";
            st +=  $" rc1:{rc1.ToString().PadLeft(2)} rc2:{rc2.ToString().PadLeft(2)}"; 
            return st;
        }

        public override int GetHashCode(){ return base.GetHashCode(); }
    }
}