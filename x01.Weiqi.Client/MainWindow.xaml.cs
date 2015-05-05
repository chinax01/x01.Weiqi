// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Text;
using System.Windows;
using System.Data.Entity;
using x01.Weiqi.Board;

namespace x01.Weiqi
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		IBoard m_Board;
		enum InitFlag { Game, Inet, Where, Step }

		public MainWindow()
		{
			Database.SetInitializer(new DropCreateDatabaseIfModelChanges<Weiqi.Model.WeiqiContext>());

			InitializeComponent();

			this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
			this.WindowState = System.Windows.WindowState.Maximized;
			InitBoard(InitFlag.Game);
			this.SizeChanged += MainWindow_SizeChanged;
		}

		bool m_isSizeChanged = false;	// For StepBoard
		void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Width = ActualWidth;
			Height = ActualHeight;

			m_isSizeChanged = true;
			Redraw();
			m_isSizeChanged = false;
		}

		private void MenuSave_Click(object sender, RoutedEventArgs e)
		{
			if (m_Board != null)
			{
				m_Board.Save();
			}
		}

		private void MenuStart_Click(object sender, RoutedEventArgs e)
		{
			InitBoard(InitFlag.Game);
		}

		private void MenuLoad_Click(object sender, RoutedEventArgs e)
		{
			InitBoard(InitFlag.Step);
			var b = m_Board as StepBoard;
			if (b.StepId == -1) {
				MessageBox.Show("Load failed! Will goto start.");
				InitBoard(InitFlag.Game);
			}
		}

		private void MenuInet_Click(object sender, RoutedEventArgs e)
		{
			InitBoard(InitFlag.Inet);
		}

		InitFlag m_initFlag = InitFlag.Game;
		StringBuilder m_sbSteps = new StringBuilder();
		private void InitBoard(InitFlag flag)
		{
			m_initFlag = flag;

			if (m_Board != null)
			{
				m_DockPanel.Children.Remove(m_Board as UIElement);
				m_Board = null;
			}

			int size = (int)((Math.Min(Width, Height) - 60) / 19);
			switch (flag)
			{
				case InitFlag.Game:
					m_Board = new GameBoard(size);
					Title = "x01.Weiqi - GameBoard";
					break;
				case InitFlag.Inet:
					try
					{
						m_Board = new InetBoard(size);
					}
					catch
					{
						m_Board = new GameBoard(size);
						Title = "x01.Weiqi - GameBoard";
						MessageBox.Show("Please start server before running InetBoard!");
						break;
					}
					Title = "x01.Weiqi - InetBoard";
					break;
				case InitFlag.Where:
					m_Board = new WhereBoard(size);
					Title = "x01.Weiqi - WhereBoard";
					break;
				case InitFlag.Step:
					m_Board = new StepBoard(size, m_isSizeChanged);
					Title = "x01.Weiqi - StepBoard";
					break;
				default:
					throw new Exception("Has not this board.");
			}

			m_DockPanel.Children.Add(m_Board as UIElement);
		}

		private void MenuWhere_Click(object sender, RoutedEventArgs e)
		{
			InitBoard(InitFlag.Where);
		}

		bool m_isShowNumber = true;
		private void MenuShowNumber_Click(object sender, RoutedEventArgs e)
		{
			m_isShowNumber = !m_isShowNumber;
			(m_Board as BoardBase).IsShowNumber = m_isShowNumber;
			Redraw();
		}

		private void Redraw()
		{
			if (m_initFlag == InitFlag.Step) {
				var b = m_Board as StepBoard;
				int id = b.StepId;
				int count = b.StepCount;
			
				InitBoard(m_initFlag);
				b = m_Board as StepBoard;
				b.StepId = id;
				b.ContentString = new StringBuilder(b.StepService.GetContent(id));
				b.FillSteps();
				for (int i = 0; i < count; i++) {
					b.NextOne();
				}
			} else {
				m_sbSteps = (m_Board as BoardBase).ContentString;
				
				InitBoard(m_initFlag);
				var b = m_Board as BoardBase;
				b.IsShowNumber = m_isShowNumber;
				b.ContentString = m_sbSteps;
				b.FillSteps();
				b.RenderChess();
			}
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			new AboutWindow().ShowDialog();
		}

		private void MenuClearAll_Click(object sender, RoutedEventArgs e)
		{
			var b = m_Board as BoardBase;
			int count = b.StepCount;
			for (int i = 0; i < count; i++) {
				b.BackOne();
			}
		}
	}
}
