using BritishCab.Models;
using API_Functions;
using API_Functions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BritishCab.Controllers
{
	public class HomeController : Controller
	{
		[HttpGet]
		public ActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public ActionResult Index(BookingEntity booking)
		{
			ApiMethods Api = new ApiMethods();
			DistanceMatrix dm = new DistanceMatrix();
			dm = Api.GetDrivingDistanceInKilometers(booking.PickUpLocation, booking.DropLocation);
			booking.DrivingDistance = dm.TravelDistance;
			booking.TransferTime = TimeSpan.FromSeconds( dm.TravelTime);
			return View("Booking",booking);
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