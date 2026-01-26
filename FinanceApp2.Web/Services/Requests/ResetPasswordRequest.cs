namespace FinanceApp2.Web.Services.Requests
{
    public class ResetPasswordRequest
    {
        public ResetPasswordRequest(string email, string resetCode, string newPassword)
        {
            Email = email;
            ResetCode = resetCode;
            NewPassword = newPassword;
        }

        public string Email { get; set; }
        public string ResetCode { get; set; }
        public string NewPassword { get; set; }
    }
}
