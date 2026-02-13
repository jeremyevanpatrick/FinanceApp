namespace FinanceApp2.Shared.Helpers
{
    public class UIStatusMessage
    {
        public UIStatusMessage(string message, UIStatus status)
        {
            Message = message;
            Status = status;
        }

        public string Message { get; set; } = string.Empty;
        public UIStatus Status { get; set; }
    }
}
