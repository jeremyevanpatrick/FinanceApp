using FinanceApp2.Api.Controllers;
using FinanceApp2.Shared.Models;

namespace FinanceApp2.Api.Helpers
{
    public class SessionsLinkHelper
    {
        private readonly LinkGenerator _linkGenerator;

        public SessionsLinkHelper(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        private string GetPath(string action, object? values = null)
        {
            return _linkGenerator.GetPathByAction(action, "Sessions", values)?.ToLower() ?? string.Empty;
        }

        public List<Link> GetLinksForSessionsCreate()
        {
            return new List<Link>()
            {
                SessionsRefresh(),
                SessionsLogout()
            };
        }

        public List<Link> GetLinksForSessionsRefresh()
        {
            return new List<Link>()
            {
                SessionsRefresh(),
                SessionsLogout()
            };
        }

        public Link SessionsLogin()
        {
            return new Link
            {
                Href = GetPath(nameof(SessionsController.Login)),
                Rel = "login",
                Method = "POST"
            };
        }

        public Link SessionsRefresh()
        {
            return new Link
            {
                Href = GetPath(nameof(SessionsController.RefreshToken)),
                Rel = "refresh",
                Method = "POST"
            };
        }

        public Link SessionsLogout()
        {
            return new Link
            {
                Href = GetPath(nameof(SessionsController.Logout)),
                Rel = "logout",
                Method = "DELETE"
            };
        }
    }
}
