using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Boards;
using x01.Weiqi.Models;

namespace x01.Weiqi.Core
{
	public static class Helper
	{
		public readonly static Pos InvalidPos = new Pos(-1, -1);

		public static List<Pos> GetArea(Pos p1, Pos p2)
		{
			List<Pos> poses = new List<Pos>();
			int r1 = Math.Min(p1.Row, p2.Row);
			int r2 = Math.Max(p1.Row, p2.Row);
			int c1 = Math.Min(p1.Col, p2.Col);
			int c2 = Math.Max(p1.Col, p2.Col);
			for (int r = r1; r <= r2; r++) {
				for (int c = c1; c <= c2; c++) {
					var pos = new Pos(r, c);
					poses.Add(pos);
				}
			}
			return poses;
		}
		public static List<Pos> GetArea(List<Pos> poses)
		{
			var leftUp = GetLeftTopPos(poses);
			var rightDown = GetRightDownPos(poses);
			return GetArea(leftUp, rightDown);
		}
		public static Pos GetLeftTopPos(List<Pos> poses)
		{
			int row = int.MaxValue, col = int.MaxValue;
			foreach (var p in poses) {
				if (row > p.Row)
					row = p.Row;
				if (col > p.Col)
					col = p.Col;
			}
			return new Pos(row, col);
		}
		public static Pos GetRightDownPos(List<Pos> poses)
		{
			int row = int.MinValue, col = int.MinValue;
			foreach (var p in poses) {
				if (row < p.Row)
					row = p.Row;
				if (col < p.Col)
					col = p.Col;
			}
			return new Pos(row, col);
		}

		public static List<List<Pos>> GetRecord(string type) // type => 0：对局，1：布局，2：定式，3：棋型
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

		public static List<Pos> GetPoses(string s)
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

		public static bool InRange(int row, int col)
		{
			if (row >= 0 && row < 19 && col >= 0 && col < 19) {
				return true;
			}
			return false;
		}
	}
}
