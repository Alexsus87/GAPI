using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BritishCab
{
	public class DistanceMatrix
	{
		public double TravelTime { get; set; }
		public double TotalTravelTime { get; set; }
		public double HomeToOriginTime { get; set; }
		public double DestinationToHomeTime { get; set; }
		public double TravelDistance { get; set; }
		public double TotalTravelDistance { get; set; }
		public double HomeToOriginDistance { get; set; }
		public double DestinationToHomeDistance { get; set; }
		public string OriginAddress { get; set; }
		public string DestinationAddress { get; set; }
		public bool ErrorBit { get; set; }

	}
}
