// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using x01.Weiqi.Dialog;
using x01.Weiqi.Model;
using x01.Weiqi.WhereHelper;

namespace x01.Weiqi.Board
{
	/// <summary>
	/// Interaction logic for BoardBase.xaml
	/// </summary>
	public abstract partial class BoardBase : UserControl, IBoard
	{
		#region Ready

		PosSearch m_PosSearch = new PosSearch(); // for AIBoard
		internal PosSearch PosSearch
		{
			get { return m_PosSearch; }
		}

		Rectangle m_rectCurrent = new Rectangle(); // current flag

		bool m_IsBlack = false; // 控制棋子的交互
		public bool IsBlack
		{
			get { return m_IsBlack; }
			set { m_IsBlack = value; }
		}

		int m_StepCount = 0; // 步数
		public int StepCount
		{
			get { return m_StepCount; }
			set { m_StepCount = value; }
		}

		List<Pos> m_DeadPos = new List<Pos>(); // for CanEat
		List<DeadStep> m_DeadSteps = new List<DeadStep>(); // 保存死子

		public List<DeadStep> DeadSteps
		{
			get { return m_DeadSteps; }
			set { m_DeadSteps = value; }
		}

		private StringBuilder m_ContentString = new StringBuilder();

		public StringBuilder ContentString
		{
			get { return m_ContentString; }
			set { m_ContentString = value; }
		}

		public BoardBase(int size = 38)
		{
			InitializeComponent();

			ChessSize = size;
			IsShowNumber = true;
			Steps = new StepInfo[19, 19];

			Init();
		}
		public void Init()
		{
			m_rectCurrent.Width = m_rectCurrent.Height = (ChessSize + 2) / 3;
			m_rectCurrent.Fill = Brushes.Salmon;

			Width = Height = ChessSize * 19;
			Background = new SolidColorBrush(Color.FromArgb(0x5e, 0xef, 0xdf, 0x56));

			NotInPos = new Pos(-1, -1);


			ClearSteps();

			// 画线
			for (int i = 0; i < 19; i++) {
				Line l = new Line();
				l.Stroke = Brushes.Black;
				int y = i * ChessSize + ChessSize / 2;
				l.X1 = ChessSize / 2;
				l.Y1 = y;
				l.X2 = 19 * ChessSize - ChessSize / 2;
				l.Y2 = y;
				m_Canvas.Children.Add(l);

				l = new Line();
				l.Stroke = Brushes.Black;
				int x = i * ChessSize + ChessSize / 2;
				l.X1 = x;
				l.Y1 = ChessSize / 2;
				l.X2 = x;
				l.Y2 = 19 * ChessSize - ChessSize / 2;
				m_Canvas.Children.Add(l);
			}

			// 画星
			for (int j = 0; j < 3; j++)
				for (int i = 0; i < 3; i++) {
					Ellipse e = new Ellipse();
					e.Fill = Brushes.Black;
					e.Width = 8;
					e.Height = 8;
					double left = 4 * ChessSize - ChessSize / 2 + j * 6 * ChessSize;
					double top = 4 * ChessSize - ChessSize / 2 + i * 6 * ChessSize;
					Canvas.SetLeft(e, left - 4);
					Canvas.SetTop(e, top - 4);
					m_Canvas.Children.Add(e);
				}
		}

		private void ClearSteps()
		{
			for (int i = 0; i < 19; i++) {
				for (int j = 0; j < 19; j++) {
					Steps[i, j].Stone = null;
					Steps[i, j].Color = ChessColor.Empty;
					Steps[i, j].IsDead = false;
					Steps[i, j].Count = -1;
					Steps[i, j].Row = j;
					Steps[i, j].Column = i;

					//for whereBoard
					m_PosSearch.EmptyPos.Add(new Pos(i, j));
				}
			}
		}

		protected Pos NotInPos { get; set; } // 打劫及禁入点
		public StepInfo[,] Steps { get; set; } // 保存棋谱

		#endregion

		#region IBoard Members

