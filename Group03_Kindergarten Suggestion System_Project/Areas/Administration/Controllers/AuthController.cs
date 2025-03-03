using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.ViewModels;
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

            // Chỉ kiểm tra thời gian 7 ngày, không yêu cầu EmailConfirmed trước
            if (!user.EmailConfirmed && DateTime.UtcNow > user.CreatedAt.AddDays(7))
            {
                Console.WriteLine($"Tài khoản {login.Email} đã bị khóa (quá 7 ngày)");
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ admin để gửi lại thông tin đăng nhập.");
                return View(login);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, login.Password, login.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                Console.WriteLine($"Đăng nhập thành công cho {login.Email}");
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
    }
}