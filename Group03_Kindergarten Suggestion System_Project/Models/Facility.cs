using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class Facility
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<SchoolFacility> SchoolFacilities { get; set; }
    }
}
