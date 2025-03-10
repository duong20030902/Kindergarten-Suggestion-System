using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo vệ chống tấn công CSRF
        // Xử lý đăng nhập
        public async Task<IActionResult> Login(LoginVm login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            // Tìm user bằng email
            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email không tồn tại trong hệ thống.");
                return View(login);
            }

            // Kiểm tra xem user có role không
            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || !roles.Any())
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn không có vai trò nào được gán. Vui lòng liên hệ hỗ trợ.");
                return View(login);
            }

            // Không cho phép Parent dùng trang đăng nhập này
            if (roles.Contains("Parent"))
            {
                ModelState.AddModelError(string.Empty, "This login is not for Parents. Please use the Parent login page.");
                return View(login);
            }

            // Kiểm tra email xác nhận trong 7 ngày kể từ khi tạo tài khoản
            if (!user.EmailConfirmed && DateTime.UtcNow > user.CreatedAt.AddDays(7))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ admin để gửi lại thông tin đăng nhập.");
                return View(login);
            }

            // Kiểm tra mật khẩu
            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Đăng nhập với Identity
                await _signInManager.SignInAsync(user, login.RememberMe);

                // Tạo claims cho scheme xác thực Admin
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "Admin")
                };
                var claimsIdentity = new ClaimsIdentity(claims, "AdminAuth");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = login.RememberMe, // Lưu cookie nếu chọn "Remember Me"
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1) // Hết hạn sau 1 ngày
                };

                // Đăng nhập với scheme AdminAuth
                await HttpContext.SignInAsync("AdminAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                // Cập nhật User hiện tại ngay lập tức
                HttpContext.User = new ClaimsPrincipal(claimsIdentity);

                // Tự động xác nhận email nếu trong 7 ngày đầu
                if (!user.EmailConfirmed && DateTime.UtcNow <= user.CreatedAt.AddDays(7))
                {
                    user.EmailConfirmed = true;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Không thể cập nhật trạng thái tài khoản. Vui lòng thử lại.");
                        return View(login);
                    }
                }
                return RedirectToAction("Index", "Home", new { area = "Administration" });
            }

            ModelState.AddModelError(string.Empty, "Mật khẩu không đúng.");
            return View(login);
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpGet]
        // Trang từ chối truy cập
        public IActionResult AccessDenied()
        {
            return BadRequest("You do not have permission to access this page.");
        }
    }
}