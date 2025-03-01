using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? RoleId { get; set; }
        public string Image { get; set; } = "avatar-default.jpg";

        [ForeignKey("Address")]
        public Guid? AddressId { get; set; }
        public Address Address { get; set; }
    }
}
