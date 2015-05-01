// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Board;
using System.Windows.Forms;

namespace x01.Weiqi.WhereHelper
{
    class PosSearch
    {
        #region Ready

        public PosSearch()
        {
            Init();
        }
        private void Init()
        {
            // for m_AllPos
            for (int i = 0; i < 19; i++)
            {
                for (int j = 0; j < 19; j++)
                {
                    m_AllPos.Add(new Pos(i, j));
                }
            }
        }

        public Pos CurrentPos { get; set; } // current black chess position

        int m_StepCount = 0;  // current black step count
        public int StepCount
        {
            get { return m_StepCount; }
            set { m_StepCount = value; }
        }

        List<Pos> m_BlackPos = new List<Pos>(); // all black pos
        public List<Pos> BlackPos
        {
            get { return m_BlackPos; }
        }
        List<Pos> m_WhitePos = new List<Pos>(); // all white pos
        public List<Pos> WhitePos
        {
            get { return m_WhitePos; }
        }
        List<Pos> m_EmptyPos = new List<Pos>(); // all empty pos
        public List<Pos> EmptyPos
        {
            get { return m_EmptyPos; }
        }
        List<Pos> m_AllPos = new List<Pos>();

        Random m_Rand = new Random();
        Pos m_InvalidPos = new Pos(-1, -1);

        List<Pos> m_LinkBlacks = new List<Pos>(); // all black link pos
        List<Pos> m_LinkWhites = new List<Pos>(); // all white link pos

        List<Pos> m_Block = new List<Pos>();

        struct BlockInfo
        {
            public int Count;
            public List<Pos> Block;
        }
        List<BlockInfo> m_BlackBlock = new List<BlockInfo>();
        List<BlockInfo> m_WhiteBlock = new List<BlockInfo>();
        List<BlockInfo> m_EmptyBlock = new List<BlockInfo>();

        //------------------------------------------------------------------------------
        // 基本约定
        // --------
        // GoldHorn: 金角
        // LineThree：3 线，LineFour：4 线, LineOne: 边线
        // FlyOne：小飞，FlyTwo：大飞，FlyThree：超大飞
        // SkipOne：一间跳，SkipTwo：二间跳，SkipThree：三间跳
        // ElephantStep：象步，Cusp：尖，Touch：碰/并
        // Block: 块，Empty：空（目），Alive：活，Dead：死
        // Strong：强，Weak：弱；Attack：攻击，Defend：防守
        // Man: BlackChess (VS) WhereBoard: WhiteChess
        //------------------------------------------------------------------------------

        List<Pos> LinkPos(Pos p, bool noCuspSelf = true)
        {
            List<Pos> linkPos = new List<Pos>();

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (noCuspSelf && (i == j || i == -j))
                    {
                        continue;
                    }

                    Pos pos = new Pos(p.X + i, p.Y + j);
                    if (m_AllPos.Contains(pos))
                    {
                        linkPos.Add(pos);
                    }
                }
            }

