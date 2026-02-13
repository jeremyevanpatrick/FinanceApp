namespace FinanceApp2.Shared.Helpers
{
    /// <summary>
    /// Centralized registry of error codes for Shared library.
    /// Format: Fi2.Shared-XXXXX
    /// </summary>
    public static class SharedErrorCodes
    {
        // RequestHelper
        public const string GetRequestUnexpected = "Fi2.Shared-00001";
        public const string PostUnexpected = "Fi2.Shared-00002";
    }
}
