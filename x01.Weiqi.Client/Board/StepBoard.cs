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
		public StepBoard(int chessSize = 38, bool isSizeChanged = false)
			: base(chessSize)
		{
			if (!isSizeChanged)
				InitData();
		}

		public void InitData()
		{
			GetSteps();
			FillSteps();
		}

		public int StepId { get; set; }
		private void GetSteps()
		{
			LoadStepWindow settingsWin = new LoadStepWindow();
			settingsWin.ShowDialog();
			StepId = settingsWin.StepId;
			ContentString = new StringBuilder(StepService.GetContent(StepId));
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
