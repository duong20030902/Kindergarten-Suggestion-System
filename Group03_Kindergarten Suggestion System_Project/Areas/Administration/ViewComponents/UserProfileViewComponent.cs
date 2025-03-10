using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewComponents
{
    public class UserProfileViewComponent : ViewComponent
    {
        private readonly UserManager<User> _userManager;

        public UserProfileViewComponent(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Content(""); // Không hiển thị nếu chưa đăng nhập
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                // Fallback: Lấy thông tin từ Claims nếu không tìm thấy user
                var email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    user = await _userManager.FindByEmailAsync(email);
                }
            }

            if (user == null)
            {
                return Content(""); // Trả về rỗng nếu vẫn không tìm thấy user
            }

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? "No Role";
            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            var userViewModel = new
            {
                FullName = string.IsNullOrEmpty(fullName) ? "Unknown" : fullName,
                Image = user.Image ?? "default.jpg", // Đảm bảo có giá trị mặc định
                RoleName = roleName,
                Id = user.Id
            };

            return View("Default", userViewModel);
        }
    }
}
