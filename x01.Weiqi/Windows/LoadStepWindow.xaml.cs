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
using x01.Weiqi.Boards;
using x01.Weiqi.Models;

namespace x01.Weiqi.Windows
{
	/// <summary>
	/// LoadStepDialog.xaml 的交互逻辑
	/// </summary>
	public partial class LoadStepWindow : Window
	{
		public Record Record { get; set; }

		public LoadStepWindow()
		{
			InitializeComponent();

			Loaded += (s, e) => {
				try {
					m_RecordGrid.DataContext = GetRecords();
					m_RecordGrid.Focus();
				} catch (Exception ex) {
					MessageBox.Show("GetRecords() Error: \n" + ex.Message);
				}
			};
		}

		public List<Record> GetRecords()
		{
			using (var db = new WeiqiContext()) {
				var r = from s in db.Records
						orderby s.SaveDate descending
						select s;
				return r.ToList();
			}
		}

		private void m_RecordGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			SelectRecord();
		}

		private void SelectRecord()
		{
			Record = m_RecordGrid.SelectedItem as Record;
			Close();
		}

		private void m_RecordGrid_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				SelectRecord();
		}
	}
}
