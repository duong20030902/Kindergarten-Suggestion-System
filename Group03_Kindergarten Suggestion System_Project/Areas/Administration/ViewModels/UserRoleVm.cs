using Group03_Kindergarten_Suggestion_System_Project.Models;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels
{
    public class UserRoleVm
    {
        public User User { get; set; }
        public string RoleName { get; set; }
        public bool IsRoleActive { get; set; }
    }
}
