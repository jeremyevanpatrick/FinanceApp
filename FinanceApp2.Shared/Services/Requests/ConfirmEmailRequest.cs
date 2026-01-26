using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.Requests
{
    public class ConfirmEmailRequest
    {
        public ConfirmEmailRequest(string userId, string token)
        {
            UserId = userId;
            Token = token;
        }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
