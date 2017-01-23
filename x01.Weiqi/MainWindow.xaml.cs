using System;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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
		AiBoard m_AiBoard =  new AiBoard();

		public MainWindow()
		{
			// 警告：取消注释，数据库表结构改变时，将删除所有数据！
			//Database.SetInitializer(new DropCreateDatabaseIfModelChanges<WeiqiContext>());

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
			m_MenuImport.Click += new RoutedEventHandler(m_MenuImport_Click);
			m_MenuEdit.Click += (s, e) => new EditStepWindow().ShowDialog();
			m_MenuExit.Click += (s, e) => Close();

			m_MenuShowNumber.Click += (s, e) => ShowNumber();
			m_MenuShowMeshes.Click += (s, e) => ShowMeshes();
			m_MenuPlaySound.Click += (s, e) => PlaySound();
			m_MenuClearAll.Click += (s, e) => ClearAll();

			m_MenuAbout.Click += m_MenuAbout_Click;
		}

		void m_MenuImport_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog();
			dlg.Multiselect = true;
			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				var filenames = dlg.FileNames;
				var steps = new StringBuilder();
				using (var db = new WeiqiContext()) {
					foreach (var filename in filenames) {
						var s = File.ReadAllText(dlg.FileName);
						string[] splits = s.Split(';');
						int count = splits.Length;
						steps.Clear();
						int stepCount = 0;
						for (int i = 2; i < count - 1; i++) {
							if (splits[i][2] >= 'a' && splits[i][2] <= 's'
							    && splits[i][3] >= 'a' && splits[i][3] <= 's')
							{
								steps.Append((splits[i][2]-'a') + "," + (splits[i][3]-'a') + "," + stepCount.ToString() + "," );
								stepCount ++;
							}
						}
					
						Record record = new Record() {
							Black = "Black",
							White = "White",
							Result = "B&W",
							Description = "Imported SGF",
							Type = "对局",
							SaveDate = DateTime.Now,
							Steps = steps.ToString()
						};
						db.Records.Add(record);
					}
					db.SaveChanges();
				}
				System.Windows.MessageBox.Show("Save SGF success!\n");
			}
		}

		void PlaySound()
		{
			m_Board.IsPlaySound = !m_Board.IsPlaySound;
			m_MenuPlaySound.IsChecked = m_Board.IsPlaySound;
		}

		private void ShowMeshes()
		{
			m_Board.IsShowMesh = m_MenuShowMeshes.IsChecked;
			if (m_Board.IsShowMesh) m_Board.ShowMeshes();
			else m_Board.HideMeshes();
			m_Board.Redraw();
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
			if (board != m_AiBoard) {
				m_AiBoard.AiTimer.Stop();
			} else {
				m_AiBoard.AiTimer.Start();
			}

			ClearAll();
			if (m_Grid.Children.Contains(m_Board))
				m_Grid.Children.Remove(m_Board);

			board.IsShowCurrent = false;
			if (board == m_StepBoard) {
				(board as StepBoard).LoadSteps();
			}
			m_Board = board;
			m_Grid.Children.Add(m_Board);
			m_Board.SetValue(Grid.RowProperty, 1);
			m_Board.StoneSize = (int)((Math.Min(ActualWidth, ActualHeight) - 60) / 19);
			board.IsShowCurrent = true;
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
				new EditStepWindow().ShowDialog();
			} else if (e.Key == Key.X && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
				Close();
			} else if (e.Key == Key.F1) {
				ShowNumber();
			} else if (e.Key == Key.F2) {
				m_MenuShowMeshes.IsChecked = !m_MenuShowMeshes.IsChecked;
				ShowMeshes();
			} else if (e.Key == Key.F3) {
				PlaySound();
			} else if (e.Key == Key.Escape) {
				ClearAll();
			} else if (e.Key == Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
				m_MenuAbout_Click(null, null);
			} else if (e.Key == Key.P) {    // Pass
				m_Board.StepCount++;
			}
		}

		private void ClearAll()
		{
			int count = m_Board.StepCount; //base 0
			for (int i = 0; i < count; i++) {
				m_Board.BackOne();
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			var board = m_Board as AiBoard;
			if (board != null) board.AiTimer.Stop();
		}
	}
}
