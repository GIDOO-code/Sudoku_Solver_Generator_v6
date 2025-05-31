using GNPX_space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

using GIDOO_space;

namespace GNPX_space{

	public static class GNPX_Puzzle_Global_Control{
		static public string GNPX_mode;	//"file", "solve", "create", "option", "transform"
		static public string GNPX_modeSub;


        static public   GNPX_Engin      pGNPX_Eng;                      //Analysis Engine
		static  private UPuzzle			_ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; } // Puzzle to analyze

		static GNPX_Puzzle_Global_Control(){ }


		static public long Get_hashValue_int81( this int[] SolX ){
			long hv = 0;
			for( int rc=0; rc<81; rc++ ){
				if( SolX[rc]>0 )  hv ^= GIDOO_Hash.hashBase[ ((rc)<<6)  | SolX[rc] ];
			}
			return hv;
		}

		static public long Get_hashValue_int81( this List<int> SolXL ){
			long hv = 0;
			foreach( var (S,rc) in SolXL.WithIndex() ){
				if( S>0 )  hv ^= GIDOO_Hash.hashBase[ ((rc)<<6)  | S ];
			}
			return hv;
		}

		static public long Get_hashValue_UCell( this UCell U ) => GIDOO_Hash.hashBase[ ((U.rc)<<6)  | U.No ];




	#region Puzzle management in the GNPX application
		static private List<UPuzzle>	_GNPX_Puzzle_List = new List<UPuzzle>();
		static private int				_current_Puzzle_No;


		static public int				GNPX_Puzzle_List_Count => _GNPX_Puzzle_List.Count();
		static public bool				IsEmpty => _GNPX_Puzzle_List.Count()==0 || _GNPX_Puzzle_List[0].BOARD.All(p=>p.No==0); 
		static public  List<UPuzzle>	GNPX_Puzzle_List => _GNPX_Puzzle_List;

		static public  void				Set_GNPX_Puzzle_List( List<UPuzzle> UPzlList )  => _GNPX_Puzzle_List = UPzlList;
		static public  void				Clear_GNPX_Puzzle_List( )  => _GNPX_Puzzle_List.Clear();

		static public void				GNPX_Puzzle_List_Initialize(){_GNPX_Puzzle_List.Clear(); }
		
		
		
		
		static public void	GNPX_Puzzle_List_Add( UPuzzle Q ){	
			if( _GNPX_Puzzle_List.Count == 1 ){
				UPuzzle P  = _GNPX_Puzzle_List.First();
				if( P.BOARD.All(p=> p.No==0) )  _GNPX_Puzzle_List.Clear();
			}

			long BHash = Q.Set_BOARD_hashVal();
			if( _GNPX_Puzzle_List.Find( P=> P.hashVal==BHash ) == null ){ 
				Q.ID = _GNPX_Puzzle_List.Count;
				_GNPX_Puzzle_List.Add(Q);
			}
		}

/*
		static public void				GNPX_Puzzle_List_AddRAnge( List<UPuzzle> QList ){	
			if( _GNPX_Puzzle_List.Count == 1 ){
				UPuzzle P  = _GNPX_Puzzle_List.First();
				if( P.BOARD.All(p=> p.No==0) )  _GNPX_Puzzle_List.Clear();
			}
			_GNPX_Puzzle_List.AddRange(QList);
		}
*/




		static public int				current_Puzzle_No{  // Consecutive numbers starting with 0
            get=> _current_Puzzle_No;
            set{ 
                int nn = value;
				nn = Min( nn, _GNPX_Puzzle_List.Count-1);
					// if( nn == int.MaxValue )  nn = _GNPX_Puzzle_List.Count-1;
                nn = Max( nn, 0 );
                if( _GNPX_Puzzle_List.Count > 0 ){
                    if( nn>=_GNPX_Puzzle_List.Count ) nn = _GNPX_Puzzle_List.Count-1;
                    _current_Puzzle_No = nn;
                    pGNPX_Eng.Set_NewPuzzle( _GNPX_Puzzle_List[nn] );
                }
            }
        }




		static public UPuzzle			GetCurrentPuzzle( int noX=0 ){
			int no = (noX==int.MaxValue)?  _GNPX_Puzzle_List.Count-1: current_Puzzle_No;
  			UPuzzle P = ( no>=0 && no<=_GNPX_Puzzle_List.Count-1 )? _GNPX_Puzzle_List[no]: null;

			if( P==null ){
				P = new UPuzzle();
				_GNPX_Puzzle_List.Add(P); 
				current_Puzzle_No = 0;
			}
            return P;
        }


		static public UPuzzle			GNPX_Get_Puzzle( int noX ){
  			UPuzzle P = ( noX>=0 && noX<=_GNPX_Puzzle_List.Count-1 )? _GNPX_Puzzle_List[noX]: null;
            return P;
        }


		static public void Randumize_All(){
			foreach( var P in _GNPX_Puzzle_List )  P.Randomize_PuzzleDigits();
		}



		static public void GNPX_Puzzle_List_Replace( int no, UPuzzle Q ) => _GNPX_Puzzle_List[no] = Q;



        static public void _GNPX_Puzzle_List_Add_EngGP(){
            GNPX_Puzzle_List_Add(_ePZL);
        }   
        static public void GNPX_Puzzle_List_Add_ifNotContain(){
            if( !Contain(_ePZL) )  _GNPX_Puzzle_List_Add_EngGP();
        }


        static public void CreateNewPuzzle( UPuzzle aPZL=null ){
            if(aPZL==null) aPZL = new UPuzzle("New Puzzle");
            aPZL.ID=_GNPX_Puzzle_List.Count; 
            pGNPX_Eng.Set_NewPuzzle(aPZL);
            GNPX_Puzzle_List_Add(aPZL);
            current_Puzzle_No = int.MaxValue;
        }
     

        static public void GNPX_Remove(){
            int PnoMemo = current_Puzzle_No;
            if( PnoMemo==_GNPX_Puzzle_List.Count-1 ) PnoMemo--;
            if( Contain(_ePZL) ) _GNPX_Puzzle_List.Remove(_ePZL);
            current_Puzzle_No = PnoMemo;
        }

        static private bool Contain( UPuzzle aPZL ){
			long _hashVal = aPZL.Set_BOARD_hashVal( );
			var P = _GNPX_Puzzle_List.Find(P=>P.hashVal==_hashVal);
            return (P!=null);
        }
	#endregion
	}
}