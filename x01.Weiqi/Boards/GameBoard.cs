using System.Windows.Input;

namespace x01.Weiqi.Boards
{
	class GameBoard : Board
	{

		#region Override

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			int row = (int)e.GetPosition(this).Y / StoneSize;
			int col = (int)e.GetPosition(this).X / StoneSize;

			NextOne(row, col);
		}

		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);

			BackOne();
		}

		#endregion
	}
}
