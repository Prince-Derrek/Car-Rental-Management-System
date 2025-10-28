using CRMS_API.Domain.DTOs;
using CRMS_API.Services.Exceptions;
using CRMS_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMS_API.Api.Controllers
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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var confirmationLink = await _authService.RegisterAsync(request);
            if (confirmationLink == null)
            {
                return Conflict(new { message = "Registration Failed. A user with this email already exists." });
            }

            return Ok(new
            {
                message = "Registration successful. Please check your email to confirm your account.",
                devConfirmationLink = confirmationLink
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.LoginAsync(request);

                if (result == null)
                {
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                return Ok(result);
            }
            catch (EmailNotConfirmedException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Invalid confirmation request." });
            }

            var success = await _authService.ConfirmEmailAsync(email, token);
            if (!success)
            {
                return BadRequest(new { message = "Invalid email or token." });
            }

            var htmlContent = @"
                <html>
                <body style='font-family: Arial, sans-serif; text-align: center; margin-top: 50px;'>
                    <h1 style='color: #28a745;'>Email Confirmed!</h1>
                    <p>Your email has been successfully verified.</p>
                    <p>You can now <a href='/Auth/Login'>log in</a> to your account.</p>
                </body>
                </html>";
            return Content(htmlContent, "text/html");
        }
        [HttpGet("users")] 
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _authService.GetUsersAsync();
            return Ok(users);
        }

        [HttpPut("users/{id}/toggle-status")] 
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var success = await _authService.ToggleUserStatusAsync(id);
            if (!success)
            {
                return NotFound(new { message = "User not found or cannot be modified." });
            }
            return Ok(new { message = "User status updated successfully." });
        }
    }
}