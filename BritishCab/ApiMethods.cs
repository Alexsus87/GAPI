using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using BritishCab.Controllers;
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

		public void PopulateBooking(DistanceMatrix dm, Booking booking)
		{
			booking.DrivingDistance = dm.TravelDistance;
			Int32 travelTime = (int)RoundSeconds(TimeSpan.FromSeconds(dm.TravelTime)).TotalMinutes;
			var remainder = travelTime%5;
			if ( remainder == 0)
			{
				booking.TransferTime = RoundSeconds(TimeSpan.FromSeconds(dm.TravelTime));
			}
			else
			{
				travelTime += (5 - remainder);
				booking.TransferTime = TimeSpan.FromMinutes(travelTime);
			}

			booking.DriverActualDepartureTime = booking.PickUpDateTime.Subtract(TimeSpan.FromSeconds(dm.HomeToOriginTime));
			booking.TotalDrivingDistance = dm.TotalTravelDistance;
			booking.TotalTime = TimeSpan.FromSeconds(dm.TotalTravelTime);
			booking.PickUpLocation = dm.OriginAddress;
			booking.DropLocation = dm.DestinationAddress;
		}

		/// <summary>
		/// returns driving distance in kilometers
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public DistanceMatrix GetRouteInformation(string origin, string destination)
		{
			var dm = new DistanceMatrix();

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
			dm.OriginAddress = clientTransfer.OriginAddress;
			dm.DestinationAddress = clientTransfer.DestinationAddress;
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
			  "&mode=driving&sensor=false&language=en-EN&units=imperial";

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
					XmlNodeList originAddress = xmldoc.GetElementsByTagName("origin_address");
					XmlNodeList destinationAddress = xmldoc.GetElementsByTagName("destination_address");

					dm.TravelTime = Convert.ToDouble(drivingTime[0].ChildNodes[0].InnerText);
					dm.TravelDistance = Convert.ToDouble(distance[0].ChildNodes[1].InnerText.Replace(" mi", ""));
					dm.OriginAddress = originAddress[0].InnerText;
					dm.DestinationAddress = destinationAddress[0].InnerText;
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

		public void InsertEventToCalendar(Booking booking)
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

		public void GetSlotAvailability(Booking booking)
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

		public bool SendEmailViaGmail(Booking booking, bool isFinal, string localUrl, string driverEmail)
		{
			SmtpClient client = new SmtpClient();
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.EnableSsl = true;
			client.Host = "smtp.gmail.com";
			client.Port = 587;

			var emailAddress = driverEmail ?? booking.Email;

			// setup Smtp authentication
			NetworkCredential credentials =
				new NetworkCredential("driverfrombritain@gmail.com", "T!T@n1130");
			client.UseDefaultCredentials = false;
			client.Credentials = credentials;

			MailMessage msg = new MailMessage();
			msg.From = new MailAddress("driverfrombritain@gmail.com");
			msg.To.Add(new MailAddress(emailAddress));
			msg.IsBodyHtml = true;

			string paymentType;
			if (isFinal)
			{
				paymentType = "Paid";
			}
			else
			{
				paymentType = "Pay in person";
			}

			var bookingInfo = String.Format(@"<h2>Thanks you for booking at VIPDRIVING!</h2>" +
										"<p><strong>Your order details:</strong></p>" +
										"<p><strong>Name:&nbsp;{11}</strong></p>" +
										"<p><strong>From:&nbsp;{6}, {0}</strong></p>" +
										"<p><strong>To:&nbsp;{7}, {1}</strong></p>" +
										"<p><strong>Pick up time:{2}</strong></p>" +
										"<p><strong>Estimated transfer time:{3}</strong></p>" +
										"<p><strong>Contact number:{4}</strong></p>" +
										"<p><strong>Additional Comments:{5}</strong></p>" +
										"<p><strong>Payment type: {8}</strong></p>" +
										"<p><strong>Number of passengers: {9}</strong></p>" +
										"<p><strong>Number of large luggage: {10}</strong></p>",
										booking.PickUpLocation, booking.DropLocation, booking.PickUpDateTime,
										booking.TransferTime, booking.PhoneNumber, booking.Comments,
										booking.PickUpAddress, booking.DropAddress, paymentType,
										booking.NumberOfPassengers, booking.NumberOfLuggage,
										booking.Name);

			var confirmationLink = string.Format("<h3>You're almost there!<p>&nbsp;</p>Please follow the following link to confirm your order:</h3>{0}?confirmation={1}<p>&nbsp;</p>", localUrl, booking.ConfirmationCode);

			if (isFinal)
			{
				msg.Subject = "Booking information";
				msg.Body = bookingInfo;
			}
			else
			{
				msg.Subject = "VipDriving Order Confirmation";
				msg.Body = confirmationLink + bookingInfo;
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

		public void GetRoutePrice(Booking bookingEntity)
		{
			var predefinedPrices = LoadPricesFromXml();
			var from = bookingEntity.PickUpLocation.ToUpper();
			var to = bookingEntity.DropLocation.ToUpper();
			var price = 0.0;
			foreach (var predefinedPrice in predefinedPrices)
			{
                if (from.Contains(predefinedPrice.From) && to.Contains(predefinedPrice.To) || 
                    from.Contains(predefinedPrice.To) && to.Contains(predefinedPrice.From))
				{
					price = predefinedPrice.Price;
				}
			}
			if (price == 0.0)
			{
				var pricePerKm = 1.80;
				var priceForTransfer = bookingEntity.DrivingDistance*pricePerKm;

				//This calculates price for total driving distance
				//var priceForTransfer = bookingEntity.TotalDrivingDistance*pricePerKm;

				bookingEntity.Price = priceForTransfer;
			}
			else
			{
				bookingEntity.Price = price;
			}
		}

		public bool OrderConfirmation(HttpRequestBase request, BookingStatus bookingStatus, string queryString )
		{
			var queryValues = request.QueryString.Get(queryString);
			Guid securityCode;

			if (Guid.TryParse(queryValues, out securityCode) && securityCode != Guid.Empty)
			{
				using (var db = new DefaultContext())
				{
					var bookingInfo = (from DBbooking in db.Bookings
									   where DBbooking.ConfirmationCode == securityCode
									   select DBbooking).FirstOrDefault();
					if (bookingInfo != null)
					{
						if (bookingInfo.IsSlotAvailable)
						{
							InsertEventToCalendar(bookingInfo);
						}
						if (bookingInfo.BookingStatus == BookingStatus.NotDefined)
						{
							bookingInfo.BookingStatus = bookingStatus;
						}
						if (bookingStatus == BookingStatus.Paid)
						{
							SendEmailViaGmail(bookingInfo, true, null, null);
						}
						SendEmailViaGmail(bookingInfo, true, null, "vipdriving@roshkani.com");
						bookingInfo.ConfirmationCode = Guid.Empty;

						db.SaveChanges();

						return true;
					}
				}
			}
			return false;
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

	    private TimeSpan RoundSeconds(TimeSpan transferTime)
	    {
	        int minutes = (int) transferTime.TotalMinutes;
	        if (transferTime.Seconds >= 30)
	        {
	            minutes ++;
	        }

	        return TimeSpan.FromMinutes(minutes);
	    }
	}
}
