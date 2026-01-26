namespace FinanceApp2.Shared.Services.Responses
{
    public class AuthResponse
    {
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}