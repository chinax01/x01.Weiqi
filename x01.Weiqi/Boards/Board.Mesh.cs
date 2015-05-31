/**
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

		#region Mesh Helper
		
		const int EndCount = 120;

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
			for (int i = 0; i < 4; i++) {	// 确保不遗漏到疯狂程度
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

		private void UpdateMeshes()
		{
			UpdateMeshes1();
			UpdateMeshes2();

			UpdateMeshes3(2);
			UpdateMeshes3(3);
			UpdateMeshes3(4);
			UpdateMeshes3(5);
			UpdateMeshes3(6);
			UpdateMeshes3(7);
			UpdateMeshes3(8); // 多次扫描有必要

			UpdateMeshes4();

			UpdateMeshes5();  // 第一次涉及到比气
			UpdateMeshes5(false);
		}

		void UpdateMeshes1()
		{
			m_BlackMeshes.Clear();
			BlackPoses.ForEach(p => m_BlackMeshes.Add(p));
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
				LinkPoses(pos).ForEach(l => { poses.Add(l); AddLinkPoses(poses, l); });
				AddLinkPoses(poses, pos);
			}
			poses.ForEach(p => { if (!m_BlackMeshes.Contains(p)) m_BlackMeshes.Add(p); });
		}
		void AddWhiteMeshes()
		{
			List<Pos> poses = new List<Pos>();
			foreach (var pos in m_WhiteMeshes) {
				LinkPoses(pos).ForEach(l => { poses.Add(l); AddLinkPoses(poses, l); });
				AddLinkPoses(poses, pos);
			}
			poses.ForEach(p => { if (!m_WhiteMeshes.Contains(p)) m_WhiteMeshes.Add(p); });
		}
		// 三四线为实地向下，五六七线为势力向上。
		private void AddLinkPoses(List<Pos> poses, Pos pos)
		{
			if (LineThree().Contains(pos) || LineFour().Contains(pos)) {
				List<Pos> links = LinkPoses(pos);
				foreach (var link in links) {	// 2,3 line
					if (LineTwo().Contains(link) || LineThree().Contains(link)) {
						if (!poses.Contains(link)) poses.Add(link);
						var lines = LinkPoses(link);
						foreach (var l in lines) {	// l,2 line
							if (LineOne().Contains(l) || LineTwo().Contains(l)) {
								if (!poses.Contains(l)) poses.Add(l);
								var ones = LinkPoses(l);
								foreach (var one in ones) {	// 1 line
									if (LineOne().Contains(one))
										if (!poses.Contains(one)) poses.Add(one);
								}
							}
						}
					}
				}
			}
			if (LineFive().Contains(pos) || LineSix().Contains(pos) || LineSeven().Contains(pos)) {
				List<Pos> links = LinkPoses(pos);
				foreach (var link in links) {	// 6,7,8 line
					if (LineSix().Contains(link) || LineSeven().Contains(link) || LineEight().Contains(link)) {
						if (!poses.Contains(link)) poses.Add(link);
						var lines = LinkPoses(link);
						foreach (var l in lines) {	// 7,8,9 line
							if (LineSeven().Contains(l) || LineEight().Contains(l) || LineNine().Contains(l)) {
								if (!poses.Contains(l)) poses.Add(l);
								var nines = LinkPoses(l);
								foreach (var nine in nines) {	// 8,9 line
									if (LineEight().Contains(nine) || LineNine().Contains(nine))
										if (!poses.Contains(nine)) poses.Add(nine);
								}
							}
						}
					}
				}
			}
		}

		void UpdateMeshes2()
		{
			var copyEmpties = m_EmptyMeshes.ToList();
			foreach (var pos in copyEmpties) {
				var links = LinkPoses(pos);
				var b_poses = links.Intersect(m_BlackMeshes).ToList();
				var w_poses = links.Intersect(m_WhiteMeshes).ToList();
				if (b_poses.Count >= 2 && b_poses.Count > w_poses.Count) {
					if (!m_BlackMeshes.Contains(pos)) {
						m_BlackMeshes.Add(pos);
						m_EmptyMeshes.Remove(pos);
					}
				} else if (w_poses.Count >= 2 && w_poses.Count > b_poses.Count) {
					if (!m_WhiteMeshes.Contains(pos)) {
						m_WhiteMeshes.Add(pos);
						m_EmptyMeshes.Remove(pos);
					}
				}
			}

			foreach (var pos in LineTwo()) {
				if (m_EmptyMeshes.Contains(pos)) {
					var links = LinkPoses(pos);
					links.ForEach(l => {
						if (LineThree().Contains(l)) {
							if (m_BlackMeshes.Contains(l) && !m_BlackMeshes.Contains(pos)) {
								m_BlackMeshes.Add(pos);
								m_EmptyMeshes.Remove(pos);
							} else if (m_WhiteMeshes.Contains(l) && !m_WhiteMeshes.Contains(pos)) {
								m_WhiteMeshes.Add(pos);
								m_EmptyMeshes.Remove(pos);
							}
						}
					});
				}
			}
			foreach (var pos in LineOne()) {
				if (m_EmptyMeshes.Contains(pos)) {
					var links = LinkPoses(pos);
					links.ForEach(l => {
						if (LineTwo().Contains(l)) {
							if (m_BlackMeshes.Contains(l) && !m_BlackMeshes.Contains(pos)) {
								m_BlackMeshes.Add(pos);
								m_EmptyMeshes.Remove(pos);
							} else if (m_WhiteMeshes.Contains(l) && !m_WhiteMeshes.Contains(pos)) {
								m_WhiteMeshes.Add(pos);
								m_EmptyMeshes.Remove(pos);
							}
						}
					});
				}
			}

			if (StepCount < EndCount) return;

			UpdateAllMeshBlocks();

			foreach (var block in m_EmptyMeshBlocks) {
				if (block.Poses.Count < 10) {
					var b_poses = new List<Pos>();
					var w_poses = new List<Pos>();
					foreach (var pos in block.Poses) {
						var links = LinkPoses(pos);
						links.ForEach(l => {
							if (m_BlackMeshes.Contains(l)) b_poses.Add(l);
							else if (m_WhiteMeshes.Contains(l)) w_poses.Add(l);
						});
					}
					if (b_poses.Count > w_poses.Count) {
						m_BlackMeshes.AddRange(block.Poses);
						m_EmptyMeshes.RemoveAll(e => block.Poses.Contains(e));
					} else if (b_poses.Count < w_poses.Count) {
						m_WhiteMeshes.AddRange(block.Poses);
						m_EmptyMeshes.RemoveAll(e => block.Poses.Contains(e));
					}
				}
			}
		}
		void UpdateMeshes3(int count)
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
			//UpdateMeshColors();
		}
		void UpdateMeshes4()
		{
			if (StepCount < EndCount) return;

			UpdateAllMeshBlocks();

			foreach (var block in m_EmptyMeshBlocks) {
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
					m_BlackMeshes.AddRange(block.Poses);
				} else if (b_poses.Count < w_poses.Count) {
					m_WhiteMeshes.AddRange(block.Poses);
				}
			}
			//UpdateMeshColors();
		}
		void UpdateMeshes5(bool isFirst = true)
		{
			if (StepCount < EndCount) return;

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

			UpdateMeshes4();	// 居然还需调用一次！

			UpdateMeshColors();
		}

		void UpdateMeshColors()
		{
			m_BlackMeshes.ForEach(p => {
				if (m_EmptyMeshes.Contains(p)) m_EmptyMeshes.Remove(p);
				p.PosColor = StoneColor.Black;
			});
			m_WhiteMeshes.ForEach(p => {
				if (m_EmptyMeshes.Contains(p)) m_EmptyMeshes.Remove(p);
				p.PosColor = StoneColor.White;
			});
			m_EmptyMeshes.ForEach(p => {
				p.PosColor = StoneColor.Empty;
			});
		}

		#endregion
	}
}
