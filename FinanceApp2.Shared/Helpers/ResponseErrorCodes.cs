namespace FinanceApp2.Shared.Helpers
{
    public enum ResponseErrorCodes
    {
        INVALID_CREDENTIALS,
        INVALID_REQUEST_PARAMETERS,
        TOKEN_INVALID_OR_EXPIRED,
        PASSWORD_DOES_NOT_MEET_REQUIREMENTS,
        EMAIL_ADDRESS_ALREADY_IN_USE,
        ACCOUNT_LOCKED,
        AUTH_NO_LONGER_VALID,
        UNAUTHORIZED,
        FORBIDDEN
    }
}
