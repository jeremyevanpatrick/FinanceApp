using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Web.Models
{
    public class ResendEmailConfirmationModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
