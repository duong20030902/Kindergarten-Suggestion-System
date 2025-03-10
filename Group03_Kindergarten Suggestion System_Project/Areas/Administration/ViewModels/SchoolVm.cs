
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels
{
    public class SchoolVM
    {
        [Key]
        public Guid Id { get; set; }

        public string? SchoolCode { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, long.MaxValue)]
        public long FeeFrom { get; set; } = 0;

        [Range(0, long.MaxValue)]
        public long FeeTo { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        [Required]
        public SchoolStatus Status { get; set; }

        public string? Phone { get; set; }
        public string? Description { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        // Address
        [Required]
        [ForeignKey("Address")]
        public Guid AddressId { get; set; }

        public AddressVM Address { get; set; } // Sử dụng AddressVM thay vì Address

        // Child Age
        [Required]
        [ForeignKey("ChildAge")]
        public Guid ChildAgeId { get; set; }
        public ChildAge? ChildAge { get; set; }

        // Education Method
        [Required]
        [ForeignKey("EducationMethod")]
        public Guid EducationMethodId { get; set; }
        public EducationMethod? EducationMethod { get; set; }

        // School Type
        [Required]
        [ForeignKey("SchoolType")]
        public Guid SchoolTypeId { get; set; }
        public SchoolType? SchoolType { get; set; }

        // User References
        [ForeignKey("Creator")]
        public string? CreatorId { get; set; }
        public User? Creator { get; set; }

        [ForeignKey("Acceptor")]
        public string? AcceptorId { get; set; }
        public User? Acceptor { get; set; }

        [ForeignKey("SchoolOwner")]
        public string? SchoolOwnerId { get; set; }
        public User? SchoolOwner { get; set; }

        // Ratings
        [Range(0, long.MaxValue)]
        public long TotalRatingCount { get; set; } = 0;

        [Range(0, long.MaxValue)]
        public long TotalRating { get; set; } = 0;
    }

    // Định nghĩa AddressVM
    public class AddressVM
    {
        public Guid Id { get; set; }
        public string Detail { get; set; }
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public int WardId { get; set; }
    }
}