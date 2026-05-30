using FinanceApp2.Shared.Services.Queues;
using FinanceApp2.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp2.Shared.Services;

public class RemoteLoggerProvider : ILoggerProvider
{
    private readonly IOptionsMonitor<RemoteLoggingSettings> _remoteLoggingSettings;
    private readonly IExternalScopeProvider _scopeProvider;
    private readonly ILogProcessorQueue _logProcessorQueue;

    public RemoteLoggerProvider(
        IOptionsMonitor<RemoteLoggingSettings> remoteLoggingSettings,
        IExternalScopeProvider scopeProvider,
        ILogProcessorQueue logProcessorQueue)
    {
        _remoteLoggingSettings = remoteLoggingSettings;
        _scopeProvider = scopeProvider;
        _logProcessorQueue = logProcessorQueue;
    }

    public ILogger CreateLogger(string categoryName) => new RemoteLogger(categoryName, _remoteLoggingSettings, _scopeProvider, _logProcessorQueue);

    public void Dispose() { }
}
