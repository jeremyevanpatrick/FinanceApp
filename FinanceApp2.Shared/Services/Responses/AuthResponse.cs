namespace FinanceApp2.Shared.Services.Responses
{
    public class AuthResponse
    {
        public string? RefreshToken { get; set; }
        public string? AccessToken { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}