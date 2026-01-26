using Microsoft.Extensions.Logging;

namespace FinanceApp2.Shared.Services;

public class RemoteLoggerProvider : ILoggerProvider
{
    private readonly IServiceProvider _serviceProvider;

    public RemoteLoggerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILogger CreateLogger(string categoryName) => new RemoteLogger(categoryName, _serviceProvider);

    public void Dispose() { }
}
