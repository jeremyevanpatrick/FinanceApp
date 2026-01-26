using Microsoft.AspNetCore.Identity;

namespace FinanceApp2.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
