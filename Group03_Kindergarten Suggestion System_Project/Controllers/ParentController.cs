using Group03_Kindergarten_Suggestion_System_Project.Models.Enums;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Group03_Kindergarten_Suggestion_System_Project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Group03_Kindergarten_Suggestion_System_Project.Controllers
{
    public class ParentController : Controller
    {
        private readonly KindergartenSSDatabase _context;
        private readonly UserManager<User> _userManager;

        public ParentController(KindergartenSSDatabase context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitRequest(string description, Guid schoolId)
        {
            var parent = await _userManager.GetUserAsync(User);
            if (parent == null)
            {
                return Unauthorized("You must be logged in to submit a request.");
            }

            var schoolExists = await _context.Schools.AnyAsync(s => s.Id == schoolId);
            if (!schoolExists)
            {
                TempData["ErrorMessage"] = "The selected school does not exist.";
                return RedirectToAction("Index", "Home");
            }

            var request = new SchoolEnrollment
            {
                SchoolId = schoolId,
                ParentId = parent.Id,
                Desctiption = description,
                Status = EnrollStatus.Pending
            };
            _context.SchoolEnrollments.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your request has been submitted successfully!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để thực hiện thao tác này";
                return RedirectToAction("Login", "Authentication"); 
            }

            var enrollment = await _context.SchoolEnrollments
                .FirstOrDefaultAsync(e => e.Id == id && e.ParentId == userId);
            if (enrollment == null)
            {
                TempData["Error"] = "Không tìm thấy đơn yêu cầu";
                return RedirectToAction("Index", "Profile");
            }

            if (enrollment.Status != EnrollStatus.Pending)
            {
                TempData["Error"] = "Chỉ có thể xóa đơn yêu cầu đang chờ duyệt";
                return RedirectToAction("Index", "Profile");
            }

            try
            {
                _context.SchoolEnrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa đơn yêu cầu thành công!";
                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index", "Profile");
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRequest(Guid id, SchoolEnrollment model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này" });
            }

            var enrollment = await _context.SchoolEnrollments
                .FirstOrDefaultAsync(e => e.Id == id && e.ParentId == userId);

            if (enrollment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn yêu cầu" });
            }

            if (enrollment.Status != EnrollStatus.Pending)
            {
                return Json(new { success = false, message = "Chỉ có thể chỉnh sửa đơn yêu cầu đang chờ duyệt" });
            }

            try
            {
                // Cập nhật chỉ mỗi trường description
                enrollment.Desctiption = model.Desctiption;
                enrollment.UpdatedAt = DateTime.Now;
                _context.Entry(enrollment).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật đơn yêu cầu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
