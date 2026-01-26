using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.Requests
{
    public class ChangeEmailRequest
    {
        public ChangeEmailRequest(string newEmail, string password)
        {
            NewEmail = newEmail;
            Password = password;
        }

        [Required]
        public string NewEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
