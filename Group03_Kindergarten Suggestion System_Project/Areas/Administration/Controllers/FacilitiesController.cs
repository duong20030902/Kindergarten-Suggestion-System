using Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels;
using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class FacilitiesController : Controller
    {
        private readonly KindergartenSSDatabase _context;

        public FacilitiesController(KindergartenSSDatabase context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schoolType = await _context.Facilities.ToListAsync();

            return View(schoolType);

        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacilityVm schoolType)
        {
            if (ModelState.IsValid)
            {

                var existingType = await _context.Facilities
                   .Where(s => s.Name.ToLower() == schoolType.Name.ToLower())
                   .FirstOrDefaultAsync();

                if (existingType != null)
                {
                    ModelState.AddModelError("Name", "Facilities Name already exists.");
                    return View(schoolType);
                }


                var untility = new Facility
                {
                    Name = schoolType.Name,
                };
                _context.Facilities.Add(untility);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(schoolType);
        }


        public async Task<IActionResult> Details(Guid id)
        {
            var schoolType = await _context.Facilities.FirstOrDefaultAsync(s => s.Id == id);
            return View(schoolType);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var schoolType = await _context.Facilities
                .Where(s => s.Id == id)
                .Select(s => new FacilityVm
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
        public async Task<IActionResult> Edit(Guid id, FacilityVm schoolType)
        {
            if (id != schoolType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingType = await _context.Facilities
                    .Where(s => s.Name.ToLower() == schoolType.Name.ToLower() && s.Id != id)
                    .FirstOrDefaultAsync();

                if (existingType != null)
                {
                    ModelState.AddModelError("Name", "Facilities Name already exists.");
                    return View(schoolType);
                }

                try
                {
                    var utility = await _context.Facilities.FindAsync(id);
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
                    if (!_context.Facilities.Any(e => e.Id == id))
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
