using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Web.Models
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

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
        public string? Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string? PasswordRepeat { get; set; }
    }
}
