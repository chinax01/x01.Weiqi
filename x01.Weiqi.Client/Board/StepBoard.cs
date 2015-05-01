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
        List<StepContent> m_StepContent = new List<StepContent>();
        int m_count = 1;

        public StepBoard(int chessSize = 38)
            : base(chessSize)
        {
			InitData();
        }

		public void InitData()
		{
			SettingsWindow settingsWin = new SettingsWindow();
			settingsWin.ShowDialog();
			int id = settingsWin.ID;
			GetStepContent(id);
		}

        public void GetStepContent(int id)
        {
            string steps = StepService.GetSteps(id);
            if (string.IsNullOrEmpty(steps))
            {
                MessageBox.Show("Cannot get step content!");
                return;
            }

            m_StepContent.Clear();
            StepContent stepContent = new StepContent();
            string[] step = steps.Split(',');
            for (int i = 0; i < step.Length; i++)
            {
                if (i % 3 == 0)
                {
                    stepContent = new StepContent();
                    stepContent.Col = Convert.ToInt32(step[i]);
                }
                else if (i % 3 == 1)
                {
                    stepContent.Row = Convert.ToInt32(step[i]);
                }
                else if (i % 3 == 2)
                {
                    stepContent.Count = Convert.ToInt32(step[i]);
                    m_StepContent.Add(stepContent);
                }
            }
        }

        public void NextOneStep()
        {
            if (m_count >= m_StepContent.Count)
            {
                return;
            }

            foreach (var item in m_StepContent)
            {
                if (item.Count == m_count)
                {
                    int col = item.Col;
                    int row = item.Row;

                    if (Steps[col, row].Color != ChessColor.Empty)
                    {
                        return;
                    }

                    if (NotInPos.X == col && NotInPos.Y == row)
                    {
                        return;
                    }
                    else
                    {
                        // If Pos(struct) is property, must use new.
                        NotInPos = new Pos(-1, -1);
                    }

                    DrawChess(col, row);

                    Eat(col, row);
                }
            }
            m_count++;
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            NextOneStep();
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (m_count <= 1)
            {
                return;
            }

            BackOne();
            m_count--;
        }
    }
}
