using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class EducationMethodController : Controller
    {

        private readonly KindergartenSSDatabase _context;

        public EducationMethodController(KindergartenSSDatabase context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schoolType = await _context.EducationMethods.ToListAsync();

            return View(schoolType);

        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EducationMethod schoolType)
        {
            if (ModelState.IsValid)
            {
                var existingType = await _context.EducationMethods
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == schoolType.Name.ToLower());

                if (existingType != null)
                {
                    ModelState.AddModelError("Name", "Education Name already exists.");
                    return View(schoolType);
                }

                _context.EducationMethods.Add(schoolType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(schoolType);
        }


        public async Task<IActionResult> Details(Guid id)
        {
            var schoolType = await _context.EducationMethods.FirstOrDefaultAsync(s => s.Id == id);
            return View(schoolType);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var schoolType = await _context.EducationMethods.FindAsync(id);
            if (schoolType == null)
            {
                return NotFound();
            }
            return View(schoolType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EducationMethod schoolType)
        {
            if (id != schoolType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingType = await _context.EducationMethods
                    .Where(s => s.Name.ToLower() == schoolType.Name.ToLower() && s.Id != id)
                    .FirstOrDefaultAsync();

                if (existingType != null)
                {
                    ModelState.AddModelError("Name", "School Type Name already exists.");
                    return View(schoolType);
                }

                try
                {
                    _context.Update(schoolType);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.EducationMethods.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(schoolType);
        }

        public IActionResult DeleteError(Guid id)
        {
            var schoolType = _context.EducationMethods.Find(id);
            if (schoolType == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.SchoolTypeName = schoolType.Name;
            return View();
        }


        public async Task<IActionResult> Delete(Guid id)
        {
            var schoolType = await _context.EducationMethods.FindAsync(id);
            if (schoolType == null)
            {
                return NotFound();
            }
            return View(schoolType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var schoolType = await _context.EducationMethods.FindAsync(id);
            if (schoolType != null)
            {
                // Kiểm tra xem có trường học nào thuộc loại này không
                bool hasRelatedSchools = await _context.Schools.AnyAsync(s => s.EducationMethodId == id);

                if (hasRelatedSchools)
                {
                    // Chuyển hướng đến trang thông báo lỗi
                    return RedirectToAction("DeleteError", new { id = id });
                }

                _context.EducationMethods.Remove(schoolType);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }



    }
}
