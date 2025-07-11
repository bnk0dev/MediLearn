using Medilearn.Models.DTOs;
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

        public InstructorController(ICourseService courseService)
        {
            _courseService = courseService;
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

    }
}
