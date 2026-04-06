using FinanceApp2.Api.Controllers;
using FinanceApp2.Shared.Models;

namespace FinanceApp2.Api.Helpers
{
    public class EmailChangeRequestsLinkHelper
    {
        private readonly LinkGenerator _linkGenerator;

        public EmailChangeRequestsLinkHelper(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        private string GetPath(string action, object? values = null)
        {
            return _linkGenerator.GetPathByAction(action, "EmailChangeRequests", values)?.ToLower() ?? string.Empty;
        }

        public Link EmailChangeRequestsSend()
        {
            return new Link
            {
                Href = GetPath(nameof(EmailChangeRequestsController.ChangeEmail)),
                Rel = "send",
                Method = "POST"
            };
        }

        public Link EmailChangeRequestsConfirm()
        {
            return new Link
            {
                Href = GetPath(nameof(EmailChangeRequestsController.ChangeEmailConfirmation)),
                Rel = "confirm",
                Method = "POST"
            };
        }
    }
}
