using Group03_Kindergarten_Suggestion_System_Project.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class CounsellingRequest
    {
        [Key]
        public Guid Id { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Inquiry { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [ForeignKey("School")]
        public Guid SchoolId { get; set; }
        public School School { get; set; }

        [ForeignKey("Parent")]
        public string ParentId { get; set; }
        public User Parent { get; set; }
    }
}
