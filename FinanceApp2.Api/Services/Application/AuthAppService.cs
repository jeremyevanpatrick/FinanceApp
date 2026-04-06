using FinanceApp2.Api.Services.Background;
using FinanceApp2.Api.Data.Repositories;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Models;
using FinanceApp2.Api.Services.Queues;
using FinanceApp2.Api.Settings;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Services.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanceApp2.Shared.Extensions;

namespace FinanceApp2.Api.Services.Application
{
    public class AuthAppService : IAuthAppService
    {
        private readonly ILogger<AuthAppService> _logger;
        private readonly IAuthRepository _authRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ClientSettings _clientSettings;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailSenderQueue _emailSenderQueue;

        public AuthAppService(
            ILogger<AuthAppService> logger,
            IAuthRepository authRepository,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<ClientSettings> clientSettings,
            IOptions<JwtSettings> jwtSettings,
            IEmailSenderQueue emailSenderQueue)
        {
            _logger = logger;
            _authRepository = authRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _clientSettings = clientSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _emailSenderQueue = emailSenderQueue;
        }

        public async Task RegisterAsync(string email, string password)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(RegisterAsync)))
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = email,
                    Email = email
                };

                IdentityResult result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    if (result.Errors.Where(e => e.Code.StartsWith("Password")).Any())
                    {
                        throw new AuthException("Password does not meet requirements.", StatusCodes.Status400BadRequest, ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS);
                    }
                    return;
                }

                await SendEmailConfirmationEmail(user);
            }
        }

        public async Task ResendConfirmationEmailAsync(string email)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(ResendConfirmationEmailAsync)))
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return;
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return;
                }

                if (user.EmailConfirmed)
                {
                    return;
                }

                await SendEmailConfirmationEmail(user);
            }
        }

        private async Task SendEmailConfirmationEmail(ApplicationUser user)
        {
            string emailSubject = "Confirm your email address";

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = $"{_clientSettings.Host}/confirmemail?userid={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(encodedToken)}";
            var messageHtml = $"Please confirm your account by <a href='{confirmationUrl}'>clicking here</a>.";

            var emailDetails = new EmailDetails(user.Email, emailSubject, messageHtml);

            _emailSenderQueue.Enqueue(emailDetails);
        }

        public async Task ConfirmEmailAsync(string userId, string token)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(ConfirmEmailAsync)))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new AuthException("Invalid or expired confirmation link.", StatusCodes.Status400BadRequest, ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }

                string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

                IdentityResult result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (!result.Succeeded)
                {
                    throw new AuthException("Invalid or expired confirmation link.", StatusCodes.Status400BadRequest, ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }
            }
        }

        public async Task<(AuthResponse authResponse, string refreshTokenString)> LoginAsync(string email, string password)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(LoginAsync)))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    throw new AuthException("Invalid credentials", StatusCodes.Status401Unauthorized, ApiErrorCodes.INVALID_CREDENTIALS);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    password,
                    lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                        var minutesRemaining = (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes;
                        throw new AuthException($"Account is locked. Please try again in {minutesRemaining} minutes.", StatusCodes.Status401Unauthorized, ApiErrorCodes.ACCOUNT_LOCKED);
                    }
                    throw new AuthException("Invalid credentials", StatusCodes.Status401Unauthorized, ApiErrorCodes.INVALID_CREDENTIALS);
                }

                var accessToken = GenerateJwtToken(user);

                AuthResponse authResponse = new AuthResponse
                {
                    AccessToken = accessToken,
                    UserId = user.Id,
                    Email = user.Email,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes)
                };

                string refreshTokenString = await CreateRefreshTokenAsync(user.Id);

                return (authResponse, refreshTokenString);
            }
        }

        public async Task<string> CreateRefreshTokenAsync(string userId)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(CreateRefreshTokenAsync)))
            {
                var refreshTokenString = GenerateRefreshToken();

                await _authRepository.AddRefreshTokenAsync(new RefreshToken
                {
                    TokenHash = TokenHashHelper.Hash(refreshTokenString),
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow
                });

                return refreshTokenString;
            }
        }

        public async Task ForgotPasswordAsync(string email)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(ForgotPasswordAsync)))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    return;
                }

                await SendPasswordResetEmail(email, user);
            }
        }

        private async Task SendPasswordResetEmail(string email, ApplicationUser user)
        {
            string emailSubject = "Reset your password";

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetUrl = $"{_clientSettings.Host}/resetpassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(encodedToken)}";
            var messageHtml = $"Reset your password by <a href='{resetUrl}'>clicking here</a>";

            var emailDetails = new EmailDetails(email, emailSubject, messageHtml);

            _emailSenderQueue.Enqueue(emailDetails);
        }

        public async Task ResetPasswordAsync(string email, string resetCode, string newPassword)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(ResetPasswordAsync)))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    throw new AuthException("Invalid or expired reset link.", StatusCodes.Status400BadRequest, ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }

                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetCode));

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
                if (!result.Succeeded)
                {
                    if (result.Errors.Where(e => e.Code.StartsWith("Password")).Any())
                    {
                        throw new AuthException("Password does not meet requirements.", StatusCodes.Status400BadRequest, ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS);
                    }

                    throw new AuthException("Invalid or expired reset link.", StatusCodes.Status400BadRequest, ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }
            }
        }

        public async Task ChangePasswordAsync(string userId, string existingPassword, string newPassword)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(ChangePasswordAsync)))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new AuthException("Authentication is no longer valid.", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID);
                }

                IdentityResult result = await _userManager.ChangePasswordAsync(user, existingPassword, newPassword);
                if (!result.Succeeded)
                {
                    if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
                    {
                        throw new AuthException("Invalid credentials", StatusCodes.Status401Unauthorized, ApiErrorCodes.INVALID_CREDENTIALS);
                    }

                    if (result.Errors.Where(e => e.Code.StartsWith("Password")).Any())
                    {
                        throw new AuthException("New password does not meet requirements.", StatusCodes.Status400BadRequest, ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS);
                    }

                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        public async Task ChangeEmailAsync(string userId, string newEmail, string password)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(ChangeEmailAsync)))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new AuthException("Authentication is no longer valid.", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID);
                }

                userId = user.Id;

                var passwordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordValid)
                {
                    throw new AuthException("Invalid credentials", StatusCodes.Status401Unauthorized, ApiErrorCodes.INVALID_CREDENTIALS);
                }

                var existingUser = await _userManager.FindByEmailAsync(newEmail);
                if (existingUser != null)
                {
                    throw new AuthException("Email address is already in use.", StatusCodes.Status400BadRequest, ApiErrorCodes.EMAIL_ADDRESS_ALREADY_IN_USE);
                }

                await SendChangeEmailEmail(newEmail, user);
            }
        }

        private async Task SendChangeEmailEmail(string newEmail, ApplicationUser user)
        {
            string emailSubject = "Confirm your new email address";

            string token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = $"{_clientSettings.Host}/confirmemailchange?userid={Uri.EscapeDataString(user.Id)}&email={Uri.EscapeDataString(newEmail)}&token={Uri.EscapeDataString(encodedToken)}";
            var messageHtml = $"Please confirm your new email by <a href='{confirmationUrl}'>clicking here</a>.";

            var emailDetails = new EmailDetails(newEmail, emailSubject, messageHtml);

            _emailSenderQueue.Enqueue(emailDetails);
        }

        public async Task ChangeEmailConfirmationAsync(string userId, string newEmail, string token)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(ChangeEmailConfirmationAsync)))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new AuthException("Authentication is no longer valid.", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID);
                }

                string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

                var result = await _userManager.ChangeEmailAsync(user, newEmail, decodedToken);
                if (!result.Succeeded)
                {
                    throw new AuthException("Invalid or expired confirmation link.", StatusCodes.Status400BadRequest, ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED);
                }
            }
        }

        public async Task<(AuthResponse authResponse, string newRefreshTokenString)> RotateRefreshTokenAsync(string? refreshTokenString)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(RotateRefreshTokenAsync)))
            {
                if (string.IsNullOrWhiteSpace(refreshTokenString))
                {
                    throw new AuthException("Authentication is no longer valid.", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID);
                }

                RefreshToken? refreshToken = await _authRepository.GetRefreshTokenAsync(TokenHashHelper.Hash(refreshTokenString));
                if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    throw new AuthException("Authentication is no longer valid.", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID);
                }
                await RevokeRefreshTokenAsync(refreshTokenString);

                var user = await _userManager.FindByIdAsync(refreshToken.UserId);
                if (user == null)
                {
                    throw new AuthException("Authentication is no longer valid.", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID);
                }

                var accessToken = GenerateJwtToken(user);

                AuthResponse authResponse = new AuthResponse
                {
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes)
                };

                string newRefreshTokenString = await CreateRefreshTokenAsync(user.Id);

                return (authResponse, newRefreshTokenString);
            }
        }

        public async Task RevokeRefreshTokenAsync(string refreshTokenString)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(RevokeRefreshTokenAsync)))
            {
                if (string.IsNullOrWhiteSpace(refreshTokenString))
                {
                    throw new AuthException("Authentication is no longer valid.", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID);
                }

                RefreshToken? refreshToken = await _authRepository.GetRefreshTokenAsync(TokenHashHelper.Hash(refreshTokenString));
                if (refreshToken == null || refreshToken.IsRevoked)
                {
                    throw new AuthException("Authentication is no longer valid.", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID);
                }

                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _authRepository.UpdateRefreshTokenAsync(refreshToken);
            }
        }

        public async Task DeleteAccountAsync(string userId, string password)
        {
            using (_logger.BeginLoggingScope(nameof(AuthAppService), nameof(DeleteAccountAsync)))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new AuthException("Authentication is no longer valid.", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID);
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordValid)
                {
                    throw new AuthException("Invalid credentials", StatusCodes.Status401Unauthorized, ApiErrorCodes.INVALID_CREDENTIALS);
                }

                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                List<RefreshToken> refreshTokenList = await _authRepository.GetUserRefreshTokensAsync(userId);
                foreach (var refreshToken in refreshTokenList)
                {
                    refreshToken.IsRevoked = true;
                    refreshToken.RevokedAt = DateTime.UtcNow;
                }
                await _authRepository.UpdateRefreshTokenRangeAsync(refreshTokenList);
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
