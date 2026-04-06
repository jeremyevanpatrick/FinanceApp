using FinanceApp2.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace FinanceApp2.Shared.Extensions
{
    public static class LoggerExtensions
    {
        public static LoggingScope BeginLoggingScope(this ILogger logger, string className, string methodName, string? correlationId = null)
            => new LoggingScope(logger, className, methodName, correlationId);
    }
}
