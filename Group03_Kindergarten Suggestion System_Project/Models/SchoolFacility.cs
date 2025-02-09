using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class SchoolFacility
    {
        [ForeignKey("School")]
        public Guid SchoolId { get; set; }
        public School School { get; set; }

        [ForeignKey("Facility")]
        public Guid FacilityId { get; set; }
        public Facility Facility { get; set; }
    }
}
