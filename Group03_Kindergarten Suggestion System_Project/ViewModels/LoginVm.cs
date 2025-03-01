using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.ViewModels
{
    public class LoginVm
    {
        [Required]
        public string Email { get; set; } 

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; } // Lưu trạng thái đăng nhập

        public string? ReturnUrl { get; set; } // Trang cần quay lại sau khi đăng nhập thành công
    }
}
