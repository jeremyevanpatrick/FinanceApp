using FinanceApp2.Shared.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp2.Shared.Helpers
{
    public class LoggingScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDisposable _scope;

        public LoggingScope(ILogger logger, string className, string methodName, string? correlationId = null)
        {
            _logger = logger;

            var lsState = new LoggingScopeState
            {
                ClassName = className,
                MethodName = methodName
            };

            if (correlationId != null)
            {
                lsState.CorrelationId = correlationId;
            }

            _scope = _logger.BeginScope(lsState) ?? NullScope.Instance;

            _logger.LogInformation("Entered method. Timestamp: {Timestamp}", DateTime.UtcNow);
        }

        public void Dispose()
        {
            _logger.LogInformation("Exited method. Timestamp: {Timestamp}", DateTime.UtcNow);

            _scope.Dispose();
        }
    }
}
