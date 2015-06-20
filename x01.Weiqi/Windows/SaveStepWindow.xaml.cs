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
	/// SaveStepDialog.xaml 的交互逻辑
	/// </summary>
	public partial class SaveStepWindow : Window
	{
		public string Description { get; set; }
		public string BlackName { get; set; }
		public string WhiteName { get; set; }
		public string Result { get; set; }
		public int Type { get; set; }

		public SaveStepWindow()
		{
			InitializeComponent();
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			Description = m_Description.Text;
			BlackName = m_BlackName.Text;
			WhiteName = m_WhiteName.Text;
			Result = m_Result.Text;
			Type = (int)m_Type.SelectedValue;
			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
