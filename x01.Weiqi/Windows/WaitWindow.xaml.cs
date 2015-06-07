using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace x01.Weiqi.Windows
{
	/// <summary>
	/// WaitWindow.xaml 的交互逻辑
	/// </summary>
	public partial class WaitWindow : Window
	{
		public WaitWindow()
		{
			InitializeComponent();

			MouseLeftButtonDown += (s, e) => Hide();
		}
	}
}
