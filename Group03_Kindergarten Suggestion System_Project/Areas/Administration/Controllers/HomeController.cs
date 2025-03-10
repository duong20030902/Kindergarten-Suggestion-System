using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Debug để kiểm tra quyền
            var userName = User.Identity.Name;
            var isAdmin = User.IsInRole("Admin");
            Console.WriteLine($"User: {userName}, IsAdmin: {isAdmin}");
            return View();
        }
    }
}
