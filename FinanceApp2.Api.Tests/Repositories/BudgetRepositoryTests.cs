using FinanceApp2.Api.Data.Context;
using FinanceApp2.Api.Data.Repositories;
using FinanceApp2.Api.Models;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp2.Api.Tests.Repositories
{
    public class BudgetRepositoryTests
    {
        private AppDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private AppDbContext CreateSqLiteDb()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetByDateAsync_WhenUserHasBudget_ReturnsBudget()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();
            var selectedMonth = 3;
            var selectedYear = 2026;

            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 1,
                Year = 2026
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Month = 2,
                Year = 2026
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 3,
                Year = 2026
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 3,
                Year = 2026,
                IsDeleted = true
            });

            await db.SaveChangesAsync();

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            var result = await repo.GetByDateAsync(userId, selectedMonth, selectedYear);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Month.Should().Be(selectedMonth);
            result.Year.Should().Be(selectedYear);
            result.IsDeleted.Should().Be(false);
        }

        [Fact]
        public async Task GetByDateIncludingDeletedAsync_WhenUserHasBudget_ReturnsBudget()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();
            var selectedMonth = 1;
            var selectedYear = 2026;

            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 1,
                Year = 2026,
                Groups = new List<Group>()
                {
                    new Group()
                    {
                        GroupId = Guid.NewGuid(),
                        GroupName = "Group 1",
                        BudgetId = Guid.NewGuid(),
                        Order = 1,
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = Guid.NewGuid(),
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Item 1",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2),
                                IsDeleted = true
                            },
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = Guid.NewGuid(),
                                Budgeted = 100,
                                Spent = 100,
                                ItemName = "Item 2",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        },
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        ModifiedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new Group()
                    {
                        GroupId = Guid.NewGuid(),
                        GroupName = "Group 2",
                        BudgetId = Guid.NewGuid(),
                        Order = 1,
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = Guid.NewGuid(),
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Item 3",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            },
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = Guid.NewGuid(),
                                Budgeted = 100,
                                Spent = 100,
                                ItemName = "Item 4",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        },
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        ModifiedAt = DateTime.UtcNow.AddDays(-2),
                        IsDeleted = true
                    }
                }
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Month = 2,
                Year = 2026
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 3,
                Year = 2026
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 1,
                Year = 2026,
                IsDeleted = true
            });

            await db.SaveChangesAsync();

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            var result = await repo.GetByDateIncludingDeletedAsync(userId, selectedMonth, selectedYear);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Month.Should().Be(selectedMonth);
            result.Year.Should().Be(selectedYear);
            result.IsDeleted.Should().Be(false);
            result.Groups.Should().HaveCount(2);
            var resultGroup1 = result.Groups[0];
            resultGroup1.Items.Should().HaveCount(2);
            resultGroup1.Items.Should().Contain(i => i.ItemName == "Item 1" && i.IsDeleted);
            var resultGroup2 = result.Groups[1];
            resultGroup2.Items.Should().HaveCount(2);
            resultGroup2.IsDeleted.Should().Be(true);
        }

        [Fact]
        public async Task GetExistsByDateAsync_WhenUserHasBudget_ReturnsTrue()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();
            var selectedMonth = 3;
            var selectedYear = 2026;

            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = selectedMonth,
                Year = selectedYear
            });

            await db.SaveChangesAsync();

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            var result = await repo.GetExistsByDateAsync(userId, selectedMonth, selectedYear);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public async Task GetExistsByDateAsync_WhenUserDoesNotHaveBudget_ReturnsFalse()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();
            var selectedMonth = 3;
            var selectedYear = 2026;

            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 1,
                Year = 2026
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Month = 3,
                Year = 2026
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 3,
                Year = 2026,
                IsDeleted = true
            });

            await db.SaveChangesAsync();

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            var result = await repo.GetExistsByDateAsync(userId, selectedMonth, selectedYear);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public async Task CreateAsync_WhenUniqueBudget_CreatesBudget()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var selectedMonth = 3;
            var selectedYear = 2026;

            var budget = new Budget()
            {
                BudgetId = budgetId,
                UserId = userId,
                Month = selectedMonth,
                Year = selectedYear
            };

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            await repo.CreateAsync(budget);

            // Assert
            var createdBudget = await db.Budgets
                .AsNoTracking()
                .FirstOrDefaultAsync();
            createdBudget.Should().NotBeNull();
            createdBudget!.BudgetId.Should().Be(budgetId);
            createdBudget.UserId.Should().Be(userId);
            createdBudget.Month.Should().Be(selectedMonth);
            createdBudget.Year.Should().Be(selectedYear);
        }

        [Fact]
        public async Task CreateAsync_WhenDuplicateBudget_ThrowsException()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var selectedMonth = 3;
            var selectedYear = 2026;

            var budget = new Budget()
            {
                BudgetId = budgetId,
                UserId = userId,
                Month = selectedMonth,
                Year = selectedYear
            };

            db.Budgets.Add(budget);

            await db.SaveChangesAsync();

            var duplicateBudget = new Budget()
            {
                BudgetId = budgetId,
                UserId = userId,
                Month = selectedMonth,
                Year = selectedYear
            };

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            Func<Task> result = () => repo.CreateAsync(duplicateBudget);

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task UpdateAsync_WhenBudgetExists_UpdatesBudget()
        {
            // Arrange
            var db = CreateDb();

            var existingArticleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var groupId = Guid.NewGuid();

            var budget = new Budget()
            {
                UserId = userId,
                BudgetId = budgetId, 
                Income = 1500,
                Month = 3,
                Year = 2026,
                Groups = new List<Group>()
                {
                    new Group()
                    {
                        GroupId = groupId,
                        GroupName = "Group 1",
                        BudgetId = budgetId,
                        Order = 1,
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = groupId,
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Item 1",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            },
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = groupId,
                                Budgeted = 100,
                                Spent = 100,
                                ItemName = "Item 2",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        },
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        ModifiedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new Group()
                    {
                        GroupId = Guid.NewGuid(),
                        GroupName = "Group 2",
                        BudgetId = budgetId,
                        Order = 2,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        ModifiedAt = DateTime.UtcNow.AddDays(-2)
                    }
                },
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                ModifiedAt = DateTime.UtcNow.AddDays(-2)
            };

            db.Budgets.Add(budget);

            await db.SaveChangesAsync();

            db.Entry(budget).State = EntityState.Detached;

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            var newLastChecked = DateTime.UtcNow;

            var budgetToUpdate = await db.Budgets
                .Include(f => f.Groups.OrderBy(g => g.Order))
                .ThenInclude(g => g.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            budgetToUpdate!.Income = 2000;
            budgetToUpdate.ModifiedAt = DateTime.UtcNow;
            
            var group1ToUpdate = budgetToUpdate.Groups[0];
            group1ToUpdate.GroupName = "Updated Group 1";
            group1ToUpdate.Order = 2;
            group1ToUpdate.ModifiedAt = DateTime.UtcNow;

            var group1Item1 = group1ToUpdate.Items[0];
            group1Item1.ItemName = "Updated Item 1";
            group1Item1.Budgeted = 1200;
            group1Item1.Spent = 1100;
            group1Item1.ModifiedAt = DateTime.UtcNow;

            var group1Item2 = group1ToUpdate.Items[1];
            group1Item2.IsDeleted = true;
            group1Item2.ModifiedAt = DateTime.UtcNow;

            var group2ToUpdate = budgetToUpdate.Groups[1];
            group2ToUpdate.IsDeleted = true;
            group2ToUpdate.ModifiedAt = DateTime.UtcNow;

            await repo.UpdateAsync(budgetToUpdate);

            // Assert
            var updatedBudgetResult = await db.Budgets
                .Include(b => b.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                .ThenInclude(g => g.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt))
                .AsNoTracking()
                .FirstOrDefaultAsync();
            updatedBudgetResult.Should().NotBeNull();
            updatedBudgetResult!.BudgetId.Should().Be(budgetId);
            updatedBudgetResult.Income.Should().Be(2000);
            updatedBudgetResult.Groups.Should().HaveCount(1);
            var updatedGroupResult = updatedBudgetResult.Groups.First();
            updatedGroupResult.Order.Should().Be(2);
            updatedGroupResult.Items.Should().HaveCount(1);
            var updatedItemResult = updatedGroupResult.Items.First();
            updatedItemResult.Budgeted.Should().Be(1200);
            updatedItemResult.Spent.Should().Be(1100);

        }

        [Fact]
        public async Task CreateGroupsAsync_WhenUniqueGroups_CreatesGroups()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var groupId1 = Guid.NewGuid();
            var groupId2 = Guid.NewGuid();

            var groups = new List<Group>()
            {
                new Group()
                {
                    BudgetId = budgetId,
                    GroupId = groupId1,
                    GroupName = "Group 1",
                    Order = 1,
                    Items = new List<Item>()
                    {
                        new Item()
                        {
                            ItemId = Guid.NewGuid(),
                            GroupId = groupId1,
                            Budgeted = 1000,
                            Spent = 1000,
                            ItemName = "Item 1",
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            ModifiedAt = DateTime.UtcNow.AddDays(-2)
                        },
                        new Item()
                        {
                            ItemId = Guid.NewGuid(),
                            GroupId = groupId1,
                            Budgeted = 100,
                            Spent = 100,
                            ItemName = "Item 2",
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            ModifiedAt = DateTime.UtcNow.AddDays(-2)
                        }
                    },
                },
                new Group()
                {
                    BudgetId = budgetId,
                    GroupId = groupId2,
                    GroupName = "Group 2",
                    Order = 2,
                    Items = new List<Item>()
                    {
                        new Item()
                        {
                            ItemId = Guid.NewGuid(),
                            GroupId = groupId2,
                            Budgeted = 1000,
                            Spent = 1000,
                            ItemName = "Item 1",
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            ModifiedAt = DateTime.UtcNow.AddDays(-2)
                        },
                        new Item()
                        {
                            ItemId = Guid.NewGuid(),
                            GroupId = groupId2,
                            Budgeted = 100,
                            Spent = 100,
                            ItemName = "Item 2",
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            ModifiedAt = DateTime.UtcNow.AddDays(-2)
                        }
                    }
                }
            };

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            await repo.CreateGroupsAsync(groups);

            // Assert
            var createdGroups = await db.Groups
                .OrderBy(g => g.Order)
                .Include(g => g.Items)
                .AsNoTracking()
                .ToListAsync();
            createdGroups!.Should().NotBeNull();
            createdGroups.Should().HaveCount(2);
            createdGroups.Should().OnlyContain(g => g.BudgetId == budgetId);
            var createdItems = createdGroups.First().Items;
            createdItems.Should().HaveCount(2);
            createdItems.Should().OnlyContain(g => g.GroupId == groupId1);
        }

        [Fact]
        public async Task CreateGroupsAsync_WhenDuplicateGroup_ThrowsException()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var groupId1 = Guid.NewGuid();
            var groupId2 = Guid.NewGuid();

            var groups = new List<Group>()
            {
                new Group()
                {
                    BudgetId = budgetId,
                    GroupId = groupId1,
                    GroupName = "Group 1",
                    Order = 1
                },
                new Group()
                {
                    BudgetId = budgetId,
                    GroupId = groupId2,
                    GroupName = "Group 2",
                    Order = 2
                }
            };

            db.Groups.AddRange(groups);

            await db.SaveChangesAsync();

            var duplicateGroup = new List<Group>()
            {
                new Group()
                {
                    BudgetId = budgetId,
                    GroupId = groupId1,
                    GroupName = "Group 1",
                    Order = 1
                }
            };

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            Func<Task> result = () => repo.CreateGroupsAsync(duplicateGroup);

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task CreateItemsAsync_WhenUniqueItems_CreatesItem()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();

            var items = new List<Item>()
            {
                new Item
                {
                    GroupId = groupId,
                    ItemId = Guid.NewGuid(),
                    Budgeted = 300,
                    Spent = 250,
                    ItemName = "Item 1"
                },
                new Item
                {
                    GroupId = groupId,
                    ItemId = Guid.NewGuid(),
                    Budgeted = 300,
                    Spent = 250,
                    ItemName = "Item 2"
                }
            };

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            await repo.CreateItemsAsync(items);

            // Assert
            var createdItems = await db.Items
                .AsNoTracking()
                .ToListAsync();
            createdItems.Should().HaveCount(2);
            createdItems.Should().OnlyContain(g => g.GroupId == groupId);
        }

        [Fact]
        public async Task CreateItemsAsync_WhenDuplicateItem_ThrowsException()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var duplicateItemId = Guid.NewGuid();

            var budget = new Budget()
            {
                UserId = userId,
                BudgetId = budgetId,
                Income = 1500,
                Month = 3,
                Year = 2026,
                Groups = new List<Group>()
                {
                    new Group()
                    {
                        GroupId = groupId,
                        GroupName = "Group 1",
                        BudgetId = budgetId,
                        Order = 1,
                        Items = new List<Item>()
                        {
                            new Item
                            {
                                GroupId = groupId,
                                ItemId = duplicateItemId,
                                Budgeted = 300,
                                Spent = 250,
                                ItemName = "Item 1"
                            },
                            new Item
                            {
                                GroupId = groupId,
                                ItemId = Guid.NewGuid(),
                                Budgeted = 300,
                                Spent = 250,
                                ItemName = "Item 2"
                            }
                        },
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    }
                },
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            db.Budgets.Add(budget);

            await db.SaveChangesAsync();

            var duplicateItem = new List<Item>()
            {
                new Item
                {
                    GroupId = groupId,
                    ItemId = duplicateItemId,
                    Budgeted = 300,
                    Spent = 250,
                    ItemName = "Item 1"
                }
            };

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            Func<Task> result = () => repo.CreateItemsAsync(duplicateItem);

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task DeleteUserDataAsync_WhenUserHasBudgets_DeletesBudgets()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var userId = Guid.NewGuid();
            var budgetId1 = Guid.NewGuid();
            var budgetId2 = Guid.NewGuid();
            var groupId1 = Guid.NewGuid();
            var groupId2 = Guid.NewGuid();
            var groupId3 = Guid.NewGuid();

            var budget1 = new Budget()
            {
                BudgetId = budgetId1,
                Income = 1500,
                UserId = userId,
                Year = 2026,
                Month = 3,
                Groups = new List<Group>()
                {
                    new Group()
                    {
                        BudgetId = budgetId1,
                        GroupId = groupId1,
                        GroupName = "Group 1",
                        Order = 1,
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = groupId1,
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Item 1",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            },
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = groupId1,
                                Budgeted = 100,
                                Spent = 100,
                                ItemName = "Item 2",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        },
                    },
                    new Group()
                    {
                        BudgetId = budgetId1,
                        GroupId = groupId2,
                        GroupName = "Group 2",
                        Order = 2,
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = groupId2,
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Item 1",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            },
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = groupId2,
                                Budgeted = 100,
                                Spent = 100,
                                ItemName = "Item 2",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        }
                    }
                }
            };

            db.Budgets.Add(budget1);

            var budget2 = new Budget()
            {
                BudgetId = budgetId2,
                Income = 1300,
                UserId = Guid.NewGuid(),
                Year = 2026,
                Month = 4,
                Groups = new List<Group>()
                {
                    new Group()
                    {
                        BudgetId = budgetId1,
                        GroupId = groupId3,
                        GroupName = "Group 3",
                        Order = 1,
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = groupId3,
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Item 1",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        }
                    }
                }
            };

            db.Budgets.Add(budget2);

            await db.SaveChangesAsync();

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            await repo.DeleteUserDataAsync(userId);

            // Assert
            var budgets = await db.Budgets
                .AsNoTracking()
                .ToListAsync();
            budgets.Should().HaveCount(1);
            budgets.Should().NotContain(f => f.UserId == userId);

            var groups = await db.Groups
                .Include(g => g.Budget)
                .AsNoTracking()
                .ToListAsync();
            groups.Should().HaveCount(1);
            groups.Should().NotContain(g => g.Budget.UserId == userId);

            var items = await db.Items
                .Include(i => i.Group)
                .ThenInclude(g => g.Budget)
                .AsNoTracking()
                .ToListAsync();
            items.Should().HaveCount(1);
            items.Should().NotContain(i => i.Group.Budget.UserId == userId);

        }

        [Fact]
        public async Task DeleteUserDataAsync_WhenUserHasNoBudgets_DoesNothing()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();

            var budget = new Budget()
            {
                BudgetId = budgetId,
                Income = 1500,
                UserId = userId,
                Year = 2026,
                Month = 3
            };

            db.Budgets.Add(budget);

            await db.SaveChangesAsync();

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            Func<Task> result = () => repo.DeleteUserDataAsync(Guid.NewGuid());

            // Assert
            var budgets = await db.Budgets
                .AsNoTracking()
                .ToListAsync();
            budgets.Should().HaveCount(1);

        }

        [Fact]
        public async Task CleanupSoftDeletedUserDataAsync_WhenSoftDeletedUserDataExists_DeletesSoftDeletedUserData()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var groupId1 = Guid.NewGuid();
            var groupId2 = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 1,
                Year = 2026,
                IsDeleted = true,
                ModifiedAt = DateTime.UtcNow.AddDays(-10)
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Month = 2,
                Year = 2026,
                IsDeleted = true,
                ModifiedAt = DateTime.UtcNow
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = budgetId,
                UserId = Guid.NewGuid(),
                Month = 3,
                Year = 2026,
                ModifiedAt = DateTime.UtcNow.AddDays(-10),
                Groups = new List<Group>()
                {
                    new Group()
                    {
                        BudgetId = budgetId,
                        GroupId = groupId1,
                        GroupName = "Group 1",
                        Order = 1,
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = groupId1,
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Item 1",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            },
                            new Item()
                            {
                                ItemId = itemId,
                                GroupId = groupId1,
                                Budgeted = 100,
                                Spent = 100,
                                ItemName = "Item 2",
                                CreatedAt = DateTime.UtcNow.AddDays(-10),
                                IsDeleted = true,
                                ModifiedAt = DateTime.UtcNow.AddDays(-10)
                            }
                        }
                    },
                    new Group()
                    {
                        BudgetId = budgetId,
                        GroupId = groupId2,
                        GroupName = "Group 2",
                        Order = 2,
                        IsDeleted = true,
                        ModifiedAt = DateTime.UtcNow.AddDays(-10),
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = groupId2,
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Item 1",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        }
                    }
                }
            });
            db.Budgets.Add(new Budget()
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = 3,
                Year = 2026,
                IsDeleted = true,
                ModifiedAt = DateTime.UtcNow.AddDays(-10),
                Groups = new List<Group>()
                {
                    new Group()
                    {
                        BudgetId = budgetId,
                        GroupId = Guid.NewGuid(),
                        GroupName = "Group 3",
                        Order = 1
                    }
                }
            });

            await db.SaveChangesAsync();

            var repo = new BudgetRepository(Mock.Of<ILogger<BudgetRepository>>(), db);

            // Act
            await repo.CleanupSoftDeletedUserDataAsync(1);

            // Assert
            var resultBudgets = await db.Budgets
                .AsNoTracking()
                .ToListAsync();
            resultBudgets.Should().HaveCount(2);
            resultBudgets.Should().NotContain(b => b.UserId == userId);

            var resultGroups = await db.Groups
                .Include(g => g.Budget)
                .AsNoTracking()
                .ToListAsync();
            resultGroups.Should().HaveCount(1);
            resultGroups.Should().NotContain(g => g.GroupId == groupId2 || g.Budget.UserId == userId);

            var resultItems = await db.Items
                .Include(i => i.Group)
                .ThenInclude(g => g.Budget)
                .AsNoTracking()
                .ToListAsync();
            resultItems.Should().HaveCount(1);
            resultItems.Should().NotContain(i => i.ItemId == itemId || i.Group.Budget.UserId == userId);

        }

    }
}
