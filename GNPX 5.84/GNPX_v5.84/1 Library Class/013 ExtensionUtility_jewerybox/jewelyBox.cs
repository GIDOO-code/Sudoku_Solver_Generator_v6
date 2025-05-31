using System;
using System.Linq;
using System.Collections.Generic;
using static System.Diagnostics.Debug;
using System.Globalization;

using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using System.Resources;
using System.Runtime.InteropServices;
using System.Diagnostics;
using GNPX_space;
using System.Xml.Serialization;
using ABI.System;


namespace GIDOO_space{

    static public partial class Extension_Utility{

		static public T DeepCopy<T>(this T src){
            using( MemoryStream stream = new MemoryStream() ){
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, src);
                stream.Position = 0;

                return (T)serializer.Deserialize(stream);
            }
        }


        static readonly int[] __BC=new int[512]; //Number of 1's in binary expression 0-511

        static Extension_Utility(){
            for(int n=0; n<512; n++ ) __BC[n] = (n+512).BitCount()-1;           //avoid recursion with "+512"

		  //WriteLine( $"qMaxB81:{qMaxB81.ToBitString81()}");
        }

        static public void Swap<T>( ref T a, ref T b ){ T w=b; b=a; a=w; }

		static public void ForEach<T>(this List<T> input, Action<T,int> action){
			for( int k=0; k<input.Count; k++){ action(input[k],k); }
		}

        static public void EnumVisual( Visual myVisual, ref List<Visual> Vis ){
            for(int i=0; i<VisualTreeHelper.GetChildrenCount(myVisual); i++){
                // Retrieve child visual at specified index value.
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(myVisual, i);
                Vis.Add(childVisual);
                WriteLine(childVisual.ToString());
                // Do processing of the child visual object.

                // Enumerate children of the child visual object.
                EnumVisual(childVisual, ref Vis);
            }
        }

        static public bool Inner( this MouseButtonEventArgs e, FrameworkElement Q ){
            Point pt = e.GetPosition(Q);
            if(pt.X<=0.0 || pt.Y<=0.0)  return false;
            if(pt.X>=Q.Width  || pt.Y>=Q.Height)  return false;
            return true;
        }

        static public double ToDouble( this string st ) =>  double.Parse( st );
        static public float ToFloat( this string st )   => float.Parse( st );

        static public int ToInt( this string st )       => (st=="")? 0: int.Parse( st );

        static public int ToInt( this char ch )         => int.Parse( ch.ToString() );

        static public int DifSet( this int A, int B )		=> A & ~B;				//(int)(A&(B^0x7FFFFFFF)); 
        static public int DifSet( this uint A, uint B )		=> (int)( A & ~B );		//(int)(A&(B^0xFFFFFFFF)); 
        static public long DifSet( this long A, long B )	=> A & ~B;				//(long)(A&(B^0x7FFFFFFFFFFFFFFF));
        static public ulong DifSet( this ulong A, ulong B ) => A & ~B;				//(ulong)(A&(B^0xFFFFFFFFFFFFFFFF));

        static public bool Include( this int A, int B ) => (A&B) == B;

		static public int ToBit(int val, int n) => (val>0)? 1<<n:0; 
		static public ulong ToBitULong(int val, int n) => (val>0)? 1u<<n:0; 
		static public UInt128 ToBitUUInt128(int val, int n) => (val>0)? UInt128.One<<n: UInt128.Zero; 

        static public bool IsHit( this int A, int n )   => (A & (1<<n)) > 0;


        static public string ToBitString( this int noB, int ncc ){
            string st="";
            for(int n=0; n<ncc; n++ ) st += (((noB>>n)&1)!=0)? (n+1).ToString(): "."; 
            return st;
        } 

        static public int BitSet( this int X, int n )   => X |= (1<<n);

        static public int BitReset( this int X, int n ) => X &= (1<<n)^0x7FFFFFFF;

		static public int BC( this int nf ) => nf.BitCount();

        static public int BitCount( this int nF ){      //by Hacker's Delight
			return ((uint)nF).BitCount();
/*
            if( nF < 512 )  return __BC[nF];  //for 9 bits or less, refer to the table. fast.
            int x = nF;
            x = (x&0x55555555) + ((x>>1)&0x55555555);
            x = (x&0x33333333) + ((x>>2)&0x33333333);
            x = (x&0x0F0F0F0F) + ((x>>4)&0x0F0F0F0F);
            x = (x&0x00FF00FF) + ((x>>8)&0x00FF00FF);
            x = (x&0x0000FFFF) + ((x>>16)&0x0000FFFF);
            return x;
*/
        }

