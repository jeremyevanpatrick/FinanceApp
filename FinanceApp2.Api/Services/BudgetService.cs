using FinanceApp2.Api.Data;
using FinanceApp2.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly AppDbContext _db;

        public BudgetService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Budget?> GetByDate(Guid userId, int month, int year)
        {
            return await _db.Budgets
                .Include(b => b.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                    .ThenInclude(g => g.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt))
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.Month == month &&
                    b.Year == year &&
                    !b.IsDeleted);
        }

        public async Task<Budget?> GetById(Guid budgetId)
        {
            return await _db.Budgets
                .Include(b => b.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                    .ThenInclude(g => g.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt))
                .FirstOrDefaultAsync(b =>
                    b.BudgetId == budgetId &&
                    !b.IsDeleted);
        }

        public async Task<bool> GetExistsByDate(Guid userId, int month, int year)
        {
            return await _db.Budgets
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

        public async Task UpdateAsync(Budget existingBudget, Budget updatedBudget)
        {
            existingBudget.Income = updatedBudget.Income;
            existingBudget.ModifiedAt = DateTime.UtcNow;

            foreach (var updatedGroup in updatedBudget.Groups)
            {
                Group? existingGroup = existingBudget.Groups.FirstOrDefault(g => g.GroupId == updatedGroup.GroupId);
                if (existingGroup == null)
                {
                    existingBudget.Groups.Add(updatedGroup);
                    _db.Entry(updatedGroup).State = EntityState.Added;

                    foreach (var updatedItem in updatedGroup.Items)
                    {
                        _db.Entry(updatedItem).State = EntityState.Added;
                    }
                }
                else
                {
                    existingGroup.GroupName = updatedGroup.GroupName;
                    existingGroup.Order = updatedGroup.Order;
                    existingGroup.IsDeleted = updatedGroup.IsDeleted;
                    existingGroup.ModifiedAt = DateTime.UtcNow;

                    foreach (var updatedItem in updatedGroup.Items)
                    {
                        Item? existingItem = existingGroup.Items.FirstOrDefault(i => i.ItemId == updatedItem.ItemId);
                        if (existingItem == null)
                        {
                            existingGroup.Items.Add(updatedItem);
                            _db.Entry(updatedItem).State = EntityState.Added;
                        }
                        else
                        {
                            existingItem.ItemName = updatedItem.ItemName;
                            existingItem.Spent = updatedItem.Spent;
                            existingItem.Budgeted = updatedItem.Budgeted;
                            existingItem.IsDeleted = updatedItem.IsDeleted;
                            existingItem.ModifiedAt = DateTime.UtcNow;
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Budget budget)
        {
            budget.IsDeleted = true;
            budget.ModifiedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

    }
}
