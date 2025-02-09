using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Group03_Kindergarten_Suggestion_System_Project.Models.Enums;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class SchoolEnrollment
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime EnrolledDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public EnrollStatus Status { get; set; }

        [ForeignKey("School")]
        public Guid SchoolId { get; set; }
        public School School { get; set; }

        [ForeignKey("Parent")]
        public string ParentId { get; set; }
        public User Parent { get; set; }
    }
}
