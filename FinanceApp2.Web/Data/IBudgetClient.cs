using FinanceApp2.Shared.Services.DTOs;

namespace FinanceApp2.Web.Data
{
    public interface IBudgetClient
    {
        Task<BaseResult<BudgetContainer?>> GetBudgetAsync(int month, int year);

        Task<BaseResult> CreateBudgetAsync(DateOnly newBudgetDate, DateOnly? sourceBudgetDate = null);

        Task<BaseResult> UpdateBudgetAsync(BudgetDto budget);

        Task<BaseResult> DeleteBudgetAsync(Guid budgetId);
    }
}
