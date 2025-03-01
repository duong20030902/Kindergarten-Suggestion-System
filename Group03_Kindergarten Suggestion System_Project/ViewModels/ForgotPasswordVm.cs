using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.ViewModels
{
    public class ForgotPasswordVm
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
