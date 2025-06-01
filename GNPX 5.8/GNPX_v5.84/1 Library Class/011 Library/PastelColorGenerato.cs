using System;
using System.Windows.Media;


// base idia  http://blog.functionalfun.net/2008/07/random-pastel-colour-generator.html
	namespace Simon_Squared{
	public class RandomPastelColorGenerator{
		private readonly Random _random;

		public RandomPastelColorGenerator(){
			const int RandomSeed = 2;
			_random = new Random(RandomSeed);
		}

		public SolidColorBrush GetNextBrush(){
			SolidColorBrush brush = new SolidColorBrush(GetNext());
			brush.Freeze();
			return brush;
		}

		public Color GetNext( int per=127 ){
			byte[] colorBytes = new byte[3];
			per = per%256;
			int a=256-per, b=per;
			colorBytes[0] = (byte)(_random.Next(a) + b);
			colorBytes[1] = (byte)(_random.Next(a) + b);
			colorBytes[2] = (byte)(_random.Next(a) + b);

			Color color = new Color();
			color.A = 255;		// make the color fully opaque
			color.R = colorBytes[0];
			color.B = colorBytes[1];
			color.G = colorBytes[2];

			return color;
		}
	}
}