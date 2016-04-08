using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Boards;

namespace x01.Weiqi.Core
{
	public class ShapeThink : AiThink
	{
		public ShapeThink(Board board) : base(board)
		{
		}

		// type: shape => 以 （9，9） 为起点保存数据
		public override Pos Think(string type)
		{
			var shapes = GetShapes(type);
			var all = m_Board.BlackPoses.Union(m_Board.WhitePoses).Union(m_Board.DeadPoses).ToList();
			var rounds = all.Intersect(Helper.RoundPoses(m_Board.CurrentPos, 6)).ToList();
			var back = all.ToList();
			var temp = new List<Pos>();

			foreach (var round in rounds) {
				temp.Clear();
				int r = 9 - round.Row;
				int c = 9 - round.Col;
				foreach (var b in back) {
					if (!Helper.InRange(b.Row + r, b.Col + c)) continue;
					var pos = new Pos(b.Row + r, b.Col + c, b.StoneColor, b.StepCount);
					temp.Add(pos);
				}
				foreach (var shape in shapes) {
					var intersect = temp.Intersect(Helper.GetArea(shape)).OrderBy(p => p.StepCount).ToList();
					var shapeOrder = shape.OrderBy(p => p.StepCount).ToList();
					var count = intersect.Count;
					var shapeCount = shapeOrder.Count;
					if (shapeCount >= count && count > 0) {
						for (int i = 0; i < shapeCount; i++) {
							if (i >= 1 && shapeOrder[i - 1].StoneColor == shapeOrder[i].StoneColor)
								continue;

							var p = shapeOrder[i];
							if (i > count - 1) {
								if (p.StoneColor == StoneColor.Black) {
									var e = new Pos(p.Row - r, p.Col - c);
									if (m_Board.EmptyPoses.Contains(e))
										return e;
								}
								break;
							}
							if (intersect[i] == p && intersect[i].StoneColor == p.StoneColor) {
								continue;
							} else {
								break;
							}
						}
					}
				}
			}

			foreach (var a in all) {
				temp.Clear();
				int r = 9 - a.Row;
				int c = 9 - a.Col;
				foreach (var b in back) {
					if (!Helper.InRange(b.Row + r, b.Col + c)) continue;
					var pos = new Pos(b.Row + r, b.Col + c, b.StoneColor, b.StepCount);
					if (!temp.Contains(pos)) temp.Add(pos);
				}
				foreach (var shape in shapes) {
					var intersect = temp.Intersect(Helper.GetArea(shape)).OrderBy(p => p.StepCount).ToList();
					var shapeOrder = shape.OrderBy(p => p.StepCount).ToList();
					var count = intersect.Count;
					var shapeCount = shapeOrder.Count;
					if (shapeCount >= count && count > 0) {
						for (int i = 0; i < shapeCount; i++) {
							if (shapeOrder[0].StoneColor == StoneColor.White || shapeOrder[1].StoneColor == StoneColor.Black)
								continue;

							var p = shapeOrder[i];
							if (i > count - 1) {
								if (p.StoneColor == StoneColor.Black) {
									var e = new Pos(p.Row - r, p.Col - c);
									if (m_Board.EmptyPoses.Contains(e))
										return e;
								}
								break;
							}
							if (intersect[i] == p && intersect[i].StoneColor == p.StoneColor) {
								continue;
							} else {
								break;
							}
						}
					}
				}
			}

			return Helper.InvalidPos;
		}
	}
}