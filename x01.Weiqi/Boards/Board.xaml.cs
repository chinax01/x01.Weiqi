using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using x01.Weiqi.Models;
using x01.Weiqi.Windows;

namespace x01.Weiqi.Boards
{
	/// <summary>
	/// Board.xaml 的交互逻辑
	/// </summary>
	public partial class Board : UserControl
	{
		public Board()
		{
			InitializeComponent();

			IsShowNumber = true;
			IsShowCurrent = true;
			IsShowMesh = false;
			IsPlaySound = false;

			AllPoses = new List<Pos>();

			Init();
		}

		public bool IsShowNumber { get; set; }		// 是否显示步数
		public bool IsShowCurrent { get; set; }		// 显示当前标志
		public bool IsShowMesh { get; set; }		// 点目
		public bool IsPlaySound { get; set; }		// 播放声音
		// 控制棋盘和棋子大小
		int m_StoneSize = 38;
		public int StoneSize
		{
			get { return m_StoneSize; }
			set
			{
				m_StoneSize = value;
				Width = Height = m_StoneSize * 19;
				m_CurrentRect.Width = m_CurrentRect.Height = m_StoneSize / 3.8;
			}
		}

		// 棋步计数
		int m_StepCount = 0;
		public int StepCount
		{
			get { return m_StepCount; }
			set { m_StepCount = value; }
		}

		public void Redraw()
		{
			// 画线
			for (int i = 0; i < 19; i++) {
				Line l = m_LinesH[i];
				int y = i * StoneSize + StoneSize / 2;
				l.X1 = StoneSize / 2;
				l.Y1 = y;
				l.X2 = 19 * StoneSize - StoneSize / 2;
				l.Y2 = y;

				l = m_LinesV[i];
				int x = i * StoneSize + StoneSize / 2;
				l.X1 = x;
				l.Y1 = StoneSize / 2;
				l.X2 = x;
				l.Y2 = 19 * StoneSize - StoneSize / 2;
			}

			// 画星
			for (int j = 0; j < 3; j++) {
				for (int i = 0; i < 3; i++) {
					Ellipse e = m_Stars[i, j];
					e.Width = e.Height = StoneSize / 3;
					double left = 4 * StoneSize + j * 6 * StoneSize - StoneSize / 2 - e.Width / 2;
					double top = 4 * StoneSize + i * 6 * StoneSize - StoneSize / 2 - e.Height / 2;
					Canvas.SetLeft(e, left);
					Canvas.SetTop(e, top);
				}
			}

			// Stones and Numbers
			for (int i = 0; i < 19; i++) {
				for (int j = 0; j < 19; j++) {
					var stone = m_Stones[i, j];
					stone.Width = stone.Height = StoneSize;
					Canvas.SetLeft(stone, j * StoneSize);
					Canvas.SetTop(stone, i * StoneSize);

					ShowNumber(i, j, m_Steps[i, j].StepCount);
				}
			}

			// 点目标志
			if (IsShowMesh)
				for (int i = 0; i < 19; i++) {
					for (int j = 0; j < 19; j++) {
						var rect = m_MeshRects[i, j];
						rect.Width = rect.Height = m_CurrentRect.Width;
						double offset = (StoneSize - rect.Width) / 2.0;
						Canvas.SetLeft(rect, j * StoneSize + offset);
						Canvas.SetTop(rect, i * StoneSize + offset);
					}
				}
		}

