namespace FinanceApp2.Api.Exceptions
{
    public class AuthException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        public AuthException(string message, int statusCode, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
