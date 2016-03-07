using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using API_Functions.Models;

namespace BritishCab
{
	public class ApiMethods
	{
		private string[] Scopes = { CalendarService.Scope.Calendar };

		private string applicationName = "Google Calendar API .NET Quickstart";

		private UserCredential credential;

		private CalendarService service;

		public ApiMethods()
		{
			using (var stream =
				new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
			{
				string credPath = System.Environment.GetFolderPath(
					System.Environment.SpecialFolder.Personal);
				credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart");

				credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					Scopes,
					"user",
					CancellationToken.None,
					new FileDataStore(credPath, true)).Result;
				Console.WriteLine("Credential file saved to: " + credPath);
			}

			// Create Google Calendar API service.
			service = new CalendarService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = applicationName,
			});
		}

		/// <summary>
		/// returns driving distance in kilometers
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public DistanceMatrix GetDrivingDistanceInKilometers(string origin, string destination)
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
			DistanceMatrix dm = new DistanceMatrix();
			xmldoc.LoadXml(responsereader);


			if (xmldoc.GetElementsByTagName("status")[0].ChildNodes[0].InnerText == "OK")
			{
				try
				{
					XmlNodeList distance = xmldoc.GetElementsByTagName("distance");
					XmlNodeList drivingTime = xmldoc.GetElementsByTagName("duration");
	
					dm.TravelTime = Convert.ToDouble(drivingTime[0].ChildNodes[0].InnerText);
					dm.TravelDistance = Convert.ToInt32(distance[0].ChildNodes[1].InnerText.Replace(" km", ""));
					dm.ErrorBit = false;
					return dm;
				}
				catch (Exception)
				{

					dm.ErrorBit = true;
					return dm;
				}
				
			}

			return null;
		}

		public void InsertEventToCalendar(string eventTitle, string pickUpCity, DateTime pickUpDateTime, DateTime estimateDropDateTime)
		{
			//Inserting event to calendar
			Event event1 = new Event()
			{
				Summary = eventTitle,
				Location = pickUpCity,
				Start = new EventDateTime()
				{
					DateTime = pickUpDateTime,
					//DateTime = new DateTime(2015, 12, 11, 14, 15, 0),
					TimeZone = "Europe/London"
				},
				End = new EventDateTime()
				{
					DateTime = estimateDropDateTime,
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

		public Events GetEventsFromCalendar(DateTime TimeMin, DateTime TimeMax)
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

		public bool SendEmailViaGmail(string emailTo)
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
			msg.To.Add(new MailAddress(emailTo));

			msg.Subject = "This is a test Email subject";
			msg.IsBodyHtml = true;
			msg.Body = string.Format("<html><head></head><body><b>Test HTML Email</b></body>");

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
	}
}