		public bool NextOne(int row, int col)
		{
			if (m_Steps[row, col].StoneColor != StoneColor.Empty)
				return false;
			if (m_BanOnce.Row == row && m_BanOnce.Col == col) {
				return false;
			}
			m_BanOnce.Row = m_BanOnce.Col = -1;

			DrawStep(row, col);
			bool isBlack;
			if (m_Steps[row, col].StoneColor == StoneColor.Black) {
				m_BlackSteps.Add(m_Steps[row, col]);
				isBlack = true;
			} else {
				m_WhiteSteps.Add(m_Steps[row, col]);
				isBlack = false;
			}
			m_EmptySteps.Remove(m_Steps[row, col]);

			UpdateAllStepBlocks();
			if (isBlack) {
				if (!UpdateDeadBlocks(m_WhiteStepBlocks))
					UpdateDeadBlocks(m_BlackStepBlocks);
			} else {
				if (!UpdateDeadBlocks(m_BlackStepBlocks))
					UpdateDeadBlocks(m_WhiteStepBlocks);
			}

			MoveCurrentRect();
			m_StepString.AppendFormat("{0},{1},{2},", row, col, m_StepCount);
			if (IsPlaySound) {
				// 如果是 XP 系统，m_Media 相关代码需去掉，换 SoundPlayer.
				// 否则，取消以下和 Board.xaml 中的注释，即可播放声音。
				// m_Media.Position = TimeSpan.Zero;
				// m_Media.Play();
			}

			m_StepCount++;

			StoneColor selfColor = isBlack ? StoneColor.Black : StoneColor.White;
			bool isKillSelf = m_DeadBlocks.ContainsKey(m_StepCount - 1)
					&& m_DeadBlocks[m_StepCount - 1].Steps.Count == 1
					&& m_DeadBlocks[m_StepCount - 1].Steps[0].StoneColor == selfColor;
			if (isKillSelf) {
				m_DeadBlocks.Remove(m_StepCount - 1);
				BackOne();
				return false;
			}

			m_CurrentStep = m_Steps[row, col];

			return true;
		}

		public void BackOne()
		{
			if (m_StepCount == 0) {
				return;
			}

			m_StepCount--;
			foreach (var item in m_Steps) {
				if (item.StepCount == m_StepCount) {
					if (m_BlackSteps.Contains(item)) {
						m_BlackSteps.Remove(item);
					} else if (m_WhiteSteps.Contains(item)) {
						m_WhiteSteps.Remove(item);
					}
					m_EmptySteps.Add(item);
					HideStep(item);

					if (m_DeadBlocks.ContainsKey(m_StepCount)) {
						foreach (var d in m_DeadBlocks[m_StepCount].Steps) {
							ShowStep(d);
						}
						m_DeadBlocks.Remove(m_StepCount);
					}

					m_StepCount--;
					MoveCurrentRect();
					m_StepCount++;

					break;
				}
			}

			// Reset BanOnce 
			int prevCount = m_StepCount - 1;
			Step prev = m_AllSteps.FirstOrDefault(step => step.StepCount == prevCount);
			if (prev == null) return;
			List<Step> empties = LinkSteps(prev, StoneColor.Empty);
			if (empties.Count == 1 && empties[0].DeadStepCount == m_BanOnce.StepCount) {
				m_BanOnce.Row = empties[0].Row;
				m_BanOnce.Col = empties[0].Col;
			}

			UpdateAllStepBlocks();

			string s = m_StepString.ToString();
			s = m_Regex.Replace(s, "");
			m_StepString.Clear();
			m_StepString.Append(s);

		}

		public void SaveSteps()
		{
			string steps = m_StepString.ToString();
			m_StepString.Clear();

			SaveStepWindow dlg = new SaveStepWindow();
			dlg.ShowDialog();
			using (var db = new WeiqiContext()) {
				Record record = new Record {
					Black = dlg.BlackName,
					White = dlg.WhiteName,
					Result = dlg.Result,
					Description = dlg.Description,
					SaveDate = DateTime.Now,
					Steps = steps
				};
				db.Records.Add(record);
				db.SaveChanges();
				MessageBox.Show("Save steps success!");
			}
		}

		#region Init

		

		SoundPlayer m_Player = new SoundPlayer();

		StringBuilder m_StepString = new StringBuilder(2000);	// 保存棋谱
		Regex m_Regex = new Regex(@"(\d+,){3}$");				// 悔棋匹配

		Rectangle m_CurrentRect = new Rectangle();			// 当前标志
		Rectangle[,] m_MeshRects = new Rectangle[19, 19];	// 点目标志

		Step[,] m_Steps = new Step[19, 19];					// 棋步，逻辑棋子
		Step m_CurrentStep = null;

		Ellipse[,] m_Stones = new Ellipse[19, 19];			// 棋子，仅为显示
		TextBlock[,] m_Numbers = new TextBlock[19, 19];		// 步数

		// 棋盘纵横线和星
		Line[] m_LinesH = new Line[19];
		Line[] m_LinesV = new Line[19];
		Ellipse[,] m_Stars = new Ellipse[3, 3];

		// 棋子填充画刷
		Brush BlackBrush { get { return (Brush)Resources["BlackBrushKey"]; } }
		Brush WhiteBrush { get { return (Brush)Resources["WhiteBrushKey"]; } }

