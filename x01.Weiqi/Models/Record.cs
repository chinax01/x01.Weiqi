using System;
using System.Collections.Generic;
using x01.Weiqi.Boards;

namespace x01.Weiqi.Models
{
	public class Record
	{
		public int Id { get; set; }
		public string Type { get; set; }	// 0：对局，1：布局，2：定式，3：棋型。
		public string Description { get; set; }
		public string White { get; set; }
		public string Black { get; set; }
		public string Result { get; set; }
		public DateTime SaveDate { get; set; }

		public string Steps { get; set; }
	}
}
