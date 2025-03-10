using Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels;
using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class ProfileController : Controller
    {
        private readonly KindergartenSSDatabase _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(KindergartenSSDatabase context, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _context.Users
                    .Include(u => u.Address)
                        .ThenInclude(a => a.Province)
                    .Include(u => u.Address)
                        .ThenInclude(a => a.District)
                    .Include(u => u.Address)
                        .ThenInclude(a => a.Ward)
                    .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

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
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            var districts = await _context.Districts
                .Where(d => d.ProvinceId == provinceId)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();
            return Json(districts);
        }

        [HttpGet]
        public async Task<IActionResult> GetWards(int districtId)
        {
            var wards = await _context.Wards
                .Where(w => w.DistrictId == districtId)
                .Select(w => new { w.Id, w.Name })
                .ToListAsync();
            return Json(wards);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile(string Id)
        {
            var currentUser = await _context.Users
                    .Include(u => u.Address)
                        .ThenInclude(a => a.Province)
                    .Include(u => u.Address)
                        .ThenInclude(a => a.District)
                    .Include(u => u.Address)
            .ThenInclude(a => a.Ward)
                    .FirstOrDefaultAsync(u => u.Id == Id);
            if (currentUser == null || currentUser.Id != Id)
            {
                return Forbid(); // Không cho phép truy cập profile của người khác
            }

            // Get user's role name, don't try to get role ID
            var roles = await _userManager.GetRolesAsync(currentUser);
            var roleName = roles.FirstOrDefault();

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
                RoleId = roleName ?? string.Empty,
                AddressDetail = currentUser.Address?.Detail,
                ProvinceId = currentUser.Address?.ProvinceId,
                DistrictId = currentUser.Address?.DistrictId,
                WardId = currentUser.Address?.WardId
            };

            // Load tất cả Province
            ViewBag.Provinces = new SelectList(await _context.Provinces.ToListAsync(), "Id", "Name", userVm.ProvinceId);

            // Load District dựa trên ProvinceId, nếu null thì để rỗng
            var districts = userVm.ProvinceId.HasValue
                ? await _context.Districts.Where(d => d.ProvinceId == userVm.ProvinceId.Value).ToListAsync()
                : new List<District>();
            ViewBag.Districts = new SelectList(districts, "Id", "Name", userVm.DistrictId);

            // Load Ward dựa trên DistrictId, nếu null thì để rỗng
            var wards = userVm.DistrictId.HasValue
                ? await _context.Wards.Where(w => w.DistrictId == userVm.DistrictId.Value).ToListAsync()
                : new List<Ward>();
            ViewBag.Wards = new SelectList(wards, "Id", "Name", userVm.WardId);
            ViewBag.UserRoleId = roleName ?? string.Empty;
            return View(userVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(string Id, UserVm userVm)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id != Id)
            {
                return Forbid(); // Không cho phép chỉnh sửa profile của người khác
            }
            // If RoleId is null or empty, try to get role name again
            if (string.IsNullOrEmpty(userVm.RoleId))
            {
                var roles = await _userManager.GetRolesAsync(currentUser);
                userVm.RoleId = roles.FirstOrDefault() ?? string.Empty;
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
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

                // Cập nhật hoặc tạo mới Address
                if (currentUser.AddressId.HasValue)
                {
                    var address = await _context.Addresses.FindAsync(currentUser.AddressId.Value);
                    if (address != null)
                    {
                        address.Detail = userVm.AddressDetail;
                        address.ProvinceId = userVm.ProvinceId ?? 0;
                        address.DistrictId = userVm.DistrictId ?? 0;
                        address.WardId = userVm.WardId ?? 0;
                        _context.Addresses.Update(address);
                    }
                }
                else if (!string.IsNullOrEmpty(userVm.AddressDetail) && userVm.ProvinceId.HasValue && userVm.DistrictId.HasValue && userVm.WardId.HasValue)
                {
                    var newAddress = new Address
                    {
                        Detail = userVm.AddressDetail,
                        ProvinceId = userVm.ProvinceId.Value,
                        DistrictId = userVm.DistrictId.Value,
                        WardId = userVm.WardId.Value
                    };
                    _context.Addresses.Add(newAddress);
                    await _context.SaveChangesAsync();
                    currentUser.AddressId = newAddress.Id;
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

            ViewBag.Provinces = new SelectList(await _context.Provinces.ToListAsync(), "Id", "Name", userVm.ProvinceId);
            ViewBag.Districts = new SelectList(await _context.Districts.Where(d => d.ProvinceId == userVm.ProvinceId).ToListAsync(), "Id", "Name", userVm.DistrictId);
            ViewBag.Wards = new SelectList(await _context.Wards.Where(w => w.DistrictId == userVm.DistrictId).ToListAsync(), "Id", "Name", userVm.WardId);
            return View(userVm);
        }
    }
}
