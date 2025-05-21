using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;
using System.Windows.Controls;

namespace GNPX_space{

    public class UFish{
        public int      ID;                         //ID
        public int      no;                         //digit
        public int      sz;                         //fish size (2D:Xwing 3D:SwordFish 4D:JellyFish 5D:Squirmbag 6D:Whale 7D:Leviathan)
        public int      HouseB     = 0;             //house No. in baseSet
        public UInt128  BaseB81    = UInt128.Zero;  //Bit expression of BaseSet cells

        public UFish    BaseSet    = null;          //baseSet reference in CoverSet
        public int      HouseC=0;                   //house No. in coverSet
        public UInt128  CoverB81   = UInt128.Zero;  //Bit expression of coverSet cells
        public UInt128  FinB81     = UInt128.Zero;  //Bit expression of fin cells
        public UInt128  EndoFinB81 = UInt128.Zero;  //Bit expression of EndoFinB81 cells
        public UInt128  CannFinB81 = UInt128.Zero;  //Bit expression of cannibalisticFin cells

        public UFish(){ }

        public UFish( int no, int sz, int HouseB, UInt128 BaseB81, UInt128 EndoFinB81 ){
            this.no=no;
            this.sz=sz;
            this.HouseB     = HouseB;
            this.BaseB81    = BaseB81;
            this.EndoFinB81 = EndoFinB81;
        }
          
        public UFish( UFish BaseSet, int HouseC, UInt128 CoverB81, UInt128 FinB81, UInt128 CannFinB81 ){
            this.sz         = BaseSet.sz;
            this.no         = BaseSet.no;
            this.BaseSet    = BaseSet;
            this.HouseB     = BaseSet.HouseB;
            this.HouseC     = HouseC;
            this.CoverB81   = CoverB81;
            this.FinB81     = FinB81;
            this.CannFinB81 = CannFinB81;
        }
        public string ToString( string ttl ){
            string st = ttl + HouseB.HouseToString();
            return st;
        }

        public override string ToString( ){
            string st = $"UFish   ID : {ID}   no+ : {no+1}   sz : {sz} "; 
            string st2="";

            if( BaseSet is null ){ //BaseSet
                st2 += $"\n\n\n====================";
                st2 += $"\n-- {st} Baseset --";
                st2 += $"\n--  BaseB81 : {BaseB81.ToBitString81N()}\n--   HouseB : {HouseB.HouseToString()}";
            }
            else{
                st2 += $"\n++ {st} Coverset ++";
                st2 += $"\n++ CoverB81 : {CoverB81.ToBitString81N()}\n++   HouseC : {HouseC.HouseToString()}";
                st2 += $"\n++   FinB81 : {FinB81}\n++  EndoFinB81 :{EndoFinB81}\n++  CannFinB81 :{CannFinB81}";
            }
            return st2;
        }
    }


}