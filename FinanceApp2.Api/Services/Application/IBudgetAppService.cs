using FinanceApp2.Shared.Services.DTOs;

namespace FinanceApp2.Api.Services.Application
{
    public interface IBudgetAppService
    {
        public Task<BudgetContainer> GetByDateAsync(Guid userId, int month, int year);

        public Task<BudgetDto> CreateAsync(Guid userId, int newBudgetMonth, int newBudgetYear, int? sourceBudgetMonth, int? sourceBudgetYear);

        public Task UpdateAsync(Guid userId, int month, int year, int? updatedIncome, List<GroupDto> updatedGroups);

        public Task DeleteAsync(Guid userId, int year, int month);

    }
}
