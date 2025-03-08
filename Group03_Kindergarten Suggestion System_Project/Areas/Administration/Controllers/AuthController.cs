using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm login)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState không hợp lệ");
                return View(login);
            }

            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null)
            {
                Console.WriteLine($"Không tìm thấy user với email: {login.Email}");
                ModelState.AddModelError(string.Empty, "Email không tồn tại trong hệ thống.");
                return View(login);
            }

            // Kiểm tra xem user có role không
            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || !roles.Any())
            {
                Console.WriteLine($"Tài khoản {login.Email} không có role nào được gán.");
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn không có vai trò nào được gán. Vui lòng liên hệ hỗ trợ.");
                return View(login);
            }

            // Kiểm tra xem user có role "Parent" không
            if (roles.Contains("Parent"))
            {
                Console.WriteLine($"Tài khoản {login.Email} có role Parent, không được phép đăng nhập ở đây.");
                ModelState.AddModelError(string.Empty, "This login is not for Parents. Please use the Parent login page.");
                return View(login);
            }

            // Chỉ kiểm tra thời gian 7 ngày, không yêu cầu EmailConfirmed trước
            if (!user.EmailConfirmed && DateTime.UtcNow > user.CreatedAt.AddDays(7))
            {
                Console.WriteLine($"Tài khoản {login.Email} đã bị khóa (quá 7 ngày)");
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ admin để gửi lại thông tin đăng nhập.");
                return View(login);
            }

            // Kiểm tra mật khẩu
            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Đăng nhập với scheme mặc định (sẽ sử dụng AdminAuth dựa trên LoginPath)
                await _signInManager.SignInAsync(user, new AuthenticationProperties
                {
                    IsPersistent = login.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1) // Tùy chỉnh thời gian hết hạn
                });

                Console.WriteLine($"Đăng nhập thành công cho {login.Email} với scheme AdminAuth");

                if (!user.EmailConfirmed && DateTime.UtcNow <= user.CreatedAt.AddDays(7))
                {
                    user.EmailConfirmed = true;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        Console.WriteLine($"Lỗi cập nhật EmailConfirmed cho {login.Email}: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                        ModelState.AddModelError(string.Empty, "Không thể cập nhật trạng thái tài khoản. Vui lòng thử lại.");
                        return View(login);
                    }
                    Console.WriteLine($"EmailConfirmed cập nhật thành true cho {login.Email}");
                }
                return RedirectToAction("Index", "Home", new { area = "Administration" });
            }

            Console.WriteLine($"Đăng nhập thất bại cho {login.Email} - Mật khẩu sai?");
            ModelState.AddModelError(string.Empty, "Mật khẩu không đúng.");
            return View(login);
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            Console.WriteLine("You do not have permission to access this page."); 
            return RedirectToAction("Index", "Home", new { area = "Administration" });
        }
    }
}