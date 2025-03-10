using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Group03_Kindergarten_Suggestion_System_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly KindergartenSSDatabase _context;
        private readonly UserManager<User> _userManager;

        public HomeController(KindergartenSSDatabase context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var schools = _context.Schools
     .Where(s => s.Status == SchoolStatus.Published)
     .Include(s => s.ChildAge)
     .ToList();
            Console.WriteLine($"Found {schools.Count} schools");

            return View(schools);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
