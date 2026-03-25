using FinanceApp2.Api.Errors;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class EmailConfirmationsController : ControllerBaseExtended
    {
        private readonly ILogger<EmailConfirmationsController> _logger;
        private readonly IAuthAppService _authAppService;

        public EmailConfirmationsController(
            ILogger<EmailConfirmationsController> logger,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _authAppService = authAppService;
        }

        [EnableRateLimiting("public-messaging-endpoints")]
        [HttpPost("email-confirmations")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
        {
            try
            {
                await _authAppService.ResendConfirmationEmailAsync(request.Email);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ResendConfirmationEmailUnexpected, ex, "Unexpected error while resending confirmation email", new Dictionary<string, string> { });
                return NoContent();
            }
        }

        [HttpPost("email-confirmations/confirm")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            try
            {
                await _authAppService.ConfirmEmailAsync(request.UserId, request.Token);
                return NoContent();
            }
            catch (AuthException ex)
            {
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ConfirmEmailUnexpected, ex, "Unexpected error while confirming email", new Dictionary<string, string> { });
                return Problem500();
            }
        }

        [HttpPost("email-change-confirmations")]
        public async Task<IActionResult> ChangeEmailConfirmation([FromBody] ChangeEmailConfirmationRequest request)
        {
            try
            {
                await _authAppService.ChangeEmailConfirmationAsync(request.UserId, request.NewEmail, request.Token);
                return NoContent();
            }
            catch (AuthException ex)
            {
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ChangeEmailConfirmationUnexpected, ex, "Unexpected error while confirming email change", new Dictionary<string, string>
                {
                    { "UserId", request.UserId }
                });
                return Problem500();
            }
        }

    }
}