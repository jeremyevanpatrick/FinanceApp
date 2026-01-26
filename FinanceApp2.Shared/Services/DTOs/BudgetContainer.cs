namespace FinanceApp2.Shared.Services.DTOs
{
    public class BudgetContainer
    {
        public BudgetDto? Budget { get; set; }

        public bool HasNextMonth { get; set; }

        public bool HasPreviousMonth { get; set; }

    }
}
