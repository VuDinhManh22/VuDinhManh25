using Microsoft.AspNetCore.Mvc;
using Training.Models;
using Training.Services;
using System.Threading.Tasks;
using Training.Service;
using Microsoft.Extensions.Logging;

namespace Training.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role // Role có thể được đặt từ request hoặc mặc định trong service
            };

            var registeredUser = await _authService.RegisterUser(user, request.Password);

            if (registeredUser == null)
            {
                return BadRequest("Email đã được đăng ký.");
            }

            return StatusCode(201, new { Message = "Đăng ký thành công." });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tokens = await _authService.AuthenticateUser(request.Email, request.Password);

            if (tokens.JwtToken == null)
            {
                _logger.LogWarning("Login failed for user: {Email}", request.Email);
                return Unauthorized("Email hoặc mật khẩu không đúng.");
            }

            // Thêm dòng log này để ghi lại khi đăng nhập thành công
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

           return Ok(new { JwtToken = tokens.JwtToken, RefreshToken = tokens.RefreshToken });
        }
       
       [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Gọi AuthService để làm mới token
            var tokens = await _authService.Refresh(request.RefreshToken);

            if (tokens.JwtToken == null)
            {
                _logger.LogWarning("Invalid or expired refresh token used.");
                return Unauthorized("Token làm mới không hợp lệ hoặc đã hết hạn.");
            }

            _logger.LogInformation("Token refreshed successfully.");

            return Ok(new { JwtToken = tokens.JwtToken, RefreshToken = tokens.RefreshToken });
        }
    }
}