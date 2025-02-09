using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class ImageUrl
    {
        [Key]
        public Guid Id { get; set; }
        public string Url { get; set; }
    }
}
