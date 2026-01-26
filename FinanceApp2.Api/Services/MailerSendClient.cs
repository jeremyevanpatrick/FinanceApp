using FinanceApp2.Api.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

public class MailerSendClient : IEmailSender
{
    private readonly MailerSendSettings _mailerSendSettings;
    private readonly HttpClient _httpClient;

    public MailerSendClient(HttpClient httpClient, IOptions<MailerSendSettings> mailerSendSettings)
    {
        _httpClient = httpClient;
        _mailerSendSettings = mailerSendSettings.Value;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _mailerSendSettings.Token);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        MailerSendEmailRequest request = new MailerSendEmailRequest
        {
            From = new MailerSendEmailAddress
            {
                Email = _mailerSendSettings.FromEmail,
                Name = _mailerSendSettings.FromName
            },
            To = new MailerSendEmailAddress[]
            {
                new MailerSendEmailAddress
                {
                    Email = email,
                    Name = email
                }
            },
            Subject = subject,
            Html = htmlMessage
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(9000));
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_mailerSendSettings.BaseUrl, request, cts.Token);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"MailerSend API error ({response.StatusCode}): {error}");
        }
    }

    private class MailerSendEmailRequest
    {
        public MailerSendEmailAddress From { get; set; } = null!;
        public MailerSendEmailAddress[] To { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Html { get; set; } = null!;
        public string? Text { get; set; }
    }

    private class MailerSendEmailAddress
    {
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
    }
}
