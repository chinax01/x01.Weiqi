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

			if (StepCount < 10)
				return RandDown();

			List<Pos> poses = new List<Pos>();
			Pos pos = Defend();
			if (pos == m_InvalidPos)
				pos = EffectPos(out poses);
			if (pos == m_InvalidPos)
				pos = FindPos(poses);

			return pos != m_InvalidPos ? pos : RandDown();
		}

		#region Think Helper

		Random m_Rand = new Random();

		// 一口气
		Pos OneEmpty()
		{
			UpdateEmptyCount();

			var w_blocks = m_WhiteStepBlocks.OrderByDescending(b => b.Steps.Count).ToList();
			var b_blocks = m_BlackStepBlocks.OrderByDescending(b => b.Steps.Count).ToList();

			foreach (var block in w_blocks) {	// Eat white
				if (block.EmptyCount == 1) {
					List<Step> empties = GetLinkEmptySteps(block);
					Pos pos = new Pos(empties[0].Row, empties[0].Col);
					if (LineOne().Contains(pos))
						continue;
					return pos;
				}
			}
			foreach (var block in b_blocks) {	// Feel black
				if (block.EmptyCount == 1) {
					List<Step> empties = GetLinkEmptySteps(block);
					Pos pos = new Pos(empties[0].Row, empties[0].Col);
					if (LineOne().Contains(pos))
						continue;
					return pos;
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
			block.EmptyCount = empties.Count;
			block.Steps.ForEach(s => s.EmptyCount = block.EmptyCount);

			return empties;
		}
		void UpdateEmptyCount()
		{
			UpdateAllStepBlocks();
			m_BlackStepBlocks.ForEach(b => GetLinkEmptySteps(b));
			m_WhiteStepBlocks.ForEach(b => GetLinkEmptySteps(b));
		}

		Pos RandDown()
		{
			int row = m_Rand.Next(0, 19);
			int col = m_Rand.Next(0, 19);
			var pos = new Pos(row, col);

			if (StepCount < 4) {
				if (GoldHorn().Contains(pos) && LinkPoses(pos).Intersect(EmptyPoses).Count() == 5) // include self
					return pos;
			} else if (StepCount < 40) {
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

		Pos EffectPos(out List<Pos> poses)
		{
			// UpdateMeshes() 开销大，所以应该多干点。
			UpdateMeshes();
			int b_count = m_BlackMeshes.Count;
			int w_count = m_WhiteMeshes.Count;

			// 除去死子
			var deads = new List<Pos>();
			foreach (var block in m_DeadBlocks) {
				var steps = block.Value.Steps;
				foreach (var item in steps) {
					deads.Add(new Pos(item.Row, item.Col));
				}
			}
			var empties = EmptyPoses.Except(deads).ToList();

			var temp = new List<Pos>();
			if (b_count > w_count) {
				foreach (var item in BlackPoses) {
					var rounds = RoundTwoPoses(item).Intersect(empties).ToList();
					foreach (var r in rounds) {
						if (!temp.Contains(r))
							temp.Add(r);
					}
				}
			} else {
				foreach (var item in WhitePoses) {
					var rounds = RoundTwoPoses(item).Intersect(empties).ToList();
					foreach (var r in rounds) {
						if (!temp.Contains(r))
							temp.Add(r);
					}
				}
			}
			if (StepCount < EndCount)
				poses = temp.Union(CurrentEffectPoses).Except(LineOne()).Except(LineTwo()).ToList();	// 根据形势判断决定落子范围
			else
				poses = temp.Union(CurrentEffectPoses).ToList();

			// 根据 UpdateMeshes() 决定是否吃子或做活
			if (StepCount < EndCount) return m_InvalidPos;

			var w_blocks = m_WhiteMeshBlocks.OrderByDescending(b => b.Poses.Count).ToList();
			var b_blocks = m_BlackMeshBlocks.OrderByDescending(b => b.Poses.Count).ToList();
			int count = -1;
			var result = m_InvalidPos;
			foreach (var b in b_blocks) {
				if (b.IsDead) {
					count = b.Poses.Count;
					result = b.KeyPos;
					break;
				}
			}
			foreach (var w in w_blocks) {
				if (w.IsDead && w.Poses.Count > count) {
					result = w.KeyPos;
					break;
				}
			}

			return result;
		}
		private Pos FindPos(List<Pos> poses)
		{
			if (poses.Count == 0) return m_InvalidPos;

			var result = m_InvalidPos;
			var empties = poses.ToList();
			int count = -1;

			bool isPlay = IsPlaySound;
			IsPlaySound = false;
			for (int i = 0; i < 20; i++) {
				int index = m_Rand.Next(0, empties.Count);
				if (!NextOne(empties[index].Row, empties[index].Col))
					continue;
				UpdateMeshes1();
				if (count < m_BlackMeshes.Count - m_WhiteMeshes.Count) {
					count = m_BlackMeshes.Count - m_WhiteMeshes.Count;
					result = empties[index];
				}
				BackOne();
			}
			IsPlaySound = isPlay;

			return result;
		}

		//Pos FindPos(List<Pos> poses)
		//{
		//	var empties = CurrentEffectPoses.Union(poses).Intersect(EmptyPoses).ToList();
		//	int count = empties.Count;
		//	Dictionary<Pos, int> store = new Dictionary<Pos, int>();
		//	int backCount = 0;

		//	for (int j = 0; j < 20; j++) {
		//		bool isFirst = false;
		//		Pos firstPos = m_InvalidPos;
		//		for (int i = 0; i < 4; i++) {
		//			int index = m_Rand.Next(0, count);
		//			Pos e = empties[index];
		//			if (!NextOne(e.Row, e.Col)) continue;
		//			if (!isFirst) {
		//				isFirst = true;
		//				firstPos = e;
		//			}
		//			backCount++;
		//		}
		//		UpdateMeshes1();
		//		store[firstPos] = m_BlackMeshes.Count - m_WhiteMeshes.Count;
		//		for (int b = 0; b < backCount; b++) {
		//			BackOne();
		//		}
		//		backCount = 0;
		//	}

		//	int value = -1;
		//	Pos pos = m_InvalidPos;
		//	foreach (var pair in store) {
		//		if (value < pair.Value) {
		//			value = pair.Value;
		//			pos = pair.Key;
		//		}
		//	}
		//	return pos;
		//}

		Pos Defend()
		{
			UpdateEmptyCount();

			if (OneEmpty() != m_InvalidPos) return OneEmpty();

			var pos = CurrentPos;
			var poses = CurrentEffectPoses;

			var links = LinkPoses(pos);
			var b_links = links.Intersect(BlackPoses).ToList();
			var w_links = links.Intersect(WhitePoses).ToList();
			var e_links = links.Intersect(EmptyPoses).ToList();
			
			// Touch
			if (b_links.Count == 1 && e_links.Count == 3) {
				var b = b_links[0];
				var step = GetStep(b);
				if (step.EmptyCount >= 3) {
					foreach (var item in e_links) {
						if (IsTouch(item, pos) && IsCusp(item, b)
							&& LinkPoses(item).Intersect(EmptyPoses).Count() > 2)
							return item;
					}
				} else if (step.EmptyCount == 2) {
					var b_empties = LinkPoses(b).Intersect(EmptyPoses).ToList();
					foreach (var e in b_empties) {
						if (LinkPoses(e).Intersect(BlackPoses).Count() > 1)
							return e;
					}
				}
			} else if (b_links.Count == 1 && w_links.Count >= 2) {
				var b = b_links[0];
				var w = w_links[0];
				if (GetStep(b).EmptyCount < GetStep(w).EmptyCount) {
					var b_empties = LinkPoses(b).Intersect(EmptyPoses).ToList();
					foreach (var item in b_empties) {
						if (IsTouch(item, b) && IsCusp(item, pos) && !LineOneTwo.Contains(item)) 
							return item;
					}
				}
			} 

			return m_InvalidPos;
		}
		Pos Touch(Pos pos, Pos white) // 碰(CurrentPos Touch black)
		{
			UpdateEmptyCount();
			int b_count = m_Steps[pos.Row, pos.Col].EmptyCount;
			int w_count = m_Steps[white.Row, white.Col].EmptyCount;
			var rounds = RoundOnePoses(pos).Intersect(EmptyPoses);
			foreach (var r in rounds) {
				if (b_count >= w_count && IsTouch(r, white) && IsCusp(r, pos))
					return r;
				else if (b_count < w_count && IsCusp(r, white) && IsTouch(r, pos))
					return r;
			}

			return m_InvalidPos;
		}
		Pos Cusp(Pos pos, Pos white)
		{
			UpdateEmptyCount();
			int b_count = m_Steps[pos.Row, pos.Col].EmptyCount;
			int w_count = m_Steps[white.Row, white.Col].EmptyCount;
			var rounds = RoundOnePoses(pos).Intersect(EmptyPoses);
			foreach (var r in rounds) {
				if (b_count >= w_count && IsTouch(r, white) && IsTouch(r, pos))
					return r;
			}

			return m_InvalidPos;
		}
		Pos JumpOne(Pos pos, Pos white) // 跳 
		{

			UpdateEmptyCount();
			int b_count = m_Steps[pos.Row, pos.Col].EmptyCount;
			int w_count = m_Steps[white.Row, white.Col].EmptyCount;
			var rounds = RoundTwoPoses(pos).Intersect(EmptyPoses);
			foreach (var r in rounds) {
				if (b_count > w_count && (IsJumpOne(r, pos) || IsFlyOne(r, pos))
					&& (IsTouch(r, white) || IsElephant(r, white)))
					return r;
				else if (b_count <= w_count && (IsJumpOne(r, pos) || IsFlyOne(r, pos)))
					return r;
			}

			return m_InvalidPos;
		}
		Pos FlyOne(Pos pos, Pos white) // 飞 
		{
			UpdateEmptyCount();
			int b_count = m_Steps[pos.Row, pos.Col].EmptyCount;
			int w_count = m_Steps[white.Row, white.Col].EmptyCount;
			var rounds = RoundTwoPoses(pos).Intersect(EmptyPoses);
			foreach (var r in rounds) {
				if (b_count > w_count && (IsTouch(r, white) || IsFlyOne(r, pos) || IsJumpOne(r, pos)))
					return r;
				else if (!IsElephant(r, pos))
					return r;
			}
			return m_InvalidPos;
		}
		Pos Elephant(Pos pos, Pos white)
		{

			UpdateEmptyCount();
			int b_count = m_Steps[pos.Row, pos.Col].EmptyCount;
			int w_count = m_Steps[white.Row, white.Col].EmptyCount;
			var rounds = RoundTwoPoses(pos).Intersect(EmptyPoses);
			foreach (var r in rounds) {
				if (b_count > w_count && IsJumpOne(r, pos) && IsJumpOne(r, white))
					return r;
				else if (IsJumpOne(r, pos) || IsFlyOne(r, pos))
					return r;
			}
			return m_InvalidPos;
		}

		#endregion

		#region Pos Helper

		Pos CurrentPos { get { return new Pos(m_CurrentStep.Row, m_CurrentStep.Col); } }
		List<Pos> CurrentEffectPoses { get { return RoundThreePoses(CurrentPos).Intersect(EmptyPoses).ToList(); } }
		Step GetStep(Pos pos)
		{
			return m_Steps[pos.Row, pos.Col];
		}
		StepBlock GetStepBlock(Pos pos)
		{
			UpdateAllStepBlocks();

			int id = GetStep(pos).BlockId;
			foreach (var block in m_BlackStepBlocks) {
				if (id == block.Id) return block;
			}
			foreach (var block in m_WhiteStepBlocks) {
				if (id == block.Id) return block;
			}
			foreach (var block in m_EmptyStepBlocks) {
				if (id == block.Id) return block;
			}

			return null;
		}

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
			var links = new List<Pos>();
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
		// + + +
		// + + +	return shape
		// + + +
		List<Pos> RoundOnePoses(Pos pos)
		{
			var links = new List<Pos>();
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if (InRange(pos.Row + i, pos.Col + j)) {
						links.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return links;
		}
		List<Pos> RoundTwoPoses(Pos pos)
		{
			var links = new List<Pos>();
			for (int i = -2; i < 3; i++) {
				for (int j = -2; j < 3; j++) {
					if (InRange(pos.Row + i, pos.Col + j)) {
						links.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return links;
		}
		List<Pos> RoundThreePoses(Pos pos)
		{
			var links = new List<Pos>();
			for (int i = -3; i < 4; i++) {
				for (int j = -3; j < 4; j++) {
					if (InRange(pos.Row + i, pos.Col + j)) {
						links.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
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

		List<Pos> GoldHorn()
		{
			List<Pos> goldHorn = new List<Pos>();
			Pos pos;
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if ((i + 3 == 4 && (j + 3 == 4 || j + 15 == 14))
						|| (i + 15 == 14 && (j + 3 == 4 || j + 15 == 14))) {
						continue; // remove 5.5
					}

					pos = new Pos(3 + i, 3 + j);
					goldHorn.Add(pos);
					pos = new Pos(15 + i, 15 + j);
					goldHorn.Add(pos);
					pos = new Pos(3 + i, 15 + j);
					goldHorn.Add(pos);
					pos = new Pos(15 + i, 3 + j);
					goldHorn.Add(pos);
				}
			}
			return goldHorn;
		}

		List<Pos> LineOneTwo { get { return LineOne().Union(LineTwo()).ToList(); } }

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
		bool IsElephant(Pos p1, Pos p2)
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
