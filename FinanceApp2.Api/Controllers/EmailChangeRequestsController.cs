using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Extensions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using FinanceApp2.Shared.Extensions;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("email-change-requests")]
    public class EmailChangeRequestsController : ControllerBaseExtended
    {
        private readonly ILogger<EmailChangeRequestsController> _logger;
        private readonly SessionsLinkHelper _sessionsLinkHelper;
        private readonly UsersLinkHelper _usersLinkHelper;
        private readonly EmailChangeRequestsLinkHelper _emailChangeRequestsLinkHelper;
        private readonly IAuthAppService _authAppService;

        public EmailChangeRequestsController(
            ILogger<EmailChangeRequestsController> logger,
            SessionsLinkHelper sessionsLinkHelper,
            UsersLinkHelper usersLinkHelper,
            EmailChangeRequestsLinkHelper emailChangeRequestsLinkHelper,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _sessionsLinkHelper = sessionsLinkHelper;
            _usersLinkHelper = usersLinkHelper;
            _emailChangeRequestsLinkHelper = emailChangeRequestsLinkHelper;
            _authAppService = authAppService;
        }

        [EnableRateLimiting("messaging-endpoints-global")]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<List<Link>>> ChangeEmail([FromBody] ChangeEmailRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(EmailChangeRequestsController), nameof(ChangeEmail), correlationId))
            {
                string userId = string.Empty;

                try
                {
                    userId = User.GetUserId().ToString();
                    await _authAppService.ChangeEmailAsync(userId, request.NewEmail, request.Password);
                    return Ok(new List<Link>
                    {
                        _emailChangeRequestsLinkHelper.EmailChangeRequestsConfirm(),
                        _emailChangeRequestsLinkHelper.EmailChangeRequestsSend(),
                        _usersLinkHelper.UsersChangePassword(),
                        _usersLinkHelper.UsersDeleteAccount()
                    });
                }
                catch (AuthException ex)
                {
                    var links = new List<Link>
                    {
                        _emailChangeRequestsLinkHelper.EmailChangeRequestsSend(),
                        _usersLinkHelper.UsersChangePassword(),
                        _usersLinkHelper.UsersDeleteAccount()
                    };
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while sending change email message. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId);
                    return Problem500();
                }
            }
        }

        [HttpPost("confirm")]
        public async Task<ActionResult<List<Link>>> ChangeEmailConfirmation([FromBody] ChangeEmailConfirmationRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(EmailChangeRequestsController), nameof(ChangeEmailConfirmation), correlationId))
            {
                try
                {
                    await _authAppService.ChangeEmailConfirmationAsync(request.UserId, request.NewEmail, request.Token);
                    return Ok(new List<Link>
                    {
                        _sessionsLinkHelper.SessionsLogin()
                    });
                }
                catch (AuthException ex)
                {
                    var links = new List<Link>
                    {
                        _emailChangeRequestsLinkHelper.EmailChangeRequestsSend(),
                        _emailChangeRequestsLinkHelper.EmailChangeRequestsConfirm()
                    };
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while confirming email change. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        request.UserId);
                    return Problem500();
                }
            }
        }

    }
}