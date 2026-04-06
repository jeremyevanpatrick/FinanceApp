using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Extensions;
using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("email-confirmation-requests")]
    public class EmailConfirmationRequestsController : ControllerBaseExtended
    {
        private readonly ILogger<EmailConfirmationRequestsController> _logger;
        private readonly SessionsLinkHelper _sessionsLinkHelper;
        private readonly EmailConfirmationRequestsLinkHelper _emailConfirmationRequestsLinkHelper;
        private readonly IAuthAppService _authAppService;

        public EmailConfirmationRequestsController(
            ILogger<EmailConfirmationRequestsController> logger,
            SessionsLinkHelper sessionsLinkHelper,
            EmailConfirmationRequestsLinkHelper emailConfirmationRequestsLinkHelper,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _sessionsLinkHelper = sessionsLinkHelper;
            _emailConfirmationRequestsLinkHelper = emailConfirmationRequestsLinkHelper;
            _authAppService = authAppService;
        }

        [EnableRateLimiting("messaging-endpoints-global")]
        [HttpPost("resend")]
        public async Task<ActionResult<List<Link>>> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(EmailConfirmationRequestsController), nameof(ResendConfirmationEmail), correlationId))
            {
                try
                {
                    await _authAppService.ResendConfirmationEmailAsync(request.Email);
                    return Ok(new List<Link>
                    {
                        _emailConfirmationRequestsLinkHelper.EmailConfirmationRequestsResend(),
                        _emailConfirmationRequestsLinkHelper.EmailConfirmationRequestsConfirm()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while resending confirmation email. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Ok(new List<Link>
                    {
                        _emailConfirmationRequestsLinkHelper.EmailConfirmationRequestsResend(),
                        _emailConfirmationRequestsLinkHelper.EmailConfirmationRequestsConfirm()
                    });
                }
            }
        }

        [HttpPost("confirm")]
        public async Task<ActionResult<List<Link>>> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(EmailConfirmationRequestsController), nameof(ConfirmEmail), correlationId))
            {
                try
                {
                    await _authAppService.ConfirmEmailAsync(request.UserId, request.Token);
                    return Ok(new List<Link>
                    {
                        _sessionsLinkHelper.SessionsLogin()
                    });
                }
                catch (AuthException ex)
                {
                    var links = new List<Link>
                    {
                        _emailConfirmationRequestsLinkHelper.EmailConfirmationRequestsConfirm(),
                        _emailConfirmationRequestsLinkHelper.EmailConfirmationRequestsResend()
                    };
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while confirming email. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }
    }
}