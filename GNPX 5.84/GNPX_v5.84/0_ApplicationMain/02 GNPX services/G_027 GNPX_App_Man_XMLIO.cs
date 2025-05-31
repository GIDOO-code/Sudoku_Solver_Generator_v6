using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using static System.Diagnostics.Debug;

using GIDOO_space;


namespace GNPX_space{

    public partial class GNPX_App_Man { 

        public List<UAlgMethod> ReadXMLFile_AnalysisConditions_MethodList( ){
			//WriteLine( "          @@@@@2 ReadXMLFile_AnalysisConditions_MethodList" );

            SolverList_Base = new();
            SolverList_Base.AddRange(SolverList_Def);

            SolverList_Base.ForEach(P=>P.IsChecked=true);

			try{
				{ // ----- XML read ----- 
					var currentDir = Directory.GetCurrentDirectory();
					var _pfName = Path.Combine(currentDir, SDK_Methods_XMLFileName);	

					if( File.Exists( _pfName ) ){
						using( StreamReader rd = new StreamReader(_pfName)){
							G6 = (G6_Base)G6.xmlSerializer.Deserialize(rd);
						}
					}
				
					G6.Dir_SDK_Methods_XMLFileName = _pfName;
				}

				{ // ----- methodList ----- 
					if( SolverList_Base == null )  SolverList_Base = SolverList_Def;
					G6.Get_methodList( SolverList_Base ); // Setting the Solver +/- Information
					G6.MSlvr_MaxLevel = SolverList_Base.Max( p=>p.difficultyLevel );
					G6.stopped_StatusMessage = "";

					SolverList_Base.Sort( (p,q)=>(p.ID-q.ID) );
				}
			}
			catch(Exception e){ WriteLine( $"{e.Message}\n{e.StackTrace}" ); }
            return SolverList_Base;
		}


        public void WriteXMLFile_AnalysisConditions_MethodList( ){
            if( SolverList_Base == null || SolverList_App.Count<=1 ) return;

			G6.Set_methodList( SolverList_Base );
			{ // ---- XML Write -----
				var currentDir = Directory.GetCurrentDirectory();
				var _pfName = Path.Combine(currentDir, SDK_Methods_XMLFileName);		

				using( StreamWriter wr = new StreamWriter(_pfName) ){
					G6.xmlSerializer.Serialize(wr, G6);
				}
			}
					WriteLine( $"===== WriteXMLFile_AnalysisConditions_MethodList() ... done" +  DateTime.Now.ToString("G") );
					WriteLine( $" path/file:{SDK_Methods_XMLFileName}" );

            bool B = G6.GeneralLogic_on;
			var QLst = SolverList_App.FindAll(x=>x.MethodName.Contains("GeneralLogic"));
			QLst.ForEach( Q => Q.IsChecked=B );

            var Q2Lst=SolverList_App.FindAll(x=>x.MethodName.Contains("GeneralLogic"));
			Q2Lst.ForEach( Q => Q.IsChecked=B );
			return;
        }
    }

}