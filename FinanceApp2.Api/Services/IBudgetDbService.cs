using FinanceApp2.Api.Models;

namespace FinanceApp2.Api.Services
{
    public interface IBudgetDbService
    {
        public Task<Budget?> GetByDate(Guid userId, int month, int year);

        public Task<Budget?> GetById(Guid budgetId, bool includeDeleted = false);

        public Task<bool> GetExistsByDate(Guid userId, int month, int year);

        public Task CreateAsync(Budget budget);

        public Task UpdateAsync(Budget existingBudget, Budget updatedBudget);

        public Task DeleteAsync(Budget budget);

        public Task DeleteUserDataAsync(Guid userId);

        public Task CleanupSoftDeletedUserDataAsync(int olderThanDays = 0);
    }
}
