/**
 * Board.Shape.cs (c) 2015 by x01
 * -----------------------------
 *  1.虽说没有全局观念是不可能下出好棋的，但还是决定先解决基本的棋形处理。
 *  2.全局扫描，根据相互位置进行处理.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Core;

namespace x01.Weiqi.Boards
{
	partial class Board
	{
		Pos TwoStar()
		{
			var blacks = BlackPoses;
			var whites = WhitePoses;
			if (blacks.Count == 1 && whites.Count == 1) { // 二连星
				var b = blacks[0];
				var w = whites[0];
				if (GetLength(b, w) > 7) {
					foreach (var star in StarFourHorn) {
						if (IsJumpX(star, b, 11) && GetLength(star, w) > 7)
							return star;
					}
				}
			}
			return m_InvalidPos;
		}

//		Pos Attack()
//		{
//			var blacks = BlackPoses;
//			var whites = WhitePoses;
//			var empties = EmptyPoses;
//			var copy_empties = RemoveEmpties(empties, blacks);
//
//			// 以黑为基准
//			foreach (var b in blacks) {
//				var rounds = RoundSixPoses(b);
//				var b_rounds = rounds.Intersect(blacks).ToList();
//				var w_rounds = rounds.Intersect(whites).ToList();
//				var e_rounds = rounds.Intersect(empties).ToList();
//
//				foreach (var br in b_rounds) {
//
//					#region 尖形
//
//					if (IsCusp(b, br)) {
//						//   + *		+ 为黑子
//						// + 0			0 为白子
//						//   0			* 为将下之子
//						if (StarFourHorn.Contains(b) && ThreeThrees.Contains(br)) {	// 三三扳立
//							var rs = RoundPoses(b);
//							var bs = rs.Intersect(b_rounds).ToList();
//							var ws = rs.Intersect(w_rounds).ToList();
//							var es = rs.Intersect(e_rounds).ToList();
//							if (bs.Count == 2 && ws.Count == 2 && IsTouch(ws[0], ws[1])) {
//								var w = IsTouch(b, ws[0]) ? ws[0] : ws[1];
//								foreach (var e in copy_empties) {
//									if (LineTwo.Contains(e) && IsCusp(e, w) && IsFlyOne(e, b) && IsTouch(e, br)) {
//										return e;
//									}
//								}
//							}
//						}
//							//   0 + 
//							//   0 +
//							// 0 +
//							//   *
//						else if (StarFourHorn.Contains(b) && LineThree.Contains(br)) {
//							var rs = RoundTwoPoses(b);
//							var bs = rs.Intersect(blacks).ToList();
//							var ws = rs.Intersect(whites).ToList();
//							var es = rs.Intersect(copy_empties).ToList();
//							if (bs.Count == 3 && ws.Count == 3) {
//								Pos w3 = m_InvalidPos;
//								Pos w2 = m_InvalidPos;
//								Pos w1 = m_InvalidPos;
//								foreach (var w in ws) {
//									if (IsCusp(b, w) && ThreeThrees.Contains(w))
//										w3 = w;
//									else if (IsTouch(b, w) && AllEyes.Contains(w))
//										w2 = w;
//									else if (IsFlyOne(b, w) && LineTwo.Contains(w))
//										w1 = w;
//								}
//								if (w1 != m_InvalidPos && w2 != m_InvalidPos && w3 != m_InvalidPos) {
//									if (IsTouch(w1, br) && IsCusp(w1, w2)) {
//										Pos b3 = m_InvalidPos;
//										foreach (var temp in bs) {
//											if (IsTouch(temp, b))
//												b3 = temp;
//										}
//										foreach (var e in es) {
//											if (IsFlyTwo(e, b3) && IsTouch(e, br))
//												return e;
//										}
//									}
//								}
//							}
//						}
//
//						//    0				 w0
//						//	+ 0   0	  ==   b wr   w1
//						//    + *			 br *	
//						foreach (var wr in w_rounds) {
//							if (IsTouch(b, wr) && IsTouch(br, wr)) {
//								var wr2 = RoundTwoPoses(wr).Intersect(w_rounds).ToList();
//								var er2 = RoundTwoPoses(wr).Intersect(e_rounds).ToList();
//								var copy_wr2 = wr2.ToList();
//								wr2.Remove(wr);
//								var br2 = RoundTwoPoses(wr).Intersect(b_rounds).ToList();
//								if (wr2.Count == 2 && br2.Count == 2) {
//									var w0 = IsTouch(wr, wr2[0]) ? wr2[0] : wr2[1];
//									var w1 = IsJumpOne(wr, wr2[0]) ? wr2[0] : wr2[1];
//									if (IsCusp(b, w0) && IsJumpTwo(b, w1)) {
//										foreach (var e in er2) {
//											if (IsTouch(br, e) && IsCusp(w1, e)) { // 扳长
//												return e;
//											}
//										}
//									}
//								}
//									//     0		
//									//	 + 0   0
//									// *   + + 0
//								else if (copy_wr2.Count == 4 && br2.Count == 3) {
//									br2.Remove(b); br2.Remove(br);
//									int r_off = br2[0].Row - br.Row;
//									int c_off = br2[0].Col - br.Col;
//									Pos p = new Pos(br2[0].Row + r_off, br2[0].Col + c_off);
//									if (IsFlyOne(p, wr)) {
//										r_off = wr.Row - br.Row;
//										c_off = wr.Col - br.Col;
//										p = new Pos(wr.Row + r_off, wr.Col + c_off);
//										if (IsCusp(b, p)) {
//											r_off = wr.Row - b.Row;
//											c_off = wr.Col - b.Col;
//											p = new Pos(wr.Row + 2 * r_off, wr.Col + 2 * c_off);
//											if (IsJumpOne(p, wr)) {
//												foreach (var e in e_rounds) {
//													if (IsCusp(e, b) && IsFlyX(e, p, 3)) {  // 虎
//														return e;
//													}
//												}
//											}
//										}
//									}
//								}
//							}
//						}
//						//    0 +
//						//  0   0 +
//						//      *
//						var b_rs = RoundPoses(b);
//						var b_ws = b_rs.Intersect(w_rounds).ToList();
//						if (b_ws.Count == 2 && IsCusp(b_ws[0], b_ws[1])) {
//							var w = (IsTouch(br, b_ws[0]) && IsFlyOne(br, b_ws[1])) ? b_ws[0] : b_ws[1];
//							var w2 = w == b_ws[0] ? b_ws[1] : b_ws[0];
//							int r_off = w.Row - br.Row;
//							int c_off = w.Col - br.Col;
//							var w3 = new Pos(w.Row + 2 * r_off, w.Col + c_off * 2);
//							if (IsCusp(w3, w2) && IsFlyOne(w3, b)) {
//								foreach (var e in copy_empties) {
//									if (IsTouch(e, w) && IsJumpOne(e, b)
//										&& LinkPoses(e).Intersect(copy_empties).Count() > 2)
//										return e;
//								}
//							}
//						}
//							//    0 + *
//							//    0 0 +
//						 else if (b_ws.Count == 3 && LinkPoses(b).Intersect(whites).Count() == 2) {
//							var links = LinkPoses(b).Intersect(whites).ToList();
//							if (IsCusp(links[0], links[1])) {
//								foreach (var w in b_ws) {
//									if (IsCusp(b, w) && IsTouch(w, links[0]) && IsTouch(w, links[1]) && IsJumpOne(br, w)) {
//										foreach (var e in b_rs) {
//											if (e_rounds.Contains(e) && IsTouch(e, b) && IsTouch(e, br) && IsFlyOne(e, w))
//												return e;
//										}
//									}
//								}
//							}
//						}
//					}
//
//					#endregion
//
//					#region 碰形
//						// Touch
//					 else if (IsTouch(b, br)) {
//						if (IsTouch(b, br)) {	// 加个包装，防止局部变量冲突
//							//  + 0
//							//  + 0
//							//    *
//							if (RoundPoses(b).Intersect(whites).Count() == 2) {
//								var ws = RoundPoses(b).Intersect(whites).ToList();
//								var w0 = IsTouch(b, ws[0]) ? ws[0] : ws[1];
//								var w1 = w0 == ws[0] ? ws[1] : ws[0];
//								if (IsTouch(w0, w1) && IsCusp(w0, br) && GetStep(b).EmptyCount >= GetStep(w0).EmptyCount) {
//									foreach (var e in e_rounds) {
//										if (IsTouch(e, w0) && IsCusp(b, e) && LinkPoses(e).Intersect(copy_empties).Count() == 3)
//											return e;
//									}
//								}
//							}
//							//    0
//							//  0 +    *
//							//    +
//							if (LineCenter.Contains(b)) {
//								var ws = LinkPoses(b).Intersect(w_rounds).ToList();
//								if (ws.Count == 2 && IsCusp(ws[0], ws[1])) {
//									var w = IsCusp(ws[0], br) ? ws[0] : ws[1];
//									int r_off = b.Row - w.Row;
//									int c_off = b.Col - w.Col;
//									var p = new Pos(b.Row + 2 * r_off, b.Col + 2 * c_off);
//									if (e_rounds.Contains(p) && RoundPoses(p).Intersect(e_rounds).Count() == 9) {
//										return p;
//									}
//								}
//							}
//							//    *
//							//  0
//							//  0 + +
//							var link_w_pos = new Pos(b.Row + (b.Row - br.Row), b.Col + (b.Col - br.Col));
//							if (w_rounds.Contains(link_w_pos)) {
//								var ws = LinkPoses(link_w_pos).Intersect(w_rounds).ToList();
//								if (ws.Count == 2 && IsTouch(ws[0], ws[1])) {
//									var w = IsTouch(b, ws[0]) ? ws[1] : ws[0];
//									if (IsFlyOne(w, br)) {
//										foreach (var e in copy_empties) {
//											if (IsJumpOne(e, b) && IsCusp(e, w)
//												&& LinkPoses(e).Intersect(copy_empties).Count() == 5) {
//												return e;
//											}
//										}
//									}
//								}
//							}
//						}
//						var rs = RoundPoses(b);
//						var rbs = rs.Intersect(b_rounds).ToList();
//						var rws = rs.Intersect(w_rounds).ToList();
//						var res = rs.Intersect(e_rounds).ToList();
//						var links = LinkPoses(b);
//						var lws = links.Intersect(w_rounds).ToList();
//						var lbs = links.Intersect(b_rounds).ToList();
//						//    +		
//						//	0 +   +
//						//    0 0 *	
//						if (lws.Count == 2 && IsCusp(lws[0], lws[1]) && lbs.Count == 2 && lbs.Contains(br)) {
//							var w = IsJumpOne(lws[0], br) ? lws[0] : lws[1];
//							var w2 = w == lws[0] ? lws[1] : lws[0];
//							int r_off = b.Row - w2.Row;
//							int c_off = b.Col - w2.Col;
//							var b3 = new Pos(b.Row + 2 * r_off, b.Col + 2 * c_off);
//							if (b_rounds.Contains(b3)) {
//								var rb3 = RoundPoses(b3);
//								var rw3s = rb3.Intersect(w_rounds).ToList();
//								if (rw3s.Count == 1) {
//									if (IsTouch(rw3s[0], w) && IsCusp(rw3s[0], b3)) {
//										foreach (var e in rb3) {
//											if (e_rounds.Contains(e)) {
//												if (IsTouch(e, b3) && IsTouch(rw3s[0], e) && IsFlyOne(e, b))
//													return e;
//											}
//										}
//									}
//								}
//							}
//						}
//							//    +	0 	
//							//	  + 0  
//							//  * 0 
//						else if (rws.Count == 3 && rbs.Count == 2) {
//							foreach (var rw in rws) {
//								if (IsTouch(rw, b)) {
//									foreach (var w2 in rws) {
//										if (IsTouch(w2, br) && IsTouch(w2, rw)) {
//											foreach (var w3 in rws) {
//												if (IsTouch(b, w3) && IsCusp(rw, w3)
//													&& LineThree.Contains(b) && LineThree.Contains(br)) {
//													foreach (var e in res) {
//														if (IsCusp(e, b) && IsElephantOne(e, w2)) {
//															return e;
//														}
//													}
//												}
//													//      + 0 	
//													//	*   + 0  
//													//      0 
//												else if (IsTouch(b, w3) && IsCusp(rw, w3) && !(LineThree.Contains(b) || LineThree.Contains(br))) {
//													int r_off = b.Row - rw.Row;
//													int c_off = b.Col - rw.Col;
//													var p = new Pos(b.Row + 2 * r_off, b.Col + 2 * c_off);
//													if (e_rounds.Contains(p))
//														return p;
//												}
//											}
//										}
//									}
//								}
//							}
//						}
//					}
//					#endregion
//
//					#region 跳形
//
//					//   +   +
//					//     * 0
//					if (IsJumpOne(b, br)) {
//						var rs = RoundPoses(b);
//						var ws = rs.Intersect(w_rounds).ToList();
//						var es = rs.Intersect(e_rounds).ToList();
//						if (ws.Count == 1 && IsTouch(ws[0], b)) {
//							foreach (var e in es) {
//								if (IsCusp(e, b) && IsCusp(e, br) && IsTouch(ws[0], e)) {
//									return e;
//								}
//									//      0
//									//
//									//  +   + 0
//									//        *
//								else if (IsCusp(e, b) && IsFlyTwo(e, br)
//									&& RoundPoses(e).Intersect(whites).Count() == 1) {
//									return e;
//								}
//							}
//						}
//					}
//
//					#endregion
//
//					#region 飞形
//
//					//  +   0 
//					//    * + 0
//					if (IsFlyOne(b, br)) {
//						var rs = RoundPoses(b);
//						var ws = rs.Intersect(w_rounds).ToList();
//						var es = rs.Intersect(e_rounds).ToList();
//						if (ws.Count == 2 && IsCusp(ws[0], ws[1])) {
//							var w = IsTouch(b, ws[0]) ? ws[0] : ws[1];
//							if (IsJumpOne(br, w)) {
//								foreach (var e in es) {
//									if (IsCusp(e, br) && IsTouch(e, b))
//										return e;
//								}
//							}
//						}
//					} else if (IsFlyTwo(b, br)) {
//						//  +     0 0
//						//      * +
//						var rs = RoundPoses(b);
//						var ws = rs.Intersect(w_rounds).ToList();
//						var es = rs.Intersect(e_rounds).ToList();
//						if (ws.Count == 2 && IsTouch(ws[0], ws[1])) {
//							var w = IsTouch(b, ws[0]) ? ws[0] : ws[1];
//							if (IsJumpTwo(br, w)) {
//								foreach (var e in es) {
//									if (IsCusp(e, w) && IsFlyOne(e, br)
//										&& LinkPoses(e).Intersect(e_rounds).Count() == 4) {
//										return e;
//									}
//								}
//							}
//						}
//					}
//
//					#endregion
//
//					// 小飞后拆二
//					if (IsFlyOne(b, br) && LineThree.Contains(b) && LineTwo.Contains(br)) {
//						foreach (var e in e_rounds) {
//							if (IsJumpTwo(b, e) && !IsCusp(e, br) && LineThree.Contains(e)
//								&& RoundTwoPoses(e).Intersect(copy_empties).Count() == 5 * 5) {
//								return e;
//							}
//						}
//					}
//
//					if (LineThree.Contains(b) && LineThree.Contains(br) && IsJumpTwo(b, br)) { // 拆二后续
//						foreach (var w in w_rounds) {
//							if ((IsCusp(w, b) || IsCusp(w, br)) && LineFour.Contains(w)) {
//								foreach (var e in e_rounds) {
//									if (IsTouch(w, e) && LineThree.Contains(e)) {
//										return e;
//									}
//								}
//							} else if (LineThree.Contains(w)
//								&& ((IsTouch(w, b) && IsJumpOne(w, br)) || (IsTouch(w, br) && IsJumpOne(w, b)))) {
//								foreach (var e in e_rounds) {
//									if (IsTouch(e, w) && LineTwo.Contains(e)
//										&& RoundPoses(w).Intersect(w_rounds).Count() == 1) {
//										return e;
//									}
//								}
//							}
//						}
//					}
//
//					if (GetLength(b, br) > 8 && StepCount > 30) {		// 相连
//						foreach (var e in empties) {
//							if (GetLength(b, e) < 5 && GetLength(br, e) < 5
//								&& RoundTwoPoses(e).Intersect(copy_empties).Count() > 23
//								&& RoundPoses(e).Intersect(copy_empties).Count() == 9) {
//								return e;
//							}
//						}
//					} else if (GetLength(b, br) > 5) {
//						var area = GetArea(b, br);
//						var es = area.Intersect(copy_empties).ToList();
//						if (es.Count == area.Count - 2) {
//							foreach (var e in es) {
//								if (IsFlyTwo(e, b) && RoundPoses(e).Intersect(copy_empties).Count() == 9) {
//									return e;
//								}
//							}
//						}
//					} else if (GetLength(b, br) > 4) {
//						foreach (var e in copy_empties) {
//							if (IsFlyOne(e, b) && RoundPoses(e).Intersect(copy_empties).Count() == 9) {
//								return e;
//							} else if (IsJumpOne(e, b) && RoundPoses(e).Intersect(copy_empties).Count() == 9) {
//								return e;
//							} else if (IsJumpTwo(e, b) && RoundPoses(e).Intersect(copy_empties).Count() == 9) {
//								return e;
//							}
//						}
//					}
//				}
//
//				foreach (var e in empties) {	// 拆二
//					//  b   e		line 3
//					if (LineThree.Contains(b) && LineThree.Contains(e) && IsJumpTwo(e, b) && StepCount > 10) {
//						int r_off = (e.Row - b.Row) / 3;
//						int c_off = (e.Col - b.Col) / 3;
//						var e2 = new Pos(b.Row + 2 * r_off, b.Col + 2 * c_off);
//						var re2 = RoundPoses(e2).Intersect(empties).ToList();
//						var re = RoundPoses(e).Intersect(empties).ToList();
//						var e3 = new Pos(b.Row + 7 * r_off, b.Col + 7 * c_off);
//						var re3 = RoundThreePoses(e3);
//						if (re2.Count == 9 && re.Count == 9 && re3.Intersect(blacks).Any()) {
//							return e;
//						}
//					}
//				}
//
//				// 小飞
//				if (LineThreeFour.Contains(b) && StepCount > 50) {
//					foreach (var e in e_rounds) {
//						if (IsFlyOne(e, b) && RoundTwoPoses(e).Intersect(e_rounds).Count() == 5 * 5) {
//							return e;
//						}
//					}
//				} else if (GoldHorn.Contains(b) && StepCount > 7) {
//					foreach (var e in e_rounds) {
//						if (IsFlyOne(e, b) && LineThreeFour.Contains(e)
//							&& RoundPoses(e).Intersect(e_rounds).Count() == 9
//							&& RoundThreePoses(b).Intersect(b_rounds).Count() == 1) {
//							return e;
//						}
//					}
//				} else if (LineThreeFour.Contains(b) && StepCount > 30 
//					&& RoundPoses(b).Intersect(copy_empties).Count() == 8) {
//					foreach (var e in e_rounds) {
//						if (LineFour.Contains(b) && IsFlyTwo(e, b) && LineThree.Contains(e) 
//							&& RoundPoses(e).Intersect(copy_empties).Count() == 9) {
//							return e;
//						} else if (LineThree.Contains(b) && IsJumpTwo(e,b) && LineThree.Contains(e)
//							&& RoundPoses(e).Intersect(copy_empties).Count() == 9) {
//							return e;
//						}
//					}
//				}
//			}
//
//			// 以白为基准
//			foreach (var w in whites) {
//				var rounds = RoundSixPoses(w);
//				var b_rounds = rounds.Intersect(blacks).ToList();
//				var w_rounds = rounds.Intersect(whites).ToList();
//				var e_rounds = rounds.Intersect(empties).ToList();
//
//				if (GoldHorn.Contains(w)) {  // 挂白角
//					foreach (var e in e_rounds) {
//						if (StarFourHorn.Contains(w) && IsFlyOne(e, w) && LineThreeFour.Contains(e)
//							&& RoundPoses(e).Intersect(copy_empties).Count() == 9
//							&& RoundThreePoses(w).Intersect(w_rounds).Count() == 1) {
//							return e;
//						} else if (AllEyes.Contains(w) && IsJumpOne(e, w) && AllEyes.Contains(e)
//								   && RoundPoses(e).Intersect(copy_empties).Count() == 9) {
//							return e;
//						} else if (ThreeThrees.Contains(w) && StarFourHorn.Contains(e) && IsCusp(e, w)
//								   && LinkPoses(e).Intersect(empties).Count() == 5) {
//							return e;
//						}
//					}
//				}
//
//				
//				foreach (var wr in w_rounds) {
//					// 碰形
//					if (IsTouch(w, wr)) {
//						var rs = RoundTwoPoses(w);
//						var ws = rs.Intersect(w_rounds).ToList();
//						var bs = rs.Intersect(b_rounds).ToList();
//						var es = rs.Intersect(e_rounds).ToList();
//						foreach (var w3 in ws) {
//							if (IsJumpOne(w3, w) && IsFlyOne(w3, wr)) {
//								//    0 0 
//								//  +  
//								//    0 * +
//								if (bs.Count == 2) {
//									var b = IsCusp(w, bs[0]) ? bs[0] : bs[1];
//									var b2 = IsJumpOne(w3, bs[0]) ? bs[0] : bs[1];
//									if (IsFlyTwo(b, b2) && IsElephantOne(b2, w)) {
//										foreach (var e in es) {
//											if (IsFlyOne(e, w) && IsTouch(e, b2))
//												return e;
//										}
//									}
//								}
//							}
//						}
//
//						if (ThreeThrees.Contains(w)) {
//							//  0
//							//  0 +
//							//    *
//							var rbs = RoundTwoPoses(w).Intersect(blacks).ToList();
//							if (rbs.Count == 1 && StarFourHorn.Contains(rbs[0])) {
//								if (IsTouch(rbs[0], wr)) {
//									foreach (var e in e_rounds) {
//										if (IsFlyOne(e, w) && IsCusp(e, wr) && IsTouch(e, rbs[0]))
//											return e;
//									}
//								}
//							}
//						}
//					} 
//					// 飞形
//					else if (IsFlyOne(w,wr) && AllEyes.Contains(w)) {
//                        //  * 0             line 3
//                        //         0
//                        //    +
//                        var bs = RoundTwoPoses(w).Intersect(blacks).ToList();
//                        if (bs.Count == 1 && AllEyes.Contains(bs[0])) {
//                            foreach (var e in empties) {
//								if (IsTouch(e, w) && IsFlyOne(e, bs[0]) && IsFlyTwo(e, wr))
//									return e;
//                            }            
//                        }
//                    } 
//					// 尖形
//					else if (IsCusp(w,wr) && AllEyes.Contains(w)) {
//                        //    0   +
//                        //  0 + *
//                        //
//                        var bs = RoundTwoPoses(w).Intersect(blacks).ToList();
//                        if (bs.Count == 2 && IsFlyOne(bs[0],bs[1])) {
//                            var b0 = IsTouch(w,bs[0]) ? bs[0] : bs[1];
//                            var b1 = b0 == bs[0] ? bs[1] : bs[0];
//                            if (IsFlyTwo(wr,b1)){
//                                foreach (var e in e_rounds) {
//									if (IsTouch(e, b0) && IsCusp(e, w))
//										return e;
//                                }
//                            }
//                        }
//                    } 
//                
//						//小飞挂星后二路进角
//					else if (IsFlyOne(w, wr) && StarFourHorn.Contains(w) && LineThree.Contains(wr)) {
//						foreach (var b in b_rounds) {
//							if (IsFlyOne(w, b) && LineThree.Contains(b)) {
//								foreach (var e in copy_empties) {		// 二路问题
//									if (IsFlyOne(e, b) && IsJumpOne(e, w) && LineTwo.Contains(e)
//										&& RoundPoses(e).Intersect(copy_empties).Count() == 9) {
//										return e;
//									}
//								}
//								// 继续进三三
//								foreach (var b2 in b_rounds) {
//									if (IsFlyOne(b, b2) && IsJumpOne(b2, w) && LineTwo.Contains(b2)) {
//										foreach (var e in e_rounds) {
//											if (ThreeThrees.Contains(e)
//												&& LinkPoses(e).Intersect(copy_empties).Count() == 5) {
//												return e;
//											}
//										}
//									}
//								}
//							}
//						}
//					}
//						//  0 * 0
//						//      +   0		line 3
//						//
//					else if (LineFour.Contains(w) && LineThree.Contains(wr) && IsFlyX(w, wr, 3)) {
//						var bs = GetArea(w, wr).Intersect(blacks).ToList();
//						if (bs.Count == 1 && IsFlyOne(w, bs[0]) && IsJumpOne(bs[0], wr)) {
//							var ws = LinkPoses(bs[0]).Intersect(whites).ToList();
//							if (ws.Count == 1 && IsJumpOne(ws[0], w) && IsFlyOne(ws[0], wr)) {
//								foreach (var e in empties) {
//									if (IsCusp(e, bs[0]) && IsTouch(e, w) && IsTouch(e, ws[0])) {
//										return e;
//									}
//								}
//							}
//						}
//					}
//				}
//			}
//
//			// 以空为基准
//			foreach (var e in empties) {
//				var rs = RoundPoses(e);
//				var bs = rs.Intersect(blacks).ToList();
//				var ws = rs.Intersect(whites).ToList();
//				foreach (var w1 in ws) {
//					foreach (var w2 in ws) {
//						if (IsFlyOne(w1, w2) && IsTouch(e, w1)) { // 断飞
//							foreach (var b in bs) {
//								if (IsTouch(e, b) && IsCusp(w1, b) && IsTouch(w2, b)) {
//									return e;
//								}
//							}
//						}
//					}
//				}
//			}
//
//			#region 封口
//
//			if (StepCount < 64) return Helper.InvalidPos;
//
//			foreach (var b in blacks) {
//				if (LineFour.Contains(b)) {
//					if (GetBlackMeshBlockCount(b) > 10) {
//						var rs = RoundPoses(b);
//						var ws = rs.Intersect(whites).ToList();
//						foreach (var w in ws) {
//							if (LineThree.Contains(w) && IsCusp(w, b)) {
//								foreach (var e in copy_empties) {
//									if (IsTouch(e, b) && IsTouch(e, w) && LineThree.Contains(e))
//										return e;
//								}
//							} else if (LineThree.Contains(w) && IsFlyOne(w, b)) {
//								foreach (var e in copy_empties) {
//									if (IsFlyOne(e, b) && IsCusp(e, w) && LineTwo.Contains(e))
//										return e;
//								}
//							} else if (LineFour.Contains(w) && IsJumpOne(w, b)) {
//								foreach (var e in copy_empties) {
//									if (IsCusp(e, b) && IsCusp(e, w) && LineThree.Contains(e))
//										return e;
//								}
//							} else if (LineFour.Contains(w) && IsTouch(w, b)) {
//								foreach (var e in copy_empties) {
//									if (IsJumpOne(e, b) && IsFlyOne(e, w) && LineTwo.Contains(e)
//										&& RoundPoses(e).Intersect(copy_empties).Count() == 3 * 3)
//										return e;
//								}
//							}
//						}
//					}
//				} else if (LineThree.Contains(b)) {
//					if (GetBlackMeshBlockCount(b) > 10) {
//						var rs = RoundPoses(b);
//						var ws = rs.Intersect(whites).ToList();
//						foreach (var w in ws) {
//							//  0 +			line 3
//							//	0 *
//							//
//							if (LineTwo.Contains(w) && IsCusp(w, b)) {
//								foreach (var e in copy_empties) {
//									if (IsTouch(e, b) && IsTouch(e, w) && LineTwo.Contains(e))
//										return e;
//								}
//							}
//							//  0			
//							//       +		line 3
//							//  *
//							//
//							else if (LineFour.Contains(w) && IsFlyOne(w, b)) {
//								foreach (var e in copy_empties) {
//									if (IsFlyOne(e, b) && IsJumpOne(e, w) && LineTwo.Contains(e)
//										&& RoundPoses(e).Intersect(copy_empties).Count() == 3 * 3)
//										return e;
//								}
//							}
//							//  0   +		line 3
//							//    *
//							//
//							else if (LineThree.Contains(w) && IsJumpOne(w, b)) {
//								foreach (var e in copy_empties) {
//									if (IsCusp(e, b) && IsCusp(e, w) && LineTwo.Contains(e)
//										&& LinkPoses(e).Intersect(copy_empties).Count() == 5)
//										return e;
//								}
//							}
//							//  0 +			line 3
//							//  * *			两种可能
//							//
//							else if (LineThree.Contains(w) && IsTouch(w, b)) {
//								foreach (var e in copy_empties) {
//									if (IsCusp(e, b) && IsTouch(e, w) && LineTwo.Contains(e)
//										&& LinkPoses(e).Intersect(copy_empties).Count() == 4) {
//										return e;
//									} else if (IsTouch(e, b) && IsCusp(e, w) && LineTwo.Contains(e)
//										 && LinkPoses(e).Intersect(copy_empties).Count() == 3) {
//										return e;
//									}
//								}
//							}
//						}
//					}
//				} else if (LineTwo.Contains(b)) {
//					if (GetBlackMeshBlockCount(b) > 10) {
//						var rs = RoundPoses(b);
//						var ws = rs.Intersect(whites).ToList();
//						foreach (var w in ws) {
//							if (LineOne.Contains(w) && IsCusp(w, b)) {
//								foreach (var e in copy_empties) {
//									if (IsTouch(e, b) && IsTouch(e, w) && LineOne.Contains(e))
//										return e;
//								}
//							}
//						}
//					}
//				}
//			}
//
//			#endregion
//
//			return Helper.InvalidPos;
//		}
//
//		Pos Defend()
//		{
//			var pos = CurrentPos;
//
//			var empties = CurrentEffectEmptyPoses;
//			var blacks = CurrentEffectBlackPoses;
//			var whites = CurrentEffectWhitePoses;
//			var all = CurrentEffectAllPoses;
//
//			var copyEmpties = RemoveEmpties(empties, blacks);
//
//			var rounds = RoundPoses(pos);
//			var b_rounds = rounds.Intersect(blacks).ToList();
//			var w_rounds = rounds.Intersect(whites).ToList();
//			var e_rounds = rounds.Intersect(empties).ToList();
//
//			var rounds2 = RoundTwoPoses(pos);
//			var b_rounds2 = rounds2.Intersect(blacks).ToList();
//			var w_rounds2 = rounds2.Intersect(whites).ToList();
//			var e_rounds2 = rounds2.Intersect(empties).ToList();
//
//			var links = LinkPoses(pos);
//			var b_links = links.Intersect(blacks).ToList();
//			var w_links = links.Intersect(whites).ToList();
//			var e_links = links.Intersect(empties).ToList();
//
//			#region 黑防守反击
//
//			foreach (var r in b_rounds) {
//				foreach (var l in b_links) {
//					if (IsFlyOne(r, l)) {	// 跨断飞
//						foreach (var e in e_links) {
//							if (IsTouch(e, r) && IsCusp(e, l))
//								return e;
//						}
//					}
//				}
//			}
//
//			foreach (var r in b_rounds2) {
//				// 碰
//				if (IsTouch(r, pos)) {
//					if (StarFourHorn.Contains(r) && LineThree.Contains(pos)) {	// 碰星位
//						if (b_rounds.Count == 1 && w_rounds.Count == 1) {
//							foreach (var e in e_rounds2) {
//								if (ThreeThrees.Contains(e))
//									return e;
//							}
//						}
//					}
//					foreach (var item in rounds2) {
//						if (IsFlyOne(item, r) && IsJumpOne(item, pos) && whites.Contains(item)
//							&& RoundPoses(item).Intersect(empties).Count() == 8) { // 跳压
//							foreach (var e in empties) {
//								if (IsCusp(e, r) && IsTouch(e, pos) && IsJumpTwo(e, item)
//									&& LinkPoses(e).Intersect(empties).Count() == 4) {
//									return e;
//								}
//							}
//						} else if (IsJumpOne(item, r) && IsFlyOne(item, pos) && whites.Contains(item)
//								   && RoundPoses(item).Intersect(empties).Count() == 8) { // 飞压
//							foreach (var e in empties) {
//								if (IsCusp(e, r) && IsTouch(e, pos) && IsFlyTwo(e, item))
//									return e;
//							}
//						} else if (IsTouch(item, pos) && IsJumpOne(item, r) && blacks.Contains(item)) { // 防止穿断
//							foreach (var e in copyEmpties) {
//								if (IsCusp(e, item) && IsCusp(e, r) && IsTouch(e, pos))
//									return e;
//							}
//						}
//					}
//
//					if (LineTwo.Contains(pos) && LineThree.Contains(r) && GetStep(pos).EmptyCount == 3) { // 二路扳粘
//						foreach (var e in copyEmpties) {
//							if (IsCusp(e, r) && LineTwo.Contains(e) && IsTouch(e, pos)
//								&& LinkPoses(e).Intersect(WhitePoses).Count() == 1) {
//								return e;
//							}
//						}
//					}
//					foreach (var e in e_rounds) {
//						if (GetStep(r).EmptyCount >= GetStep(pos).EmptyCount) {
//							if (IsCusp(e, r) && IsTouch(e, pos) && LinkPoses(e).Intersect(w_rounds).Count() == 1)
//								return e;
//						}
//						if (GetStep(r).EmptyCount <= GetStep(pos).EmptyCount) {
//							if (IsCusp(e, pos) && IsTouch(e, r))
//								return e;
//						}
//					}
//				}
//
//				// 尖
//				if (IsCusp(r, pos)) {
//					if (LineTwo.Contains(pos)) {
//						foreach (var e in copyEmpties) {
//							if (IsTouch(e, r) && LineTwo.Contains(e)) {
//								return e;
//							}
//						}
//					}
//					foreach (var item in rounds2) { // 靠压之继续
//						Pos w_fly = m_InvalidPos, w_touch = m_InvalidPos;
//						foreach (var w in w_rounds2) {
//							if (IsTouch(w, pos) && IsTouch(w, r))
//								w_touch = w;
//							else if (IsFlyOne(w, pos) && IsJumpTwo(w, r))
//								w_fly = w;
//						}
//						Pos b_cusp = m_InvalidPos;
//						foreach (var b in b_rounds2) {
//							if (IsJumpOne(b, pos) && IsCusp(b, r) && IsFlyOne(b, w_fly))
//								b_cusp = b;
//						}
//						foreach (var e in empties) {
//							if (IsFlyOne(e, pos) && IsCusp(e, w_fly) && IsTouch(e, b_cusp))
//								return e;
//						}
//
//					}
//
//					//	+ +
//					//	  0 0	后推车则飞之
//					int r_off = pos.Row - r.Row;
//					int c_off = pos.Col - r.Col;
//					var b_row = new Pos(r.Row - r_off, r.Col);
//					var w_row = new Pos(pos.Row - r_off, pos.Col);
//					var b_col = new Pos(r.Row, r.Col - c_off);
//					var w_col = new Pos(pos.Row, pos.Col - c_off);
//					if (b_rounds2.Contains(b_row) && w_links.Contains(w_row)) {
//						foreach (var e in e_rounds2) {
//							if (IsFlyOne(e, r) && IsFlyOne(e, pos) && IsFlyTwo(e, b_row)
//								&& RoundPoses(e).Intersect(empties).Count() == 9)
//								return e;
//						}
//					} else if (b_rounds2.Contains(b_col) && w_links.Contains(w_col)) {
//						foreach (var e in e_rounds2) {
//							if (IsFlyOne(e, r) && IsFlyOne(e, pos) && IsFlyTwo(e, b_col)
//								&& RoundPoses(e).Intersect(empties).Count() == 9)
//								return e;
//						}
//					}
//
//					foreach (var e in e_links) {
//						if (LinkPoses(e).Intersect(b_rounds2).Count() == 2)	// 尖刺优先
//							return e;
//					}
//					foreach (var e in e_links) {
//						if (IsTouch(e, r) && IsTouch(e, pos)
//							&& LinkPoses(r).Intersect(b_rounds2).Count() == 1)
//							return e;
//					}
//				}
//
//				// 跳
//				if (IsJumpOne(r, pos)) {
//					if (LineTwo.Contains(r) && LineTwo.Contains(pos)) {	// 扳粘之后续
//						var erounds = RoundPoses(r).Intersect(empties).ToList();
//						foreach (var e in erounds) {
//							if (IsCusp(e, r) && IsFlyTwo(e, pos) && LineThree.Contains(e)
//								&& RoundPoses(e).Intersect(blacks).Count() == 1)
//								return e;
//						}
//
//					}
//					foreach (var e in empties) {
//						if (IsElephantOne(e, pos) && IsJumpOne(e, r)
//							&& RoundPoses(r).Intersect(copyEmpties).Count() == 8) {
//							return e;
//						}
//					}
//				}
//
//				// 飞
//				if (IsFlyOne(r, pos)) {
//					foreach (var e in empties) {
//						if (IsJumpOne(e, r) && IsTouch(e, pos) && RoundPoses(e).Intersect(empties).Count() == 8) {
//							int r_off = e.Row - pos.Row;
//							int c_off = e.Col - pos.Col;
//							var p = new Pos(e.Row + 2 * r_off, e.Col + 2 * c_off);
//							if (!RoundPoses(p).Intersect(whites).Any())
//								return e;
//						} else if (IsFlyOne(e, r) && IsCusp(e, pos) && RoundPoses(e).Intersect(empties).Count() == 8)
//							return e;
//						else if (IsJumpOne(e, r) && IsFlyOne(e, pos) && RoundPoses(e).Intersect(empties).Count() == 9)
//							return e;
//					}
//				}
//			}
//
//			#endregion
//
//			return m_InvalidPos;
//		}
//
		int GetLength(Pos p1, Pos p2)
		{
			return (int)new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length;
		}
//		int GetBlackMeshBlockCount(Pos p)
//		{
//			UpdateAllMeshBlocks();
//			foreach (var block in m_BlackMeshBlocks) {
//				if (block.Poses.Contains(p)) {
//					return block.Poses.Count;
//				}
//			}
//			return -1;
//		}
//		List<Pos> GetArea(Pos p1, Pos p2)
//		{
//			List<Pos> poses = new List<Pos>();
//			int r1 = Math.Min(p1.Row, p2.Row);
//			int r2 = Math.Max(p1.Row, p2.Row);
//			int c1 = Math.Min(p1.Col, p2.Col);
//			int c2 = Math.Max(p1.Col, p2.Col);
//			for (int r = r1; r <= r2; r++) {
//				for (int c = c1; c <= c2; c++) {
//					var pos = new Pos(r, c);
//					poses.Add(pos);
//				}
//			}
//			return poses;
//		}
//		List<Pos> RemoveEmpties(List<Pos> empties, List<Pos> blacks)
//		{
//			var copyEmpties = empties.ToList();
//
//			foreach (var item in m_LevyPoses) { // 征子
//				if (empties.Contains(item))
//					empties.Remove(item);
//			}
//
//			foreach (var e in copyEmpties) {
//				if (LinkPoses(e).Intersect(BlackPoses).Count() >= 3
//					|| LinkPoses(e).Intersect(WhitePoses).Count() >= 3 || LineOneTwo.Contains(e)) {
//					empties.Remove(e);  // 虎口和禁入
//				}
//
//				// 排除双
//				var _links = LinkPoses(e);
//				var _elinks = _links.Intersect(empties).ToList();
//				_elinks.Remove(e);
//				var _blinks = _links.Intersect(blacks).ToList();
//				if (_blinks.Count == 2) {
//					foreach (var el in _elinks) {
//						bool isRow = el.Row == e.Row ? true : false;
//						int r_off = isRow ? 0 : el.Row - e.Row;
//						int c_off = isRow ? el.Col - e.Col : 0;
//						var b0 = new Pos(_blinks[0].Row + r_off, _blinks[0].Col + c_off);
//						var b1 = new Pos(_blinks[1].Row + r_off, _blinks[1].Col + c_off);
//						if (IsTouch(b0, el) && IsTouch(b1, el) && blacks.Contains(b0) && blacks.Contains(b1)) {
//							empties.Remove(el);
//							empties.Remove(e);
//						}
//					}
//				}
//
//				// 排除死子
//				var deads = new List<Step>();
//				foreach (var block in m_DeadBlocks) {
//					StepBlock stepBlock = block.Value;
//					foreach (var item in stepBlock.Steps) {
//						if (!deads.Contains(item))
//							deads.Add(item);
//					}
//				}
//				foreach (var item in deads) {
//					var p = GetPos(item);
//					if (e == p) empties.Remove(e);
//				}
//			}
//			return copyEmpties;
//		}
//		List<Pos> RemoveEmpties(List<Pos> empties)
//		{
//			UpdateMeshes();
//			var w_boundary = new List<Pos>();
//			foreach (var w in m_WhiteMeshes) {
//				if (!w_boundary.Contains(w) &&
//					(LinkPoses(w).Intersect(m_BlackMeshes).Any()
//						 || LinkPoses(w).Intersect(m_EmptyMeshes).Any())) {
//					w_boundary.Add(w);
//				}
//			}
//
//			var copyEmpties = empties.ToList();
//
//			foreach (var e in empties) {
//				if (LinkPoses(e).Intersect(WhitePoses).Count() >= 3
//						&& LinkPoses(e).Intersect(empties).Count() <= 2) {
//					copyEmpties.Remove(e);    // 虎口和禁入
//				}
//
//				// 排除白占领
//				if (m_WhiteMeshes.Contains(e)) {
//					foreach (var w in w_boundary) {
//						if (GetLength(w, e) > 2)
//							copyEmpties.Remove(e);
//					}
//				}
//
//				// 排除死子
//				var deads = new List<Step>();
//				foreach (var block in m_DeadBlocks) {
//					StepBlock stepBlock = block.Value;
//					foreach (var item in stepBlock.Steps) {
//						if (!deads.Contains(item))
//							deads.Add(item);
//					}
//				}
//				foreach (var item in deads) {
//					var p = GetPos(item);
//					if (e == p) copyEmpties.Remove(e);
//				}
//			}
//
//			return copyEmpties;
//		}
	}
}
