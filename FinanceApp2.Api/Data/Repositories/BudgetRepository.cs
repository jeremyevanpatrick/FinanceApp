using FinanceApp2.Api.Data.Context;
using FinanceApp2.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Data.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly AppDbContext _db;

        public BudgetRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Budget?> GetByDateAsync(Guid userId, int month, int year)
        {
            return await _db.Budgets
                .Include(b => b.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                    .ThenInclude(g => g.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt))
                .AsNoTracking()
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.Month == month &&
                    b.Year == year &&
                    !b.IsDeleted);
        }

        public async Task<bool> GetExistsByDateAsync(Guid userId, int month, int year)
        {
            return await _db.Budgets
                .AsNoTracking()
                .AnyAsync(b =>
                    b.UserId == userId &&
                    b.Month == month &&
                    b.Year == year &&
                    !b.IsDeleted);
        }

        public async Task CreateAsync(Budget budget)
        {
            _db.Budgets.Add(budget);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Budget budget)
        {
            _db.Budgets.Update(budget);
            await _db.SaveChangesAsync();
        }

        public async Task CreateGroupsAsync(List<Group> groups)
        {
            _db.Groups.AddRange(groups);
            await _db.SaveChangesAsync();
        }

        public async Task CreateItemsAsync(List<Item> items)
        {
            _db.Items.AddRange(items);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteUserDataAsync(Guid userId)
        {
            await _db.Budgets
                .Where(b => b.UserId == userId)
                .ExecuteDeleteAsync();
        }

        public async Task CleanupSoftDeletedUserDataAsync(int olderThanDays = 0)
        {
            DateTime olderThanDateTime = DateTime.UtcNow.AddDays(-olderThanDays);

            await _db.Budgets
                .Where(b => b.IsDeleted && b.ModifiedAt < olderThanDateTime)
                .ExecuteDeleteAsync();

            await _db.Groups
                .Where(g => g.IsDeleted && g.ModifiedAt < olderThanDateTime)
                .ExecuteDeleteAsync();

            await _db.Items
                .Where(i => i.IsDeleted && i.ModifiedAt < olderThanDateTime)
                .ExecuteDeleteAsync();
        }
    }
}
