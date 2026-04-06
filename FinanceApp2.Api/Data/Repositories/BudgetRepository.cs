using FinanceApp2.Api.Data.Context;
using FinanceApp2.Api.Models;
using Microsoft.EntityFrameworkCore;
using FinanceApp2.Shared.Extensions;

namespace FinanceApp2.Api.Data.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly ILogger<BudgetRepository> _logger;
        private readonly AppDbContext _db;

        public BudgetRepository(
            ILogger<BudgetRepository> logger,
            AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<Budget?> GetByDateAsync(Guid userId, int month, int year)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetRepository), nameof(GetByDateAsync)))
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
        }

        public async Task<Budget?> GetByDateIncludingDeletedAsync(Guid userId, int month, int year)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetRepository), nameof(GetByDateIncludingDeletedAsync)))
            {
                return await _db.Budgets
                    .Include(b => b.Groups.OrderBy(g => g.Order))
                        .ThenInclude(g => g.Items.OrderBy(i => i.CreatedAt))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b =>
                        b.UserId == userId &&
                        b.Month == month &&
                        b.Year == year &&
                        !b.IsDeleted);
            }
        }

        public async Task<bool> GetExistsByDateAsync(Guid userId, int month, int year)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetRepository), nameof(GetExistsByDateAsync)))
            {
                return await _db.Budgets
                    .AsNoTracking()
                    .AnyAsync(b =>
                        b.UserId == userId &&
                        b.Month == month &&
                        b.Year == year &&
                        !b.IsDeleted);
            }
        }

        public async Task CreateAsync(Budget budget)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetRepository), nameof(CreateAsync)))
            {
                _db.Budgets.Add(budget);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Budget budget)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetRepository), nameof(UpdateAsync)))
            {
                _db.Budgets.Update(budget);
                await _db.SaveChangesAsync();
            }
        }

        public async Task CreateGroupsAsync(List<Group> groups)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetRepository), nameof(CreateGroupsAsync)))
            {
                _db.Groups.AddRange(groups);
                await _db.SaveChangesAsync();
            }
        }

        public async Task CreateItemsAsync(List<Item> items)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetRepository), nameof(CreateItemsAsync)))
            {
                _db.Items.AddRange(items);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteUserDataAsync(Guid userId)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetRepository), nameof(DeleteUserDataAsync)))
            {
                await _db.Budgets
                    .Where(b => b.UserId == userId)
                    .ExecuteDeleteAsync();
            }
        }

        public async Task CleanupSoftDeletedUserDataAsync(int purgeAfterDays)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetRepository), nameof(CleanupSoftDeletedUserDataAsync)))
            {
                DateTime olderThanDateTime = DateTime.UtcNow.AddDays(-purgeAfterDays);

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
}
