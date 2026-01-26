using System.Security.Claims;

namespace FinanceApp2.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("Invalid user ID claim");
        }
    }
}
