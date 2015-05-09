using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x01.Weiqi.Model
{
	public class Record
	{
		public int Id { get; set; }
		public string Description { get; set; }
		public string Steps { get; set; }	// multi "col,row,count,"
		public string White { get; set; }
		public string Black { get; set; }
		public string Result { get; set; }
		public DateTime SaveDate { get; set; }
	}
}