		public int ChessSize { get; set; }
		public Brush BlackBrush { get { return (Brush)FindResource("brushBlack"); } }
		public Brush WhiteBrush { get { return (Brush)FindResource("brushWhite"); } }
		public void DrawChess(int col, int row)
		{
			int left = col * ChessSize;
			int top = row * ChessSize;

			Brush brush;
			if (m_IsBlack = !m_IsBlack) {
				brush = BlackBrush; //(Brush)FindResource("brushBlack"); //Brushes.Black;
				Steps[col, row].Color = ChessColor.Black;
			} else {
				brush = WhiteBrush; // (Brush)FindResource("brushWhite"); //Brushes.White;
				Steps[col, row].Color = ChessColor.White;
			}

			if (Steps[col, row].Stone == null || Steps[col, row].Stone.Content == null) {
				Stone chess = new Stone(brush, ChessSize);
				SetShowNumber(chess);
				Canvas.SetLeft(chess, left);
				Canvas.SetTop(chess, top);
				m_Canvas.Children.Add(chess);
				Steps[col, row].Stone = chess;
			} else {
				throw new Exception(string.Format("Cannot Draw Chess at: col = {0}, row = {1}.", col, row));
			}

			Steps[col, row].Count = ++m_StepCount;

			// For WhereBoard
			AddPos(col, row);

			if (m_Canvas.Children.Contains(m_rectCurrent)) {
				m_Canvas.Children.Remove(m_rectCurrent);
			}
			Canvas.SetLeft(m_rectCurrent, left + m_rectCurrent.Width);
			Canvas.SetTop(m_rectCurrent, top + m_rectCurrent.Height);
			m_Canvas.Children.Add(m_rectCurrent);

			m_ContentString.Append(col.ToString() + "," + row.ToString() + "," + m_StepCount.ToString() + ",");
		}

		private void SetShowNumber(Stone chess)
		{
			double size = (double)ChessSize;
			chess.NumberFontSize = (m_StepCount + 1) > 99
				? size / 2 : (m_StepCount + 1) > 9
				? size / 1.6 : size / 1.2;
			chess.NumberPadding = (m_StepCount + 1) > 99
				? new Thickness(size / 28, size / 5.5, 0, 0) : (m_StepCount + 1) > 9
				? new Thickness(size / 8, size / 8, 0, 0) : new Thickness(size / 4, 0, 0, 0);
			chess.NumberText = (m_StepCount + 1).ToString();
			chess.NumberBrush = (m_StepCount % 2 == 1) ? Brushes.Black : Brushes.White;
			chess.SetShow(IsShowNumber);
		}

