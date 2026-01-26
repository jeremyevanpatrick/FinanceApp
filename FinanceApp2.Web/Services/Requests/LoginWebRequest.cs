namespace FinanceApp2.Web.Services.Requests
{
    public class LoginWebRequest
    {
        public LoginWebRequest(string email, string password)
        {
            Email = email;
            Password = password;
        }

        public string Email { get; set; }

        public string Password { get; set; }

        public string? TwoFactorCode { get; set; }

        public string? TwoFactorRecoveryCode { get; set; }
    }
}
