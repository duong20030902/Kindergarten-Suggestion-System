using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class Address
    {
        [Key]
        public Guid Id { get; set; }
        public string Detail { get; set; }

        [ForeignKey("Province")]
        public int ProvinceId { get; set; }
        public Province Province { get; set; }

        [ForeignKey("District")]
        public int DistrictId { get; set; }
        public District District { get; set; }

        [ForeignKey("Ward")]
        public int WardId { get; set; }
        public Ward Ward { get; set; }
    }
}
