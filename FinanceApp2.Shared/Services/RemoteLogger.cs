using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.Queues;
using FinanceApp2.Shared.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp2.Shared.Services;

public class RemoteLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IServiceProvider _serviceProvider;
    private readonly IExternalScopeProvider _scopeProvider;

    public RemoteLogger(string categoryName, IServiceProvider serviceProvider)
    {
        _categoryName = categoryName;
        _serviceProvider = serviceProvider;
        _scopeProvider = serviceProvider.GetRequiredService<IExternalScopeProvider>();
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return _scopeProvider.Push(state);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();

            var remoteLoggingSettings = scope.ServiceProvider.GetRequiredService<IOptions<RemoteLoggingSettings>>().Value;

            var logProcessorQueue = scope.ServiceProvider.GetRequiredService<ILogProcessorQueue>();

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
                ApplicationName = remoteLoggingSettings.ApplicationName,
                ErrorCode = errorCode,
                Message = message,
                MessageTemplate = messageTemplate,
                Exception = exception?.ToString(),
                CorrelationId = correlationId
            };

            logProcessorQueue.Enqueue(applicationLog);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
