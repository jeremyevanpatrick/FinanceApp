using FinanceApp2.Api.Models;
using FinanceApp2.Shared.Services.DTOs;

namespace FinanceApp2.Shared.Helpers
{
    public static class BudgetMapper
    {
        public static BudgetDto? ToDto(Budget? b)
        {
            if (b == null)
            {
                return null;
            }

            return new BudgetDto()
            {
                BudgetId = b.BudgetId,
                Month = b.Month,
                Year = b.Year,
                Income = b.Income,
                UserId = b.UserId,
                Groups = b.Groups.Select(g => new GroupDto()
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    BudgetId = g.BudgetId,
                    Order = g.Order,
                    Items = g.Items.Select(i => new ItemDto()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName,
                        GroupId = i.GroupId,
                        Budgeted = i.Budgeted,
                        Spent = i.Spent,
                        CreatedAt = i.CreatedAt,
                        ModifiedAt = i.ModifiedAt,
                        IsDeleted = i.IsDeleted
                    }).ToList(),
                    CreatedAt = g.CreatedAt,
                    ModifiedAt = g.ModifiedAt,
                    IsDeleted = g.IsDeleted
                }).ToList(),
                CreatedAt = b.CreatedAt,
                ModifiedAt = b.ModifiedAt,
                IsDeleted = b.IsDeleted
            };
        }

        public static Budget? ToEntity(BudgetDto b)
        {
            if (b == null)
            {
                return null;
            }

            return new Budget()
            {
                BudgetId = b.BudgetId,
                Month = b.Month,
                Year = b.Year,
                Income = b.Income ?? 0,
                UserId = b.UserId,
                Groups = b.Groups.Select(g => new Group()
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    BudgetId = g.BudgetId,
                    Order = g.Order,
                    Items = g.Items.Select(i => new Item()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName,
                        GroupId = i.GroupId,
                        Budgeted = i.Budgeted,
                        Spent = i.Spent,
                        CreatedAt = i.CreatedAt,
                        ModifiedAt = i.ModifiedAt,
                        IsDeleted = i.IsDeleted
                    }).ToList(),
                    CreatedAt = g.CreatedAt,
                    ModifiedAt = g.ModifiedAt,
                    IsDeleted = g.IsDeleted
                }).ToList(),
                CreatedAt = b.CreatedAt,
                ModifiedAt = b.ModifiedAt,
                IsDeleted = b.IsDeleted
            };
        }
    }
}
