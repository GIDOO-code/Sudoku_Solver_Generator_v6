using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Xml.Linq;

namespace GNPX_space {

    [Serializable]



	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//            eNW_Link_IntraCell
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	public class eNW_Link_IntraCell: eNetwork_Link, IComparable{
		public UCell   qUC;

		public eNW_Link_IntraCell( UCell UC, ULogical_Node OrgN, ULogical_Node DesN ): base( OrgN, DesN ){
			this.qUC = UC;
			this.eObject = UInt128.Zero;//One<<UC.rc;  //OrgN.b081;   //UInt128.One<<UC.rc;
			this.lkCode = 10 + DesN.b081Size;
			this.keySt = CreateKey();
		}
		public override string CreateKey() => this.ToString().Trim();
		public override int CompareTo( object obj ){
			var xUC = obj as eNW_Link_IntraCell;
			if( xUC is null  )  return -1;
			int cmp = this.qUC.rc - xUC.qUC.rc;
			if( cmp != 0 )  return cmp;
			return (this.noOrg-xUC.noDes);
		}

		public override string ToString(){
			string  stOrg="--", stDes="--";
			if( OrgN!=null )  stOrg = $"{OrgN.ToString_SameHouseComp()}#{noOrg+1}{OrgN.TFmark}";
			if( DesN!=null ) stDes = $"{DesN.ToString_SameHouseComp()}#{noDes+1}{DesN.TFmark}";
			string st = $"{base.ToString()}{stOrg} => [intra cell -> {stDes}]";
			return st;
		}
	}





	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//            eNW_Link_InterCells
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	public class eNW_Link_InterCells: eNetwork_Link, IComparable{

		public eNW_Link_InterCells( ULogical_Node OrgN, ULogical_Node DesN ): base( OrgN, DesN){
			this.eObject = DesN.b081;//OrgN.b081 | DesN.b081;      // (InterCells is not displayed)  ???
			this.lkCode = 20 + DesN.b081Size;
			this.keySt = CreateKey();
		}

		public override string CreateKey() => this.ToString().Trim();
		public override int CompareTo( object obj ){
			var eGL = obj as eNW_Link_InterCells;
			if( eGL is null  )  return -1;
			if( this.noOrg != eGL.noOrg )  return (this.noOrg-eGL.noOrg);
			if( this.OrgN.b081 != eGL.OrgN.b081) return this.OrgN.CompareTo( eGL.OrgN );
			if( this.DesN.b081 != eGL.DesN.b081) return this.DesN.CompareTo( eGL.DesN );
			return  0;
		}

		public override string ToString(){
			string  stOrg="--", stDes="--";
			if( OrgN!=null )  stOrg = $"{OrgN.ToString_SameHouseComp()}#{noOrg+1}{OrgN.TFmark}";
			if( DesN!=null )  stDes = $"{DesN.ToString_SameHouseComp()}#{noDes+1}{DesN.TFmark}";
			string st = $"{base.ToString()}{stOrg} => [inter cell -> {stDes}]";
			return st;
		}
	}




  
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//            eNW_Link_ALS
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//ALS(Almost Locked Set) 
	// ALS is a state where there are "n+1 candidate digits" in "n cells" belonging to the same house. 
	// https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page26.html
	public class eNW_Link_ALS: eNetwork_Link, IComparable{
		public UAnLS     qALS;

		public eNW_Link_ALS( UAnLS qALS, ULogical_Node OrgN, ULogical_Node DesN ): base( OrgN, DesN ){
			this.qALS = qALS;
			this.eObject = qALS.bitExp;
			this.lkCode = 30;
			this.keySt = CreateKey();
		}

		public override string CreateKey() => this.ToString().Trim();
		public override int CompareTo( object xObj ){
			var x = xObj as eNW_Link_ALS;
			if( x is null )  return -1;

			int cmp = this.qALS.CompareTo(x.qALS );
			if( cmp != 0 )  return cmp;
			cmp = this.keySt.CompareTo(x.keySt);
			if( cmp != 0 )  return cmp;
			return base.CompareTo(xObj);
		}

