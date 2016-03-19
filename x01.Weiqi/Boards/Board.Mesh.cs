﻿/**
 * Board.Mesh.cs (c) 2015 by x01
 * -----------------------------
 *	1.如果 Pos 是具体位置，那么，Mesh 就是逻辑位置，所谓点目也。
 *    点目（ShowMeshes）准备如编译器的词法分析，进行多遍扫描。
 *	2.按理是先解决死活问题，再讨论点目，但死活过于复杂，只好反其道
 *	  而行之。先点目，再解决死活，可能会缩小范围，降低复杂度吧。
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;

namespace x01.Weiqi.Boards
{
	partial class Board
	{
		public void ShowMeshes()
		{
			UpdateMeshes();

			foreach (var pos in m_BlackMeshes) {
				m_MeshRects[pos.Row, pos.Col].Visibility = System.Windows.Visibility.Visible;
				m_MeshRects[pos.Row, pos.Col].Fill = Brushes.Black;
			}

			foreach (var pos in m_WhiteMeshes) {
				m_MeshRects[pos.Row, pos.Col].Visibility = System.Windows.Visibility.Visible;
				m_MeshRects[pos.Row, pos.Col].Fill = Brushes.White;
			}

			if (StepCount > EndCount) {
				int blackCount = m_BlackMeshes.Count;
				int whiteCount = m_WhiteMeshes.Count;
				int winCount = blackCount - whiteCount;
				string s = winCount > 0 ? "黑胜: " + winCount.ToString() :
					winCount < 0 ? "白胜: " + (-winCount).ToString() : "和棋";
				MessageBox.Show(s);
			}
		}
		public void HideMeshes()
		{
			foreach (var item in m_MeshRects) {
				item.Visibility = System.Windows.Visibility.Hidden;
			}
		}


		List<Tuple<int, Pos>> m_BestPoses = new List<Tuple<int, Pos>>();
		List<Pos> GetBestPoses()
		{
			m_BestPoses.Clear();
			var empties = EmptyPoses.ToList();
			foreach (var e in empties) {
				UpdateMeshes_Base(e);
				int count = m_BlackMeshes.Count - m_WhiteMeshes.Count;
				m_BestPoses.Add(new Tuple<int, Pos>(count, e));
			}

			var key = m_BestPoses.OrderByDescending(b => b.Item1).First().Item1;
			var result = from e in m_BestPoses
						 where e.Item1 == key
						 select e.Item2;
			return result.ToList();
		}

		#region Mesh Helper
		
		const int EndCount = 0;

		// Mesh: 目，与 Empty 区分
		List<Pos> m_BlackMeshes = new List<Pos>();
		List<Pos> m_WhiteMeshes = new List<Pos>();
		List<Pos> m_EmptyMeshes = new List<Pos>();

		// Learning UpdateStepBlocks().
		List<PosBlock> m_BlackMeshBlocks = new List<PosBlock>();
		List<PosBlock> m_WhiteMeshBlocks = new List<PosBlock>();
		List<PosBlock> m_EmptyMeshBlocks = new List<PosBlock>();
		void UpdateMeshBlocks(List<Pos> poses, List<PosBlock> blocks)
		{
			List<Pos> copyPoses = poses.ToList();
			if (copyPoses.Count == 0) return;

			List<Pos> tmp = new List<Pos>();
			foreach (var pos in copyPoses) {
				if (tmp.Count == 0) tmp.Add(pos);
				var links = LinkPoses(pos);
				if (tmp.Intersect(links).Any()) {
					links.ForEach(l => {
						if (copyPoses.Contains(l) && !tmp.Contains(l))
							tmp.Add(l);
					});
				}
			}
			for (int i = 0; i < 6; i++) {	// 确保不遗漏到疯狂程度
				foreach (var pos in copyPoses) {
					var links = LinkPoses(pos);
					if (tmp.Intersect(links).Any()) {
						links.ForEach(l => {
							if (copyPoses.Contains(l) && !tmp.Contains(l))
								tmp.Add(l);
						});
					}
				}
			}

			PosBlock block = new PosBlock();
			block.Poses = tmp;
			blocks.Add(block);

			copyPoses.RemoveAll(p => tmp.Contains(p));
			UpdateMeshBlocks(copyPoses, blocks);
		}
		void UpdateBlackMeshBlocks()
		{
			m_BlackMeshBlocks.Clear();
			UpdateMeshBlocks(m_BlackMeshes, m_BlackMeshBlocks);
		}
		void UpdateWhiteMeshBlocks()
		{
			m_WhiteMeshBlocks.Clear();
			UpdateMeshBlocks(m_WhiteMeshes, m_WhiteMeshBlocks);
		}
		void UpdateEmptyMeshBlocks()
		{
			m_EmptyMeshBlocks.Clear();
			UpdateMeshBlocks(m_EmptyMeshes, m_EmptyMeshBlocks);
		}
		void UpdateAllMeshBlocks()
		{
			UpdateBlackMeshBlocks();
			UpdateWhiteMeshBlocks();
			UpdateEmptyMeshBlocks();
		}

		private void UpdateMeshes(Pos next = default(Pos))
		{
			UpdateMeshes_Base(next);

			UpdateMeshes_SmallEmpty();
			UpdateMeshes_SmallEmpty();

			UpdateMeshe_DeleteDead(2);
			UpdateMeshe_DeleteDead(3);
			UpdateMeshe_DeleteDead(4);
			UpdateMeshe_DeleteDead(5);
			UpdateMeshe_DeleteDead(6);
			UpdateMeshe_DeleteDead(7);
			UpdateMeshe_DeleteDead(8); // 多次扫描有必要

			UpdateMeshes_BigEmpty();

			UpdateMeshes_DeadLife();  // 第一次涉及到比气
			UpdateMeshes_DeadLife(false);

			UpdateMeshes_End(); // 修正
		}

		void UpdateMeshes_Base(Pos next = default(Pos))	// 基本目添加
		{
			m_BlackMeshes.Clear();
			BlackPoses.ForEach(p => m_BlackMeshes.Add(p));
			if (next != default(Pos)) m_BlackMeshes.Add(next);
			AddBlackMeshes();

			m_WhiteMeshes.Clear();
			WhitePoses.ForEach(p => m_WhiteMeshes.Add(p));
			AddWhiteMeshes();

			var intersect = m_BlackMeshes.Intersect(m_WhiteMeshes).ToList();
			m_BlackMeshes.RemoveAll(b => intersect.Contains(b));
			m_WhiteMeshes.RemoveAll(w => intersect.Contains(w));

			BlackPoses.ForEach(p => { if (!m_BlackMeshes.Contains(p)) m_BlackMeshes.Add(p); });
			WhitePoses.ForEach(p => { if (!m_WhiteMeshes.Contains(p)) m_WhiteMeshes.Add(p); });

			m_EmptyMeshes.Clear();
			AllPoses.ForEach(p => {
				if (!(m_BlackMeshes.Contains(p) || m_WhiteMeshes.Contains(p))) m_EmptyMeshes.Add(p);
			});
		}
		void AddBlackMeshes()
		{
			List<Pos> poses = new List<Pos>();
			foreach (var pos in m_BlackMeshes) {
				//LinkPoses(pos).ForEach(l => { poses.Add(l); AddPoses(poses, l); });
				AddPoses(poses, pos);
			}
			poses.ForEach(p => { if (!m_BlackMeshes.Contains(p)) m_BlackMeshes.Add(p); });
		}
		void AddWhiteMeshes()
		{
			List<Pos> poses = new List<Pos>();
			foreach (var pos in m_WhiteMeshes) {
				//LinkPoses(pos).ForEach(l => { poses.Add(l); AddPoses(poses, l); });
				AddPoses(poses, pos);
			}
			poses.ForEach(p => { if (!m_WhiteMeshes.Contains(p)) m_WhiteMeshes.Add(p); });
		}

		void AddPoses(List<Pos> poses, Pos pos)
		{
			var rounds = RoundOnePoses(pos).Intersect(EmptyPoses).ToList();
			foreach (var r in rounds) {
				if (!poses.Contains(r)) poses.Add(r);
			}

			var threeline = rounds.Intersect(LineThree).ToList();
			foreach (var t in threeline) {
				var links = LinkPoses(t).Intersect(LineTwo).ToList();
				foreach (var l in links) {
					if (!poses.Contains(l)) poses.Add(l);
				}
			}

			var twoline = poses.Intersect(LineTwo).ToList();
			foreach (var t in twoline) {
				var links = LinkPoses(t).Intersect(LineOne).ToList();
				foreach (var l in links) {
					if (!poses.Contains(l)) poses.Add(l);
				}
			}
		}

		void UpdateMeshes_SmallEmpty()	// 添加小的空块，暂不考虑死活
		{
			UpdateAllMeshBlocks();

			var emptyMeshBlocks = m_EmptyMeshBlocks.ToList();
			foreach (var eblock in emptyMeshBlocks) {
				int count = eblock.Poses.Count;
				if (count < 12) {
					var bs = new List<Pos>();
					var ws = new List<Pos>();
					var bws = new List<Pos>();
					foreach (var e in eblock.Poses) {
						var links = LinkPoses(e);
						if (links.Intersect(m_BlackMeshes).Any() && !bs.Contains(e))
							bs.Add(e);
						if (links.Intersect(m_WhiteMeshes).Any() && !ws.Contains(e))
							ws.Add(e);
					}
					bws = bs.Intersect(ws).ToList();
					bs = bs.Except(bws).ToList();
					ws = ws.Except(bws).ToList();
					foreach (var b in bs) {
						m_EmptyMeshes.Remove(b);
						m_BlackMeshes.Add(b);
					}
					foreach (var w in ws) {
						m_EmptyMeshes.Remove(w);
						m_WhiteMeshes.Add(w);
					}
					int c = 0;
					foreach (var bw in bws) {
						m_EmptyMeshes.Remove(bw);
						if (c++ % 2  == 1)
							m_BlackMeshes.Add(bw);
						else
							m_WhiteMeshes.Add(bw);
					}
				}
			}
		}
		void UpdateMeshe_DeleteDead(int count)	// 逐步剔除死子
		{
			UpdateAllMeshBlocks();
			foreach (var block in m_BlackMeshBlocks) {
				if (block.Poses.Count < count) {
					block.Poses.ForEach(p => {
						m_BlackMeshes.Remove(p);
						m_WhiteMeshes.Add(p);
					});
				}
			}
			foreach (var block in m_WhiteMeshBlocks) {
				if (block.Poses.Count < count) {
					block.Poses.ForEach(p => {
						m_WhiteMeshes.Remove(p);
						m_BlackMeshes.Add(p);
					});
				}
			}
		}
		void UpdateMeshes_BigEmpty()	// 添加大的空块
		{
			UpdateAllMeshBlocks();

			foreach (var block in m_EmptyMeshBlocks) {
				if (block.Poses.Count < 30) {
					List<Pos> b_poses = new List<Pos>();
					List<Pos> w_poses = new List<Pos>();
					foreach (var pos in block.Poses) {
						var links = LinkPoses(pos);
						links.ForEach(l => {
							if (m_BlackMeshes.Contains(l) && !b_poses.Contains(l))
								b_poses.Add(l);
							if (m_WhiteMeshes.Contains(l) && !w_poses.Contains(l))
								w_poses.Add(l);
						});
					}
					if (b_poses.Count > w_poses.Count) {
						block.Poses.ForEach(p => { if (!m_BlackMeshes.Contains(p)) m_BlackMeshes.Add(p); });
					} else if (b_poses.Count < w_poses.Count) {
						block.Poses.ForEach(p => { if (!m_WhiteMeshes.Contains(p)) m_WhiteMeshes.Add(p); });
					}
				}
			}
		}
		void UpdateMeshes_DeadLife(bool isFirst = true) // 死活
		{
			UpdateAllMeshBlocks();

			m_BlackMeshBlocks.ForEach(block => {
				var poses = block.Poses.ToList();
				block.Poses.ForEach(p => {
					if (BlackPoses.Contains(p))
						poses.Remove(p);
					LinkPoses(p).ForEach(l => {
						if (m_WhiteMeshes.Contains(l))
							poses.Remove(p);
					});
				});

				if (poses.Count == 6) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 5) {	// 梅花六
							var tmp = poses.Except(links).ToList();
							if (IsCusp(tmp[0], p)) {
								block.IsDead = true;
								block.KeyPos = p;
							}
						}
					});
				} else if (poses.Count == 5) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 4) {	// 刀把五
							var tmp = poses.Except(links).ToList();
							if (IsCusp(tmp[0], p)) {
								block.IsDead = true;
								block.KeyPos = p;
							}
						}
					});
				} else if (poses.Count == 4) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 4) {	// 斗笠四
							block.IsDead = true;
							block.KeyPos = p;
						} else if (links.Intersect(poses).Count() == 3	// 盘角曲四
								   && (p == new Pos(0, 0) || p == new Pos(0, 18) || p == new Pos(18, 0) || p == new Pos(18, 18))) {
							block.IsDead = true;
							if (StepCount > 150)	// 劫尽棋亡
								foreach (var key in links) {
									if (key != p && LinkPoses(key).Intersect(poses).Count() == 3)
										block.KeyPos = key;
								}
						}
					});
				} else if (poses.Count == 3) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 3) {	// 直三、曲三
							block.IsDead = true;
							block.KeyPos = p;
						}
					});
				} else if (poses.Count == 2) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 2) {
							block.IsDead = true;
						}
					});
				} else if (poses.Count < 2) {
					block.IsDead = true;
				}

				if (!isFirst && block.IsDead) {
					m_BlackMeshes.RemoveAll(b => block.Poses.Contains(b));
					m_WhiteMeshes.AddRange(block.Poses);
				}
			});

			m_WhiteMeshBlocks.ForEach(block => {
				var poses = block.Poses.ToList();
				block.Poses.ForEach(p => {
					if (WhitePoses.Contains(p))
						poses.Remove(p);
					LinkPoses(p).ForEach(l => {
						if (m_BlackMeshes.Contains(l))
							poses.Remove(p);
					});
				});
				if (poses.Count == 6) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 5) {	// 梅花六
							var tmp = poses.Except(links).ToList();
							if (IsCusp(tmp[0], p)) {
								block.IsDead = true;
								block.KeyPos = p;
							}
						}
					});
				} else if (poses.Count == 5) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 4) {	// 刀把五
							var tmp = poses.Except(links).ToList();
							if (IsCusp(tmp[0], p)) {
								block.IsDead = true;
								block.KeyPos = p;
							}
						}
					});
				} else if (poses.Count == 4) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 4) {	// 斗笠四
							block.IsDead = true;
							block.KeyPos = p;
						} else if (links.Intersect(poses).Count() == 3	// 盘角曲四
							&& (p == new Pos(0, 0) || p == new Pos(0, 18) || p == new Pos(18, 0) || p == new Pos(18, 18))) {
							block.IsDead = true;
							if (StepCount > 150)
								foreach (var key in links) {
									if (key != p && LinkPoses(key).Intersect(poses).Count() == 3)
										block.KeyPos = key;
								}
						}
					});
				} else if (poses.Count == 3) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 3) {	// 直三、曲三
							block.IsDead = true;
							block.KeyPos = p;
						}
					});
				} else if (poses.Count == 2) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 2) {
							block.IsDead = true;
						}
					});
				} else if (poses.Count < 2) {
					block.IsDead = true;
				}

				if (!isFirst && block.IsDead) {
					m_WhiteMeshes.RemoveAll(b => block.Poses.Contains(b));
					m_BlackMeshes.AddRange(block.Poses);
				}
			});

			if (isFirst) {
				m_BlackMeshBlocks.ForEach(block => {
					if (block.IsDead) {
						foreach (var pos in block.Poses) {
							var links = LinkPoses(pos);
							m_WhiteMeshBlocks.ForEach(w_block => {
								if (links.Intersect(w_block.Poses).Count() > 0) {
									if (w_block.IsDead) {
										BlackPosBlocks.ForEach(bp_block => {
											if (bp_block.Poses.Contains(pos)) {
												block.EmptyCount = bp_block.EmptyCount;
												}
											WhitePosBlocks.ForEach(wp_block => {
												if (wp_block.Poses.Intersect(w_block.Poses).Count() > 0) {
													w_block.EmptyCount = wp_block.EmptyCount;
												}
											});
										});
										if (block.EmptyCount > w_block.EmptyCount) {
											m_WhiteMeshes.RemoveAll(w => w_block.Poses.Contains(w));
											m_BlackMeshes.AddRange(w_block.Poses);
										} else if (block.EmptyCount < w_block.EmptyCount) {
											m_BlackMeshes.RemoveAll(b => block.Poses.Contains(b));
											m_WhiteMeshes.AddRange(block.Poses);
										}
									}
								}
							});
						}
					}
				});
			}
		}
		void UpdateMeshes_End()
		{
			UpdateAllStepBlocks();

			foreach (var block in m_BlackStepBlocks) {
				foreach (var step in block.Steps) {
					var pos = GetPos(step);
					if (m_BlackMeshes.Contains(pos)) {
						foreach (var item in block.Steps) {
							var p = GetPos(item);
							if (!m_BlackMeshes.Contains(p) && m_WhiteMeshes.Contains(p)) {
								m_WhiteMeshes.Remove(p);
								m_BlackMeshes.Add(p);
							}
						}
						break;
					}
				}
			}

			foreach (var block in m_WhiteStepBlocks) {
				foreach (var step in block.Steps) {
					var pos = GetPos(step);
					if (m_WhiteMeshes.Contains(pos)) {
						foreach (var item in block.Steps) {
							var p = GetPos(item);
							if (!m_WhiteMeshes.Contains(p) && m_BlackMeshes.Contains(p)) {
								m_BlackMeshes.Remove(p);
								m_WhiteMeshes.Add(p);
							}
						}
						break;
					}
				}
			}
		}

		#endregion
	}
}
