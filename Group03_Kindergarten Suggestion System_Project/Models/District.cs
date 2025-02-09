using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class District
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [ForeignKey("Province")]
        public int ProvinceId { get; set; }
        public Province Province { get; set; }
    }
}