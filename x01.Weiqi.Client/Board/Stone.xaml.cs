// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System.Windows.Controls;
using System.Windows.Media;

namespace x01.Weiqi.Board
{
	/// <summary>
	/// Interaction logic for Chess.xaml
	/// </summary>
	public partial class Stone : UserControl
	{
		public string NumberText
		{
			get { return m_TxtNumber.Text; }
			set { m_TxtNumber.Text = value; }
		}

		public double NumberFontSize
		{
			get { return m_TxtNumber.FontSize; }
			set { m_TxtNumber.FontSize = value; }
		}

		public System.Windows.Thickness NumberPadding
		{
			get { return m_TxtNumber.Padding; }
			set { m_TxtNumber.Padding = value; }
		}

		public Brush NumberBrush
		{
			get { return m_TxtNumber.Foreground; }
			set { m_TxtNumber.Foreground = value; }
		}

		public Stone(Brush fillBrush, int size = 38)
		{
			InitializeComponent();
			m_Grid.Width = m_Grid.Height = size;
			m_Ellipse.Fill = fillBrush;
			m_Ellipse.Width = m_Ellipse.Height = size;
		}

		public void SetShow(bool isShow)
		{
			if (isShow)
				m_TxtNumber.Visibility = System.Windows.Visibility.Visible;
			else
				m_TxtNumber.Visibility = System.Windows.Visibility.Hidden;
		}
	}
}
