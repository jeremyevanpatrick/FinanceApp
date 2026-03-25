using FinanceApp2.Api.Models;

namespace FinanceApp2.Api.Data.Repositories
{
    public interface IBudgetRepository
    {
        public Task<Budget?> GetByDateAsync(Guid userId, int month, int year);

        public Task<bool> GetExistsByDateAsync(Guid userId, int month, int year);

        public Task CreateAsync(Budget budget);

        public Task UpdateAsync(Budget budget);
        
        public Task CreateGroupsAsync(List<Group> groups);

        public Task CreateItemsAsync(List<Item> items);

        public Task DeleteUserDataAsync(Guid userId);

        public Task CleanupSoftDeletedUserDataAsync(int olderThanDays = 0);
    }
}
