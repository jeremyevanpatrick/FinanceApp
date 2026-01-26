using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Web.Models
{
    public class DeleteAccountModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