        static public int BitCount( this uint nF ){     //by Hacker's Delight
            if( nF < 512 )  return __BC[nF];  //for 9 bits or less, refer to the table. fast.
            uint x = nF;
            x = (x&0x55555555) + ((x>>1)&0x55555555);
            x = (x&0x33333333) + ((x>>2)&0x33333333);
            x = (x&0x0F0F0F0F) + ((x>>4)&0x0F0F0F0F);
            x = (x&0x00FF00FF) + ((x>>8)&0x00FF00FF);
            x = (x&0x0000FFFF) + ((x>>16)&0x0000FFFF);
            return (int)x;
        }
        static public bool IsNumeric( this string stTarget ){ //Test whether a string is a number
            int nNullable;
            return int.TryParse( stTarget, NumberStyles.Any, null, out nNullable );
        }
        static public bool IsNumeric( this char chTarget ){ //Test whether a string is a number
            int nNullable;
            return int.TryParse( chTarget.ToString(), NumberStyles.Any, null, out nNullable );
        }

		static public string TimespanToString( this System.TimeSpan ts ){
            string st = "";
            if( ts.TotalMinutes>2.0 ) st += ts.TotalMinutes.ToString("0.0") + " min";
			else if( ts.TotalSeconds>1.0 ) st += ts.TotalSeconds.ToString("0.0") + " sec";
            else                      st += ts.TotalMilliseconds.ToString("0.0") + " msec";
            return st;
        }

		static public string RemainingTimeToString( this System.TimeSpan ts, int nc, int ncMax ){
			System.TimeSpan tsT = ((double)(ncMax)) / (nc) * ts;
			ts = tsT * ((double)(ncMax-nc)) / nc;
			return  ts.TimespanToString();
		}

   //http://stackoverflow.com/questions/677373/generate-random-values-in-c-sharp
        static public long NextInt64( this Random rnd ){
            byte[] bytes = new byte[8];
            rnd.NextBytes(bytes);
            return BitConverter.ToInt64(bytes,0);

            // Assume rng refers to an instance of System.Random
            //byte[] bytes = new byte[8];
            //rng.NextBytes(bytes);
            //long int64 = BitConverter.ToInt64(bytes, 0);
            //ulong uint64 = BitConverter.ToUInt64(bytes, 0);
            //Note that using Random.Next() twice, shifting one value and then ORing/adding doesn't work.
            //Random.Next() only produces non-negative integers, i.e.
            //it generates 31 bits, not 32, so the result of two calls only produces 62 random bits
            //instead of the 64 bits required to cover the complete range of Int64/UInt64.
            //(Guffa's answer shows how to do it with three calls to Random.Next() though.)
        }


		static public List<T> Copy<T>( this List<T> L ){
			List<T> LCopy = new List<T>();
			L.ForEach( Q => LCopy.Add(Q));
			return LCopy;
		}

        static public List<T> GetControlsCollection<T>( object parent ) where T :DependencyObject{
            //WPF - Getting a collection of all controls for a given type on a Page
            //http://stackoverflow.com/questions/7153248/wpf-getting-a-collection-of-all-controls-for-a-given-type-on-a-page
            //using: List<Button> buttons = GetControlsCollection<Button>(yourPage);

			// @Attension : Use with care during initialization. The controls must already be defined.

            List<T> logicalCollection = new List<T>();
            GetControlsCollection( parent as DependencyObject, logicalCollection );
            return logicalCollection;
        }
        static private void GetControlsCollection<T>( DependencyObject parent, List<T> logicalCollection ) where T: DependencyObject{
            var children = LogicalTreeHelper.GetChildren(parent);
            foreach( object child in children ){
                if(child is DependencyObject){
                    DependencyObject depChild = child as DependencyObject;
                    if( child is T )logicalCollection.Add(child as T);
                    GetControlsCollection(depChild, logicalCollection);
                }
            }
        }

