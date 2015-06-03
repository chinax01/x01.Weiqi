/**
 * Board.Shape.cs (c) 2015 by x01
 * -----------------------------
 *  1.虽说没有全局观念是不可能下出好棋的，但还是决定先解决基本的棋形处理。
 *  2.全局扫描，根据相互位置进行处理，谓之 Positive().
 *  3.以当前子为出发点，根据相互位置进行处理，谓之 Passive().
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x01.Weiqi.Boards
{
	partial class Board
	{
		Pos Positive()
		{
			var blacks = BlackPoses;
			var whites = WhitePoses;
			var empties = EmptyPoses;
			var all = AllPoses.Intersect(blacks.Union(whites)).ToList();

			foreach (var a in all) {
				var rounds = RoundSixPoses(a);
				var b_rounds = rounds.Intersect(blacks).ToList();
				var w_rounds = rounds.Intersect(whites).ToList();
				var e_rounds = rounds.Intersect(empties).ToList();

				#region 连接跳飞

				foreach (var e in e_rounds) { 
					var links = LinkPoses(e);
					var b_links = links.Intersect(b_rounds).ToList();
					var w_links = links.Intersect(w_rounds).ToList();
					if (b_links.Count == 2 && w_links.Count == 1) { // jump
						var step0 = GetStep(b_links[0]);
						var step1 = GetStep(b_links[1]);
						if (step0.BlockId != step1.BlockId)
							return e;
					}
					var _rounds = RoundOnePoses(e).Intersect(blacks).ToList();
					if (b_links.Count == 1 && w_links.Count == 1) { // fly
						foreach (var item in _rounds) {
							if (IsTouch(item, w_links[0]) && IsCusp(b_links[0],w_links[0]) && IsCusp(item, e) && IsFlyOne(item, b_links[0])) {
								var step0 = GetStep(b_links[0]);
								var step1 = GetStep(item);
								if (step0.BlockId != step1.BlockId)
									return e;
							}
						}
					}
				}

				#endregion

				#region 黑先落子

				// black star
				if (StarFourHorn.Contains(a) && b_rounds.Contains(a)) {
					if (b_rounds.Count == 1 && w_rounds.Count == 1) {
						var w = w_rounds[0];
						if (IsFlyOne(w, a) && LineThree.Contains(w)) {	// 小飞挂
							foreach (var e in e_rounds) {
								if (IsFlyOne(e, a) && LineThree.Contains(e))
									return e;
							}
						}
					} else if (b_rounds.Count == 2 && w_rounds.Count == 1) {
						var b = b_rounds[0] == a ? b_rounds[1] : b_rounds[0];
						var w = w_rounds[0];
						if (IsFlyOne(w, a) && IsElephantTwo(w, b)) {
							foreach (var e in e_rounds) {
								if (IsFlyOne(e, w) && IsJumpOne(e, a) && LineThree.Contains(e))
									return e;
							}
						}
					} else if (b_rounds.Count == 2 && w_rounds.Count == 2) {
						var b = b_rounds[0] == a ? b_rounds[1] : b_rounds[0];
						var w_fly = IsFlyOne(w_rounds[0], a) ? w_rounds[0] : w_rounds[1];
						var w_2 = w_fly == w_rounds[0] ? w_rounds[1] : w_rounds[0];
						if (IsFlyOne(a, w_fly) && IsJumpOne(a, w_2) && IsFlyOne(a, b)) {
							foreach (var e in e_rounds) {
								if (IsCusp(a, e) && ThreeThrees.Contains(e))
									return e;
							}
						}
					}
				}
					// black small eye
				else if (AllEyes.Contains(a) && LineThree.Contains(a) && blacks.Contains(a)) {
					if (b_rounds.Count == 1 && w_rounds.Count == 1) {
						var w = w_rounds[0];
						if (IsJumpOne(w, a) && LineFour.Contains(w)) {
							foreach (var e in e_rounds) {
								if (IsFlyOne(e, a) && IsFlyOne(e, w) && LineFour.Contains(e))
									return e;
							}
						} else if (IsFlyOne(w, a) && LineThree.Contains(w)) {
							foreach (var e in e_rounds) {
								if (IsFlyOne(a, e) && LineFour.Contains(e))
									return e;
							}
						} else if (IsFlyTwo(w, a) && LineThree.Contains(w)) {
							foreach (var e in e_rounds) {
								if (IsJumpOne(e, w) && IsCusp(e, a))
									return e;
							}
						}
					}
				}
					// black high eye
				else if (AllEyes.Contains(a) && LineFour.Contains(a) && blacks.Contains(a)) {
					if (b_rounds.Count == 1 && w_rounds.Count == 1) {
						var w = w_rounds[0];
						if (IsJumpOne(a, w) && LineThree.Contains(w)) {
							foreach (var e in e_rounds) {
								if (IsFlyOne(e, a) && IsFlyOne(e, w) && LineFour.Contains(e)) {
									return e;
								}
							}
						}
					}
				}

				#endregion

				#region 白先落子

					// white star
				else if (StarFourHorn.Contains(a) && w_rounds.Contains(a)) {
					if (w_rounds.Count == 1 && b_rounds.Count == 0) {
						foreach (var e in e_rounds) {
							if (IsFlyOne(e, a) && LineThree.Contains(e))
								return e;
						}
					} else if (w_rounds.Count == 2 && b_rounds.Count == 1) {
						var w = w_rounds[0] == a ? w_rounds[1] : w_rounds[0];
						var b = b_rounds[0];
						if (IsFlyOne(b, a) && (IsElephantTwo(w, b) || IsJumpOne(w, a))) {
							foreach (var e in e_rounds) {
								if (IsFlyOne(e, b) && IsJumpOne(e, a) && LineTwo.Contains(e))
									return e;
							}
						}
					} else if (w_rounds.Count == 3 && b_rounds.Count == 2) {
						Pos three3 = m_InvalidPos;
						foreach (var w in w_rounds) {
							if (IsCusp(a, w) && ThreeThrees.Contains(w))
								three3 = w;
						}
						if (three3 != m_InvalidPos) {
							var b = IsFlyOne(a, b_rounds[0]) ? b_rounds[0] : b_rounds[1];
							foreach (var e in e_rounds) {
								if (IsJumpTwo(e, b) && LineThree.Contains(e))
									return e;
							}
						}
					}
				}

				#endregion

				#region 黑两子棋形

				if (b_rounds.Contains(a) && LinkPoses(a).Intersect(b_rounds).Count() == 2) {
					var bs = LinkPoses(a).Intersect(b_rounds).ToList();
					var b = a == bs[0] ? bs[1] : bs[0];

					//立二拆三
					var a_IsThree = LineThree.Contains(a) && LineFour.Contains(b);
					var b_IsThree = LineThree.Contains(b) && LineFour.Contains(a);
					if (a_IsThree) {
						int a_count = HasStoneCount(a, Directions.Up);
						int b_count = HasStoneCount(b, Directions.Up);
						if (a_count > 5 && b_count > 5) {
							var p = new Pos(a.Row - 3, a.Col);
							if (RoundTwoPoses(p).Count == 5 * 5)
								return new Pos(a.Row - 4, a.Col);
						}
						a_count = HasStoneCount(a, Directions.Down);
						b_count = HasStoneCount(b, Directions.Down);
						if (a_count > 5 && b_count > 5) {
							var p = new Pos(a.Row + 3, a.Col);
							if (RoundTwoPoses(p).Count == 5 * 5)
								return new Pos(a.Row + 4, a.Col);
						}
						a_count = HasStoneCount(a, Directions.Left);
						b_count = HasStoneCount(b, Directions.Left);
						if (a_count > 5 && b_count > 5) {
							var p = new Pos(a.Row, a.Col - 3);
							if (RoundTwoPoses(p).Count == 5 * 5)
								return new Pos(a.Row, a.Col - 4);
						}
						a_count = HasStoneCount(a, Directions.Right);
						b_count = HasStoneCount(b, Directions.Right);
						if (a_count > 5 && b_count > 5) {
							var p = new Pos(a.Row, a.Col + 3);
							if (RoundTwoPoses(p).Count == 5 * 5)
								return new Pos(a.Row, a.Col + 4);
						}
					} else if (b_IsThree) {
						int a_count = HasStoneCount(a, Directions.Up);
						int b_count = HasStoneCount(b, Directions.Up);
						if (a_count > 5 && b_count > 5) {
							var p = new Pos(b.Row - 3, b.Col);
							if (RoundTwoPoses(p).Count == 5 * 5)
								return new Pos(b.Row - 4, b.Col);
						}
						a_count = HasStoneCount(a, Directions.Down);
						b_count = HasStoneCount(b, Directions.Down);
						if (a_count > 5 && b_count > 5) {
							var p = new Pos(b.Row + 3, b.Col);
							if (RoundTwoPoses(p).Count == 5 * 5)
								return new Pos(b.Row + 4, b.Col);
						}
						a_count = HasStoneCount(a, Directions.Left);
						b_count = HasStoneCount(b, Directions.Left);
						if (a_count > 5 && b_count > 5) {
							var p = new Pos(b.Row, b.Col - 3);
							if (RoundTwoPoses(p).Count == 5 * 5)
								return new Pos(b.Row, b.Col - 4);
						}
						a_count = HasStoneCount(a, Directions.Right);
						b_count = HasStoneCount(b, Directions.Right);
						if (a_count > 5 && b_count > 5) {
							var p = new Pos(b.Row, b.Col + 3);
							if (RoundTwoPoses(p).Count == 5 * 5)
								return new Pos(b.Row, b.Col + 4);
						}
					}
				}

				#endregion
			}

			return m_InvalidPos;
		}
		Pos Passive()
		{
			var pos = CurrentPos;

			var empties = CurrentEffectEmptyPoses;
			var blacks = CurrentEffectBlackPoses;
			var whites = CurrentEffectWhitePoses;
			var all = CurrentEffectAllPoses;

			var copyEmpties = RemoveEmpties(empties, blacks);

			var rounds = RoundOnePoses(pos);
			var b_rounds = rounds.Intersect(blacks).ToList();
			var w_rounds = rounds.Intersect(whites).ToList();
			var e_rounds = rounds.Intersect(empties).ToList();

			var rounds2 = RoundTwoPoses(pos);
			var b_rounds2 = rounds2.Intersect(blacks).ToList();
			var w_rounds2 = rounds2.Intersect(whites).ToList();
			var e_rounds2 = rounds2.Intersect(empties).ToList();

			var links = LinkPoses(pos);
			var b_links = links.Intersect(blacks).ToList();
			var w_links = links.Intersect(whites).ToList();
			var e_links = links.Intersect(empties).ToList();

			foreach (var r in b_rounds) {
				foreach (var l in b_links) {
					if (IsFlyOne(r, l)) {	// 跨断飞
						foreach (var e in e_links) {
							if (IsTouch(e, r) && IsCusp(e, l))
								return e;
						}
					}
				}
			}

			foreach (var r in b_rounds2) {
				// 碰
				if (IsTouch(r, pos)) {
					foreach (var item in rounds2) {
						if (IsFlyOne(item, r) && IsJumpOne(item, pos) && whites.Contains(item)) { // 跳压
							foreach (var e in empties) {
								if (IsCusp(e, r) && IsJumpTwo(e, item))
									return e;
							}
						} else if (IsJumpOne(item, r) && IsFlyOne(item, pos) && whites.Contains(item)) { // 飞压
							foreach (var e in empties) {
								if (IsCusp(e, r) && IsFlyTwo(e, item))
									return e;
							}
						} else if (IsTouch(item, pos) && IsJumpOne(item, r) && blacks.Contains(item)) { // 防止穿断
							foreach (var e in copyEmpties) {
								if (IsCusp(e, item) && IsCusp(e, r) && IsTouch(e, pos))
									return e;
							}
						}
					}

					if (LineTwo.Contains(pos) && LineThree.Contains(r) && GetStep(pos).EmptyCount == 3) { // 二路扳粘
						foreach (var e in copyEmpties) {
							if (IsCusp(e, r) && LineTwo.Contains(e) && IsTouch(e, pos)
								&& LinkPoses(e).Intersect(WhitePoses).Count() == 1) {
								return e;
							}
						}
					}
					foreach (var e in e_rounds) {
						if (GetStep(r).EmptyCount >= GetStep(pos).EmptyCount) {
							if (IsCusp(e, r) && IsTouch(e, pos) && LinkPoses(e).Intersect(w_rounds).Count() == 1)
								return e;
						}
						if (GetStep(r).EmptyCount <= GetStep(pos).EmptyCount) {
							if (IsCusp(e, pos) && IsTouch(e, r))
								return e;
						}
					}
				}

				// 尖
				if (IsCusp(r, pos)) {
					if (LineTwo.Contains(pos)) {
						foreach (var e in copyEmpties) {
							if (IsTouch(e, r) && LineTwo.Contains(e))
								return e;
						}
					}
					foreach (var item in rounds2) { // 靠压之继续
						Pos w_fly = m_InvalidPos, w_touch = m_InvalidPos;
						foreach (var w in w_rounds2) {
							if (IsTouch(w, pos) && IsTouch(w, r))
								w_touch = w;
							else if (IsFlyOne(w, pos) && IsJumpTwo(w, r))
								w_fly = w;
						}
						Pos b_cusp = m_InvalidPos;
						foreach (var b in b_rounds2) {
							if (IsJumpOne(b, pos) && IsCusp(b, r) && IsFlyOne(b, w_fly))
								b_cusp = b;
						}
						foreach (var e in empties) {
							if (IsFlyOne(e, pos) && IsCusp(e, w_fly) && IsTouch(e, b_cusp))
								return e;
						}

					}

					//	+ +
					//	  0 0	后推车则飞之
					int r_off = pos.Row - r.Row;
					int c_off = pos.Col - r.Col;
					var b_row = new Pos(r.Row - r_off, r.Col);
					var w_row = new Pos(pos.Row - r_off, pos.Col);
					var b_col = new Pos(r.Row, r.Col - c_off);
					var w_col = new Pos(pos.Row, pos.Col - c_off);
					if (b_rounds2.Contains(b_row) && w_links.Contains(w_row)) {
						foreach (var e in e_rounds2) {
							if (IsFlyOne(e, r) && IsFlyOne(e, pos) && IsFlyTwo(e, b_row)
								&& !RoundOnePoses(e).Intersect(whites).Any())
								return e;
						}
					} else if (b_rounds2.Contains(b_col) && w_links.Contains(w_col)) {
						foreach (var e in e_rounds2) {
							if (IsFlyOne(e, r) && IsFlyOne(e, pos) && IsFlyTwo(e, b_col)
								&& !RoundOnePoses(e).Intersect(whites).Any())
								return e;
						}
					}

					foreach (var e in e_links) {
						if (LinkPoses(e).Intersect(b_rounds2).Count() == 2)	// 尖刺优先
							return e;
					}

					foreach (var e in e_links) {
						if (IsTouch(e, r) && IsTouch(e, pos))
							return e;
					}
				}

				// 跳
				if (IsJumpOne(r, pos)) {
					if (LineTwo.Contains(r) && LineTwo.Contains(pos)) {	// 扳粘之后续
						var erounds = RoundOnePoses(r).Intersect(empties).ToList();
						foreach (var e in erounds) {
							if (IsCusp(e, r) && IsFlyTwo(e, pos) && LineThree.Contains(e))
								return e;
						}

					}
					foreach (var e in empties) {
						if (IsElephantOne(e, pos) && IsJumpOne(e, r)
							&& RoundOnePoses(r).Intersect(empties).Count() == 8) {
							return e;
						}
					}
				}

				// 飞
				if (IsFlyOne(r, pos)) {
					foreach (var e in empties) {
						if (IsJumpOne(e, r) && IsTouch(e, pos))
							return e;
						else if (IsFlyOne(e, r) && IsCusp(e, pos) && LinkPoses(e).Intersect(empties).Count() == 5)
							return e;
						else if (IsJumpOne(e, r) && IsFlyOne(e, pos) && LinkPoses(e).Intersect(empties).Count() == 5)
							return e;
					}
				}
			}

			if (StepCount > m_EndCount) {
				foreach (var black in m_BlackMeshes) {
					if (LinkPoses(black).Intersect(m_WhiteMeshes).Any()) {
						if (EmptyPoses.Contains(black)) return black;
					}
				}
			}

			return m_InvalidPos;
		}

		List<Pos> RemoveEmpties(List<Pos> empties, List<Pos> blacks)
		{
			var copyEmpties = empties.ToList();

			foreach (var item in m_LevyPoses) { // 征子
				if (empties.Contains(item))
					empties.Remove(item);
			}

			foreach (var e in copyEmpties) {
				if (LinkPoses(e).Intersect(BlackPoses).Count() >= 3
					|| LinkPoses(e).Intersect(WhitePoses).Count() >= 3 || LineOneTwo.Contains(e)) {
					empties.Remove(e);	// 虎口和禁入
				}

				// 排除双
				var _links = LinkPoses(e);
				var _elinks = _links.Intersect(empties).ToList();
				_elinks.Remove(e);
				var _blinks = _links.Intersect(blacks).ToList();
				if (_blinks.Count == 2) {
					foreach (var el in _elinks) {
						bool isRow = el.Row == e.Row ? true : false;
						int r_off = isRow ? 0 : el.Row - e.Row;
						int c_off = isRow ? el.Col - e.Col : 0;
						var b0 = new Pos(_blinks[0].Row + r_off, _blinks[0].Col + c_off);
						var b1 = new Pos(_blinks[1].Row + r_off, _blinks[1].Col + c_off);
						if (IsTouch(b0, el) && IsTouch(b1, el) && blacks.Contains(b0) && blacks.Contains(b1)) {
							empties.Remove(el);
							empties.Remove(e);
						}
					}
				}

				// 排除死子
				var deads = new List<Step>();
				foreach (var block in m_DeadBlocks) {
					StepBlock stepBlock = block.Value;
					foreach (var item in stepBlock.Steps) {
						if (!deads.Contains(item))
							deads.Add(item);
					}
				}
				foreach (var item in deads) {
					var p = GetPos(item);
					if (e == p) empties.Remove(e);
				}
			}
			return copyEmpties;
		}
		int HasStoneCount(Pos pos, Directions direction)   // 多少步有子
		{
			int count = 0;
			var stones = BlackPoses.Union(WhitePoses).ToList();
			switch (direction) {
				case Directions.Up:
					while (true) {
						count++;
						int row = pos.Row - count;
						int col = pos.Col;
						if (!InRange(row, col)) {
							count--;
							break;
						}
						if (stones.Contains(new Pos(row, col))) break;
					}
					break;
				case Directions.Down:
					while (true) {
						count++;
						int row = pos.Row + count;
						int col = pos.Col;
						if (!InRange(row, col)) {
							count--;
							break;
						}
						if (stones.Contains(new Pos(row, col))) break;
					}
					break;
				case Directions.Left:
					while (true) {
						count++;
						int row = pos.Row;
						int col = pos.Col - count;
						if (!InRange(row, col)) {
							count--;
							break;
						}
						if (stones.Contains(new Pos(row, col))) break;
					}
					break;
				case Directions.Right:
					while (true) {
						count++;
						int row = pos.Row;
						int col = pos.Col + count;
						if (!InRange(row, col)) {
							count--;
							break;
						}
						if (stones.Contains(new Pos(row, col))) break;
					}
					break;
				default:
					break;
			}
			return count;
		}
	}
}
