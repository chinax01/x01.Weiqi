using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x01.Weiqi.Model
{
	public class Step
	{
		public int Id { get; set; }
		public string Content { get; set; }
		public string WhiteName { get; set; }
		public string BlackName { get; set; }
		public string Winer { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
