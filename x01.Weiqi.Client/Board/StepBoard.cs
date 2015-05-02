// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace x01.Weiqi.Board
{
	class StepBoard : BoardBase
	{
		public StepBoard(int chessSize = 38)
			: base(chessSize)
		{
			InitData();
		}

		public void InitData()
		{
			GetSteps();
			FillSteps();
		}

		private void GetSteps()
		{
			LoadStepWindow settingsWin = new LoadStepWindow();
			settingsWin.ShowDialog();
			int id = settingsWin.StepId;
			ContentString = new StringBuilder(StepService.GetContent(id));
		}

		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			NextOne();
		}

		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			BackOne();
		}
	}
}
