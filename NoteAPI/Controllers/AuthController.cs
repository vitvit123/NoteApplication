using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NoteAppApi.Models;
using NoteAppApi.Repositories;
using NoteAppApi.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace NoteAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepo;
        private readonly IConfiguration _config;

        private bool IsStrongPassword(string password, out string error)
        {
            error = "";

            if (password.Length < 8)
            {
                error = "Password must be at least 8 characters long.";
                return false;
            }

            // Require uppercase, lowercase, digit, and special character
            var strongPasswordPattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$";

            if (!Regex.IsMatch(password, strongPasswordPattern))
            {
                error = "Password must include at least one uppercase letter, one lowercase letter, one number, and one special character.";
                return false;
            }

            return true;
        }




        public AuthController(UserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }
        private List<string> ValidateUserRegistration(UserRegisterDto dto)
        {
            var errors = new List<string>();


            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                errors.Add("Username is required.");
            }
            else
            {
                if (dto.Username.Length < 4)
                    errors.Add("Username must be at least 4 characters long.");

                if (dto.Username.Contains(' '))
                    errors.Add("Username cannot contain spaces.");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                errors.Add("Password is required.");
            }
            else if (!IsStrongPassword(dto.Password, out var passwordError))
            {
                errors.Add(passwordError);
            }

            return errors;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            var validationErrors = ValidateUserRegistration(dto);

            if (validationErrors.Any())
            {
                return BadRequest(new { errors = validationErrors });
            }

            var existingUser = await _userRepo.GetByUsername(dto.Username);
            if (existingUser != null)
            {
                return BadRequest(new { errors = new[] { "Username is already taken." } });
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

    
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hashedPassword
            };

            user.Id = await _userRepo.CreateUser(user);

            return Ok(new { message = "User registered successfully." });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var user = await _userRepo.GetByUsername(dto.Username);

            if (user == null)
                return Unauthorized(new { errors = new[] { "Invalid credentials." } });

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                var minutesLeft = (user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
                return Unauthorized(new { errors = new[] { $"Account locked. Try again in {Math.Ceiling(minutesLeft)} minute(s)." } });
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(5);
                    user.FailedLoginAttempts = 0;
                }

                await _userRepo.UpdateUser(user);

                return Unauthorized(new { errors = new[] { "Invalid credentials." } });
            }

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _userRepo.UpdateUser(user);

            var token = GenerateJwtToken(user);
            return Ok(new { token ,
                                userId = user.Id,
                                username = user.Username
                            });
        }


        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
                return BadRequest("Invalid token.");

            // Get token expiry from claims
            var expClaim = User.FindFirst("exp")?.Value;
            if (expClaim == null || !long.TryParse(expClaim, out long expSeconds))
                return BadRequest("Invalid token expiry.");

            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;

            // Add token jti to blacklist until it expires
            TokenBlacklist.Add(jti, expiryDate);

            return Ok(new { message = "Logged out successfully." });
        }


        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            Console.WriteLine(claims);

            var keyString = _config["JwtSettings:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JWT Key is missing in configuration.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var issuer = _config["JwtSettings:Issuer"];
            var audience = _config["JwtSettings:Audience"];
            int expiresInMinutes = 60;
            int.TryParse(_config["JwtSettings:ExpiresInMinutes"], out expiresInMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }

    public class UserRegisterDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserLoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
