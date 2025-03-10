using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Group03_Kindergarten_Suggestion_System_Project.ViewComponents
{
    public class RequestsViewComponent : ViewComponent
    {
        private readonly KindergartenSSDatabase _context;
        private readonly UserManager<User> _userManager;

        public RequestsViewComponent(KindergartenSSDatabase context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return View("Default", new List<SchoolEnrollment>());
            }

            var enrollmentRequests = await _context.SchoolEnrollments
                .Include(e => e.School)
                .Where(e => e.ParentId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View("Default", enrollmentRequests);
        }
    }
}
