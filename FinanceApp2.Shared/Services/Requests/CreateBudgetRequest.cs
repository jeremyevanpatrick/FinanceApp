using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.Requests
{
    public class CreateBudgetRequest
    {
        [Required]
        [Range(1, 12)]
        public int NewBudgetMonth { get; set; }
        [Required]
        [Range(1900, 3000)]
        public int NewBudgetYear { get; set; }
        [Range(1, 12)]
        public int? SourceBudgetMonth { get; set; }
        [Range(1900, 3000)]
        public int? SourceBudgetYear { get; set; }
    }
}
