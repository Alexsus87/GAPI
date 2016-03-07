using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BritishCab.Models
{
	public class BookingEntity
	{
		public int BookingEntityId { get; set; }
		[Required(ErrorMessage = "Pick Up location cannot be empty")]
		public string PickUpLocation { get; set; }
		[Required(ErrorMessage = "Drop location cannot be empty")]
		public string DropLocation { get; set; }
		[DisplayName("Pick up date")]
		public DateTime PickUpDateTime { get; set; }
		[DisplayName("Pick up time")]
		public DateTime PickUpTime { get; set; }
		public DateTime DriverActualDepartureTime { get; set; }
		public TimeSpan TransferTime { get; set; }
		public TimeSpan TotalTime { get; set; }
		public int DrivingDistance { get; set; }
		public int TotalDrivingDistance { get; set; }
		public string ErrorMessage { get; set; }
		public string PhoneNumber { get; set; }
		public string Email { get; set; }
		public bool IsSlotAvailable { get; set; }
		public bool IsSlotCheckWasMade { get; set; }
		public string Name { get; set; }
		public double Price { get; set; }
		public Guid ConfirmationCode { get; set; }

	}
}
