namespace FinanceApp2.Api.Helpers
{
    /// <summary>
    /// Centralized registry of error codes for DataCleanupService.
    /// Format: Fi2.Api.DCS-XXXXX
    /// </summary>
    public static class CleanupErrorCodes
    {
        // Cleanup refresh tokens
        public const string CleanupRefreshTokensUnexpected = "Fi2.Api.DCS-00001";

        // Cleanup soft deleted users
        public const string CleanupSoftDeletedUsersUnexpected = "Fi2.Api.DCS-00002";
        public const string CleanupSoftDeletedUsersFailed = "Fi2.Api.DCS-00003";

        // Cleanup soft deleted user data
        public const string CleanupSoftDeletedUserDataUnexpected = "Fi2.Api.DCS-00004";

    }
}
