using BritishCab.Models;
using API_Functions;
//using API_Functions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BritishCab.Controllers
{
	public class HomeController : Controller
	{
		private ApiMethods Api = new ApiMethods();

		[HttpGet]
		public ActionResult Index()
		{
			var queryValues = Request.QueryString.Get("confirmation");
			var securityCode = Guid.NewGuid();

			return View();
		}
		[HttpPost]
		public ActionResult Index(BookingEntity booking)
		{
			DistanceMatrix dm = new DistanceMatrix();
			dm = Api.GetDrivingDistanceInKilometers(booking.PickUpLocation, booking.DropLocation);
			if (dm.ErrorBit)
			{
				booking.ErrorMessage = "Please check spelling on locations";
				ViewBag.Error = "Please check spelling on locations";
				return View(booking);
			}
			booking.DrivingDistance = dm.TravelDistance;
			booking.TransferTime = TimeSpan.FromSeconds( dm.TravelTime);
			//Api.InsertEventToCalendar("Test booking", booking.PickUpLocation, new DateTime(2016, 02, 29, 14, 15, 0), new DateTime(2016, 02, 29, 14, 30, 0));
			return RedirectToAction("Booking",booking);
		}


		public ActionResult Booking(BookingEntity booking)
		{
			if (!booking.IsSlotCheckWasMade)
			{
				var events = Api.GetEventsFromCalendar(booking.PickUpDateTime, booking.PickUpDateTime.Add(booking.TransferTime));
				if (events.Items != null && events.Items.Count > 0)
				{
					booking.IsSlotAvailable = false;
				}
				else
				{
					booking.IsSlotAvailable = true;
				}
				booking.IsSlotCheckWasMade = true;
			}
			return View(booking);
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}