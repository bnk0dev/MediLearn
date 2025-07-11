using Medilearn.Models.DTOs;
using Medilearn.Models.ViewModels;
using Medilearn.Services.Interfaces;
using Medilearn.Services.Services;
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
        public IActionResult AddMaterial(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(int courseId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    ModelState.AddModelError("", "Lütfen dosya seçiniz.");
                    ViewBag.CourseId = courseId;
                    return View();
                }

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

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Instructor/Profile
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

        // POST: /Instructor/Profile
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

    }
}
