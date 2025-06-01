using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;
using static System.Math;
using System.IO;

using GIDOO_space;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using System.Diagnostics;
using System.Xml.Linq;
using System.Reflection.Emit;


namespace GNPX_space{

		using tuple4 = (int,int,int,int);
		using tuple381 = (int,int,UInt128);

// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
//    under development (GNPXv5.1)
// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

	//1.....5.82.........4..9.7.6..3..2....26.8.9..5....92......73....8....1.....9...42
	//1.5984..6..9.65.84486.....9...64.9.8.948.1.6.6.8.93.4.5..4168929427386..861..9473

    public partial class Firework_TechGen: AnalyzerBaseV2{

        public bool Firework_Quadruple( ){
			// ===== Prepare =====
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_Firework_TechGen();
			}
			debugPrint = false;

		// ===== Analize =====
			SolCode = -1;
			if( FireworkAgg_List==null || FireworkAgg_List.Count<=0 )  return false; 
			var FWAgg2_List = FireworkAgg_List.FindAll(x => x.FreeBC>=2);
						//FWAgg2_List.ForEach( P=> WriteLine(P) );

			//__DebugPrint(pBOARD, false, "Quadruple2");


			if( FWAgg2_List.Count>=2 ){
				Combination cmb = new( FWAgg2_List.Count(), 2 );  
				while( cmb.Successor() ){
					var ufw0 = FWAgg2_List[ cmb.Index[0] ];
					var ufw1 = FWAgg2_List[ cmb.Index[1] ];
				
					if( ( ufw0.FreeB & ufw1.FreeB ) != 0 )  continue;	// No overlapping digits
				    if( ufw0.rc12B81 != ufw1.rc12B81 )  continue;		// Leafs matches
						//WriteLine( $"ufw0:{ufw0} \nufw1:{ufw1}" ); 
					
					UInt128 or128 = (ufw0.rc12B81 | ufw1.rc12B81);
						//WriteLine( $"ufw0.rc12B81 | ufw1.rc12B81:{or128.ToBitString81N()}" );
						//WriteLine( $"ufw0.rc12B81 | ufw1.rc12B81:{ufw0} \nufw1:{ufw1}" ); 

					bool solfound = Firework_Quadruple_SolResult( ufw0, ufw1 );
					if( !solfound )  continue;
					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
				}
			}

			return false;
		}

		private bool Firework_Quadruple_SolResult( UFirework ufw0, UFirework ufw1 ){	
			bool IsBothAnd( int X, int A, int B ) => (X&A)>0 && (X&B)>0;
			int FreeBa=ufw0.FreeB, FreeBb=ufw1.FreeB;
			UInt128 leaf=ufw0.rc12B81;
				 //Utility_Display.__DBUG_Print2(pBOARD, false, "Quadruple2");

			var (rc1, rc2) = leaf.Get_rc1_rc2();
			UCell UC0a=pBOARD[ufw0.rcStem],  UC0b=pBOARD[ufw1.rcStem], UC1=pBOARD[rc1], UC2=pBOARD[rc2];

			int FreeBT = FreeBa | FreeBb;
			UC0a.CancelB = UC0a.FreeB.DifSet(FreeBa);
			UC0b.CancelB = UC0b.FreeB.DifSet(FreeBb);
			UC1.CancelB = UC1.FreeB.DifSet(FreeBT);
			UC2.CancelB = UC2.FreeB.DifSet(FreeBT);
		
			if( pBOARD.All(p=>p.CancelB==0) ){ pBOARD.ForEach(p=> p.ECrLst=null ); }
			else{
				UC0a.Set_CellBKGColor(SolBkCr);
				UC0b.Set_CellBKGColor(SolBkCr);
				UC1.Set_CellBKGColor(SolBkCr2);
				UC2.Set_CellBKGColor(SolBkCr2);
				SolCode = 2;
				Result     = $"Firework_Quadruple FW1:{ufw0.ToStringResult()} FW2:{ufw1.ToStringResult()} ";
				ResultLong = $"Firework_Quadruple\n  Firework1 : {ufw0.ToStringResult()}\n  Firework2 : {ufw1.ToStringResult()} ";
			}

			return (SolCode==2);
		}

	}
}