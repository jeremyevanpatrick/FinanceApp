using FinanceApp2.Api.Data.Repositories;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Models;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Services.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp2.Api.Tests.Services
{
    public class BudgetAppServiceTests
    {
        [Fact]
        public async Task GetByDateAsync_WhenUserHasBudget_ReturnsBudget()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;

            var budget = new Budget() {
                UserId = Guid.NewGuid(),
                Month = selectedMonth,
                Year = selectedYear,
                Income = 5000,
                Groups = new List<Group>()
            };

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(budget);
            mockRepo.Setup(r => r.GetExistsByDateAsync(It.IsAny<Guid>(), selectedMonth - 1, It.IsAny<int>()))
                .ReturnsAsync(true);
            mockRepo.Setup(r => r.GetExistsByDateAsync(It.IsAny<Guid>(), selectedMonth + 1, It.IsAny<int>()))
                .ReturnsAsync(false);

            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            var result = await service.GetByDateAsync(Guid.NewGuid(), selectedMonth, selectedYear);

            // Assert
            result.Budget.Should().NotBeNull();
            result.HasPreviousMonth.Should().Be(true);
            result.HasNextMonth.Should().Be(false);
        }

        [Fact]
        public async Task GetByDateAsync_WhenNoBudgetFound_ReturnsWithoutBudget()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((Budget?)null);
            mockRepo.Setup(r => r.GetExistsByDateAsync(It.IsAny<Guid>(), selectedMonth - 1, It.IsAny<int>()))
                .ReturnsAsync(true);
            mockRepo.Setup(r => r.GetExistsByDateAsync(It.IsAny<Guid>(), selectedMonth + 1, It.IsAny<int>()))
                .ReturnsAsync(false);

            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            var result = await service.GetByDateAsync(Guid.NewGuid(), selectedMonth, selectedYear);

            // Assert
            result.Budget.Should().BeNull();
            result.HasPreviousMonth.Should().Be(true);
            result.HasNextMonth.Should().Be(false);
        }

        [Fact]
        public async Task CreateAsync_WhenValid_CreatesBudget()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((Budget?)null);
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<Budget>()))
                .Returns(Task.CompletedTask);

            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            var result = await service.CreateAsync(Guid.NewGuid(), selectedMonth, selectedYear, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Month.Should().Be(selectedMonth);
            result.Year.Should().Be(selectedYear);
            result.BudgetId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task CreateAsync_WhenValidClone_CreatesClonedBudget()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;
            var userId = Guid.NewGuid();
            var sourceBudgetId = Guid.NewGuid();
            var sourceGrouptId = Guid.NewGuid();
            var sourceMonth = 4;
            var sourceYear = 2024;

            var sourceBudget = new Budget()
            {
                BudgetId = sourceBudgetId,
                UserId = userId,
                Month = sourceMonth,
                Year = sourceYear,
                Income = 5000,
                Groups = new List<Group>()
                {
                    new Group()
                    {
                        GroupId = sourceGrouptId,
                        GroupName = "Group 1",
                        BudgetId = sourceBudgetId,
                        Order = 1,
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = sourceGrouptId,
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Item 1",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            },
                            new Item()
                            {
                                ItemId = Guid.NewGuid(),
                                GroupId = sourceGrouptId,
                                Budgeted = 100,
                                Spent = 100,
                                ItemName = "Item 1",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        },
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        ModifiedAt = DateTime.UtcNow.AddDays(-2)
                    }
                }
            };

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), selectedMonth, selectedYear))
                .ReturnsAsync((Budget?)null);
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), sourceMonth, sourceYear))
                .ReturnsAsync(sourceBudget);
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<Budget>()))
                .Returns(Task.CompletedTask);

            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            var result = await service.CreateAsync(userId, selectedMonth, selectedYear, sourceMonth, sourceYear);

            // Assert
            result.Should().NotBeNull();
            result.Month.Should().Be(selectedMonth);
            result.Year.Should().Be(selectedYear);
            result.BudgetId.Should().NotBe(Guid.Empty);
            result.Groups.Should().HaveCount(1);
            result.Groups.First().Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateAsync_WhenDuplicate_ThrowsException()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Budget()
                {
                    Month = selectedMonth,
                    Year = selectedYear
                });
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<Budget>()))
                .Returns(Task.CompletedTask);

            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            Func<Task> result = () => service.CreateAsync(Guid.NewGuid(), selectedMonth, selectedYear, null, null);

            // Assert
            await result.Should().ThrowAsync<BudgetConflictException>();
        }

        [Fact]
        public async Task CreateAsync_WhenInvalidClone_ThrowsException()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;
            var userId = Guid.NewGuid();
            var sourceMonth = 4;
            var sourceYear = 2024;

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), selectedMonth, selectedYear))
                .ReturnsAsync((Budget?)null);
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), sourceMonth, sourceYear))
                .ReturnsAsync((Budget?)null);

            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            Func<Task> result = () => service.CreateAsync(userId, selectedMonth, selectedYear, sourceMonth, sourceYear);

            // Assert
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenValid_UpdatesBudget()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;
            var updatedIncome = 125;
            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var grouptId1 = Guid.NewGuid();
            var grouptId2 = Guid.NewGuid();
            var itemId1 = Guid.NewGuid();

            var existingBudget = new Budget()
            {
                BudgetId = budgetId,
                UserId = userId,
                Month = selectedMonth,
                Year = selectedYear,
                Income = 5000,
                Groups = new List<Group>()
                {
                    new Group()
                    {
                        GroupId = grouptId1,
                        GroupName = "Existing Group before",
                        BudgetId = budgetId,
                        Order = 1,
                        Items = new List<Item>()
                        {
                            new Item()
                            {
                                ItemId = itemId1,
                                GroupId = grouptId1,
                                Budgeted = 1000,
                                Spent = 1000,
                                ItemName = "Existing Item before",
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                ModifiedAt = DateTime.UtcNow.AddDays(-2)
                            }
                        },
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        ModifiedAt = DateTime.UtcNow.AddDays(-2)
                    }
                }
            };

            var updatedGroups = new List<GroupDto>()
            {
                new GroupDto()
                {
                    GroupId = grouptId1,
                    GroupName = "Existing group after",
                    BudgetId = budgetId,
                    Order = 2,
                    Items = new List<ItemDto>()
                    {
                        new ItemDto()
                        {
                            ItemId = itemId1,
                            GroupId = grouptId1,
                            Budgeted = 120,
                            Spent = 120,
                            ItemName = "Existing Item after",
                            ModifiedAt = DateTime.UtcNow,
                            IsDeleted = true
                        },
                        new ItemDto()
                        {
                            ItemId = Guid.NewGuid(),
                            GroupId = grouptId1,
                            Budgeted = 100,
                            Spent = 100,
                            ItemName = "New Item",
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        }
                    },
                    ModifiedAt = DateTime.UtcNow
                },
                new GroupDto()
                {
                    GroupId = grouptId2,
                    GroupName = "New group",
                    BudgetId = budgetId,
                    Order = 1,
                    Items = new List<ItemDto>()
                    {
                        new ItemDto()
                        {
                            ItemId = Guid.NewGuid(),
                            GroupId = grouptId2,
                            Budgeted = 1000,
                            Spent = 1000,
                            ItemName = "Item 1",
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            ModifiedAt = DateTime.UtcNow
                        }
                    },
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                }
            };

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateIncludingDeletedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(existingBudget);
            List<Group>? groupsToAdd = null;
            mockRepo.Setup(r => r.CreateGroupsAsync(It.IsAny<List<Group>>()))
                .Callback<List<Group>>(groups => groupsToAdd = groups)
                .Returns(Task.CompletedTask);
            List<Item>? itemsToAdd = null;
            mockRepo.Setup(r => r.CreateItemsAsync(It.IsAny<List<Item>>()))
                .Callback<List<Item>>(items => itemsToAdd = items)
                .Returns(Task.CompletedTask);
            Budget? budgetToUpdate = null;
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Budget>()))
                .Callback<Budget>(b => budgetToUpdate = b)
                .Returns(Task.CompletedTask);

            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            Func<Task> result = () => service.UpdateAsync(userId, selectedMonth, selectedYear, updatedIncome, updatedGroups);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            budgetToUpdate.Groups.Should().HaveCount(1);
            budgetToUpdate.Groups.First().Items.Should().HaveCount(1);
            groupsToAdd.Should().HaveCount(1);
            itemsToAdd.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateAsync_WhenExistingNotFound_ThrowsException()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;
            var updatedIncome = 125;
            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var grouptId1 = Guid.NewGuid();

            var updatedGroups = new List<GroupDto>()
            {
                new GroupDto()
                {
                    GroupId = grouptId1,
                    GroupName = "Existing group after",
                    BudgetId = budgetId,
                    Order = 1,
                    Items = new List<ItemDto>(),
                    ModifiedAt = DateTime.UtcNow
                }
            };

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateIncludingDeletedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new NotFoundException("test"));

            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            Func<Task> result = () => service.UpdateAsync(userId, selectedMonth, selectedYear, updatedIncome, updatedGroups);

            // Assert
            await result.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeleteAsync_WhenValid_SoftDeletesBudget()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;

            var budget = new Budget()
            {
                UserId = Guid.NewGuid(),
                Month = selectedMonth,
                Year = selectedYear,
                Income = 5000,
                Groups = new List<Group>()
            };

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(budget);
            Budget? updatedBudget = null;
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Budget>()))
                .Callback<Budget>(b => updatedBudget = b)
                .Returns(Task.CompletedTask);

            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            Func<Task> result = () => service.DeleteAsync(Guid.NewGuid(), selectedMonth, selectedYear);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            updatedBudget.IsDeleted.Should().Be(true);
        }

        [Fact]
        public async Task DeleteAsync_WhenExistingNotFound_ThrowsException()
        {
            // Arrange
            var selectedMonth = 5;
            var selectedYear = 2024;

            var mockRepo = new Mock<IBudgetRepository>();
            mockRepo.Setup(r => r.GetByDateAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((Budget?)null);
            var service = new BudgetAppService(Mock.Of<ILogger<BudgetAppService>>(), mockRepo.Object);

            // Act
            Func<Task> result = () => service.DeleteAsync(Guid.NewGuid(), selectedMonth, selectedYear);

            // Assert
            await result.Should().ThrowAsync<NotFoundException>();
        }

    }
}