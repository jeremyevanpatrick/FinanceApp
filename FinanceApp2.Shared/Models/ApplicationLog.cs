namespace FinanceApp2.Shared.Models
{
    public class ApplicationLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public required string Level { get; set; }
        public string? ErrorCode { get; set; }
        public required string Message { get; set; }
        public string? MessageTemplate { get; set; }
        public string? Exception { get; set; }
        public string? CorrelationId { get; set; }
        public required string ServerName { get; set; }
        public required string ApplicationName { get; set; }
    }
}
