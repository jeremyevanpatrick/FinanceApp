namespace FinanceApp2.Api.Models
{
    public class EmailDetails
    {
        public EmailDetails(string emailAddress, string subject, string messageHtml)
        {
            EmailAddress = emailAddress;
            Subject = subject;
            MessageHtml = messageHtml;
        }

        public string EmailAddress { get; set; }
        public string Subject { get; set; }
        public string MessageHtml { get; set; }
    }
}
