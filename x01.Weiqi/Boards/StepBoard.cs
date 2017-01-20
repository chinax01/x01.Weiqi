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
		Record m_Record = null;
		List<StepInfo> m_StepInfos = new List<StepInfo>();
		
		public void LoadSteps()
		{
			LoadStepWindow dlg = new LoadStepWindow();
			dlg.ShowDialog();
			
			var waitWindow = new WaitWindow();
			waitWindow.Show();
			var old = m_Record;
			m_Record = dlg.Record;
			if (m_Record == null && old != null) {
				m_Record = old;
			}
			if (m_Record != null)
				ShowAll();
			waitWindow.Close();
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
			int count = m_StepInfos.Count;
			for (int i = 0; i < count; i++ ) {
				NextOne();
			}
		}

		private void InitStepInfos()
		{
			m_StepInfos.Clear();

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
