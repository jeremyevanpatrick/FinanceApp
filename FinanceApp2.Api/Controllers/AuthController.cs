using FinanceApp2.Api.Models;
using FinanceApp2.Api.Settings;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.Requests;
using FinanceApp2.Shared.Services.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ClientSettings _clientSettings;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailSender _emailSender;

        public AuthController(ILogger<AuthController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<ClientSettings> clientSettings, IOptions<JwtSettings> jwtSettings, IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _clientSettings = clientSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email
                };

                IdentityResult result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    _logger.LogErrorWithDictionary("FI2.A-00009", null, "Registration failed", new Dictionary<string, string> { });
                    return BadRequest(new { message = "Registration failed" });
                }
                else
                {
                    await SendEmailConfirmationEmail(user);

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00008", ex, "Registration failed", new Dictionary<string, string> { });
            }

            return Problem("Registration failed");
        }

        [EnableRateLimiting("public-messaging-endpoints")]
        [HttpPost("resendconfirmationemail")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Ok();
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Ok();
                }

                if (user.EmailConfirmed)
                {
                    return Ok();
                }

                await SendEmailConfirmationEmail(user);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00010", ex, "Resend confirmation email failed", new Dictionary<string, string> { });
            }

            return Ok();
        }

        private async Task SendEmailConfirmationEmail(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = $"{_clientSettings.Host}/confirmemail?userid={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(encodedToken)}";
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Please confirm your account by <a href='{confirmationUrl}'>clicking here</a>.");
        }

        [HttpPost("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Problem("Unable to confirm email");
                }

                string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

                IdentityResult result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00011", ex, "Email confirmation failed", new Dictionary<string, string> { });
            }

            return Problem("Unable to confirm email");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    var token = GenerateJwtToken(user);

                    return Ok(new AuthResponse
                    {
                        Token = token,
                        UserId = user.Id,
                        Email = user.Email,
                        ExpiresAt = DateTime.UtcNow.AddDays(30)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00012", ex, "Unexpected error during login", new Dictionary<string, string> { });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }

        [EnableRateLimiting("public-messaging-endpoints")]
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    return Ok();
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var resetUrl = $"{_clientSettings.Host}/resetpassword?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(encodedToken)}";
                await _emailSender.SendEmailAsync(request.Email, "Reset your password", $"Reset your password by <a href='{resetUrl}'>clicking here</a>");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00020", ex, "Unexpected error changing password", new Dictionary<string, string> { });
            }

            return Ok();
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Ok();
                }

                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.ResetCode));

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
                if (!result.Succeeded)
                {
                    _logger.LogErrorWithDictionary("FI2.A-00022", null, "Reset password failed", new Dictionary<string, string> {
                        { "Description", string.Join(", ", result.Errors.Select(e => e.Description)) }
                    });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00021", ex, "Unexpected error resetting password", new Dictionary<string, string> { });
            }

            return Problem("Reset password failed");
        }

        [HttpPost("changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                IdentityResult result = await _userManager.ChangePasswordAsync(user, request.ExistingPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    _logger.LogErrorWithDictionary("FI2.A-00014", null, "Change password failed", new Dictionary<string, string> {
                        { "Description", string.Join(", ", result.Errors.Select(e => e.Description)) }
                    });
                    return BadRequest(new { message = "Change password failed" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00013", ex, "Unexpected error changing password", new Dictionary<string, string> { });
            }

            return Problem("Unable to change password");
        }

        [HttpPost("changeemail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    return BadRequest(new { message = "Invalid credentials" });
                }

                string token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var confirmationUrl = $"{_clientSettings.Host}/confirmemailchange?userid={Uri.EscapeDataString(user.Id)}&email={Uri.EscapeDataString(request.NewEmail)}&token={Uri.EscapeDataString(encodedToken)}";
                await _emailSender.SendEmailAsync(request.NewEmail, "Confirm your new email", $"Please confirm your new email by <a href='{confirmationUrl}'>clicking here</a>.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00015", ex, "Unexpected error changing email", new Dictionary<string, string> { });
            }

            return Problem("Unable to change email");
        }

        [HttpPost("changeemailconfirmation")]
        public async Task<IActionResult> ChangeEmailConfirmation([FromBody] ChangeEmailConfirmationRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return BadRequest(new { message = "Confirm email change failed" });
                }

                string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

                var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, decodedToken);
                if (!result.Succeeded)
                {
                    _logger.LogErrorWithDictionary("FI2.A-00017", null, "Confirm email change failed", new Dictionary<string, string> {
                        { "Description", string.Join(", ", result.Errors.Select(e => e.Description)) }
                    });
                    return BadRequest(new { message = "Confirm email change failed" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00016", ex, "Unexpected error confirming email change", new Dictionary<string, string> { });
            }

            return Problem("Confirm email change failed");
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Unauthorized();
                }

                var token = GenerateJwtToken(user);

                return Ok(new AuthResponse
                {
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    ExpiresAt = DateTime.UtcNow.AddDays(30)
                });
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00024", ex, "Unexpected error refreshing token", new Dictionary<string, string> { });
            }

            return Problem("Refresh token failed");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00023", ex, "Unexpected error signing out", new Dictionary<string, string> { });
            }

            return Problem("Logout failed");
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    return BadRequest(new { message = "Invalid credentials" });
                }

                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogErrorWithDictionary("FI2.A-00019", null, "Delete account failed", new Dictionary<string, string> {
                        { "Description", string.Join(", ", result.Errors.Select(e => e.Description)) }
                    });
                    return BadRequest(new { message = "Delete account failed" });
                }

                await _userManager.UpdateSecurityStampAsync(user);
                await _signInManager.SignOutAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.A-00018", ex, "Unexpected error deleting account", new Dictionary<string, string> { });
            }

            return Problem("Delete account failed");
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
