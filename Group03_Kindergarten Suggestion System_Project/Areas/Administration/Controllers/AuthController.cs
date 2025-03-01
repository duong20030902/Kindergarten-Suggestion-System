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
                System.Diagnostics.Debug.WriteLine("ModelState không hợp lệ");
                return View(login);
            }

            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"Không tìm thấy user với email: {login.Email}");
                ModelState.AddModelError(string.Empty, "Email không tồn tại trong hệ thống.");
                return View(login);
            }

            if (!user.EmailConfirmed && DateTime.UtcNow > user.CreatedAt.AddDays(7))
            {
                System.Diagnostics.Debug.WriteLine($"Tài khoản {login.Email} đã bị khóa (quá 7 ngày)");
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ admin để gửi lại thông tin đăng nhập.");
                return View(login);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, login.Password, login.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                System.Diagnostics.Debug.WriteLine($"Đăng nhập thành công cho {login.Email}");
                if (!user.EmailConfirmed && DateTime.UtcNow <= user.CreatedAt.AddDays(7))
                {
                    user.EmailConfirmed = true;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật EmailConfirmed cho {login.Email}");
                        ModelState.AddModelError(string.Empty, "Không thể cập nhật trạng thái tài khoản. Vui lòng thử lại.");
                        return View(login);
                    }
                }
                return RedirectToAction("Index", "Home");
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
