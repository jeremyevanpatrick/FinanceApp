namespace FinanceApp2.Api.Settings
{
    public class MailerSendSettings
    {
        public string BaseUrl { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public string FromName { get; set; } = null!;
    }

}
