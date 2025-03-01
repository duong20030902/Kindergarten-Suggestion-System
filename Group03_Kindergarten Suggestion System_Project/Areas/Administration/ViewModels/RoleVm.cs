using Group03_Kindergarten_Suggestion_System_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels
{
    public class RoleVm
    {
        public IEnumerable<Role> Roles { get; set; }

        [Required]
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
