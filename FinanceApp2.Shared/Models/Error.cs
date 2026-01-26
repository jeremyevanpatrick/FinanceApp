namespace FinanceApp2.Shared.Models
{
    public class Error
    {
        public int? ErrorId { get; set; }
        public string ApplicationName { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string? Notes { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
