using FinanceApp2.Api.Controllers;
using FinanceApp2.Shared.Models;

namespace FinanceApp2.Api.Helpers
{
    public class EmailConfirmationRequestsLinkHelper
    {
        private readonly LinkGenerator _linkGenerator;

        public EmailConfirmationRequestsLinkHelper(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        private string GetPath(string action, object? values = null)
        {
            return _linkGenerator.GetPathByAction(action, "EmailConfirmationRequests", values)?.ToLower() ?? string.Empty;
        }

        public Link EmailConfirmationRequestsResend()
        {
            return new Link
            {
                Href = GetPath(nameof(EmailConfirmationRequestsController.ResendConfirmationEmail)),
                Rel = "resend",
                Method = "POST"
            };
        }

        public Link EmailConfirmationRequestsConfirm()
        {
            return new Link
            {
                Href = GetPath(nameof(EmailConfirmationRequestsController.ConfirmEmail)),
                Rel = "confirm",
                Method = "POST"
            };
        }
    }
}
