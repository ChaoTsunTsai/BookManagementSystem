using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using BookManagementSystem.Data;
using BookManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;

namespace BookManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // 顯示登入頁面
        [HttpGet]
        public IActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 處理登入請求
        [HttpPost]
        public async Task<IActionResult> SignIn(string email, string password)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // 查詢用戶
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                // 驗證密碼
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    // 建立使用者的認證資訊，包含用戶 ID
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // 確保 NameIdentifier 設置正確
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("IsAdmin", user.IsAdmin.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true // 保持登入狀態
                    };

                    // 執行登入
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    // 驗證成功後可以跳轉到首頁或其他頁面
                    return RedirectToAction("Index", "Home");
                }
            }

            // 驗證失敗，顯示錯誤訊息
            ViewBag.ErrorMessage = "Invalid email or password.";
            return View();
        }

        // 顯示註冊頁面
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 處理註冊請求
        [HttpPost]
        public IActionResult Register(string username, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View();
            }

            // 檢查是否有相同的用戶名或電子郵件
            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == username || u.Email == email);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Username or Email already exists.";
                return View();
            }

            // 建立新用戶並加密密碼
            var user = new User
            {
                UserName = username,
                Email = email,
                Password = _passwordHasher.HashPassword(null, password), // 密碼加密處理
                IsAdmin = false // 預設為非管理員
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // 註冊成功後重定向到 SignIn 頁面，未設置登入狀態
            return RedirectToAction("SignIn", "Account");
        }

        // Sign Out
        [HttpPost]
        public async Task<IActionResult> SignOut()
        {
            // 執行登出
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Account");
        }
    }
}