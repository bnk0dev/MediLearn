using Medilearn.Models.DTOs;
using Medilearn.Services.Interfaces;
using Medilearn.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Medilearn.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(UserCreateDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Status = ((UserRole)model.Role) == UserRole.Instructor
                 ? UserStatus.Pending
                 : UserStatus.Active;

            var success = await _userService.CreateUserAsync(model);
            if (!success)
            {
                ModelState.AddModelError("", "Email zaten kayıtlı.");
                return View(model);
            }

            TempData["Success"] = "Kayıt başarıyla oluşturuldu. Lütfen giriş yapınız.";
            return RedirectToAction("Login");
        }


        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrEmpty(model.TCNo))
            {
                ModelState.AddModelError("", "TC No boş olamaz.");
                return View(model);
            }
            var user = await _userService.GetUserByTCNoAsync(model.TCNo);
            if (user == null || user.Status != UserStatus.Active)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı veya aktif değil.");
                return View(model);
            }

            var hashedPassword = _userService.HashPassword(model.Password);
            var storedHash = await _userService.GetPasswordHashByTCNoAsync(model.TCNo);

            if (hashedPassword != storedHash)
            {
                ModelState.AddModelError("", "Şifre hatalı.");
                return View(model);
            }

            // Claims
            if (string.IsNullOrEmpty(user.TCNo))
            {
                ModelState.AddModelError("", "Geçersiz kullanıcı bilgisi.");
                return View(model);
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.TCNo),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Rol bazlı yönlendirme
            return user.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin"),
                UserRole.Instructor => RedirectToAction("Index", "Instructor"),
                UserRole.Personnel => RedirectToAction("Index", "Personnel"),
                _ => RedirectToAction("Login")
            };
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }




    }
}