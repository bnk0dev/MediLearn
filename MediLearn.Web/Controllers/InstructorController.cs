using Medilearn.Data.Entities;
using Medilearn.Models.DTOs;
using Medilearn.Models.ViewModels;
using Medilearn.Services.Interfaces;
using Medilearn.Services.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Medilearn.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly ICourseMaterialService _courseMaterialService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PowerPointConversionService _pptConverter;

        // Constructor ile gerekli servisler dependency injection yoluyla alınır
        public InstructorController(ICourseService courseService, IUserService userService, ICourseMaterialService courseMaterialService, IWebHostEnvironment webHostEnvironment, PowerPointConversionService pptConverter)
        {
            _courseService = courseService;
            _userService = userService;
            _courseMaterialService = courseMaterialService;
            _webHostEnvironment = webHostEnvironment;
            _pptConverter = pptConverter;
        }

        // Eğitmen ana sayfası, kendi kurslarını listeler
        public async Task<IActionResult> Index()
        {
            var instructorTcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(instructorTcNo))
                return Unauthorized();

            var courses = await _courseService.GetCoursesByInstructorAsync(instructorTcNo);

            var courseDtos = courses.Select(course =>
            {
                bool hasMat = !string.IsNullOrWhiteSpace(course.MaterialFileName?.Trim());
                return new CourseDto
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    StartDate = course.StartDate,
                    EndDate = course.EndDate,
                    MaterialFileName = course.MaterialFileName,

                    HasMaterial = hasMat,
                    MaterialUrl = hasMat ? Url.Action("ViewMaterial", "Instructor", new { courseId = course.Id }) : null
                };
            }).ToList();

            ViewBag.FullName = (await _userService.GetUserByTCNoAsync(instructorTcNo))?.FullName ?? "";

            return View(courseDtos);
        }

        // Yeni kurs oluşturma sayfası
        [HttpGet]
        public IActionResult AddCourse()
        {
            return View();
        }

        // Yeni kurs oluşturma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(CourseDto model)
        {
            var instructorTcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(instructorTcNo))
                return Forbid();

            // Modelde Instructor bilgisi atanır
            model.InstructorTCNo = instructorTcNo;
            model.InstructorId = instructorTcNo;

            // Model doğrulamasından bu alanlar çıkarılır çünkü sunucuda set ediliyor
            ModelState.Remove(nameof(model.InstructorTCNo));
            ModelState.Remove(nameof(model.InstructorId));
            ModelState.Remove(nameof(model.MaterialFileName));

            if (!ModelState.IsValid)
                return View(model);

            // Kurs eklenir ve eklenen kursun Id'si alınır
            var courseId = await _courseService.AddCourseAndReturnIdAsync(model, instructorTcNo);
            if (courseId <= 0)
            {
                ModelState.AddModelError("", "Kurs oluşturulamadı.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // Eğitmenin kendi kurslarını listelediği sayfa
      /*  [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var instructorTcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(instructorTcNo))
                return Forbid();

            var courses = await _courseService.GetCoursesByInstructorAsync(instructorTcNo);
            return View(courses);
        }
      */
        // Eğitmenin profil bilgilerini gösteren sayfa
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

        // Profil resmi güncelleme sayfası
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
            if (!ModelState.IsValid)
                return Json(new { success = false, error = "Form doğrulama hatası." });

            if (model.ProfileImage == null || model.ProfileImage.Length == 0)
                return Json(new { success = false, error = "Lütfen bir dosya seçiniz." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return Json(new { success = false, error = "Sadece .jpg, .jpeg veya .png dosyaları yüklenebilir." });

            var user = await _userService.GetUserByTCNoAsync(model.TCNo);
            if (user == null)
                return Json(new { success = false, error = "Kullanıcı bulunamadı." });

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

                // CLAIM GÜNCELLE
                var identity = (ClaimsIdentity)User.Identity;
                var existingClaim = identity.FindFirst("ProfileImage");
                if (existingClaim != null)
                    identity.RemoveClaim(existingClaim);

                identity.AddClaim(new Claim("ProfileImage", user.ProfileImagePath));
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                return Json(new { success = true, imageUrl = user.ProfileImagePath });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Dosya yüklenirken hata oluştu: " + ex.Message });
            }
        }


        // Kurs materyallerini listeleme
        [HttpGet]
        public async Task<IActionResult> CourseMaterials(int courseId)
        {
            var materials = await _courseMaterialService.GetCourseMaterialsByCourseIdAsync(courseId);
            ViewBag.CourseId = courseId;
            return View(materials);
        }

        // Kursa materyal ekleme sayfası
        [HttpGet]
        public IActionResult AddMaterial(int courseId)
        {
            return View(new AddMaterialViewModel { CourseId = courseId });
        }

        // Kursa materyal ekleme işlemi
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
            if (extension != ".ppt" && extension != ".pptx")
            {
                ModelState.AddModelError("MaterialFile", "Sadece .ppt veya .pptx dosyaları yükleyebilirsiniz.");
                return View(model);
            }

            // 1. PPT dosyasını kaydet
            var pptUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "ppt");
            if (!Directory.Exists(pptUploadsFolder))
                Directory.CreateDirectory(pptUploadsFolder);

            var pptFileName = Guid.NewGuid() + extension;
            var pptFilePath = Path.Combine(pptUploadsFolder, pptFileName);

            using (var stream = new FileStream(pptFilePath, FileMode.Create))
            {
                await model.MaterialFile.CopyToAsync(stream);
            }

            // 2. PDF klasörü hazırla ve PPT -> PDF dönüştür
            var pdfUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "pdf");
            if (!Directory.Exists(pdfUploadsFolder))
                Directory.CreateDirectory(pdfUploadsFolder);

            var pdfFileName = Path.ChangeExtension(pptFileName, ".pdf");
            var pdfFilePath = Path.Combine(pdfUploadsFolder, pdfFileName);

            try
            {
                _pptConverter.ConvertPptToPdf(pptFilePath, pdfFilePath);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "PowerPoint dosyası PDF'ye dönüştürülemedi: " + ex.Message);
                return View(model);
            }

            // 3. Veritabanını güncelle (PptxFileName ve MaterialFileName)
            var courseDto = await _courseService.GetCourseByIdAsync(model.CourseId);
            if (courseDto != null)
            {
                // DİKKAT: Veritabanına göre mutlak ya da göreceli yol kullan
                // Burada göreceli yol örneği verildi, ihtiyacına göre düzenle
                courseDto.PptxFileName = "/uploads/ppt/" + pptFileName;
                courseDto.MaterialFileName = "/uploads/pdf/" + pdfFileName;

                await _courseService.UpdateCourseAsync(courseDto);
            }

            TempData["Success"] = "Materyal başarıyla yüklendi ve PDF'ye dönüştürüldü.";
            return RedirectToAction("Index", new { courseId = model.CourseId });
        }




        public async Task<IActionResult> MyCourses()
        {
            var instructorTcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(instructorTcNo))
                return Forbid();

            var courses = await _courseService.GetCoursesByInstructorAsync(instructorTcNo);

            var courseDtos = courses.Select(course => new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                MaterialFileName = course.MaterialFileName,

                HasMaterial = !string.IsNullOrEmpty(course.MaterialFileName),
                MaterialUrl = Url.Action("ViewMaterial", "Instructor", new { courseId = course.Id })
            }).ToList();

            return View(courseDtos);
        }

        // Materyal görüntüleme sayfası (örnek PDF gösterimi)
        [HttpGet]
        public async Task<IActionResult> ViewMaterial(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
                return NotFound();

            var materials = await _courseMaterialService.GetCourseMaterialsByCourseIdAsync(courseId);

            var model = new ViewMaterialsViewModel
            {
                CourseId = courseId,
                CourseTitle = course.Title,
                Materials = materials.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMaterial(int materialId, int courseId)
        {
            var material = await _courseMaterialService.GetByIdAsync(materialId);
            if (material != null)
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, material.MaterialPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _courseMaterialService.DeleteCourseMaterialAsync(materialId);

            // Kalan materyalleri kontrol et
            var remainingMaterials = await _courseMaterialService.GetCourseMaterialsByCourseIdAsync(courseId);
            var courseDto = await _courseService.GetCourseByIdAsync(courseId);
            if (courseDto != null)
            {
                if (!remainingMaterials.Any())
                {
                    courseDto.MaterialFileName = null;
                }
                else
                {
                    courseDto.MaterialFileName = remainingMaterials.OrderByDescending(m => m.UploadDate).First().MaterialPath;
                }

                await _courseService.UpdateCourseAsync(courseDto);
            }

            return RedirectToAction("ViewMaterial", new { courseId });
        }


        //SSSSSSSSSSSSSSSSSSSSSSSSSSS

        [HttpGet]
        public IActionResult UploadCourseMaterial(int courseId)
        {
            var model = new CourseMaterialUploadDto { CourseId = courseId };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadCourseMaterial(CourseMaterialUploadDto model)
        {
            if (model.PowerPointFile == null || (model.PowerPointFile.FileName.EndsWith(".pptx") == false && model.PowerPointFile.FileName.EndsWith(".ppt") == false))
            {
                ModelState.AddModelError("PowerPointFile", "Sadece .ppt veya .pptx dosyası yükleyebilirsiniz.");
                return View(model);
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/ppt");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.PowerPointFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.PowerPointFile.CopyToAsync(fileStream);
            }

            // PDF olarak kaydetmek için yol belirle
            var pdfFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/pdf");
            if (!Directory.Exists(pdfFolder))
                Directory.CreateDirectory(pdfFolder);

            var pdfFileName = Path.ChangeExtension(fileName, ".pdf");
            var pdfFilePath = Path.Combine(pdfFolder, pdfFileName);

            try
            {
                _pptConverter.ConvertPptToPdf(filePath, pdfFilePath);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "PowerPoint dosyası PDF'ye dönüştürülemedi: " + ex.Message);
                return View(model);
            }

            // TODO: Burada pdfFileName'i ve courseId'yi DB'ye kaydet (Course materyal alanı vb.)

            TempData["Success"] = "Materyal başarıyla yüklendi.";
            return RedirectToAction("CourseMaterials", new { courseId = model.CourseId });
        }

       

    }
}