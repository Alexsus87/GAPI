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
	public enum BookingStatus
	{
		PayOnSight,
		NotDefined,
		Paid,
		NotActive
	};
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
		public ActionResult Index(Booking booking)
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
		public ActionResult Booking(Booking booking)
		{
			return View(booking);
		}

		[HttpPost]
		public ActionResult Booking(Booking booking, string post)
		{
			var bookingInput = new Booking
			{
			    PickUpLocation = booking.PickUpLocation,
			    DropLocation = booking.DropLocation,
			    PickUpDateTime = booking.PickUpDateTime
			};

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

		public ActionResult FinalizeBooking(Booking booking)
		{
			if (booking.PickUpLocation == null || booking.DropLocation == null || booking.Price == 0.0)
			{
				return RedirectToAction("Booking");
			}

			if (Request.HttpMethod != "POST") return View(booking);

			booking.ConfirmationCode = Guid.NewGuid();
			using (var db = new DefaultContext())
			{
				booking.DriverActualDepartureTime = booking.PickUpDateTime;
				booking.BookingStatus = BookingStatus.NotDefined;
				db.Bookings.Add(booking);
				db.SaveChanges();
			}

			return View("Payment", booking);
		}

		public ActionResult Submit(Booking booking)
		{
			#region Confirming the order

			if (_api.OrderConfirmation(Request, BookingStatus.PayOnSight, "confirmation"))
			{
				return View("Confirmation");
			}
			if (Request.QueryString.Get("confirmation")!=null)
			{
				return RedirectToAction("Index");
			}

			#endregion

			var Url = HttpContext.Request.Url.ToString();

			using (var db = new DefaultContext())
			{
				booking = (from book in db.Bookings
					where book.ConfirmationCode == booking.ConfirmationCode
					select book).FirstOrDefault();
			}
			ViewBag.Url = string.Format("{0}?confirmation={1}", Url, booking.ConfirmationCode);
			if (Request.HttpMethod == "POST")
			{
				_api.SendEmailViaGmail(booking, false, Url, null);
				//booking.BookingStatus = BookingStatus.PayOnSight;
				return View();
			}
			return View("Payment",booking);
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

	    public ActionResult PDT()
	    {
            #region Confirming the order
			if (_api.OrderConfirmation(Request, BookingStatus.Paid, "item_number"))
			{
				return View("Confirmation");
			}
            #endregion
	        return View("Submit");
	    }

		public ActionResult Services()
		{
			return View();
		}

		public ActionResult Redirect(Booking booking)
		{
			return RedirectToAction("Booking", booking);
		}
	}
}