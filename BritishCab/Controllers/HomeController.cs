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
			return View();
		}

		[HttpPost]
		public ActionResult Index(BookingEntity booking)
		{

			DistanceMatrix dm = new DistanceMatrix();
			dm = Api.GetRouteInformation(booking.PickUpLocation, booking.DropLocation);
			if (dm.ErrorBit)
			{
				booking.ErrorMessage = "Please check spelling on locations";
				ViewBag.Error = "Please check spelling on locations";
				return View(booking);
			}

			booking.DrivingDistance = dm.TravelDistance;
			booking.TransferTime = TimeSpan.FromSeconds( dm.TravelTime);
			TimeSpan GetToOrigin = TimeSpan.FromSeconds(dm.HomeToOriginTime);
			booking.DriverActualDepartureTime = booking.PickUpDateTime.Subtract(GetToOrigin);
			booking.TotalDrivingDistance = dm.TotalTravelDistance;
			TimeSpan totalTime = TimeSpan.FromSeconds(dm.TotalTravelTime);
			booking.TotalTime = totalTime;
			return RedirectToAction("Booking",booking);
		}


		public ActionResult Booking(BookingEntity booking)
		{
			#region Confirming the order
			var queryValues = Request.QueryString.Get("confirmation");
			Guid securityCode;

			if (Guid.TryParse(queryValues, out securityCode) && securityCode != Guid.Empty)
			{
				using (var db = new DefaultContext())
				{
					var bookingInfo = (from DBbooking in db.BookingEntities
									   where DBbooking.ConfirmationCode == securityCode
									   select DBbooking).FirstOrDefault();
					if (bookingInfo != null)
					{
						if (bookingInfo.IsSlotAvailable)
						{
							Api.InsertEventToCalendar(bookingInfo);
						}
						Api.SendEmailViaGmail(bookingInfo, true, null);
						bookingInfo.ConfirmationCode = Guid.Empty;
						db.SaveChanges();
					}
					return View("Confirmation");
				}
			}
			#endregion

			if (!booking.IsSlotCheckWasMade)
			{
				var events = Api.GetEventsFromCalendar(booking.DriverActualDepartureTime, booking.DriverActualDepartureTime.Add(booking.TotalTime));
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
			//if(Request.HttpMethod == "POST")
			//{
			//	using (var db = new DefaultContext())
			//	{
			//		booking.ConfirmationCode = Guid.NewGuid();
			//		//TODO: fix this
			//		booking.DriverActualDepartureTime = booking.PickUpDateTime;
			//		db.BookingEntities.Add(booking);
			//		db.SaveChanges();
			//	}
			//	var Url = HttpContext.Request.Url.ToString();
			//	Api.SendEmailViaGmail(booking, false, Url);
			//	return View("Submit");
			//}

			//Get route price
			if (booking.PickUpLocation != null && booking.DropLocation != null)
			{
				Api.LoadPricesFromXml();
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