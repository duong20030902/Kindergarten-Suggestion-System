using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class SchoolTypeController: Controller
    {
        private readonly KindergartenSSDatabase _context;

        public SchoolTypeController(KindergartenSSDatabase context) { 
            _context = context;
        }

        public  async Task<IActionResult> Index() {
            var schoolType = await _context.SchoolTypes.ToListAsync();

            return View(schoolType);
        
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //[HttpPost]

        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create( SchoolType schoolType)
        //{
        //    if (ModelState.IsValid) {


        //        var schoolTypes = new SchoolType
        //        {
        //            Name = schoolType.Name
        //        };
        //        _context.SchoolTypes.Add(schoolTypes);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(schoolType);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SchoolType schoolType)
        {
            if (ModelState.IsValid)
            {
                var existingType = await _context.SchoolTypes
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == schoolType.Name.ToLower());

                if (existingType != null)
                {
                    ModelState.AddModelError("Name", "School Type Name already exists.");
                    return View(schoolType);
                }

                _context.SchoolTypes.Add(schoolType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(schoolType);
        }
        public async Task<IActionResult> Details(Guid id)
        {
            var schoolType = await _context.SchoolTypes.FirstOrDefaultAsync(s => s.Id == id);
            return View(schoolType);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var schoolType = await _context.SchoolTypes.FindAsync(id);
            if (schoolType == null)
            {
                return NotFound();
            }
            return View(schoolType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SchoolType schoolType)
        {
            if (id != schoolType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingType = await _context.SchoolTypes
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
                    if (!_context.SchoolTypes.Any(e => e.Id == id))
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
            var schoolType = _context.SchoolTypes.Find(id);
            if (schoolType == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.SchoolTypeName = schoolType.Name;
            return View();
        }


        public async Task<IActionResult> Delete(Guid id)
        {
            var schoolType = await _context.SchoolTypes.FindAsync(id);
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
            var schoolType = await _context.SchoolTypes.FindAsync(id);
            if (schoolType != null)
            {
                // Kiểm tra xem có trường học nào thuộc loại này không
                bool hasRelatedSchools = await _context.Schools.AnyAsync(s => s.SchoolTypeId == id);

                if (hasRelatedSchools)
                {
                    // Chuyển hướng đến trang thông báo lỗi
                    return RedirectToAction("DeleteError", new { id = id });
                }

                _context.SchoolTypes.Remove(schoolType);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }


    }
}
