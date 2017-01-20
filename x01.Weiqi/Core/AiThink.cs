using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Boards;

namespace x01.Weiqi.Core
{
	public class AiThink : IThink
	{
		protected Board m_Board;
		public AiThink(Board board)
		{
			m_Board = board;
		}

		List<List<Pos>> m_Shapes = null;
		public List<List<Pos>> GetShapes(string type)
		{
			if (m_Shapes != null) return m_Shapes;
			m_Shapes = new List<List<Pos>>();
			var shapes = Helper.GetRecord(type);
			foreach (var shape in shapes) {
				var temp = Helper.ExchangeAll(shape);
				m_Shapes.AddRange(temp);
			}
			return m_Shapes;
		}

		public virtual List<Pos> Think(string type)
		{
			List<Pos> result = new List<Pos>();
			
			var shapes = GetShapes(type);
			var all = m_Board.BlackPoses.Union(m_Board.WhitePoses).Union(m_Board.DeadPoses);

//			var rounds = all.Intersect(Helper.RoundPoses(m_Board.CurrentPos, 5));
//			foreach (var shape in shapes) {
//				var area = rounds.OrderBy(r => r.StepCount).ToList();
//				int areaCount = area.Count;
//				int shapeCound = shape.Count;
//				if (shapeCound >= areaCount && areaCount > 0) {
//					for (int i = 0; i < shapeCound; i++) {
//						if (i > areaCount - 1) {
//							if (shape[i].StoneColor == StoneColor.Black) {
//								if (m_Board.EmptyPoses.Contains(shape[i]) && !result.Contains(shape[i]))
//									//return shape[i];
//									result.Add(shape[i]);
//							}
//							break;
//						}
//
//						if (area[i] == shape[i] && area[i].StoneColor == shape[i].StoneColor) {
//							continue;
//						} else {
//							break;
//						}
//					}
//				}
//			}

			foreach (var shape in shapes) {
				var area = all.Intersect(Helper.GetArea(shape)).OrderBy(p => p.StepCount).ToList();
				int areaCount = area.Count;
				int shapeCound = shape.Count;
				if (shapeCound >= areaCount && areaCount > 0) {
					for (int i = 0; i < shapeCound; i++) {
						if (i > areaCount - 1) {
							if (shape[i].StoneColor == StoneColor.Black) {
								if (m_Board.EmptyPoses.Contains(shape[i]) && !result.Contains(shape[i]))
									//return shape[i];
									result.Add(shape[i]);
							}
							break;
						}

						if (area[i] == shape[i] && area[i].StoneColor == shape[i].StoneColor) {
							continue;
						} else {
							break;
						}
					}
				}
			}
			
			return result;
		}
	}
}
