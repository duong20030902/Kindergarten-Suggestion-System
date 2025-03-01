using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels
{
    public class UserVm
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string RoleId { get; set; }
    }
}
