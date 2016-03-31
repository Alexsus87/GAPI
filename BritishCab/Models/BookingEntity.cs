﻿using System;
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

		[DisplayName("Pick up at")]
		[Required(ErrorMessage = "Please choose pick up city")]
		public string PickUpLocation { get; set; }

		[DisplayName("Destination")]
		[Required(ErrorMessage = "Please choose destination")]
		public string DropLocation { get; set; }

		[DisplayName("Pick up date")]
		public DateTime PickUpDateTime { get; set; }

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
		public double Price { get; set; }
		public Guid ConfirmationCode { get; set; }
		public string Comments { get; set; }

	}
}
