using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

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
			Init();
		}

		public bool IsShowNumber { get; set; }	// 是否显示步数

		// 控制棋盘和棋子大小
		int m_StoneSize = 38;
		public int StoneSize
		{
			get { return m_StoneSize; }
			set
			{
				m_StoneSize = value;
				Width = Height = m_StoneSize * 19;
			}
		}

		public void NextOne(int row, int col)
		{
			if (m_Steps[row, col].StoneColor != StoneColor.Empty)
				return;
			if (m_BanOnce.Row == row && m_BanOnce.Col == col) {
				return;
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

			UpdateBlackBlocks();
			UpdateWhiteBlocks();
			if (isBlack) {
				if (!UpdateDeadBlocks(m_WhiteBlocks))
					UpdateDeadBlocks(m_BlackBlocks);
			} else {
				if (!UpdateDeadBlocks(m_BlackBlocks))
					UpdateDeadBlocks(m_WhiteBlocks);
			}

			m_StepCount++;

			StoneColor selfColor = isBlack ? StoneColor.Black : StoneColor.White;
			bool isKillSelf = m_DeadBlocks.ContainsKey(m_StepCount - 1)
					&& m_DeadBlocks[m_StepCount - 1].Steps.Count == 1
					&& m_DeadBlocks[m_StepCount - 1].Steps[0].StoneColor == selfColor;
			if (isKillSelf) {
				m_DeadBlocks.Remove(m_StepCount - 1);
				BackOne();
			}


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
					break;
				}
			}

			// Reset BanOnce 
			int prevCount = m_StepCount - 1;
			Step prev = m_AllSteps.First(s => s.StepCount == prevCount);
			List<Step> empties = LinkSteps(prev, StoneColor.Empty);
			if (empties.Count == 1 && empties[0].StepCount == m_BanOnce.StepCount) {
				m_BanOnce.Row = empties[0].Row;
				m_BanOnce.Col = empties[0].Col;
			}
			
		}

		#region Init

		Step[,] m_Steps = new Step[19, 19];					// 棋步，逻辑棋子

		Ellipse[,] m_Stones = new Ellipse[19, 19];			// 棋子，仅为显示
		TextBlock[,] m_Numbers = new TextBlock[19, 19];		// 步数

		int m_StepCount = 0;					// 棋步计数

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
		Dictionary<int, Block> m_DeadBlocks = new Dictionary<int, Block>();

		List<Block> m_BlackBlocks = new List<Block>();
		List<Block> m_WhiteBlocks = new List<Block>();

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
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			Redraw();
		}
		void Redraw()
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
			//m_Steps[row, col].StepCount = -1;
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
			m_Steps[row, col].StepCount = step.StepCount;

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

		void UpdateBlackBlocks()
		{
			m_BlackBlocks.Clear();
			UpdateBlocks(m_BlackSteps, m_BlackBlocks);
		}
		void UpdateWhiteBlocks()
		{
			m_WhiteBlocks.Clear();
			UpdateBlocks(m_WhiteSteps, m_WhiteBlocks);
		}
		private void UpdateBlocks(List<Step> steps, List<Block> blocks)
		{
			List<Step> copySteps = new List<Step>();
			foreach (var item in steps) {
				copySteps.Add(item);
			}

			if (copySteps.Count == 0) return;

			var tmp = new List<Step>();
			foreach (var step in copySteps) {
				if (tmp.Contains(step)) continue;
				List<Step> sameLinks = LinkSteps(step, step.StoneColor);
				foreach (var link in sameLinks) {
					if (tmp.Count != 0 && tmp.Intersect(sameLinks).Count() != 0) {
						if (!tmp.Contains(link)) tmp.Add(link);
					} else if (tmp.Count == 0) {	// first
						tmp.Add(link);
					}
				}
			}
			if (tmp.Count == 0) return;

			Block block = new Block();
			block.Steps.AddRange(tmp);
			//block.UpdateBlockId();
			blocks.Add(block);

			copySteps.RemoveAll(s => tmp.Contains(s));	// next block
			UpdateBlocks(copySteps, blocks);	// 递归
		}

		private List<Step> GetLinkEmpties(Step step)
		{
			return LinkSteps(step, StoneColor.Empty).ToList();
		}


		bool UpdateDeadBlocks(List<Block> selfBlocks)
		{
			List<Block> blocks = new List<Block>();
			foreach (var item in selfBlocks) {
				blocks.Add(item);	//copy
			}

			foreach (var block in blocks) {
				List<Step> empties = new List<Step>();
				foreach (var step in block.Steps) {
					empties = empties.Union(GetLinkEmpties(step)).ToList();
				}
				block.EmptyCount = empties.Count;

				if (empties.Count == 0) {
					if (block.Steps.Count == 1) {
						m_BanOnce.Row = block.Steps[0].Row;
						m_BanOnce.Col = block.Steps[0].Col;
						m_BanOnce.StepCount = block.Steps[0].StepCount;
					}

					Block deadInfo = new Block();
					foreach (var dead in block.Steps) {
						var d = CloneStep(dead);
						deadInfo.Steps.Add(d);

						if (dead.StoneColor == StoneColor.Black)
							m_BlackSteps.Remove(dead);
						else if (dead.StoneColor == StoneColor.White)
							m_WhiteSteps.Remove(dead);
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
			s.StoneColor = step.StoneColor;
			return s;
		}

		//   +
		// + + +	与 step 相连的棋子，包含自身
		//   +		根据 color 参数决定是同色，黑色，白色，还是空色。
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
