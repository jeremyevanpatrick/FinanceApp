using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.Requests
{
    public class ChangeEmailConfirmationRequest
    {
        public ChangeEmailConfirmationRequest(string userId, string newEmail, string token)
        {
            UserId = userId;
            NewEmail = newEmail;
            Token = token;
        }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string NewEmail { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
