using Microsoft.Extensions.Logging;

namespace FinanceApp2.Shared.Helpers
{
    public static class LoggerExtensions
    {
        public static void LogErrorWithDictionary(this ILogger logger, string errorCode, Exception? ex, string message, Dictionary<string, string>? dictionary = null)
        {
            EventId eventId = GetEventIdFromErrorCode(errorCode);
            string detailString = string.Empty;
            if (dictionary != null)
            {
                detailString = ": " + GetDictionaryAsString(dictionary);
            }
            logger.LogError(eventId, ex, message + detailString);
        }

        private static EventId GetEventIdFromErrorCode(string errorCode)
        {
            int errorId = 0;
            var errorCodeParts = errorCode.Split('-');
            if (errorCodeParts.Length > 1)
            {
                Int32.TryParse(errorCodeParts[1], out errorId);
            }

            return new EventId(errorId, errorCode);
        }

        private static string GetDictionaryAsString(Dictionary<string, string> dictionary)
        {
            return string.Join(";", dictionary.Select(kv => $"{kv.Key}={kv.Value}"));
        }
    }
}
