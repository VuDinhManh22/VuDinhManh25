using Microsoft.AspNetCore.Mvc;
using Training.Models;
using Training.Services;
using System.Threading.Tasks;
using Training.Service;

namespace Training.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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

            var token = await _authService.AuthenticateUser(request.Email, request.Password);

            if (token == null)
            {
                return Unauthorized("Email hoặc mật khẩu không đúng."); // 401 Unauthorized
            }

            return Ok(new { Token = token }); // Trả về token
        }
    }
}