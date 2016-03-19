using System;
using System.Collections.Generic;

namespace x01.Weiqi.Boards
{
	public static class R
	{
		// 棋谱类型
		public const string Game = "对局";
		public const string Layout = "布局";
		public const string Pattern = "定式";
		public const string Shape = "棋型";
	}

	public enum StoneColor
	{
		Empty, Black, White, All
	}

	public enum Directions
	{
		Up, Down, Left, Right
	}

	class Step
	{
		public Step()
		{
			Row = Col = StepCount = BlockId = DeadStepCount = EmptyCount = -1;
			StoneColor = Boards.StoneColor.Empty;
		}

		public int Row { get; set; }
		public int Col { get; set; }
		public int StepCount { get; set; }
		public int BlockId { get; set; }
		public int DeadStepCount { get; set; }
		public StoneColor StoneColor { get; set; }
		public int EmptyCount { get; set; }
	}

	struct StepInfo
	{
		public int Row, Col, StepCount;
	}

	class StepBlock
	{
		public StepBlock()
		{
			Steps = new List<Step>();
			Id = EmptyCount = -1;
		}

		public int Id { get; set; }
		public List<Step> Steps { get; set; }
		public int EmptyCount { get; set; }

		public void UpdateBlockId()
		{
			int blockId = -1;

			foreach (var item in Steps) {
				if (blockId < item.StepCount)
					blockId = item.StepCount;
			}

			foreach (var item in Steps) {
				item.BlockId = blockId;
			}

			Id = blockId;
		}
	}

	// 本欲用 Point，但 double 类型不方便
	public struct Pos
	{
		public int Row;
		public int Col;
		public int StepCount;
		// 当有多个点可供选择时，其价值：Worth 是选择的依据。
		// 保存棋谱当添加每步棋的价值：Worth，似乎又到了重构的时候了。
		public int Worth;
		public StoneColor StoneColor;

		public Pos(int row, int col, StoneColor color = Boards.StoneColor.Empty, int stepCount = -1, int worth = -1)
		{
			Row = row;
			Col = col;
			StepCount = stepCount;
			Worth = worth;
			StoneColor = color;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType()) {
				return false;
			}

			Pos p = (Pos)obj;

			return p.Col == this.Col && p.Row == this.Row;
		}

		public override int GetHashCode()
		{
			return Col.GetHashCode() ^ Row.GetHashCode() * 83;
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

	struct Vector
	{
		public int Row;
		public int Col;

		public Vector(int row, int col)
		{
			Row = row;
			Col = col;
		}

		public double Length
		{
			get { return Math.Round(Math.Sqrt(Row * Row + Col * Col), 2); }
		}
	}

	class PosBlock
	{
		public PosBlock()
		{
			Poses = new List<Pos>();
			IsDead = false;
			EmptyCount = -1;
			KeyPos = new Pos(-1, -1);
		}
		public List<Pos> Poses { get; set; }
		public bool IsDead { get; set; }
		public Pos KeyPos { get; set; }
		public int EmptyCount { get; set; }
	}
}
