using Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels;
using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly KindergartenSSDatabase _context;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleController(KindergartenSSDatabase context, RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.ToListAsync();
            var model = new RoleVm
            {
                Roles = roles
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(Role model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _roleManager.Roles.AnyAsync(r => r.Name.ToLower() == model.Name.ToLower()))
                    {
                        TempData["warning"] = $"Role {model.Name} already exists.";
                    }
                    else
                    {
                        var role = new Role(model.Name)
                        {
                            IsActive = true 
                        };
                        var result = await _roleManager.CreateAsync(role);
                        if (result.Succeeded)
                        {
                            TempData["success"] = $"Role {model.Name} created successfully!";
                        }
                        else
                        {
                            TempData["warning"] = "Unable to create role.";
                        }
                    }
                }
                else
                {
                    TempData["warning"] = "Invalid input!";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "System error. Please try again!";
                Console.WriteLine("Error: " + ex.Message);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(Role model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    TempData["warning"] = "Role not found!";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    TempData["warning"] = "Role not found!";
                    return RedirectToAction("Index");
                }

                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    TempData["warning"] = "Role not found!";
                    return RedirectToAction("Index");
                }

                if (await _roleManager.Roles.AnyAsync(r => r.Name.ToLower() == model.Name.ToLower() && r.Id != model.Id))
                {
                    TempData["warning"] = $"Role {model.Name} already exists.";
                    return RedirectToAction("Index");
                }

                role.Name = model.Name; 
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    TempData["success"] = $"Role {model.Name} was updated successfully!";
                }
                else
                {
                    TempData["warning"] = "Role update error!";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "System error. Please try again! " + ex.Message;
                Console.WriteLine("Error: " + ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
