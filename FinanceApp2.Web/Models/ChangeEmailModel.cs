using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Web.Models
{
    public class ChangeEmailModel
    {
        [Required]
        [EmailAddress]
        public string? NewEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
