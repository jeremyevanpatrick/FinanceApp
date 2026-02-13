namespace FinanceApp2.Api.Helpers
{
    /// <summary>
    /// Centralized registry of error codes for BudgetsController.
    /// Format: Fi2.Api.Budget-XXXXX
    /// </summary>
    public static class BudgetErrorCodes
    {
        // Get budget by date
        public const string GetByDateUnexpected = "Fi2.Api.Budget-00001";

        // Create budget
        public const string CreateBudgetUnexpected = "Fi2.Api.Budget-00002";

        // Update budget
        public const string UpdateBudgetUnexpected = "Fi2.Api.Budget-00003";

        // Delete budget
        public const string DeleteBudgetUnexpected = "Fi2.Api.Budget-00004";

    }
}
