using Microsoft.AspNetCore.Mvc;

namespace Group03_Kindergarten_Suggestion_System_Project.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
