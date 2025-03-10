using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.Services.Email;
using Group03_Kindergarten_Suggestion_System_Project.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Web;
using Microsoft.EntityFrameworkCore;

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

        // Constructor: Khởi tạo các dependency cần thiết cho controller
        public AuthenticationController(IConfiguration configuration, KindergartenSSDatabase context,
            UserManager<User> userManager, SignInManager<User> signInManager, EmailSender emailSender,
            IMemoryCache memoryCache)
        {
            _configuration = configuration; // Cấu hình ứng dụng
            _context = context; // Database context để truy cập dữ liệu
            _userManager = userManager; // Quản lý user (tạo, xóa, gán role, v.v.)
            _signInManager = signInManager; // Quản lý đăng nhập/đăng xuất
            _emailSender = emailSender; // Dịch vụ gửi email
            _memoryCache = memoryCache; // Bộ nhớ cache để lưu trữ tạm thời (OTP)
        }

        [HttpGet]
        // Hiển thị form đăng ký
        public async Task<IActionResult> Register()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ nếu đã đăng nhập
            }

            return View(); // Trả về Register nếu chưa đăng nhập
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo vệ chống tấn công CSRF
                                   // Xử lý yêu cầu đăng ký tài khoản mới
        public async Task<IActionResult> Register(RegisterVm register)
        {
            try
            {
                // Tính tuổi của người dùng dựa trên ngày sinh
                var today = DateTime.Today;
                var age = today.Year - register.BirthDate.Year;
                if (register.BirthDate.Date > today.AddYears(-age))
                {
                    age--; // Giảm tuổi nếu sinh nhật chưa đến trong năm nay
                }
                // Kiểm tra tuổi tối thiểu
                if (age < 18)
                {
                    ModelState.AddModelError("BirthDate", "You must be 18 years old to register."); // Báo lỗi nếu dưới 18 tuổi
                    return View(register); // Trả lại form với thông tin đã nhập
                }

                // Kiểm tra xem email hoặc số điện thoại đã tồn tại chưa
                var existingUser = await _userManager.Users.AnyAsync(u => u.Email == register.Email || u.PhoneNumber == register.PhoneNumber);
                if (existingUser)
                {
                    var userByEmail = await _userManager.FindByEmailAsync(register.Email);
                    if (userByEmail != null)
                    {
                        ModelState.AddModelError("Email", $"Email {register.Email} exists."); // Báo lỗi nếu email đã được sử dụng
                    }

                    var userByPhone = await _userManager.Users.AnyAsync(u => u.PhoneNumber == register.PhoneNumber);
                    if (userByPhone)
                    {
                        ModelState.AddModelError("PhoneNumber", $"Phone number {register.PhoneNumber} exists."); // Báo lỗi nếu số điện thoại đã được sử dụng
                    }

                    return View(register); // Trả lại form với thông tin đã nhập
                }

                // Kiểm tra dữ liệu đầu vào có hợp lệ không (theo validation rules trong RegisterVm)
                if (ModelState.IsValid)
                {
                    // Tạo một đối tượng User mới với thông tin từ form
                    var user = new User
                    {
                        FirstName = register.FirstName,
                        LastName = register.LastName,
                        UserName = register.UserName,
                        Email = register.Email,
                        PhoneNumber = register.PhoneNumber,
                        BirthDate = register.BirthDate,
                        EmailConfirmed = false // Chưa xác nhận email
                    };

                    // Tạo user trong database với mật khẩu đã mã hóa
                    var result = await _userManager.CreateAsync(user, register.Password);

                    // Nếu tạo user thành công
                    if (result.Succeeded)
                    {
                        // Kiểm tra xem role "Parent" có tồn tại trong database không
                        var roleExists = await _context.Roles.AnyAsync(r => r.Name == "Parent");
                        // Nếu role "Parent" không tồn tại
                        if (!roleExists)
                        {
                            // Xóa user vừa tạo để tránh lưu thông tin không hợp lệ
                            var deleteResult = await _userManager.DeleteAsync(user);
                            // Nếu xóa user thất bại
                            if (!deleteResult.Succeeded)
                            {
                                // Ghi log lỗi để debug và thông báo cho người dùng
                                Console.WriteLine("Failed to delete user: " + string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                                ModelState.AddModelError(string.Empty, "An error occurred while cleaning up the registration. Please try again.");
                                return View(register); // Trả lại form với lỗi
                            }

                            // Kiểm tra lại xem user đã thực sự bị xóa chưa
                            var deletedUser = await _userManager.FindByEmailAsync(user.Email);
                            if (deletedUser != null)
                            {
                                // Nếu user vẫn tồn tại, báo lỗi hệ thống
                                Console.WriteLine("User was not deleted from database!");
                                ModelState.AddModelError(string.Empty, "An error occurred during registration cleanup. Please try again.");
                                return View(register); // Trả lại form với lỗi
                            }

                            // Thông báo rằng role "Parent" không tồn tại và yêu cầu liên hệ admin
                            ModelState.AddModelError(string.Empty, "The 'Parent' role does not exist. Please contact an administrator to create this role.");
                            return View(register); // Trả lại form với lỗi
                        }

                        // Lấy thông tin role "Parent" từ database để lấy RoleId
                        var parentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Parent");
                        if (parentRole == null)
                        {
                            // Trường hợp này không nên xảy ra vì đã kiểm tra roleExists, nhưng thêm để an toàn
                            await _userManager.DeleteAsync(user);
                            ModelState.AddModelError(string.Empty, "An error occurred while retrieving the 'Parent' role.");
                            return View(register);
                        }

                        // Gán role "Parent" cho user trong bảng AspNetUserRoles
                        var roleResult = await _userManager.AddToRoleAsync(user, "Parent");
                        // Nếu gán role thất bại
                        if (!roleResult.Succeeded)
                        {
                            // Xóa user vừa tạo để tránh lưu thông tin không hợp lệ
                            await _userManager.DeleteAsync(user);
                            // Thêm tất cả lỗi từ roleResult vào ModelState để hiển thị
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(register); // Trả lại form với lỗi
                        }

                        // Gán RoleId vào trường RoleId của user trong bảng AspNetUsers
                        user.RoleId = parentRole.Id; // Gán RoleId từ role "Parent"
                        var updateResult = await _userManager.UpdateAsync(user); // Cập nhật user trong database
                        if (!updateResult.Succeeded)
                        {
                            // Nếu cập nhật thất bại, xóa user và báo lỗi
                            await _userManager.DeleteAsync(user);
                            foreach (var error in updateResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(register);
                        }

                        // Tạo token để xác nhận email
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var encodedToken = HttpUtility.UrlEncode(token); // Mã hóa token để dùng trong URL

                        // Tạo link xác nhận email
                        var confirmationLink = Url.Action("ConfirmEmail", "Authentication",
                            new { userId = user.Id, token = encodedToken }, Request.Scheme);

                        // Nội dung email xác nhận
                        var emailBody = $@"
                    <h2>Xác nhận tài khoản của bạn</h2>
                    <p>Cảm ơn bạn đã đăng ký. Vui lòng click vào link bên dưới để xác nhận email của bạn:</p>
                    <p><a href='{confirmationLink}'>Xác nhận email</a></p>";

                        // Gửi email xác nhận tới người dùng
                        _emailSender.SendEmail(user.Email, "Xác nhận tài khoản", emailBody);

                        // Thông báo đăng ký thành công và chuyển hướng đến trang đăng nhập
                        TempData["success"] = "Registration successful! Please check your email to confirm your account.";
                        return RedirectToAction(nameof(Login));
                    }

                    // Nếu tạo user thất bại, hiển thị các lỗi từ Identity
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                return View(register); // Trả lại form nếu có lỗi trong ModelState
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ: ghi lỗi vào TempData và trả lại form
                TempData["error"] = $"An error occurred during registration: {ex.Message}";
                return View(register);
            }
        }

        [HttpGet]
        // Hiển thị form đăng nhập
        public IActionResult Login(string? returnUrl = null)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ nếu đã đăng nhập
            }

            var model = new LoginVm
            {
                ReturnUrl = returnUrl // URL để quay lại sau khi đăng nhập thành công
            };
            return View(model); // Trả về Login nếu chưa đăng nhập
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Xử lý yêu cầu đăng nhập
        public async Task<IActionResult> Login(LoginVm login)
        {
            // Kiểm tra dữ liệu đầu vào có hợp lệ không
            if (!ModelState.IsValid)
            {
                return View(login); // Trả lại form nếu dữ liệu không hợp lệ
            }

            // Tìm user theo email
            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email does not exist."); // Báo lỗi nếu email không tồn tại
                return View(login);
            }

            // Kiểm tra xem user có role không
            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || !roles.Any())
            {
                ModelState.AddModelError(string.Empty, "Your account does not have a role assigned. Please contact support."); // Báo lỗi nếu user không có role
                return View(login);
            }

            // Kiểm tra xem user có role "Parent" không
            if (!roles.Contains("Parent"))
            {
                ModelState.AddModelError(string.Empty, "This login is only for Parents. Please use the appropriate login page."); // Báo lỗi nếu không phải role "Parent"
                return View(login);
            }

            // Kiểm tra xem email đã được xác nhận chưa
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Please confirm email before logging in."); // Báo lỗi nếu email chưa xác nhận
                return View(login);
            }

            // Kiểm tra mật khẩu của user và xử lý đăng nhập
            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                // - `isPersistent`: Nếu `RememberMe` được chọn (true), cookie xác thực sẽ được lưu lâu dài (theo thời gian hết hạn trong cấu hình)
                // - Nếu không chọn `RememberMe` (false), cookie chỉ tồn tại trong phiên hiện tại (đóng trình duyệt sẽ hết hiệu lực)
                await _signInManager.SignInAsync(user, isPersistent: login.RememberMe);

                // Kiểm tra xem ReturnUrl có tồn tại và là URL nội bộ không
                if (!string.IsNullOrEmpty(login.ReturnUrl) && Url.IsLocalUrl(login.ReturnUrl))
                {
                    return Redirect(login.ReturnUrl); // Chuyển hướng người dùng về URL ban đầu mà họ muốn truy cập
                }
                return RedirectToAction("Index", "Home"); // Nếu không có `ReturnUrl` hoặc không hợp lệ, chuyển hướng về trang chủ
            }

            ModelState.AddModelError(string.Empty, "Incorrect email or password.");
            return View(login);
        }

        [HttpGet]
        // Xử lý xác nhận email qua link
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            // Kiểm tra tham số đầu vào
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Invalid email confirmation link."; // Báo lỗi nếu link không hợp lệ
                return RedirectToAction(nameof(Login));
            }

            // Tìm user theo userId
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["error"] = "User not found."; // Báo lỗi nếu user không tồn tại
                return RedirectToAction(nameof(Login));
            }

            // Kiểm tra xem email đã được xác nhận trước đó chưa
            if (user.EmailConfirmed)
            {
                TempData["info"] = "Email has been previously confirmed."; // Thông báo nếu email đã được xác nhận
                return RedirectToAction(nameof(Login));
            }

            // Giải mã token và xác nhận email
            var decodedToken = HttpUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                TempData["success"] = "Email confirmation successful! You can log in now."; // Thông báo xác nhận thành công
            }
            else
            {
                TempData["error"] = "Email confirmation failed. Please try again."; // Báo lỗi nếu xác nhận thất bại
            }

            return RedirectToAction(nameof(Login)); // Chuyển hướng về trang đăng nhập
        }

        [HttpGet]
        // Hiển thị form quên mật khẩu
        public IActionResult ForgotPassword()
        {
            return View(); // Trả về view ForgotPassword.cshtml
        }

        [HttpPost]
        // Xử lý yêu cầu gửi OTP để đặt lại mật khẩu
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Trả lại form nếu dữ liệu không hợp lệ
            }

            // Tìm user theo email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
            {
                ModelState.AddModelError("", "Email does not exist."); // Báo lỗi nếu email không tồn tại hoặc user đã bị xóa
                return View(model);
            }

            // Tạo mã OTP ngẫu nhiên (6 chữ số)
            var otp = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"OTP_{model.Email}";
            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5)); // Lưu OTP vào cache với thời hạn 5 phút

            // Gửi email chứa OTP
            string subject = "Your OTP Code";
            string body = $"Your OTP code is: <b>{otp}</b>. It is valid for 5 minutes.";
            _emailSender.SendEmail(model.Email, subject, body);

            TempData["Success"] = "OTP has been sent to your email."; // Thông báo gửi OTP thành công
            return RedirectToAction("VerifyOtp", new { email = model.Email }); // Chuyển hướng đến trang xác nhận OTP
        }

        [HttpGet]
        // Hiển thị form xác nhận OTP
        public IActionResult VerifyOtp(string email)
        {
            return View(new VerifyOtpVm { Email = email }); // Trả về view VerifyOtp.cshtml với email
        }

        [HttpPost]
        // Xử lý xác nhận OTP
        public IActionResult VerifyOtp(VerifyOtpVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Trả lại form nếu dữ liệu không hợp lệ
            }

            // Lấy OTP từ cache
            var cacheKey = $"OTP_{model.Email}";
            if (!_memoryCache.TryGetValue(cacheKey, out string otpStored) || otpStored != model.Otp)
            {
                ModelState.AddModelError("", "OTP invalid or expired."); // Báo lỗi nếu OTP không khớp hoặc hết hạn
                return View(model);
            }

            _memoryCache.Remove(cacheKey); // Xóa OTP khỏi cache sau khi xác nhận thành công
            return RedirectToAction("ResetPassword", new { email = model.Email }); // Chuyển hướng đến trang đặt lại mật khẩu
        }

        [HttpGet]
        // Hiển thị form đặt lại mật khẩu
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword"); // Chuyển hướng về quên mật khẩu nếu email trống
            }

            var model = new ResetPasswordVm { Email = email };
            return View(model); // Trả về view ResetPassword.cshtml
        }

        [HttpPost]
        // Xử lý yêu cầu đặt lại mật khẩu
        public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Trả lại form nếu dữ liệu không hợp lệ
            }

            // Tìm user theo email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email does not exist."); // Báo lỗi nếu email không tồn tại
                return View(model);
            }

            // Tạo token để đặt lại mật khẩu và thực hiện đổi mật khẩu
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Mật khẩu đã được đặt lại. Vui lòng đăng nhập."; // Thông báo đổi mật khẩu thành công
                return RedirectToAction("Login");
            }

            // Nếu đổi mật khẩu thất bại, hiển thị lỗi
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        // Gửi lại OTP nếu người dùng yêu cầu
        public async Task<IActionResult> ResendOtp([FromBody] VerifyOtpVm model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new { message = "Invalid request" }); // Báo lỗi nếu email trống
            }

            // Tạo và lưu OTP mới
            var otp = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"OTP_{model.Email}";
            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

            // Gửi email chứa OTP mới
            string subject = "Your New OTP Code";
            string body = $"Your new OTP code is: {otp}. It is valid for 5 minutes.";
            _emailSender.SendEmail(model.Email, subject, body);

            return Ok(new { message = "OTP has been resent successfully" }); // Trả về phản hồi JSON thành công
        }

        [HttpPost]
        // Xử lý đăng xuất
        public async Task<IActionResult> Logout()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User); // Lấy thông tin user hiện tại
                if (user != null)
                {
                    await _userManager.UpdateSecurityStampAsync(user); // Cập nhật security stamp để vô hiệu hóa phiên đăng nhập cũ
                }

                await _signInManager.SignOutAsync(); // Đăng xuất khỏi Identity

                // Xóa tất cả cookie từ request
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                TempData.Clear(); // Xóa dữ liệu tạm
                HttpContext.Session.Clear(); // Xóa session

                // Nếu là yêu cầu AJAX, trả về JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                }

                return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred while logging out: {ex.Message}"; // Báo lỗi nếu có ngoại lệ
                return RedirectToAction("Index", "Home");
            }
        }
    }
}