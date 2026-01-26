using FinanceApp2.Shared.Services.Responses;

namespace FinanceApp2.Shared.Data
{
    public interface IAuthClient
    {
        Task<AuthResponse> LoginAsync(string email, string password);
        Task RegisterAsync(string email, string password);
        Task ConfirmEmailAsync(string userId, string token);
        Task ResendConfirmationEmailAsync(string email);
        Task ChangeEmailAsync(string newEmail, string password);
        Task ChangeEmailConfirmationAsync(string userId, string newEmail, string token);
        Task ChangePasswordAsync(string existingPassword, string newPassword);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string email, string resetCode, string newPassword);
        Task DeleteAccountAsync(string password);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetTokenAsync();
        Task<string> GetUserEmailAsync();
    }
}
