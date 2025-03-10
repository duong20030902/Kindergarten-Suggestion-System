using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Group03_Kindergarten_Suggestion_System_Project.Services.Email;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<KindergartenSSDatabase>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnect"),
        sqlServerOptionsAction: sqlOptions =>
        {
            // Thử lại 5 lần nếu lỗi kết nối
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(120); // Timeout lệnh SQL là 120 giây
        })
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)); // Không theo dõi truy vấn để tăng hiệu suất

builder.Services.AddControllersWithViews();

// Cấu hình xác thực với hai scheme: ParentAuth và AdminAuth (Sử dụng Cookie)
builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Scheme mặc định
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie("ParentAuth", options => // Cấu hình xác thực cho Parent
    {
        options.LoginPath = "/Authentication/Login";
        options.LogoutPath = "/Authentication/Logout";
        options.AccessDeniedPath = "/Authentication/AccessDenied"; // Đường dẫn khi bị từ chối truy cập
        options.ExpireTimeSpan = TimeSpan.FromDays(1); // Cookie hết hạn sau 1 ngày
        options.SlidingExpiration = true; // Gia hạn cookie khi hoạt động
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Events = new CookieAuthenticationEvents
        {
            OnSigningOut = async context =>
            {
                context.CookieOptions.Expires = DateTime.Now.AddDays(-1);
            }
        };
    })
    .AddCookie("AdminAuth", options => // Cấu hình xác thực cho admin
    {
        options.LoginPath = "/Administration/Auth/Login";
        options.LogoutPath = "/Administration/Auth/Logout";
        options.AccessDeniedPath = "/Authentication/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Events = new CookieAuthenticationEvents
        {
            OnSigningOut = async context =>
            {
                context.CookieOptions.Expires = DateTime.Now.AddDays(-1);
            }
        };
    });

// Cấu hình Identity - Quản lý người dùng và vai trò
builder.Services.AddIdentity<User, Role>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Email confirmation requirement
    options.SignIn.RequireConfirmedEmail = false; // Tắt yêu cầu chung để AuthController hoạt động
})
    .AddEntityFrameworkStores<KindergartenSSDatabase>() // Lưu trữ trong database
    .AddDefaultTokenProviders(); // Hỗ trợ tạo token (ví dụ: reset mật khẩu)

builder.Services.AddDistributedMemoryCache(); // Cache trong bộ nhớ
builder.Services.AddTransient<EmailSender>(); // Dịch vụ gửi email

var app = builder.Build();

// Khởi tạo tài khoản Admin mặc định
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<User>>(); // Quản lý user
    var roleManager = services.GetRequiredService<RoleManager<Role>>(); // Quản lý role

    // Tạo role "Admin" nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var role = new Role
        {
            Name = "Admin"
        };
        await roleManager.CreateAsync(role);
    }

    // Tạo tài khoản Admin nếu chưa tồn tại
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = "admin",
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            BirthDate = DateTime.Now,
            RoleId = (await roleManager.FindByNameAsync("Admin")).Id,
            EmailConfirmed = true // Bỏ qua xác nhận email
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Tạo với mật khẩu mặc định
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin"); // Gán role Admin
        }
        else
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine(error.Description);
            }
        }
    }
}

// Middleware pipeline - Xử lý request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Trang lỗi khi không ở chế độ Development
    app.UseHsts(); // Bảo mật HTTPS
}

app.UseHttpsRedirection(); // Chuyển hướng sang HTTPS
app.UseStaticFiles(); // Phục vụ file tĩnh (CSS, JS, hình ảnh)
app.UseRouting(); // Định tuyến request
app.UseAuthentication(); // Xác thực người dùng
app.UseAuthorization(); // Phân quyền

// Định tuyến URL
app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();