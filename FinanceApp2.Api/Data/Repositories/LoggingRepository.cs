using FinanceApp2.Api.Data.Context;
using Microsoft.EntityFrameworkCore;
using FinanceApp2.Shared.Extensions;

namespace FinanceApp2.Api.Data.Repositories
{
    public class LoggingRepository : ILoggingRepository
    {
        private readonly ILogger<LoggingRepository> _logger;
        private readonly LoggingDbContext _db;

        public LoggingRepository(
            ILogger<LoggingRepository> logger,
            LoggingDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task DeleteInfoLogsByDaysAsync(int purgeAfterDays)
        {
            using (_logger.BeginLoggingScope(nameof(LoggingRepository), nameof(DeleteInfoLogsByDaysAsync)))
            {
                string logLevelString = LogLevel.Information.ToString();
                DateTime purgeDate = DateTime.UtcNow.AddDays(-purgeAfterDays);
                await _db.ApplicationLogs
                    .Where(t =>
                        t.Level == logLevelString &&
                        t.Timestamp < purgeDate)
                    .ExecuteDeleteAsync();
            }
        }

        public async Task DeleteErrorLogsByDaysAsync(int purgeAfterDays)
        {
            using (_logger.BeginLoggingScope(nameof(LoggingRepository), nameof(DeleteErrorLogsByDaysAsync)))
            {
                string logLevelString = LogLevel.Error.ToString();
                DateTime purgeDate = DateTime.UtcNow.AddDays(-purgeAfterDays);
                await _db.ApplicationLogs
                    .Where(t =>
                        t.Level == logLevelString &&
                        t.Timestamp < purgeDate)
                    .ExecuteDeleteAsync();
            }
        }

    }
}
