using Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); 
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            string roleName = roles.FirstOrDefault(); 

            var userRoleVm = new UserRoleVm
            {
                User = user,
                RoleName = roleName
            };

            return View(userRoleVm);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id != userId)
            {
                return Forbid(); // Không cho phép truy cập profile của người khác
            }

            var userVm = new UserVm
            {
                Id = currentUser.Id,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber,
                BirthDate = currentUser.BirthDate,
                EmailConfirmed = currentUser.EmailConfirmed,
                Image = currentUser.Image,
                RoleId = currentUser.RoleId
            };

            return View(userVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(string userId, UserVm userVm)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id != userId)
            {
                return Forbid(); // Không cho phép chỉnh sửa profile của người khác
            }

            if (!ModelState.IsValid)
            {
                return View(userVm);
            }

            try
            {
                // Cập nhật thông tin người dùng
                currentUser.FirstName = userVm.FirstName;
                currentUser.LastName = userVm.LastName;
                currentUser.UserName = userVm.UserName;
                currentUser.Email = userVm.Email;
                currentUser.PhoneNumber = userVm.PhoneNumber;
                currentUser.BirthDate = userVm.BirthDate;
                currentUser.UpdatedAt = DateTime.Now;

                // Xử lý ảnh nếu có upload
                if (userVm.ImageFile != null && userVm.ImageFile.Length > 0)
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "img/users", currentUser.Image);
                    if (System.IO.File.Exists(oldImagePath) && currentUser.Image != "avatar-default.jpg")
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img/users");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string imageName = Guid.NewGuid().ToString() + "_" + userVm.ImageFile.FileName;
                    string filePath = Path.Combine(uploadFolder, imageName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await userVm.ImageFile.CopyToAsync(fileStream);
                    }

                    currentUser.Image = imageName;
                }
                else
                {
                    currentUser.Image = currentUser.Image; // Giữ nguyên ảnh cũ nếu không upload mới
                }

                var result = await _userManager.UpdateAsync(currentUser);
                if (result.Succeeded)
                {
                    TempData["success"] = "Profile updated successfully.";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return View(userVm);
        }
    }
}
