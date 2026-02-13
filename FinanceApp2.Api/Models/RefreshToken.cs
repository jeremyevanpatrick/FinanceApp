using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Api.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public string TokenHash { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

    }
}
