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


namespace GNPX_space{
// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
//    under development (GNPXv5.1)
// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

/*
	//The New Sudoku Players' Forum, ÅwFireworksÅx,
	// http://forum.enjoysudoku.com/fireworks-t39513.html
	//http://forum.enjoysudoku.com/fireworks-t39513-45.html
*/

// 148753.6.63594218772916853498.516.7.367429815.51387..6.1.8756..89623475157.691.48
// 148753.6.63594218772916853498.516.7.367429815.51387..6.1.8756..89623475157.691.48

    public partial class Firework_TechGen: AnalyzerBaseV2{
        public bool Firework_LWing( ){
			// ===== Prepare =====
			if( stageNoP != stageNoPMemo ){
				stageNoPMemo = stageNoP;
				base.AnalyzerBaseV2_PrepareStage();
				Prepare_Firework_TechGen();
			}
			debugPrint = false;	
			 
			// ===== Analize =====
			if( FireworkAgg_List==null || FireworkAgg_List.Count<=0 )  return false; 
			var FWAgg2_List = FireworkAgg_List.FindAll(q=> q.sw && q.FreeBC==2);	// Dual Firework
					//FWAgg2_List.ForEach( P=> WriteLine(P) );

			//*** Select fwStem
			foreach( UFirework fwStem in FWAgg2_List ){												// [1] Select fwStem
					//WriteLine( $"\n fwStem:{fwStem}" );

				//*** Select fwAssist
				foreach( UFirework fwAssist in Firework_List.
					Where( p=> p.sw && p.rc12B81==fwStem.rc12B81 && p.rcStem!=fwStem.rcStem) ){
					//WriteLine( $"fwAssist:{fwAssist}" );

					int no = fwAssist.FreeB.BitToNum();
					if( (FreeCell81b9[no] & fwAssist.rc12B81).BitCount() != 2 )  continue;

					if( (fwStem.FreeB & fwAssist.FreeB) > 0 )  continue;

					bool solfound = Firework_Dual_LWing_s_SolResult( fwStem, fwAssist );
					if( !solfound )  continue;

					if( !pAnMan.IsContinueAnalysis() )  return true; // @is Valid
				}
			}
			return false;
		}

		private bool Firework_Dual_LWing_s_SolResult( UFirework fwStem, UFirework fwAssist ){	
			int   rcStem=fwStem.rcStem, FreeB=fwStem.FreeB, FreeBLeaf=fwAssist.FreeB;

			UCell UCStem=pBOARD[rcStem],  UC1=pBOARD[fwStem.rc1], UC2=pBOARD[fwStem.rc2];
			UCell UCAssist=pBOARD[fwAssist.rcStem];

			UC1.CancelB = UC1.FreeB & fwAssist.FreeB;
			UC2.CancelB = UC2.FreeB & fwAssist.FreeB;

			if( pBOARD.All(p=>p.CancelB==0) ){ pBOARD.ForEach(p=> p.ECrLst=null ); }
			else{
				SolCode = 2;

				//public void Set_CellColorBkgColor_noBit( int noB, Color clr, Color clrBkg ){
				UCStem.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr);
				UC1.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr);
				UC2.Set_CellColorBkgColor_noBit( FreeB, AttCr, SolBkCr);
				UCAssist.Set_CellColorBkgColor_noBit( fwAssist.FreeB, AttCr, SolBkCr2);

				string st_fw = fwStem.ToStringResult();
				string st_Assist = fwAssist.ToStringResult();

				string st_Exclude = "";
				if( UC1.CancelB>0 )  st_Exclude = $" {UC1.rc.ToRCString()}#{fwAssist.FreeB.ToBitStringNZ(9)}";
				if( UC2.CancelB>0 )  st_Exclude += $" {UC2.rc.ToRCString()}#{fwAssist.FreeB.ToBitStringNZ(9)}";

				Result     = $"Firework_LWing Firework:{st_fw} Assist:{st_Assist} Exclude:{st_Exclude}";
				ResultLong = $"Firework_LWing\n  Firework : {st_fw}\n  Assist : {st_Assist}\n  Exclude :{st_Exclude}";
			}

			return (SolCode==2);
		}
	}
}