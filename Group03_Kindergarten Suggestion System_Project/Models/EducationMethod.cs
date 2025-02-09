using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class EducationMethod
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
