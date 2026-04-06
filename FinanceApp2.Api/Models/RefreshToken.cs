namespace FinanceApp2.Api.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public required string TokenHash { get; set; }

        public required string UserId { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime? RevokedAt { get; set; }

    }
}
