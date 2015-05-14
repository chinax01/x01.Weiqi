using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using x01.Weiqi.Boards;

namespace x01.Weiqi
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		Board m_Board;
		GameBoard m_GameBoard = new GameBoard();

		public MainWindow()
		{
			InitializeComponent();

			m_Board = m_GameBoard;

			Loaded += (s, e) => InitBoard(m_GameBoard);
			SizeChanged += (s, e) =>
				m_Board.StoneSize = (int)((Math.Min(ActualWidth, ActualHeight) - 60) / 19);
		}

		void InitBoard(Board board)
		{
			m_Board = board;
			m_Grid.Children.Add(m_Board);
			m_Board.SetValue(Grid.RowProperty, 1);
			m_Board.StoneSize = (int)((Math.Min(ActualWidth, ActualHeight) - 60) / 19);
		}
	}
}
