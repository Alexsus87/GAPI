using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using BritishCab.Models;

namespace BritishCab
{
	public class ApiMethods
	{
		private string[] Scopes = { CalendarService.Scope.Calendar };

		private string applicationName = "Google Calendar API .NET Quickstart";

		private UserCredential credential;

		private CalendarService service;

		const string homeTown = "Bristol";

		public ApiMethods()
		{
			string path = HttpContext.Current.Server.MapPath("~/client_secret.json");
			using (var stream =
				new FileStream(path, FileMode.Open, FileAccess.Read))
			{
			//string credPath = System.Environment.GetFolderPath(
			//    System.Environment.SpecialFolder.Personal);

			//credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart");

			//credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
			//    GoogleClientSecrets.Load(stream).Secrets,
			//    Scopes,
			//    "user",
			//    CancellationToken.None,
			//    new FileDataStore(credPath, true)).Result;
			credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
				GoogleClientSecrets.Load(stream).Secrets,
				Scopes,
				"user",
				CancellationToken.None,
				new FileDataStore(HttpContext.Current.Server.MapPath("~/Content"), true)).Result;
			}
			// Create Google Calendar API service.
			service = new CalendarService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = applicationName,
			});
		}

		public void PopulateBooking(DistanceMatrix dm, BookingEntity booking)
		{
			booking.DrivingDistance = dm.TravelDistance;
			booking.TransferTime = TimeSpan.FromSeconds(dm.TravelTime);
			booking.DriverActualDepartureTime = booking.PickUpDateTime.Subtract(TimeSpan.FromSeconds(dm.HomeToOriginTime));
			booking.TotalDrivingDistance = dm.TotalTravelDistance;
			booking.TotalTime = TimeSpan.FromSeconds(dm.TotalTravelTime);
		}

		/// <summary>
		/// returns driving distance in kilometers
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public DistanceMatrix GetRouteInformation(string origin, string destination)
		{
			DistanceMatrix dm = new DistanceMatrix();

			// Getting Distance and time for roundTrip
			var clientTransfer = GetDistanceAndTime(origin, destination);
			if (clientTransfer.ErrorBit)
			{
				dm.ErrorBit = clientTransfer.ErrorBit;
				return dm;
			}
			var homeToOrigin = GetDistanceAndTime(homeTown, origin);
			var destinationToHome = GetDistanceAndTime(destination, homeTown);

			//Filling values
			dm.TravelTime = clientTransfer.TravelTime;
			dm.TravelDistance = clientTransfer.TravelDistance;
			dm.HomeToOriginTime = homeToOrigin.TravelTime;
			dm.HomeToOriginDistance = homeToOrigin.TravelDistance;
			dm.DestinationToHomeTime = destinationToHome.TravelTime;
			dm.DestinationToHomeDistance = destinationToHome.TravelDistance;
			dm.TotalTravelTime = clientTransfer.TravelTime + homeToOrigin.TravelTime + destinationToHome.TravelTime;
			dm.TotalTravelDistance = clientTransfer.TravelDistance + homeToOrigin.TravelDistance +
									destinationToHome.TravelDistance;

			return dm;
		}

		private TravelDistanceTime GetDistanceAndTime(string origin, string destination)
		{
			string url = @"http://maps.googleapis.com/maps/api/distancematrix/xml?origins=" +
			  origin + "&destinations=" + destination +
			  "&mode=driving&sensor=false&language=en-EN";

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			WebResponse response = request.GetResponse();
			Stream dataStream = response.GetResponseStream();
			StreamReader sreader = new StreamReader(dataStream);
			string responsereader = sreader.ReadToEnd();
			response.Close();

			XmlDocument xmldoc = new XmlDocument();
			xmldoc.LoadXml(responsereader);
			var dm = new TravelDistanceTime();

			if (xmldoc.GetElementsByTagName("status")[0].ChildNodes[0].InnerText == "OK")
			{
				try
				{
					XmlNodeList distance = xmldoc.GetElementsByTagName("distance");
					XmlNodeList drivingTime = xmldoc.GetElementsByTagName("duration");

					dm.TravelTime = Convert.ToDouble(drivingTime[0].ChildNodes[0].InnerText);
					dm.TravelDistance = Convert.ToDouble(distance[0].ChildNodes[1].InnerText.Replace(" km", ""));
					dm.ErrorBit = false;
					return dm;
				}
				catch (Exception)
				{
					dm.ErrorBit = true;
					return dm;
				}
			}
			return dm;
		}

		public void InsertEventToCalendar(BookingEntity booking)
		{
			//Inserting event to calendar
			Event event1 = new Event()
			{
				Summary = String.Format("Route: {0} - {1}, Client: {2}",booking.PickUpLocation, booking.DropLocation, booking.Name),
				Location = booking.PickUpLocation,
				Start = new EventDateTime()
				{
					DateTime = booking.PickUpDateTime,
					//DateTime = new DateTime(2015, 12, 11, 14, 15, 0),
					TimeZone = "Europe/London"
				},
				End = new EventDateTime()
				{
					DateTime = booking.PickUpDateTime.Add(booking.TransferTime),
					//DateTime = new DateTime(2015, 12, 11, 15, 15, 0),
					TimeZone = "Europe/London"
				},
			};
			service.Events.Insert(event1, "primary").Execute();
		}

		public Events GetEventsFromCalendar()
		{
			//Define parameters of request.
			EventsResource.ListRequest request = service.Events.List("primary");
			request.TimeMin = DateTime.Now;
			request.ShowDeleted = false;
			request.SingleEvents = true;
			request.MaxResults = 10;
			request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

			// List events.
			Events events = request.Execute();
			Console.WriteLine("Upcoming events:");
			if (events.Items != null && events.Items.Count > 0)
			{
				foreach (var eventItem in events.Items)
				{
					string when = eventItem.Start.DateTime.ToString();
					if (String.IsNullOrEmpty(when))
					{
						when = eventItem.Start.Date;
					}
					Console.WriteLine("{0} ({1})", eventItem.Summary, when);
				}
				return events;
			}
			else
			{
				Console.WriteLine("No upcoming events found.");
				return null;
			}
		}

		public void GetSlotAvailability(BookingEntity booking)
		{
			var events = GetEventsFromCalendar(booking.DriverActualDepartureTime, booking.DriverActualDepartureTime.Add(booking.TotalTime));
			if (events.Items != null && events.Items.Count > 0)
			{
				booking.IsSlotAvailable = false;
			}
			else
			{
				booking.IsSlotAvailable = true;
			}
		}
		private Events GetEventsFromCalendar(DateTime TimeMin, DateTime TimeMax)
		{
			//Define parameters of request.
			EventsResource.ListRequest request = service.Events.List("primary");
			request.TimeMin = TimeMin;
			request.TimeMax = TimeMax;
			request.ShowDeleted = false;
			request.SingleEvents = true;
			request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

			// List events.
			Events events = request.Execute();
			return events;
		}

		public bool SendEmailViaGmail(BookingEntity booking, bool isFinal, string localUrl)
		{
			SmtpClient client = new SmtpClient();
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.EnableSsl = true;
			client.Host = "smtp.gmail.com";
			client.Port = 587;

			var securityCode = new Guid();

			// setup Smtp authentication
			NetworkCredential credentials =
				new NetworkCredential("driverfrombritain@gmail.com", "T!T@n1130");
			client.UseDefaultCredentials = false;
			client.Credentials = credentials;

			MailMessage msg = new MailMessage();
			msg.From = new MailAddress("driverfrombritain@gmail.com");
			msg.To.Add(new MailAddress(booking.Email));

			msg.IsBodyHtml = true;
			if (isFinal)
			{
				msg.Subject = "Booking information";
				msg.Body = "<html><head></head><body><b>Thanks for booking! We will be in touch with you shortly!</b></body>";
			}
			else
			{
				msg.Subject = "This is a test Email subject";
				msg.Body = string.Format("<html><head></head><body><b>{0}?confirmation={1}</b></body>", localUrl, booking.ConfirmationCode);
			}

			try
			{
				client.Send(msg);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}

		}

		public void GetRoutePrice(BookingEntity bookingEntity)
		{
			var predefinedPrices = LoadPricesFromXml();
			var from = bookingEntity.PickUpLocation.ToUpper();
			var to = bookingEntity.DropLocation.ToUpper();
			var price = 0.0;
			foreach (var predefinedPrice in predefinedPrices)
			{
				if (from.Contains(predefinedPrice.From) && to.Contains(predefinedPrice.To))
				{
					price = predefinedPrice.Price;
				}
			}
			if (price == 0.0)
			{
				var pricePerKm = 1.80;
				var priceForTransfer = bookingEntity.TotalDrivingDistance*pricePerKm;
				bookingEntity.Price = priceForTransfer;
			}
			else
			{
				bookingEntity.Price = price;
			}
		}

		private IEnumerable<PredefinedPrice> LoadPricesFromXml()
		{
			var listOfPrices = new List<PredefinedPrice>();
			string path = HttpContext.Current.Server.MapPath("Prices/Prices.xml");
			XmlDocument xmlDocument = new XmlDocument(); 
			xmlDocument.Load(path);
			XmlElement xelRoot = xmlDocument.DocumentElement;
			XmlNodeList xnlNodes = xelRoot.SelectNodes("/prices/route");

			foreach (XmlNode xndNode in xnlNodes)
			{
				try
				{
					var pricingOption = new PredefinedPrice();

					pricingOption.From = xndNode["From"].InnerText.ToUpper();
					pricingOption.To = xndNode["To"].InnerText.ToUpper();
					string priceFromXml = xndNode["Price"].InnerText;
					double price;
					if (Double.TryParse(priceFromXml, out price))
					{
						pricingOption.Price = price;
						listOfPrices.Add(pricingOption);
					}
				}
				catch (Exception)
				{ }
			}
			return listOfPrices;
		}
	}
}
