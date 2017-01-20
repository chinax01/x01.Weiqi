/**
 * Stone.cs (c) 2015 by x01
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace x01.Weiqi.Boards
{
	/// <summary>
	/// Interaction logic for Stone.xaml
	/// </summary>
	public partial class Stone : UserControl
	{
		public Stone()
		{
			InitializeComponent();
		}
		
		public Brush StoneBrush 
		{ 
			get { return stone.Fill; }
			set { stone.Fill = value; }
		}
		public double StoneSize 
		{ 
			get { return stone.Width; }
			set { stone.Width = stone.Height = value; }
		}
		public string NumberText 
		{ 
			get { return number.Text; }
			set { number.Text = value; }
		}
		public Visibility NumberVisibility 
		{ 
			get { return number.Visibility; }
			set { number.Visibility = value; }
		}
		public double NumberFontSize 
		{ 
			get { return number.FontSize; }
			set { number.FontSize = value; }
		}
		public Brush NumberForeground 
		{
			get { return number.Foreground; }
			set { number.Foreground = value; }
		}
	}
}