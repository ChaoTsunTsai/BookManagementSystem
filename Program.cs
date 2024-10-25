using Microsoft.EntityFrameworkCore;
using BookManagementSystem.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using BookManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// 設定資料庫上下文使用 SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加控制器和視圖服務
builder.Services.AddControllersWithViews();

// 註冊密碼雜湊服務
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// 增加驗證服務，使用 Cookie 驗證
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // 配置 Cookie 認證的選項，例如登出後的重定向路徑
        options.LoginPath = "/Account/SignIn";
        options.LogoutPath = "/Account/SignOut";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie 的過期時間
    });

var app = builder.Build();

// 設定中介軟體
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 確保在 UseAuthorization 之前調用 UseAuthentication()
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();