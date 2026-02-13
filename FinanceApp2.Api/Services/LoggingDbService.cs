using FinanceApp2.Api.Data;
using FinanceApp2.Shared.Models;
using System.Threading.Channels;

namespace FinanceApp2.Api.Services
{
    public class LoggingDbService
    {
        private readonly Channel<Error> _channel;
        private readonly IServiceProvider _serviceProvider;

        public LoggingDbService(IServiceProvider serviceProvider)
        {
            _channel = Channel.CreateUnbounded<Error>();
            _serviceProvider = serviceProvider;

            _ = Task.Run(ProcessQueueAsync);
        }

        private async Task ProcessQueueAsync()
        {
            await foreach (var error in _channel.Reader.ReadAllAsync())
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    LoggingDbContext db = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();

                    db.Errors.Add(error);
                    await db.SaveChangesAsync();
                }
                catch {}
            }
        }

        public void LogError(Error error)
        {
            try
            {
                _channel.Writer.TryWrite(error);
            }
            catch {}
        }

    }

}
