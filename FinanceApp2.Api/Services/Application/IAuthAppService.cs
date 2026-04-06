using FinanceApp2.Shared.Services.Responses;

namespace FinanceApp2.Api.Services.Application
{
    public interface IAuthAppService
    {
        Task ChangeEmailAsync(string userId, string newEmail, string password);
        Task ChangeEmailConfirmationAsync(string userId, string newEmail, string token);
        Task ChangePasswordAsync(string userId, string existingPassword, string newPassword);
        Task ConfirmEmailAsync(string userId, string token);
        Task DeleteAccountAsync(string userId, string password);
        Task ForgotPasswordAsync(string email);
        Task<(AuthResponse authResponse, string refreshTokenString)> LoginAsync(string email, string password);
        Task<string> CreateRefreshTokenAsync(string userId);
        Task RevokeRefreshTokenAsync(string refreshTokenString);
        Task<(AuthResponse authResponse, string newRefreshTokenString)> RotateRefreshTokenAsync(string? refreshTokenString);
        Task RegisterAsync(string email, string password);
        Task ResendConfirmationEmailAsync(string email);
        Task ResetPasswordAsync(string email, string resetCode, string newPassword);
    }
}