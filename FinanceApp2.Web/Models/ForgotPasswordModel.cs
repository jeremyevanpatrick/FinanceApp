using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Web.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