		public override string ToString(){
			string  stOrg="--", stDes="--";

			if( OrgN!=null ) stOrg = $"{OrgN.ToString_SameHouseComp()}#{OrgN.no+1}";

			int fb0=qALS.FreeB, fb1=fb0.DifSet(1<<noOrg);
			string stObj = $"ALS:{qALS.bitExp.ToRCStringComp()} #{fb0.ToBitStringNZ(9)}";           
			stObj += $"->#{fb1.ToBitStringNZ(9)}";
			if( _withID_ )  stObj = $"ID:{ID} " + stObj;

			if( DesN!=null ){
				stDes = $"{DesN.ToString_SameHouseComp()}";
				if( qConnectTo_RC>=0 )  stDes=qConnectTo_RC.ToRCString();   //=== Refine the display of solutions ===

				stDes += $"#{DesN.no+1}{DesN.TFmark}";
			}

			string st = $"{base.ToString()}{stOrg}+ => [{stObj} ->{stDes}]";   

			return st;
		}
	}




	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//            eNW_Link_refALS
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//refALS(reconfigure Almost Locked Set) 
	// ALS is a state where there are "n+1 candidate digits" in "n cells" belonging to the same house. 
	// https://gidoo-code.github.io/Sudoku_Solver_Generator_v5/page26.html

	public class eNW_Link_refALS: eNetwork_Link, IComparable {
		public UAnLS qrefALS;

		public eNW_Link_refALS(UAnLS qALS, ULogical_Node OrgN, ULogical_Node DesN ): base( OrgN, DesN ){
			this.qrefALS = qALS;
			this.eObject = qALS.bitExp;
			this.lkCode = 32;
			this.keySt = CreateKey();
		}

		public override string CreateKey( ) => this.ToString().Trim();

		public override int CompareTo(object xObj) {
			var x = xObj as eNW_Link_refALS;
			if(x is null)
				return -1;

			int cmp = this.qrefALS.CompareTo(x.qrefALS);
			if(cmp != 0)
				return cmp;
			cmp = this.keySt.CompareTo(x.keySt);
			if(cmp != 0)
				return cmp;
			return base.CompareTo(xObj);
		}

		public override string ToString( ) {
			string stOrg = "--", stDes = "--";

			if(OrgN != null)
				stOrg = $"{OrgN.ToString_SameHouseComp()}#{OrgN.no + 1}";

			int fb0 = qrefALS.FreeB, fb1 = qrefALS.FreeBwk;//  fb0.DifSet(1<<noOrg);		//<<< FreeBwk
			string stObj = $"eALS:{qrefALS.bitExp.ToRCStringComp()} #{fb0.ToBitStringNZ(9)}";
			stObj += $"->#{fb1.ToBitStringNZ(9)}";
			if(_withID_)
				stObj = $"ID:{ID} " + stObj;

			if(DesN != null) {
			stDes = $"{DesN.ToString_SameHouseComp()}";
			if(qConnectTo_RC >= 0)
				stDes = qConnectTo_RC.ToRCString();   //=== Refine the display of solutions ===
			stDes += $"#{DesN.no + 1}{DesN.TFmark}";
			}

			string st = $"{base.ToString()}{stOrg}+ => [{stObj} ->{stDes}]";

			return st;
		}
	}




	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//            eNW_Link_AIC
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	public class eNW_Link_AIC: eNetwork_Link, IComparable{   
		public ULogical_Node qAIC;
		public ULogical_Node B81OrgN_2;

		public eNW_Link_AIC( ULogical_Node qAIC, ULogical_Node  OrgN, ULogical_Node OrgN2, ULogical_Node DesN ): base( OrgN, DesN ){

			this.qAIC = qAIC;
			this.eObject = qAIC.b081;
			this.B81OrgN_2 = OrgN2;
			this.lkCode = 40;
			this.keySt = CreateKey();
		}

		public override string CreateKey() => this.ToString().Trim();
		public override int CompareTo( object obj ){
			var xAIC = obj as eNW_Link_AIC;
			if( xAIC is null  )  return -1;
			int cmp = this.qAIC.CompareTo(xAIC);
			if( cmp != 0 )  return cmp;
			return base.CompareTo(obj);
		}

		public override string ToString(){
			string  stOrg="--", stDes="--", stObj="--";

			if( OrgN!=null ){
				stOrg = $"{OrgN.ToString_SameHouseComp()}#{noOrg+1}{OrgN.TFmark}";
				stObj = $"AIC:{qAIC.ToString_SameHouseComp()} {B81OrgN_2.ToString_SameHouseComp()}#{noOrg+1}{B81OrgN_2.TFmark}";
					if( _withID_ )  stObj = $"ID:{ID} " + stObj;
			}
			if( DesN!=null ){
				stDes = $"{DesN.ToString_SameHouseComp()}";
				if( qConnectTo_RC>=0 )  stDes=qConnectTo_RC.ToRCString();   //=== Refine the display of solutions ===
				stDes += $"#{noDes+1}{DesN.TFmark}";
			}
			string st = $"{base.ToString()}{stOrg} => [{stObj} ->{stDes}]";
			return st;
		}
	}



	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	//            eNW_Link_ALSXZ
	// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
	public class eNW_Link_ALSXZ: eNetwork_Link, IComparable{
		private UAnLS   qALS1;
		private UAnLS   qAALS2;
		private int    RCC;
		public  int    RCCsize;

