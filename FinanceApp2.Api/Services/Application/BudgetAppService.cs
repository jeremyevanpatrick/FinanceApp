using FinanceApp2.Api.Data.Repositories;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Models;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using FinanceApp2.Shared.Extensions;

namespace FinanceApp2.Api.Services.Application
{
    public class BudgetAppService : IBudgetAppService
    {
        private readonly ILogger<BudgetAppService> _logger;
        private readonly IBudgetRepository _budgetRepository;

        public BudgetAppService(
            ILogger<BudgetAppService> logger,
            IBudgetRepository budgetRepository)
        {
            _logger = logger;
            _budgetRepository = budgetRepository;
        }

        public async Task<BudgetContainerDto> GetByDateAsync(Guid userId, int month, int year)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetAppService), nameof(GetByDateAsync)))
            {
                DateOnly requestedDate = new DateOnly(year, month, 1);
                DateOnly previousMonthDate = requestedDate.AddMonths(-1);
                DateOnly nextMonthDate = requestedDate.AddMonths(1);

                Budget? budget = await _budgetRepository.GetByDateAsync(userId, month, year);
                bool hasPreviousMonth = await _budgetRepository.GetExistsByDateAsync(userId, previousMonthDate.Month, previousMonthDate.Year);
                bool hasNextMonth = await _budgetRepository.GetExistsByDateAsync(userId, nextMonthDate.Month, nextMonthDate.Year);

                BudgetContainerDto budgetContainer = new BudgetContainerDto()
                {
                    Budget = BudgetMapper.ToDto(budget),
                    HasPreviousMonth = hasPreviousMonth,
                    HasNextMonth = hasNextMonth
                };

                return budgetContainer;
            }
        }

        public async Task<BudgetDto> CreateAsync(Guid userId, int newBudgetMonth, int newBudgetYear, int? sourceBudgetMonth, int? sourceBudgetYear)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetAppService), nameof(CreateAsync)))
            {
                Budget? existingBudget = await _budgetRepository.GetByDateAsync(userId, newBudgetMonth, newBudgetYear);

                if (existingBudget != null)
                {
                    throw new BudgetConflictException($"Budget for {newBudgetMonth}/{newBudgetYear} already exists.");
                }

                Budget newBudget = new Budget
                {
                    Month = newBudgetMonth,
                    Year = newBudgetYear,
                    UserId = userId,
                    Income = 0,
                    Groups = new List<Group>(),
                    ModifiedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                if (sourceBudgetMonth != null && sourceBudgetYear != null)
                {
                    //if source fields are included in the request, clone contents from the source budget
                    Budget? sourceBudget = await _budgetRepository.GetByDateAsync(userId, (int)sourceBudgetMonth, (int)sourceBudgetYear);

                    if (sourceBudget == null)
                    {
                        throw new NotFoundException("Source budget");
                    }

                    newBudget.Groups = sourceBudget.Groups.Select(g =>
                    {
                        Group newGroup = new Group
                        {
                            BudgetId = newBudget.BudgetId,
                            Budget = newBudget,
                            GroupName = g.GroupName,
                            Order = g.Order,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        };

                        newGroup.Items = g.Items.Select(i => new Item
                        {
                            GroupId = newGroup.GroupId,
                            ItemName = i.ItemName,
                            Budgeted = i.Budgeted,
                            Group = newGroup,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        }).ToList();

                        return newGroup;
                    }).ToList();
                }

                await _budgetRepository.CreateAsync(newBudget);

                return BudgetMapper.ToDto(newBudget);
            }
        }

        public async Task UpdateAsync(Guid userId, int month, int year, int? updatedIncome, List<GroupDto> updatedGroups)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetAppService), nameof(UpdateAsync)))
            {
                Budget? existingBudget = await _budgetRepository.GetByDateIncludingDeletedAsync(userId, month, year);

                if (existingBudget == null)
                {
                    throw new NotFoundException("year/month");
                }

                List<Group> groupsToAdd = new List<Group>();
                List<Item> itemsToAdd = new List<Item>();

                foreach (var updatedGroup in updatedGroups)
                {
                    Group? existingGroup = existingBudget.Groups.FirstOrDefault(g => g.GroupId == updatedGroup.GroupId);
                    if (existingGroup == null)
                    {
                        groupsToAdd.Add(BudgetMapper.ToEntity(updatedGroup));
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
                                itemsToAdd.Add(BudgetMapper.ToEntity(updatedItem));
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

                if (groupsToAdd.Any())
                {
                    await _budgetRepository.CreateGroupsAsync(groupsToAdd);
                }

                if (itemsToAdd.Any())
                {
                    await _budgetRepository.CreateItemsAsync(itemsToAdd);
                }

                existingBudget.Income = updatedIncome ?? 0;
                existingBudget.ModifiedAt = DateTime.UtcNow;

                await _budgetRepository.UpdateAsync(existingBudget);
            }
        }

        public async Task DeleteAsync(Guid userId, int year, int month)
        {
            using (_logger.BeginLoggingScope(nameof(BudgetAppService), nameof(DeleteAsync)))
            {
                Budget? existingBudget = await _budgetRepository.GetByDateAsync(userId, month, year);

                if (existingBudget == null)
                {
                    throw new NotFoundException("year/month");
                }

                existingBudget.IsDeleted = true;
                existingBudget.ModifiedAt = DateTime.UtcNow;

                await _budgetRepository.UpdateAsync(existingBudget);
            }
        }

    }
}
