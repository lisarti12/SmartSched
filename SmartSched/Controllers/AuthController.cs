using Microsoft.AspNetCore.Mvc;
using SmartSched.Data;
using SmartSched.DTOs;
using SmartSched.Models;
using BCrypt.Net;

namespace SmartSched.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(dto.FullName))
                errors["fullName"] = "Name required";

            if (!dto.Email.Contains("@"))
                errors["email"] = "Invalid email";

            if (dto.Password.Length < 6)
                errors["password"] = "Too short";

            if (errors.Count > 0)
                return BadRequest(new { errors });

            if (_db.Users.Any(x => x.Email == dto.Email))
                return BadRequest(new { message = "Email already exists" });

            // ROLE LOGIC 👇
            var role = dto.Role;

            if (role != "Student" && role != "Employer")
                role = "Student";

            // YOUR ACCOUNT ALWAYS ADMIN
            if (dto.Email.ToLower() == "lisart.mella@gmail.com")
                role = "Admin";

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = role,
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok(new { message = "Registered" });
        }



        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = _db.Users
                .FirstOrDefault(x => x.Email == dto.Email);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return BadRequest(new
                {
                    message = "Invalid credentials"
                });
            }

            return Ok(new
            {
                role = user.Role,
                name = user.FullName
            });
        }


    }
}
