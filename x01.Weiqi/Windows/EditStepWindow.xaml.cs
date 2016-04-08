/**
 * EditStepWindow.cs (c) 2015 by x01
 */
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

using x01.Weiqi.Boards;
using x01.Weiqi.Models;

namespace x01.Weiqi.Windows
{
	/// <summary>
	/// Interaction logic for EditWindow.xaml
	/// </summary>
	public partial class EditStepWindow : Window
	{
		public EditStepWindow()
		{
			InitializeComponent();

			SizeChanged += (s, e) =>
				m_Board.StoneSize = (int)((Math.Min(ActualWidth - 300, ActualHeight) - 52) / 19);

			m_Id.ItemsSource = GetIds();
			m_Id.SelectedIndex = 0;
			M_Id_SelectionChanged(null, null);

			m_Board.MouseLeftButtonDown += new MouseButtonEventHandler(m_Board_MouseLeftButtonDown);
			m_Board.MouseRightButtonDown += new MouseButtonEventHandler(m_Board_MouseRightButtonDown);
		}

		void m_Board_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var board = sender as Board;
			board.BackOne();
			var s = m_Steps.Text;
			var arr = s.Split(',');
			s = "";
			for (int i = 0; i < arr.Length - 4; i++) {
				s += arr[i] + ",";
			}
			m_Steps.Text = s;
		}

		void m_Board_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var board = sender as Board;
			int row = (int)e.GetPosition(board).Y / board.StoneSize;
			int col = (int)e.GetPosition(board).X / board.StoneSize;
			board.NextOne(row, col);
			string s = row.ToString() + "," + col.ToString() + "," + board.StepCount.ToString() + ",";
			m_Steps.Text += s;
		}

		List<int> GetIds()
		{
			using (var db = new WeiqiContext()) {
				var result = from r in db.Records
							 select r.Id;
				return result.ToList();
			}
		}

		void M_Id_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int id = (int)m_Id.SelectedValue;
			using (var db = new WeiqiContext()) {
				var result = (from r in db.Records
							  where r.Id == id
							  select r).First();
				m_BlackName.Text = result.Black;
				m_WhiteName.Text = result.White;
				m_Result.Text = result.Result;
				m_Steps.Text = result.Steps;
				m_Type.Text = result.Type;
			}

			BtnPreview_Click(null, null);
		}

		void BtnPreview_Click(object sender, RoutedEventArgs e)
		{
			int count = m_Board.StepCount;
			for (int i = 0; i < count; i++) {
				m_Board.BackOne();
			}
			m_Board.StepCount = 0;

			var infos = GetStepInfos(m_Steps.Text);
			foreach (var info in infos) {
				if (m_Board.StepCount < info.StepCount) {
					m_Board.StepCount++;
				}
				if (m_Board.StepCount == info.StepCount)
					m_Board.NextOne(info.Row, info.Col);
			}
		}

		List<StepInfo> GetStepInfos(string stepsText)
		{
			var result = new List<StepInfo>();
			string s = stepsText;
			string[] steps = s.Substring(0, s.Length - 1).Split(',');
			StepInfo info = new StepInfo();
			for (int i = 0; i < steps.Length; i++) {
				if (i % 3 == 0) {
					int.TryParse(steps[i], out info.Row);
				} else if (i % 3 == 1) {
					int.TryParse(steps[i], out info.Col);
				} else if (i % 3 == 2) {
					int.TryParse(steps[i], out info.StepCount);
					result.Add(info);
					info = new StepInfo();
				}
			}
			return result;
		}

		void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			using (var db = new WeiqiContext()) {
				var r = db.Records.First(t => t.Id == (int)m_Id.SelectedValue);
				r.Black = m_BlackName.Text;
				r.White = m_WhiteName.Text;
				r.Steps = m_Steps.Text;
				r.Result = m_Result.Text;
				r.Type = m_Type.Text;
				db.SaveChanges();
				MessageBox.Show("Edit success!");
			}
		}
	}
}