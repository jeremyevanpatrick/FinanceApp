using FinanceApp2.Api.Data.Context;
using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.Queues;

namespace FinanceApp2.Api.Services.Background
{
    public class ErrorLoggingService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogProcessorQueue _queue;

        private readonly int MaxBatchSize = 50;
        private readonly TimeSpan MaxBatchWait = TimeSpan.FromSeconds(5);

        public ErrorLoggingService(IServiceProvider services, ILogProcessorQueue queue)
        {
            _services = services;
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var batch = new List<ApplicationLog>(MaxBatchSize);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (await _queue.Reader.WaitToReadAsync(stoppingToken))
                {
                    while (_queue.Reader.TryRead(out var item))
                    {
                        batch.Add(item);

                        if (batch.Count >= MaxBatchSize)
                        {
                            await FlushBatch(batch);
                            batch.Clear();
                        }
                    }

                    if (batch.Count > 0)
                    {
                        await Task.Delay(MaxBatchWait, stoppingToken);
                        await FlushBatch(batch);
                        batch.Clear();
                    }
                }
            }
        }

        private async Task FlushBatch(List<ApplicationLog> batch)
        {
            if (batch.Count == 0) return;

            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();

                db.ApplicationLogs.AddRange(batch);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }
}
