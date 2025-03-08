using Group03_Kindergarten_Suggestion_System_Project.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels
{
    public class UserVm
    {
        public string Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string RoleId { get; set; }

        [Phone]
        public string PhoneNumber { get; set; } 

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; } 

        //public string Address { get; set; } 

        public bool EmailConfirmed { get; set; } 

        public string? Image { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageFile { get; set; }
    }
}
