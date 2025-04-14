using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connStr = $"server={Environment.GetEnvironmentVariable("DB_HOST")};" +
              $"port={Environment.GetEnvironmentVariable("DB_PORT")};" +
              $"database={Environment.GetEnvironmentVariable("DB_NAME")};" +
              $"user={Environment.GetEnvironmentVariable("DB_USER")};" +
              $"password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
              $"SslMode=Required";

Console.WriteLine($"Connection String: {connStr}");

// Cấu hình kết nối MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connStr, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Cấu hình Identity với ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Cấu hình mật khẩu (như đã làm trước đó)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Cấu hình yêu cầu tài khoản đã xác minh (nếu cần)s
    options.SignIn.RequireConfirmedAccount = false; // Hoặc true nếu bạn muốn yêu cầu xác minh
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cấu hình cookie đăng nhập
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";  // Trang đăng nhập
    options.AccessDeniedPath = "/Account/AccessDenied"; // Trang lỗi quyền
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Thời gian hết hạn của cookie
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Cấu hình middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // Quan trọng!
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Container}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
