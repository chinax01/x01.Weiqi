using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Models;
using x01.Weiqi.Windows;

namespace x01.Weiqi.Boards
{
	class StepBoard : Board
	{
		struct StepInfo
		{
			public int Row, Col, StepCount;
		}

		Record m_Record = new Record();
		List<StepInfo> m_StepInfos = new List<StepInfo>();

		public StepBoard()
		{
			Loaded += StepBoard_Loaded;
		}

		void StepBoard_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			LoadStepWindow dlg = new LoadStepWindow();
			dlg.ShowDialog();
			m_Record = dlg.Record;
			ShowAll();
		}


		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			NextOne();
		}

		private void NextOne()
		{
			int count = StepCount;
			foreach (var item in m_StepInfos) {
				if (item.StepCount == count)
					NextOne(item.Row, item.Col);
			}
		}

		public void ShowAll()
		{
			InitStepInfos();

			foreach (var item in m_StepInfos) {
				if (item.StepCount == StepCount) {
					NextOne(item.Row, item.Col);
				}
			}
		}

		private void InitStepInfos()
		{
			string s = m_Record.Steps;
			string[] steps = s.Substring(0, s.Length - 1).Split(',');
			StepInfo info = new StepInfo();
			for (int i = 0; i < steps.Length; i++) {
				if (i % 3 == 0) {
					int.TryParse(steps[i], out info.Row);
				} else if (i % 3 == 1) {
					int.TryParse(steps[i], out info.Col);
				} else if (i % 3 == 2) {
					int.TryParse(steps[i], out info.StepCount);
					m_StepInfos.Add(info);
					info = new StepInfo();
				}
			}
		}

		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);

			BackOne();
		}
	}
}