		public void Eat(int col, int row)
		{
			if (!EatOther(col, row)) {
				if (EatSelf(col, row)) {
					// 自杀一子为禁入
					if (m_DeadPos.Count == 1) {
						BackOne();
					}
				}
			}
		}
		private bool EatOther(int col, int row)
		{
			// 上、下、左、右各吃一通。
			bool ok1 = false;
			if (col != 0 && Steps[col, row].Color != Steps[col - 1, row].Color) {
				ok1 = EatSelf(col - 1, row);
			}
			bool ok2 = false;
			if (col != 18 && Steps[col, row].Color != Steps[col + 1, row].Color) {
				ok2 = EatSelf(col + 1, row);
			}
			bool ok3 = false;
			if (row != 0 && Steps[col, row].Color != Steps[col, row - 1].Color) {
				ok3 = EatSelf(col, row - 1);
			}
			bool ok4 = false;
			if (row != 18 && Steps[col, row].Color != Steps[col, row + 1].Color) {
				ok4 = EatSelf(col, row + 1);
			}

			return ok1 || ok2 || ok3 || ok4;
		}
		private bool EatSelf(int col, int row)
		{
			ClearForEat();
			if (CanEat(col, row, Steps[col, row].Color)) {
				DeadStep deadStep;
				deadStep.Count = m_StepCount;
				deadStep.Color = Steps[col, row].Color;
				deadStep.DeadInfo = new Dictionary<int, Pos>();
				if (m_DeadPos.Count == 1) {
					Pos pos;
					pos.X = m_DeadPos[0].X;
					pos.Y = m_DeadPos[0].Y;

					NotInPos = new Pos(pos.X, pos.Y);

					foreach (var item in Steps) {
						// 变色而已，m_Count 为上一步
						if (item.Count == m_StepCount) {
							ClearForEat(); // 预想下一步的进行
							if (CanEat(pos.X, pos.Y, item.Color)) {
								// 因有一子变色，故减一。实际 Count 至少为 3
								if (m_DeadPos.Count - 1 > 0) {
									NotInPos = new Pos(-1, -1);
								}
							}
						}
					}

					deadStep.DeadInfo.Add(Steps[pos.X, pos.Y].Count, new Pos(pos.X, pos.Y));

					if (Steps[pos.X, pos.Y].Color == ChessColor.Black) {
						m_PosSearch.BlackPos.Remove(new Pos(pos.X, pos.Y));
						m_PosSearch.EmptyPos.Add(new Pos(pos.X, pos.Y));
					} else if (Steps[pos.X, pos.Y].Color == ChessColor.White) {
						m_PosSearch.WhitePos.Remove(new Pos(pos.X, pos.Y));
						m_PosSearch.EmptyPos.Add(new Pos(pos.X, pos.Y));
					}

					Steps[pos.X, pos.Y].Color = ChessColor.Empty;
					Steps[pos.X, pos.Y].Stone.Content = null;
				} else {
					foreach (var item in m_DeadPos) {
						deadStep.DeadInfo.Add(Steps[item.X, item.Y].Count, new Pos(item.X, item.Y));

						if (Steps[item.X, item.Y].Color == ChessColor.Black) {
							m_PosSearch.BlackPos.Remove(new Pos(item.X, item.Y));
							m_PosSearch.EmptyPos.Add(new Pos(item.X, item.Y));
						} else if (Steps[item.X, item.Y].Color == ChessColor.White) {
							m_PosSearch.WhitePos.Remove(new Pos(item.X, item.Y));
							m_PosSearch.EmptyPos.Add(new Pos(item.X, item.Y));
						}

						Steps[item.X, item.Y].Color = ChessColor.Empty;
						Steps[item.X, item.Y].Stone.Content = null;
					}
				}
				m_DeadSteps.Add(deadStep);

				return true;
			}

			return false;
		}
		protected void ClearForEat()
		{
			foreach (var item in m_DeadPos) {
				Steps[item.X, item.Y].IsDead = false;
			}
			m_DeadPos.Clear();
		}
		protected bool CanEat(int col, int row, ChessColor currentColor)
		{
			if (Steps[col, row].Color == ChessColor.Empty) {
				return false;
			}

			m_DeadPos.Add(new Pos(col, row));
			Steps[col, row].IsDead = true;

			// 边界问题，头疼问题。笨人笨法，分别处理之
			return CanEatLeft(col, row, currentColor) && CanEatRight(col, row, currentColor)
				&& CanEatUp(col, row, currentColor) && CanEatDown(col, row, currentColor);
		}
		bool CanEatLeft(int col, int row, ChessColor currentColor)
		{
			if (col != 0) {
				if (Steps[col - 1, row].Color == ChessColor.Empty) {
					return false;
				} else if (Steps[col - 1, row].Color == currentColor && Steps[col - 1, row].IsDead == false) {
					m_DeadPos.Add(new Pos(col - 1, row));
					Steps[col - 1, row].IsDead = true;
					if (!CanEatUp(col - 1, row, currentColor)) {
						return false;
					}
					if (!CanEatDown(col - 1, row, currentColor)) {
						return false;
					}
					if (!CanEatLeft(col - 1, row, currentColor)) {
						return false;
					}
				}
			}

			return true;
		}
		bool CanEatRight(int col, int row, ChessColor currentColor)
		{
			if (col != 18) {
				if (Steps[col + 1, row].Color == ChessColor.Empty) {
					return false;
				} else if (Steps[col + 1, row].Color == currentColor && Steps[col + 1, row].IsDead == false) {
					m_DeadPos.Add(new Pos(col + 1, row));
					Steps[col + 1, row].IsDead = true;
					if (!CanEatUp(col + 1, row, currentColor)) {
						return false;
					}
					if (!CanEatDown(col + 1, row, currentColor)) {
						return false;
					}
					if (!CanEatRight(col + 1, row, currentColor)) {
						return false;
					}
				}
			}

			return true;
		}
		bool CanEatUp(int col, int row, ChessColor currentColor)
		{
			if (row != 0) {
				if (Steps[col, row - 1].Color == ChessColor.Empty) {
					return false;
				} else if (Steps[col, row - 1].Color == currentColor && Steps[col, row - 1].IsDead == false) {
					m_DeadPos.Add(new Pos(col, row - 1));
					Steps[col, row - 1].IsDead = true;
					if (!CanEatLeft(col, row - 1, currentColor)) {
						return false;
					}
					if (!CanEatRight(col, row - 1, currentColor)) {
						return false;
					}
					if (!CanEatUp(col, row - 1, currentColor)) {
						return false;
					}
				}
			}

			return true;
		}
		bool CanEatDown(int col, int row, ChessColor currentColor)
		{
			if (row != 18) {
				if (Steps[col, row + 1].Color == ChessColor.Empty) {
					return false;
				} else if (Steps[col, row + 1].Color == currentColor && Steps[col, row + 1].IsDead == false) {
					m_DeadPos.Add(new Pos(col, row + 1));
					Steps[col, row + 1].IsDead = true;
					if (!CanEatLeft(col, row + 1, currentColor)) {
						return false;
					}
					if (!CanEatRight(col, row + 1, currentColor)) {
						return false;
					}
					if (!CanEatDown(col, row + 1, currentColor)) {
						return false;
					}
				}
			}

			return true;
		}

