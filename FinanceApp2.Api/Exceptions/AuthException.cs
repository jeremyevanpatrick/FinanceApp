using FinanceApp2.Shared.Helpers;

namespace FinanceApp2.Api.Exceptions
{
    public class AuthException : Exception
    {
        public int StatusCode { get; }
        public ResponseErrorCodes ErrorCode { get; }

        public AuthException(string message, int statusCode, ResponseErrorCodes errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
