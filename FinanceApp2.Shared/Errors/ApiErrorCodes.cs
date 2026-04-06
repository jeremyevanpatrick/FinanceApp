namespace FinanceApp2.Shared.Errors
{
    public static class ApiErrorCodes
    {
        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
        public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
        public const string INVALID_REQUEST_PARAMETERS = "INVALID_REQUEST_PARAMETERS";
        public const string TOKEN_INVALID_OR_EXPIRED = "TOKEN_INVALID_OR_EXPIRED";
        public const string PASSWORD_DOES_NOT_MEET_REQUIREMENTS = "PASSWORD_DOES_NOT_MEET_REQUIREMENTS";
        public const string EMAIL_ADDRESS_ALREADY_IN_USE = "EMAIL_ADDRESS_ALREADY_IN_USE";
        public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
        public const string AUTH_NO_LONGER_VALID = "AUTH_NO_LONGER_VALID";
        public const string UNAUTHORIZED = "UNAUTHORIZED";
        public const string FORBIDDEN = "FORBIDDEN";
        public const string TOOMANYREQUESTS = "TOOMANYREQUESTS";
    }
}
