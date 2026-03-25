using FinanceApp2.Api.Data.Repositories;
using FinanceApp2.Api.Models;
using FinanceApp2.Api.Services.Background;
using FinanceApp2.Api.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FinanceApp2.Api.Tests.Services
{
    public class DataCleanupServiceTests
    {
        private IServiceScopeFactory CreateScopeFactory(IServiceProvider provider)
        {
            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider).Returns(provider);

            var scopeFactory = new Mock<IServiceScopeFactory>();
            scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

            return scopeFactory.Object;
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                new List<IUserValidator<ApplicationUser>>(),
                new List<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_WhenValid_CompletesSuccessfully()
        {
            // Arrange
            var olderThanDays = 30;
            var testId1 = Guid.NewGuid();
            var testId2 = Guid.NewGuid();
            var softDeletedUsers = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = testId1.ToString(),
                    IsDeleted = true,
                    DeletedAt = DateTime.UtcNow.AddDays(-60)
                },
                new ApplicationUser
                {
                    Id = testId2.ToString(),
                    IsDeleted = true,
                    DeletedAt = DateTime.UtcNow.AddDays(-65)
                }
            };

            var authRepoMock = new Mock<IAuthRepository>();
            authRepoMock.Setup(x => x.DeleteRefreshTokensExpiredByDaysAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            authRepoMock.Setup(x => x.GetSoftDeletedUsersAsync(It.IsAny<int>()))
                .ReturnsAsync(softDeletedUsers);

            var budgetRepoMock = new Mock<IBudgetRepository>();
            budgetRepoMock.Setup(x => x.DeleteUserDataAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            budgetRepoMock.Setup(x => x.CleanupSoftDeletedUserDataAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var userManagerMock = CreateUserManager();
            userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var mockScopeProvider = new Mock<IServiceProvider>();
            mockScopeProvider.Setup(x => x.GetService(typeof(IAuthRepository)))
                .Returns(authRepoMock.Object);
            mockScopeProvider.Setup(x => x.GetService(typeof(IBudgetRepository)))
                .Returns(budgetRepoMock.Object);
            mockScopeProvider.Setup(x => x.GetService(typeof(UserManager<ApplicationUser>)))
                .Returns(userManagerMock.Object);

            var mockScopeFactory = CreateScopeFactory(mockScopeProvider.Object);

            var dataCleanupSettings = Options.Create(new DataCleanupSettings
            {
                ScheduledHour = DateTime.UtcNow.Hour,
                OlderThanDays = olderThanDays
            });
            
            var service = new DataCleanupService(mockScopeFactory, Mock.Of<ILogger<DataCleanupService>>(), dataCleanupSettings);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(100);

            // Act
            await service.StartAsync(cts.Token);
            await Task.Delay(50);

            // Assert
            authRepoMock.Verify(
                x => x.DeleteRefreshTokensExpiredByDaysAsync(olderThanDays),
                Times.Once);

            budgetRepoMock.Verify(
                x => x.DeleteUserDataAsync(testId1),
                Times.Once);
            budgetRepoMock.Verify(
                x => x.DeleteUserDataAsync(testId2),
                Times.Once);

            budgetRepoMock.Verify(
                x => x.CleanupSoftDeletedUserDataAsync(olderThanDays),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenDatabaseIsUnreachable_DoesNothing()
        {
            // Arrange
            var authRepoMock = new Mock<IAuthRepository>();
            authRepoMock.Setup(x => x.DeleteRefreshTokensExpiredByDaysAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("test"));
            authRepoMock.Setup(x => x.GetSoftDeletedUsersAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("test"));

            var budgetRepoMock = new Mock<IBudgetRepository>();
            budgetRepoMock.Setup(x => x.CleanupSoftDeletedUserDataAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("test"));

            var mockScopeProvider = new Mock<IServiceProvider>();
            mockScopeProvider.Setup(x => x.GetService(typeof(IAuthRepository)))
                .Returns(authRepoMock.Object);
            mockScopeProvider.Setup(x => x.GetService(typeof(IBudgetRepository)))
                .Returns(budgetRepoMock.Object);

            var mockScopeFactory = CreateScopeFactory(mockScopeProvider.Object);

            var dataCleanupSettings = Options.Create(new DataCleanupSettings
            {
                ScheduledHour = DateTime.UtcNow.Hour,
                OlderThanDays = 30
            });

            var mockLogger = new Mock<ILogger<DataCleanupService>>();

            var service = new DataCleanupService(mockScopeFactory, mockLogger.Object, dataCleanupSettings);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(100);

            // Act
            Func<Task> result = () => service.StartAsync(cts.Token);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
        }

    }
}