using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.Services.Email;
using Group03_Kindergarten_Suggestion_System_Project.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Group03_Kindergarten_Suggestion_System_Project.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly KindergartenSSDatabase _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;

        public AuthenticationController(IConfiguration configuration, KindergartenSSDatabase context,
            UserManager<User> userManager, SignInManager<User> signInManager, EmailSender emailSender,
            IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm register)
        {
            try
            {
                var today = DateTime.Today;
                var age = today.Year - register.BirthDate.Year;
                if (register.BirthDate.Date > today.AddYears(-age))
                {
                    age--;
                }
                if (age < 18)
                {
                    ModelState.AddModelError("BirthDate", "You must be 18 years old to register.");
                    return View(register);
                }

                var existingUser = await _userManager.Users.AnyAsync(u => u.Email == register.Email || u.PhoneNumber == register.PhoneNumber);
                if (existingUser)
                {
                    var userByEmail = await _userManager.FindByEmailAsync(register.Email);
                    if (userByEmail != null)
                    {
                        ModelState.AddModelError("Email", $"Email {register.Email} exists.");
                    }

                    var userByPhone = await _userManager.Users.AnyAsync(u => u.PhoneNumber == register.PhoneNumber);
                    if (userByPhone)
                    {
                        ModelState.AddModelError("PhoneNumber", $"Phone number {register.PhoneNumber} exists.");
                    }

                    return View(register);
                }

                if (ModelState.IsValid)
                {
                    var user = new User
                    {
                        FirstName = register.FirstName,
                        LastName = register.LastName,
                        UserName = register.UserName,
                        Email = register.Email,
                        PhoneNumber = register.PhoneNumber,
                        BirthDate = register.BirthDate,
                        EmailConfirmed = false
                    };

                    var result = await _userManager.CreateAsync(user, register.Password);

                    if (result.Succeeded)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var encodedToken = HttpUtility.UrlEncode(token);

                        var confirmationLink = Url.Action("ConfirmEmail", "Authentication",
                            new { userId = user.Id, token = encodedToken }, Request.Scheme);

                        // Tạo nội dung email
                        var emailBody = $@"
        <h2>Xác nhận tài khoản của bạn</h2>
        <p>Cảm ơn bạn đã đăng ký. Vui lòng click vào link bên dưới để xác nhận email của bạn:</p>
        <p><a href='{confirmationLink}'>Xác nhận email</a></p>";

                        _emailSender.SendEmail(user.Email, "Xác nhận tài khoản", emailBody);

                        TempData["success"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.";
                        return RedirectToAction(nameof(Login));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                return View(register);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Register: {ex.Message}");
                TempData["error"] = "Có lỗi xảy ra trong quá trình đăng ký.";
                return View(register);
            }
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginVm
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm login)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(login.Email);
                if (user != null)
                {
                    if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError("", "Please confirm the email before login.");
                        return View(login);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user.UserName, login.Password, login.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(login.ReturnUrl) && Url.IsLocalUrl(login.ReturnUrl))
                        {
                            return Redirect(login.ReturnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                }
                else
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                }
            }
            return View(login);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Invalid email confirmation link.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(Login));
            }

            if (user.EmailConfirmed)
            {
                TempData["info"] = "Email đã được xác nhận trước đó.";
                return RedirectToAction(nameof(Login));
            }

            var decodedToken = HttpUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                TempData["success"] = "Xác nhận email thành công! Bạn có thể đăng nhập ngay bây giờ.";
            }
            else
            {
                TempData["error"] = "Xác nhận email thất bại. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
            {
                ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                return View(model);
            }

            // Tạo mã OTP 6 chữ số
            var otp = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"OTP_{model.Email}";

            // Lưu OTP vào bộ nhớ cache với thời gian hiệu lực 5 phút
            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

            // Gửi email chứa OTP
            string subject = "Your OTP Code";
            string body = $"Your OTP code is: <b>{otp}</b>. It is valid for 5 minutes.";
            _emailSender.SendEmail(model.Email, subject, body);

            TempData["Success"] = "OTP đã được gửi đến email của bạn.";
            return RedirectToAction("VerifyOtp", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            return View(new VerifyOtpVm { Email = email });
        }

        // Xác nhận OTP
        [HttpPost]
        public IActionResult VerifyOtp(VerifyOtpVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cacheKey = $"OTP_{model.Email}";
            if (!_memoryCache.TryGetValue(cacheKey, out string otpStored) || otpStored != model.Otp)
            {
                ModelState.AddModelError("", "OTP không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }
            // Nếu OTP hợp lệ, cho phép đặt lại mật khẩu
            _memoryCache.Remove(cacheKey);
            return RedirectToAction("ResetPassword", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordVm { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email không hợp lệ.");
                return View(model);
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Mật khẩu đã được đặt lại. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResendOtp([FromBody] VerifyOtpVm model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new { message = "Invalid request" });
            }

            // Tạo OTP mới
            var otp = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"OTP_{model.Email}";

            // Lưu OTP vào bộ nhớ cache với thời gian hiệu lực 5 phút
            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

            // Gửi lại email OTP
            string subject = "Your New OTP Code";
            string body = $"Your new OTP code is: {otp}. It is valid for 5 minutes.";
            _emailSender.SendEmail(model.Email, subject, body);

            return Ok(new { message = "OTP has been resent successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Update security stamp to invalidate all existing cookies
                    await _userManager.UpdateSecurityStampAsync(user);

                    // Sign out from all schemes
                    await _signInManager.SignOutAsync();

                    // Clear authentication cookie
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // Clear all cookies
                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookie);
                    }

                    // Clear TempData
                    TempData.Clear();

                    // Clear Session if you're using it
                    HttpContext.Session.Clear();
                }

                // Return JSON if it's an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error in Logout: {ex.Message}");
                TempData["error"] = "Có lỗi xảy ra trong quá trình đăng xuất.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
