using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class FAQ
    {
        [Key]
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool Status { get; set; }
    }
}
