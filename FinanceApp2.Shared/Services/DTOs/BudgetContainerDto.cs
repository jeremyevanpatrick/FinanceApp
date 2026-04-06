using FinanceApp2.Shared.Models;

namespace FinanceApp2.Shared.Services.DTOs
{
    public class BudgetContainerDto : Resource
    {
        public BudgetDto? Budget { get; set; }

        public bool HasNextMonth { get; set; }

        public bool HasPreviousMonth { get; set; }

    }
}
