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
		private ApiMethods _api = new ApiMethods();
		private DistanceMatrix _dm = new DistanceMatrix();

		[HttpGet]
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Index(BookingEntity booking)
		{

			//Validate input and get route stats
			_dm = _api.GetRouteInformation(booking.PickUpLocation, booking.DropLocation);
			if (_dm.ErrorBit)
			{
				booking.ErrorMessage = "Please check spelling on locations";
				ViewBag.Error = "Please check spelling on locations";
				return View(booking);
			}

			_api.PopulateBooking(_dm,booking);

			return RedirectToAction("Booking",booking);
		}


		public ActionResult Booking(BookingEntity booking, string btnBooking, string btnCalculate )
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
							_api.InsertEventToCalendar(bookingInfo);
						}
						_api.SendEmailViaGmail(bookingInfo, true, null);
						bookingInfo.ConfirmationCode = Guid.Empty;
						db.SaveChanges();
					}
					return View("Confirmation");
				}
			}
			#endregion

			if (Request.Form["Booking"] != null)
			{
				_api.GetSlotAvailability(booking);
			}
			_api.GetSlotAvailability(booking);
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
				_api.GetRoutePrice(booking);
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