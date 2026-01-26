using FinanceApp2.Shared.Services.DTOs;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.Requests
{
    public class UpdateBudgetRequest
    {
        [Required]
        public BudgetDto Budget { get; set; } = null!;
    }
}