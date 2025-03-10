using Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels;
using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    //[Authorize(Roles = "Admin")]
    public class SchoolController : Controller
    {
        private readonly KindergartenSSDatabase _context;

        public SchoolController(KindergartenSSDatabase context)
        {
            _context = context;
        }

        // GET: /Administration/School
        public async Task<IActionResult> Index()
        {

            var schools = await _context.Schools
    .Include(s => s.Address)
        .ThenInclude(a => a.Ward)
    .Include(s => s.Address)
        .ThenInclude(a => a.District)
    .Include(s => s.Address)
        .ThenInclude(a => a.Province)
    .Include(s => s.ChildAge)
    .Include(s => s.EducationMethod)
    .Include(s => s.SchoolType)
    .Include(s => s.Creator)

    .Include(s => s.Acceptor)

    .Include(s => s.ShoolOwner)
    .ToListAsync();


            return View(schools);
        }

    
        // GET: /Administration/School/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var school = await _context.Schools
                .Include(s => s.Address)
                    .ThenInclude(a => a.Ward)
                .Include(s => s.Address)
                    .ThenInclude(a => a.District)
                .Include(s => s.Address)
                    .ThenInclude(a => a.Province)
                .Include(s => s.ChildAge)
                .Include(s => s.EducationMethod)
                .Include(s => s.SchoolType)
                .Include(s => s.Creator)
                .Include(s => s.Acceptor)
                .Include(s => s.ShoolOwner)

                .FirstOrDefaultAsync(s => s.Id == id); // Lấy đúng 1 trường theo ID

            if (school == null)
            {
                return NotFound();
            }

            return View(school);
        }


        // GET: /Administration/School/Create
        [HttpGet]
        public IActionResult Create()
        {
            SchoolVM schoolVM = new SchoolVM();
            ViewData["Provinces"] = new SelectList(_context.Provinces, "Id", "Name");
            ViewData["Districts"] = new SelectList(new List<District>(), "Id", "Name");
            ViewData["Wards"] = new SelectList(new List<Ward>(), "Id", "Name");
            ViewBag.ChildAges = new SelectList(
            _context.ChildAges,
            "Id",
            "Name",
            schoolVM.ChildAgeId // Đặt giá trị đã chọn
        );
            //ViewData["EducationMethods"] = new SelectList(_context.EducationMethods, "Id", "MethodName");
            //ViewData["SchoolTypes"] = new SelectList(_context.SchoolTypes, "Id", "TypeName");
            //ViewData["Creator"] = new SelectList(_context.Users, "Id", "FullName");
            //ViewData["Acceptor"] = new SelectList(_context.Users, "Id", "FullName");
            //ViewData["SchoolOwners"] = new SelectList(_context.Users, "Id", "FullName");
            ViewBag.EducationMethods = new SelectList(
                _context.EducationMethods,
                "Id",
                "Name",
                schoolVM.EducationMethodId // Đặt giá trị đã chọn
            );

            ViewBag.SchoolTypes = new SelectList(
                _context.SchoolTypes,
                "Id",
                "Name",
                schoolVM.SchoolTypeId // Đặt giá trị đã chọn
            );

            ViewBag.SchoolOwners = new SelectList(
             _context.Users
                .Where(u => u.IsActive && !u.IsDeleted && u.RoleId.Equals("1c0e3aa4-9b9d-4065-86b5-a28cfe9d2c52"))
                .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                .ToList(),
            "Id",
            "FullName",
            schoolVM.SchoolOwnerId // Đặt giá trị đã chọn
        );

            ViewBag.Creator = new SelectList(
                _context.Users
                   .Where(u => u.IsActive && !u.IsDeleted)
                   .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                   .ToList(),
               "Id",
               "FullName",
               schoolVM.CreatorId // Đặt giá trị đã chọn
           );

            ViewBag.Acceptor = new SelectList(
                 _context.Users
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                    .ToList(),
                "Id",
                "FullName",
                schoolVM.AcceptorId // Đặt giá trị đã chọn
            );

            return View();
        }

        // Xử lý việc tạo mới trường học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SchoolVM model)
        {
            if (ModelState.IsValid)
            {
                var school = new School
                {
                    SchoolCode = model.SchoolCode,
                    Name = model.Name,
                    FeeFrom = model.FeeFrom,
                    FeeTo = model.FeeTo,
                    Phone = model.Phone,
                    Email = model.Email,
                    Description = model.Description,
                    Status = model.Status,
                    TotalRatingCount = model.TotalRatingCount,
                    TotalRating = model.TotalRating,
                    Address = new Address
                    {
                        ProvinceId = model.Address.ProvinceId,
                        DistrictId = model.Address.DistrictId,
                        WardId = model.Address.WardId,
                        Detail = model.Address.Detail
                    },
                    ChildAgeId = model.ChildAgeId,
                    EducationMethodId = model.EducationMethodId,
                    SchoolTypeId = model.SchoolTypeId,
                    CreatorId = model.CreatorId,
                    AcceptorId = model.AcceptorId,
                    ShoolOwnerId = model.SchoolOwnerId
                };

                _context.Schools.Add(school);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

          

            return View(model);
        }





        public async Task<IActionResult> Edit(Guid id)
        {
            var school = await _context.Schools
                .Include(s => s.Address)
                    .ThenInclude(a => a.Province)
                .Include(s => s.Address)
                    .ThenInclude(a => a.District)
                .Include(s => s.Address)
                    .ThenInclude(a => a.Ward)
                .Include(s => s.ChildAge)
                .Include(s => s.EducationMethod)
                .Include(s => s.SchoolType)
                .Include(s => s.Creator)
                .Include(s => s.Acceptor)
                .Include(s => s.ShoolOwner)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (school == null)
            {
                return NotFound();
            }

            // Ánh xạ từ School sang SchoolVM
            var schoolVM = new SchoolVM
            {
                Id = school.Id,
                SchoolCode = school.SchoolCode,
                Name = school.Name,
                FeeFrom = school.FeeFrom,
                FeeTo = school.FeeTo,
                Phone = school.Phone,
                Email = school.Email,
                Description = school.Description,
                Status = school.Status,
                TotalRatingCount = school.TotalRatingCount,
                TotalRating = school.TotalRating,
                AddressId = school.AddressId,
                ChildAgeId = school.ChildAgeId,
                EducationMethodId = school.EducationMethodId,
                SchoolTypeId = school.SchoolTypeId,
                CreatorId = school.CreatorId,
                AcceptorId = school.AcceptorId,
                SchoolOwnerId = school.ShoolOwnerId,
                Address = school.Address != null ? new AddressVM
                {
                    Id = school.Address.Id,
                    Detail = school.Address.Detail,
                    ProvinceId = school.Address.ProvinceId,
                    DistrictId = school.Address.DistrictId,
                    WardId = school.Address.WardId
                } : new AddressVM()
            };



            // Load danh sách School Owners
            ViewBag.SchoolOwners = new SelectList(
                await _context.Users
                    .Where(u => u.IsActive && !u.IsDeleted && u.RoleId.Equals("1c0e3aa4-9b9d-4065-86b5-a28cfe9d2c52"))
                    .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                    .ToListAsync(),
                "Id",
                "FullName",
                schoolVM.SchoolOwnerId // Đặt giá trị đã chọn
            );

            ViewBag.Creator = new SelectList(
                await _context.Users
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                    .ToListAsync(),
                "Id",
                "FullName",
                schoolVM.CreatorId // Đặt giá trị đã chọn
            );

            ViewBag.Acceptor = new SelectList(
                await _context.Users
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                    .ToListAsync(),
                "Id",
                "FullName",
                schoolVM.AcceptorId // Đặt giá trị đã chọn
            );

            // Xử lý các dropdown liên quan đến Address
            ViewBag.Provinces = new SelectList(
                _context.Provinces,
                "Id",
                "Name",
                schoolVM.Address?.ProvinceId // Đặt giá trị đã chọn
            );

            ViewBag.Districts = new SelectList(
                schoolVM.Address != null && schoolVM.Address.ProvinceId != 0
                    ? _context.Districts.Where(d => d.ProvinceId == schoolVM.Address.ProvinceId)
                    : new List<District>(),
                "Id",
                "Name",
                schoolVM.Address?.DistrictId // Đặt giá trị đã chọn
            );

            ViewBag.Wards = new SelectList(
                schoolVM.Address != null && schoolVM.Address.DistrictId != 0
                    ? _context.Wards.Where(w => w.DistrictId == schoolVM.Address.DistrictId)
                    : new List<Ward>(),
                "Id",
                "Name",
                schoolVM.Address?.WardId // Đặt giá trị đã chọn
            );

            ViewBag.ChildAges = new SelectList(
                _context.ChildAges,
                "Id",
                "Name",
                schoolVM.ChildAgeId // Đặt giá trị đã chọn
            );

            ViewBag.EducationMethods = new SelectList(
                _context.EducationMethods,
                "Id",
                "Name",
                schoolVM.EducationMethodId // Đặt giá trị đã chọn
            );

            ViewBag.SchoolTypes = new SelectList(
                _context.SchoolTypes,
                "Id",
                "Name",
                schoolVM.SchoolTypeId // Đặt giá trị đã chọn
            );

            return View(schoolVM);
        }


        // Action để lấy danh sách Districts theo ProvinceId
        [HttpGet]
        public IActionResult GetDistricts(int provinceId)
        {
            var districts = _context.Districts
                .Where(d => d.ProvinceId == provinceId)
                .Select(d => new { id = d.Id, name = d.Name })
                .ToList();
            return Json(districts);
        }

        // Action để lấy danh sách Wards theo DistrictId
        [HttpGet]
        public IActionResult GetWards(int districtId)
        {
            var wards = _context.Wards
                .Where(w => w.DistrictId == districtId)
                .Select(w => new { id = w.Id, name = w.Name })
                .ToList();
            return Json(wards);
        }


      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SchoolVM school)
        {
            if (id != school.Id)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(school);
            }

            var existingSchool = await _context.Schools
                .Include(s => s.Address)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingSchool == null)
            {
                return NotFound();
            }

            // Cập nhật các trường không null
            existingSchool.Name = !string.IsNullOrEmpty(school.Name) ? school.Name : existingSchool.Name;
            existingSchool.Phone = !string.IsNullOrEmpty(school.Phone) ? school.Phone : existingSchool.Phone;
            existingSchool.Description = !string.IsNullOrEmpty(school.Description) ? school.Description : existingSchool.Description;
            existingSchool.Email = !string.IsNullOrEmpty(school.Email) ? school.Email : existingSchool.Email;

            // Xử lý Address
            Address address;
            if (existingSchool.Address == null)
            {
                address = new Address
                {
                    Id = Guid.NewGuid()
                };
                existingSchool.Address = address;
                _context.Addresses.Add(address); // Thêm mới nếu chưa có
            }
            else
            {
                address = existingSchool.Address;
            }

            // Cập nhật thông tin Address từ form
            address.ProvinceId = school.Address.ProvinceId; 
            address.DistrictId = school.Address.DistrictId;
            address.WardId = school.Address.WardId;
            address.Detail = !string.IsNullOrEmpty(school.Address.Detail) ? school.Address.Detail : address.Detail;

            // Gán AddressId vào existingSchool
            existingSchool.AddressId = address.Id;

            // Cập nhật các trường khác
            existingSchool.ChildAgeId = school.ChildAgeId != Guid.Empty ? school.ChildAgeId : Guid.Parse("550e8400-e29b-41d4-a716-446655440005");
            existingSchool.EducationMethodId = school.EducationMethodId != Guid.Empty ? school.EducationMethodId : Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
            existingSchool.SchoolTypeId = school.SchoolTypeId != Guid.Empty ? school.SchoolTypeId : Guid.Parse("550e8400-e29b-41d4-a716-446655440002");

            existingSchool.CreatorId = !string.IsNullOrEmpty(school.CreatorId) ? school.CreatorId : "1c0e3aa4-9b9d-4065-86b5-a28cfe9d2c5d";
            existingSchool.AcceptorId = !string.IsNullOrEmpty(school.AcceptorId) ? school.AcceptorId : "1c0e3aa4-9b9d-4065-86b5-a28cfe9d2c5d";
            existingSchool.ShoolOwnerId = !string.IsNullOrEmpty(school.SchoolOwnerId) ? school.SchoolOwnerId : "1c0e3aa4-9b9d-4065-86b5-a28cfe9d2c5d";

            // Cập nhật thời gian sửa đổi
            existingSchool.UpdatedAt = DateTime.Now;

            // Chuyển đổi Status từ enum sang kiểu int trước khi lưu
            if (Enum.IsDefined(typeof(SchoolStatus), school.Status))
            {
                existingSchool.Status = (SchoolStatus)school.Status;
            }

            try
            {
                _context.Schools.Update(existingSchool);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật. Vui lòng thử lại!");
                return View(school);
            }
        }




    }
}
