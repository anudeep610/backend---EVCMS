using backend.Models;
using backend.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace backend.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly EcmsContext _context;

        public AdminController(EcmsContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("stations")]
        [Authorize]
        public IActionResult AllStationsUnderId([FromQuery]string id)
        {
            try
            {
                var LoggedInUsername = User.FindFirstValue("email");
                var LoggedInUSerrole = User.FindFirstValue("sub");

                var stations = _context.Chargingstations.Where(s => s.Userid == id).OrderBy(s => s.Location).ToList();

                return Ok(stations);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { type = "error", message = "Something went wrong" });
            }

        }

        [HttpGet]
        [Route("station/get")]
        [Authorize]
        public IActionResult GetSpecificStation([FromQuery] int stationId)
        {
            try
            {
                var existingStation = _context.Chargingstations.FirstOrDefault(s => s.Stationid == stationId);

                if (existingStation == null)
                {
                    return NotFound(new { type = "error", message = "Station not found" });
                }

                var LoggedInUsername = User.FindFirstValue("email");
                var LoggedInUSerrole = User.FindFirstValue("sub");

                if (LoggedInUSerrole != "admin" || LoggedInUsername != existingStation.Userid)
                {
                    return Unauthorized(new { type = "error", message = "not authorized to access this page" });
                }

                DateTime currentDate = DateTime.UtcNow;
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve // Handle circular references
                };
                var reservations = _context.Reservations.Where(r => r.Stationid == 942775528 && r.Starttime > currentDate)
                                    .OrderBy(r => r.Starttime)
                                    .ToList();
                var jsonResult = JsonSerializer.Serialize(reservations, options);

                var totalRevenue = _context.Billings.Where(b => b.Stationid == stationId).Sum(b => b.Totalamount);
                
                DateTime futureDate = currentDate.AddDays(5);
                var upcomingReservationsCountByDate = Enumerable.Range(0, (int)(futureDate - currentDate).TotalDays)
                     .Select(offset => currentDate.Date.AddDays(offset))
                        .Select(date => new
                        {
                            ReservationDate = date,
                            ReservationCount = _context.Reservations
                                .Count(r => r.Stationid == stationId && r.Starttime.Date == date)
                        })
                        .ToList();

                var EmissionReducedByDate = Enumerable.Range(0, (int)(futureDate - currentDate).TotalDays)
                     .Select(offset => currentDate.Date.AddDays(offset))
                        .Select(date => new
                        {
                            ReservationDate = date,
                            EmissionReduced = _context.Reservations
                                            .Where(r => r.Stationid == stationId && r.Starttime.Date == date.Date)
                                            .Sum(record => (record.Endtime - record.Starttime).TotalMinutes / 60 * 10)

                        })
                        .ToList();

                return Ok(new { type = "success", message = "successfull", revenue = totalRevenue, graphData1 = upcomingReservationsCountByDate, graphData2 = EmissionReducedByDate });


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { type = "error", message = "Something went wrong" });
            }
        }

        [HttpPost]
        [Route("station/create")]
        [Authorize]
        public IActionResult CreateStation([FromBody] StationRequest Station)
        {

            try
            {
                var LoggedInUsername = User.FindFirstValue("email");
                var LoggedInUSerrole = User.FindFirstValue("sub");

                if (LoggedInUSerrole != "admin")
                {
                    return Unauthorized(new { type = "error", message = "not authorized to access this page" });
                }

                if (Station == null || Station.Location == null || Station.Ports == null || Station.Chargingrate == null || Station.Availability == null)
                {
                    return StatusCode(500, new { type = "error", message = "request not valid" });
                }

                var user = _context.Users.Find(LoggedInUsername);
                Console.WriteLine(user.Role);
                var newStation = new Chargingstation
                {
                    Userid = LoggedInUsername,
                    Ports = Station.Ports,
                    Availability = Station.Availability,
                    Location = Station.Location,
                    Stationid = new Random().Next(100000000, 999999999 + 1),
                    Chargingrate = Station.Chargingrate,
                    User = user
                };

                _context.Chargingstations.Add(newStation);
                _context.SaveChanges();

                return Ok(new { type="success", message = "Station successfully created"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { type = "error", message = "Something went wrong" });
            }
        }


        [HttpPut("station/update/{id}")]
        [Authorize]
        public IActionResult UpdateStation(int id, [FromBody] StationRequest updatedStationData)
        {
            try
            {
                var existingStation = _context.Chargingstations.FirstOrDefault(s => s.Stationid == id);

                if (existingStation == null)
                {
                    return NotFound(new { type = "error", message = "Station not found" });
                }

                var LoggedInUsername = User.FindFirstValue("email");
                var LoggedInUSerrole = User.FindFirstValue("sub");

                if (LoggedInUSerrole != "admin" || LoggedInUsername != existingStation.Userid)
                {
                    return Unauthorized(new { type = "error", message = "not authorized to access this page" });
                }

                existingStation.Location = updatedStationData.Location;
                existingStation.Availability = updatedStationData.Availability;
                existingStation.Chargingrate = updatedStationData.Chargingrate;
                existingStation.Ports = updatedStationData.Ports;

                _context.SaveChanges();

                return Ok(new { type = "success", message = "Station updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the station");
                return StatusCode(500, new { type = "error", message = "Something went wrong" });
            }
        }


        [HttpDelete("station/delete/{id}")]
        [Authorize]
        public IActionResult DeleteStation(int id)
        {
            try
            {
                var existingStation = _context.Chargingstations.FirstOrDefault(s => s.Stationid == id);

                if (existingStation == null)
                {
                    return NotFound(new { type = "error", message = "Station not found" });
                }

                var LoggedInUsername = User.FindFirstValue("email");
                var LoggedInUSerrole = User.FindFirstValue("sub");

                if (LoggedInUSerrole != "admin" || LoggedInUsername != existingStation.Userid)
                {
                    return Unauthorized(new { type = "error", message = "not authorized to access this page" });
                }
                
                _context.Remove(existingStation);

                _context.SaveChanges();

                return Ok(new { type = "success", message = "Station deleted successfully" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { type = "error", message = "Something went wrong" });
            }
        }
    }
}
