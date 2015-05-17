using System;
using System.Collections.Generic;
using x01.Weiqi.Boards;

namespace x01.Weiqi.Models
{
	public class Record
	{
		public int Id { get; set; }
		public string Description { get; set; }
		public string White { get; set; }
		public string Black { get; set; }
		public string Result { get; set; }
		public DateTime SaveDate { get; set; }

		public string Steps { get; set; }
	}
}
