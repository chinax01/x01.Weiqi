using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace x01.Weiqi.Boards
{
	class AiBoard : Board
	{
		Timer m_Timer = new Timer();

		public AiBoard()
		{
			m_Timer.Interval = 100;
			m_Timer.Tick += m_Timer_Tick;
			m_Timer.Start();
		}

		void m_Timer_Tick(object sender, EventArgs e)
		{
			if (m_CanDown) return;
			Pos pos = Think();
			if (NextOne(pos.Row, pos.Col))
				m_CanDown = true;
		}

		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			if (!m_CanDown) return;
			int row = (int)e.GetPosition(this).Y / StoneSize;
			int col = (int)e.GetPosition(this).X / StoneSize;
			if (NextOne(row, col))
				m_CanDown = false;
		}

		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			BackOne();
			BackOne();
		}
	}
}
