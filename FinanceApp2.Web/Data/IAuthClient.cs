using FinanceApp2.Shared.Services.DTOs;

namespace FinanceApp2.Web.Data
{
    public interface IAuthClient
    {
        Task<BaseResult> LoginAsync(string email, string password);
        Task<BaseResult> RegisterAsync(string email, string password);
        Task<BaseResult> ConfirmEmailAsync(string userId, string token);
        Task<BaseResult> ResendConfirmationEmailAsync(string email);
        Task<BaseResult> ChangeEmailAsync(string newEmail, string password);
        Task<BaseResult> ChangeEmailConfirmationAsync(string userId, string newEmail, string token);
        Task<BaseResult> ChangePasswordAsync(string existingPassword, string newPassword);
        Task<BaseResult> ForgotPasswordAsync(string email);
        Task<BaseResult> ResetPasswordAsync(string email, string resetCode, string newPassword);
        Task<BaseResult> DeleteAccountAsync(string password);
        Task<BaseResult> LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetTokenAsync();
        Task<string> GetUserEmailAsync();
    }
}
