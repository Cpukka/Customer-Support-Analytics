using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomerSupportAPI.Data;
using CustomerSupportAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupportAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { 
                message = "Auth controller is reachable!",
                timestamp = DateTime.UtcNow,
                status = "healthy"
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation($"Registration attempt for email: {request.Email}");

                // Check if user already exists
                var existingUser = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.Email);

                if (existingUser != null)
                {
                    _logger.LogWarning($"User already exists: {request.Email}");
                    return BadRequest(new { error = "User with this email already exists" });
                }

                // Create new user
                var customer = new Customer
                {
                    Name = request.Name,
                    Email = request.Email,
                    Company = "Customer",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User registered successfully: {request.Email}");

                // Generate JWT token
                var token = GenerateJwtToken(customer);

                return Ok(new AuthResponse
                {
                    Token = token,
                    Email = customer.Email,
                    Name = customer.Name,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Registration failed for {request.Email}");
                return StatusCode(500, new { error = "Registration failed. Please try again." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Login attempt for email: {request.Email}");

                // Find user
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.Email);

                if (customer == null)
                {
                    _logger.LogWarning($"User not found: {request.Email}");
                    return Unauthorized(new { error = "Invalid email or password" });
                }

                // For demo purposes, accept any password (in production, verify hashed password)
                if (string.IsNullOrEmpty(request.Password))
                {
                    return Unauthorized(new { error = "Invalid email or password" });
                }

                _logger.LogInformation($"User logged in: {request.Email}");

                // Generate JWT token
                var token = GenerateJwtToken(customer);

                return Ok(new AuthResponse
                {
                    Token = token,
                    Email = customer.Email,
                    Name = customer.Name,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login failed for {request.Email}");
                return StatusCode(500, new { error = "Login failed. Please try again." });
            }
        }

        private string GenerateJwtToken(Customer customer)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secret = _configuration["JwtSettings:Secret"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!1234567890";
                var key = Encoding.ASCII.GetBytes(secret);
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.CustomerID.ToString()),
                    new Claim(ClaimTypes.Email, customer.Email),
                    new Claim(ClaimTypes.Name, customer.Name),
                    new Claim(ClaimTypes.Role, "User")
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(24),
                    Issuer = _configuration["JwtSettings:Issuer"] ?? "CustomerSupportAPI",
                    Audience = _configuration["JwtSettings:Audience"] ?? "CustomerSupportClient",
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token generation failed");
                throw;
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { error = "Not authenticated" });
                }

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == email);

                if (customer == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                return Ok(new
                {
                    customer.CustomerID,
                    customer.Name,
                    customer.Email,
                    customer.Company,
                    customer.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current user");
                return StatusCode(500, new { error = "Failed to get user information" });
            }
        }
    }

    // Request/Response Models
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}