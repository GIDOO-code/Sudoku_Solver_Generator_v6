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
using System.Collections;


namespace GNPX_space{
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//    under development (GNPXv5.1)
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

	//http://forum.enjoysudoku.com/fireworks-t39513.html
	//..128495.82.597..1945361872.5782.1.9...94.5.7..9715...562138794398472615..46592..
	//1457326..936185472.8.9641535..4..7.6....7...4..4..6.1..5..198474...58931819347265

    public partial class Firework_TechGen: AnalyzerBaseV2{
		private int[] sol_int81 => TandE_sol_int81;

        public bool Firework_Triple( ) {

			// ===== Prepare =====
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_Firework_TechGen();
			}
//##				__debug_sol_check();

			// ===== Analize =====
			//FireworkAgg_List.ForEach( P=> WriteLine(P) );
			if( FireworkAgg_List==null || FireworkAgg_List.Count<=0 )  return false; 
			var FW_Triple = FireworkAgg_List.FindAll(x => x.FreeB.BitCount() == 3); //Are there any cases of 4 or more?
			foreach(var fw3 in FW_Triple) {
				bool solfound = Firework_Triple_SolResult(fw3);

				if( !solfound )  continue;
				if( !pAnMan.IsContinueAnalysis() ) return true; // @is Valid
			}
			return false;
		}

		private bool Firework_Triple_SolResult( UFirework ufw ){
//##				__debug_sol_check();

			int FreeB = ufw.FreeB;
			int rc0=ufw.rcStem, rc1=ufw.rc1, rc2=ufw.rc2;
			UCell UC0=pBOARD[rc0], UC1=pBOARD[rc1], UC2=pBOARD[rc2];
			UC0.CancelB = UC0.FreeB.DifSet(FreeB);
			UC1.CancelB = UC1.FreeB.DifSet(FreeB);
			UC2.CancelB = UC2.FreeB.DifSet(FreeB);

			if( pBOARD.All(p=>p.CancelB==0) ){ pBOARD.ForEach(p=> p.ECrLst=null ); }
			else{			
				UC0.Set_CellBKGColor(SolBkCr);
				UC1.Set_CellBKGColor(SolBkCr2);
				UC2.Set_CellBKGColor(SolBkCr2);
				SolCode = 2;
				string st_StemR = ufw.ToStringResult();
				Result     = $"Firework_Triple: {st_StemR}";
				ResultLong = $"Firework_Triple\n  Firework: {st_StemR}";
			}
//##				__debug_sol_check();
			return (SolCode== 2);
		}
	}
}