		List<Step> m_AllSteps = new List<Step>();		// 所有逻辑棋子
		List<Step> m_BlackSteps = new List<Step>();		// 所有黑棋
		List<Step> m_WhiteSteps = new List<Step>();		// 所有白棋
		List<Step> m_EmptySteps = new List<Step>();		// 所有空棋

		// key: StepCount, value: Steps 死子块的集合
		Dictionary<int, StepBlock> m_DeadBlocks = new Dictionary<int, StepBlock>();

		List<StepBlock> m_BlackStepBlocks = new List<StepBlock>();
		List<StepBlock> m_WhiteStepBlocks = new List<StepBlock>();
		List<StepBlock> m_EmptyStepBlocks = new List<StepBlock>();

		Step m_BanOnce = new Step();	// 劫争

		void Init()
		{
			// 线
			for (int i = 0; i < 19; i++) {
				m_LinesH[i] = new Line();
				m_LinesH[i].Stroke = Brushes.Black;
				m_Canvas.Children.Add(m_LinesH[i]);

				m_LinesV[i] = new Line();
				m_LinesV[i].Stroke = Brushes.Black;
				m_Canvas.Children.Add(m_LinesV[i]);
			}

			// 星
			for (int j = 0; j < 3; j++) {
				for (int i = 0; i < 3; i++) {
					m_Stars[i, j] = new Ellipse();
					m_Stars[i, j].Fill = Brushes.Black;
					m_Canvas.Children.Add(m_Stars[i, j]);
				}
			}

			for (int i = 0; i < 19; i++) {
				for (int j = 0; j < 19; j++) {
					m_Stones[i, j] = new Ellipse();
					m_Stones[i, j].Visibility = Visibility.Hidden;
					m_Canvas.Children.Add(m_Stones[i, j]);

					m_Numbers[i, j] = new TextBlock();
					m_Numbers[i, j].Background = Brushes.Transparent;
					m_Numbers[i, j].Visibility = Visibility.Hidden;
					m_Canvas.Children.Add(m_Numbers[i, j]);

					m_Steps[i, j] = new Step();
					m_Steps[i, j].Row = i;
					m_Steps[i, j].Col = j;
					m_EmptySteps.Add(m_Steps[i, j]);
					m_AllSteps.Add(m_Steps[i, j]);


				}
			}

			// 当前标志
			m_CurrentRect.Visibility = System.Windows.Visibility.Hidden;
			m_CurrentRect.Fill = Brushes.Red;
			m_Canvas.Children.Add(m_CurrentRect);

			for (int i = 0; i < 19; i++) {
				for (int j = 0; j < 19; j++) {
					Rectangle rect = new Rectangle();
					rect.Visibility = System.Windows.Visibility.Hidden;
					m_MeshRects[i, j] = rect;
					m_Canvas.Children.Add(m_MeshRects[i, j]);

					AllPoses.Add(new Pos(i, j));
				}
			}
		}

		void MoveCurrentRect()
		{
			if (StepCount < 0) {
				m_CurrentRect.Visibility = System.Windows.Visibility.Hidden;
				return;
			}

			if (IsShowCurrent)
				m_CurrentRect.Visibility = System.Windows.Visibility.Visible;
			else
				m_CurrentRect.Visibility = System.Windows.Visibility.Hidden;

			Step step = m_AllSteps.FirstOrDefault(s => s.StepCount == StepCount);
			if (step == null) return;
			int row = step.Row;
			int col = step.Col;
			double offset = (StoneSize - m_CurrentRect.Width) / 2.0;
			Canvas.SetLeft(m_CurrentRect, col * StoneSize + offset);
			Canvas.SetTop(m_CurrentRect, row * StoneSize + offset);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			Redraw();

			StepCount--;
			MoveCurrentRect();
			StepCount++;
		}