		public void BackOne()
		{
			m_ContentCount--;

			if (m_Canvas.Children.Contains(m_rectCurrent)) {
				m_Canvas.Children.Remove(m_rectCurrent);
			}

			if (m_StepCount == 0) {
				return;
			}

			int count = m_StepCount--;
			int index = -1;

			foreach (var dead in m_DeadSteps) {
				if (dead.Count == count) {
					index = m_DeadSteps.Count - 1;
					Brush brush;
					if (dead.Color == ChessColor.Black) {
						brush = BlackBrush; //Brushes.Black;
					} else {
						brush = WhiteBrush; //Brushes.White;
					}

					foreach (var info in dead.DeadInfo) {
						int col = info.Value.X;
						int row = info.Value.Y;

						//AddPos(col, row);
						if (dead.Color == ChessColor.Black) {
							m_PosSearch.BlackPos.Add(new Pos(col, row));
							m_PosSearch.EmptyPos.Remove(new Pos(col, row));
						} else if (dead.Color == ChessColor.White) {
							m_PosSearch.WhitePos.Add(new Pos(col, row));
							m_PosSearch.EmptyPos.Remove(new Pos(col, row));
						}

						Ellipse e = new Ellipse();
						e.Fill = brush;
						e.Width = e.Height = ChessSize;
						Steps[col, row].Stone.Content = e;
						Steps[col, row].Color = dead.Color;
						Steps[col, row].Count = info.Key;
					}
				}
			}

			if (index != -1) {
				m_DeadSteps.RemoveAt(index);
			}

			Pos backPos = new Pos(-1, -1);
			foreach (var item in Steps) {
				if (item.Count == count - 1) {
					backPos = new Pos(item.Column, item.Row);
					break;
				}
			}

			foreach (var item in Steps) {
				if (item.Color != ChessColor.Empty && item.Count == count) {
					int col = item.Column;
					int row = item.Row;

					RemovePos(col, row); // for PosSearch

					Steps[col, row].Stone.Content = null;
					Steps[col, row].Count = -1;
					Steps[col, row].Color = ChessColor.Empty;
					m_IsBlack = !m_IsBlack;

					string s = m_ContentString.ToString();
					bool skip = false;
					for (int i = 0; i < 4; i++) {
						if (s.LastIndexOf(",") < 3) {
							m_ContentString = new StringBuilder();
							skip = true;
							break;
						}
						s = s.Substring(0, s.LastIndexOf(","));
					}
					if (!skip) {
						m_ContentString = new StringBuilder(s + ",");
					}

					if (backPos == new Pos(-1, -1)) {
						if (m_Canvas.Children.Contains(m_rectCurrent)) {
							m_Canvas.Children.Remove(m_rectCurrent);
						}
					} else {
						Canvas.SetLeft(m_rectCurrent, backPos.X * ChessSize + m_rectCurrent.Width);
						Canvas.SetTop(m_rectCurrent, backPos.Y * ChessSize + m_rectCurrent.Height);
						m_Canvas.Children.Add(m_rectCurrent);
					}
				}
			}
		}

