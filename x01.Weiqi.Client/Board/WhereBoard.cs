// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using x01.Weiqi.WhereHelper;

namespace x01.Weiqi.Board
{
    class WhereBoard : BoardBase
    {
        Timer m_Timer = new Timer();
        bool m_CanDown = true;

        public WhereBoard(int size = 38)
            : base(size)
        {
            m_Timer.Interval = 100;
            m_Timer.Tick += new EventHandler(Timer_Tick);
            m_Timer.Start();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (m_CanDown)
            {
                return;
            }

            Pos pos = PosSearch.Think();
            if (pos == new Pos(-1, -1))
            {
                return;
            }

            int col = pos.X;
            int row = pos.Y;

            if (NotIn(col, row))
            {
                return;
            }

            DrawChess(col, row);
            Eat(col, row);

            m_CanDown = true;
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (!m_CanDown)
            {
                return;
            }

            int col = (int)e.GetPosition(this).X / ChessSize;
            int row = (int)e.GetPosition(this).Y / ChessSize;

            if (NotIn(col, row))
            {
                return;
            }

            DrawChess(col, row);
            Eat(col, row);

            m_CanDown = false;

            PosSearch.CurrentPos = new Pos(col, row); // black
            PosSearch.StepCount = this.StepCount;
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            BackOne();
        }
    }
}
