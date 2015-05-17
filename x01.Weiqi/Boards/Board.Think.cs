using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x01.Weiqi.Boards
{
	partial class Board
	{
		protected bool m_CanDown = false;
		Random m_Rand = new Random();
		readonly Pos m_InvalidPos = new Pos(-1, -1);

		public Pos Think()
		{
			// 下棋是一种由不确定到确定的过程，没有人能证明这步棋不好，那么就可以抢先确定这一步。
			if (StepCount == 0)
				return new Pos(3, 15);

			return OneEmpty() != m_InvalidPos ? OneEmpty() : RandDown();
		}

		Pos OneEmpty()
		{
			foreach (var block in m_WhiteBlocks) {	// Eat white
				if (block.EmptyCount == 1) {
					List<Step> empties = GetEmpties(block);
					return new Pos(empties[0].Row, empties[0].Col);
				}
			}
			foreach (var block in m_BlackBlocks) {	// Feel black
				if (block.EmptyCount == 1) {
					List<Step> empties = GetEmpties(block);
					return new Pos(empties[0].Row, empties[0].Col);
				}
			}
			return m_InvalidPos;
		}

		private List<Step> GetEmpties(Block block)
		{
			List<Step> empties = new List<Step>();
			foreach (var step in block.Steps) {
				empties = empties.Union(LinkSteps(step, StoneColor.Empty)).ToList();
			}
			return empties;
		}

		private Pos RandDown()
		{
			if (StepCount < 40) {
				int flag = m_Rand.Next(4) % 4;
				if (flag == 0) return Row3_4Rand();
				else if (flag == 1) return Row15_16Rand();
				else if (flag == 2) return Col3_4Rand();
				else return Col15_16Rand();
			} else if (StepCount < 100) {
				int row = m_Rand.Next(2, 18);
				int col = m_Rand.Next(2, 18);
				return new Pos(row, col);
			} else {
				int row = m_Rand.Next(0, 19);
				int col = m_Rand.Next(0, 19);
				return new Pos(row, col);
			}
		}
		Pos Row3_4Rand()
		{
			int row = m_Rand.Next(2, 4);
			int col = m_Rand.Next(2, 18);
			return new Pos(row, col);
		}
		Pos Row15_16Rand()
		{
			int row = m_Rand.Next(15, 16);
			int col = m_Rand.Next(2, 18);
			return new Pos(row, col);
		}
		Pos Col3_4Rand()
		{
			int row = m_Rand.Next(2, 18);
			int col = m_Rand.Next(2, 4);
			return new Pos(row, col);
		}
		Pos Col15_16Rand()
		{
			int row = m_Rand.Next(2, 18);
			int col = m_Rand.Next(15, 16);
			return new Pos(row, col);
		}

		#region Pos Helper

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
			switch (flag) {
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
			for (int i = 0; i < count; i++) {
				lines.Add(new Pos(i + offset, offset));
				lines.Add(new Pos(i + offset, 18 - offset));
				if (i != 0 && i != count - 1) {
					lines.Add(new Pos(offset, i + offset));
					lines.Add(new Pos(18 - offset, i + offset));
				}
			}
			return lines;
		}

		#endregion
	}
}
