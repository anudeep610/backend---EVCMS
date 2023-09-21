using Microsoft.Build.Framework;

namespace backend.Utilities
{
    public class StationRequest
    {
        public int Stationid { get; set; }

        public string Location { get; set; }

        public int Ports { get; set; }

        public decimal Chargingrate { get; set; }

        public string Availability { get; set; } 

    }
}
