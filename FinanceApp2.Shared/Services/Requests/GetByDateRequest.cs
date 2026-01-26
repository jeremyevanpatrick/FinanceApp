using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.Requests
{
    public class GetByDateRequest
    {
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }
        [Required]
        [Range(1900, 3000)]
        public int Year { get; set; }
    }
}