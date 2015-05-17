using System.Collections.Generic;

namespace x01.Weiqi.Boards
{
	class Step
	{
		public Step()
		{
			Row = Col = StepCount = BlockId = DeadStepCount = -1;
			StoneColor = Boards.StoneColor.Empty;
		}

		public int Row { get; set; }
		public int Col { get; set; }
		public int StepCount { get; set; }
		public int BlockId { get; set; }
		public int DeadStepCount { get; set; }
		public StoneColor StoneColor { get; set; }
	}

	enum StoneColor
	{
		Black, White, Empty, All
	}

	class Block
	{
		public Block()
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
		public int Col;
		public int Row;

		public Pos(int row, int col)
		{
			Row = row;
			Col = col;
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
			return Col ^ Row;
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
}
