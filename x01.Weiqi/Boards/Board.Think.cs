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
using x01.Weiqi.Core;

namespace x01.Weiqi.Boards
{
	partial class Board
	{
		Random m_Rand = new Random();
		protected readonly Pos m_InvalidPos = new Pos(-1, -1);
		protected List<Pos> AllPoses { get; set; }
		List<Pos> m_ThinkPoses = new List<Pos>(); // for FindPoses()

		public Pos Think()
		{
			// 下棋是一种由不确定到确定的过程，没有人能证明这步棋不好，那么就可以抢先确定这一步。
			if (StepCount == 0)
				return new Pos(3, 3);
			if (TwoStar() != m_InvalidPos)
				return TwoStar();
			
			FindPoses();

			return BestPos();
		}

		#region Think Helper

		Pos CompareEmpty()
		{
			Pos pos = OneEmpty();
			if (pos != m_InvalidPos) return pos;
			pos = TwoEmpty();
			pos = CheckBlackEmpty(pos);
			if (pos != m_InvalidPos) return pos;
			return m_InvalidPos;

		}

		private Pos CheckBlackEmpty(Pos pos)
		{
			var bs = LinkPoses(pos).Intersect(BlackPoses).ToList();
			int max = int.MinValue;
			foreach (var b in bs) {
				if (max < GetStep(b).EmptyCount)
					max = GetStep(b).EmptyCount;
			}
			if (max <= 2) return m_InvalidPos;
			return pos;
		}
		List<Pos> m_LevyPoses = new List<Pos>();
		Pos OneEmpty()  // 一口气
		{
			UpdateEmptyCount();

			m_LevyPoses.Clear();

			var w_blocks = m_WhiteStepBlocks.OrderByDescending(b => b.Steps.Count).ToList();
			var b_blocks = m_BlackStepBlocks.OrderByDescending(b => b.Steps.Count).ToList();

			foreach (var block in w_blocks) {   // Eat white
				if (block.EmptyCount == 1) {
					List<Step> empties = GetLinkEmptySteps(block);
					Pos pos = new Pos(empties[0].Row, empties[0].Col);
					if (new Pos(m_BanOnce.Row, m_BanOnce.Col) == pos)
						continue;
					return pos;
				}
			}
			foreach (var block in b_blocks) {   // Feel black
				if (block.EmptyCount == 1) {
					List<Step> empties = GetLinkEmptySteps(block);
					Pos pos = new Pos(empties[0].Row, empties[0].Col);
					var blinks = LinkPoses(pos).Intersect(BlackPoses).ToList();
					if (blinks.Count >= 2) {  //有子接应
						bool ok = false;
						foreach (var b in blinks) {
							if (GetStep(b).EmptyCount >= 2) {
								ok = true;
								break;
							}
						}
						if (ok) return pos;
					}
					var links = LinkPoses(pos).Intersect(EmptyPoses).ToList();
					links.Remove(pos);
					if (links.Count <= 1) continue;
					if (links.Count == 2) {
						Pos p1, p2;
						var links0_w = LinkPoses(links[0]).Intersect(WhitePoses).ToList();
						var links1_w = LinkPoses(links[1]).Intersect(WhitePoses).ToList();
						if (links0_w.Count == 1) {
							p2 = links[0]; p1 = links[1];
							if (CanLevy(p1, p2, pos)) {
								if (!m_LevyPoses.Contains(pos))
									m_LevyPoses.Add(pos);
								continue;
							}
						} else if (links1_w.Count == 1) {
							p2 = links[1]; p1 = links[0];
							if (CanLevy(p1, p2, pos)) {
								if (!m_LevyPoses.Contains(pos))
									m_LevyPoses.Add(pos);
								continue;
							}
						}
					}
					if ((LinkPoses(pos).Intersect(BlackPoses).Count() < 2) &&
						(LineOne.Contains(pos) || new Pos(m_BanOnce.Row, m_BanOnce.Col) == pos))    // 暂不考虑征子有利
						continue;
					return pos;
				}
			}
			return m_InvalidPos;
		}
		Pos TwoEmpty()
		{
			UpdateEmptyCount();

			var w_blocks = m_WhiteStepBlocks.OrderByDescending(b => b.Steps.Count).ToList();
			var b_blocks = m_BlackStepBlocks.OrderByDescending(b => b.Steps.Count).ToList();

			foreach (var w_block in w_blocks) {     // 吃白
				if (w_block.EmptyCount == 2) {
					var empties = GetLinkEmptySteps(w_block);
					Pos p = GetPos(w_block, empties);
					var p1 = GetPos(empties[0]);
					var p2 = GetPos(empties[1]);
					if (LineTwo.Contains(p) && LineTwo.Contains(p1)
						&& LinkPoses(p1).Intersect(EmptyPoses).Count() > 2) {
						return p1;
					} else if (LineTwo.Contains(p) && LineTwo.Contains(p2)
						&& LinkPoses(p2).Intersect(EmptyPoses).Count() > 2) {
						return p2;
					}
					if (FourSharp.Contains(p1) || FourSharp.Contains(p2)) continue;
					if (LinkPoses(p1).Intersect(BlackPoses).Count() >= 1 && CanLevy(p1, p2, p, false)) { // 有黑子帮忙
						if (LinkPoses(p2).Intersect(WhitePoses).Count() >= 3) continue;// 虎口
						return p2;
					} else if (LinkPoses(p2).Intersect(BlackPoses).Count() >= 1 && CanLevy(p2, p1, p, false)) {
						if (LinkPoses(p1).Intersect(WhitePoses).Count() >= 3) continue;
						return p1;
					}

					// 双叫吃
					var p1_links = LinkPoses(p1).Intersect(WhitePoses).ToList();
					var p2_links = LinkPoses(p2).Intersect(WhitePoses).ToList();
					foreach (var p1_l in p1_links) {
						if (IsCusp(p1_l, p) && LinkPoses(p1_l).Intersect(EmptyPoses).Count() == 2) {
							if (GetStep(p1_l).EmptyCount != 2) continue;
							if (LinkPoses(p1).Intersect(WhitePoses).Count() >= 3) continue;
							if (LinkPoses(p1).Intersect(EmptyPoses).Count() == 2 && LineOne.Contains(p1)) continue;
							return p1;
						}
					}
					foreach (var p2_l in p2_links) {
						if (IsCusp(p2_l, p) && LinkPoses(p2_l).Intersect(EmptyPoses).Count() == 2) {
							if (GetStep(p2_l).EmptyCount != 2) continue;
							if (LinkPoses(p2).Intersect(WhitePoses).Count() >= 3) continue;
							if (LinkPoses(p2).Intersect(EmptyPoses).Count() == 2 && LineOne.Contains(p2)) continue;
							return p2;
						}
					}

					// 三路 
					if (LineThree.Contains(p1) && LineTwo.Contains(p2)) {
						if (LinkPoses(p1).Intersect(WhitePoses).Count() >= 3) continue;
						return p1;
					} else if (LineThree.Contains(p2) && LineTwo.Contains(p1)) {
						if (LinkPoses(p2).Intersect(WhitePoses).Count() >= 3) continue;
						return p2;
					}

					// 二路
					if (LineTwo.Contains(p1)) {
						if (LinkPoses(p1).Intersect(WhitePoses).Count() < 3) {
							return p1;
						}
					} else if (LineTwo.Contains(p2)) {
						if ((LinkPoses(p2).Intersect(WhitePoses).Count() < 3))
							return p2;
					} else if (LineOne.Contains(p1) && LineOne.Contains(p2)) {
						if (LinkPoses(p1).Intersect(WhitePoses).Count() == 1)
							return p1;
						if ((LinkPoses(p2).Intersect(WhitePoses).Count() == 1))
							return p2;
					}

					// 枷
					if (LinkPoses(p1).Intersect(BlackPoses).Count() == 1 && LinkPoses(p2).Intersect(BlackPoses).Count() == 1) {
						var e_rounds = RoundPoses(p).Intersect(EmptyPoses).ToList();
						foreach (var e in e_rounds) {
							if (IsCusp(e, p) && IsTouch(e, p1) && IsTouch(e, p2))
								return e;
						}
					}

					// 打断飞
					var r2 = RoundTwoPoses(p).Intersect(WhitePoses).ToList();
					foreach (var r in r2) {
						if (IsFlyOne(p, r) && IsCusp(p1, r)
							&& RoundPoses(r).Intersect(EmptyPoses).Count() == 8) {
							return p1;
						} else if (IsFlyOne(p, r) && IsCusp(p2, r)
								   && RoundPoses(r).Intersect(EmptyPoses).Count() == 8) {
							return p2;
						}
					}
				}
			}

			foreach (var b_block in b_blocks) {     // 逃黑
				if (b_block.EmptyCount == 2) {
					var empties = GetLinkEmptySteps(b_block);
					Pos p = GetPos(b_block, empties);
					var p1 = GetPos(empties[0]);
					var p2 = GetPos(empties[1]);

					var p1_links = LinkPoses(p1).Intersect(BlackPoses).ToList();
					var p2_links = LinkPoses(p2).Intersect(BlackPoses).ToList();
					foreach (var p1_l in p1_links) {
						if (IsCusp(p1_l, p) && GetStep(p1_l).EmptyCount == 2) {
							if (LinkPoses(p1).Intersect(BlackPoses).Count() >= 3) continue;
							return p1;
						} else if (LineOne.Contains(p) && IsCusp(p1_l, p)
								   && GetStep(p1_l).EmptyCount > 2) {
							foreach (var e in EmptyPoses) {
								if (IsCusp(e, p) && LinkPoses(e).Intersect(EmptyPoses).Count() == 5)
									return e;
								else if (IsCusp(e, p) && LineOne.Contains(e) &&
										 LinkPoses(e).Intersect(EmptyPoses).Count() == 4) {
									return e;
								}
							}
						}
					}
					foreach (var p2_l in p2_links) {
						if (IsCusp(p2_l, p) && GetStep(p2_l).EmptyCount == 2) {
							if (LinkPoses(p2).Intersect(BlackPoses).Count() >= 3) continue;
							return p2;
						} else if (LineOne.Contains(p) && IsCusp(p2_l, p)
								   && GetStep(p2_l).EmptyCount > 2) {
							foreach (var e in EmptyPoses) {
								if (IsCusp(e, p)
									&& LinkPoses(e).Intersect(EmptyPoses).Count() == 5)
									return e;
								else if (IsCusp(e, p) && LineOne.Contains(e) &&
										 LinkPoses(e).Intersect(EmptyPoses).Count() == 4) {
									return e;
								}
							}
						}
					}
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

		private Pos GetPos(StepBlock block, List<Step> empties) // For CanLevy()
		{
			Pos p = m_InvalidPos;

			if (empties.Count != 2)
				return p;

			foreach (var item in block.Steps) {
				var i_empties = LinkSteps(item);
				if (i_empties.Contains(empties[0]) && i_empties.Contains(empties[1])) {
					p = GetPos(item);
					break;
				}
			}
			return p;
		}
		// 征子判断，p1, p2 为气，p2 为前进方向，p 为逃跑之子。
		bool CanLevy(Pos p1, Pos p2, Pos p, bool isBlack = true)
		{
			if (!IsCusp(p1, p2)) return true;
			if (p == m_InvalidPos) return true;

			List<Pos> selfPoses = isBlack ? BlackPoses : WhitePoses;
			List<Pos> otherPoses = !isBlack ? BlackPoses : WhitePoses;

			// 征而被叫，岂不大笑？
			var p1_links = LinkPoses(p1).Intersect(otherPoses).ToList();
			if (p1_links.Count == 1 && LinkPoses(p1_links[0]).Intersect(EmptyPoses).Count() == 2)
				return isBlack ? false : true; ;
			var p2_links = LinkPoses(p2).Intersect(otherPoses).ToList();
			if (p2_links.Count == 1 && LinkPoses(p2_links[0]).Intersect(EmptyPoses).Count() == 2)
				return isBlack ? false : true;

			int count = 0;
			while (true) {
				if (!Helper.InRange(p2.Row, p2.Col))
					break;
				bool isRow = p2.Row - p.Row == 0 ? true : false;
				int rowOffset = isRow ? (count == 0 ? p1.Row - p2.Row : p2.Row - p1.Row) : 0;
				int colOffset = isRow ? 0 : (count == 0 ? p1.Col - p2.Col : p2.Col - p1.Col);
				Pos pos = new Pos(p2.Row + rowOffset, p2.Col + colOffset);
				var rounds = count < 5 ? LinkPoses(pos) : RoundTwoPoses(pos);
				var c_rounds = rounds.ToList();
				foreach (var c in c_rounds) {
					if (IsElephantOne(c, pos))
						rounds.Remove(c);       // 去角
				}
				foreach (var r in rounds) {
					if (selfPoses.Contains(r))
						return false;
					if (isBlack && count < 2) continue; // 黑需先走两步
					if (otherPoses.Contains(r)) {
						return true;
					}
				}
				count++;
				p1 = p;
				p = p2;
				p2 = pos;
			}

			return true;
		}

		List<Pos> m_BestPoses = new List<Pos>();
		Pos BestPos()
		{
			m_BestPoses.Clear();
			var empties = EmptyPoses.ToList();
			foreach (var e in empties) {
				UpdateMeshWorths(e);
				int worth = m_BlackMeshes.Count - m_WhiteMeshes.Count;
				var p = new Pos(e.Row,e.Col,e.StoneColor,e.StepCount, worth);
				if (!m_BestPoses.Contains(p))
					m_BestPoses.Add(p);
			}
			
			var poses = m_BestPoses.OrderByDescending(p=>p.Worth);
			int repeat = 0;
			foreach (var p in poses) {
				if (m_ThinkPoses.Contains(p)) return p;
				if (repeat++ < 10) {
					m_ShapeThink.CurrentPos = p;
					var temp = m_ShapeThink.Think(R.Game);
					m_ShapeThink.CurrentPos = Helper.InvalidPos;
					if (temp.Contains(p)) return p;
				}
			}
			
			if (m_StepCount > 260) {
				ShowMeshes();  // game over
			}
			
			return poses.First();
		}
		
		private void FindPoses()
		{
			m_ThinkPoses.Clear();
			
			var temp = m_ShapeThink.Think(R.Game);
			m_ThinkPoses.AddRange(temp);
			
			temp = m_PatternThink.Think(R.Pattern);
			m_ThinkPoses.AddRange(temp);
			
			var oneEmpty = OneEmpty();
			if (m_ThinkPoses.Contains(oneEmpty))
				m_ThinkPoses.Add(oneEmpty);
			
			//UpdateMeshWorths(m_InvalidPos);
			//m_ThinkPoses.AddRange(m_DeadLifeKeyPoses);
		}

		#endregion

		#region Pos Helper

		public Pos CurrentPos { get { return new Pos(m_CurrentStep.Row, m_CurrentStep.Col); } }
		List<Pos> CurrentEffectEmptyPoses { get { return RoundFivePoses(CurrentPos).Intersect(EmptyPoses).ToList(); } }
		List<Pos> CurrentEffectBlackPoses { get { return RoundFivePoses(CurrentPos).Intersect(BlackPoses).ToList(); } }
		List<Pos> CurrentEffectWhitePoses { get { return RoundFivePoses(CurrentPos).Intersect(WhitePoses).ToList(); } }
		List<Pos> CurrentEffectAllPoses { get { return RoundFivePoses(CurrentPos); } }
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
		Pos GetPos(Step step)
		{
			return new Pos(step.Row, step.Col, step.StoneColor, step.StepCount);
		}

		// 使用属性而不是字段
		List<Pos> m_BlackPoses = new List<Pos>();
		public List<Pos> BlackPoses
		{
			get
			{
				m_BlackPoses.Clear();
				foreach (var step in m_BlackSteps) {
					m_BlackPoses.Add(new Pos(step.Row, step.Col, step.StoneColor, step.StepCount));
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
					m_WhitePoses.Add(new Pos(step.Row, step.Col, step.StoneColor, step.StepCount));
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
					m_EmptyPoses.Add(new Pos(step.Row, step.Col, step.StoneColor, step.StepCount));
				}
				return m_EmptyPoses;
			}
		}
		List<Pos> m_DeadPoses = new List<Pos>();
		public List<Pos> DeadPoses
		{
			get
			{
				m_DeadPoses.Clear();
				foreach (var block in m_DeadBlocks) {
					var steps = block.Value.Steps;
					foreach (var step in steps) {
						var pos = GetPos(step);
						m_DeadPoses.Add(pos);
					}
				}
				return m_DeadPoses;
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
					block.Steps.ForEach(s => pb.Poses.Add(new Pos(s.Row, s.Col)));
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
					block.Steps.ForEach(s => pb.Poses.Add(new Pos(s.Row, s.Col)));
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
					block.Steps.ForEach(s => pb.Poses.Add(new Pos(s.Row, s.Col)));
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
					if (Helper.InRange(pos.Row + i, pos.Col + j)) {
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
		List<Pos> RoundPoses(Pos pos) // one
		{
			var rounds = new List<Pos>();
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if (Helper.InRange(pos.Row + i, pos.Col + j)) {
						rounds.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return rounds;
		}
		List<Pos> RoundTwoPoses(Pos pos)
		{
			var rounds = new List<Pos>();
			for (int i = -2; i < 3; i++) {
				for (int j = -2; j < 3; j++) {
					if (Helper.InRange(pos.Row + i, pos.Col + j)) {
						rounds.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return rounds;
		}
		List<Pos> RoundThreePoses(Pos pos)
		{
			var rounds = new List<Pos>();
			for (int i = -3; i < 4; i++) {
				for (int j = -3; j < 4; j++) {
					if (Helper.InRange(pos.Row + i, pos.Col + j)) {
						rounds.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return rounds;
		}
		List<Pos> RoundFourPoses(Pos pos)
		{
			var rounds = new List<Pos>();
			for (int i = -4; i < 5; i++) {
				for (int j = -4; j < 5; j++) {
					if (Helper.InRange(pos.Row + i, pos.Col + j)) {
						rounds.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return rounds;
		}
		List<Pos> RoundFivePoses(Pos pos)
		{
			var rounds = new List<Pos>();
			for (int i = -5; i < 6; i++) {
				for (int j = -5; j < 6; j++) {
					if (Helper.InRange(pos.Row + i, pos.Col + j)) {
						rounds.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return rounds;
		}
		List<Pos> RoundSixPoses(Pos pos)
		{
			var rounds = new List<Pos>();
			for (int i = -6; i < 7; i++) {
				for (int j = -6; j < 7; j++) {
					if (Helper.InRange(pos.Row + i, pos.Col + j)) {
						rounds.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return rounds;
		}
		List<Pos> RoundSevenPoses(Pos pos)
		{
			var rounds = new List<Pos>();
			for (int i = -7; i < 8; i++) {
				for (int j = -7; j < 8; j++) {
					if (Helper.InRange(pos.Row + i, pos.Col + j)) {
						rounds.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return rounds;
		}
		List<Pos> RoundEightPoses(Pos pos)
		{
			var rounds = new List<Pos>();
			for (int i = -8; i < 9; i++) {
				for (int j = -8; j < 9; j++) {
					if (Helper.InRange(pos.Row + i, pos.Col + j)) {
						rounds.Add(new Pos(pos.Row + i, pos.Col + j));
					}
				}
			}
			return rounds;
		}

		// Nine Star
		Pos StarLeftTop { get { return new Pos(3, 3); } }
		Pos StarTop { get { return new Pos(9, 3); } }
		Pos StarRightTop { get { return new Pos(15, 3); } }
		Pos StarLeft { get { return new Pos(3, 9); } }
		Pos StarCenter { get { return new Pos(9, 9); } }
		Pos StarRight { get { return new Pos(15, 9); } }
		Pos StarLeftBottom { get { return new Pos(3, 15); } }
		Pos StarBottom { get { return new Pos(9, 15); } }
		Pos StarRightBottom { get { return new Pos(15, 15); } }
		List<Pos> m_NineStars = null;
		List<Pos> NineStars
		{
			get
			{
				if (m_NineStars != null) return m_NineStars;
				m_NineStars = new List<Pos>();
				m_NineStars.Add(StarLeftTop);
				m_NineStars.Add(StarTop);
				m_NineStars.Add(StarRightTop);
				m_NineStars.Add(StarLeft);
				m_NineStars.Add(StarCenter);
				m_NineStars.Add(StarRight);
				m_NineStars.Add(StarLeftBottom);
				m_NineStars.Add(StarBottom);
				m_NineStars.Add(StarRightBottom);
				return m_NineStars;
			}
		}

		// Three-Three
		Pos Three3LeftTop { get { return new Pos(2, 2); } }
		Pos Three3RightTop { get { return new Pos(2, 16); } }
		Pos Three3LeftBottom { get { return new Pos(16, 2); } }
		Pos Three3RightBottom { get { return new Pos(16, 16); } }

		// 小目、高目
		List<Pos> m_LeftTopEyes = null;
		List<Pos> LeftTopEyes
		{
			get
			{
				if (m_LeftTopEyes != null) return m_LeftTopEyes;

				m_LeftTopEyes = LinkPoses(StarLeftTop);
				m_LeftTopEyes.Remove(StarLeftTop);
				return m_LeftTopEyes;
			}
		}
		List<Pos> m_RightTopEyes = null;
		List<Pos> RightTopEyes
		{
			get
			{
				if (m_RightTopEyes != null) return m_RightTopEyes;

				m_RightTopEyes = LinkPoses(StarRightTop);
				m_RightTopEyes.Remove(StarRightTop);
				return m_RightTopEyes;
			}
		}
		List<Pos> m_LeftBottomEyes = null;
		List<Pos> LeftBottomEyes
		{
			get
			{
				if (m_LeftBottomEyes != null) return m_LeftBottomEyes;

				m_LeftBottomEyes = LinkPoses(StarLeftBottom);
				m_LeftBottomEyes.Remove(StarLeftBottom);
				return m_LeftBottomEyes;
			}
		}
		List<Pos> m_RightBottomEyes = null;
		List<Pos> RightBottomEyes
		{
			get
			{
				if (m_RightBottomEyes != null) return m_RightBottomEyes;

				m_RightBottomEyes = LinkPoses(StarRightBottom);
				m_RightBottomEyes.Remove(StarRightBottom);
				return m_RightBottomEyes;
			}
		}

		List<Pos> m_AllEyes = null;
		List<Pos> AllEyes
		{
			get
			{
				if (m_AllEyes != null) return m_AllEyes;

				m_AllEyes = LeftTopEyes.Union(RightTopEyes).Union(LeftBottomEyes).Union(RightBottomEyes).ToList();
				return m_AllEyes;
			}
		}
		List<Pos> m_ThreeThrees = null;
		List<Pos> ThreeThrees
		{
			get
			{
				if (m_ThreeThrees != null) return m_ThreeThrees;

				m_ThreeThrees = new List<Pos>();
				m_ThreeThrees.Add(Three3LeftTop);
				m_ThreeThrees.Add(Three3RightTop);
				m_ThreeThrees.Add(Three3LeftBottom);
				m_ThreeThrees.Add(Three3RightBottom);
				return m_ThreeThrees;
			}
		}
		List<Pos> m_StarFourHorn = null;
		List<Pos> StarFourHorn
		{
			get
			{
				if (m_StarFourHorn != null) return m_StarFourHorn;

				m_StarFourHorn = new List<Pos>();
				List<Pos> stars = m_StarFourHorn;
				stars.Add(StarLeftTop);
				stars.Add(StarRightTop);
				stars.Add(StarLeftBottom);
				stars.Add(StarRightBottom);
				return stars;
			}
		}
		List<Pos> m_GoldHorn = null;
		List<Pos> GoldHorn
		{
			get
			{
				if (m_GoldHorn != null) return m_GoldHorn;

				m_GoldHorn = StarFourHorn.Union(ThreeThrees).Union(AllEyes).ToList();
				return m_GoldHorn;
			}
		}

		List<Pos> m_AreaLeftTop = null;
		List<Pos> AreaLeftTop
		{
			get
			{
				if (m_AreaLeftTop != null) return m_AreaLeftTop;

				m_AreaLeftTop = new List<Pos>();
				List<Pos> area = m_AreaLeftTop;
				for (int row = 0; row < 10; row++) {
					for (int col = 0; col < 10; col++) {
						area.Add(new Pos(row, col));
					}
				}
				return area;
			}
		}
		List<Pos> m_AreaRightTop = null;
		List<Pos> AreaRightTop
		{
			get
			{
				if (m_AreaRightTop != null) return m_AreaRightTop;

				m_AreaRightTop = new List<Pos>();
				List<Pos> area = m_AreaRightTop;
				for (int row = 0; row < 10; row++) {
					for (int col = 10; col < 19; col++) {
						area.Add(new Pos(row, col));
					}
				}
				return area;
			}
		}
		List<Pos> m_AreaLeftBottom = null;
		List<Pos> AreaLeftBottom
		{
			get
			{
				if (m_AreaLeftBottom != null) return m_AreaLeftBottom;

				m_AreaLeftBottom = new List<Pos>();
				List<Pos> area = m_AreaLeftBottom;
				for (int row = 10; row < 19; row++) {
					for (int col = 0; col < 10; col++) {
						area.Add(new Pos(row, col));
					}
				}
				return area;
			}
		}
		List<Pos> m_AreaRightBottom = null;
		List<Pos> AreaRightBottom
		{
			get
			{
				if (m_AreaRightBottom != null) return m_AreaRightBottom;

				m_AreaRightBottom = new List<Pos>();
				List<Pos> area = m_AreaRightBottom;
				for (int row = 10; row < 19; row++) {
					for (int col = 10; col < 19; col++) {
						area.Add(new Pos(row, col));
					}
				}
				return area;
			}
		}

		List<Pos> m_LineOneTwo = null;
		List<Pos> LineOneTwo
		{
			get
			{
				if (m_LineOneTwo != null) return m_LineOneTwo;
				m_LineOneTwo = LineOne.Union(LineTwo).ToList();
				return m_LineOneTwo;
			}
		}
		List<Pos> m_LineThreeFour = null;
		List<Pos> LineThreeFour
		{
			get
			{
				if (m_LineThreeFour != null) return m_LineThreeFour;
				m_LineThreeFour = LineThree.Union(LineFour).ToList();
				return m_LineThreeFour;
			}
		}

		List<Pos> m_FourSharp = null;
		List<Pos> FourSharp
		{
			get
			{
				if (m_FourSharp != null) return m_FourSharp;
				m_FourSharp = new List<Pos>();
				m_FourSharp.Add(new Pos(0, 0));
				m_FourSharp.Add(new Pos(0, 18));
				m_FourSharp.Add(new Pos(18, 0));
				m_FourSharp.Add(new Pos(18, 18));
				return m_FourSharp;
			}
		}

		enum LineFlag { One, Two, Three, Four, Five, Six, Seven, Eight, Nine }
		List<Pos> m_LineOne = null;
		List<Pos> LineOne
		{
			get
			{
				if (m_LineOne != null) return m_LineOne;
				m_LineOne = GetLines(LineFlag.One);
				return m_LineOne;
			}
		}
		List<Pos> m_LineTwo = null;
		List<Pos> LineTwo
		{
			get
			{
				if (m_LineTwo != null) return m_LineTwo;

				m_LineTwo = GetLines(LineFlag.Two);
				return m_LineTwo;
			}
		}
		List<Pos> m_LineThree = null;
		List<Pos> LineThree
		{
			get
			{
				if (m_LineThree != null) return m_LineThree;

				m_LineThree = GetLines(LineFlag.Three);
				return m_LineThree;
			}
		}
		List<Pos> m_LineFour = null;
		List<Pos> LineFour
		{
			get
			{
				if (m_LineFour != null) return m_LineFour;

				m_LineFour = GetLines(LineFlag.Four);
				return m_LineFour;
			}
		}
		List<Pos> m_LineFive = null;
		List<Pos> LineFive
		{
			get
			{
				if (m_LineFive != null) return m_LineFive;

				m_LineFive = GetLines(LineFlag.Five);
				return m_LineFive;
			}
		}
		List<Pos> m_LineSix = null;
		List<Pos> LineSix
		{
			get
			{
				if (m_LineSix != null) return m_LineSix;

				m_LineSix = GetLines(LineFlag.Six);
				return m_LineSix;
			}
		}
		List<Pos> m_LineSeven = null;
		List<Pos> LineSeven
		{
			get
			{
				if (m_LineSeven != null) return m_LineSeven;
				m_LineSeven = GetLines(LineFlag.Seven);
				return m_LineSeven;
			}
		}
		List<Pos> m_LineEight = null;
		List<Pos> LineEight
		{
			get
			{
				if (m_LineEight != null) return m_LineEight;

				m_LineEight = GetLines(LineFlag.Eight);
				return m_LineEight;
			}
		}
		List<Pos> m_LineNine = null;
		List<Pos> LineNine
		{
			get
			{
				if (m_LineNine != null) return m_LineNine;

				m_LineNine = GetLines(LineFlag.Nine);
				return m_LineNine;
			}
		}
		List<Pos> m_LineCenter = null;
		List<Pos> LineCenter
		{
			get
			{
				if (m_LineCenter != null) return m_LineCenter;
				m_LineCenter = LineThree.Union(LineFour).Union(LineFive).Union(LineSix)
					.Union(LineSeven).Union(LineEight).Union(LineNine).ToList();    // Line3-9 area
				if (!m_LineCenter.Contains(StarCenter))
					m_LineCenter.Add(StarCenter);
				return m_LineCenter;
			}
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
		bool IsFlyX(Pos p1, Pos p2, int x)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(1, x + 1).Length;
		}
		bool IsJumpOne(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(0, 2).Length;
		}
		bool IsJumpTwo(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(0, 3).Length;
		}
		bool IsJumpX(Pos p1, Pos p2, int x)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(0, x + 1).Length;
		}
		bool IsElephantOne(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(2, 2).Length;
		}
		bool IsElephantTwo(Pos p1, Pos p2)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(3, 3).Length;
		}
		bool IsElephantX(Pos p1, Pos p2, int x)
		{
			return new Vector(p1.Row - p2.Row, p1.Col - p2.Col).Length == new Vector(x + 1, x + 1).Length;
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