            return linkPos;
        }
        List<Pos> EffectPos(Pos p, int x = 2, int y = 2)
        {
            List<Pos> list = new List<Pos>();
            foreach (var item in m_AllPos)
            {
                if (new Vector(item.X - p.X, item.Y - p.Y).Length <= new Vector(x, y).Length)
                {
                    list.Add(item);
                }
            }
            return list;
        }
        List<Pos> CurrentLinkPos()
        {
            return LinkPos(CurrentPos);
        }
        List<Pos> CurrentEffectBlacks()
        {
            List<Pos> effects = EffectPos(CurrentPos);
            List<Pos> temp = new List<Pos>();
            foreach (var e in effects)
            {
                if (m_BlackPos.Contains(e))
                {
                    temp.Add(e);
                }
            }
            return temp;
        }
        List<Pos> CurrentEffectWhites()
        {
            List<Pos> effects = EffectPos(CurrentPos);
            List<Pos> temp = new List<Pos>();
            foreach (var e in effects)
            {
                if (m_WhitePos.Contains(e))
                {
                    temp.Add(e);
                }
            }
            return temp;
        }
        List<Pos> CurrentEffectEmpties()
        {
            List<Pos> effects = EffectPos(CurrentPos);
            List<Pos> temp = new List<Pos>();
            foreach (var e in effects)
            {
                if (m_EmptyPos.Contains(e))
                {
                    temp.Add(e);
                }
            }
            return temp;
        }

        // + + +
        // + 0 +   RoundOne is "+" Pos List
        // + + +
        List<Pos> RoundOne(Pos pos)
        {
            List<Pos> scope = LinkPos(pos, false);
            scope.Remove(pos);
            return scope;
        }
        List<Pos> RoundTwo(Pos pos)
        {
            List<Pos> scope = EffectPos(pos, 2, 2);
            scope = scope.RemoveSamePos(EffectPos(pos, 1, 1));
            return scope;
        }
        List<Pos> RoundThree(Pos pos)
        {
            List<Pos> scope = EffectPos(pos, 3, 3);
            scope = scope.RemoveSamePos(EffectPos(pos, 2, 2));
            return scope;
        }
        List<Pos> RoundFour(Pos pos)
        {
            List<Pos> scope = EffectPos(pos, 4, 4);
            scope = scope.RemoveSamePos(EffectPos(pos, 3, 3));
            return scope;
        }
        List<Pos> RoundFive(Pos pos)
        {
            List<Pos> scope = EffectPos(pos, 5, 5);
            scope = scope.RemoveSamePos(EffectPos(pos, 4, 4));
            return scope;
        }
        List<Pos> RoundSix(Pos pos)
        {
            List<Pos> scope = EffectPos(pos, 6, 6);
            scope = scope.RemoveSamePos(EffectPos(pos, 5, 5));
            return scope;
        }

        List<Pos> AreaLeftTop()
        {
            List<Pos> area = new List<Pos>();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    area.Add(new Pos(i, j));
                }
            }
            return area;
        }
        List<Pos> AreaRightTop()
        {
            List<Pos> area = new List<Pos>();
            for (int i = 10; i < 19; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    area.Add(new Pos(i, j));
                }
            }
            return area;
        }
        List<Pos> AreaLeftBottom()
        {
            List<Pos> area = new List<Pos>();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 10; j < 19; j++)
                {
                    area.Add(new Pos(i, j));
                }
            }
            return area;
        }
        List<Pos> AreaRightBottom()
        {
            List<Pos> area = new List<Pos>();
            for (int i = 10; i < 19; i++)
            {
                for (int j = 10; j < 19; j++)
                {
                    area.Add(new Pos(i, j));
                }
            }
            return area;
        }

        List<Pos> LeftTopBalcks()
        {
            return AreaLeftTop().GetSamePos(m_BlackPos);
        }
        List<Pos> LeftTopWhites()
        {
            return AreaLeftTop().GetSamePos(m_WhitePos);
        }
        List<Pos> LeftTopeEmpties()
        {
            return AreaLeftTop().GetSamePos(m_EmptyPos);
        }

        List<Pos> RightTopBlacks()
        {
            return AreaRightTop().GetSamePos(m_BlackPos);
        }
        List<Pos> RightTopWhites()
        {
            return AreaRightTop().GetSamePos(m_WhitePos);
        }
        List<Pos> RightTopEmpties()
        {
            return AreaRightTop().GetSamePos(m_EmptyPos);
        }

        List<Pos> LeftBottomBlacks()
        {
            return AreaLeftBottom().GetSamePos(m_BlackPos);
        }
        List<Pos> LeftBottomWhites()
        {
            return AreaLeftBottom().GetSamePos(m_WhitePos);
        }
        List<Pos> LeftBottomEmpties()
        {
            return AreaLeftBottom().GetSamePos(m_EmptyPos);
        }

        List<Pos> RightBottomBlacks()
        {
            return AreaRightBottom().GetSamePos(m_BlackPos);
        }
        List<Pos> RightBottomWhites()
        {
            return AreaRightBottom().GetSamePos(m_WhitePos);
        }
        List<Pos> RightBottomEmpties()
        {
            return AreaRightBottom().GetSamePos(m_EmptyPos);
        }

        List<Pos> GoldHorn()
        {
            List<Pos> goldHorn = new List<Pos>();
            Pos pos;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if ((i + 3 == 4 && (j + 3 == 4 || j + 15 == 14))
                        || (i + 15 == 14 && (j + 3 == 4 || j + 15 == 14)))
                    {
                        continue; // remove 5.5
                    }

                    pos = new Pos(3 + i, 3 + j);
                    goldHorn.Add(pos);
                    pos = new Pos(15 + i, 15 + j);
                    goldHorn.Add(pos);
                    pos = new Pos(3 + i, 15 + j);
                    goldHorn.Add(pos);
                    pos = new Pos(15 + i, 3 + j);
                    goldHorn.Add(pos);
                }
            }
            return goldHorn;
        }

        enum LineFlag { One, Two, Three, Four, Five, Six, Seven, Eight, Nine }
        List<Pos> LineOne()
        {
            return GetLines(LineFlag.One);
        }
        List<Pos> LineTwo()
        {
            return GetLines(LineFlag.Two);
        }
        List<Pos> LineThree()
        {
            return GetLines(LineFlag.Three);
        }
        List<Pos> LineFour()
        {
            return GetLines(LineFlag.Four);
        }
        List<Pos> LineFive()
        {
            return GetLines(LineFlag.Five);
        }
        List<Pos> LineSix()
        {
            return GetLines(LineFlag.Six);
        }
        List<Pos> LineSeven()
        {
            return GetLines(LineFlag.Seven);
        }
        List<Pos> LineEight()
        {
            return GetLines(LineFlag.Eight);
        }
        List<Pos> LineNine()
        {
            return GetLines(LineFlag.Nine);
        }
        List<Pos> GetLines(LineFlag flag)
        {
            List<Pos> lines = new List<Pos>();

            int count = -1;
            switch (flag)
            {
                case LineFlag.One:
                    count = 19;
                    break;
                case LineFlag.Two:
                    count = 17;
                    break;
                case LineFlag.Three:
                    count = 15;
                    break;
                case LineFlag.Four:
                    break;
                case LineFlag.Five:
                    count = 13;
                    break;
                case LineFlag.Six:
                    count = 11;
                    break;
                case LineFlag.Seven:
                    count = 9;
                    break;
                case LineFlag.Eight:
                    count = 7;
                    break;
                case LineFlag.Nine:
                    count = 5;
                    break;
                default:
                    throw new Exception("LineFlag does not exist.");
            }

            int offset = (19 - count) / 2;
            for (int i = 0; i < count; i++)
            {
                lines.Add(new Pos(i + offset, offset));
                lines.Add(new Pos(i + offset, 18 - offset));
                if (i != 0 && i != count - 1)
                {
                    lines.Add(new Pos(offset, i + offset));
                    lines.Add(new Pos(18 - offset, i + offset));
                }
            }
            return lines;
        }

        // Nine Star
        Pos StarLeftTop()
        {
            return new Pos(3, 3);
        }
        Pos StarTop()
        {
            return new Pos(9, 3);
        }
        Pos StarRightTop()
        {
            return new Pos(15, 3);
        }
        Pos StarLeft()
        {
            return new Pos(3, 9);
        }
        Pos StarCenter()
        {
            return new Pos(9, 9);
        }
        Pos StarRight()
        {
            return new Pos(15, 9);
        }
        Pos StarLeftBottom()
        {
            return new Pos(3, 15);
        }
        Pos StarBottom()
        {
            return new Pos(9, 15);
        }
        Pos StarRightBottom()
        {
            return new Pos(15, 15);
        }
        List<Pos> StarFourHorn()
        {
            List<Pos> stars = new List<Pos>();
            stars.Add(StarLeftTop());
            stars.Add(StarRightTop());
            stars.Add(StarLeftBottom());
            stars.Add(StarRightBottom());
            return stars;

        }

        List<Pos> LineOneTwoEmpties()
        {
            return LineOne().AddDifferentPos(LineTwo()).GetSamePos(m_EmptyPos);
        }
        List<Pos> LineThreeFourEmpties()
        {
            return LineThree().AddDifferentPos(LineFour()).GetSamePos(m_EmptyPos);
        }
        List<Pos> LineCenterEmpties()
        {
            List<Pos> empties = m_EmptyPos
                .RemoveSamePos(LineOneTwoEmpties()).RemoveSamePos(LineThreeFourEmpties());
            return empties;
        }

        bool IsFlyOne(Pos p1, Pos p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y).Length == new Vector(1, 2).Length;
        }
        bool IsFlyTwo(Pos p1, Pos p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y).Length == new Vector(1, 3).Length;
        }
        bool IsFlyThree(Pos p1, Pos p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y).Length == new Vector(1, 4).Length;
        }
        bool IsSkipOne(Pos p1, Pos p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y).Length == new Vector(0, 2).Length;
        }
        bool IsSkipTwo(Pos p1, Pos p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y).Length == new Vector(0, 3).Length;
        }
        bool IsSkipThree(Pos p1, Pos p2)
        {
            return new Vector(p1.X - p2.X, p2.Y - p2.Y).Length == new Vector(0, 4).Length;
        }
        bool IsElephantStep(Pos p1, Pos p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y).Length == new Vector(2, 2).Length;
        }
        bool IsCusp(Pos p1, Pos p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y).Length == new Vector(1, 1).Length;
        }
        bool IsTouch(Pos p1, Pos p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y).Length == new Vector(0, 1).Length;
        }

        bool Unsafe()
        {
            if (EffectPos(CurrentPos).CountSamePos(CurrentEffectWhites()) > 0)
            {
                return true;
            }
            return false;
        }
        // + 0 0  Two empty is "0 0" shape
        bool HasTwoEmpty(Pos pos)
        {
            return LinkPos(pos).CountSamePos(m_EmptyPos) > 0;
        }
        //   0 
        // 0 + 0  Four link empty is four "0" shape
        //   0
        bool HasFourLinkEmpty(Pos pos)
        {
            return LinkPos(pos).CountSamePos(m_EmptyPos) == 4;
        }
        // 0   0
        //   +    Four cusp empty is four "0" shape
        // 0   0
        bool HasFourCuspEmpty(Pos pos)
        {
            return RoundOne(pos).RemoveSamePos(LinkPos(pos)).CountSamePos(m_EmptyPos) == 4;
        }
        // 0 0 0
        // 0 + 0  Eight empty is eight "0" shape
        // 0 0 0
        bool HasEightEmpty(Pos pos)
        {
            return RoundOne(pos).CountSamePos(m_EmptyPos) == 8;
        }
        #endregion

        #region Think

        public Pos Think()
        {
            return
                Attack() != m_InvalidPos ? Attack() :
                Defend() != m_InvalidPos ? Defend() :
                RandDown();
        }

        Pos Attack()
        {
            return
                EatBlack() != m_InvalidPos ? EatBlack() :
                FleeWhite() != m_InvalidPos ? FleeWhite() :
                WillEatBlack() != m_InvalidPos ? WillEatBlack() :
                m_InvalidPos;
        }
        Pos Defend()
        {
            if (Unsafe())
            {
                foreach (var item in CurrentEffectWhites())
                {
                    if (IsTouch(item, CurrentPos))
                    {
                        return Touch(item);
                    }
                }
                foreach (var item in CurrentEffectWhites())
                {
                    if (IsCusp(item, CurrentPos))
                    {
                        return Cusp(item);
                    }
                }
                foreach (var item in CurrentEffectWhites())
                {
                    if (IsFlyOne(item, CurrentPos))
                    {
                        return FlyOne(item);
                    }
                }
                foreach (var item in CurrentEffectWhites())
                {
                    if (IsSkipOne(item, CurrentPos))
                    {
                        return SkipOne(item);
                    }
                }
            }
            return m_InvalidPos;
        }
        Pos RandDown()
        {
            List<Pos> empties = new List<Pos>();
            UpdateBlackWhiteLink();

            if (m_StepCount < 6)
            {
                empties = GoldHorn().RemoveSamePos(m_LinkBlacks).RemoveSamePos(m_LinkWhites);
            }
            else if (m_StepCount < 20)
            {
                empties = LineThreeFourEmpties()
                    .RemoveSamePos(m_LinkBlacks).RemoveSamePos(m_LinkWhites);
            }
            else if (m_StepCount < 40)
            {
                empties = LineThreeFourEmpties().AddDifferentPos(LineCenterEmpties())
                    .RemoveSamePos(m_LinkWhites).RemoveSamePos(m_LinkBlacks);
            }
            else if (m_StepCount < 200)
            {
                empties = LineThreeFourEmpties().AddDifferentPos(LineCenterEmpties())
                    .RemoveSamePos(m_LinkBlacks);
            }
            else
            {
                empties = m_EmptyPos;
            }

            for (int i = 0; i < 100; i++)
            {
                if (empties.Count > 0)
                {
                    Pos pos = empties[m_Rand.Next(0, empties.Count - 1)];
                    if (!NotIn(pos))
                    {
                        return pos;
                    }
                }
            }

            return m_InvalidPos;
        }
        bool NotIn(Pos pos)
        {
            bool ok = LinkPos(pos).CountSamePos(m_WhitePos) != 4;

            List<Pos> cuspBlacks = RoundOne(pos).RemoveSamePos(LinkPos(pos)).GetSamePos(m_BlackPos);
            if (cuspBlacks.Count == 2 && IsElephantStep(cuspBlacks[0],cuspBlacks[1]))
            {
                ok = ok || true;
            }

            foreach (var item in LinkPos(pos).GetSamePos(m_WhitePos))
            {
                List<Pos> blacks = LinkPos(item).GetSamePos(m_BlackPos);
                if (blacks.Count == 2 && IsCusp(blacks[0], pos) && IsCusp(blacks[1], pos))
                {
                    ok = ok || true;
                }
                else if (blacks.Count == 3)
                {
                    ok = ok || true;
                }
            }

            if (ok && LinkPos(pos).CountSamePos(m_BlackPos) < 3)
            {
                return false;
            }
            return true;
        }

        Pos EatBlack() // 吃 
        {
            foreach (var item in m_BlackPos)
            {
                List<Pos> empties = LinkPos(item).GetSamePos(m_EmptyPos);
                if (empties.Count == 1 && LinkPos(item).CountSamePos(m_BlackPos) < 1 && !NotIn(empties[0]))
                {
                    return empties[0];
                }
            }

            List<Pos> blacks = EatBlockFilter(m_BlackPos);
            foreach (var item in blacks)
            {
                List<Pos> empties;
                Pos pos = EatBlock(item, m_BlackPos, out empties);
                if (pos != m_InvalidPos && !NotIn(pos))
                {
                    return pos;
                }
            }

            return m_InvalidPos;
        }
        // For eat block
        List<Pos> EatBlockFilter(List<Pos> scope)
        {
            List<Pos> block = new List<Pos>();
            foreach (var item in scope)
            {
                if (LinkPos(item).CountSamePos(m_EmptyPos) == 1
                    && LinkPos(item).CountSamePos(scope) > 0)
                {
                    bool isEmpty = false;
                    foreach (var again in LinkPos(item).GetSamePos(scope))
                    {
                        if (LinkPos(again).CountSamePos(m_EmptyPos) > 0)
                        {
                            isEmpty = true;
                        }
                    }
                    if (isEmpty)
                    {
                        continue;
                    }
                    block.Add(item);
                }
            }
            return block;
        }
        /// <summary>
        /// 目标棋子 pos 所在块只有一口气时的紧气位置。
        /// </summary>
        /// <param name="pos">目标棋子位置</param>
        /// <param name="scope">目标棋子所属范围，如 m_BlackPos</param>
        /// <param name="empties">目标棋子块的气（empty）位置集合</param>
        /// <returns>如只有一口气时，返回紧气位置，否则，返回 m_InvalidPos.</returns>
        Pos EatBlock(Pos pos, List<Pos> scope, out List<Pos> empties)
        {
            empties = new List<Pos>();

            m_Block.Clear();
            List<Pos> block = Block(pos, scope);
            foreach (var item in block)
            {
                if (LinkPos(item).CountSamePos(m_EmptyPos) > 0)
                {
                    empties = empties.AddDifferentPos(LinkPos(item).GetSamePos(m_EmptyPos));
                }
            }
            if (empties.Count == 1)
            {
                return empties[0];
            }

            return m_InvalidPos;
        }

        Pos FleeWhite() // 逃
        {
            List<Pos> whites = LinkPos(CurrentPos, false).GetSamePos(CurrentEffectWhites());
            foreach (var item in whites)
            {
                List<Pos> empties = LinkPos(item).GetSamePos(CurrentEffectEmpties());
                if (empties.Count == 1 && IsLevy(item, m_BlackPos))
                {
                    if (CanLevy(empties[0], m_WhitePos, m_BlackPos))
                    {
                        return m_InvalidPos;
                    }

                }
                else if (empties.Count == 1 && !NotIn(empties[0]))
                {
                    return empties[0];
                }

                //if (empties.Count == 1 && !NotIn(empties[0]) &&
                //    LinkPos(item).CountSamePos(m_WhitePos) < 1 && LinkPos(empties[0]).CountSamePos(m_EmptyPos) > 2)
                //{
                //    return empties[0];
                //}

            }

            whites = EatBlockFilter(m_WhitePos);
            foreach (var item in whites)
            {
                List<Pos> empties;
                Pos pos = EatBlock(item, m_WhitePos, out empties);
                if (!NotIn(pos) && LinkPos(pos).CountSamePos(m_EmptyPos) > 2
                    && (pos != m_InvalidPos && HasTwoEmpty(pos) || LinkPos(pos).CountSamePos(m_WhitePos) > 1))
                {
                    if (IsLevy(item, m_BlackPos) && CanLevy(pos, m_WhitePos, m_BlackPos))
                    {
                        return m_InvalidPos;
                    }
                    return pos;
                }
            }

            return m_InvalidPos;
        }
        /// <summary>
        /// 并不考虑引征：pos 为黑，则 scope 为白；pos 为白，则 scope 为黑。
        /// </summary>
        /// <param name="pos">被征之子</param>
        /// <param name="scope">征吃之子的集合</param>
        /// <returns>是否可以征。</returns>
        bool IsLevy(Pos pos, List<Pos> scope) // 征
        {
            List<Pos> empties = LinkPos(pos).GetSamePos(m_EmptyPos);
            if (empties.Count == 1)
            {
                List<Pos> list = LinkPos(empties[0]).GetSamePos(scope);
                if (list.Count == 1)
                {
                    return true;
                }
            }
            return false;
        }
        bool CanLevy(Pos empty, List<Pos> self, List<Pos> other)
        {
            List<Pos> selfPos = LinkPos(empty).GetSamePos(self);
            List<Pos> otherPos = LinkPos(empty).GetSamePos(other);
            if (selfPos.Count > 1)
            {
                return false;
            }

            if (selfPos.Count == 1 && otherPos.Count == 1)
            {
                Pos s = selfPos[0];
                Pos o = otherPos[0];
                int sx = empty.X - s.X;
                int sy = empty.Y - s.Y;
                int ox = empty.X - o.X;
                int oy = empty.Y - o.Y;
                while (true)
                {
                    if (LineOneTwoEmpties().Contains(empty))
                    {
                        break;
                    }
                    empty.X += ox;
                    empty.Y += oy;
                    if (self.Contains(empty))
                    {
                        return false;
                    }
                    else if (other.Contains(empty))
                    {
                        return true;
                    }
                    empty.X += sx;
                    empty.Y += sy;
                    if (self.Contains(empty))
                    {
                        return false;
                    }
                    else if (other.Contains(empty))
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        Pos WillEatBlack()
        {
            return
                TwoEat() != m_InvalidPos ? TwoEat() :
                LineTwoEat() != m_InvalidPos ? LineTwoEat() :
                LineOneEat() != m_InvalidPos ? LineOneEat() :
                m_InvalidPos;
        }
        Pos TwoEat() // 双叫吃 
        {
            foreach (var black in m_BlackPos)
            {
                List<Pos> empties = LinkPos(black).GetSamePos(m_EmptyPos);
                if (empties.Count == 2)
                {
                    foreach (var item in empties)
                    {
                        List<Pos> blacks = LinkPos(item).GetSamePos(m_BlackPos);
                        if (blacks.Count == 2 && IsCusp(blacks[0], blacks[1])
                            && LinkPos(blacks[0]).CountSamePos(m_EmptyPos) == 2
                            && LinkPos(blacks[1]).CountSamePos(m_EmptyPos) == 2
                            && LinkPos(blacks[0]).CountSamePos(m_BlackPos) < 1
                            && LinkPos(blacks[1]).CountSamePos(m_BlackPos) < 1)
                        {
                            return item;
                        }
                    }
                }
            }
            return m_InvalidPos;
        }
        Pos LineTwoEat() // 二线叫吃 
        {
            foreach (var item in m_BlackPos)
            {
                if (LineTwo().Contains(item) && LinkPos(item).CountSamePos(m_EmptyPos) == 2)
                {
                    foreach (var empty in LinkPos(item).GetSamePos(m_EmptyPos))
                    {
                        if (LineTwo().Contains(empty) && !NotIn(empty)
                            && LinkPos(item).CountSamePos(m_BlackPos) < 1)
                        {
                            return empty;
                        }
                    }
                }
            }

            return m_InvalidPos;
        }

        Pos LineOneEat()
        {
            foreach (var item in m_BlackPos)
            {
                if (LineOne().Contains(item) && LinkPos(item).CountSamePos(m_EmptyPos) == 2)
                {
                    foreach (var empty in LinkPos(item).GetSamePos(m_EmptyPos))
                    {
                        if (LineOne().Contains(empty) && !NotIn(empty))
                        {
                            return empty;
                        }
                    }
                }
            }

            return m_InvalidPos;
        }

        Pos Touch(Pos white) // 碰(CurrentPos Touch white)
        {
            int x = white.X - CurrentPos.X; // offset
            int y = white.Y - CurrentPos.Y;

            if (LinkPos(white).CountSamePos(m_EmptyPos) == 3 && CurrentLinkPos().CountSamePos(m_EmptyPos) == 3)
            {
                Pos offsetTwo = new Pos(white.X + x + x, white.Y + y + y);
                List<Pos> offsetPos = LinkPos(offsetTwo, false);
                if (offsetPos.CountSamePos(m_WhitePos) > 0 && offsetPos.CountSamePos(m_BlackPos) < 1)
                {
                    foreach (var item in CurrentLinkPos().GetSamePos(m_EmptyPos))
                    {
                        if (IsCusp(item, white) && !LineOneTwoEmpties().Contains(item)
                            && !NotIn(item) && HasFourCuspEmpty(item))
                        {
                            return item;
                        }
                    }
                }
                else
                {
                    foreach (var item in LinkPos(white).GetSamePos(m_EmptyPos))
                    {
                        if (IsTouch(item, white) && IsCusp(item, CurrentPos)
                            && !NotIn(item) && !LineOneTwoEmpties().Contains(item))
                        {
                            return item;
                        }
                    }
                }
            }

            // 扳立
            List<Pos> blacks = LinkPos(white).GetSamePos(m_BlackPos);
            List<Pos> empties = LinkPos(white).GetSamePos(m_EmptyPos);
            List<Pos> whites = LinkPos(white).GetSamePos(m_WhitePos);
            if (blacks.Count == 2 && IsCusp(blacks[0], blacks[1])
                && !(LineThree().AddDifferentPos(LineTwo()).AddDifferentPos(LineOne())).Contains(white))
            {
                foreach (var item in empties)
                {
                    if (IsCusp(item, CurrentPos) && !NotIn(item))
                    {
                        return item;
                    }
                }
            }

            // 断或扳
            if (blacks.Count == 2 && empties.Count == 1 && whites.Count == 1
                && IsCusp(blacks[0], blacks[1]) && IsCusp(empties[0], whites[0]))
            {
                foreach (var empty in CurrentLinkPos().GetSamePos(m_EmptyPos))
                {
                    if (IsCusp(white, empty) && !NotIn(empty))
                    {
                        return empty;
                    }
                }
            }

            // 二路连扳
            if (LineTwo().Contains(white) && LineTwo().Contains(CurrentPos) && blacks.Count == 2)
            {
                List<Pos> temp = CurrentLinkPos().GetSamePos(m_EmptyPos);
                foreach (var item in temp)
                {
                    if (LineThree().Contains(item) && !NotIn(item))
                    {
                        return item;
                    }
                }
            }

            // 挡
            foreach (var empty in CurrentLinkPos().GetSamePos(m_EmptyPos))
            {
                if (IsCusp(white, empty) && LinkPos(empty).CountSamePos(m_WhitePos) > 0 && !NotIn(empty))
                {
                    return empty;
                }
            }


            if (TurnTwo() != m_InvalidPos)
            {
                return TurnTwo();
            }

            return m_InvalidPos;
        }
        Pos TurnTwo() // 扳二子头 
        {
            List<Pos> posList = LinkPos(CurrentPos, false);
            List<Pos> whites = posList.GetSamePos(CurrentEffectWhites());
            List<Pos> blacks = posList.GetSamePos(CurrentEffectBlacks());
            List<Pos> empties = posList.GetSamePos(CurrentEffectEmpties());
            if (whites.Count == 2 && blacks.Count == 2)
            {
                if (IsTouch(whites[0], whites[1]) && IsTouch(blacks[0], blacks[1]))
                {
                    foreach (var item in empties)
                    {
                        if (!NotIn(item)
                            && (IsTouch(CurrentPos, item)
                                && (IsCusp(whites[0], item) || IsCusp(whites[1], item))))
                        {
                            if (!LineOneTwoEmpties().Contains(item))
                            {
                                return item;
                            }
                        }
                    }
                }
            }
            return m_InvalidPos;
        }

        Pos Cusp(Pos white) // 尖 
        {
            int x = CurrentPos.X - white.X; // offset
            int y = CurrentPos.Y - white.Y;

            if (HasFourLinkEmpty(white) && HasFourLinkEmpty(CurrentPos))
            {
                Pos twoX = new Pos(white.X + x + x, white.Y);
                Pos threeX = new Pos(white.X + 3 * x, white.Y);
                if (m_WhitePos.Contains(twoX) || m_WhitePos.Contains(threeX))
                {
                    return new Pos(white.X + x, white.Y);
                }

                Pos twoY = new Pos(white.X, white.Y + y + y);
                Pos threeY = new Pos(white.X, white.Y + 3 * y);
                if (m_WhitePos.Contains(twoY) || m_WhitePos.Contains(threeY))
                {
                    return new Pos(white.X, white.Y + y);
                }

                foreach (var item in CurrentLinkPos().GetSamePos(m_EmptyPos))
                {
                    if (IsTouch(item, white))
                    {
                        return item;
                    }
                }
            }

            // 长挡
            if (HasFourLinkEmpty(white) && LinkPos(CurrentPos).CountSamePos(m_BlackPos) == 1)
            {
                Pos black = LinkPos(CurrentPos).GetSamePos(m_BlackPos)[0];
                foreach (var item in LinkPos(white))
                {
                    if (IsSkipOne(item, black))
                    {
                        return item;
                    }
                }
            }

            return m_InvalidPos;
        }
        Pos SkipOne(Pos white) // 跳 
        {
            foreach (var item in RoundTwo(white))
            {
                if (IsElephantStep(item, CurrentPos) && !LineOneTwoEmpties().Contains(item)
                    && m_EmptyPos.Contains(item) && HasEightEmpty(item))
                {
                    return item;
                }
            }
            return m_InvalidPos;
        }
        Pos FlyOne(Pos white) // 飞 
        {
            if (HasEightEmpty(white) && HasFourLinkEmpty(CurrentPos))
            {
                Pos pos = m_InvalidPos;

                int x = white.X - CurrentPos.X;
                int y = white.Y - CurrentPos.Y;
                if (x == 1 || x == -1)
                {
                    pos = new Pos(CurrentPos.X + x, CurrentPos.Y);
                }
                else if (y == 1 || y == -1)
                {
                    pos = new Pos(CurrentPos.X, CurrentPos.Y + y);
                }
                if (HasTwoEmpty(pos))
                {
                    return pos;
                }
            }
            return m_InvalidPos;
        }

        #endregion

        #region Next

        // -------------------------------------------------
        // 以下为点目和形势判断而准备，但观弈城乃一步一点。
        // 如此，将不得不改动基类，这是我所不愿见的。
        // -------------------------------------------------

        // Please clear m_Block before call Block
        List<Pos> Block(Pos pos, List<Pos> scope) // passed UnitTest
        {
            if (!m_Block.Contains(pos))
            {
                m_Block.Add(pos);
            }


            List<Pos> posList = LinkPos(pos).GetSamePos(scope);
            m_Block = m_Block.AddDifferentPos(posList);
            foreach (var p in posList)
            {
                List<Pos> again = LinkPos(p).GetSamePos(scope); // 进一步
                List<Pos> different = again.RemoveSamePos(m_Block); // 防止重复
                foreach (var item in different)
                {
                    m_Block = m_Block.AddDifferentPos(Block(item, scope)); // 递归
                }
            }

            return m_Block;
        }
        void UpdateBlackWhiteLink()
        {
            List<Pos> links = new List<Pos>();

            m_LinkBlacks.Clear();
            foreach (var item in m_BlackPos)
            {
                links = LinkPos(item, false);
                m_LinkBlacks = m_LinkBlacks.AddDifferentPos(links);
            }

            m_LinkWhites.Clear();
            foreach (var item in m_WhitePos)
            {
                links = LinkPos(item, false);
                m_LinkWhites = m_LinkWhites.AddDifferentPos(links);
            }
        }
        /// <summary>
        /// Store black, white, empty block.
        /// </summary>
        /// <param name="scope">scope is m_BlackPos/m_WhitePos/m_EmptyPos copy</param>
        /// <param name="block">m_BlackBlock/m_WhiteBlock/m_EmptyBlock, please call Clear().</param>
        /// <example>
        /// use m_BlackPos Example: (result store into m_BlackBlock)
        ///    m_BlackBlock.Clear();
        ///    List&gt;Pos&lt; blacks = new List&gt;Pos&lt;();
        ///    blacks = blacks.AddDifferentPos(m_BlackPos);
        ///    UpdateBlock(blacks, m_BlackBlock);
        /// </example>
        void UpdateBlock(List<Pos> scope, List<BlockInfo> block) // passed Test().
        {
            if (scope.Count > 0)
            {
                m_Block.Clear();
                List<Pos> bblock = Block(scope[0], scope);
                int count = bblock.Count;
                BlockInfo cb = new BlockInfo();
                cb.Count = bblock.Count;
                cb.Block = bblock;
                block.Add(cb);
                scope = scope.RemoveSamePos(bblock);
                UpdateBlock(scope, block);
            }
        }
        void UpdateAllBlock()
        {
            m_BlackBlock.Clear();
            List<Pos> blacks = new List<Pos>();
            blacks = blacks.AddDifferentPos(m_BlackPos);
            UpdateBlock(blacks, m_BlackBlock);

            m_WhiteBlock.Clear();
            List<Pos> whites = new List<Pos>();
            whites = whites.AddDifferentPos(m_WhitePos);
            UpdateBlock(whites, m_WhiteBlock);

            m_EmptyBlock.Clear();
            List<Pos> empties = new List<Pos>();
            empties = empties.AddDifferentPos(m_EmptyPos);
            UpdateBlock(empties, m_EmptyBlock);
        }

        // Parameter pos is in the linethree
        List<Pos> GetLineThreeEmpties(Pos pos)
        {
            List<Pos> temp = new List<Pos>();

            List<Pos> empties = LinkPos(pos).GetSamePos(m_EmptyPos);
            temp = temp.AddDifferentPos(empties);
            foreach (var item in empties)
            {
                if (LineTwo().Contains(item))
                {
                    List<Pos> emptiesTwo = LinkPos(item, false).GetSamePos(m_EmptyPos);
                    temp = temp.AddDifferentPos(emptiesTwo);
                }
            }
            return temp;
        }
        List<Pos> GetLineFourEmpties(Pos pos)
        {
            List<Pos> temp = new List<Pos>();

            List<Pos> empties = LinkPos(pos).GetSamePos(m_EmptyPos);
            temp = temp.AddDifferentPos(empties);
            foreach (var item in empties)
            {
                if (LineThree().Contains(item))
                {
                    temp = temp.AddDifferentPos(GetLineThreeEmpties(item));
                }
            }
            return temp;
        }

        #endregion
    }
}
