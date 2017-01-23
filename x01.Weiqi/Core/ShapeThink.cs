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
			CurrentPos = Helper.InvalidPos;
		}
		
		public Pos CurrentPos { get; set; }
		
		// type == R.Game
		public override List<Pos> Think(string type)
		{
			var result = new List<Pos>();
			var shapes = GetShapes(type);
			var all = m_Board.BlackPoses.Union(m_Board.WhitePoses).Union(m_Board.DeadPoses).ToList();
			
			if (CurrentPos == Helper.InvalidPos)
				CurrentPos = m_Board.CurrentPos;
			var rounds = Helper.RoundPoses(CurrentPos, 3);
			
			foreach (var shape in shapes) {
				var area = all.Intersect(rounds).OrderBy(p => p.StepCount).ToList();
				var shapeArea = shape.Intersect(rounds).OrderBy(p=>p.StepCount).ToList();
				int areaCount = area.Count;
				int shapeCound = shapeArea.Count;
				if (shapeCound >= areaCount && areaCount > 0) {
					for (int i = 0; i < shapeCound; i++) {
						if (i > areaCount - 1) {
							if (shapeArea[i].StoneColor == StoneColor.Black) {
								if (m_Board.EmptyPoses.Contains(shapeArea[i]) && !result.Contains(shapeArea[i]))
									result.Add(shape[i]);
							}
							break;
						}

						if (area[i] == shapeArea[i] && area[i].StoneColor == shapeArea[i].StoneColor) {
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