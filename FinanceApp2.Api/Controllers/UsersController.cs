using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Extensions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Extensions;
using FinanceApp2.Shared.Models;
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
        private readonly SessionsLinkHelper _sessionsLinkHelper;
        private readonly UsersLinkHelper _usersLinkHelper;
        private readonly EmailConfirmationRequestsLinkHelper _emailConfirmationRequestsLinkHelper;
        private readonly EmailChangeRequestsLinkHelper _emailChangeRequestsLinkHelper;
        private readonly IAuthAppService _authAppService;

        public UsersController(
            ILogger<UsersController> logger,
            SessionsLinkHelper sessionsLinkHelper,
            UsersLinkHelper usersLinkHelper,
            EmailConfirmationRequestsLinkHelper emailConfirmationRequestsLinkHelper,
            EmailChangeRequestsLinkHelper emailChangeRequestsLinkHelper,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _sessionsLinkHelper = sessionsLinkHelper;
            _usersLinkHelper = usersLinkHelper;
            _emailConfirmationRequestsLinkHelper = emailConfirmationRequestsLinkHelper;
            _emailChangeRequestsLinkHelper = emailChangeRequestsLinkHelper;
            _authAppService = authAppService;
        }

        [EnableRateLimiting("messaging-endpoints-global")]
        [HttpPost]
        public async Task<ActionResult<List<Link>>> Register([FromBody] RegisterRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(UsersController), nameof(Register), correlationId))
            {
                try
                {
                    await _authAppService.RegisterAsync(request.Email, request.Password);
                    return Created((string?)null, new List<Link>
                    {
                        _emailConfirmationRequestsLinkHelper.EmailConfirmationRequestsConfirm(),
                        _emailConfirmationRequestsLinkHelper.EmailConfirmationRequestsResend(),
                        _sessionsLinkHelper.SessionsLogin()
                    });
                }
                catch (AuthException ex)
                {
                    var links = new List<Link>
                    {
                        _usersLinkHelper.UsersRegister(),
                        _sessionsLinkHelper.SessionsLogin()
                    };
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while registering user. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<ActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(UsersController), nameof(DeleteAccount), correlationId))
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
                    var links = new List<Link>
                    {
                        _usersLinkHelper.UsersDeleteAccount(),
                        _sessionsLinkHelper.SessionsLogin()
                    };
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while deleting user. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId);
                    return Problem500();
                }
            }
        }

        [HttpPatch("me/password")]
        [Authorize]
        public async Task<ActionResult<List<Link>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(UsersController), nameof(ChangePassword), correlationId))
            {
                string userId = string.Empty;

                try
                {
                    userId = User.GetUserId().ToString();
                    await _authAppService.ChangePasswordAsync(userId, request.ExistingPassword, request.NewPassword);
                    return Ok(new List<Link>
                    {
                        _emailChangeRequestsLinkHelper.EmailChangeRequestsSend(),
                        _usersLinkHelper.UsersDeleteAccount(),
                        _sessionsLinkHelper.SessionsLogout()
                    });
                }
                catch (AuthException ex)
                {
                    var links = new List<Link>
                    {
                        _usersLinkHelper.UsersChangePassword(),
                        _emailChangeRequestsLinkHelper.EmailChangeRequestsSend(),
                        _usersLinkHelper.UsersDeleteAccount(),
                        _sessionsLinkHelper.SessionsLogout()
                    };
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while changing user password. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId);
                    return Problem500();
                }
            }
        }

    }
}