using Microsoft.AspNetCore.Mvc;

namespace Group03_Kindergarten_Suggestion_System_Project.Controllers
{
    public class ClassController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail()
        {
            return View();
        }

        public IActionResult LearningReport()
        {
            return View();
        }       
        
        public IActionResult TrialRegistration()
        {
            return View();
        }
    }
}
