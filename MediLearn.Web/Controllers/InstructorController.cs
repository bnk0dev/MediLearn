using Medilearn.Models.DTOs;
using Medilearn.Models.ViewModels;
using Medilearn.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Medilearn.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public InstructorController(ICourseService courseService, IUserService userService)
        {
            _courseService = courseService;
            _userService = userService;
        }

        // -------------------------------
        // PANEL INDEX
        // -------------------------------
        public async Task<IActionResult> Index()
        {
            var instructorTcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(instructorTcNo))
                return Unauthorized();

            var user = await _userService.GetUserByTCNoAsync(instructorTcNo);
            ViewBag.FullName = $"{user.FirstName} {user.LastName}";

            var courses = await _courseService.GetCoursesByInstructorAsync(instructorTcNo);
            return View(courses);
        }


        // -------------------------------
        // KURS OLUŞTURMA
        // -------------------------------
        [HttpGet]
        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(CourseDto model)
        {
            var instructorTcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(instructorTcNo))
                return Forbid();

            model.InstructorTCNo = instructorTcNo;
            model.InstructorId = instructorTcNo;

            // Bu alanlar view tarafında olmayabilir, o yüzden çıkar
            ModelState.Remove(nameof(model.InstructorTCNo));
            ModelState.Remove(nameof(model.InstructorId));
            ModelState.Remove(nameof(model.MaterialFileName));

            if (!ModelState.IsValid)
                return View(model);

            var courseId = await _courseService.AddCourseAndReturnIdAsync(model, instructorTcNo);
            if (courseId <= 0)
            {
                ModelState.AddModelError("", "Kurs oluşturulamadı.");
                return View(model);
            }

            return RedirectToAction(nameof(MyCourses));
        }

        // -------------------------------
        // EĞİTMENİN KENDİ KURSLARI
        // -------------------------------
        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var instructorTcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(instructorTcNo))
                return Forbid();

            var courses = await _courseService.GetCoursesByInstructorAsync(instructorTcNo);
            return View(courses);
        }

        // -------------------------------
        // MATERYAL EKLEME
        // -------------------------------
        [HttpGet]
        public IActionResult AddMaterial(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(int courseId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen dosya seçiniz.");
                ViewBag.CourseId = courseId;
                return View();
            }

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                await _courseService.AddCourseMaterialAsync(courseId, uniqueFileName);
                return RedirectToAction(nameof(MyCourses));
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("errorlog.txt", ex.ToString());
                ModelState.AddModelError("", "HATA: " + ex.Message);
                ViewBag.CourseId = courseId;
                return View();
            }
        }

        // -------------------------------
        // PROFİL GÖRÜNTÜLEME VE GÜNCELLEME
        // -------------------------------
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Unauthorized();

            var user = await _userService.GetUserByTCNoAsync(tcNo);
            if (user == null)
                return NotFound();

            var model = new ProfileDto
            {
                TCNo = user.TCNo,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tcNo = User.Identity?.Name;
            if (model.TCNo != tcNo)
                return BadRequest("TC No değiştirilemez.");

            var user = await _userService.GetUserByTCNoAsync(tcNo);
            if (user == null)
                return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            await _userService.UpdateUserAsync(user);

            ViewBag.Message = "Profiliniz başarıyla güncellendi.";
            return View(model);
        }

        // -------------------------------
        // PROFİL RESMİ YÜKLEME
        // -------------------------------
        [HttpGet]
        public IActionResult UpdateProfileImage()
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Unauthorized();

            var model = new ProfileImageDto { TCNo = tcNo };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileImage(ProfileImageDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetUserByTCNoAsync(model.TCNo);
            if (user == null)
                return NotFound();

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }

                user.ProfileImagePath = "/uploads/profiles/" + uniqueFileName;
                await _userService.UpdateUserAsync(user);

                TempData["Success"] = "Profil resminiz başarıyla güncellendi.";
                return RedirectToAction("UpdateProfileImage");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Dosya yüklenirken hata oluştu: " + ex.Message);
                return View(model);
            }
        }
    }
}
