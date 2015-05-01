// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
namespace x01.Weiqi.Board
{
    interface IBoard
    {
        int ChessSize { get; set; }

        void DrawChess(int col, int row);
        void Eat(int col, int row);
        void BackOne();
        void Save();
    }
}
