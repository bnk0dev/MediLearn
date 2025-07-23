using Medilearn.Models.DTOs;
using Medilearn.Models.ViewModels;
using Medilearn.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medilearn.Web.Controllers
{
    [Authorize(Roles = "Personnel")]
    public class PersonnelController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IUserService _userService;

        // Servisler constructor ile alınır
        public PersonnelController(
            ICourseService courseService,
            IEnrollmentService enrollmentService,
            IUserService userService)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _userService = userService;
        }

        // Personelin ana sayfası: tamamlanan ve kayıtlı kursları listeler
        public async Task<IActionResult> Index()
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Forbid();

            // Kullanıcının adı ViewBag'e konur
            var user = await _userService.GetUserByTCNoAsync(tcNo);
            ViewBag.FullName = user != null ? $"{user.FirstName} {user.LastName}" : "Personel";

            // Tamamlanan ve kayıtlı kurslar çekilir
            var completedCourses = await _enrollmentService.GetCompletedCoursesByPersonnelAsync(tcNo);
            var enrolledCourses = await _enrollmentService.GetEnrolledCoursesByPersonnelAsync(tcNo);

            var model = new PersonnelDashboardViewModel
            {
                CompletedCourses = completedCourses,
                EnrolledCourses = enrolledCourses
            };

            return View(model);
        }

        // Kurs detaylarını gösterir, yalnızca kayıtlı personel görebilir
        public async Task<IActionResult> CourseDetails(int id)
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Unauthorized();

            var isEnrolled = await _enrollmentService.IsEnrolledAsync(tcNo, id);
            if (!isEnrolled)
                return Forbid();

            var courseDto = await _courseService.GetCourseByIdAsync(id);
            if (courseDto == null)
                return NotFound();

            var model = new CourseDetailsViewModel
            {
                Id = courseDto.Id,
                Title = courseDto.Title,
                Description = courseDto.Description,
                StartDate = courseDto.StartDate,
                EndDate = courseDto.EndDate,
                MaterialPath = courseDto.MaterialFileName
            };

            return View(model);
        }

        // Kursu tamamlandı olarak işaretleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteCourse([FromBody] CompleteCourseRequest request)
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Unauthorized();

            var result = await _enrollmentService.MarkCourseCompletedAsync(tcNo, request.CourseId);
            return result ? Ok() : BadRequest();
        }

        // Sadece tamamlanan kurs bilgisini içeren DTO
        public class CompleteCourseRequest
        {
            public int CourseId { get; set; }
        }

        // Tüm kurslar ve personelin kayıtlı olduğu kurslar gösterilir
        public async Task<IActionResult> Courses()
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Forbid();

            var allCourses = await _courseService.GetAllCoursesAsync();
            var enrolledCourses = await _enrollmentService.GetEnrolledCoursesByPersonnelAsync(tcNo);

            var model = new CoursesViewModel
            {
                AllCourses = allCourses,
                EnrolledCourses = enrolledCourses,
                CurrentDate = DateTime.Today
            };

            return View(model);
        }

        // Personelin kayıtlı olduğu kurslar listelenir
        public async Task<IActionResult> RegisteredCourses()
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Forbid();

            var enrolledCourses = await _enrollmentService.GetEnrolledCoursesByPersonnelAsync(tcNo);
            return View(enrolledCourses);
        }

        // Kursa kayıt olmak için onay sayfası
        [HttpGet]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Unauthorized();

            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
                return NotFound();

            var isEnrolled = await _enrollmentService.IsEnrolledAsync(tcNo, courseId);

            var model = new EnrollCourseViewModel
            {
                Course = course,
                IsAlreadyEnrolled = isEnrolled
            };

            return View(model);
        }

        // Kursa kayıt işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollConfirm(int courseId)
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Unauthorized();

            var isEnrolled = await _enrollmentService.IsEnrolledAsync(tcNo, courseId);
            if (isEnrolled)
            {
                TempData["Message"] = "Zaten bu kursa kayıtlısınız.";
                return RedirectToAction("Courses");
            }

            await _enrollmentService.EnrollAsync(tcNo, courseId);
            TempData["Message"] = "Kursa başarıyla kayıt oldunuz.";
            return RedirectToAction("Courses");
        }

        // Personel profilini görüntüleme
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

        // Personel profil güncelleme
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpGet]
        public async Task<IActionResult> UpdateProfileImage()
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Unauthorized();

            var user = await _userService.GetUserByTCNoAsync(tcNo);
            if (user == null)
                return NotFound();

            ViewBag.CurrentImagePath = user.ProfileImagePath ?? "/uploads/profiles/default.png";

            var model = new ProfileImageDto { TCNo = tcNo };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileImage(ProfileImageDto model)
        {
            if (model.ProfileImage == null || model.ProfileImage.Length == 0)
                return Json(new { success = false, message = "Lütfen bir dosya seçiniz." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return Json(new { success = false, message = "Sadece .jpg, .jpeg veya .png uzantılı dosyalar geçerlidir." });

            var user = await _userService.GetUserByTCNoAsync(model.TCNo);
            if (user == null)
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });

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

                return Json(new { success = true, imagePath = user.ProfileImagePath });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Yükleme hatası: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadOriginalMaterial(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
                return NotFound("Kurs bulunamadı.");

            if (string.IsNullOrEmpty(course.PptxFileName))
                return NotFound("PowerPoint dosyası mevcut değil.");

            // Veritabanında tam path değilse relative path olarak varsayalım
            var pptRelativePath = course.PptxFileName.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var pptPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pptRelativePath);

            Console.WriteLine($"[DownloadOriginalMaterial] Dosya yolu: {pptPath}");
            bool fileExists = System.IO.File.Exists(pptPath);
            Console.WriteLine($"[DownloadOriginalMaterial] Dosya var mı? {fileExists}");

            if (!fileExists)
                return NotFound("PowerPoint dosyası bulunamadı.");

            var pptMime = "application/vnd.openxmlformats-officedocument.presentationml.presentation";

            try
            {
                var stream = new FileStream(pptPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(stream, pptMime, Path.GetFileName(pptPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DownloadOriginalMaterial] Dosya indirirken hata: {ex}");
                return StatusCode(500, $"Dosya indirilemedi: {ex.Message}");
            }
        }









    }
}
