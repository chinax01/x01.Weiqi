using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x01.Weiqi.Boards
{
	class Step
	{
		public Step()
		{
			Row = Col = StepCount = BlockId = -1;
			StoneColor = Boards.StoneColor.Empty;
		}

		public int Row { get; set; }
		public int Col { get; set; }
		public int StepCount { get; set; }
		public int BlockId { get; set; }
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
		}

		public List<Step> Steps { get; set; }
		public int EmptyCount { get; set; }
	}
}
