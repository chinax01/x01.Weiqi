using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Boards;

namespace x01.Weiqi.Core
{
	public class AiShape
	{
		List<Pos> m_ThinkPoses = new List<Pos>();
		Board m_Board;

		public AiShape(Board board)
		{
			this.m_Board = board;
		}

		public List<Pos> Think()
		{
			var all = m_Board.BlackPoses.Union(m_Board.WhitePoses).ToList();
			var back = all.ToList();
			var temp = new List<Pos>();
			m_ThinkPoses.Clear();
			if (all.Count > 64) return m_ThinkPoses;

			foreach (var a in all) {
				temp.Clear();
				int r = 9 - a.Row;
				int c = 9 - a.Col;
				foreach (var b in back) {
					var pos = new Pos(b.Row + r, b.Col + c, b.StoneColor, b.StepCount);
					if (!temp.Contains(pos)) temp.Add(pos);
				}
				foreach (var shape in Shapes) {
					var intersect = temp.Intersect(shape).OrderBy(p => p.StepCount).ToList();
					var shapeOrder = shape.OrderBy(p => p.StepCount).ToList();
					var count = intersect.Count;
					var shapeCount = shapeOrder.Count;
					if (shapeCount >= count && count > 0) {
						for (int i = 0; i < shapeCount; i++) {
							var p = shapeOrder[i];
							if (i > count - 1) {
								if (p.StoneColor == StoneColor.Black) {
									m_ThinkPoses.Add(new Pos(p.Row - r, p.Col - c)); 
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

			return m_ThinkPoses.Intersect(m_Board.EmptyPoses).ToList();
		}

		List<List<Pos>> m_Shapes = null;
		public List<List<Pos>> Shapes
		{
			get
			{
				if (m_Shapes != null) return m_Shapes;
				var temp = Helper.GetRecord(R.Shape);
				m_Shapes = GetShapes(temp);
				return m_Shapes;
			}
		}

		private List<List<Pos>> GetShapes(List<List<Pos>> shapes)
		{
			var result = new List<List<Pos>>();

			foreach (var shape in shapes) {
				if (!result.Contains(shape)) result.Add(shape);
				var c_shape = ChangeColor(shape);
				if (!result.Contains(c_shape)) result.Add(c_shape);

				var rotate = Rotate(shape);
				if (!result.Contains(rotate)) result.Add(rotate);
				var c_rotate = ChangeColor(rotate);
				if (!result.Contains(c_rotate)) result.Add(c_rotate);

				var sLR = LeftRight(shape);
				if (!result.Contains(sLR)) result.Add(sLR);
				var c_sLR = ChangeColor(sLR);
				if (!result.Contains(c_sLR)) result.Add(c_sLR);

				var rLR = LeftRight(rotate);
				if (!result.Contains(rLR)) result.Add(rLR);
				var c_rLR = ChangeColor(rLR);
				if (!result.Contains(c_rLR)) result.Add(c_rLR);

				var s_UD = UpDown(shape);
				if (!result.Contains(s_UD)) result.Add(s_UD);
				var c_s_UD = ChangeColor(s_UD);
				if (!result.Contains(c_s_UD)) result.Add(c_s_UD);

				var r_UD = UpDown(rotate);
				if (!result.Contains(r_UD)) result.Add(r_UD);
				var c_r_UD = ChangeColor(r_UD);
				if (!result.Contains(c_r_UD)) result.Add(c_r_UD);

				var sLR_UD = UpDown(sLR);
				if (!result.Contains(sLR_UD)) result.Add(sLR_UD);
				var c_sLR_UD = ChangeColor(sLR_UD);
				if (!result.Contains(c_sLR_UD)) result.Add(c_sLR_UD);

				var rLR_UD = UpDown(rLR);
				if (!result.Contains(rLR_UD)) result.Add(rLR_UD);
				var c_rLR_UD = ChangeColor(rLR_UD);
				if (!result.Contains(c_rLR_UD)) result.Add(c_rLR_UD);
			}

			return result;
		}

		private List<Pos> Rotate(List<Pos> shape)
		{
			List<Pos> result = new List<Pos>();
			foreach (var s in shape) {
				var pos = new Pos(s.Col, s.Row, s.StoneColor, s.StepCount);
				//if (!result.Contains(pos))
					result.Add(pos);
			}
			return result;
		}

		List<Pos> LeftRight(List<Pos> shape)
		{
			List<Pos> result = new List<Pos>();
			foreach (var s in shape) {
				var pos = new Pos(s.Row, s.Col + (9 - s.Col), s.StoneColor, s.StepCount);
				//if (!result.Contains(pos))
					result.Add(pos);
			}
			return result;
		}

		List<Pos> UpDown(List<Pos> shape)
		{
			List<Pos> result = new List<Pos>();
			foreach (var s in shape) {
				var pos = new Pos(s.Row + (9 - s.Row), s.Col, s.StoneColor, s.StepCount);
				//if (!result.Contains(pos))
					result.Add(pos);
			}
			return result;
		}

		List<Pos> ChangeColor(List<Pos> shape)
		{
			var result = new List<Pos>();
			foreach (var p in shape) {
				var color = p.StoneColor == StoneColor.Black ? StoneColor.White
					: p.StoneColor == StoneColor.White ? StoneColor.Black
					: StoneColor.Empty;
				var pos = new Pos(p.Row, p.Col, color, p.StepCount);
				result.Add(pos);
			}
			return result;
		}
	}
}
