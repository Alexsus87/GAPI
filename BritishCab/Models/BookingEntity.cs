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
		private DateTime _pickUpDateTime;

		public int BookingEntityId { get; set; }

		[DisplayName("Pick up at")]
		public string PickUpLocation { get; set; }

		[DisplayName("Pick Up Address")]
		public string PickUpAddress { get; set; }

		[DisplayName("Destination")]
		public string DropLocation { get; set; }

		[DisplayName("Destination Address")]
		public string DropAddress { get; set; }

		[DisplayName("Pick up date")]
		public DateTime PickUpDateTime
		{
			get
			{
				if (_pickUpDateTime == DateTime.MinValue)
				{
					return DateTime.Now;
				}
				return _pickUpDateTime;
			}
			set { _pickUpDateTime = value; }
		}

		[DisplayName("Pick up time")]
		public DateTime DriverActualDepartureTime { get; set; }

		[DisplayName("Estimate transfer time")]
		public TimeSpan TransferTime { get; set; }
		public TimeSpan TotalTime { get; set; }
		public double DrivingDistance { get; set; }
		public double TotalDrivingDistance { get; set; }
		public string ErrorMessage { get; set; }

		[DisplayName("Contact number")]
		public string PhoneNumber { get; set; }
		public string Email { get; set; }
		public bool IsSlotAvailable { get; set; }
		public bool IsSlotCheckWasMade { get; set; }
		public string Name { get; set; }
		[DisplayName("Price(£)")]
		public double Price { get; set; }
		public Guid ConfirmationCode { get; set; }
		[DisplayName("Additional comments")]
		public string Comments { get; set; }

	}
}
