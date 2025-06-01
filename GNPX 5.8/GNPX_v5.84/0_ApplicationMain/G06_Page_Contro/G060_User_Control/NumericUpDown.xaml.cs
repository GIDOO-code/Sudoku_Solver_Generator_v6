using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using static System.Diagnostics.Debug;
using static System.Math;
using System.Windows.Controls;
using System.Xml.Linq;

namespace GIDOO_space{
    public delegate void GIDOOEventHandler( object sender, GIDOOEventArgs args );
 
    // [ATT] The following are important and essential:
    //       Up/Down images must specify "resource" properties. Then compile. 

    public partial class NumericUpDown: UserControl{
        public event   GIDOOEventHandler ValueChanged; 
		private bool   EventStop=false;
		private string txtMemo;

        public static readonly DependencyProperty ValueProperty    = DependencyProperty.Register(     "Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(1));
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(  "MinValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(1));
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(  "MaxValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(20));
        public static readonly DependencyProperty IncrementProperty= DependencyProperty.Register( "Increment", typeof(int), typeof(NumericUpDown), new PropertyMetadata(1));

        public int Value{
            get=> (int)GetValue(ValueProperty);
            set{
                var v = Max( Min(value,MaxValue), MinValue);
                SetValue(ValueProperty, v);
				txtMemo = v.ToString();
            }
        }
        public int MinValue{
            get=> (int)GetValue(MinValueProperty);
            set{
                SetValue(MinValueProperty, value);
                if( Value<value )  Value = value;
            }
        }
        public int MaxValue{
            get=> (int)GetValue(MaxValueProperty);
            set{ 
                SetValue(MaxValueProperty, value);
                if( Value>value )  Value = value;
            }
        }
        public int Increment{
            get=> (int)GetValue(IncrementProperty);
            set=> SetValue(IncrementProperty, value);
        }

        public NumericUpDown(){
            InitializeComponent();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e){
            int inc=(Increment>1)? Increment: 1;
            int k = Value+inc;
            Value = Min(k,MaxValue);
		  //Send_Changed();
        }
        private void DownButton_Click(object sender, RoutedEventArgs e){
            int inc=(Increment>1)? Increment: 1;
            int k = Value-inc;
            Value = Max(k,MinValue);
		  //Send_Changed();
        }

        private void textBoxValue_TextChanged(Object sender,TextChangedEventArgs e){   
			if( EventStop )  return;
			try{
				int _Value = int.Parse(textBoxValue.Text);
				_Value = Min( _Value, MaxValue);
				Value = Max( _Value, MinValue);
				textBoxValue.Text = Value.ToString();
			}
			catch(Exception ex){
				EventStop=true;
				textBoxValue.Text = txtMemo;
				EventStop=false;;
			}
			Send_Changed();
		}

		private void Send_Changed(){
			if( ValueChanged == null )  return;
			ValueChanged( this, new GIDOOEventArgs( eName:"UD_Changed", eValue:Value ));
        }
    }


    public class GIDOOEventArgs: EventArgs{
	    public string eName;
	    public int    eValue;

	    public GIDOOEventArgs( string eName=null, int eValue=-1 ){
            try{
		        this.eName = eName;
		        this.eValue = eValue;
            }
            catch(Exception e ){ WriteLine(e.Message); WriteLine(e.StackTrace); }
	    }
    }
}