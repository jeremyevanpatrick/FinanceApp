using FinanceApp2.Shared.Services.DTOs;

namespace FinanceApp2.Shared.Data
{
    public interface IBudgetClient
    {
        Task<BudgetContainer> GetBudgetAsync(int month, int year);

        Task CreateBudgetAsync(DateOnly newBudgetDate, DateOnly? sourceBudgetDate = null);

        Task UpdateBudgetAsync(BudgetDto budget);

        Task DeleteBudgetAsync(Guid budgetId);
    }
}
