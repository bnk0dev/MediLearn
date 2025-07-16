using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Models.DTOs;
using Medilearn.Models.ViewModels;
using Medilearn.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Medilearn.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly ICourseMaterialService _courseMaterialService;

        public InstructorController(ICourseService courseService, IUserService userService, ICourseMaterialService courseMaterialService)
        {
            _courseService = courseService;
            _userService = userService;
            _courseMaterialService = courseMaterialService;
        }

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

        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var instructorTcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(instructorTcNo))
                return Forbid();

            var courses = await _courseService.GetCoursesByInstructorAsync(instructorTcNo);
            return View(courses);
        }

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

            if (model.ProfileImage == null || model.ProfileImage.Length == 0)
            {
                ModelState.AddModelError("ProfileImage", "Lütfen bir dosya seçiniz.");
                return View(model);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ProfileImage", "Sadece .jpg, .jpeg veya .png dosyaları yüklenebilir.");
                return View(model);
            }

            var user = await _userService.GetUserByTCNoAsync(model.TCNo);
            if (user == null)
                return NotFound();

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
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

        // GET: Kursa ait materyaller
        [HttpGet]
        public async Task<IActionResult> CourseMaterials(int courseId)
        {
            var materials = await _courseMaterialService.GetCourseMaterialsByCourseIdAsync(courseId);
            ViewBag.CourseId = courseId;
            return View(materials);
        }

        // GET: Materyal ekleme formu
        [HttpGet]
        public IActionResult AddMaterial(int courseId)
        {
            return View(new AddMaterialViewModel { CourseId = courseId });
        }

        // POST: PDF materyal yükleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(AddMaterialViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.MaterialFile == null || model.MaterialFile.Length == 0)
            {
                ModelState.AddModelError("MaterialFile", "Dosya seçilmedi.");
                return View(model);
            }

            var extension = Path.GetExtension(model.MaterialFile.FileName).ToLowerInvariant();
            if (extension != ".pdf")
            {
                ModelState.AddModelError("MaterialFile", "Sadece PDF dosyaları yüklenebilir.");
                return View(model);
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/materials");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid() + extension;
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await model.MaterialFile.CopyToAsync(stream);
            }

            var relativePath = "/uploads/materials/" + uniqueFileName;

            // CourseMaterial tablosuna ekle
            var material = new CourseMaterial
            {
                CourseId = model.CourseId,
                MaterialPath = relativePath,
                UploadDate = DateTime.Now
            };
            await _courseMaterialService.AddCourseMaterialAsync(material);

            // Course tablosuna tek materyal için yol yaz (personel tarafı için)
            var course = await _courseService.GetCourseByIdAsync(model.CourseId);
            if (course != null)
            {
                course.MaterialFileName = relativePath;
                await _courseService.UpdateCourseAsync(course);
            }

            return RedirectToAction("MyCourses");
        }
    }
}
