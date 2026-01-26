using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.Requests
{
    public class DeleteBudgetRequest
    {
        [Required]
        public Guid BudgetId { get; set; }
    }
}
