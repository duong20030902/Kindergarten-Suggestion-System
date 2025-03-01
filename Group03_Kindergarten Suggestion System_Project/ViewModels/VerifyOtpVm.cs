using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.ViewModels
{
    public class VerifyOtpVm
    {
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must have 6 digits.")]
        public string Otp { get; set; }
    }
}
