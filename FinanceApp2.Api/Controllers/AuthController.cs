using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Models;
using FinanceApp2.Api.Services;
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
using System.Security.Cryptography;
using System.Text;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBaseExtended
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthDbService _authDbService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ClientSettings _clientSettings;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailSender _emailSender;

        public AuthController(
            ILogger<AuthController> logger,
            IAuthDbService authDbService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<ClientSettings> clientSettings,
            IOptions<JwtSettings> jwtSettings,
            IEmailSender emailSender)
        {
            _logger = logger;
            _authDbService = authDbService;
            _userManager = userManager;
            _signInManager = signInManager;
            _clientSettings = clientSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _emailSender = emailSender;
        }

        [EnableRateLimiting("public-messaging-endpoints")]
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
                    if (result.Errors.Where(e => e.Code.StartsWith("Password")).Any())
                    {
                        return Problem400("Password does not meet requirements.", ResponseErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS);
                    }
                    return Ok();
                }

                await SendEmailConfirmationEmail(user);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.RegisterUnexpected, ex, "Unexpected error during user registration", new Dictionary<string, string> { });
                return Problem("An unexpected error occurred. Please try again later.");
            }
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.ResendConfirmationEmailUnexpected, ex, "Unexpected error while resending confirmation email", new Dictionary<string, string> { });
                return Ok();
            }
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
                    return Problem400("Invalid or expired confirmation link.", ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }

                string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

                IdentityResult result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (!result.Succeeded)
                {
                    return Problem400("Invalid or expired confirmation link.", ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ConfirmEmailUnexpected, ex, "Unexpected error while confirming email", new Dictionary<string, string> { });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Problem401("Invalid credentials", ResponseErrorCodes.INVALID_CREDENTIALS);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                        var minutesRemaining = (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes;
                        return Problem401($"Account is locked. Please try again in {minutesRemaining} minutes.", ResponseErrorCodes.ACCOUNT_LOCKED);
                    }
                    return Problem401("Invalid credentials", ResponseErrorCodes.INVALID_CREDENTIALS);
                }

                var refreshTokenString = GenerateRefreshToken();
                await _authDbService.AddRefreshTokenAsync(new RefreshToken
                {
                    TokenHash = TokenHashHelper.Hash(refreshTokenString),
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow
                });

                Response.Cookies.Append("refresh_token", refreshTokenString, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(30)
                });

                var accessToken = GenerateJwtToken(user);

                return Ok(new AuthResponse
                {
                    AccessToken = accessToken,
                    UserId = user.Id,
                    Email = user.Email,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes)
                });
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.LoginUnexpected, ex, "Unexpected error during user login", new Dictionary<string, string> { });
                return Problem("An unexpected error occurred. Please try again later.");
            }
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.ForgotPasswordUnexpected, ex, "Unexpected error while sending reset password email", new Dictionary<string, string> { });
                return Ok();
            }
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Problem400("Invalid or expired reset link.", ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }

                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.ResetCode));

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
                if (!result.Succeeded)
                {
                    if (result.Errors.Where(e => e.Code.StartsWith("Password")).Any())
                    {
                        return Problem400("Password does not meet requirements.", ResponseErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS);
                    }

                    return Problem400("Invalid or expired reset link.", ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ResetPasswordUnexpected, ex, "Unexpected error while resetting password", new Dictionary<string, string> { });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            string userId = string.Empty;

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Problem401("Authentication is no longer valid.", ResponseErrorCodes.AUTH_NO_LONGER_VALID);
                }

                userId = user.Id;

                IdentityResult result = await _userManager.ChangePasswordAsync(user, request.ExistingPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
                    {
                        return Problem400("Invalid credentials", ResponseErrorCodes.INVALID_CREDENTIALS);
                    }

                    if (result.Errors.Where(e => e.Code.StartsWith("Password")).Any())
                    {
                        return Problem400("New password does not meet requirements.", ResponseErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS);
                    }

                    _logger.LogErrorWithDictionary(AuthErrorCodes.ChangePasswordFailed, null, "Change password failed unexpectedly", new Dictionary<string, string> {
                        { "Description", string.Join(", ", result.Errors.Select(e => e.Description)) }
                    });
                    return Problem("Change password failed unexpectedly. Please try again later.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ChangePasswordUnexpected, ex, "Unexpected error while changing password", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("changeemail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
        {
            string userId = string.Empty;

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Problem401("Authentication is no longer valid.", ResponseErrorCodes.AUTH_NO_LONGER_VALID);
                }

                userId = user.Id;

                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    return Problem400("Invalid credentials", ResponseErrorCodes.INVALID_CREDENTIALS);
                }

                var existingUser = await _userManager.FindByEmailAsync(request.NewEmail);
                if (existingUser != null)
                {
                    return Problem400("Email address is already in use.", ResponseErrorCodes.EMAIL_ADDRESS_ALREADY_IN_USE);
                }

                string token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var confirmationUrl = $"{_clientSettings.Host}/confirmemailchange?userid={Uri.EscapeDataString(user.Id)}&email={Uri.EscapeDataString(request.NewEmail)}&token={Uri.EscapeDataString(encodedToken)}";
                await _emailSender.SendEmailAsync(request.NewEmail, "Confirm your new email", $"Please confirm your new email by <a href='{confirmationUrl}'>clicking here</a>.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ChangeEmailUnexpected, ex, "Unexpected error while sending change email message", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("changeemailconfirmation")]
        public async Task<IActionResult> ChangeEmailConfirmation([FromBody] ChangeEmailConfirmationRequest request)
        {
            string userId = string.Empty;

            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Problem401("Authentication is no longer valid.", ResponseErrorCodes.AUTH_NO_LONGER_VALID);
                }

                userId = user.Id;

                string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

                var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, decodedToken);
                if (!result.Succeeded)
                {
                    return Problem400("Invalid or expired change email link.", ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ChangeEmailConfirmationUnexpected, ex, "Unexpected error while confirming email change", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            string userId = string.Empty;

            try
            {
                var refreshTokenString = Request.Cookies["refresh_token"];
                if (string.IsNullOrWhiteSpace(refreshTokenString))
                {
                    return Problem401("Authentication is no longer valid.", ResponseErrorCodes.AUTH_NO_LONGER_VALID);
                }

                RefreshToken? refreshToken = await _authDbService.GetRefreshTokenAsync(TokenHashHelper.Hash(refreshTokenString));
                if (refreshToken == null)
                {
                    return Problem401("Authentication is no longer valid.", ResponseErrorCodes.AUTH_NO_LONGER_VALID);
                }

                userId = refreshToken.UserId;
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Problem401("Authentication is no longer valid.", ResponseErrorCodes.AUTH_NO_LONGER_VALID);
                }

                var newRefreshTokenString = GenerateRefreshToken();
                await _authDbService.AddRefreshTokenAsync(new RefreshToken
                {
                    TokenHash = TokenHashHelper.Hash(newRefreshTokenString),
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow
                });
                await _authDbService.RevokeRefreshTokenAsync(TokenHashHelper.Hash(refreshTokenString));

                Response.Cookies.Append("refresh_token", newRefreshTokenString, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(30)
                });

                var accessToken = GenerateJwtToken(user);

                return Ok(new AuthResponse
                {
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes)
                });
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.RefreshTokenUnexpected, ex, "Unexpected error while refreshing token", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshTokenString = Request.Cookies["refresh_token"];
                if (string.IsNullOrWhiteSpace(refreshTokenString))
                {
                    return Ok();
                }

                await _authDbService.RevokeRefreshTokenAsync(TokenHashHelper.Hash(refreshTokenString));
                Response.Cookies.Delete("refresh_token");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.LogoutUnexpected, ex, "Unexpected error while signing out", new Dictionary<string, string> { });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            string userId = string.Empty;

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Problem401("Authentication is no longer valid.", ResponseErrorCodes.AUTH_NO_LONGER_VALID);
                }

                userId = user.Id;

                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    return Problem400("Invalid credentials", ResponseErrorCodes.INVALID_CREDENTIALS);
                }

                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogErrorWithDictionary(AuthErrorCodes.DeleteAccountFailed, null, "Delete account failed unexpectedly", new Dictionary<string, string> {
                        { "Description", string.Join(", ", result.Errors.Select(e => e.Description)) }
                    });
                    return Problem("Delete account failed unexpectedly. Please try again later.");
                }

                await _authDbService.RevokeAllUserRefreshTokensAsync(userId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.DeleteAccountUnexpected, ex, "Unexpected error while deleting account", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
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

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

    }
}