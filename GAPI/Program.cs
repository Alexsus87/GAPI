using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using API_Functions;

namespace GAPI
{
	class Program
	{

		static void Main(string[] args)
		{
			ApiMethods apiMethods = new ApiMethods();

			var distance = apiMethods.GetDrivingDistanceInKilometers("Kiev", "Moscow");
			Console.WriteLine(distance);

			DateTime StartEventTime = new DateTime(2016, 01, 22, 12, 00, 00);
			DateTime EndEventTime = new DateTime(2016, 01, 22, 15, 01, 00);

			Events events = apiMethods.GetEventsFromCalendar(StartEventTime, EndEventTime);

			if (events.Items.Count == 0)
			{
				Console.WriteLine("No events found");
			}
			else
			{
				Console.WriteLine("Events overlap. Event start: {0}", events.Items[0].Start.DateTime.Value.ToString());
				TimeSpan a = EndEventTime - events.Items[0].Start.DateTime.Value;

				StartEventTime.Subtract(a);
				EndEventTime.Subtract(a);
				Events events2 = apiMethods.GetEventsFromCalendar(StartEventTime, EndEventTime);


			}
			Console.ReadKey();
		}
	}
}
