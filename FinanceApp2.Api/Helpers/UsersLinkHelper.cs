using FinanceApp2.Api.Controllers;
using FinanceApp2.Shared.Models;

namespace FinanceApp2.Api.Helpers
{
    public class UsersLinkHelper
    {
        private readonly LinkGenerator _linkGenerator;

        public UsersLinkHelper(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        private string GetPath(string action, object? values = null)
        {
            return _linkGenerator.GetPathByAction(action, "Users", values)?.ToLower() ?? string.Empty;
        }

        public Link UsersRegister()
        {
            return new Link
            {
                Href = GetPath(nameof(UsersController.Register)),
                Rel = "register",
                Method = "POST"
            };
        }

        public Link UsersDeleteAccount()
        {
            return new Link
            {
                Href = GetPath(nameof(UsersController.DeleteAccount)),
                Rel = "delete",
                Method = "DELETE"
            };
        }

        public Link UsersChangePassword()
        {
            return new Link
            {
                Href = GetPath(nameof(UsersController.ChangePassword)),
                Rel = "password-update",
                Method = "PATCH"
            };
        }
    }
}
