using Microsoft.AspNetCore.Mvc;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }    
        
        public IActionResult EditProfile()
        {
            return View();
        }
    }
}
