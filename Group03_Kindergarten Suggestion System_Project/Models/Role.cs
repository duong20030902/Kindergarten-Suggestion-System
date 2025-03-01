using Microsoft.AspNetCore.Identity;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class Role : IdentityRole
    {
        public Role() : base() 
        {
        }

        public Role(string roleName) : base(roleName)
        {
        }

        public bool IsActive { get; set; }
    }
}
