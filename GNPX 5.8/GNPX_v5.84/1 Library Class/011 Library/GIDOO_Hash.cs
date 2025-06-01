using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using static System.Math;
using GIDOO_space;
using System.Windows.Media.Animation;
//using Accessibility;
using System.Security.Policy;
using System.Runtime.Serialization;

namespace GIDOO_space{
    static public class GIDOO_Hash{
        static private int _NSZ = 8192;
        static public  long[] hashBase = new long[_NSZ];  
        static Random rnd = new Random(314);
        static GIDOO_Hash(){
            Random rnd = new Random(314);
            for( int k=0; k<hashBase.Length; k++ ) hashBase[k] = rnd.NextInt64();
        }
    }
}