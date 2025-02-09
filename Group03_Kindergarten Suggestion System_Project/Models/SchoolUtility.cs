using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class SchoolUtility
    {
        [ForeignKey("School")]
        public Guid SchoolId { get; set; }
        public School School { get; set; }

        [ForeignKey("Utility")]
        public Guid UtilityId { get; set; }
        public Utility Utility { get; set; }
    }
}
