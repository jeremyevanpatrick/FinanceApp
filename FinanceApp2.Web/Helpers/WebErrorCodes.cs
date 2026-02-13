namespace FinanceApp2.Web.Helpers
{
    /// <summary>
    /// Centralized registry of error codes for Web application.
    /// Format: Fi2.Web.XXXX-XXXXX
    /// </summary>
    public static class WebErrorCodes
    {
        // Client level exception
        public const string ClientUnexpected = "Fi2.Web.Client-00001";

        // RefreshTokenAsync
        public const string RefreshTokenUnexpected = "Fi2.Web.Auth-00001";

    }
}
