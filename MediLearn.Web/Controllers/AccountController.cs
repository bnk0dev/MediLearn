using Medilearn.Data.Enums;
using Medilearn.Models.DTOs;
using Medilearn.Models.ViewModels;  // ProfileDto ve ProfileImageDto
using Medilearn.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Medilearn.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;

        public AccountController(IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        //DİL DEĞİŞTİRME
        [AllowAnonymous]
        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl = "/")
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                }
            );

            if (!Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }

            return LocalRedirect(returnUrl);
        }


        // GET: /Account/Register
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(UserCreateDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Status = ((UserRole)model.Role) == UserRole.Instructor
                 ? UserStatus.Pending
                 : UserStatus.Active;

            // Profil resmi set etmiyoruz, boş bırakıyoruz.

            var success = await _userService.CreateUserAsync(model);
            if (!success)
            {
                ModelState.AddModelError("", "Email veya TCNo zaten kayıtlı.");
                return View(model);
            }

            TempData["Success"] = "Kayıt başarıyla oluşturuldu. Lütfen giriş yapınız.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [AllowAnonymous]
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

            var profileImagePath = string.IsNullOrEmpty(user.ProfileImagePath)
                ? "/uploads/profiles/default.png"
                : user.ProfileImagePath;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.TCNo),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? ""),
                new Claim("ProfileImage", profileImagePath)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return user.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin"),
                UserRole.Instructor => RedirectToAction("Index", "Instructor"),
                UserRole.Personnel => RedirectToAction("Index", "Personnel"),
                _ => RedirectToAction("Login")
            };
        }

        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // PROFİL BİLGİLERİ GÖRÜNTÜLE VE GÜNCELLE
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var tcNo = User.Identity.Name;
            var user = await _userService.GetUserByTCNoAsync(tcNo);
            if (user == null) return NotFound();

            var model = new ProfileDto
            {
                TCNo = user.TCNo ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userService.GetUserByTCNoAsync(model.TCNo);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            await _userService.UpdateUserAsync(user);

            TempData["Success"] = "Profiliniz güncellendi.";
            return View(model);
        }

        // PROFİL RESMİ GÜNCELLEME SAYFASI
        [HttpGet]
        public IActionResult UpdateProfileImage()
        {
            var tcNo = User.Identity.Name;
            var model = new ProfileImageDto { TCNo = tcNo };
            return View(model);
        }

        // PROFİL RESMİ GÜNCELLEME POST
        [HttpPost]
        public async Task<IActionResult> UpdateProfileImage(ProfileImageDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userService.GetUserByTCNoAsync(model.TCNo);
            if (user == null) return NotFound();

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfileImage.CopyToAsync(fileStream);
            }

            user.ProfileImagePath = "/uploads/profiles/" + uniqueFileName;

            await _userService.UpdateUserAsync(user);

            TempData["Success"] = "Profil resmi güncellendi.";

            return RedirectToAction("Profile");
        }
    }
}
