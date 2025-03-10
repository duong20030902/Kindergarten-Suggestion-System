using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class SchoolUtilitiesController : Controller
    {
        private readonly KindergartenSSDatabase _context;

        public SchoolUtilitiesController(KindergartenSSDatabase context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schools = await _context.Schools.ToListAsync();
            return View(schools);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var school = await _context.Schools
                .Include(s => s.SchoolUtilities)
                .ThenInclude(su => su.Utility)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (school == null)
            {
                return NotFound();
            }

            ViewBag.Utilities = await _context.Utilities.ToListAsync();
            return View(school);
        }

        [HttpPost]
        public async Task<IActionResult> AddUtility(Guid schoolId, Guid utilityId)
        {
            var school = await _context.Schools
                .Include(s => s.SchoolUtilities)
                .FirstOrDefaultAsync(s => s.Id == schoolId);

            if (school == null)
            {
                return NotFound();
            }

            // Kiểm tra tiện ích đã tồn tại trong trường hay chưa
            if (school.SchoolUtilities.Any(su => su.UtilityId == utilityId))
            {
                TempData["Error"] = "Tiện ích này đã tồn tại trong trường học.";
                return RedirectToAction("Details", new { id = schoolId });
            }

            // Thêm tiện ích vào trường
            var schoolUtility = new SchoolUtility
            {
                SchoolId = schoolId,
                UtilityId = utilityId
            };

            _context.SchoolUtilities.Add(schoolUtility);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm tiện ích thành công.";
            return RedirectToAction("Details", new { id = schoolId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUtility(Guid schoolId, Guid utilityId)
        {
            var schoolUtility = await _context.SchoolUtilities
                .FirstOrDefaultAsync(su => su.SchoolId == schoolId && su.UtilityId == utilityId);

            if (schoolUtility == null)
            {
                return NotFound();
            }

            _context.SchoolUtilities.Remove(schoolUtility);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa tiện ích thành công.";
            return RedirectToAction("Details", new { id = schoolId });
        }

    }
}
