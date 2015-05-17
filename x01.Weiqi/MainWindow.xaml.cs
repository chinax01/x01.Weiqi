using System;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using x01.Weiqi.Boards;
using x01.Weiqi.Models;
using x01.Weiqi.Windows;

namespace x01.Weiqi
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		Board m_Board;
		GameBoard m_GameBoard = new GameBoard();
		StepBoard m_StepBoard = new StepBoard();
		AiBoard m_AiBoard = new AiBoard();

		public MainWindow()
		{
			Database.SetInitializer(new DropCreateDatabaseIfModelChanges<WeiqiContext>());

			InitializeComponent();

			m_Board = m_GameBoard;

			Loaded += (s, e) => InitBoard(m_GameBoard);
			SizeChanged += (s, e) =>
				m_Board.StoneSize = (int)((Math.Min(ActualWidth, ActualHeight) - 60) / 19);

			m_MenuStart.Click += (s, e) => InitBoard(m_GameBoard);
			m_MenuAI.Click += (s, e) => InitBoard(m_AiBoard);
			m_MenuLoad.Click += (s, e) => InitBoard(m_StepBoard);
			m_MenuSave.Click += (s, e) => m_Board.SaveSteps();
			m_MenuDelete.Click += (s, e) => new DeleteStepWindow().ShowDialog();
			m_MenuExit.Click += (s, e) => Close();

			m_MenuShowNumber.Click += (s, e) => ShowNumber();
			m_MenuClearAll.Click += (s, e) => ClearAll();

			m_MenuAbout.Click += m_MenuAbout_Click;
		}

		void m_MenuAbout_Click(object sender, RoutedEventArgs e)
		{
			var win = new NavigationWindow();
			win.Content = new AboutPage();
			win.Width = 360;
			win.Height = 280;
			win.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
			win.Show();

			win.NavigationFailed += (w, arg) => arg.Handled = true;

			bool isFirst = true;
			win.Navigated += (w, arg) => {
				if (!isFirst)
					win.WindowState = System.Windows.WindowState.Maximized;
				isFirst = false;
			};

			win.KeyDown += (w, arg) => {
				if (arg.Key == Key.Escape || arg.Key == Key.Enter)
					win.Close();
			};
		}

		void InitBoard(Board board)
		{
			if (m_Grid.Children.Contains(m_Board))
				m_Grid.Children.Remove(m_Board);
			m_Board = board;
			m_Grid.Children.Add(m_Board);
			m_Board.SetValue(Grid.RowProperty, 1);
			m_Board.StoneSize = (int)((Math.Min(ActualWidth, ActualHeight) - 60) / 19);
		}

		void ShowNumber()
		{
			m_Board.IsShowNumber = !m_Board.IsShowNumber;
			m_MenuShowNumber.IsChecked = m_Board.IsShowNumber;
			m_Board.Redraw();
		}

		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Key == Key.G && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
				InitBoard(m_GameBoard);
			} else if (e.Key == Key.I && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
				InitBoard(m_AiBoard);
			} else if (e.Key == Key.L && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
				InitBoard(m_StepBoard);
			} else if (e.Key == Key.S && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
				m_Board.SaveSteps();
			} else if (e.Key == Key.D && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
				new DeleteStepWindow().ShowDialog();
			} else if (e.Key == Key.E && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
				Close();
			} else if (e.Key == Key.F1) {
				ShowNumber();
			} else if (e.Key == Key.Escape) {
				ClearAll();
			} else if (e.Key == Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
				m_MenuAbout_Click(null, null);
			}
		}

		private void ClearAll()
		{
			int count = m_Board.StepCount; //base 0
			for (int i = 0; i < count; i++) {
				m_Board.BackOne();
			}
		}
	}
}
