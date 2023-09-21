using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.AspNetCore.Cors;

using backend.Models;
using backend.Utilities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace backend.Controllers
{

    [Route("api")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        IConfiguration configuration;

        private readonly ILogger<CustomerController> _logger;
        private readonly EcmsContext _context;

        public AuthController(IConfiguration configuration, EcmsContext context, ILogger<CustomerController> logger)
        {
            this.configuration = configuration;
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] AuthenticationRequest user)
        {
            IActionResult response = Unauthorized();

            try
            {
                if (user != null)
                {
                    var userFromDb = _context.Users.SingleOrDefault(u => u.Username == user.Username);

                    if (userFromDb == null)
                    {
                        return StatusCode(500, new { type = "error", message = "user is not found" });
                    }
                    if (VerifyPassword(userFromDb.Passwordhash, user.Password))
                    {
                        var issuer = configuration["Jwt:Issuer"];
                        var audience = configuration["Jwt:Audience"];
                        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
                        var signingCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(key),
                            SecurityAlgorithms.HmacSha512Signature
                        );

                        var subject = new ClaimsIdentity(new[]
                        {
                        new Claim(JwtRegisteredClaimNames.Sub, userFromDb.Role),
                        new Claim(JwtRegisteredClaimNames.Email, userFromDb.Userid),
                        new Claim(ClaimTypes.Role, userFromDb.Role)
                    });

                        var expires = DateTime.UtcNow.AddMinutes(10);

                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = subject,
                            Expires = expires,
                            Issuer = issuer,
                            Audience = audience,
                            SigningCredentials = signingCredentials
                        };

                        var tokenHandler = new JwtSecurityTokenHandler();
                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        var jwtToken = tokenHandler.WriteToken(token);

                        return Ok(new { type = "success", message = "successfull", jwtToken = jwtToken, userrole = userFromDb.Role, name = userFromDb.Username, userid = userFromDb.Userid });
                    }
                    else
                    {
                        return StatusCode(500, new { type = "error", message = "wrong password" });
                    }
                }

                return StatusCode(500, new { type = "error", message = "request is not valid" });
            }catch(Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { message = ex.ToString(), type = "error" });
            }
        }
        private bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
        }
        private string EncryptPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromBody] UserRegistrationRequest user)
        {
            if(user == null || user.Role == null || (user.Role != "admin" && user.Role != "normal") || user.Username == null || user.Email == null || user.Password == null)
            {
                return StatusCode(500, new { type = "error", message = "request is not valid" });
            }

            try
            {
                string encryptedPassword = EncryptPassword(user.Password);
                string userID = Guid.NewGuid().ToString();
                var newUser = new User
                {
                    Username = user.Username,
                    Email = user.Email,
                    Passwordhash = encryptedPassword,
                    Role = user.Role,
                    Userid = userID
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();
                return Ok(new { type = "success" });
            }catch (Exception ex)
            {
                _logger.LogError(ex, "A error occured");
                return StatusCode(500, new { message = ex.ToString(), type = "error" });
            }


        }
    }
}
