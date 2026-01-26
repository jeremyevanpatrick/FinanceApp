using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.Requests
{
    public class DeleteAccountRequest
    {
        public DeleteAccountRequest(string password)
        {
            Password = password;
        }

        [Required]
        public string Password { get; set; }
    }
}
