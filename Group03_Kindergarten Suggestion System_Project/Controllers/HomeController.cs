using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Group03_Kindergarten_Suggestion_System_Project.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            Console.WriteLine($"Home/Index - IsAuthenticated: {User.Identity?.IsAuthenticated ?? false}, UserName: {User.Identity?.Name ?? "No user"}");
            Console.WriteLine("Cookies in Request at Home/Index:");
            foreach (var cookie in Request.Cookies)
            {
                Console.WriteLine($"Request Cookie: {cookie.Key} = {cookie.Value}");
            }
            Console.WriteLine("Claims in User:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }
            return View();
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
