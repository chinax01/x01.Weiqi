using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using x01.Weiqi.Core;
using x01.Weiqi.Windows;

namespace x01.Weiqi.Boards
{
	class AiBoard : Board
	{
		Timer m_AiTimer = new Timer();
		public Timer AiTimer { get { return m_AiTimer; } }

		bool m_CanDown = false;
		WaitWindow m_WaitWindow = new WaitWindow();

		public AiBoard()
		{
			IsShowCurrent = false;

			m_AiTimer.Interval = 100;
			m_AiTimer.Tick += m_Timer_Tick;
			m_AiTimer.Start();
		}

		void m_Timer_Tick(object sender, EventArgs e)
		{
			if (StepCount == 0) m_CanDown = false;
			if (m_CanDown) return;

			
			Pos pos = Think();

			if (pos == m_InvalidPos) {
				return;
			}

			if (NextOne(pos.Row, pos.Col)) {
				m_WaitWindow.Hide();
				m_CanDown = true;
			}  
		}

		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			if (!m_CanDown) return;
			
			int row = (int)e.GetPosition(this).Y / StoneSize;
			int col = (int)e.GetPosition(this).X / StoneSize;
			if (NextOne(row, col)) {
				m_CanDown = false;
				m_WaitWindow.Show();
			}
		}

		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			BackOne();
			BackOne();
		}
	}
}
