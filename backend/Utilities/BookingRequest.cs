// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace backend.Utilities
{
    public class BookingRequest
    {
        public int Stationid { get; set; }

        public DateTime Starttime { get; set; }

        public DateTime Endtime { get; set; }
    }
}
