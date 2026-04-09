using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Extensions;
using FinanceApp2.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("password-reset-requests")]
    public class PasswordResetRequestsController : ControllerBaseExtended
    {
        private readonly ILogger<PasswordResetRequestsController> _logger;
        private readonly SessionsLinkHelper _sessionsLinkHelper;
        private readonly PasswordResetRequestsLinkHelper _passwordResetRequestsLinkHelper;
        private readonly IAuthAppService _authAppService;

        public PasswordResetRequestsController(
            ILogger<PasswordResetRequestsController> logger,
            SessionsLinkHelper sessionsLinkHelper,
            PasswordResetRequestsLinkHelper passwordResetRequestsLinkHelper,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _sessionsLinkHelper = sessionsLinkHelper;
            _passwordResetRequestsLinkHelper = passwordResetRequestsLinkHelper;
            _authAppService = authAppService;
        }

        [EnableRateLimiting("messaging-endpoints-global")]
        [HttpPost]
        public async Task<ActionResult<List<Link>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(PasswordResetRequestsController), nameof(ForgotPassword), correlationId))
            {
                try
                {
                    await _authAppService.ForgotPasswordAsync(request.Email);
                    return Ok(new List<Link>
                    {
                        _passwordResetRequestsLinkHelper.PasswordResetRequestsConfirm(),
                        _passwordResetRequestsLinkHelper.PasswordResetRequestsSend(),
                        _sessionsLinkHelper.SessionsLogin()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while sending reset password email. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Ok(new List<Link>
                    {
                        _passwordResetRequestsLinkHelper.PasswordResetRequestsConfirm(),
                        _passwordResetRequestsLinkHelper.PasswordResetRequestsSend(),
                        _sessionsLinkHelper.SessionsLogin()
                    });
                }
            }
        }

        [HttpPost("confirm")]
        public async Task<ActionResult<List<Link>>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(PasswordResetRequestsController), nameof(ResetPassword), correlationId))
            {
                try
                {
                    await _authAppService.ResetPasswordAsync(request.Email, request.ResetCode, request.NewPassword);
                    return Ok(new List<Link>
                    {
                        _sessionsLinkHelper.SessionsLogin()
                    });
                }
                catch (AuthException ex)
                {
                    var links = new List<Link>
                    {
                        _passwordResetRequestsLinkHelper.PasswordResetRequestsConfirm(),
                        _passwordResetRequestsLinkHelper.PasswordResetRequestsSend(),
                        _sessionsLinkHelper.SessionsLogin()
                    };
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while resetting password. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

    }
}