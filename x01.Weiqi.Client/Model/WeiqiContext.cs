using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace x01.Weiqi.Model
{
	class WeiqiContext : DbContext
	{
		public WeiqiContext()
			: base("DefaultConnection")
		{
		}

		public DbSet<Step> Steps { get; set; }
	}
}