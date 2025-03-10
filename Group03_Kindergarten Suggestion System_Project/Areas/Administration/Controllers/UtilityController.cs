using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.ViewModels;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class UtilityController: Controller
    {

        private readonly KindergartenSSDatabase _context;

        public UtilityController(KindergartenSSDatabase context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schoolType = await _context.Utilities.ToListAsync();

            return View(schoolType);

        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( UntilityVm schoolType)
        {
            if (ModelState.IsValid)
            {

                var existingType = await _context.Utilities
                   .Where(s => s.Name.ToLower() == schoolType.Name.ToLower())
                   .FirstOrDefaultAsync();

                if (existingType != null)
                {
                    ModelState.AddModelError("Name", "Untility Name already exists.");
                    return View(schoolType);
                }


                var untility = new Utility
                {
                    Name = schoolType.Name,
                };
                _context.Utilities.Add(untility);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(schoolType);
        }


        public async Task<IActionResult> Details(Guid id)
        {
            var schoolType = await _context.Utilities.FirstOrDefaultAsync(s => s.Id == id);
            return View(schoolType);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var schoolType = await _context.Utilities
                .Where(s => s.Id == id)
                .Select(s => new UntilityVm
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .FirstOrDefaultAsync();

            if (schoolType == null)
            {
                return NotFound();
            }

            return View(schoolType);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UntilityVm schoolType)
        {
            if (id != schoolType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingType = await _context.Utilities
                    .Where(s => s.Name.ToLower() == schoolType.Name.ToLower() && s.Id != id)
                    .FirstOrDefaultAsync();

                if (existingType != null)
                {
                    ModelState.AddModelError("Name", "Untility Name already exists.");
                    return View(schoolType);
                }

                try
                {
                    var utility = await _context.Utilities.FindAsync(id);
                    if (utility == null)
                    {
                        return NotFound();
                    }

                    utility.Name = schoolType.Name; // Update the name property with the value from the ViewModel

                    _context.Update(utility);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Utilities.Any(e => e.Id == id))
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

    }
}
