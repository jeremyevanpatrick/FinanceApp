using FinanceApp2.Api.Errors;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionsController : ControllerBaseExtended
    {
        private readonly ILogger<SessionsController> _logger;
        private readonly IAuthAppService _authAppService;

        public SessionsController(
            ILogger<SessionsController> logger,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _authAppService = authAppService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                AuthResponse authResponse = await _authAppService.LoginAsync(request.Email, request.Password);

                string refreshTokenString = await _authAppService.CreateRefreshTokenAsync(authResponse.UserId);

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
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.LoginUnexpected, ex, "Unexpected error during user login", new Dictionary<string, string> { });
                return Problem500();
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshTokenString = Request.Cookies["refresh_token"];
                await _authAppService.LogoutAsync(refreshTokenString);

                Response.Cookies.Delete("refresh_token");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.LogoutUnexpected, ex, "Unexpected error while signing out", new Dictionary<string, string> { });
                return Problem500();
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            string userId = string.Empty;

            try
            {
                var refreshTokenString = Request.Cookies["refresh_token"];

                AuthResponse authResponse = await _authAppService.RefreshTokenAsync(refreshTokenString);

                string newRefreshTokenString = await _authAppService.CreateRefreshTokenAsync(authResponse.UserId);

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
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.RefreshTokenUnexpected, ex, "Unexpected error while refreshing token", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem500();
            }
        }

    }
}