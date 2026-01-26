using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.Requests
{
    public class ChangePasswordRequest
    {
        public ChangePasswordRequest(string existingPassword, string newPassword)
        {
            ExistingPassword = existingPassword;
            NewPassword = newPassword;
        }

        [Required]
        public string ExistingPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
