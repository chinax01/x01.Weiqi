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

namespace x01.Weiqi.Dialog
{
	/// <summary>
	/// AboutWindow.xaml 的交互逻辑
	/// </summary>
	public partial class AboutDialog : Window
	{
		public AboutDialog()
		{
			InitializeComponent();

			this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Key == Key.Enter || e.Key == Key.Escape)
				Close();
		}
	}
}
