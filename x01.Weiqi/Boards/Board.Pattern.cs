/**
 * Board.Pattern.cs (c) 2015 by x01
 * --------------------------------
 * 1.虽说记住定式，棋弱三分。但无论如何，每局棋都是从定式开始的。
 * 2.由于定式手数较多，Shape 中的方式将不再适用，故决定采用对比法，
 *   即先将定式的步数记录，然后对比之。
 * 3.定式的记录，采用四角八变换的办法，先记左上角，上方挂的变化，再
 *   变换之。
 * 4.定式太多，且在演进中，故只能择其要记录，且尚须不断修改增补。
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace x01.Weiqi.Boards
{
	/// <summary>
	/// Description of Board.Pattern.
	/// </summary>
	public partial class Board
	{
		Pos P_GetPos()
		{
			// 全局配合仍然缺乏！
			return GetPos_FromPatterns(Patterns);
		}

		List<List<Pos>> m_Patterns = null;
		List<List<Pos>> Patterns
		{
			get
			{
				if (m_Patterns != null) return m_Patterns;
				m_Patterns = new List<List<Pos>>();

				var patterns = GetRecord(RecordType.Pattern);

				m_Patterns = m_Patterns.Union(LeftUpPatterns(patterns))
					.Union(RightUpPatterns(patterns))
					.Union(LeftDownPatterns(patterns))
					.Union(RightDownPatterns(patterns)).ToList();

				return m_Patterns;
			}
		}

		#region 四角八变换

		List<Pos> Lu_Up(List<Pos> poses, bool firstIsBlack = true)	// lu：左上； Up: 上挂
		{
			return GetPattern(poses, firstIsBlack);
		}
		List<Pos> Lu_Left(List<Pos> poses, bool firstIsBlack = true)	// 左上，左挂
		{
			var result = new List<Pos>();
			var temp = Lu_Up(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(p.Col, p.Row, p.StoneColor, p.StepCount));
			}
			return result;
		}
		List<Pos> Ru_Up(List<Pos> poses, bool firstIsBlack = true)	// NewCol = 9 + (9 - OldCol);    列变行不变
		{
			var result = new List<Pos>();
			var temp = Lu_Up(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(p.Row, 18 - p.Col, p.StoneColor, p.StepCount));
			}
			return result;
		}
		List<Pos> Ru_Right(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Lu_Left(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(p.Row, 18 - p.Col, p.StoneColor, p.StepCount));
			}
			return result;
		}
		List<Pos> Ld_Down(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Lu_Up(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(18 - p.Row, p.Col, p.StoneColor, p.StepCount));
			}
			return result;
		}
		List<Pos> Ld_Left(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Lu_Left(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(18 - p.Row, p.Col, p.StoneColor, p.StepCount));
			}
			return result;
		}
		List<Pos> Rd_Down(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Ld_Down(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(p.Row, 18 - p.Col, p.StoneColor, p.StepCount));
			}
			return result;
		}
		List<Pos> Rd_Right(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Ru_Right(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(18 - p.Row, p.Col, p.StoneColor, p.StepCount));
			}
			return result;
		}
		private List<Pos> GetPattern(List<Pos> poses, bool firstIsBlack = true)
		{
			if (firstIsBlack) {
				return poses;
			} else {
				var t = new List<Pos>();
				foreach (var p in poses) {
					var pos = new Pos(p.Row, p.Col, 
						p.StoneColor == StoneColor.Black ? StoneColor.White : StoneColor.Black, p.StepCount);
					t.Add(pos);
				}
				return t;
			}

			//var firstColor = firstIsBlack ? StoneColor.Black : StoneColor.White;
			//var otherColor = firstColor == StoneColor.Black ? StoneColor.White : StoneColor.Black;
			//int count = 0;
			//var temp = new List<Pos>();
			//foreach (var p in poses) {
			//	var color = count++ % 2 == 0 ? firstColor : otherColor;
			//	var t = new Pos(p.Row, p.Col, color);
			//	temp.Add(t);
			//}
			//return temp;
		}

		List<List<Pos>> LeftUpPatterns(List<List<Pos>> patterns)
		{
			List<List<Pos>> result = new List<List<Pos>>();
			foreach (var pattern in patterns) {
				result.Add(Lu_Left(pattern));
				result.Add(Lu_Left(pattern, false));
				result.Add(Lu_Up(pattern));
				result.Add(Lu_Up(pattern, false));
			}
			return result;
		}
		List<List<Pos>> RightUpPatterns(List<List<Pos>> patterns)
		{
			var result = new List<List<Pos>>();
			foreach (var pattern in patterns) {
				result.Add(Ru_Right(pattern));
				result.Add(Ru_Right(pattern, false));
				result.Add(Ru_Up(pattern));
				result.Add(Ru_Up(pattern, false));
			}
			return result;
		}
		List<List<Pos>> LeftDownPatterns(List<List<Pos>> patterns)
		{
			var result = new List<List<Pos>>();
			foreach (var pattern in patterns) {
				result.Add(Ld_Down(pattern));
				result.Add(Ld_Down(pattern, false));
				result.Add(Ld_Left(pattern));
				result.Add(Ld_Left(pattern, false));
			}
			return result;
		}
		List<List<Pos>> RightDownPatterns(List<List<Pos>> patterns)
		{
			var result = new List<List<Pos>>();
			foreach (var pattern in patterns) {
				result.Add(Rd_Down(pattern));
				result.Add(Rd_Down(pattern, false));
				result.Add(Rd_Right(pattern));
				result.Add(Rd_Right(pattern, false));
			}
			return result;
		}

		Pos GetPos_FromPatterns(List<List<Pos>> patterns)
		{
			var deads = new List<Pos>();
			foreach (var block in m_DeadBlocks) {
				List<Step> steps = block.Value.Steps;
				foreach (var step in steps) {
					var pos = GetPos(step);
					if (!deads.Contains(pos)) {
						deads.Add(pos);
					}
				}
			}

			var all = BlackPoses.Union(WhitePoses).ToList();
			foreach (var d in deads) {
				all.Add(d);
			}

			Pos ltpos, rdpos;
			//var all = BlackPoses.Union(WhitePoses).ToList();
			foreach (List<Pos> pattern in patterns) {
				ltpos = GetLeftTopPos(pattern);
				rdpos = GetRightDownPos(pattern);
				var poses = all.Intersect(GetArea(ltpos, rdpos)).OrderBy(p => p.StepCount).ToList();
				int posCount = poses.Count;
				int count = pattern.Count;
				if (count >= posCount && posCount > 0) {
					//var firstColor = GetStep(poses[0]).StoneColor;
					//var otherColor = firstColor == StoneColor.Black ? StoneColor.White : StoneColor.Black;
					for (int i = 0; i < count; i++) {
						if (i > posCount - 1) {
							if (pattern[i].StoneColor == StoneColor.Black)
								return pattern[i];
							break;
						}
						if (poses[i] == pattern[i] && poses[i].StoneColor == pattern[i].StoneColor) {
							continue;
						} else {
							break;
						}
					}
				}
			}

			return m_InvalidPos;
		}

		Pos GetLeftTopPos(List<Pos> poses)
		{
			int row = int.MaxValue, col = int.MaxValue;
			foreach (var p in poses) {
				if (row > p.Row)
					row = p.Row;
				if (col > p.Col)
					col = p.Col;
			}
			return new Pos(row - 1, col - 1);
		}
		Pos GetRightDownPos(List<Pos> poses)
		{
			int row = int.MinValue, col = int.MinValue;
			foreach (var p in poses) {
				if (row < p.Row)
					row = p.Row;
				if (col < p.Col)
					col = p.Col;
			}
			return new Pos(row + 1, col + 1);
		}

		#endregion

	}
}