		public eNW_Link_ALSXZ( UAnLS qALS1, UAnLS qAALS2, int RCC, ULogical_Node OrgN, ULogical_Node DesN ): base( OrgN, DesN ){

			this.qALS1 = qALS1;
			this.qAALS2 = qAALS2;
			this.RCC = RCC;
			this.RCCsize = RCC.BitCount();

			this.eObject = qALS1.bitExp;
			if( qAALS2!=null ) this.eObject = qALS1.bitExp | qAALS2.bitExp;
			this.lkCode = 50;
			this.keySt = CreateKey();
		}

		public override string CreateKey() => this.ToString().Trim();
		public override int CompareTo( object xObj ){
			var x = xObj as eNW_Link_ALSXZ;
			if( x is null )  return -1;

			int cmp1 = this.qALS1.CompareTo(x.qALS1 );
			if( cmp1 != 0 )  return cmp1;
			int cmp2 = this.qAALS2.CompareTo(x.qAALS2 );
			if( cmp2 != 0 )  return cmp2;
			int cmp = this.keySt.CompareTo(x.keySt);
			if( cmp != 0 )  return cmp;
			return base.CompareTo(xObj);
		}

		public override string ToString(){
			string  stOrg="--", stDes="--";

			if( OrgN!=null ) stOrg = $"{OrgN.ToString_SameHouseComp()}#{OrgN.no+1}{OrgN.TFmark}";

			int fb1=qALS1.FreeB, fb2=qAALS2.FreeB;
			string stObj1 = $"ALS1:{qALS1.bitExp.ToRCStringComp()}#{fb1.ToBitStringNZ(9)}";  
			string stObj2 = $"AALS2:{qAALS2.bitExp.ToRCStringComp()}#{fb2.ToBitStringNZ(9)}"; 

			if( _withID_ ){
				stObj1 = $"ID:{qALS1.ID} " + stObj1;
				stObj1 = $"ID:{qAALS2.ID} " + stObj1;
			}
			string stObj = $"{stObj1} @ {stObj2} (RCC:#{RCC.ToBitStringNZ(9)})";

			if( DesN!=null ) stDes = $"{DesN.ToString_SameHouseComp()}";
			stDes += $"#{DesN.no+1}{DesN.TFmark}";

		  	//string stNxt = "";
			//if( eGLink_Nxt != null ){
			//	stNxt = $"{eGLink_Nxt.ToString_SameHouseComp()}";
			//	if( qConnectTo_RC>=0 )  stNxt=eGLink_Nxt.ToRCBString();     //=== Refine the display of solutions ===
			//	stNxt += $"#{eGLink_Nxt.no+1}{eGLink_Nxt.TFmark}";
			//}

			//string st = $"ŸŸŸ{stOrg} =>ALSXZ[{stObj}] =>{stDes}";   
			string st = $"{base.ToString()}[{stOrg} => ALSXZ[{stObj}] =>{stDes}]";   
			return st;
		}
	}

}
