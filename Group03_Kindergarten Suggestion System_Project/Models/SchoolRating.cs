using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class SchoolRating
    {
        [Key]
        public Guid Id { get; set; }
        public string Feedback { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Range(0, 5)]
        public byte Rating1 { get; set; }

        [Range(0, 5)]
        public byte Rating2 { get; set; }

        [Range(0, 5)]
        public byte Rating3 { get; set; }

        [Range(0, 5)]
        public byte Rating4 { get; set; }

        [Range(0, 5)]
        public byte Rating5 { get; set; }

        [ForeignKey("School")]
        public Guid SchoolId { get; set; }
        public School School { get; set; }

        [ForeignKey("Parent")]
        public string ParentId { get; set; }
        public User Parent { get; set; }

        public double GetAvgRating()
        {
            return (Rating1 + Rating2 + Rating3 + Rating4 + Rating5) / 5.0;
        }
    }
}
