using FinanceApp2.Shared.Models;

namespace FinanceApp2.Shared.Services.Responses
{
    public class AuthResponse : Resource
    {
        public string? AccessToken { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}