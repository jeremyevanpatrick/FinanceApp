namespace FinanceApp2.Api.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string keyName)
            : base($"Invalid {keyName}. Please check your information and try again.") { }
    }
}
