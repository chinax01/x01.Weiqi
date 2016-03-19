/**
 * Board.Layout.cs (c) 2015 by x01
 * -------------------------------
 * 1.有了定式打基础，布局也就顺理成章了。基本上是一个路数，
 *   只是第一步需调整到（3，3）位，方便变换而已。
 * 2.暂只考虑二连星。任他多路来，我只一路去，这相当合理。
 * 3.将数据库中保存的棋谱导出添加到 Layouts 中，对局足够多足够好时，
 *   棋力大进是可以预期的。
 * 4.本只准备保存布局开头的几步棋，但忽然发现保存整盘棋是有意义的，尤其对付
 *   gnu-go 时，因为 gnu-go 对相同棋形的反应也相同，这就为战胜它提供了
 *   可能。以二连星对二连星为例，保存 200 到 2000 黑先胜 gnu-go 的棋谱
 *   到数据库中，然后以此来对付 gnu-go，当有取胜的可能。此猜想并没验证，
 *   因为即使 200 到 2000 局棋，也需约 200 到 2000 小时方可完成。很显然，
 *   没有那么多时间去验证其可行性。
 * 5.棋形处理较弱，而不按定式布局走棋时，又不得不倚靠棋形处理，
 *   看来，棋形处理才是真正的关键。而要处理好棋形，不仅仅是棋形的问题，
 *   还有大小，死活，全局相关等诸多问题需要解决。
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Models;

namespace x01.Weiqi.Boards
{
	public static class RecordType
	{
		public const string Game = "对局";
		public const string Layout = "布局";
		public const string Pattern = "布局";
		public const string Shape = "布局";
	}

	public partial class Board
	{
		Pos L_GetPos()
		{
			// 不能不说这是从全局考虑了。
			return GetPos_FromLayouts(Layouts);
		}

		List<List<Pos>> m_Layouts = null;
		List<List<Pos>> Layouts
		{
			get
			{
				if (m_Layouts != null) return m_Layouts;
				m_Layouts = new List<List<Pos>>();

				// 重新开始，方可生效。
				var layouts = GetRecord(RecordType.Layout);

				m_Layouts = m_Layouts.Union(LeftLayouts(layouts))
					.Union(UpLayouts(layouts)).ToList();

				return m_Layouts;
			}
		}

		List<List<Pos>> GetRecord(string type) // type => 0：对局，1：布局，2：定式，3：棋型
		{
			var record = new List<List<Pos>>();
			using (var db = new WeiqiContext()) {
				var result = from r in db.Records
							 where r.Type == type
							 select r.Steps;
				foreach (string s in result) {
					var poses = GetPoses(s);
					if (!record.Contains(poses))
						record.Add(poses);
				}
			}
			return record;
		}
		List<Pos> GetPoses(string s)
		{
			List<Pos> result = new List<Pos>();

			s = s.Substring(0, s.Length - 1);
			string[] steps = s.Split(',');
			int count = steps.Count();
			int row = -1, col = -1, step_count;
			for (int i = 0; i < count; i++) {
				if (i % 3 == 0 && int.TryParse(steps[i], out row)) {
				} else if (i % 3 == 1 && int.TryParse(steps[i], out col)) {
				} else if (i % 3 == 2 && int.TryParse(steps[i], out step_count)) {
					if (InRange(row, col)) {
						var pos = new Pos(row, col, step_count % 2 == 0 ? StoneColor.Black : StoneColor.White, step_count);
							result.Add(pos);
					}
				}
			}

			return result;
		}

		#region Layout Helper

		List<Pos> GetLeftLayout(List<Pos> poses)
		{
			//if (!m_IsChangeStoneColor) return poses;

			var temp = new List<Pos>();
			int count = 0;
			foreach (var p in poses) {
				temp.Add(new Pos(p.Row, p.Col, count++ % 2 == 0 ? StoneColor.Black : StoneColor.White));
			}
			return temp;
		}
		List<Pos> GetUpLayout(List<Pos> poses)
		{
			//if (!m_IsChangeStoneColor) return poses;

			var temp = new List<Pos>();
			int count = 0;
			foreach (var p in poses) {
				temp.Add(new Pos(p.Col, p.Row, count++ % 2 == 0 ? StoneColor.Black : StoneColor.White));
			}
			return temp;
		}

		List<List<Pos>> LeftLayouts(List<List<Pos>> layouts)
		{
			var result = new List<List<Pos>>();
			foreach (var l in layouts) {
				result.Add(GetLeftLayout(l));
			}
			return result;
		}
		List<List<Pos>> UpLayouts(List<List<Pos>> layouts)
		{
			var result = new List<List<Pos>>();
			foreach (var l in layouts) {
				result.Add(GetUpLayout(l));
			}
			return result;
		}

		Pos GetPos_FromLayouts(List<List<Pos>> layouts)
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
			foreach (List<Pos> layout in layouts) {
				var poses = all.OrderBy(p=>p.StepCount).ToList();
				int posCount = poses.Count;
				int count = layout.Count;
				if (count >= posCount && posCount > 0) {
					var firstColor = GetStep(poses[0]).StoneColor;
					var otherColor = firstColor == StoneColor.Black ? StoneColor.White : StoneColor.Black;
					for (int i = 0; i < count; i++) {
						if (i > posCount - 1) {
							if (layout[i].StoneColor == StoneColor.Black)
								return layout[i];
							break;
						}
						if (poses[i] == layout[i] && poses[i].StoneColor == layout[i].StoneColor) {
							continue;
						} else {
							break;
						}
					}
				}
			}

			return m_InvalidPos;
		}

		#endregion

	}
}

