using System;
using System.Collections.Generic;
using static System.Math;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Linq;
using System.Windows.Documents;
using System.Xml.Linq;

namespace GNPX_space{
    // UPuzzle : Class of Sudoku puzzle
    public class UPuzzle{
		static private object obj = new object();
        static private int    _IDpzl000=-1;				//Used twice in preparation. (1,2,3,...)
		static public G6_Base G6 => GNPX_App_Man.G6;

        private UInt128[]    pConnectedCells81 => AnalyzerBaseV2.ConnectedCells81;

        private int          _ID_obj;

        public int           ID_obj{ get=>_ID_obj; set=>_ID_obj=value; }  

        public int           ID;
		public long		     hashVal;
        public bool          _Not_Puzzle_;				// No solution (depending on the method used)
        
		public int           stageNo;					// stage No.
        public List<UCell>   BOARD;						// Board cells(81 cell)  Defined in List for use the Linq functions.
	  //public List<UCell>   Current_BOARD = null;
        public int[]         AnsNum;					// used in dPuzzleTrans

        public string        Name;						// Name
        public int           difficultyLevel;			//{ get; set; }    // -1:InitialState　0:Manual

        public string        TimeStamp;					// TimeStamp
        public UAlgMethod    pMethod = null;			// Analytical method
        public string        solMessage;				// Message of how to solve
        public string        Sol_Result;  //{ get; set; }  // Solution description
        public string        Sol_ResultLong;			// Solution description(Long)
        public string        extResult;   //{ get; set; }   // Solution description(ext.)
        public string        __SolResultKey;			// key for identity confirmation    
        public int           SolCode;


        public UPuzzle		 pre_PZL  = null;			// parents.
        public List<UPuzzle> Child_PZLs = new();	// children
        public int           selectedIX = -1;
        public UAlgMethod    method_MostDiffcult;	
/*		
		public UAlgMethod    method_maxDiffcult(){
			int maxL = (Child_PZLs!=null)? Child_PZLs.Max(p=>p.difficultyLevel): 99;
			return Child_PZLs.Find(p=>p.difficultyLevel==maxL).pMethod;
		}
*/

		// -------------------------------
		public bool		     g7MarkA;				// for develop (ver 5.2+)
		public bool			 g7Error;
		public string	     g7MarkA_Msg=null;		// for develop (ver 5.2+)
		// -------------------------------

        public UPuzzle( ){
            this.ID_obj     = _IDpzl000++;
			this.stageNo	= 0;
            this.BOARD      = new List<UCell>();
            for(int rc=0; rc<81; rc++ ) this.BOARD.Add(new UCell(rc));
            this.difficultyLevel = 0;
            this.extResult = "";
            this.Sol_ResultLong = "";
            this.__SolResultKey = "";
		
			Set_BOARD_hashVal( );

			G6.g7FinalState_TE = null;
			G6.g7CurrentState  = null;
        }

		public long Set_BOARD_hashVal( ) => hashVal = BOARD.Aggregate( 0L, (a,U) => a^ U.Get_hashValue_UCell() );
		
        public UPuzzle( string Name ): this(){ this.Name=Name; }
			
        public UPuzzle( List<UCell> BOARD, bool resetB=false ): this(){	
            this.BOARD    = BOARD;
            this.difficultyLevel = 0;	
            if(resetB) BOARD.ForEach(u=>u.Reset_result());
        }

        public UPuzzle( int ID, List<UCell> BOARD, string Name="", int difficultyLevel=0, string TimeStamp="" ): this(){
            this.ID       = ID;
            this.BOARD    = BOARD;
            this.Name     = Name;
            this.difficultyLevel = difficultyLevel;
            this.TimeStamp = TimeStamp;
        }

		public void Reset_ToInitial() => BOARD.ForEach( p=> p.No=Max(p.No,0));

		public void Clear(){
			Child_PZLs = null;	// children
			selectedIX = -1;
		}

        public UPuzzle Copy( int stageNo_Increments=0, int IDm=0 ){
            UPuzzle tmpPZL = (UPuzzle)this.MemberwiseClone();
            tmpPZL.ID_obj  = _IDpzl000*10000 + (this.ID_obj)%1000+1; 
//			tmpPZL.difficultyLevel = this.difficultyLevel;
            tmpPZL.BOARD = new List<UCell>();
            foreach( var q in BOARD ) tmpPZL.BOARD.Add(q.Copy());
            return tmpPZL;
        }

		public List<UCell>  LockAndCopy_BOARD(){
			lock(obj){
				List<UCell> _BOARD = new();
				foreach( var q in this.BOARD ) _BOARD.Add( q.Copy() );
				return BOARD;
			}
		}

		public void Randomize_PuzzleDigits( ){
			List<int> ranNum = new List<int>();
			for(int no=0; no<9; no++)  ranNum.Add( GNPX_App_Ctrl.GNPX_Random.Next(0,9)*10+no );
			ranNum.Sort((x,y) => (x-y));
			for(int no=0; no<9; no++) ranNum[no] = (ranNum[no]%10)+1;

			foreach( var q in BOARD ){
				int no=q.No, noR=ranNum[Abs(no)-1];
				q.No = (no>0)? no: -no;
			}
		} 
/*
		public void Save_Current_BOARD() => Current_BOARD = BOARD.Copy();
		public void Recover_Current_BOARD(){ 
			if(Current_BOARD!=null)  BOARD = Current_BOARD;
			Current_BOARD = null;
		}
*/
        public void ToPreStage( ) => ToInitial( resetAll:false );
        public void ToInitial( bool resetAll=true ){
            this.BOARD.ForEach( p => p.Reset_result(resetAll:resetAll) );
            this.AnsNum     = null;     // used in dPuzzleTrans
         // this.difficultyLevel   = -1;       // -1:InitialState　0:Manual
            this._Not_Puzzle_   = false;    // No solution (depending on the method used)
            this.pMethod    = null;     // Analytical method
            this.solMessage = "";       // Message of how to solve
            this.Sol_Result = "";       // Solution description
            this.Sol_ResultLong = "";   // Solution description(Long)
            this.extResult  = "";       // Solution description(ext.) 

            this.__SolResultKey = "";   // key for identity confirmation
      
            this.SolCode    = 0;
        }

        public string ToString_check( UPuzzle aPZL ){
            string st = $" ID:{aPZL.ID} ID_obj:{aPZL.ID_obj}  ";
            foreach( var P in aPZL.BOARD ){
                if( P.rc%9==0 ) st += " ";
                int n = P.No;
              //st += ((n>=0)? $" {n}": $"{n}"); 
                st += ((n==0)? " .": (n>0)? $" {n}": $"{n}"); 
            }

            st += "  FixedNo:";
            foreach( var P in aPZL.BOARD.Where(p=>p.FixedNo>0) )   st += $" , {P.rc.ToRCString()}#{P.FixedNo }";

            st += "  Canceled: ";
            foreach( var P in aPZL.BOARD.Where(p=>p.CancelB>0) )   st += $" , {P.rc.ToRCString()}#{P.CancelB.ToBitStringN(9)}";

         //   WriteLine(st);
            return st;
        }

    }
}