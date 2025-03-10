using Group03_Kindergarten_Suggestion_System_Project.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group03_Kindergarten_Suggestion_System_Project.Controllers
{
    public class SchoolController : Controller
    {
        private readonly KindergartenSSDatabase _context;

        public SchoolController(KindergartenSSDatabase context)
        {
            _context = context;
        }

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

        [HttpGet]
        public async Task<IActionResult> SendApplication(Guid id)
        {
            var school = await _context.Schools
                .FirstOrDefaultAsync(s => s.Id == id);

            if (school == null)
            {
                return NotFound("School not found.");
            }

            // Truyền thông tin trường vào ViewData hoặc ViewBag để hiển thị trong view
            ViewData["SchoolId"] = school.Id;
            ViewData["SchoolName"] = school.Name;

            return View();
        }
    }
}
