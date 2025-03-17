using CustomerService.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly CustomerRepository _customerRepository;

        public AuthController(AuthService authService, CustomerRepository customerRepository)
        {
            _authService = authService;
            _customerRepository = customerRepository;
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (token, customerId) = await _authService.LoginAsync(request.Username, request.Password);
                return Ok(new { Token = token, CustomerId = customerId });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authService.SendPasswordResetLinkAsync(request.Email);
                return Ok("Password reset link sent.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public class LoginRequest
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = null!;
        }
    }
}
