/**
 * Board.Shape.cs (c) 2015 by x01
 * -----------------------------
 *  1.虽说没有全局观念是不可能下出好棋的，但还是决定先解决基本的棋形处理。
 *  2.全局扫描，根据相互位置进行处理，谓之 Positive().
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x01.Weiqi.Boards
{
	partial class Board
	{
		void Positive1()
		{
			var blacks = BlackPoses;
			var whites = WhitePoses;
			var empties = EmptyPoses;
			var all = AllPoses.Intersect(blacks.Union(whites)).ToList();

			foreach (var e in empties) {
				var r4 = RoundFourPoses(e);		// 大场
				if (!r4.Intersect(whites).Any() && !r4.Intersect(blacks).Any()) {
					if (StarFourHorn.Contains(e))
						AddDecreaseThinkPos(e, 5);
					else if (LineThree.Contains(e))
						AddDecreaseThinkPos(e, 5);
				}
			}

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
						if (step0.BlockId != step1.BlockId) {
							AddDecreaseThinkPos(e);
						}
					}
					var _rounds = RoundOnePoses(e).Intersect(blacks).ToList();
					if (b_links.Count == 1 && w_links.Count == 1) { // fly
						foreach (var item in _rounds) {
							if (IsTouch(item, w_links[0]) && IsCusp(b_links[0], w_links[0]) && IsCusp(item, e) && IsFlyOne(item, b_links[0])) {
								var step0 = GetStep(b_links[0]);
								var step1 = GetStep(item);
								if (step0.BlockId != step1.BlockId) {
									AddDecreaseThinkPos(e);
								}
							}
						}
					}
				}

				#endregion

				#region 黑先落子

				// 星
				if (StarFourHorn.Contains(a) && b_rounds.Contains(a)) {
					if (b_rounds.Count == 1 && w_rounds.Count == 1) {
						var w = w_rounds[0];
						// 小飞挂
						if (IsFlyOne(w, a) && LineThree.Contains(w)) {
							foreach (var e in e_rounds) {
								if (IsFlyOne(e, a) && LineThree.Contains(e)) // 小飞守
									AddDecreaseThinkPos(e, 5);
								else if (IsJumpOne(e, a) && LineFour.Contains(e)) // 跳守或压
									AddDecreaseThinkPos(e);
								else if (IsFlyTwo(e, a) && LineThree.Contains(e) && !IsTouch(e, w)) // 大飞守
									AddDecreaseThinkPos(e);
								else if (IsCusp(e, a) && LineThree.Contains(e) && IsTouch(e, w)) // 尖顶
									AddDecreaseThinkPos(e);
								else if (IsFlyOne(e, a) && LineTwo.Contains(e)
										&& (IsCusp(e, w) || IsFlyOne(e, w)))  // 二路守
									AddDecreaseThinkPos(e, -5);
								else if (IsTouch(e, a) && IsJumpOne(e, w)) // 立守
									AddDecreaseThinkPos(e, -2);
							}
						}
							// 大飞挂
						else if (IsFlyTwo(w, a) && LineThree.Contains(w)) {
							foreach (var e in e_rounds) {
								if (IsFlyTwo(e, a) && LineThree.Contains(e)) // 大飞守
									AddDecreaseThinkPos(e, 5);
								else if (IsFlyOne(e, a) && LineThree.Contains(e) && !IsTouch(e, w)) // 小飞守
									AddDecreaseThinkPos(e);
								else if (IsCusp(e, a) && IsJumpOne(e, w)) // 尖守
									AddDecreaseThinkPos(e, 3);
							}
						}
					} else if (b_rounds.Count == 2 && w_rounds.Count == 2) {
						var b = b_rounds[0] == a ? b_rounds[1] : b_rounds[0];
						var w_fly = IsFlyOne(w_rounds[0], a) ? w_rounds[0] : w_rounds[1];
						var w_2 = w_fly == w_rounds[0] ? w_rounds[1] : w_rounds[0];
						if (IsFlyOne(a, w_fly) && IsJumpOne(a, w_2) && IsFlyOne(a, b)) {
							foreach (var e in e_rounds) {
								if (IsCusp(a, e) && ThreeThrees.Contains(e)) // 三三
									AddDecreaseThinkPos(e, 5);
							}
						}
					}
				}
					// 小目
				else if (AllEyes.Contains(a) && LineThree.Contains(a) && b_rounds.Contains(a)) {
					if (b_rounds.Count == 1 && w_rounds.Count == 1) {
						var w = w_rounds[0];
						if (IsJumpOne(w, a) && LineFour.Contains(w)) {  // 一间高挂
							foreach (var e in e_rounds) {
								if (IsFlyOne(e, a) && IsFlyOne(e, w) && LineFour.Contains(e))
									AddDecreaseThinkPos(e, 5);
								else if (IsFlyOne(e, a) && IsTouch(e, w))  // 托或靠
									AddDecreaseThinkPos(e, 3);
								else if (IsJumpTwo(e, a) && LineThree.Contains(e)) // 拆二
									AddDecreaseThinkPos(e, -2);
								else if (IsJumpX(e, a, 4) && LineFour.Contains(e)) // 二间高夹
									AddDecreaseThinkPos(e, 3);
								else if (IsFlyX(e, a, 3) && LineThree.Contains(e))	// 一间低夹
									AddDecreaseThinkPos(e, 3);

							}
						} else if (IsFlyOne(w, a) && LineThree.Contains(w)) { // 小飞挂
							foreach (var e in e_rounds) {
								if (IsFlyOne(a, e) && LineFour.Contains(e)) // 飞守
									AddDecreaseThinkPos(e, 3);
								else if (IsJumpX(e, a, 4) && LineFour.Contains(e)) // 二间高夹
									AddDecreaseThinkPos(e, 5);
								else if (IsFlyX(e, a, 3) && LineThree.Contains(e)) // 一间低夹
									AddDecreaseThinkPos(e, 3);
							}
						} else if (IsFlyTwo(w, a) && LineThree.Contains(w)) { // 大飞挂
							foreach (var e in e_rounds) {
								if (IsJumpOne(e, w) && IsCusp(e, a))	// 尖守
									AddDecreaseThinkPos(e, 5);
								else if (IsJumpOne(e, a) && IsCusp(e, w)) // 跳攻
									AddDecreaseThinkPos(e);
								else if (IsFlyX(e, a, 4) && IsJumpOne(e, w)) // 一间低夹
									AddDecreaseThinkPos(e, 2);
							}
						}
					}
				}
					// 高目
				else if (AllEyes.Contains(a) && LineFour.Contains(a) && blacks.Contains(a)) {
					if (b_rounds.Count == 1 && w_rounds.Count == 1) {
						var w = w_rounds[0];
						if (IsJumpOne(a, w) && LineThree.Contains(w)) {		// 一间低挂
							foreach (var e in e_rounds) {
								if (IsFlyOne(e, a) && IsFlyOne(e, w) && LineFour.Contains(e)) {  // 飞罩
									AddDecreaseThinkPos(e);
								} else if (IsFlyTwo(e, a) && IsFlyTwo(e, w) && LineFour.Contains(e)) { // 大飞罩
									AddDecreaseThinkPos(e, -2);
								} else if (IsFlyOne(e, a) && IsTouch(e, w)) { // 飞靠
									AddDecreaseThinkPos(e, 3);
								}
							}
						}
					}
				}

				#endregion

				#region 白先落子

					// 星
				else if (StarFourHorn.Contains(a) && w_rounds.Contains(a)) {
					if (w_rounds.Count == 1 && b_rounds.Count == 0) {
						foreach (var e in e_rounds) {
							if (IsFlyOne(e, a) && LineThree.Contains(e)) // 小飞挂
								AddDecreaseThinkPos(e, 5);
							else if (IsFlyTwo(e, a) && LineThree.Contains(e)) // 大飞挂
								AddDecreaseThinkPos(e);
							else if (IsJumpOne(e, a) && LineFour.Contains(e)) // 一间挂
								AddDecreaseThinkPos(e, -2);
							else if (IsFlyOne(e, a) && LineTwo.Contains(e))	// 二路挂
								AddDecreaseThinkPos(e, -5);
							else if (IsCusp(e, a) && ThreeThrees.Contains(e))	// 点三三
								AddDecreaseThinkPos(e, -10);
						}
					} else if (w_rounds.Count == 2 && b_rounds.Count == 1) {
						var w = w_rounds[0] == a ? w_rounds[1] : w_rounds[0];
						var b = b_rounds[0];
						if (IsFlyOne(b, a) && (IsElephantTwo(w, b) || IsJumpOne(w, a))) {
							foreach (var e in e_rounds) {
								if (IsFlyOne(e, b) && IsJumpOne(e, a) && LineTwo.Contains(e))	// 二路飞
									AddDecreaseThinkPos(e, 3);
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
								if (IsJumpTwo(e, b) && LineThree.Contains(e))	// 拆二
									AddDecreaseThinkPos(e, 3);
							}
						}
					}
				}

				#endregion

				#region 黑一子棋形

				// 一线
				if (LineOne.Contains(a) && b_rounds.Contains(a)) {
					var ws = LinkPoses(a).Intersect(w_rounds).ToList();
					var es = LinkPoses(a).Intersect(e_rounds).ToList();
					if (es.Count == 2 && ws.Count == 1) {
						var e2 = LineTwo.Contains(es[0]) ? es[1] : es[0];
						var w = ws[0];
						var rs = RoundOnePoses(a).Intersect(e_rounds).ToList();
						foreach (var r in rs) {
							if (IsCusp(r, a) && IsFlyOne(r, w)) {
								AddDecreaseThinkPos(r);
								AddDecreaseThinkPos(e2);
							}
						}
					}
				}
					// 二线
				else if (LineTwo.Contains(a) && b_rounds.Contains(a)) {
					var ws = RoundTwoPoses(a).Intersect(w_rounds).ToList();
					foreach (var e in e_rounds) {
						if (ws.Count == 1) {
							if (IsFlyOne(a, ws[0]) && IsCusp(e, ws[0]) && IsFlyOne(e, a))
								AddDecreaseThinkPos(e);
							else if (IsJumpOne(a, ws[0]) && IsCusp(e, ws[0]) && IsCusp(e, a))
								AddDecreaseThinkPos(e);
						}
					}
				}
					// 三线
				else if (LineThree.Contains(a) && b_rounds.Contains(a)) {
					var ws = RoundTwoPoses(a).Intersect(w_rounds).ToList();
					if (ws.Count == 1 && IsFlyOne(ws[0], a) && LineFour.Contains(ws[0])) { // 二路飞入
						foreach (var e in e_rounds) {
							if (IsFlyOne(e, a) && IsJumpOne(e, ws[0]))
								AddDecreaseThinkPos(e, 3);
						}
					}

					var range = 6;
					int count = HasStoneCount(a, Directions.Up);
					if (count > range) {
						Pos p = new Pos(a.Row - 3, a.Col);
						if (LineThree.Contains(p) && RoundTwoPoses(p).Intersect(empties).Count() == 5 * 5) { // 拆二
							AddDecreaseThinkPos(p, 5);
							var rs = RoundOnePoses(p);
							foreach (var r in rs) {
								if (LineThreeFour.Contains(r)) {
									if (IsFlyTwo(r, a))			// 大飞拆
										AddDecreaseThinkPos(r);
									else if (IsFlyX(r, a, 3))	// 超大飞拆
										AddDecreaseThinkPos(r, 3);
									else if (IsJumpThree(r, a))	// 拆三
										AddDecreaseThinkPos(r, -1);
								}
							}
						}
					}
					count = HasStoneCount(a, Directions.Down);
					if (count > range) {
						Pos p = new Pos(a.Row + 3, a.Col);
						if (LineThree.Contains(p) && RoundTwoPoses(p).Intersect(empties).Count() == 5 * 5) {
							AddDecreaseThinkPos(p);
							var rs = RoundOnePoses(p);
							foreach (var r in rs) {
								if (LineThreeFour.Contains(r)) {
									if (IsFlyTwo(r, a))			// 大飞拆
										AddDecreaseThinkPos(r);
									else if (IsFlyX(r, a, 3))	// 超大飞拆
										AddDecreaseThinkPos(r, 3);
									else if (IsJumpThree(r, a))	// 拆三
										AddDecreaseThinkPos(r, -1);
								}
							}
						}
					}
					count = HasStoneCount(a, Directions.Left);
					if (count > range) {
						Pos p = new Pos(a.Row, a.Col - 3);
						if (LineThree.Contains(p) && RoundTwoPoses(p).Intersect(empties).Count() == 5 * 5) {
							AddDecreaseThinkPos(p);
							var rs = RoundOnePoses(p);
							foreach (var r in rs) {
								if (LineThreeFour.Contains(r)) {
									if (IsFlyTwo(r, a))			// 大飞拆
										AddDecreaseThinkPos(r);
									else if (IsFlyX(r, a, 3))	// 超大飞拆
										AddDecreaseThinkPos(r, 3);
									else if (IsJumpThree(r, a))	// 拆三
										AddDecreaseThinkPos(r, -1);
								}
							}
						}
					}
					count = HasStoneCount(a, Directions.Right);
					if (count > range) {
						Pos p = new Pos(a.Row, a.Col + 3);
						if (LineThree.Contains(p) && RoundTwoPoses(p).Intersect(empties).Count() == 5 * 5) {
							AddDecreaseThinkPos(p);
							var rs = RoundOnePoses(p);
							foreach (var r in rs) {
								if (LineThreeFour.Contains(r)) {
									if (IsFlyTwo(r, a))			// 大飞拆
										AddDecreaseThinkPos(r);
									else if (IsFlyX(r, a, 3))	// 超大飞拆
										AddDecreaseThinkPos(r, 3);
									else if (IsJumpThree(r, a))	// 拆三
										AddDecreaseThinkPos(r, -1);
								}
							}
						}
					}
				}
					// 四线
				else if (LineFour.Contains(a) && b_rounds.Contains(a)) {
					foreach (var e in e_rounds) {
						if (LineThreeFour.Contains(e)) {
							if (RoundOnePoses(e).Intersect(e_rounds).Count() == 3 * 3 &&
								(IsFlyOne(e, a) || IsJumpOne(e, a))) {	// 小飞守
								AddDecreaseThinkPos(e, 5);
							} else if (IsFlyTwo(e, a)
								&& RoundTwoPoses(e).Intersect(e_rounds).Count() == 5 * 5) {	// 大飞守
								AddDecreaseThinkPos(e, 3);
							} else if (IsFlyX(e, a, 3)
								&& RoundTwoPoses(e).Intersect(e_rounds).Count() == 5 * 5) { // 超大飞
								AddDecreaseThinkPos(e);
							}
						}
					}
				}

				#endregion

				#region 白一子棋形

				// 一线
				if (w_rounds.Contains(a) && LineOne.Contains(a)) {
					foreach (var e in e_rounds) {
						if (IsCusp(e, a) && LineTwo.Contains(e)
							&& RoundOnePoses(e).Intersect(b_rounds).Count() >= 2) {
							AddDecreaseThinkPos(e);
						} else if (IsTouch(e, a) && LineOne.Contains(e)
							&& RoundOnePoses(e).Intersect(b_rounds).Count() >= 2) {
							AddDecreaseThinkPos(e, -2);
						}
					}
				}
					// 二线
				else if (w_rounds.Contains(a) && LineTwo.Contains(a)) {
					foreach (var e in e_rounds) {
						if (IsTouch(e, a) && LineTwo.Contains(e)
							&& RoundOnePoses(e).Intersect(b_rounds).Count() >= 2) {
							AddDecreaseThinkPos(e, -2);
						} else if (IsCusp(e, a) && LineThree.Contains(e)
							&& RoundOnePoses(e).Intersect(b_rounds).Count() >= 2) {
							AddDecreaseThinkPos(e);
						}
					}
				}
					// 三线
				else if (w_rounds.Contains(a) && LineThree.Contains(a)) {
					foreach (var e in e_rounds) {
						if (IsJumpOne(e, a) && LineCenter.Contains(e)
							&& RoundOnePoses(e).Intersect(e_rounds).Count() == 3 * 3) { // 一间逼或镇
							AddDecreaseThinkPos(e);
						} else if (IsJumpTwo(e, a) && LineThree.Contains(e)
							&& RoundOnePoses(e).Intersect(e_rounds).Count() == 3 * 3) {	// 二间逼
							AddDecreaseThinkPos(e, 3);
						} else if (IsCusp(e, a) && LineFour.Contains(e)
							&& LinkPoses(e).Intersect(e_rounds).Count() == 5) { // 尖冲
							AddDecreaseThinkPos(e, -1);
						}
					}
				}
					// 四线
				else if (w_rounds.Contains(a) && LineFour.Contains(a)) {
					foreach (var e in e_rounds) {
						if (IsFlyOne(e, a) && LineThree.Contains(e)
							&& RoundOnePoses(e).Intersect(e_rounds).Count() == 3 * 3) {	// 小飞逼
							AddDecreaseThinkPos(e, 3);
						} else if (IsFlyTwo(e, a) && LineThree.Contains(e)
							&& RoundOnePoses(e).Intersect(e_rounds).Count() == 3 * 3) { // 大飞逼
							AddDecreaseThinkPos(e);
						} else if (IsJumpOne(e, a) && LineTwo.Contains(e)
							 && RoundOnePoses(e).Intersect(e_rounds).Count() == 3 * 3) { // 二线漏
							AddDecreaseThinkPos(e, -2);
						}
					}
				}
				#endregion

				#region 黑两子棋形

				if (b_rounds.Contains(a) && RoundOnePoses(a).Intersect(b_rounds).Count() == 2) {
					var bs = RoundOnePoses(a).Intersect(b_rounds).ToList();
					var b = a == bs[0] ? bs[1] : bs[0];

					//立二、尖二拆三
					var range = 6;
					var b_IsFour = LineThree.Contains(a) && LineFour.Contains(b);
					var a_IsFour = LineThree.Contains(b) && LineFour.Contains(a);
					if (a_IsFour) {
						int a_count = HasStoneCount(a, Directions.Up);
						int b_count = HasStoneCount(b, Directions.Up);
						if (a_count > range && b_count > range) {
							var p = new Pos(a.Row - 3, a.Col);
							if (RoundTwoPoses(p).Intersect(e_rounds).Count() == 5 * 5) {
								AddDecreaseThinkPos(new Pos(a.Row - 4, b.Col), 3);
								AddDecreaseThinkPos(new Pos(a.Row - 4, a.Col));
							}
						}
						a_count = HasStoneCount(a, Directions.Down);
						b_count = HasStoneCount(b, Directions.Down);
						if (a_count > range && b_count > range) {
							var p = new Pos(a.Row + 3, a.Col);
							if (RoundTwoPoses(p).Intersect(e_rounds).Count() == 5 * 5) {
								AddDecreaseThinkPos(new Pos(a.Row + 4, b.Col), 3);
								AddDecreaseThinkPos(new Pos(a.Row + 4, a.Col));
							}
						}
						a_count = HasStoneCount(a, Directions.Left);
						b_count = HasStoneCount(b, Directions.Left);
						if (a_count > range && b_count > range) {
							var p = new Pos(a.Row, a.Col - 3);
							if (RoundTwoPoses(p).Intersect(e_rounds).Count() == 5 * 5) {
								AddDecreaseThinkPos(new Pos(b.Row, a.Col - 4), 3);
								AddDecreaseThinkPos(new Pos(a.Row, a.Col - 4));
							}
						}
						a_count = HasStoneCount(a, Directions.Right);
						b_count = HasStoneCount(b, Directions.Right);
						if (a_count > range && b_count > range) {
							var p = new Pos(a.Row, a.Col + 3);
							if (RoundTwoPoses(p).Intersect(e_rounds).Count() == 5 * 5) {
								AddDecreaseThinkPos(new Pos(b.Row, a.Col + 4), 3);
								AddDecreaseThinkPos(new Pos(a.Row, a.Col + 4));
							}
						}
					} else if (b_IsFour) {
						int a_count = HasStoneCount(a, Directions.Up);
						int b_count = HasStoneCount(b, Directions.Up);
						if (a_count > range && b_count > range) {
							var p = new Pos(b.Row - 3, b.Col);
							if (RoundTwoPoses(p).Intersect(e_rounds).Count() == 5 * 5) {
								AddDecreaseThinkPos(new Pos(b.Row - 4, a.Col), 3);
								AddDecreaseThinkPos(new Pos(b.Row - 4, b.Col));
							}
						}
						a_count = HasStoneCount(a, Directions.Down);
						b_count = HasStoneCount(b, Directions.Down);
						if (a_count > range && b_count > range) {
							var p = new Pos(b.Row + 3, b.Col);
							if (RoundTwoPoses(p).Intersect(e_rounds).Count() == 5 * 5) {
								AddDecreaseThinkPos(new Pos(b.Row + 4, a.Col), 3);
								AddDecreaseThinkPos(new Pos(b.Row + 4, b.Col));
							}
						}
						a_count = HasStoneCount(a, Directions.Left);
						b_count = HasStoneCount(b, Directions.Left);
						if (a_count > range && b_count > range) {
							var p = new Pos(b.Row, b.Col - 3);
							if (RoundTwoPoses(p).Intersect(e_rounds).Count() == 5 * 5) {
								AddDecreaseThinkPos(new Pos(a.Row, b.Col - 4), 3);
								AddDecreaseThinkPos(new Pos(b.Row, b.Col - 4));
							}
						}
						a_count = HasStoneCount(a, Directions.Right);
						b_count = HasStoneCount(b, Directions.Right);
						if (a_count > range && b_count > range) {
							var p = new Pos(b.Row, b.Col + 3);
							if (RoundTwoPoses(p).Intersect(e_rounds).Count() == 5 * 5) {
								AddDecreaseThinkPos(new Pos(a.Row, b.Col + 4), 3);
								AddDecreaseThinkPos(new Pos(b.Row, b.Col + 4));
							}
						}
					}
				}

				#endregion

				#region 白两子棋形

				if (w_rounds.Contains(a) && LinkPoses(a).Intersect(w_rounds).Count() == 2) {
					var ws = LinkPoses(a).Intersect(w_rounds).ToList();
					var w = a == ws[0] ? ws[1] : ws[0];
					// 黑星位扳长
					if (AllEyes.Contains(a) && IsTouch(a, w)
						&& LineThree.Contains(a) && LineThree.Contains(w)) {
						var blinks = LinkPoses(a).Intersect(b_rounds).ToList();
						if (blinks.Count == 2 && IsCusp(blinks[0], blinks[1])) {
							var star = StarFourHorn.Contains(blinks[0]) ? blinks[0]
								: StarFourHorn.Contains(blinks[1]) ? blinks[1]
								: m_InvalidPos;
							var three = ThreeThrees.Contains(blinks[0]) ? blinks[0]
								: ThreeThrees.Contains(blinks[1]) ? blinks[1]
								: m_InvalidPos;
							if (star == m_InvalidPos || three == m_InvalidPos)
								continue;
							foreach (var e in e_rounds) {
								if (LineTwo.Contains(e) && IsCusp(e, a) && IsTouch(e, three))
									AddDecreaseThinkPos(e);
							}
						}
					}
						// 黑粘
					else if (IsTouch(a, w) && LineThree.Contains(a) && LineThree.Contains(w)) {
						var blinks = LinkPoses(a).Intersect(b_rounds).ToList();
						if (blinks.Count == 2 && IsCusp(blinks[0], blinks[1]) &&
							LineThreeFour.Contains(blinks[0]) && LineThreeFour.Contains(blinks[1])) {
							foreach (var e in empties) {
								if (IsTouch(e, blinks[0]) && IsTouch(e, blinks[1]))
									AddDecreaseThinkPos(e);
							}
						}
					}
				}

				#endregion

				#region 封口

				if (StepCount < 30) continue;

				foreach (var b in blacks) {
					if (LineFour.Contains(b)) {
						if (GetBlackMeshBlockCount(b) > 10) {
							var rs = RoundOnePoses(b);
							var ws = rs.Intersect(whites).ToList();
							foreach (var w in ws) {
								if (LineThree.Contains(w) && IsCusp(w, b)) {
									foreach (var e in empties) {
										if (IsTouch(e, b) && IsTouch(e, w) && LineThree.Contains(e))
											AddIncreaseThinkPos(e);
									}
								} else if (LineThree.Contains(w) && IsFlyOne(w, b)) {
									foreach (var e in empties) {
										if (IsFlyOne(e, b) && IsCusp(e, w) && LineTwo.Contains(e))
											AddIncreaseThinkPos(e);
									}
								} else if (LineFour.Contains(w) && IsJumpOne(w, b)) {
									foreach (var e in empties) {
										if (IsCusp(e, b) && IsCusp(e, w) && LineThree.Contains(e))
											AddIncreaseThinkPos(e);
									}
								} else if (LineFour.Contains(w) && IsTouch(w, b)) {
									foreach (var e in empties) {
										if (IsJumpOne(e, b) && IsFlyOne(e, w) && LineTwo.Contains(e)
											&& RoundOnePoses(e).Intersect(empties).Count() == 3 * 3)
											AddIncreaseThinkPos(e);
									}
								}
							}
						}
					} else if (LineThree.Contains(b)) {
						if (GetBlackMeshBlockCount(b) > 10) {
							var rs = RoundOnePoses(b);
							var ws = rs.Intersect(whites).ToList();
							foreach (var w in ws) {
								//  0 +			line 3
								//	0 *
								//
								if (LineTwo.Contains(w) && IsCusp(w, b)) {
									foreach (var e in empties) {
										if (IsTouch(e, b) && IsTouch(e, w) && LineTwo.Contains(e))
											AddIncreaseThinkPos(e);
									}
								}
									//  0			
									//       +		line 3
									//  *
									//
								else if (LineFour.Contains(w) && IsFlyOne(w, b)) {
									foreach (var e in empties) {
										if (IsFlyOne(e, b) && IsJumpOne(e, w) && LineTwo.Contains(e)
											&& RoundOnePoses(e).Intersect(empties).Count() == 3 * 3)
											AddIncreaseThinkPos(e);
									}
								}
									//  0   +		line 3
									//    *
									//
								else if (LineThree.Contains(w) && IsJumpOne(w, b)) {
									foreach (var e in empties) {
										if (IsCusp(e, b) && IsCusp(e, w) && LineTwo.Contains(e)
											&& LinkPoses(e).Intersect(empties).Count() == 5)
											AddIncreaseThinkPos(e);
									}
								}
									//  0 +			line 3
									//  * *			两种可能
									//
								else if (LineThree.Contains(w) && IsTouch(w, b)) {
									foreach (var e in empties) {
										if (IsCusp(e, b) && IsTouch(e, w) && LineTwo.Contains(e)
											&& LinkPoses(e).Intersect(empties).Count() == 4) {
											AddIncreaseThinkPos(e);
										} else if (IsTouch(e, b) && IsCusp(e, w) && LineTwo.Contains(e)
											 && LinkPoses(e).Intersect(empties).Count() == 3) {
											AddIncreaseThinkPos(e);
										}
									}
								}
							}
						}
					} else if (LineTwo.Contains(b)) {
						if (GetBlackMeshBlockCount(b) > 10) {
							var rs = RoundOnePoses(b);
							var ws = rs.Intersect(whites).ToList();
							foreach (var w in ws) {
								if (LineOne.Contains(w) && IsCusp(w, b)) {
									foreach (var e in empties) {
										if (IsTouch(e, b) && IsTouch(e, w) && LineOne.Contains(e))
											AddIncreaseThinkPos(e);
									}
								}
							}
						}
					}
				}
				#endregion

			}
		}
		void Positive2()
		{
			UpdateMeshes();

			List<Pos> b_rounds = new List<Pos>(); // 黑白之边界
			foreach (var b in m_BlackMeshes) {
				var links = LinkPoses(b);
				foreach (var l in links) {
					if (m_WhiteMeshes.Contains(l) && EmptyPoses.Contains(b) && !b_rounds.Contains(l))
						b_rounds.Add(l);
				}
			}

			foreach (var b in b_rounds) {
				AddDecreaseThinkPos(b);
			}
		}
		void Positive3()
		{
			UpdateAllMeshBlocks();

			var w_blocks = m_WhiteMeshBlocks.OrderByDescending(b => b.Poses.Count).ToList();
			var b_blocks = m_BlackMeshBlocks.OrderByDescending(b => b.Poses.Count).ToList();
			foreach (var b in b_blocks) {
				if (b.IsDead) {
					AddThinkPos(b.KeyPos);
				}
			}
			foreach (var w in w_blocks) {
				if (w.IsDead) {
					AddThinkPos(w.KeyPos);
				}
			}
			foreach (var s in NineStars) {
				if (StarFourHorn.Contains(s))
					AddDecreaseThinkPos(s, 5);
				else
					AddDecreaseThinkPos(s);
			}
		}

		Pos Attack()
		{
			var blacks = BlackPoses;
			var whites = WhitePoses;
			var empties = EmptyPoses;
			var copy_empties = RemoveEmpties(empties, blacks);

			// 以黑为基准
			foreach (var b in blacks) {
				var rounds = RoundSixPoses(b);
				var b_rounds = rounds.Intersect(blacks).ToList();
				var w_rounds = rounds.Intersect(whites).ToList();
				var e_rounds = rounds.Intersect(empties).ToList();

				// 小飞
				if (LineThreeFour.Contains(b) && StepCount > 50) {
					foreach (var e in e_rounds) {
						if (IsFlyOne(e, b) && RoundTwoPoses(e).Intersect(e_rounds).Count() == 5 * 5) {
							return e;
						}
					}
				} else if (GoldHorn.Contains(b) && StepCount > 7) {
					foreach (var e in e_rounds) {
						if (IsFlyOne(e, b) && LineThreeFour.Contains(e)
							&& RoundOnePoses(e).Intersect(e_rounds).Count() == 9
							&& RoundThreePoses(b).Intersect(b_rounds).Count() == 1) {
							return e;
						}
					}
				}

				foreach (var br in b_rounds) {

					#region 尖形

					if (IsCusp(b, br)) {
						//   + *		+ 为黑子
						// + 0			0 为白子
						//   0			* 为将下之子
						if (StarFourHorn.Contains(b) && ThreeThrees.Contains(br)) {	// 三三扳立
							var rs = RoundOnePoses(b);
							var bs = rs.Intersect(b_rounds).ToList();
							var ws = rs.Intersect(w_rounds).ToList();
							var es = rs.Intersect(e_rounds).ToList();
							if (bs.Count == 2 && ws.Count == 2 && IsTouch(ws[0], ws[1])) {
								var w = IsTouch(b, ws[0]) ? ws[0] : ws[1];
								foreach (var e in copy_empties) {
									if (LineTwo.Contains(e) && IsCusp(e, w) && IsFlyOne(e, b) && IsTouch(e, br)) {
										return e;
									}
								}
							}
						}
						//    0				 w0
						//	+ 0   0	  ==   b wr   w1
						//    + *			 br *	
						foreach (var wr in w_rounds) {
							if (IsTouch(b, wr) && IsTouch(br, wr)) {
								var wr2 = RoundTwoPoses(wr).Intersect(w_rounds).ToList();
								var er2 = RoundTwoPoses(wr).Intersect(e_rounds).ToList();
								var copy_wr2 = wr2.ToList();
								wr2.Remove(wr);
								var br2 = RoundTwoPoses(wr).Intersect(b_rounds).ToList();
								if (wr2.Count == 2 && br2.Count == 2) {
									var w0 = IsTouch(wr, wr2[0]) ? wr2[0] : wr2[1];
									var w1 = IsJumpOne(wr, wr2[0]) ? wr2[0] : wr2[1];
									if (IsCusp(b, w0) && IsJumpTwo(b, w1)) {
										foreach (var e in er2) {
											if (IsTouch(br, e) && IsCusp(w1, e)) { // 扳长
												return e;
											}
										}
									}
								}
									//     0		
									//	 + 0   0
									// *   + + 0
								else if (copy_wr2.Count == 4 && br2.Count == 3) {
									br2.Remove(b); br2.Remove(br);
									int r_off = br2[0].Row - br.Row;
									int c_off = br2[0].Col - br.Col;
									Pos p = new Pos(br2[0].Row + r_off, br2[0].Col + c_off);
									if (IsFlyOne(p, wr)) {
										r_off = wr.Row - br.Row;
										c_off = wr.Col - br.Col;
										p = new Pos(wr.Row + r_off, wr.Col + c_off);
										if (IsCusp(b, p)) {
											r_off = wr.Row - b.Row;
											c_off = wr.Col - b.Col;
											p = new Pos(wr.Row + 2 * r_off, wr.Col + 2 * c_off);
											if (IsJumpOne(p, wr)) {
												foreach (var e in e_rounds) {
													if (IsCusp(e, b) && IsFlyX(e, p, 3)) {  // 虎
														return e;
													}
												}
											}
										}
									}
								}
							}
						}
						//    0 +
						//  0   0 +
						//      *
						var b_rs = RoundOnePoses(b);
						var b_ws = b_rs.Intersect(w_rounds).ToList();
						if (b_ws.Count == 2 && IsCusp(b_ws[0], b_ws[1])) {
							var w = (IsTouch(br, b_ws[0]) && IsFlyOne(br, b_ws[1])) ? b_ws[0] : b_ws[1];
							b_ws.Remove(w);
							int r_off = w.Row - br.Row;
							int c_off = w.Col - br.Col;
							var w3 = new Pos(w.Row + 2 * r_off, w.Col + c_off * 2);
							if (IsCusp(w3, b_ws[0]) && IsFlyOne(w3, b)) {
								foreach (var e in e_rounds) {
									if (IsTouch(e, w) && IsJumpOne(e, b))
										return e;
								}
							}
						}
							//    0 + *
							//    0 0 +
						else if (b_ws.Count == 3) {
							foreach (var w in b_ws) {
								if (IsCusp(b, w) && IsJumpOne(br, w)) {
									foreach (var e in b_rs) {
										if (e_rounds.Contains(e) && IsTouch(e, b) && IsTouch(e, br) && IsFlyOne(e, w))
											return e;
									}
								}
							}
						}
					}

					#endregion

					#region 碰形
						// Touch
					 else if (IsTouch(b, br)) {
						var rs = RoundOnePoses(b);
						var rbs = rs.Intersect(b_rounds).ToList();
						var rws = rs.Intersect(w_rounds).ToList();
						var res = rs.Intersect(e_rounds).ToList();
						var links = LinkPoses(b);
						var lws = links.Intersect(w_rounds).ToList();
						var lbs = links.Intersect(b_rounds).ToList();
						//    +		
						//	0 +   +
						//    0 0 *	
						if (lws.Count == 2 && IsCusp(lws[0], lws[1]) && lbs.Count == 2 && lbs.Contains(br)) {
							var w = IsJumpOne(lws[0], br) ? lws[0] : lws[1];
							var w2 = w == lws[0] ? lws[1] : lws[0];
							int r_off = b.Row - w2.Row;
							int c_off = b.Col - w2.Col;
							var b3 = new Pos(b.Row + 2 * r_off, b.Col + 2 * c_off);
							if (b_rounds.Contains(b3)) {
								var rb3 = RoundOnePoses(b3);
								var rw3s = rb3.Intersect(w_rounds).ToList();
								if (rw3s.Count == 1) {
									if (IsTouch(rw3s[0], w) && IsCusp(rw3s[0], b3)) {
										foreach (var e in rb3) {
											if (e_rounds.Contains(e)) {
												if (IsTouch(e, b3) && IsTouch(rw3s[0], e) && IsFlyOne(e, b))
													return e;
											}
										}
									}
								}
							}
						}
							//    +	0 	
							//	  + 0  
							//  * 0 
						else if (rws.Count == 3 && rbs.Count == 2) {
							foreach (var rw in rws) {
								if (IsTouch(rw, b)) {
									foreach (var w2 in rws) {
										if (IsTouch(w2, br) && IsTouch(w2, rw)) {
											foreach (var w3 in rws) {
												if (IsTouch(b, w3) && IsCusp(rw, w3)
													&& LineThree.Contains(b) && LineThree.Contains(br)) {
													foreach (var e in res) {
														if (IsCusp(e, b) && IsElephantOne(e, w2)) {
															return e;
														}
													}
												}
													//      + 0 	
													//	*   + 0  
													//      0 
												else if (IsTouch(b, w3) && IsCusp(rw, w3) && !(LineThree.Contains(b) || LineThree.Contains(br))) {
													int r_off = b.Row - rw.Row;
													int c_off = b.Col - rw.Col;
													var p = new Pos(b.Row + 2 * r_off, b.Col + 2 * c_off);
													if (e_rounds.Contains(p))
														return p;
												}
											}
										}
									}
								}
							}
						}
					}
					#endregion

					// 小飞后拆二
					if (IsFlyOne(b, br) && LineThree.Contains(b) && LineTwo.Contains(br)) {
						foreach (var e in e_rounds) {
							if (IsJumpTwo(b, e) && !IsCusp(e, br) && LineThree.Contains(e)
								&& RoundTwoPoses(e).Intersect(copy_empties).Count() == 5 * 5) {
								return e;
							}
						}
					}

					if (LineThree.Contains(b) && LineThree.Contains(br) && IsJumpTwo(b, br)) { // 拆二后续
						foreach (var w in w_rounds) {
							if ((IsCusp(w, b) || IsCusp(w, br)) && LineFour.Contains(w)) {
								foreach (var e in e_rounds) {
									if (IsTouch(w, e) && LineThree.Contains(e)) {
										return e;
									}
								}
							} else if (LineThree.Contains(w)
								&& ((IsTouch(w, b) && IsJumpOne(w, br)) || (IsTouch(w, br) && IsJumpOne(w, b)))) {
								foreach (var e in e_rounds) {
									if (IsTouch(e, w) && LineTwo.Contains(e)
										&& RoundOnePoses(w).Intersect(w_rounds).Count() == 1) {
										return e;
									}
								}
							}
						}
					}

					if (GetLength(b, br) > 8 && StepCount > 30) {		// 相连
						foreach (var e in empties) {
							if (GetLength(b, e) < 5 && GetLength(br, e) < 5 
								&& RoundTwoPoses(e).Intersect(empties).Count() > 23
								&& RoundOnePoses(e).Intersect(empties).Count() == 9) {
								return e;
							}
						}
					}
				}

				foreach (var e in empties) {	// 拆二
					//  b   e		line 3
					if (LineThree.Contains(b) && LineThree.Contains(e) && IsJumpTwo(e, b) && StepCount > 10) {
						int r_off = (e.Row - b.Row) / 3;
						int c_off = (e.Col - b.Col) / 3;
						var e2 = new Pos(b.Row + 2 * r_off, b.Col + 2 * c_off);
						var re2 = RoundOnePoses(e2).Intersect(empties).ToList();
						var re = RoundOnePoses(e).Intersect(empties).ToList();
						var e3 = new Pos(b.Row + 7 * r_off, b.Col + 7 * c_off);
						var re3 = RoundThreePoses(e3);
						if (re2.Count == 9 && re.Count == 9 && re3.Intersect(blacks).Any()) {
							return e;
						}
					}
				}
			}

			// 以白为基准
			foreach (var w in whites) {
				var rounds = RoundSixPoses(w);
				var b_rounds = rounds.Intersect(blacks).ToList();
				var w_rounds = rounds.Intersect(whites).ToList();
				var e_rounds = rounds.Intersect(empties).ToList();

				if (GoldHorn.Contains(w) && StepCount < 10) {
					foreach (var e in e_rounds) {
						if (IsFlyOne(e,w) && LineThreeFour.Contains(e)
							&& RoundOnePoses(e).Intersect(e_rounds).Count() == 9
							&& RoundThreePoses(w).Intersect(w_rounds).Count() == 1) {
								return e;
						}
					}
				}

				//    0 0 
				//  +  
				//    0 * +
				foreach (var wr in w_rounds) {
					if (IsTouch(w,wr)) {
						var rs = RoundTwoPoses(w);
						var ws = rs.Intersect(w_rounds).ToList();
						var bs = rs.Intersect(b_rounds).ToList();
						var es = rs.Intersect(e_rounds).ToList();
						foreach (var w3 in ws) {
							if (IsJumpOne(w3, w) && IsFlyOne(w3, wr)) {
								if (bs.Count == 2) {
									var b = IsCusp(w, bs[0]) ? bs[0] : bs[1];
									var b2 = IsJumpOne(w3, bs[0]) ? bs[0] : bs[1];
									if (IsFlyTwo(b, b2) && IsElephantOne(b2, w)) {
										foreach (var e in es) {
											if (IsFlyOne(e, w) && IsTouch(e, b2))
												return e;
										}
									}
								}
							}
						}
					} 
					//小飞挂星后二路进角
					else if (IsFlyOne(w, wr) && StarFourHorn.Contains(w) && LineThree.Contains(wr)) {
						foreach (var b in b_rounds) {
							if (IsFlyOne(w, b) && LineThree.Contains(b)) {
								foreach (var e in copy_empties) {		// 二路问题
									if (IsFlyOne(e, b) && IsJumpOne(e, w) && LineTwo.Contains(e) 
										&& RoundOnePoses(e).Intersect(copy_empties).Count() == 9) {
											return e;
									}
								}
								// 继续进三三
								foreach (var b2 in b_rounds) {
									if (IsFlyOne(b, b2) && IsJumpOne(b2, w) && LineTwo.Contains(b2)) {
										foreach (var e in e_rounds) {
											if (ThreeThrees.Contains(e) 
												&& LinkPoses(e).Intersect(copy_empties).Count() == 5) {
												return e;
											}
										}
									}
								}
							}
						}
					}
					
				}
			}

			// 以空为基准
			foreach (var e in empties) {
				var rs = RoundOnePoses(e);
				var bs = rs.Intersect(blacks).ToList();
				var ws = rs.Intersect(whites).ToList();
				foreach (var w1 in ws) {
					foreach (var w2 in ws) {
						if (IsFlyOne(w1, w2) && IsTouch(e, w1)) { // 断飞
							foreach (var b in bs) {
								if (IsTouch(e, b) && IsCusp(w1, b) && IsTouch(w2, b)) {
									return e;
								}
							}
						}
					}
				}
			}

			#region 封口

			if (StepCount < 80) return m_InvalidPos;

			foreach (var b in blacks) {
				if (LineFour.Contains(b)) {
					if (GetBlackMeshBlockCount(b) > 10) {
						var rs = RoundOnePoses(b);
						var ws = rs.Intersect(whites).ToList();
						foreach (var w in ws) {
							if (LineThree.Contains(w) && IsCusp(w, b)) {
								foreach (var e in empties) {
									if (IsTouch(e, b) && IsTouch(e, w) && LineThree.Contains(e))
										return e;
								}
							} else if (LineThree.Contains(w) && IsFlyOne(w, b)) {
								foreach (var e in empties) {
									if (IsFlyOne(e, b) && IsCusp(e, w) && LineTwo.Contains(e))
										return e;
								}
							} else if (LineFour.Contains(w) && IsJumpOne(w, b)) {
								foreach (var e in empties) {
									if (IsCusp(e, b) && IsCusp(e, w) && LineThree.Contains(e))
										return e;
								}
							} else if (LineFour.Contains(w) && IsTouch(w, b)) {
								foreach (var e in empties) {
									if (IsJumpOne(e, b) && IsFlyOne(e, w) && LineTwo.Contains(e)
										&& RoundOnePoses(e).Intersect(empties).Count() == 3 * 3)
										return e;
								}
							}
						}
					}
				} else if (LineThree.Contains(b)) {
					if (GetBlackMeshBlockCount(b) > 10) {
						var rs = RoundOnePoses(b);
						var ws = rs.Intersect(whites).ToList();
						foreach (var w in ws) {
							//  0 +			line 3
							//	0 *
							//
							if (LineTwo.Contains(w) && IsCusp(w, b)) {
								foreach (var e in empties) {
									if (IsTouch(e, b) && IsTouch(e, w) && LineTwo.Contains(e))
										return e;
								}
							}
								//  0			
								//       +		line 3
								//  *
								//
							else if (LineFour.Contains(w) && IsFlyOne(w, b)) {
								foreach (var e in empties) {
									if (IsFlyOne(e, b) && IsJumpOne(e, w) && LineTwo.Contains(e)
										&& RoundOnePoses(e).Intersect(empties).Count() == 3 * 3)
										return e;
								}
							}
								//  0   +		line 3
								//    *
								//
							else if (LineThree.Contains(w) && IsJumpOne(w, b)) {
								foreach (var e in empties) {
									if (IsCusp(e, b) && IsCusp(e, w) && LineTwo.Contains(e)
										&& LinkPoses(e).Intersect(empties).Count() == 5)
										return e;
								}
							}
								//  0 +			line 3
								//  * *			两种可能
								//
							else if (LineThree.Contains(w) && IsTouch(w, b)) {
								foreach (var e in empties) {
									if (IsCusp(e, b) && IsTouch(e, w) && LineTwo.Contains(e)
										&& LinkPoses(e).Intersect(empties).Count() == 4) {
											return e;
									} else if (IsTouch(e, b) && IsCusp(e, w) && LineTwo.Contains(e)
										 && LinkPoses(e).Intersect(empties).Count() == 3) {
											 return e;
									}
								}
							}
						}
					}
				} else if (LineTwo.Contains(b)) {
					if (GetBlackMeshBlockCount(b) > 10) {
						var rs = RoundOnePoses(b);
						var ws = rs.Intersect(whites).ToList();
						foreach (var w in ws) {
							if (LineOne.Contains(w) && IsCusp(w, b)) {
								foreach (var e in empties) {
									if (IsTouch(e, b) && IsTouch(e, w) && LineOne.Contains(e))
										return e;
								}
							}
						}
					}
				}
			}

			#endregion

			return m_InvalidPos;
		}

		Pos Defend()
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

			#region 黑防守反击

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
					if (StarFourHorn.Contains(r) && LineThree.Contains(pos)) {	// 碰星位
						if (b_rounds.Count == 1 && w_rounds.Count == 1) {
							foreach (var e in e_rounds2) {
								if (ThreeThrees.Contains(e))
									return e;
							}
						}
					}
					foreach (var item in rounds2) {
						if (IsFlyOne(item, r) && IsJumpOne(item, pos) && whites.Contains(item)) { // 跳压
							foreach (var e in empties) {
								if (IsCusp(e, r) && IsJumpTwo(e, item) && LinkPoses(e).Intersect(empties).Count() == 4)
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
							if (IsTouch(e, r) && LineTwo.Contains(e)) {
								return e;
							}
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
								&& RoundOnePoses(e).Intersect(empties).Count() == 9)
								return e;
						}
					} else if (b_rounds2.Contains(b_col) && w_links.Contains(w_col)) {
						foreach (var e in e_rounds2) {
							if (IsFlyOne(e, r) && IsFlyOne(e, pos) && IsFlyTwo(e, b_col)
								&& RoundOnePoses(e).Intersect(empties).Count() == 9)
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
							&& RoundOnePoses(r).Intersect(copyEmpties).Count() == 8) {
							return e;
						}
					}
				}

				// 飞
				if (IsFlyOne(r, pos)) {
					foreach (var e in empties) {
						if (IsJumpOne(e, r) && IsTouch(e, pos) && RoundOnePoses(e).Intersect(empties).Count() == 8)
							return e;
						else if (IsFlyOne(e, r) && IsCusp(e, pos) && RoundOnePoses(e).Intersect(empties).Count() == 8)
							return e;
						else if (IsJumpOne(e, r) && IsFlyOne(e, pos) && RoundOnePoses(e).Intersect(empties).Count() == 9)
							return e;
					}
				}
			}

			#endregion

			return m_InvalidPos;
		}

		private void AddThinkPos(Pos e, int worth = 20)
		{
			Pos p = e;
			p.Worth = worth;
			m_ThinkPoses.Add(p);
		}
		void AddIncreaseThinkPos(Pos e, int worth = 0)
		{
			e.Worth = IncreaseWorth() + worth;
			m_ThinkPoses.Add(e);
		}
		private void AddDecreaseThinkPos(Pos e, int worth = 0)
		{
			e.Worth = DecreaseWorth() + worth;
			m_ThinkPoses.Add(e);
		}
		int DecreaseWorth()
		{
			if (StepCount < 30) {
				return 10;
			} else if (StepCount < 100) {
				return 5;
			} else if (StepCount < 150) {
				return 2;
			} else {
				return 1;
			}
		}
		int IncreaseWorth()
		{
			if (StepCount < 30) {
				return 1;
			} else if (StepCount < 50) {
				return 2;
			} else if (StepCount < 120) {
				return 5;
			} else {
				return 10;
			}
		}
		int GetLength(Pos p1, Pos p2)
		{
			return (int)new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length;
		}
		int GetBlackMeshBlockCount(Pos p)
		{
			UpdateAllMeshBlocks();
			foreach (var block in m_BlackMeshBlocks) {
				if (block.Poses.Contains(p)) {
					return block.Poses.Count;
				}
			}
			return -1;
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
