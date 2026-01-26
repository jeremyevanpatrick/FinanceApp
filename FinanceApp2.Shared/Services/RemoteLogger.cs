using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace FinanceApp2.Shared.Services;

public class RemoteLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IServiceProvider _serviceProvider;

    public RemoteLogger(string categoryName, IServiceProvider serviceProvider)
    {
        _categoryName = categoryName;
        _serviceProvider = serviceProvider;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        try
        {
            var message = formatter(state, exception);

            using var scope = _serviceProvider.CreateScope();

            var remoteLoggingSettings = scope.ServiceProvider.GetRequiredService<IOptions<RemoteLoggingSettings>>().Value;

            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            var httpClient = httpClientFactory.CreateClient();

            Error error = new Error
            {
                ApplicationName = remoteLoggingSettings.ApplicationName,
                ErrorCode = eventId.Name ?? string.Empty,
                ErrorMessage = message,
                Notes = exception?.ToString()
            };

            httpClient
                .PostAsJsonAsync(remoteLoggingSettings.Endpoint, error)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
