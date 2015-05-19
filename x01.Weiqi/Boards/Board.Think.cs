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
				m_EmptyRects[pos.Row, pos.Col].Visibility = System.Windows.Visibility.Visible;
				m_EmptyRects[pos.Row, pos.Col].Fill = Brushes.Black;
			}

			foreach (var pos in m_WhiteMeshes) {
				m_EmptyRects[pos.Row, pos.Col].Visibility = System.Windows.Visibility.Visible;
				m_EmptyRects[pos.Row, pos.Col].Fill = Brushes.White;
			}

			int blackCount = m_BlackMeshes.Count;
			int whiteCount = m_WhiteMeshes.Count;
			int winCount = blackCount - whiteCount;
			string s = winCount > 0 ? "Black win: " + winCount.ToString() : "White win: " + (-winCount).ToString();
			//MessageBox.Show(s);
		}
		public void HideMeshes()
		{
			foreach (var item in m_EmptyRects) {
				item.Visibility = System.Windows.Visibility.Hidden;
			}
		}

		#region Think Helper

		Random m_Rand = new Random();

		// 一口气
		Pos OneEmpty()
		{
			foreach (var block in m_WhiteBlocks) {	// Eat white
				if (block.EmptyCount == 1) {
					List<Step> empties = GetEmpties(block);
					return new Pos(empties[0].Row, empties[0].Col);
				}
			}
			foreach (var block in m_BlackBlocks) {	// Feel black
				if (block.EmptyCount == 1) {
					List<Step> empties = GetEmpties(block);
					return new Pos(empties[0].Row, empties[0].Col);
				}
			}
			return m_InvalidPos;
		}
		private List<Step> GetEmpties(Block block)
		{
			List<Step> empties = new List<Step>();
			foreach (var step in block.Steps) {
				empties = empties.Union(LinkSteps(step, StoneColor.Empty)).ToList();
			}
			return empties;
		}
		List<Step> GetBlackEmpties()
		{
			var steps = new List<Step>();
			foreach (var block in m_BlackBlocks) {
				steps = steps.Union(GetEmpties(block)).ToList();
			}
			return steps;
		}
		List<Step> GetWhiteEmpties()
		{
			var steps = new List<Step>();
			foreach (var block in m_WhiteBlocks) {
				steps = steps.Union(GetEmpties(block)).ToList();
			}
			return steps;
		}
		List<Step> GetBlackEmpties_W()
		{
			return GetBlackEmpties().Except(GetWhiteEmpties()).ToList();
		}
		List<Step> GetWhiteEmpties_B()
		{
			return GetWhiteEmpties().Except(GetBlackEmpties()).ToList();
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
		List<Pos> AllPos()
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

		// Learning UpdateBlocks(), But not use.
		List<PosBlock> m_BlackMeshBlocks = new List<PosBlock>();
		List<PosBlock> m_WhiteMeshBlocks = new List<PosBlock>();
		void UpdateMeshBlocks(List<Pos> poses, List<PosBlock> blocks, StoneColor stoneColor)
		{
			List<Pos> copyPoses = new List<Pos>();
			foreach (var item in poses) {
				copyPoses.Add(item);
			}

			if (copyPoses.Count == 0) return;

			var tmp = new List<Pos>();
			foreach (var pos in copyPoses) {
				if (tmp.Count == 0) tmp.Add(pos);
				var links = LinkPoses(pos);
				if (tmp.Intersect(links).Count() != 0)
					tmp = tmp.Union(links).ToList();
			}
			//foreach (var step in copyPoses) {	// 防止遗漏
			//	var sameLinks = LinkPoses(step);
			//	if (tmp.Intersect(sameLinks).Count() != 0)
			//		tmp = tmp.Union(sameLinks).ToList();
			//}
			if (tmp.Count == 0) return;

			PosBlock block = new PosBlock();
			block.Poses = tmp;
			block.PosColor = stoneColor;
			block.Count = tmp.Count;
			blocks.Add(block);

			copyPoses.RemoveAll(s => tmp.Contains(s));	// next block
			UpdateMeshBlocks(copyPoses, blocks, stoneColor);	// 递归
		}
		void UpdateBlackMeshBlocks()
		{
			m_BlackMeshBlocks.Clear();
			UpdateMeshBlocks(m_BlackMeshes.Union(BlackPoses).ToList(), m_BlackMeshBlocks, StoneColor.Black);

			m_BlackMeshBlocks.ForEach(b => { if (b.Count < 10) b.PosColor = StoneColor.White; });
		}
		void UpdateWhiteMeshBlocks()
		{
			m_WhiteMeshBlocks.Clear();
			UpdateMeshBlocks(m_WhiteMeshes.Union(WhitePoses).ToList(), m_WhiteMeshBlocks, StoneColor.White);

			m_WhiteMeshBlocks.ForEach(b => { if (b.Count < 10) b.PosColor = StoneColor.Black; });
		}

		private void InitMeshes()
		{
			UpdateMeshes();
			AddBlackMeshes();
			AddWhiteMeshes();

			m_BlackMeshes = m_BlackMeshes.Except(m_WhiteMeshes).Union(BlackPoses).ToList();
			m_WhiteMeshes = m_WhiteMeshes.Except(m_BlackMeshes).Union(WhitePoses).ToList();

			UpdateBlackMeshBlocks();
			UpdateWhiteMeshBlocks();

			foreach (var block in m_BlackMeshBlocks) {
				if (block.PosColor == StoneColor.White) {
					foreach (var pos in block.Poses) {
						m_BlackMeshes.Remove(pos);
						m_WhiteMeshes.Add(pos);
					}
				}
			}

			foreach (var block in m_WhiteMeshBlocks) {
				if (block.PosColor == StoneColor.Black) {
					foreach (var pos in block.Poses) {
						m_WhiteMeshes.Remove(pos);
						m_BlackMeshes.Add(pos);
					}
				}
			}
		}
		void UpdateMeshes()
		{
			m_BlackMeshes.Clear();
			foreach (var step in m_BlackSteps) {
				int row = step.Row;
				int col = step.Col;
				m_BlackMeshes.Add(new Pos(row, col));
			}
			foreach (var step in GetBlackEmpties_W()) {
				int row = step.Row;
				int col = step.Col;
				m_BlackMeshes.Add(new Pos(row, col));
			}

			m_WhiteMeshes.Clear();
			foreach (var step in m_WhiteSteps) {
				int row = step.Row;
				int col = step.Col;
				m_WhiteMeshes.Add(new Pos(row, col));
			}
			foreach (var step in GetWhiteEmpties_B()) {
				int row = step.Row;
				int col = step.Col;
				m_WhiteMeshes.Add(new Pos(row, col));
			}
		}
		void AddBlackMeshes()
		{
			List<Pos> poses = new List<Pos>();
			foreach (var pos in m_BlackMeshes) {
				AddLinkPoses(poses, pos);
			}
			m_BlackMeshes = m_BlackMeshes.Union(poses).ToList();
		}
		void AddWhiteMeshes()
		{
			List<Pos> poses = new List<Pos>();
			foreach (var pos in m_WhiteMeshes) {
				AddLinkPoses(poses, pos);
			}
			m_WhiteMeshes = m_WhiteMeshes.Union(poses).ToList();
		}
		// 三四线为实地向下，五六七线为势力向上。
		private void AddLinkPoses(List<Pos> poses, Pos pos)
		{
			if (LineThree().Contains(pos) || LineFour().Contains(pos)) {
				List<Pos> links = LinkPoses(pos);
				foreach (var link in links) {	// 2,3 line
					if (LineTwo().Contains(link) || LineThree().Contains(link)) {
						poses.Add(link);
						var lines = LinkPoses(link);
						foreach (var l in lines) {	// l,2 line
							if (LineOne().Contains(l) || LineTwo().Contains(l)) {
								poses.Add(l);
								var ones = LinkPoses(l);
								foreach (var one in ones) {	// 1 line
									if (LineOne().Contains(one))
										poses.Add(one);
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
						poses.Add(link);
						var lines = LinkPoses(link);
						foreach (var l in lines) {	// 7,8,9 line
							if (LineSeven().Contains(l) || LineEight().Contains(l) || LineNine().Contains(l)) {
								poses.Add(l);
								var nines = LinkPoses(l);
								foreach (var nine in nines) {	// 8,9 line
									if (LineEight().Contains(nine) || LineNine().Contains(nine))
										poses.Add(nine);
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
