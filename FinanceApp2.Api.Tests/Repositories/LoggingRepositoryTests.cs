using FinanceApp2.Api.Data.Context;
using FinanceApp2.Api.Data.Repositories;
using FinanceApp2.Api.Tests.Helpers;
using FinanceApp2.Shared.Models;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp2.Api.Tests.Repositories
{
    public class LoggingRepositoryTests
    {
        private LoggingDbContextTest CreateSqLiteDb()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<LoggingDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new LoggingDbContextTest(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task DeleteInfoLogsByDaysAsync_WhenLogsExist_DeletesInfoLogs()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var logsList = new List<ApplicationLog> {
                new ApplicationLog
                {
                    Level = LogLevel.Information.ToString(),
                    ServerName = Environment.MachineName,
                    ApplicationName = "TestAppName",
                    ErrorCode = null,
                    Message = "Test Info message",
                    MessageTemplate = null,
                    Exception = null,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow.AddDays(-2)
                },
                new ApplicationLog
                {
                    Level = LogLevel.Error.ToString(),
                    ServerName = Environment.MachineName,
                    ApplicationName = "TestAppName",
                    ErrorCode = "Test-123",
                    Message = "Test Error message",
                    MessageTemplate = null,
                    Exception = null,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow.AddDays(-2)
                },
                new ApplicationLog
                {
                    Level = LogLevel.Information.ToString(),
                    ServerName = Environment.MachineName,
                    ApplicationName = "TestAppName",
                    ErrorCode = null,
                    Message = "Test Info message",
                    MessageTemplate = null,
                    Exception = null,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            };

            db.ApplicationLogs.AddRange(logsList);

            await db.SaveChangesAsync();

            var repo = new LoggingRepository(Mock.Of<ILogger<LoggingRepository>>(), db);

            // Act
            await repo.DeleteInfoLogsByDaysAsync(1);

            // Assert
            var updatedLogList = await db.ApplicationLogs
                .AsNoTracking()
                .ToListAsync();
            updatedLogList!.Count.Should().Be(2);
            updatedLogList.Should().NotContain(t => t.Timestamp < DateTime.UtcNow.AddDays(-1) && t.Level == LogLevel.Information.ToString());
        }

        [Fact]
        public async Task DeleteErrorLogsByDaysAsync_WhenLogsExist_DeletesErrorLogs()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var logsList = new List<ApplicationLog> {
                new ApplicationLog
                {
                    Level = LogLevel.Information.ToString(),
                    ServerName = Environment.MachineName,
                    ApplicationName = "TestAppName",
                    ErrorCode = null,
                    Message = "Test Info message",
                    MessageTemplate = null,
                    Exception = null,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow.AddDays(-8)
                },
                new ApplicationLog
                {
                    Level = LogLevel.Error.ToString(),
                    ServerName = Environment.MachineName,
                    ApplicationName = "TestAppName",
                    ErrorCode = "Test-123",
                    Message = "Test Error message",
                    MessageTemplate = null,
                    Exception = null,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow.AddDays(-8)
                },
                new ApplicationLog
                {
                    Level = LogLevel.Error.ToString(),
                    ServerName = Environment.MachineName,
                    ApplicationName = "TestAppName",
                    ErrorCode = "Test-123",
                    Message = "Test Error message",
                    MessageTemplate = null,
                    Exception = null,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            };

            db.ApplicationLogs.AddRange(logsList);

            await db.SaveChangesAsync();

            var repo = new LoggingRepository(Mock.Of<ILogger<LoggingRepository>>(), db);

            // Act
            await repo.DeleteErrorLogsByDaysAsync(7);

            // Assert
            var updatedLogList = await db.ApplicationLogs
                .AsNoTracking()
                .ToListAsync();
            updatedLogList!.Count.Should().Be(2);
            updatedLogList.Should().NotContain(t => t.Timestamp < DateTime.UtcNow.AddDays(-7) && t.Level == LogLevel.Error.ToString());
        }
    }
}
