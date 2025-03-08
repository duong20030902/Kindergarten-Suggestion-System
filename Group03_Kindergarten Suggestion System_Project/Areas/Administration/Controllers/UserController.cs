using Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels;
using Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels.ArchiveData;
using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.Services.Email;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(KindergartenSSDatabase context, RoleManager<Role> roleManager, UserManager<User> userManager, EmailSender emailSender, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _emailSender = emailSender;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _context.Users
                                        join r in _context.Roles on u.RoleId equals r.Id into roles
                                        from r in roles.DefaultIfEmpty()
                                        select new UserRoleVm
                                        {
                                            User = u,
                                            RoleName = r != null ? r.Name : "No Role Assigned",
                                            IsRoleActive = r != null && r.IsActive
                                        }).ToListAsync();

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(usersWithRoles);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Address)
                .Join(_context.UserRoles,
                    u => u.Id,
                    ur => ur.UserId,
                    (u, ur) => new { User = u, RoleId = ur.RoleId })
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new UserRoleVm
                    {
                        User = ur.User,
                        RoleName = r.Name,
                        IsRoleActive = r.IsActive
                    })
                .FirstOrDefaultAsync(u => u.User.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Lấy thông tin lịch sử gần nhất từ UserHistory
            var latestHistory = await _context.UserHistories
                .Where(uh => uh.UserId == id)
                .OrderByDescending(uh => uh.UpdatedAt)
                .FirstOrDefaultAsync();

            if (latestHistory != null)
            {
                var previousRole = await _roleManager.FindByIdAsync(latestHistory.RoleId);
                ViewBag.PreviousUserData = new PreviousUserData
                {
                    FirstName = latestHistory.FirstName,
                    LastName = latestHistory.LastName,
                    UserName = latestHistory.UserName,
                    Email = latestHistory.Email,
                    PhoneNumber = latestHistory.PhoneNumber,
                    BirthDate = latestHistory.BirthDate.ToString("MMMM d, yyyy"),
                    EmailConfirmed = latestHistory.EmailConfirmed ? "Active" : "Inactive",
                    RoleId = latestHistory.RoleId,
                    UpdatedAt = latestHistory.UpdatedAt.ToString("dddd, MMMM d, yyyy HH:mm")
                };
                ViewBag.PreviousRoleName = previousRole?.Name ?? "No Role Assigned";
            }

            return View(user);
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

            string password = new string[]
            {
                Uppercase[_random.Next(Uppercase.Length)].ToString(),
                Lowercase[_random.Next(Lowercase.Length)].ToString(),
                Digits[_random.Next(Digits.Length)].ToString(),
                Special[_random.Next(Special.Length)].ToString()
            }.Aggregate((a, b) => a + b);

            string allChars = Uppercase + Lowercase + Digits + Special;
            while (password.Length < length)
            {
                password += allChars[_random.Next(allChars.Length)];
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string userId, bool emailConfirmed)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("ToggleStatus: userId is null or empty");
                return Json(new { success = false, message = "Invalid user ID." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"ToggleStatus: User not found with ID: {userId}");
                return Json(new { success = false, message = "User not found." });
            }

            Console.WriteLine($"ToggleStatus: Updating EmailConfirmed to {emailConfirmed} for user {userId}");
            user.EmailConfirmed = emailConfirmed;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                Console.WriteLine($"ToggleStatus: Successfully updated EmailConfirmed for user {userId}");
                return Json(new { success = true });
            }

            Console.WriteLine($"ToggleStatus: Failed to update EmailConfirmed for user {userId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var userVm = new UserVm
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                RoleId = user.RoleId,
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                // Address = user.Address?.ToString() ?? "",
                EmailConfirmed = user.EmailConfirmed,
                Image = user.Image
            };

            var roles = await _roleManager.Roles.Where(r => r.Id != "b8d166f3-4953-48fc-acb8-51b0e6ee46d3").ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name", user.RoleId);
            return View(userVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, UserVm userVm)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Lưu thông tin cũ vào UserHistory trước khi cập nhật
                var userHistory = new UserHistory
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber ?? "N/A",
                    BirthDate = user.BirthDate,
                    EmailConfirmed = user.EmailConfirmed,
                    RoleId = user.RoleId, // Có thể là NULL, không gây lỗi vì bảng đã cho phép NULL
                    UpdatedAt = user.UpdatedAt ?? DateTime.Now // Nếu chưa có UpdatedAt, dùng giờ hiện tại
                };
                _context.UserHistories.Add(userHistory);
                await _context.SaveChangesAsync();

                // Cập nhật thông tin mới
                user.FirstName = userVm.FirstName;
                user.LastName = userVm.LastName;
                user.UserName = userVm.UserName;
                user.Email = userVm.Email;
                user.PhoneNumber = userVm.PhoneNumber;
                user.BirthDate = userVm.BirthDate;
                user.EmailConfirmed = userVm.EmailConfirmed;
                user.UpdatedAt = DateTime.Now;

                // Xử lý ảnh (nếu có)
                if (userVm.ImageFile != null && userVm.ImageFile.Length > 0)
                {
                    var oldImagePath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "img/users", user.Image);
                    if (System.IO.File.Exists(oldImagePath) && user.Image != "avatar-default.jpg")
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                    var uploadFolder = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "img/users");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string imageName = Guid.NewGuid().ToString() + "_" + userVm.ImageFile.FileName;
                    string filePath = System.IO.Path.Combine(uploadFolder, imageName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await userVm.ImageFile.CopyToAsync(fileStream);
                    }

                    user.Image = imageName;
                }
                else
                {
                    user.Image = user.Image; // Giữ nguyên ảnh cũ nếu không upload mới
                }

                // Cập nhật vai trò
                if (user.RoleId != userVm.RoleId)
                {
                    var oldRole = await _userManager.GetRolesAsync(user);
                    if (oldRole.Any())
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, oldRole);
                        if (!removeResult.Succeeded)
                        {
                            ModelState.AddModelError("", "Không thể xóa vai trò cũ.");
                            var roles = await _roleManager.Roles
                                .Where(r => r.Id != "b8d166f3-4953-48fc-acb8-51b0e6ee46d3")
                                .ToListAsync();
                            ViewBag.Roles = new SelectList(roles, "Id", "Name", userVm.RoleId);
                            return View(userVm);
                        }
                    }

                    var newRole = await _roleManager.FindByIdAsync(userVm.RoleId);
                    if (newRole != null)
                    {
                        var addResult = await _userManager.AddToRoleAsync(user, newRole.Name);
                        if (!addResult.Succeeded)
                        {
                            ModelState.AddModelError("", "Không thể thêm vai trò mới.");
                            var roles = await _roleManager.Roles
                                .Where(r => r.Id != "b8d166f3-4953-48fc-acb8-51b0e6ee46d3")
                                .ToListAsync();
                            ViewBag.Roles = new SelectList(roles, "Id", "Name", userVm.RoleId);
                            return View(userVm);
                        }
                    }

                    user.RoleId = userVm.RoleId; // Cập nhật RoleId trong AspNetUsers
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["success"] = "User updated successfully.";
                    return RedirectToAction("Detail", new { id = user.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var rolesList = await _roleManager.Roles
                .Where(r => r.Id != "b8d166f3-4953-48fc-acb8-51b0e6ee46d3")
                .ToListAsync();
            ViewBag.Roles = new SelectList(rolesList, "Id", "Name", userVm.RoleId);
            return View(userVm);
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

        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
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

            // Tạo PDF với khổ giấy lớn hơn (A4 mở rộng)
            using (var memoryStream = new MemoryStream())
            {
                // Định nghĩa khổ giấy lớn hơn A4 (ví dụ: 842 x 1191 điểm, gấp đôi chiều rộng A4)
                var pageSize = new PageSize(842, 1191); // Tùy chỉnh kích thước (đơn vị: điểm)
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                pdf.SetDefaultPageSize(pageSize);
                var document = new iText.Layout.Document(pdf);

                // Thiết lập khoảng cách lề
                document.SetMargins(30, 30, 30, 30); // Tăng lề để phù hợp với khổ giấy lớn

                // Tiêu đề chính
                var title = new iText.Layout.Element.Paragraph("User List Report")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(24);
                var boldFont = PdfFontFactory.CreateFont("Helvetica-Bold");
                title.SetFont(boldFont);
                document.Add(title);

                // Tiêu đề phụ (Generated on)
                var subTitle = new iText.Layout.Element.Paragraph($"Generated on: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14);
                var italicFont = PdfFontFactory.CreateFont("Helvetica-Oblique");
                subTitle.SetFont(italicFont);
                document.Add(subTitle);
                document.Add(new iText.Layout.Element.Paragraph("\n")); // Khoảng cách

                // Tạo bảng
                float[] columnWidths = { 10f, 15f, 15f, 20f, 10f, 15f, 10f, 15f, 15f }; // Điều chỉnh tỷ lệ cột cho các trường mới
                iText.Layout.Element.Table table = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(columnWidths))
                    .UseAllAvailableWidth();

                // Định dạng tiêu đề bảng
                var headerFont = PdfFontFactory.CreateFont("Helvetica");
                table.AddHeaderCell(CreateStyledCell("Full Name", headerFont, true, TextAlignment.CENTER));
                table.AddHeaderCell(CreateStyledCell("User Name", headerFont, true, TextAlignment.CENTER));
                table.AddHeaderCell(CreateStyledCell("Email", headerFont, true, TextAlignment.CENTER));
                table.AddHeaderCell(CreateStyledCell("Phone", headerFont, true, TextAlignment.CENTER));
                table.AddHeaderCell(CreateStyledCell("Birth Date", headerFont, true, TextAlignment.CENTER));
                table.AddHeaderCell(CreateStyledCell("Address", headerFont, true, TextAlignment.CENTER));
                table.AddHeaderCell(CreateStyledCell("Status", headerFont, true, TextAlignment.CENTER));
                table.AddHeaderCell(CreateStyledCell("Join Date", headerFont, true, TextAlignment.CENTER));
                table.AddHeaderCell(CreateStyledCell("Role", headerFont, true, TextAlignment.CENTER));

                // Dữ liệu bảng
                var normalFont = PdfFontFactory.CreateFont("Helvetica");
                foreach (var user in allUsers)
                {
                    table.AddCell(CreateStyledCell($"{user.User.FirstName} {user.User.LastName}", normalFont, false, TextAlignment.LEFT));
                    table.AddCell(CreateStyledCell(user.User.UserName, normalFont, false, TextAlignment.LEFT));
                    table.AddCell(CreateStyledCell(user.User.Email, normalFont, false, TextAlignment.LEFT));
                    table.AddCell(CreateStyledCell(user.User.PhoneNumber ?? "N/A", normalFont, false, TextAlignment.CENTER));
                    table.AddCell(CreateStyledCell(user.User.BirthDate.ToString("dd/MM/yyyy"), normalFont, false, TextAlignment.CENTER));
                    table.AddCell(CreateStyledCell(user.User.Address?.ToString() ?? "N/A", normalFont, false, TextAlignment.LEFT));
                    table.AddCell(CreateStyledCell(user.User.EmailConfirmed ? "Active" : "Inactive", normalFont, false, TextAlignment.CENTER));
                    table.AddCell(CreateStyledCell(user.User.CreatedAt.ToString("dd/MM/yyyy"), normalFont, false, TextAlignment.CENTER));
                    table.AddCell(CreateStyledCell(user.RoleName, normalFont, false, TextAlignment.CENTER));
                }

                // Thêm bảng vào tài liệu
                document.Add(table);
                document.Close();

                var content = memoryStream.ToArray();
                return File(content, "application/pdf", "Users.pdf");
            }
        }

        // Helper method để tạo ô với định dạng
        private iText.Layout.Element.Cell CreateStyledCell(string text, PdfFont font, bool isHeader, TextAlignment alignment)
        {
            var cell = new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(text).SetFont(font));
            cell.SetTextAlignment(alignment);
            cell.SetPadding(5);
            if (isHeader)
            {
                cell.SetBackgroundColor(new DeviceRgb(200, 220, 255)); // Màu nền cho tiêu đề
                cell.SetFontSize(12);
            }
            else
            {
                cell.SetFontSize(10);
            }
            cell.SetBorder(new iText.Layout.Borders.SolidBorder(0.5f)); // Thêm viền 0.5 đơn vị
            return cell;
        }
    }
}