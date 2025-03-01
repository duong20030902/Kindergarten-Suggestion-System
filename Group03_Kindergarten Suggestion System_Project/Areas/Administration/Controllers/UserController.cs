using Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels;
using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class UserController : Controller
    {
        private readonly KindergartenSSDatabase _context;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly EmailSender _emailSender;
        private static readonly Random _random = new Random();

        public UserController(KindergartenSSDatabase context, RoleManager<Role> roleManager, UserManager<User> userManager, EmailSender emailSender)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _emailSender = emailSender;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy người dùng có vai trò
            var usersWithRoles = await (from u in _context.Users
                                        join ur in _context.UserRoles on u.Id equals ur.UserId
                                        join r in _context.Roles on ur.RoleId equals r.Id
                                        select new UserRoleVm
                                        {
                                            User = u,
                                            RoleName = r.Name,
                                            IsRoleActive = r.IsActive
                                        }).ToListAsync();

            // Lấy người dùng không có vai trò
            var usersWithoutRoles = await (from u in _context.Users
                                           where !_context.UserRoles.Any(ur => ur.UserId == u.Id)
                                           select new UserRoleVm
                                           {
                                               User = u,
                                               RoleName = "No Role Assigned",
                                               IsRoleActive = true // Mặc định true nếu không có role
                                           }).ToListAsync();

            var allUsers = usersWithRoles.Concat(usersWithoutRoles).ToList();

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(allUsers);
        }

        [HttpGet]
        public async Task<IActionResult> AddUser()
        {
            var roles = await _roleManager.Roles
                .Where(r => r.Id != "b8d166f3-4953-48fc-acb8-51b0e6ee46d3"
                    && r.Id != "65a83d1b-66b0-4871-98a6-bd50ab14052e")
                .ToListAsync();

            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new UserVm());
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string Digits = "0123456789";
            const string Special = "!@#$%^&*";

            // Đảm bảo ít nhất 1 ký tự mỗi loại
            string password = new string[]
            {
                Uppercase[_random.Next(Uppercase.Length)].ToString(),
                Lowercase[_random.Next(Lowercase.Length)].ToString(),
                Digits[_random.Next(Digits.Length)].ToString(),
                Special[_random.Next(Special.Length)].ToString()
            }.Aggregate((a, b) => a + b);

            // Thêm các ký tự ngẫu nhiên cho đủ độ dài
            string allChars = Uppercase + Lowercase + Digits + Special;
            while (password.Length < length)
            {
                password += allChars[_random.Next(allChars.Length)];
            }

            // Xáo trộn chuỗi
            char[] passwordArray = password.ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = _random.Next(0, i + 1);
                char temp = passwordArray[i];
                passwordArray[i] = passwordArray[j];
                passwordArray[j] = temp;
            }

            return new string(passwordArray);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(UserVm userVm)
        {
            if (await _userManager.Users.AnyAsync(u => u.Email == userVm.Email))
            {
                TempData["warning"] = $"Email {userVm.Email} already exists. Please enter another email.";
                return View(userVm);
            }

            var role = await _roleManager.FindByIdAsync(userVm.RoleId);
            if (role == null)
            {
                TempData["error"] = "Invalid role.";
                return View(userVm);
            }

            var randomPassword = GenerateRandomPassword();

            var user = new User
            {
                FirstName = userVm.FirstName,
                LastName = userVm.LastName,
                UserName = userVm.UserName,
                Email = userVm.Email,
                RoleId = userVm.RoleId,
                BirthDate = DateTime.MinValue,
                Image = "avatar-default.jpg"
            };

            var createResult = await _userManager.CreateAsync(user, randomPassword);
            if (!createResult.Succeeded)
            {
                TempData["error"] = $"Unable to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}";
                return View(userVm);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, role.Name);
            if (!addToRoleResult.Succeeded)
            {
                TempData["error"] = $"Cannot assign role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}";
                return View(userVm);
            }

            try
            {
                string body = $@"
                    <h2>Welcome to Kindergarten Suggestion System!</h2>
                    <p>Dear {user.FirstName} {user.LastName},</p>
                    <p>Your account has been successfully created. Here are your login details:</p>
                    <ul>
                        <li>UserName: {user.UserName}</li>
                        <li>Email: {user.Email}</li>
                        <li>Password: {randomPassword}</li>
                        <li>Role: {role.Name}</li>
                    </ul>
                    <p>Please login with these credentials within 7 days to activate your account.</p>
                    <p>If you do not login within 7 days, your account will be locked until an admin resends the credentials.</p>
                    <p>Please change your password after your first login.</p>
                    <p>Best regards,<br>The Kindergarten SS Team</p>";

                _emailSender.SendEmail(user.Email, "Welcome to Kindergarten Suggestion System", body);
            }
            catch (Exception ex)
            {
                TempData["warning"] = "User created successfully but failed to send welcome email: " + ex.Message;
                return RedirectToAction("Index", "User");
            }

            TempData["success"] = "Account successfully created and email sent.";
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public async Task<IActionResult> ResendCredentials(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction("Index");
            }

            if (user.EmailConfirmed)
            {
                TempData["warning"] = "This user's account is already activated.";
                return RedirectToAction("Index");
            }

            var randomPassword = GenerateRandomPassword();
            var passwordResetResult = await _userManager.RemovePasswordAsync(user);
            if (!passwordResetResult.Succeeded)
            {
                TempData["error"] = "Failed to reset password.";
                return RedirectToAction("Index");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, randomPassword);
            if (!addPasswordResult.Succeeded)
            {
                TempData["error"] = "Failed to set new password.";
                return RedirectToAction("Index");
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(user.RoleId);
                var roleName = role?.Name ?? "No Role Assigned";

                string body = $@"
                    <h2>Resent Credentials - Kindergarten Suggestion System</h2>
                    <p>Dear {user.FirstName} {user.LastName},</p>
                    <p>Your account credentials have been resent. Here are your new login details:</p>
                    <ul>
                        <li>UserName: {user.UserName}</li>
                        <li>Email: {user.Email}</li>
                        <li>Password: {randomPassword}</li>
                        <li>Role: {roleName}</li>
                    </ul>
                    <p>Please login with these credentials within 7 days to activate your account.</p>
                    <p>If you do not login within 7 days, your account will remain locked.</p>
                    <p>Please change your password after your first login.</p>
                    <p>Best regards,<br>The Kindergarten SS Team</p>";

                _emailSender.SendEmail(user.Email, "Resend Credentials - Kindergarten Suggestion System", body);
                TempData["success"] = $"Credentials resent to {user.Email}.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Failed to resend credentials: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var usersWithRoles = await (from u in _context.Users
                                        join ur in _context.UserRoles on u.Id equals ur.UserId
                                        join r in _context.Roles on ur.RoleId equals r.Id
                                        select new UserRoleVm
                                        {
                                            User = u,
                                            RoleName = r.Name,
                                            IsRoleActive = true
                                        }).ToListAsync();

            var usersWithoutRoles = await (from u in _context.Users
                                           where !_context.UserRoles.Any(ur => ur.UserId == u.Id)
                                           select new UserRoleVm
                                           {
                                               User = u,
                                               RoleName = "No Role Assigned",
                                               IsRoleActive = true
                                           }).ToListAsync();

            var allUsers = usersWithRoles.Concat(usersWithoutRoles).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");
                worksheet.Cells[1, 1].Value = "Profile";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "User Name";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Status";
                worksheet.Cells[1, 6].Value = "Join Date";
                worksheet.Cells[1, 7].Value = "Role";

                int row = 2;
                foreach (var user in allUsers)
                {
                    worksheet.Cells[row, 1].Value = user.User.Image ?? "avatar-default.jpg";
                    worksheet.Cells[row, 2].Value = $"{user.User.FirstName} {user.User.LastName}";
                    worksheet.Cells[row, 3].Value = user.User.UserName;
                    worksheet.Cells[row, 4].Value = user.User.Email;
                    worksheet.Cells[row, 5].Value = user.User.EmailConfirmed ? "Active" : "Inactive";
                    worksheet.Cells[row, 6].Value = user.User.CreatedAt.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 7].Value = user.RoleName;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
            }
        }


    }
}