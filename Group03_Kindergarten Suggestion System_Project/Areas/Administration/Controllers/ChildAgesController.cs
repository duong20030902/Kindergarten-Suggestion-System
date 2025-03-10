using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class ChildAgesController : Controller
    {
        private readonly KindergartenSSDatabase _context;

        public ChildAgesController(KindergartenSSDatabase context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var childAge = await _context.ChildAges.ToListAsync();

            return View(childAge);

        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        public async Task<IActionResult> Create(ChildAge ChildAge)
        {
            if (ModelState.IsValid)
            {
                var existingType = await _context.ChildAges
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == ChildAge.Name.ToLower());

                if (existingType != null)
                {
                    ModelState.AddModelError("Name", "Child Age Name already exists.");
                    return View(ChildAge);
                }

                _context.ChildAges.Add(ChildAge);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(ChildAge);
        }
        public async Task<IActionResult> Details(Guid id)
        {
            var schoolType = await _context.ChildAges.FirstOrDefaultAsync(s => s.Id == id);
            return View(schoolType);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var schoolType = await _context.ChildAges.FindAsync(id);
            if (schoolType == null)
            {
                return NotFound();
            }
            return View(schoolType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ChildAge schoolType)
        {
            if (id != schoolType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingType = await _context.ChildAges
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
                    if (!_context.ChildAges.Any(e => e.Id == id))
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
            var schoolType = _context.ChildAges.Find(id);
            if (schoolType == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.SchoolTypeName = schoolType.Name;
            return View();
        }


        public async Task<IActionResult> Delete(Guid id)
        {
            var schoolType = await _context.ChildAges.FindAsync(id);
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
            var schoolType = await _context.ChildAges.FindAsync(id);
            if (schoolType != null)
            {
                // Kiểm tra xem có trường học nào thuộc loại này không
                bool hasRelatedSchools = await _context.Schools.AnyAsync(s => s.ChildAgeId == id);

                if (hasRelatedSchools)
                {
                    // Chuyển hướng đến trang thông báo lỗi
                    return RedirectToAction("DeleteError", new { id = id });
                }

                _context.ChildAges.Remove(schoolType);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

    }
}
