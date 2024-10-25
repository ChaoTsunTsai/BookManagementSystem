using Microsoft.EntityFrameworkCore;
using BookManagementSystem.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using BookManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// �]�w��Ʈw�W�U��ϥ� SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// �K�[����M���ϪA��
builder.Services.AddControllersWithViews();

// ���U�K�X����A��
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// �W�[���ҪA�ȡA�ϥ� Cookie ����
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // �t�m Cookie �{�Ҫ��ﶵ�A�Ҧp�n�X�᪺���w�V���|
        options.LoginPath = "/Account/SignIn";
        options.LogoutPath = "/Account/SignOut";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie ���L���ɶ�
    });

var app = builder.Build();

// �]�w�����n��
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // �T�O�b UseAuthorization ���e�ե� UseAuthentication()
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();