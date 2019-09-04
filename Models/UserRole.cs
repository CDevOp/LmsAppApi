using LmsApp.API.Models;
using Microsoft.AspNetCore.Identity;

namespace lmsapp.api.Models
{
    public class UserRole : IdentityUserRole<int>
    {
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}