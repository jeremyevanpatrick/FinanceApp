using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Web.Services.Requests
{
    public class ResendConfirmationEmailRequest
    {
        public ResendConfirmationEmailRequest(string email)
        {
            Email = email;
        }

        [Required]
        public string Email { get; set; }
    }
}
