using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x01.Weiqi.Boards;

namespace x01.Weiqi.Core
{
	public interface IThink
	{
		// type: 对局，布局，定式，棋型
		List<Pos> Think(string type);
	}
}
