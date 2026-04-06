using FinanceApp2.Api.Errors;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Extensions;
using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionsController : ControllerBaseExtended
    {
        private readonly ILogger<SessionsController> _logger;
        private readonly SessionsLinkHelper _sessionsLinkHelper;
        private readonly IAuthAppService _authAppService;

        public SessionsController(
            ILogger<SessionsController> logger,
            SessionsLinkHelper sessionsLinkHelper,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _sessionsLinkHelper = sessionsLinkHelper;
            _authAppService = authAppService;
        }

        [HttpPost]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(SessionsController), nameof(Login), correlationId))
            {
                try
                {
                    (AuthResponse authResponse, string refreshTokenString) = await _authAppService.LoginAsync(request.Email, request.Password);

                    authResponse.Links = _sessionsLinkHelper.GetLinksForSessionsCreate();

                    Response.Cookies.Append("refresh_token", refreshTokenString, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(30)
                    });

                    return Ok(authResponse);
                }
                catch (AuthException ex)
                {
                    var links = new List<Link>
                    {
                        _sessionsLinkHelper.SessionsLogin()
                    };
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error during user login. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<ActionResult<List<Link>>> Logout()
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(SessionsController), nameof(Logout), correlationId))
            {
                try
                {
                    var refreshTokenString = Request.Cookies["refresh_token"];

                    try
                    {
                        await _authAppService.RevokeRefreshTokenAsync(refreshTokenString);
                    }
                    catch (AuthException ex) { }

                    Response.Cookies.Delete("refresh_token");

                    return Ok(new List<Link>
                    {
                        _sessionsLinkHelper.SessionsLogin()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while signing out. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

        [EnableRateLimiting("public-token-refresh-endpoint")]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> RefreshToken()
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(SessionsController), nameof(RefreshToken), correlationId))
            {
                try
                {
                    var refreshTokenString = Request.Cookies["refresh_token"];

                    (AuthResponse authResponse, string newRefreshTokenString) = await _authAppService.RotateRefreshTokenAsync(refreshTokenString);

                    authResponse.Links = _sessionsLinkHelper.GetLinksForSessionsRefresh();

                    Response.Cookies.Append("refresh_token", newRefreshTokenString, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(30)
                    });

                    return Ok(authResponse);
                }
                catch (AuthException ex)
                {
                    var links = new List<Link>
                    {
                        _sessionsLinkHelper.SessionsLogin()
                    };
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while refreshing token. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

    }
}