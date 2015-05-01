// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Board;

namespace x01.Weiqi.WhereHelper
{
    struct Vector
    {
        int m_X;
        int m_Y;

        public Vector(int x, int y)
        {
            m_X = x;
            m_Y = y;
        }

        public double Length
        {
            get { return Math.Round(Math.Sqrt(m_X * m_X + m_Y * m_Y), 2); }
        }
    }

    static class PosListExtensions
    {
        public static List<Pos> AddDifferentPos(this List<Pos> self, List<Pos> other)
        {
            List<Pos> result = new List<Pos>();
            foreach (var item in self)
            {
                result.Add(item);
            }

            foreach (var item in other)
            {
                if (!result.Contains(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static List<Pos> RemoveSamePos(this List<Pos> self, List<Pos> other)
        {
            List<Pos> result = new List<Pos>();
            foreach (var item in self)
            {
                result.Add(item);
            }

            foreach (var p in self)
            {
                if (other.Contains(p))
                {
                    result.Remove(p);
                }
            }

            return result;
        }

        public static int CountSamePos(this List<Pos> self, List<Pos> other)
        {
            int count = 0;
            foreach (var item in other)
            {
                if (self.Contains(item))
                {
                    count++;
                }
            }
            return count;
        }

        public static List<Pos> GetSamePos(this List<Pos> self, List<Pos> other)
        {
            List<Pos> result = new List<Pos>();
            foreach (var item in other)
            {
                if (self.Contains(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static bool Include(this List<Pos> self, List<Pos> other)
        {
            foreach (var item in other)
            {
                if (!self.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }

    }

}
