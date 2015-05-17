using System.Data.Entity;

namespace x01.Weiqi.Models
{
	class WeiqiContext : DbContext
	{
		public WeiqiContext()
			: base("DefaultConnection")
		{
		}

		public DbSet<Record> Records { get; set; }
	}
}