		private void ShowNumber(int row, int col, int stepCount)
		{
			var number = m_Numbers[row, col];
			if (IsShowNumber && stepCount >= 0) {
				number.FontSize = GetFontSize(stepCount);
				number.Padding = GetPadding(stepCount);
				number.Foreground = stepCount % 2 == 0 ? Brushes.White : Brushes.Black;
				number.Text = (stepCount + 1).ToString();
				if (m_Steps[row, col].StoneColor == StoneColor.Empty)
					number.Visibility = System.Windows.Visibility.Hidden;
				else
					number.Visibility = System.Windows.Visibility.Visible;
				Canvas.SetLeft(number, col * StoneSize);
				Canvas.SetTop(number, row * StoneSize);
			} else {
				number.Visibility = System.Windows.Visibility.Hidden;
			}
		}
		private double GetFontSize(int stepCount)
		{
			return stepCount < 9 ? StoneSize / 1.2
				: stepCount < 99 ? StoneSize / 1.6
				: StoneSize / 2;
		}
		private Thickness GetPadding(int stepCount)
		{
			return stepCount < 9 ? new Thickness(StoneSize / 4.1, 0, 0, 0)
				: stepCount < 99 ? new Thickness(StoneSize / 8.1, StoneSize / 8.1, 0, 0)
				: new Thickness(StoneSize / 22.1, StoneSize / 6.1, 0, 0);
		}

		#endregion

		#region DrawStep

		void DrawStep(int row, int col) // base 0
		{
			int count = m_StepCount;
			var step = m_Steps[row, col];
			StoneColor currentColor = count % 2 == 0 ? StoneColor.Black : StoneColor.White;

			step.StepCount = count;
			step.Row = row;
			step.Col = col;
			step.StoneColor = currentColor;

			Brush brush = count % 2 == 0 ? BlackBrush : WhiteBrush;
			var stone = m_Stones[row, col];
			stone.Width = stone.Height = StoneSize;
			stone.Fill = brush;
			stone.Visibility = System.Windows.Visibility.Visible;
			Canvas.SetLeft(stone, col * StoneSize);
			Canvas.SetTop(stone, row * StoneSize);

			ShowNumber(row, col, count);
		}

		void HideStep(Step step)
		{
			int row = step.Row;
			int col = step.Col;
			m_Steps[row, col].DeadStepCount = step.StepCount;
			m_Steps[row, col].StepCount = -1;
			m_Steps[row, col].StoneColor = StoneColor.Empty;
			m_Stones[row, col].Visibility = System.Windows.Visibility.Hidden;
			m_Numbers[row, col].Visibility = System.Windows.Visibility.Hidden;
		}

		/// <summary>
		/// 重新显示死子需要注意：
		///		m_Stones[row,col]，m_Number[row,col], m_Steps[row，col]. m_BlackSteps, m_WhiteSteps 
		///		的内容都有可能被改变，所以要作特别处理。
		/// </summary>
		/// <param name="step">所保存的死子</param>
		void ShowStep(Step step)
		{
			int row = step.Row;
			int col = step.Col;
			m_Steps[row, col].StoneColor = step.StoneColor;
			m_Steps[row, col].StepCount = step.StepCount == -1 ? step.DeadStepCount : step.StepCount;

			if (step.StoneColor == StoneColor.Black) {
				m_BlackSteps.Add(m_Steps[row, col]);
			} else if (step.StoneColor == StoneColor.White) {
				m_WhiteSteps.Add(m_Steps[row, col]);
			}

			m_Stones[row, col].Visibility = System.Windows.Visibility.Visible;
			m_Stones[row, col].Fill =
				step.StoneColor == StoneColor.Black ? BlackBrush : WhiteBrush;

			ShowNumber(row, col, step.StepCount);
		}

		#endregion

		#region Blocks

