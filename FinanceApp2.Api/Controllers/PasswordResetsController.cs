using FinanceApp2.Api.Errors;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class PasswordResetsController : ControllerBaseExtended
    {
        private readonly ILogger<PasswordResetsController> _logger;
        private readonly IAuthAppService _authAppService;

        public PasswordResetsController(
            ILogger<PasswordResetsController> logger,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _authAppService = authAppService;
        }

        [EnableRateLimiting("public-messaging-endpoints")]
        [HttpPost("password-reset-requests")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authAppService.ForgotPasswordAsync(request.Email);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ForgotPasswordUnexpected, ex, "Unexpected error while sending reset password email", new Dictionary<string, string> { });
                return NoContent();
            }
        }

        [HttpPost("password-resets")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _authAppService.ResetPasswordAsync(request.Email, request.ResetCode, request.NewPassword);
                return NoContent();
            }
            catch (AuthException ex)
            {
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ResetPasswordUnexpected, ex, "Unexpected error while resetting password", new Dictionary<string, string> { });
                return Problem500();
            }
        }

    }
}