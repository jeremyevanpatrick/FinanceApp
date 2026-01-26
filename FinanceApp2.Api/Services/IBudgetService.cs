using FinanceApp2.Shared.Models;

namespace FinanceApp2.Api.Services
{
    public interface IBudgetService
    {
        public Task<Budget?> GetByDate(Guid userId, int month, int year);

        public Task<Budget?> GetById(Guid budgetId);

        public Task<bool> GetExistsByDate(Guid userId, int month, int year);

        public Task CreateAsync(Budget budget);

        public Task UpdateAsync(Budget existingBudget, Budget updatedBudget);

        public Task DeleteAsync(Budget budget);
    }
}
