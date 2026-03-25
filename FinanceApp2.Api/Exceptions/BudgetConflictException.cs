namespace FinanceApp2.Api.Exceptions
{
    public class BudgetConflictException : Exception
    {
        public BudgetConflictException(string message)
            : base(message) { }
    }
}
