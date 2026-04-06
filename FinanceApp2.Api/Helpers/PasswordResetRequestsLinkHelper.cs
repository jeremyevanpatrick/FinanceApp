using FinanceApp2.Api.Controllers;
using FinanceApp2.Shared.Models;

namespace FinanceApp2.Api.Helpers
{
    public class PasswordResetRequestsLinkHelper
    {
        private readonly LinkGenerator _linkGenerator;

        public PasswordResetRequestsLinkHelper(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        private string GetPath(string action, object? values = null)
        {
            return _linkGenerator.GetPathByAction(action, "PasswordResetRequests", values)?.ToLower() ?? string.Empty;
        }

        public Link PasswordResetRequestsSend()
        {
            return new Link
            {
                Href = GetPath(nameof(PasswordResetRequestsController.ForgotPassword)),
                Rel = "send",
                Method = "POST"
            };
        }

        public Link PasswordResetRequestsConfirm()
        {
            return new Link
            {
                Href = GetPath(nameof(PasswordResetRequestsController.ResetPassword)),
                Rel = "confirm",
                Method = "POST"
            };
        }
    }
}
