using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace AssetManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // 🔹 LOGIN (Identity)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Ok(new { message = "Login successful", username = user.UserName });
            }

            return Unauthorized(new { message = "Invalid username or password." });
        }

        // 🔹 LOGOUT (Identity)
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logout successful" });
        }

        // 🔹 STATUS (Check if cookie is still valid)
        // 🔹 STATUS (Check if cookie is still valid)
        [HttpGet("status")]
        [Produces("application/json")] // ✅ Force JSON content type
        public IActionResult GetAuthStatus()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated == true;
            var username = isAuthenticated ? User.Identity?.Name : null;

            var response = new
            {
                isAuthenticated,
                username
            };

            return new JsonResult(response) // ✅ Explicitly return JSON
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = "application/json"
            };
        }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
