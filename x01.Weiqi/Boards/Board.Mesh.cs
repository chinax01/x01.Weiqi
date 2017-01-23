/**
 * Board_Mesh.cs (c) 2015 by x01
 * ------------------------------
 *   根据相关点的值来完成点目功能。
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;

using x01.Weiqi.Core;

namespace x01.Weiqi.Boards
{
	/// <summary>
	/// Description of Board_Mesh.
	/// </summary>
	public partial class Board
	{
		int[,] m_MeshWorths = new int[19,19];
		List<Pos> m_BlackMeshes = new List<Pos>();
		List<Pos> m_WhiteMeshes = new List<Pos>();
		
		public void UpdateMeshWorths(Pos next, bool isBlack = true, List<Pos> blackPoses = 	null, List<Pos> whitePoses = null)
		{
			if (blackPoses == null)
				blackPoses = BlackPoses.ToList();
			if (whitePoses == null)
				whitePoses = WhitePoses.ToList();
			for (int i = 0; i < 19; i++) {
				for (int j = 0; j < 19; j++) {
					m_MeshWorths[i,j] = 0;
				}
			}
			
			foreach (var p in blackPoses) {
				SetMeshWorth(p, true);
			}
			foreach (var p in whitePoses) {
				SetMeshWorth(p,false);
			}
			
			if (EmptyPoses.Contains(next) && isBlack)
				SetMeshWorth(next,true);
			else if (EmptyPoses.Contains(next) && !isBlack)
				SetMeshWorth(next, false);
			
			m_BlackMeshes.Clear();
			m_WhiteMeshes.Clear();
			for (int i = 0; i < 19; i++) {
				for (int j = 0; j < 19; j++) {
					if (m_MeshWorths[i,j] > 0)
						m_BlackMeshes.Add(new Pos(i,j));
					else if (m_MeshWorths[i,j] < 0)
						m_WhiteMeshes.Add(new Pos(i,j));
				}
			}
			
			FillMeshEmpty(1);
		}
		void SetMeshWorth(Pos pos, bool isBlack)
		{
			int sixteen = isBlack ? 16 : -16;
			int four = isBlack ? 4 : -4;
			int one = isBlack ? 1 : -1;
			m_MeshWorths[pos.Row,pos.Col] = sixteen;
			var poses = RoundThreePoses(pos); 
			foreach (var p in poses) {
				if (IsTouch(p,pos)) 
					m_MeshWorths[p.Row,p.Col] += four;
				else if(IsCusp(p,pos) || IsJumpOne(p,pos)) {
					m_MeshWorths[p.Row,p.Col] += one;
					
					if (LineTwo.Contains(p)) {
						var lineOne = LinkPoses(p).Intersect(LineOne).First();
						m_MeshWorths[lineOne.Row,lineOne.Col] += one;
					}
				}
			}
		}
		void FillMeshEmpty(int repeat=1)
		{
			for (int i=0; i < repeat; i++) {
				var blacks = m_BlackMeshes.ToList();
				var whites = m_WhiteMeshes.ToList();
				var empties = EmptyPoses.Except(blacks).Except(whites);
				foreach (var e in empties) {
					var links = LinkPoses(e);
					var linkBlacks	= links.Intersect(blacks);
					var linkWhites = links.Intersect(whites);
					if (linkBlacks.Count() >= 2 && linkWhites.Count() == 0) {
						m_BlackMeshes.Add(e);
					} else if (linkWhites.Count() >= 2 && linkBlacks.Count() == 0) {
						m_WhiteMeshes.Add(e);
					}
				}
			}
		}
		
		public void ShowMeshes()
		{
			UpdateMeshWorths(Helper.InvalidPos);

//			if (StepCount > 120) {
//				UpdateMeshes_DeadLife(true);
//				UpdateMeshes_DeadLife(false);
//				FillMeshEmpty(2);
//			}
			
			foreach (var b in m_BlackMeshes) {
				m_MeshRects[b.Row,b.Col].Visibility = System.Windows.Visibility.Visible;
				m_MeshRects[b.Row,b.Col].Fill = Brushes.Black;
			}
			foreach (var w in m_WhiteMeshes) {
				m_MeshRects[w.Row,w.Col].Visibility = System.Windows.Visibility.Visible;
				m_MeshRects[w.Row,w.Col].Fill = Brushes.White;
			}
			
			int win = m_BlackMeshes.Count - m_WhiteMeshes.Count;
			string s = win > 0 ? "黑胜：" + win.ToString() 
				: win < 0 ? "白胜：" + (-win).ToString() 
				: "和棋";
			MessageBox.Show(s);
		}
		public void HideMeshes()
		{
			foreach (var item in m_MeshRects) {
				item.Visibility = System.Windows.Visibility.Hidden;
			}
		}
		
		#region For dead-life
		
		List<Pos> m_DeadLifeKeyPoses = new List<Pos>();
		
		// Usage:
		//   Up..(true); 涉及到比气
		//   Up..(false); 剔除死子
		void UpdateMeshes_DeadLife(bool isFirst = true) // 死活
		{
			m_DeadLifeKeyPoses.Clear();
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
						if (links.Intersect(poses).Count() == 5) {  // 梅花六
							var tmp = poses.Except(links).ToList();
							if (IsCusp(tmp[0], p)) {
								block.IsDead = true;
								block.KeyPos = p;
								m_DeadLifeKeyPoses.Add(p);
							}
						}
					});
				} else if (poses.Count == 5) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 4) {  // 刀把五
							var tmp = poses.Except(links).ToList();
							if (IsCusp(tmp[0], p)) {
								block.IsDead = true;
								block.KeyPos = p;
								m_DeadLifeKeyPoses.Add(p);
							}
						}
					});
				} else if (poses.Count == 4) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 4) {  // 斗笠四
							block.IsDead = true;
							block.KeyPos = p;
							m_DeadLifeKeyPoses.Add(p);
						} else if (links.Intersect(poses).Count() == 3  // 盘角曲四
								   && (p == new Pos(0, 0) || p == new Pos(0, 18) || p == new Pos(18, 0) || p == new Pos(18, 18))) {
							block.IsDead = true;
							if (StepCount > 150)    // 劫尽棋亡
								foreach (var key in links) {
									if (key != p && LinkPoses(key).Intersect(poses).Count() == 3){
										block.KeyPos = key;
										m_DeadLifeKeyPoses.Add(key);
									}
								}
						}
					});
				} else if (poses.Count == 3) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 3) {  // 直三、曲三
							block.IsDead = true;
							block.KeyPos = p;
							m_DeadLifeKeyPoses.Add(p);
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
						if (links.Intersect(poses).Count() == 5) {  // 梅花六
							var tmp = poses.Except(links).ToList();
							if (IsCusp(tmp[0], p)) {
								block.IsDead = true;
								block.KeyPos = p;
								m_DeadLifeKeyPoses.Add(p);
							}
						}
					});
				} else if (poses.Count == 5) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 4) {  // 刀把五
							var tmp = poses.Except(links).ToList();
							if (IsCusp(tmp[0], p)) {
								block.IsDead = true;
								block.KeyPos = p;
								m_DeadLifeKeyPoses.Add(p);
							}
						}
					});
				} else if (poses.Count == 4) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 4) {  // 斗笠四
							block.IsDead = true;
							block.KeyPos = p;
							m_DeadLifeKeyPoses.Add(p);
						} else if (links.Intersect(poses).Count() == 3  // 盘角曲四
							&& (p == new Pos(0, 0) || p == new Pos(0, 18) || p == new Pos(18, 0) || p == new Pos(18, 18))) {
							block.IsDead = true;
							if (StepCount > 150) {
								foreach (var key in links) {
									if (key != p && LinkPoses(key).Intersect(poses).Count() == 3) {
										block.KeyPos = key;
										m_DeadLifeKeyPoses.Add(key);
									}
								}
							}
						}
					});
				} else if (poses.Count == 3) {
					poses.ForEach(p => {
						var links = LinkPoses(p);
						if (links.Intersect(poses).Count() == 3) {  // 直三、曲三
							block.IsDead = true;
							block.KeyPos = p;
							m_DeadLifeKeyPoses.Add(p);
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
		
		// Learning UpdateStepBlocks().
		List<PosBlock> m_BlackMeshBlocks = new List<PosBlock>();
		List<PosBlock> m_WhiteMeshBlocks = new List<PosBlock>();
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
			for (int i = 0; i < 6; i++) {   // 确保不遗漏到疯狂程度
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
		void UpdateAllMeshBlocks()
		{
			UpdateBlackMeshBlocks();
			UpdateWhiteMeshBlocks();
		}
		
		#endregion
	}
}
