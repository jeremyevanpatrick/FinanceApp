using FinanceApp2.Api.Data.Context;
using FinanceApp2.Api.Services;

namespace FeedApp3.Api.Services.Background
{
    public class ErrorLoggingService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ErrorLogQueue _queue;

        public ErrorLoggingService(IServiceProvider services, ErrorLogQueue queue)
        {
            _services = services;
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var error in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();

                    db.Errors.Add(error);
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
