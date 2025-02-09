using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class SchoolType
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
