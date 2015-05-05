using System.Windows;

namespace x01.Weiqi.Board
{
	/// <summary>
	/// SaveStepWindow.xaml 的交互逻辑
	/// </summary>
	public partial class SaveStepWindow : Window
	{
		public string Description { get; set; }
		public string BlackName { get; set; }
		public string WhiteName { get; set; }
		public string Winer { get; set; }
		public bool IsSaved { get; set; }

		public SaveStepWindow()
		{
			InitializeComponent();
			IsSaved = false;
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			Description = m_Title.Text;
			BlackName = m_BlackName.Text;
			WhiteName = m_WhiteName.Text;
			Winer = m_Winer.Text;
			IsSaved = true;
			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}