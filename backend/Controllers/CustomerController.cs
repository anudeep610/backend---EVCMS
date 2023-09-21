using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;

using backend.Models;
using backend.Utilities;
using NuGet.Packaging.Signing;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace backend.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly EcmsContext _context;
        private readonly ILogger<CustomerController> _logger;
        public CustomerController(ILogger<CustomerController> logger, EcmsContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("stations")]
        [Authorize]
        
        public async Task<ActionResult<List<Chargingstation>>> Get()
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var stations = await _context.Chargingstations.ToListAsync();
                return Ok(stations);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { type="error" ,message = "An error occurred while fetching charging stations." });
            }
        }

        [HttpGet]
        [Route("station/{id}")]
        [Authorize]
        
        public IActionResult GetSpecificStation(int id) 
        {
            try
            {
                var LoggedInUsername = User.FindFirstValue("email");
                var LoggedInUSerrole = User.FindFirstValue("sub");

                var existingStation = _context.Chargingstations.FirstOrDefault(s => s.Stationid == id);

                return Ok(new { type = "success", message = "successfull", data = existingStation });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { type = "error", message = "Something went wrong" });
            }
        }

        [HttpPost]
        [Route("book")]
        [Authorize]
        
        public IActionResult ReserveChargingPort([FromBody] BookingRequest Booking)
        {
            try
            {
                var LoggedInUsername = User.FindFirstValue("email");
                var LoggedInUSerrole= User.FindFirstValue("sub");

                if (Booking == null || Booking.Starttime == null || Booking.Endtime == null || Booking.Stationid == null)
                {
                    return StatusCode(500, new { type = "error", message = "request not valid" });
                }

                bool isAvailable = checkAvailability(Booking.Stationid, Booking.Starttime, Booking.Endtime);

                if (isAvailable)
                {
                    var station = _context.Chargingstations.FirstOrDefault(s => s.Stationid == Booking.Stationid);
                    var billId = new Random().Next(100000000, 999999999 + 1);
                    TimeSpan difference = Booking.Endtime - Booking.Starttime;
                    var totalCost = ((double)station.Chargingrate) * (difference.TotalDays) * 24;
                    var newBill = new Billing
                    {
                        Stationid = Booking.Stationid,
                        Billingid = billId,
                        Userid = LoggedInUsername,
                        Billingdate = DateTime.UtcNow,
                        Totalamount = (decimal)totalCost,
                        Paymentstatus = "paid",
                    };
                    var resId = new Random().Next(100000000, 999999999 + 1);
                    var newReservation = new Reservation
                    {
                        Starttime = Booking.Starttime,
                        Endtime = Booking.Endtime,
                        Stationid = Booking.Stationid,
                        Reservationid = resId,
                        Userid = LoggedInUsername,

                    };
                    _context.Billings.Add(newBill);
                    _context.Reservations.Add(newReservation);
                    _context.SaveChanges();
                    return Ok(new { type = "success", message = "Booked successfully", id = resId, cost=totalCost });
                }
                else
                {
                    return StatusCode(404, new { type = "error", message = "No port available in the station" });
                }

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { type = "error", message = "Something went wrong" });
            }
        }

        private bool checkAvailability(int stationId, DateTime startTime, DateTime endTime)
        {
            var station = _context.Chargingstations.FirstOrDefault(s => s.Stationid == stationId);

            if (station != null)
            {
                int overlappingReservationsCount = _context.Reservations
                .Count(r =>
                    r.Stationid == stationId &&
                    ((r.Starttime <= startTime && r.Endtime >= startTime) || // Reservation starts within the range
                     (r.Starttime <= endTime && r.Endtime >= endTime) ||     // Reservation ends within the range
                     (r.Starttime >= startTime && r.Endtime <= endTime)));  // Reservation fully overlaps the range

                bool isAvailable = overlappingReservationsCount < station.Ports;

                return isAvailable;
            }
            return false;
        }

        [HttpPost]
        [Route("ports")]
        [Authorize]

        public IActionResult GetPortsAvailableInStation([FromBody] BookingRequest Booking)
        {
            try
            {
                var LoggedInUsername = User.FindFirstValue("email");
                var LoggedInUSerrole = User.FindFirstValue("sub");

                var noOfPortsAvailable = GetPort(Booking.Stationid, Booking.Starttime, Booking.Endtime);

                return Ok(new { type = "success", message = "successfull", ports = noOfPortsAvailable});

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { type = "error", message = "Something went wrong" });
            }
        }

        private int GetPort(int stationId, DateTime startTime, DateTime endTime)
        {
            var station = _context.Chargingstations.FirstOrDefault(s => s.Stationid == stationId);

            if (station != null)
            {
                int overlappingReservationsCount = _context.Reservations
                .Count(r =>
                    r.Stationid == stationId &&
                    ((r.Starttime <= startTime && r.Endtime >= startTime) || // Reservation starts within the range
                     (r.Starttime <= endTime && r.Endtime >= endTime) ||     // Reservation ends within the range
                     (r.Starttime >= startTime && r.Endtime <= endTime)));  // Reservation fully overlaps the range

                return station.Ports - overlappingReservationsCount;
            }
            return 0;
        }
    }
}
