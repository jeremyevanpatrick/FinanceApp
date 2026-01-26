using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Web.Models
{
    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string? ExistingPassword { get; set; }

        [Required]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long"
        )]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character"
        )]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
    }
}
