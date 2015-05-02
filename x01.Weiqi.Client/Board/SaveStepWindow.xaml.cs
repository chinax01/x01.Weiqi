using System.Windows;

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