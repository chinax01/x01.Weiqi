// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Linq;
using x01.Weiqi.Model;

namespace x01.Weiqi.Board
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class LoadStepWindow : Window
	{
		IStepService m_StepService = new StepService();

		public int StepId { get; set; }

		public LoadStepWindow()
		{
			InitializeComponent();
			StepId = 1;

			int[] stepIds = m_StepService.GetIds();
			m_ComboBox.ItemsSource = stepIds;
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			StepId = (int)m_ComboBox.SelectedItem;
			ShowInfo();
		}

		void ShowInfo()
		{
			using (var db = new WeiqiContext()) {
				var r = (from w in db.Steps
						 where w.Id == StepId
						 select w).First();
				m_Description.Text = "黑方： " + r.BlackName + "\n"
					+ "白方： " + r.WhiteName + "\n"
					+ "对局时间： " + r.CreateDate.ToShortDateString() + "\n"
					+ "对局结果： " + r.Winer;
			}
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			StepId = -1;
			Close();
		}

	}
}