using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class UserHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? RoleId { get; set; }
        public DateTime UpdatedAt { get; set; } 

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
