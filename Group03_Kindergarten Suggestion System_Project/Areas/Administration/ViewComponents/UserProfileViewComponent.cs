using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var roleName = roles.FirstOrDefault() ?? "No Role";

                    var fullName = $"{user.FirstName} {user.LastName}".Trim();

                    var userViewModel = new
                    {
                        FullName = string.IsNullOrEmpty(fullName) ? "Unknown" : fullName, 
                        Image = user.Image, 
                        RoleName = roleName,
                        Id = user.Id
                    };
                    return View("Default", userViewModel);
                }
            }
            return Content(""); // Trả về rỗng nếu không đăng nhập
        }
    }
}
