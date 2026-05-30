using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.Queues;
using FinanceApp2.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp2.Shared.Services;

public class RemoteLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IOptionsMonitor<RemoteLoggingSettings> _remoteLoggingSettings;
    private readonly IExternalScopeProvider _scopeProvider;
    private readonly ILogProcessorQueue _logProcessorQueue;

    public RemoteLogger(
        string categoryName,
        IOptionsMonitor<RemoteLoggingSettings> remoteLoggingSettings,
        IExternalScopeProvider scopeProvider,
        ILogProcessorQueue logProcessorQueue)
    {
        _categoryName = categoryName;
        _remoteLoggingSettings = remoteLoggingSettings;
        _scopeProvider = scopeProvider;
        _logProcessorQueue = logProcessorQueue;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return _scopeProvider.Push(state);
    }

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel >= LogLevel.Information &&
        (logLevel >= LogLevel.Error || _remoteLoggingSettings.CurrentValue.EnableRunningLogs);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        try
        {
            string? className = null;
            string? methodName = null;
            string? correlationId = null;
            _scopeProvider.ForEachScope((scope, _) =>
            {
                if (scope is LoggingScopeState lsState)
                {
                    className = lsState.ClassName ?? className;
                    methodName = lsState.MethodName ?? methodName;
                    correlationId = lsState.CorrelationId ?? correlationId;
                }
            }, state);

            string message = formatter(state, null);

            if (!string.IsNullOrWhiteSpace(className) && !string.IsNullOrWhiteSpace(methodName))
            {
                message = $"{className}.{methodName}: {message}";
            }

            string? errorCode = null;
            string? messageTemplate = null;
            if (state is IReadOnlyList<KeyValuePair<string, object?>> properties)
            {
                errorCode = properties
                    .FirstOrDefault(p => p.Key == "ErrorCode")
                    .Value?.ToString();

                messageTemplate = properties
                    .FirstOrDefault(p => p.Key == "{OriginalFormat}")
                    .Value?.ToString();
            }

            ApplicationLog applicationLog = new ApplicationLog
            {
                Level = logLevel.ToString(),
                ServerName = Environment.MachineName,
                ApplicationName = _remoteLoggingSettings.CurrentValue.ApplicationName,
                ErrorCode = errorCode,
                Message = message,
                MessageTemplate = messageTemplate,
                Exception = exception?.ToString(),
                CorrelationId = correlationId
            };

            _logProcessorQueue.Enqueue(applicationLog);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}