using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace x01.Weiqi.Board
{
	/// <summary>
	/// SaveStepWindow.xaml 的交互逻辑
	/// </summary>
	public partial class SaveStepWindow : Window
	{
		public string BlackName { get; set; }
		public string WhiteName { get; set; }
		public string Winer { get; set; }

		public SaveStepWindow()
		{
			InitializeComponent();
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			BlackName = m_BlackName.Text;
			WhiteName = m_WhiteName.Text;
			Winer = m_Winer.Text;
			Close();
		}
	}
}
