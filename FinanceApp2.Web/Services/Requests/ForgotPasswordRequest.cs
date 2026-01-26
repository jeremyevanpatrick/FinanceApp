namespace FinanceApp2.Web.Services.Requests
{
    public class ForgotPasswordRequest
    {
        public ForgotPasswordRequest(string email)
        {
            Email = email;
        }

        public string Email { get; set; }
    }
}
