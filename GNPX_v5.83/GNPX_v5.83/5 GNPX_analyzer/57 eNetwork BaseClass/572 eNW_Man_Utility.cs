using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Windows.Documents;
using System.Diagnostics;

namespace GNPX_space {

    static public class eNetwork_Man_Utility{

        static public string eLinkFormatter( this string stSource, int len=60, int sft=-1 ){
            List<string> stList = stSource.Split(new []{ "=>" }, StringSplitOptions.None).ToList();
            var stList2 = stList.ConvertAll( p => p.Trim() );
            return  stList2.eLinkFormatter_sub(len,sft);
        }

        static public string eLinkFormatter_sub( this List<string> stList, int len, int sft=-1 ){
            List<int>  lenX = stList.ConvertAll( p => p.Length );

            string st = stList[0];
            int  sftX=(sft>0)? sft:(st.Length), nn=sftX;
            for( int k=1; k<stList.Count; k++ ){
                if( nn+lenX[k]>len ){
                    st += $"\n{new string(' ',sftX)}";
                    nn = sftX;
                }
                st += $" => {stList[k]}";
                nn += (lenX[k]+4);
            }
            return st;
        }

		        
		static private int _wk;
		static public int  rcbRow(this int rcbFrame) => rcbFrame&0x1FF;      //row expression
        static public int  rcbCol(this int rcbFrame) => (rcbFrame>>9)&0x1FF; //column expression
        static public int  rcbBlk(this int rcbFrame) => (rcbFrame>>18)&0x1FF;//block expression

        static public int  rcbFr_to_Row(this int rcbFrame) => ( (_wk=rcbFrame.rcbRow()).BitCount()==1 )? _wk.BitToNum(): -1;
        static public int  rcbFr_to_Col(this int rcbFrame) => ( (_wk=rcbFrame.rcbCol()).BitCount()==1 )? _wk.BitToNum(): -1;
        static public int  rcbFr_to_Blk(this int rcbFrame) => ( (_wk=rcbFrame.rcbBlk()).BitCount()==1 )? _wk.BitToNum(): -1;

        static public int  houseNo_Row(this int rcbFrame) => ( (_wk=rcbFrame.rcbRow()).BitCount()==1 )? _wk.BitToNum(): -1;
        static public int  houseNo_Col(this int rcbFrame) => ( (_wk=rcbFrame.rcbCol()).BitCount()==1 )? _wk.BitToNum()+9: -1;
        static public int  houseNo_Blk(this int rcbFrame) => ( (_wk=rcbFrame.rcbBlk()).BitCount()==1 )? _wk.BitToNum()+18: -1;
		static public bool IsInsideBlock(this int rcbFrame) => rcbBlk(rcbFrame).BitCount()==1;
    }

}
                        