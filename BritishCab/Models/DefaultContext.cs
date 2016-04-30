using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BritishCab.Models
{
	public class DefaultContext: DbContext
	{
		public DefaultContext()
			: base("DefaultConnection")
		{
			
		}

		public DbSet<Booking> Bookings { get; set; }
	}
}