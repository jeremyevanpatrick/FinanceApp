namespace FinanceApp2.Api.Errors
{
    /// <summary>
    /// Centralized registry of error codes for AuthController.
    /// Format: Fi2.Api.Auth-XXXXX
    /// </summary>
    public static class AuthErrorCodes
    {
        // Registration
        public const string RegisterUnexpected = "Fi2.Api.Auth-00001";

        // Resend confirmation email
        public const string ResendConfirmationEmailUnexpected = "Fi2.Api.Auth-00002";

        // Confirm email
        public const string ConfirmEmailUnexpected = "Fi2.Api.Auth-00003";

        // Login
        public const string LoginUnexpected = "Fi2.Api.Auth-00004";

        // Forgot password
        public const string ForgotPasswordUnexpected = "Fi2.Api.Auth-00005";

        // Reset password
        public const string ResetPasswordUnexpected = "Fi2.Api.Auth-00006";

        // Change password
        public const string ChangePasswordUnexpected = "Fi2.Api.Auth-00007";
        public const string ChangePasswordFailed = "Fi2.Api.Auth-00008";

        // Change email
        public const string ChangeEmailUnexpected = "Fi2.Api.Auth-00009";

        // Confirm email change
        public const string ChangeEmailConfirmationUnexpected = "Fi2.Api.Auth-00010";

        // Refresh token
        public const string RefreshTokenUnexpected = "Fi2.Api.Auth-00011";

        // Logout
        public const string LogoutUnexpected = "Fi2.Api.Auth-00012";

        // Delete account
        public const string DeleteAccountUnexpected = "Fi2.Api.Auth-00013";
        public const string DeleteAccountFailed = "Fi2.Api.Auth-00014";
    }
}
