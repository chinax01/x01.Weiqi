// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x01.Weiqi.Board
{
    public struct Step
    {
        public int Col;
        public int Row;
        public int Count;
    }

    public enum StoneColor
    {
        Black, White, Empty
    }

    public struct DeadStep
    {
        public int Count;
        public StoneColor Color;

        // key 为死子 Count，value 为死子 Column，Row
        public Dictionary<int, Pos> DeadInfo;
    }

    // 本欲用 Point，但 double 类型不方便
    public struct Pos
    {
        public int X;
        public int Y;

        public Pos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Pos p = (Pos)obj;

            return p.X == this.X && p.Y == this.Y;
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }

        public static bool operator ==(Pos p1, Pos p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(Pos p1, Pos p2)
        {
            return !(p1 == p2);
        }
    }

    // 每一步的棋子信息
    public struct StepInfo
    {
        public Stone Stone;
        public StoneColor Color;
        public bool IsDead;
        public int Count;
        public int Column;
        public int Row;
    }
}
