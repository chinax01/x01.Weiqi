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
using x01.Weiqi.Core;
using x01.Weiqi.Models;

namespace x01.Weiqi.Boards
{
	public partial class Board
	{
		List<Pos> L_Think()
		{
			// 不能不说这是从全局考虑了。
			return L_GetPoses(Layouts);
		}

		List<Pos> m_LayoutPoses = new List<Pos>();

		List<List<Pos>> m_Layouts = null;
		List<List<Pos>> Layouts
		{
			get
			{
				if (m_Layouts != null) return m_Layouts;
				m_Layouts = new List<List<Pos>>();

				// 重新开始，方可生效。
				var layouts = Helper.GetRecord(R.Layout);

				m_Layouts = m_Layouts.Union(LeftLayouts(layouts))
					.Union(UpLayouts(layouts)).ToList();

				return m_Layouts;
			}
		}

		//public List<List<Pos>> GetRecord(string type) // type => 0：对局，1：布局，2：定式，3：棋型
		//{
		//	var record = new List<List<Pos>>();
		//	using (var db = new WeiqiContext()) {
		//		var result = from r in db.Records
		//					 where r.Type == type
		//					 select r.Steps;
		//		foreach (string s in result) {
		//			var poses = GetPoses(s);
		//			if (!record.Contains(poses))
		//				record.Add(poses);
		//		}
		//	}
		//	return record;
		//}



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

		List<Pos> L_GetPoses(List<List<Pos>> layouts)
		{
			m_LayoutPoses.Clear();

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
								//return layout[i];
								m_LayoutPoses.Add(layout[i]);
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

			return m_LayoutPoses;
		}

		#endregion

	}
}

