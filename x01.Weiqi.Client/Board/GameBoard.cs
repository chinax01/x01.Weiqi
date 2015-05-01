// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x01.Weiqi.Board
{
    public class GameBoard : BoardBase
    {
        public GameBoard(int size = 38)
            : base(size)
        {
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            int col = (int)e.GetPosition(this).X / ChessSize;
            int row = (int)e.GetPosition(this).Y / ChessSize;

            if (NotIn(col,row))
            {
                return;
            }

            DrawChess(col, row);

            Eat(col, row);
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            BackOne();
        }
    }
}
