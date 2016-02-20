using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Functions.Models
{
	public class BookingEntity
	{
		public int Id { get; set; }
		public string PickUpLocation { get; set; }
		public string DropLocation { get; set; }
		public DateTime PickUpDateTime { get; set; }
		public DateTime DriverActualDepartureTime { get; set; }
		public TimeSpan TransferTime { get; set; }
		public TimeSpan TotalTime { get; set; }
		public int DrivingDistance { get; set; }
		public int TotalDrivingDistance { get; set; }
	}
}
