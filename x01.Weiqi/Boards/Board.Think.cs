/**
 * Board.Think.cs (c) 2015 by x01
 * ------------------------------
 *	1.此部分类主要是为 AI Board 提供服务，之所以要单独列出来，
 *	  一是内容多，二是为了不影响其他部分。
 *	2.Pos 针对 Step 的 Row、Col，但 Pos 更利于 Think，且不影响
 *	  其他部分，单列出来是有意义的。当然，LinkSteps 与 LinkPoses 
 *	  之类的重复也就在所难免了。
 *	3.如果 Pos 是具体位置，那么，Mesh 就是逻辑位置，所谓点目也。
 *	4.点目（ShowMeshes）中死活是关键，准备如编译器的扫描部分，
 *	  多遍进行调整。
 *	5.按理是先解决死活问题，再讨论点目，但死活过于复杂，只好反其道
 *	  而行之。先点目，再解决死活，可能会缩小范围，降低复杂度吧？
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
		protected readonly Pos m_InvalidPos = new Pos(-1, -1);

		public Pos Think()
		{
			// 下棋是一种由不确定到确定的过程，没有人能证明这步棋不好，那么就可以抢先确定这一步。
			if (StepCount == 0)
				return new Pos(3, 15);

			return OneEmpty() != m_InvalidPos ? OneEmpty() : RandDown();
		}

		public void ShowMeshes()
		{
			InitMeshes();

			foreach (var pos in m_BlackMeshes) {
				m_MeshRects[pos.Row, pos.Col].Visibility = System.Windows.Visibility.Visible;
				m_MeshRects[pos.Row, pos.Col].Fill = Brushes.Black;
			}

			foreach (var pos in m_WhiteMeshes) {
				m_MeshRects[pos.Row, pos.Col].Visibility = System.Windows.Visibility.Visible;
				m_MeshRects[pos.Row, pos.Col].Fill = Brushes.White;
			}

			int blackCount = m_BlackMeshes.Count;
			int whiteCount = m_WhiteMeshes.Count;
			int winCount = blackCount - whiteCount;
			string s = winCount > 0 ? "Black win: " + winCount.ToString() : "White win: " + (-winCount).ToString();
			//MessageBox.Show(s);
		}
		public void HideMeshes()
		{
			foreach (var item in m_MeshRects) {
				item.Visibility = System.Windows.Visibility.Hidden;
			}
		}

		#region Think Helper

		Random m_Rand = new Random();

		// 一口气
		Pos OneEmpty()
		{
			foreach (var block in m_WhiteStepBlocks) {	// Eat white
				if (block.EmptyCount == 1) {
					List<Step> empties = GetLinkEmptySteps(block);
					return new Pos(empties[0].Row, empties[0].Col);
				}
			}
			foreach (var block in m_BlackStepBlocks) {	// Feel black
				if (block.EmptyCount == 1) {
					List<Step> empties = GetLinkEmptySteps(block);
					return new Pos(empties[0].Row, empties[0].Col);
				}
			}
			return m_InvalidPos;
		}
		List<Step> GetLinkBlackSteps(StepBlock block)
		{
			List<Step> blacks = new List<Step>();
			foreach (var step in block.Steps) {
				LinkSteps(step, StoneColor.Black).ForEach(s => {
					if (!blacks.Contains(s)) blacks.Add(s);
				});
			}
			return blacks;
		}
		List<Step> GetLinkWhiteSteps(StepBlock block)
		{
			List<Step> whites = new List<Step>();
			foreach (var step in block.Steps) {
				LinkSteps(step, StoneColor.White).ForEach(s => {
					if (!whites.Contains(s)) whites.Add(s);
				});
			}
			return whites;
		}
		List<Step> GetLinkEmptySteps(StepBlock block)
		{
			List<Step> empties = new List<Step>();
			foreach (var step in block.Steps) {
				LinkSteps(step, StoneColor.Empty).ForEach(s => {
					if (!empties.Contains(s)) empties.Add(s);
				});
			}
			return empties;
		}
		
		private Pos RandDown()
		{
			int row = m_Rand.Next(0, 19);
			int col = m_Rand.Next(0, 19);
			Pos pos = new Pos(row, col);

			if (StepCount < 4) {
				if (StarFourHorn().Contains(pos))
					return pos;
			}
			if (StepCount < 40) {
				if (LineThree().Contains(pos) || LineFour().Contains(pos))
					return pos;
			} else if (StepCount < 100) {
				if (LineCenter().Contains(pos))
					return pos;
			} else if (StepCount < 361) {
				return pos;
			}

			return m_InvalidPos;
		}

		#endregion

		#region Pos Helper

		// 使用属性而不是字段
		List<Pos> m_BlackPoses = new List<Pos>();
		public List<Pos> BlackPoses
		{
			get
			{
				m_BlackPoses.Clear();
				foreach (var step in m_BlackSteps) {
					m_BlackPoses.Add(new Pos(step.Row, step.Col, StoneColor.Black));
				}
				return m_BlackPoses;
			}
		}
		List<Pos> m_WhitePoses = new List<Pos>();
		public List<Pos> WhitePoses
		{
			get
			{
				m_WhitePoses.Clear();
				foreach (var step in m_WhiteSteps) {
					m_WhitePoses.Add(new Pos(step.Row, step.Col, StoneColor.White));
				}
				return m_WhitePoses;
			}
		}
		List<Pos> m_EmptyPoses = new List<Pos>();
		public List<Pos> EmptyPoses
		{
			get
			{
				m_EmptyPoses.Clear();
				foreach (var step in m_EmptySteps) {
					m_EmptyPoses.Add(new Pos(step.Row, step.Col));
				}
				return m_EmptyPoses;
			}
		}

		List<PosBlock> m_EmptyPosBlocks = new List<PosBlock>();
		List<PosBlock> EmptyPosBlocks
		{
			get
			{
				m_EmptyPosBlocks.Clear();
				foreach (var block in m_EmptyStepBlocks) {
					PosBlock pb = new PosBlock(block);
					GetLinkBlackSteps(block).ForEach(s => pb.LinkBlackPoses.Add(new Pos(s.Row, s.Col, StoneColor.Black)));
					pb.LinkBlackCount = pb.LinkBlackPoses.Count;
					GetLinkWhiteSteps(block).ForEach(s => pb.LinkWhitePoses.Add(new Pos(s.Row, s.Col, StoneColor.White)));
					pb.LinkWhiteCount = pb.LinkWhitePoses.Count;
					pb.LinkEmptyPoses = pb.Poses;
					pb.LinkEmptyCount = pb.Poses.Count;
					m_EmptyPosBlocks.Add(pb);
				}
				return m_EmptyPosBlocks;
			}
		}
		List<PosBlock> m_BlackPosBlocks = new List<PosBlock>();
		List<PosBlock> BlackPosBlocks
		{
			get
			{
				m_BlackPosBlocks.Clear();
				foreach (var block in m_BlackStepBlocks) {
					PosBlock pb = new PosBlock(block);
					GetLinkEmptySteps(block).ForEach(s => pb.LinkEmptyPoses.Add(new Pos(s.Row, s.Col)));
					pb.LinkEmptyCount = pb.LinkEmptyPoses.Count;
					GetLinkWhiteSteps(block).ForEach(s => pb.LinkWhitePoses.Add(new Pos(s.Row, s.Col, StoneColor.White)));
					pb.LinkWhiteCount = pb.LinkWhitePoses.Count;
					pb.LinkBlackPoses = pb.Poses;
					pb.LinkBlackCount = pb.Poses.Count;
					m_BlackPosBlocks.Add(pb);
				}
				return m_BlackPosBlocks;
			}
		}
		List<PosBlock> m_WhitePosBlocks = new List<PosBlock>();
		List<PosBlock> WhitePosBlocks
		{
			get
			{
				m_WhitePosBlocks.Clear();
				foreach (var block in m_WhiteStepBlocks) {
					PosBlock pb = new PosBlock(block);
					GetLinkEmptySteps(block).ForEach(s => pb.LinkEmptyPoses.Add(new Pos(s.Row, s.Col)));
					pb.LinkEmptyCount = pb.LinkEmptyPoses.Count;
					GetLinkBlackSteps(block).ForEach(s => pb.LinkBlackPoses.Add(new Pos(s.Row, s.Col, StoneColor.Black)));
					pb.LinkBlackCount = pb.LinkBlackPoses.Count;
					pb.LinkWhitePoses = pb.Poses;
					pb.LinkWhiteCount = pb.Poses.Count;
					m_WhitePosBlocks.Add(pb);
				}
				return m_WhitePosBlocks;
			}
		}

		//   +
		// + + +	Copy code from LinkSteps().
		//   +
		List<Pos> LinkPoses(Pos pos, StoneColor posColor = StoneColor.Empty)
		{
			List<Pos> links = new List<Pos>();
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if (i == j || i == -j) {
						continue;
					}
					if (InRange(pos.Row + i, pos.Col + j)) {
						links.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			links.Add(pos);

			if (posColor == StoneColor.All) {
				return links;
			} else {
				links.RemoveAll(l => l.PosColor != posColor);
				return links;
			}
		}

		// Nine Star
		Pos StarLeftTop()
		{
			return new Pos(3, 3);
		}
		Pos StarTop()
		{
			return new Pos(9, 3);
		}
		Pos StarRightTop()
		{
			return new Pos(15, 3);
		}
		Pos StarLeft()
		{
			return new Pos(3, 9);
		}
		Pos StarCenter()
		{
			return new Pos(9, 9);
		}
		Pos StarRight()
		{
			return new Pos(15, 9);
		}
		Pos StarLeftBottom()
		{
			return new Pos(3, 15);
		}
		Pos StarBottom()
		{
			return new Pos(9, 15);
		}
		Pos StarRightBottom()
		{
			return new Pos(15, 15);
		}
		List<Pos> StarFourHorn()
		{
			List<Pos> stars = new List<Pos>();
			stars.Add(StarLeftTop());
			stars.Add(StarRightTop());
			stars.Add(StarLeftBottom());
			stars.Add(StarRightBottom());
			return stars;
		}

		enum LineFlag { One, Two, Three, Four, Five, Six, Seven, Eight, Nine }
		List<Pos> LineOne()
		{
			return GetLines(LineFlag.One);
		}
		List<Pos> LineTwo()
		{
			return GetLines(LineFlag.Two);
		}
		List<Pos> LineThree()
		{
			return GetLines(LineFlag.Three);
		}
		List<Pos> LineFour()
		{
			return GetLines(LineFlag.Four);
		}
		List<Pos> LineFive()
		{
			return GetLines(LineFlag.Five);
		}
		List<Pos> LineSix()
		{
			return GetLines(LineFlag.Six);
		}
		List<Pos> LineSeven()
		{
			return GetLines(LineFlag.Seven);
		}
		List<Pos> LineEight()
		{
			return GetLines(LineFlag.Eight);
		}
		List<Pos> LineNine()
		{
			return GetLines(LineFlag.Nine);
		}
		List<Pos> LineCenter()
		{
			return LineThree().Union(LineFour()).Union(LineFive()).Union(LineSix())
				.Union(LineSeven()).Union(LineEight()).Union(LineNine()).ToList();	// Line3-9 area
		}
		List<Pos> AllPoses()
		{
			return LineCenter().Union(LineOne()).Union(LineTwo()).ToList();
		}
		List<Pos> GetLines(LineFlag flag)
		{
			List<Pos> lines = new List<Pos>();

			int count = -1;
			switch (flag) {
				case LineFlag.One:
					count = 19;
					break;
				case LineFlag.Two:
					count = 17;
					break;
				case LineFlag.Three:
					count = 15;
					break;
				case LineFlag.Four:
					count = 13;
					break;
				case LineFlag.Five:
					count = 11;
					break;
				case LineFlag.Six:
					count = 9;
					break;
				case LineFlag.Seven:
					count = 7;
					break;
				case LineFlag.Eight:
					count = 5;
					break;
				case LineFlag.Nine:
					count = 3;
					break;
				default:
					throw new Exception("LineFlag does not exist.");
			}

			int offset = (19 - count) / 2;
			for (int i = 0; i < count; i++) {
				lines.Add(new Pos(i + offset, offset));
				lines.Add(new Pos(i + offset, 18 - offset));
				if (i != 0 && i != count - 1) {
					lines.Add(new Pos(offset, i + offset));
					lines.Add(new Pos(18 - offset, i + offset));
				}
			}
			return lines;
		}

		bool IsFlyOne(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(1, 2).Length;
		}
		bool IsFlyTwo(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(1, 3).Length;
		}
		bool IsFlyThree(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(1, 4).Length;
		}
		bool IsJumpOne(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(0, 2).Length;
		}
		bool IsJumpTwo(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(0, 3).Length;
		}
		bool IsJumpThree(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p2.Col - p2.Col).Length == new Vector(0, 4).Length;
		}
		bool IsElephantStep(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(2, 2).Length;
		}
		bool IsCusp(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(1, 1).Length;
		}
		bool IsTouch(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(0, 1).Length;
		}

		#endregion

		#region Mesh Helper

		// Mesh: 目，与 Empty 区分
		List<Pos> m_BlackMeshes = new List<Pos>();
		List<Pos> m_WhiteMeshes = new List<Pos>();
		List<Pos> m_EmptyMeshes = new List<Pos>();

		// Learning UpdateBlocks(), But not use.
		//List<PosBlock> m_BlackMeshBlocks = new List<PosBlock>();
		//List<PosBlock> m_WhiteMeshBlocks = new List<PosBlock>();
		//List<PosBlock> m_EmptyMeshBlocks = new List<PosBlock>();
		//void UpdateMeshBlocks(List<Pos> poses, List<PosBlock> blocks, StoneColor stoneColor)
		//{
		//	List<Pos> copyPoses = new List<Pos>();
		//	foreach (var item in poses) {
		//		copyPoses.Add(item);
		//	}

		//	if (copyPoses.Count == 0) return;

		//	var tmp = new List<Pos>();
		//	foreach (var pos in copyPoses) {
		//		if (tmp.Count == 0) tmp.Add(pos);
		//		var links = LinkPoses(pos);
		//		if (tmp.Intersect(links).Count() != 0)
		//			links.ForEach(l => { if (!tmp.Contains(l)) tmp.Add(l); });
		//	}
		//	foreach (var step in copyPoses) {	// 防止遗漏
		//		var sameLinks = LinkPoses(step);
		//		if (tmp.Intersect(sameLinks).Count() != 0)
		//			sameLinks.ForEach(s => { if (!tmp.Contains(s)) tmp.Add(s); });
		//	}
		//	if (tmp.Count == 0) return;

		//	PosBlock block = new PosBlock();
		//	block.Poses = tmp;
		//	block.PosColor = stoneColor;
		//	block.LinkEmptyCount = tmp.Count;
		//	blocks.Add(block);

		//	copyPoses.RemoveAll(s => tmp.Contains(s));	// next block
		//	UpdateMeshBlocks(copyPoses, blocks, stoneColor);	// 递归
		//}
		//void UpdateBlackMeshBlocks()
		//{
		//	m_BlackMeshBlocks.Clear();
		//	UpdateMeshBlocks(m_BlackMeshes.Union(BlackPoses).ToList(), m_BlackMeshBlocks, StoneColor.Black);
		//}
		//void UpdateWhiteMeshBlocks()
		//{
		//	m_WhiteMeshBlocks.Clear();
		//	UpdateMeshBlocks(m_WhiteMeshes.Union(WhitePoses).ToList(), m_WhiteMeshBlocks, StoneColor.White);
		//}
		//void UpdateEmptyMeshBlocks()
		//{
		//	m_EmptyMeshBlocks.Clear();
		//	UpdateMeshBlocks(m_EmptyMeshes.Union(EmptyPoses).ToList(), m_EmptyMeshBlocks, StoneColor.Empty);
		//}

		private void InitMeshes()
		{
			UpdateMeshes();
		}

		void UpdateMeshes()
		{
			m_BlackMeshes.Clear();
			BlackPosBlocks.ForEach(b => b.Poses.ForEach(p => m_BlackMeshes.Add(p)));
			AddBlackMeshes();

			m_WhiteMeshes.Clear();
			WhitePosBlocks.ForEach(b => b.Poses.ForEach(p => m_WhiteMeshes.Add(p)));
			AddWhiteMeshes();

			var intersect = m_BlackMeshes.Intersect(m_WhiteMeshes).ToList();
			m_BlackMeshes.RemoveAll(b => intersect.Contains(b));
			m_WhiteMeshes.RemoveAll(w => intersect.Contains(w));

			BlackPoses.ForEach(p => m_BlackMeshes.Add(p));
			m_BlackMeshes.ForEach(b => b.PosColor = StoneColor.Black);
			WhitePoses.ForEach(p => m_WhiteMeshes.Add(p));
			m_WhiteMeshes.ForEach(w => w.PosColor = StoneColor.White);
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

		#endregion
	}
}