		public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source){
			return source.Select((item, index) => (item, index));
		}
		public static IEnumerable<string> ToLower(this IEnumerable<string> source){
			return source.Select(v => v.ToLower());
		}
		public static IEnumerable<string> ToUpper(this IEnumerable<string> source){
			return source.Select(v => v.ToUpper());
		}

        static private char[] sepDef=new Char[]{ ' ', ',', '\t' };
/*
        static public string[] SplitEx( this string st, char[] sep ){
            List<string> eLst = st.Split(sep,StringSplitOptions.RemoveEmptyEntries).ToList();
			for( int k=0; k<eLst.Count; ++k ){
                if( eLst[k] != string.Empty && eLst[k].TrimStart()[0] == '"' ){
                    while( eLst[k].TrimEnd().LastOrDefault() != '"'){
                        eLst[k] = eLst[k] + " " + eLst[k+1];
                        eLst.RemoveAt(k+1);
                    }
					eLst[k] = eLst[k].Replace("\"","");
                }
            }
            return eLst.ToArray();
        }
*/

		static public string AddSpace9_81( this string st ){
			string stA = "";
			for( int k=0; k<st.Length-1; k+=9 ) stA += " " + st.Substring( k, 9 );
			return stA.Trim();
		}

		static public string[] SplitEx( this string st, char[] sep, bool withTrimB=true ){
			// 234562789" a b12"x y  Z"" 123 "z z" "y  // ... Bad test data
			string[] eLst = null;

			int nx = st.IndexOf( "\"", 0 );
			if( nx < 0 ){ 
				eLst = st.Split(sep,StringSplitOptions.RemoveEmptyEntries);
				if(withTrimB){ 
					for( int k=0; k<eLst.Length ; ++k ){ eLst[k] = eLst[k].Trim(); }
				}
				return  eLst;
			}
			
			string st2 = st.Substring( 0, nx );
			st2 = st2.Replace( " ", "\b" ) + "\b"; 
			foreach( char _sp in sep )  st2 = st2.Replace( _sp, '\b' );
			st2 += "\b"; 


			bool   Qin = false;
			for( int k=nx; k<st.Length; k++ ){
				char s = st[k];
				if( s == '\"' ){
					Qin = !Qin;
					st2 += '\b';
				}
				else{
					if( !Qin && IsSeparator(s,sep) ) s = '\b';
					st2 += s;
				}
			}

			eLst = st2.Split("\b",StringSplitOptions.RemoveEmptyEntries);
			if(withTrimB){ 
				for( int k=0; k<eLst.Length ; ++k ){ eLst[k] = eLst[k].Trim(); }
			}
			return eLst;

			bool IsSeparator( char s, char[] sep ){
				foreach( char _sp in sep ){
					if( s == _sp )  return true;
				}
				return  false;
			}
        }


        static public void ProcessExe( string url ){
            // Process.Start for URLs on .NET Core
            // https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
            try{  
                Process.Start(url);  
            }  
            catch{  
                if( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ){      //for Windows  
                    url = url.Replace("&", "^&");  
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow=true });  
                }  
                else if( RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ){   //for Linux  
                    Process.Start("xdg-open", url);  
                }  
                else if( RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ){     //for Mac  
                    Process.Start("open", url);  
                }  
                else{ throw; }  
            }  

        }



/*   *****   Scheduled for deletion ... Learning.
		static public string DBVal<T>( this T val ) => val.DegugValue<T>( );

		static public string DegugValue( this string val ) => $"{nameof(val)} : {val}";
		static public string DegugValue( this int val ) => $"{nameof(val)} : {val}";
		static public string DegugValue( this uint val ) => $"{nameof(val)} : {val}";

		static public string DegugValue<T>( this T val ){
			string st = nameof(val);
			var t = typeof(T);
			foreach (var f in t.GetFields())	st += $"{f.Name} : {f.GetValue(f) }";

			foreach (var f in typeof(val).GetFields())	st += $"{f.Name} : {f.GetValue(f) }";
			return st;
		}

		static public string DegugValue( this UCell val ){
			string st = nameof(val);
			var t = typeof(UCell);
			foreach (var f in t.GetFields())	st += $"{f.Name} : {f.GetValue(f) }";
			foreach (var f in typeof(UCell).GetFields())	st += $"{f.Name} : {f.GetValue(f) }";
			return st;
		}
*/

    }

        

}