using Group03_Kindergarten_Suggestion_System_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group03_Kindergarten_Suggestion_System_Project.Models
{
    public class School
    {
        [Key]
        public Guid Id { get; set; }
        public string SchoolCode { get; set; }
        public string Name { get; set; }
        public long FeeFrom { get; set; } = 0;
        public long FeeTo { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public SchoolStatus Status { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [ForeignKey("Address")]
        public Guid AddressId { get; set; }
        public Address Address { get; set; }

        [ForeignKey("ChildAge")]
        public Guid ChildAgeId { get; set; }
        public ChildAge ChildAge { get; set; }

        [ForeignKey("EducationMethod")]
        public Guid EducationMethodId { get; set; }
        public EducationMethod EducationMethod { get; set; }

        [ForeignKey("SchoolType")]
        public Guid SchoolTypeId { get; set; }
        public SchoolType SchoolType { get; set; }

        [ForeignKey("Creator")]
        public string CreatorId { get; set; }
        public User Creator { get; set; }

        [ForeignKey("Acceptor")]
        public string AcceptorId { get; set; }
        public User Acceptor { get; set; }

        public long TotalRatingCount { get; set; } = 0;
        public long TotalRating { get; set; } = 0;

        public ICollection<SchoolFacility> SchoolFacilities { get; set; }
        public ICollection<SchoolUtility> SchoolUtilities { get; set; }
        public ICollection<ImageUrl> Images { get; set; }

        public double GetRating()
        {
            return TotalRatingCount == 0 ? 0 : (double)TotalRating / TotalRatingCount / 5;
        }
    }
}
