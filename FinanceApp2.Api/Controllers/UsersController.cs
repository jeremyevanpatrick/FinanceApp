using FinanceApp2.Api.Errors;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Extensions;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBaseExtended
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IAuthAppService _authAppService;

        public UsersController(
            ILogger<UsersController> logger,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _authAppService = authAppService;
        }

        [EnableRateLimiting("public-messaging-endpoints")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                await _authAppService.RegisterAsync(request.Email, request.Password);
                return Created();
            }
            catch (AuthException ex)
            {
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.RegisterUnexpected, ex, "Unexpected error during user registration", new Dictionary<string, string> { });
                return Problem500();
            }
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            string userId = string.Empty;

            try
            {
                userId = User.GetUserId().ToString();
                await _authAppService.DeleteAccountAsync(userId, request.Password);
                return NoContent();
            }
            catch (AuthException ex)
            {
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.DeleteAccountUnexpected, ex, "Unexpected error while deleting account", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem500();
            }
        }

        [HttpPatch("me/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            string userId = string.Empty;

            try
            {
                userId = User.GetUserId().ToString();
                await _authAppService.ChangePasswordAsync(userId, request.ExistingPassword, request.NewPassword);
                return NoContent();
            }
            catch (AuthException ex)
            {
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ChangePasswordUnexpected, ex, "Unexpected error while changing password", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem500();
            }
        }

        [HttpPatch("me/email")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
        {
            string userId = string.Empty;

            try
            {
                userId = User.GetUserId().ToString();
                await _authAppService.ChangeEmailAsync(userId, request.NewEmail, request.Password);
                return NoContent();
            }
            catch (AuthException ex)
            {
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ChangeEmailUnexpected, ex, "Unexpected error while sending change email message", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem500();
            }
        }
    }
}