		// For PosSearch
		private void AddPos(int col, int row)
		{
			if (Steps[col, row].Color == ChessColor.Black) {
				m_PosSearch.BlackPos.Add(new Pos(col, row));
				m_PosSearch.EmptyPos.Remove(new Pos(col, row));
			} else if (Steps[col, row].Color == ChessColor.White) {
				m_PosSearch.WhitePos.Add(new Pos(col, row));
				m_PosSearch.EmptyPos.Remove(new Pos(col, row));
			}
		}
		private void RemovePos(int col, int row)
		{
			if (Steps[col, row].Color == ChessColor.Black) {
				m_PosSearch.BlackPos.Remove(new Pos(col, row));
				m_PosSearch.EmptyPos.Add(new Pos(col, row));
			} else if (Steps[col, row].Color == ChessColor.White) {
				m_PosSearch.WhitePos.Remove(new Pos(col, row));
				m_PosSearch.EmptyPos.Add(new Pos(col, row));
			}
		}

		public void Save()
		{
			var dlg = new SaveStepWindow();
			dlg.ShowDialog();

			string s = ContentString.ToString();
			Chess step = new Chess {
				Step = s,
				Description = dlg.Description,
				SaveDate = DateTime.Now,
				Black = dlg.BlackName,
				White = dlg.WhiteName,
				Result = dlg.Result,
			};

			if (dlg.IsSaved) {
				using (var db = new WeiqiContext()) {
					db.Chesses.Add(step);
					db.SaveChanges();
					MessageBox.Show("Save step success!");
				}
			}
		}

		#endregion

		protected bool NotIn(int col, int row)
		{
			if (col < 0 || col > 18 || row < 0 || row > 18) {
				return true;
			}

			if (Steps[col, row].Color != ChessColor.Empty) {
				return true;
			}

			if (NotInPos.X == col && NotInPos.Y == row) {
				return true;
			} else {
				// If Pos(struct) is property, must use new.
				NotInPos = new Pos(-1, -1);
			}

			return false;
		}

		// For Size Change and Show Number

		List<Step> m_steps = new List<Step>();
		public void FillSteps()
		{
			m_steps.Clear();
			string s = m_ContentString.ToString();
			if (s.Length < 1) return;
			string[] steps = s.Substring(0, s.Length - 1).Split(',');
			Step step = new Step();
			for (int i = 0; i < steps.Length; i++) {
				if (i % 3 == 0) {
					step = new Step();
					step.Col = Convert.ToInt32(steps[i]);
				} else if (i % 3 == 1) {
					step.Row = Convert.ToInt32(steps[i]);
				} else if (i % 3 == 2) {
					step.Count = Convert.ToInt32(steps[i]);
					m_steps.Add(step);
				}
			}
		}
		public void RenderChess()
		{
			m_ContentString.Clear();
			foreach (var item in m_steps) {
				NextOne();
			}
		}

		int m_ContentCount = 1;
		public void NextOne()
		{
			if (m_ContentCount > m_steps.Count) {
				return;
			}

			foreach (var item in m_steps) {
				if (item.Count == m_ContentCount) {
					int col = item.Col;
					int row = item.Row;

					if (Steps[col, row].Color != ChessColor.Empty) {
						return;
					}

					if (NotInPos.X == col && NotInPos.Y == row) {
						return;
					} else {
						// If Pos(struct) is property, must use new.
						NotInPos = new Pos(-1, -1);
					}

					DrawChess(col, row);

					Eat(col, row);
				}
			}
			m_ContentCount++;
		}

		public bool IsShowNumber { get; set; }
	}
}
