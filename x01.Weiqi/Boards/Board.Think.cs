/**
 * Board.Think.cs (c) 2015 by x01
 * ------------------------------
 *	1.此部分类主要是为 AI Board 提供服务，之所以要单独列出来，
 *	  一是内容多，二是为了不影响其他部分。
 *	2.Pos 针对 Step 的 Row、Col，但 Pos 更利于 Think，且不影响
 *	  其他部分，单列出来是有意义的。当然，LinkSteps 与 LinkPoses 
 *	  之类的重复也就在所难免了。
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
		protected List<Pos> AllPoses { get; set; }

		public Pos Think()
		{
			// 下棋是一种由不确定到确定的过程，没有人能证明这步棋不好，那么就可以抢先确定这一步。
			if (StepCount == 0)
				return new Pos(3, 15);

			return StepCount < 10 ? RandDown() : 
				TryNext() != m_InvalidPos ? TryNext() :
				OneEmpty() != m_InvalidPos ? OneEmpty() : 
				RandDown();
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

		Pos RandDown()
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
		Pos TryNext()
		{
			if (StepCount < 100) 
				return m_InvalidPos;

			// InitMeshes(),除去 UpdateMeshes4()
			UpdateMeshes1();
			UpdateMeshes2();
			UpdateMeshes3(2);
			UpdateMeshes3(3);
			UpdateMeshes3(4);
			UpdateMeshes3(5);
			UpdateMeshes3(10); // 多次扫描有必要
			
			var empties = EmptyPoses.ToList();
			var blacks = m_BlackMeshes.ToList();
			int max = -1;
			Pos result = m_InvalidPos;
			
			foreach (var pos in empties) {
				m_BlackMeshes = blacks.ToList();	// reset.
				m_BlackMeshes.Add(pos);

				var tmp = new List<Pos>();
				AddLinkPoses(tmp, pos);
				tmp.ForEach(t => { if (!m_BlackMeshes.Contains(t)) m_BlackMeshes.Add(t); });
				
				UpdateMeshes2();
				UpdateMeshes3(5);
				
				if (max < m_BlackMeshes.Count) {
					max = m_BlackMeshes.Count;
					result = pos;
				}
			}

			if (LineOne().Contains(result) || LineTwo().Contains(result))
				result = m_InvalidPos;

			return result;
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
					PosBlock pb = new PosBlock();
					block.Steps.ForEach(s => pb.Poses.Add(new Pos(s.Row, s.Col, s.StoneColor)));
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
					PosBlock pb = new PosBlock();
					block.Steps.ForEach(s => pb.Poses.Add(new Pos(s.Row, s.Col, s.StoneColor)));
					pb.EmptyCount = block.EmptyCount;
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
					PosBlock pb = new PosBlock();
					block.Steps.ForEach(s => pb.Poses.Add(new Pos(s.Row, s.Col, s.StoneColor)));
					pb.EmptyCount = block.EmptyCount;
					m_WhitePosBlocks.Add(pb);
				}
				return m_WhitePosBlocks;
			}
		}

		//   +
		// + + +	Copy code from LinkSteps().
		//   +
		List<Pos> LinkPoses(Pos pos)
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

			return links;
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

	}
}