		void UpdateAllStepBlocks()
		{
			UpdateEmptyStepBlocks();
			UpdateBlackStepBlocks();
			UpdateWhiteStepBlocks();
		}
		void UpdateEmptyStepBlocks()
		{
			m_EmptyStepBlocks.Clear();
			UpdateStepBlocks(m_EmptySteps, m_EmptyStepBlocks);
		}
		void UpdateBlackStepBlocks()
		{
			m_BlackStepBlocks.Clear();
			UpdateStepBlocks(m_BlackSteps, m_BlackStepBlocks);
		}
		void UpdateWhiteStepBlocks()
		{
			m_WhiteStepBlocks.Clear();
			UpdateStepBlocks(m_WhiteSteps, m_WhiteStepBlocks);
		}
		private void UpdateStepBlocks(List<Step> steps, List<StepBlock> blocks)
		{
			List<Step> copySteps = new List<Step>();
			foreach (var item in steps) {
				copySteps.Add(item);
			}

			if (copySteps.Count == 0) return;

			var tmp = new List<Step>();
			foreach (var step in copySteps) {
				if (tmp.Count == 0) tmp.Add(step);
				var sameLinks = LinkSteps(step, step.StoneColor);
				if (tmp.Intersect(sameLinks).Count() != 0)
					sameLinks.ForEach(s => { if (!tmp.Contains(s)) tmp.Add(s); });

			}
			for (int i = 0; i < 6; i++) {
				foreach (var step in copySteps) {	// 防止遗漏
					var sameLinks = LinkSteps(step, step.StoneColor);
					if (tmp.Intersect(sameLinks).Count() != 0)
						sameLinks.ForEach(s => { if (!tmp.Contains(s)) tmp.Add(s); });
				}
			}
			if (tmp.Count == 0) return;

			StepBlock block = new StepBlock();
			block.Steps = tmp;
			block.UpdateBlockId();
			blocks.Add(block);

			copySteps.RemoveAll(s => tmp.Contains(s));	// next block
			UpdateStepBlocks(copySteps, blocks);	// 递归
		}

		bool UpdateDeadBlocks(List<StepBlock> selfBlocks)
		{
			List<StepBlock> blocks = new List<StepBlock>();
			foreach (var item in selfBlocks) {
				blocks.Add(item);	//copy
			}

			foreach (var block in blocks) {
				List<Step> empties = GetLinkEmptySteps(block);
				block.EmptyCount = empties.Count;

				if (empties.Count == 0) {
					if (block.Steps.Count == 1) {
						var step = block.Steps[0];
						var otherColor = step.StoneColor == StoneColor.Black ? StoneColor.White : StoneColor.Black;
						var links = LinkSteps(step,otherColor);
						bool isBanOnce = false;
						foreach (var link in links) {
							if (link.EmptyCount == 1 && LinkSteps(link, link.StoneColor).Count == 1)
								isBanOnce = true;
						}
						if (isBanOnce) { // 为倒扑
							m_BanOnce.Row = block.Steps[0].Row;
							m_BanOnce.Col = block.Steps[0].Col;
							m_BanOnce.StepCount = block.Steps[0].StepCount;
						}
					}

					StepBlock deadInfo = new StepBlock();
					foreach (var dead in block.Steps) {
						var d = CloneStep(dead);
						deadInfo.Steps.Add(d);

						if (dead.StoneColor == StoneColor.Black) {
							m_BlackSteps.Remove(dead);
						} else if (dead.StoneColor == StoneColor.White) {
							m_WhiteSteps.Remove(dead);
						}
						m_EmptySteps.Add(dead);
						HideStep(dead);
					}
					m_DeadBlocks[m_StepCount] = deadInfo;

					selfBlocks.Remove(block);
					return true;
				}
			}
			return false;
		}
		Step CloneStep(Step step)
		{
			var s = new Step();
			s.Row = step.Row;
			s.Col = step.Col;
			s.BlockId = step.BlockId;
			s.StepCount = step.StepCount;
			s.DeadStepCount = step.DeadStepCount;
			s.StoneColor = step.StoneColor;
			return s;
		}

		//   +
		// + + +	与 step 相连的棋子，包含自身
		//   +		根据 color 参数决定是所有，同色，黑色，白色，还是空色。
		List<Step> LinkSteps(Step step, StoneColor color = StoneColor.Empty)
		{
			List<Step> links = new List<Step>();
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if (i == j || i == -j) {
						continue;
					}
					if (InRange(step.Row + i, step.Col + j)) {
						links.Add(m_Steps[step.Row + i, step.Col + j]);
					}
				}
			}
			links.Add(step);
			if (color == StoneColor.All) {
				return links;
			} else {
				links.RemoveAll(l => l.StoneColor != color);
				return links;
			}
		}
		bool InRange(int row, int col)
		{
			if (row >= 0 && row < 19 && col >= 0 && col < 19) {
				return true;
			}
			return false;
		}

		// + + +
		// + + +	与 step 相围的棋子，包含自身
		// + + +
		List<Step> RoundSteps(Step step)
		{
			List<Step> links = new List<Step>();
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if (InRange(step.Row + i, step.Col + j)) {
						links.Add(m_Steps[step.Row + i, step.Col + j]);
					}
				}
			}
			return links;
		}

		#endregion
	}
}
