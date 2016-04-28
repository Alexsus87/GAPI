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
			_api.GetRoutePrice(booking);

			return RedirectToAction("Redirect",booking);
		}

		[HttpGet]
		public ActionResult Booking(BookingEntity booking)
		{
			return View(booking);
		}

		[HttpPost]
		public ActionResult Booking(BookingEntity booking, string post)
		{
			var bookingInput = new BookingEntity();
			bookingInput.PickUpLocation = booking.PickUpLocation;
			bookingInput.DropLocation = booking.DropLocation;
			bookingInput.PickUpDateTime = booking.PickUpDateTime;

			_dm = _api.GetRouteInformation(bookingInput.PickUpLocation, bookingInput.DropLocation);
			if (_dm.ErrorBit)
			{
				bookingInput.ErrorMessage = "Please check spelling on locations";
				ViewBag.Error = "Please check spelling on locations";
				return View(bookingInput);
			}

			_api.PopulateBooking(_dm, bookingInput);
			_api.GetRoutePrice(bookingInput);

			if (Request.Form["Book"] != null)
			{
				if (Math.Round(bookingInput.Price,2) == booking.Price)
				{
					return RedirectToAction("FinalizeBooking", booking);
				}
			}
			return RedirectToAction("Redirect", bookingInput);
		}

		public ActionResult FinalizeBooking(BookingEntity booking)
		{
			if (Request.HttpMethod == "POST")
			{
				booking.ConfirmationCode = Guid.NewGuid();
				using (var db = new DefaultContext())
				{
					//TODO: fix this
					booking.DriverActualDepartureTime = booking.PickUpDateTime;
					db.BookingEntities.Add(booking);
					db.SaveChanges();
				}

				//_api.SendEmailViaGmail(booking, false, Url);
				return View("Payment", booking);
			}
			return View(booking);
		}

		public ActionResult Submit(BookingEntity booking)
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
			var Url = HttpContext.Request.Url.ToString();

			using (var db = new DefaultContext())
			{
				booking = (from book in db.BookingEntities
					where book.ConfirmationCode == booking.ConfirmationCode
					select book).FirstOrDefault();
			}
			ViewBag.Url = string.Format("{0}?confirmation={1}", Url, booking.ConfirmationCode);
			if (Request.HttpMethod == "POST")
			{
				_api.SendEmailViaGmail(booking, false, Url);
				return View();
			}
			return View("Payment",booking);
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Services()
		{
			return View();
		}

		public ActionResult Redirect(BookingEntity booking)
		{
			return RedirectToAction("Booking", booking);
		}
	}
}