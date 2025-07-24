using ClosedXML.Excel;
using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Data.Enums;
using Medilearn.Models.DTOs;
using Medilearn.Models.ViewModels;
using Medilearn.Services.Interfaces;
using MediLearn.Models.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ICourseService _courseService;
        private readonly IInstructorService _instructorService;
        private readonly MedilearnDbContext _context;

        public AdminController(
            MedilearnDbContext context,
            IUserService userService,
            IEnrollmentService enrollmentService,
            ICourseService courseService,
            IInstructorService instructorService)
        {
            _context = context;
            _userService = userService;
            _enrollmentService = enrollmentService;
            _courseService = courseService;
            _instructorService = instructorService;
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

            TempData["Success"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Profile));
        }
        // Admin ana sayfa dashboard
        public async Task<IActionResult> Index()
        {
            var allCourses = await _courseService.GetAllCoursesAsync();
            var allUsers = await _userService.GetAllUsersAsync();

            var oneWeekAgo = DateTime.Now.AddDays(-7);

            var newInstructors = allUsers
                .Where(u => u.Role == UserRole.Instructor && u.CreatedDate >= oneWeekAgo && u.Status == UserStatus.Active)
                .ToList();

            var pendingInstructors = allUsers
                .Where(u => u.Role == UserRole.Instructor && u.Status == UserStatus.Pending && u.CreatedDate >= oneWeekAgo)
                .ToList();

            var newPersonnel = allUsers
                .Where(u => u.Role == UserRole.Personnel && u.CreatedDate >= oneWeekAgo)
                .ToList();

            var rejectedInstructors = allUsers
                .Where(u => u.Role == UserRole.Instructor && u.Status == UserStatus.Banned)
                .ToList();

            var model = new AdminDashboardViewModel
            {
                PendingInstructors = pendingInstructors,
                NewInstructors = newInstructors,
                NewPersonnel = newPersonnel,
                RejectedInstructors = rejectedInstructors,
                TotalCourses = allCourses.Count(),
                TotalInstructors = allUsers.Count(u => u.Role == UserRole.Instructor),
                TotalPersonnel = allUsers.Count(u => u.Role == UserRole.Personnel)
            };

            return View(model);
        }

        // Eğitmen onayla
        [HttpPost]
        public async Task<IActionResult> ApproveInstructor(string tcNo)
        {
            if (string.IsNullOrEmpty(tcNo))
            {
                TempData["Error"] = "Geçersiz TC kimlik numarası.";
                return RedirectToAction(nameof(Index));
            }

            bool result = await _userService.ApproveUserAsync(tcNo);

            if (result)
                TempData["onayegitmen"] = "Eğitmen başarıyla onaylandı.";
            else
                TempData["Error"] = "Onaylama işlemi başarısız oldu.";

            return RedirectToAction(nameof(Index));
        }

        // Eğitmeni reddet (banla)
        [HttpPost]
        public async Task<IActionResult> RejectInstructor(string tcNo)
        {
            if (string.IsNullOrEmpty(tcNo))
                return BadRequest();

            await _userService.UpdateUserStatusAsync(tcNo, UserStatus.Banned);

            TempData["egitmensil"] = "Eğitmen banlandı.";
            return RedirectToAction(nameof(Index));
        }

        // Ban kaldır
        [HttpPost]
        public async Task<IActionResult> ReinstateInstructor(string tcNo)
        {
            if (string.IsNullOrEmpty(tcNo))
                return BadRequest();

            await _userService.UpdateUserStatusAsync(tcNo, UserStatus.Active);

            TempData["egitmenaktif"] = "Eğitmen yeniden aktif edildi.";
            return RedirectToAction(nameof(Index));
        }

        // Eğitmen sil
        [HttpPost]
        public async Task<IActionResult> DeleteInstructor(string tcNo)
        {
            if (string.IsNullOrEmpty(tcNo))
                return BadRequest();

            await _userService.DeleteUserAsync(tcNo);

            TempData["basariylasil"] = "Eğitmen başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // İstatistik sayfası
        public async Task<IActionResult> Statistics()
        {
            var users = await _context.Users.ToListAsync();
            var courses = await _context.Courses.ToListAsync();
            var enrollments = await _context.Enrollments.ToListAsync();

            int totalCourses = courses.Count;
            int totalInstructors = users.Count(u => u.Role == UserRole.Instructor);
            int totalPersonnel = users.Count(u => u.Role == UserRole.Personnel);
            int totalBanned = users.Count(u => u.Status == UserStatus.Banned);
            int totalEnrollments = enrollments.Count;
            int totalCompletedEnrollments = enrollments.Count(e => e.IsCompleted);

            double overallCompletionRate = totalEnrollments == 0
                ? 0
                : (double)totalCompletedEnrollments / totalEnrollments * 100;

            var coursesSummary = courses.Select(course =>
            {
                var enrolled = enrollments.Where(e => e.CourseId == course.Id).ToList();
                int enrolledCount = enrolled.Count;
                int completedCount = enrolled.Count(e => e.IsCompleted);
                double completionRate = enrolledCount == 0 ? 0 : (double)completedCount / enrolledCount * 100;

                var instructor = users.FirstOrDefault(u => u.TCNo == course.InstructorTCNo);
                string instructorName = instructor != null ? $"{instructor.FirstName} {instructor.LastName}" : "—";

                return new CourseSummaryDto
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    InstructorName = instructorName,
                    EnrolledPersonnelCount = enrolledCount,
                    CompletedPersonnelCount = completedCount,
                    CompletionRate = Math.Round(completionRate, 2),
                    IsActive = course.IsActive
                };
            }).ToList();



            var model = new AdminStatisticsViewModel
            {
                TotalCourses = totalCourses,
                TotalInstructors = totalInstructors,
                TotalPersonnel = totalPersonnel,
                TotalBannedUsers = totalBanned,
                TotalEnrollments = totalEnrollments,
                TotalCompletedEnrollments = totalCompletedEnrollments,
                OverallCompletionRate = Math.Round(overallCompletionRate, 2),
                CoursesSummary = coursesSummary
            };

            return View(model);
        }


        // Excel dışa aktarım
        [HttpPost]
        public async Task<IActionResult> ExportStatisticsExcel()
        {
            var model = await GetStatisticsModelAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("İstatistikler");

            worksheet.Cell(1, 1).Value = "Kurs Adı";
            worksheet.Cell(1, 2).Value = "Eğitmen";
            worksheet.Cell(1, 3).Value = "Kayıtlı Personel";
            worksheet.Cell(1, 4).Value = "Tamamlayan Personel";
            worksheet.Cell(1, 5).Value = "Tamamlanma Oranı (%)";
            worksheet.Cell(1, 6).Value = "Aktif Mi?";


            int row = 2;

            foreach (var course in model.CoursesSummary)
            {
                worksheet.Cell(row, 1).Value = course.CourseTitle;
                worksheet.Cell(row, 2).Value = course.InstructorName;
                worksheet.Cell(row, 3).Value = course.EnrolledPersonnelCount;
                worksheet.Cell(row, 4).Value = course.CompletedPersonnelCount;
                worksheet.Cell(row, 5).Value = course.CompletionRate;
                worksheet.Cell(row, 6).Value = course.IsActive ? "Evet" : "Hayır";
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "KursIstatistikleri.xlsx");
        }

        private async Task<AdminStatisticsViewModel> GetStatisticsModelAsync()
        {
            var users = await _context.Users.ToListAsync();
            var courses = await _context.Courses.ToListAsync();
            var enrollments = await _context.Enrollments.ToListAsync();

            int totalCourses = courses.Count;
            int totalInstructors = users.Count(u => u.Role == UserRole.Instructor);
            int totalPersonnel = users.Count(u => u.Role == UserRole.Personnel);
            int totalBanned = users.Count(u => u.Status == UserStatus.Banned);
            int totalEnrollments = enrollments.Count;
            int totalCompletedEnrollments = enrollments.Count(e => e.IsCompleted);

            var coursesSummary = courses.Select(course =>
            {
                var enrolled = enrollments.Where(e => e.CourseId == course.Id).ToList();
                int enrolledCount = enrolled.Count;
                int completedCount = enrolled.Count(e => e.IsCompleted);
                double completionRate = enrolledCount == 0 ? 0 : (double)completedCount / enrolledCount * 100;

                var instructor = users.FirstOrDefault(u => u.TCNo == course.InstructorTCNo);
                string instructorName = instructor != null ? $"{instructor.FirstName} {instructor.LastName}" : "—";

                return new CourseSummaryDto
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    InstructorName = instructorName, // ✅ Eksik olan kısım bu
                    EnrolledPersonnelCount = enrolledCount,
                    CompletedPersonnelCount = completedCount,
                    CompletionRate = Math.Round(completionRate, 2),
                    IsActive = course.IsActive
                };
            }).ToList();

            return new AdminStatisticsViewModel
            {
                TotalCourses = totalCourses,
                TotalInstructors = totalInstructors,
                TotalPersonnel = totalPersonnel,
                TotalBannedUsers = totalBanned,
                TotalEnrollments = totalEnrollments,
                TotalCompletedEnrollments = totalCompletedEnrollments,
                CoursesSummary = coursesSummary
            };
        }


        // Kullanıcı listesi (Eğitmen ve Personel)
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Where(u => u.Role == UserRole.Instructor || u.Role == UserRole.Personnel)
                .ToListAsync();

            return View(users);
        }

        // Kullanıcı düzenleme sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TCNo == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // Kullanıcı düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User updatedUser)
        {
            ModelState.Remove("PasswordHash");

            if (!ModelState.IsValid)
                return View(updatedUser);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TCNo == updatedUser.TCNo);
            if (user == null)
                return NotFound();

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;
            user.Status = updatedUser.Status;

            await _context.SaveChangesAsync();

            TempData["kullaniciguncelle"] = "Kullanıcı güncellendi.";
            return RedirectToAction(nameof(Users));
        }

        // Kullanıcı silme (GET)
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["UserDeletedSuccess"] = "Kullanıcı başarıyla silindi.";
            }
            return RedirectToAction(nameof(Users));
        }

        // Eğitmenler ve kurslarını listele
        public async Task<IActionResult> InstructorsWithCourses()
        {
            var instructors = await _instructorService.GetAllWithCoursesAsync();
            return View(instructors);
        }

        // Kurs düzenleme sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseMaterials)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            var dto = new CourseEditDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Materials = course.CourseMaterials.ToList()
            };

            return View(dto);
        }

        // Kurs düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(CourseEditDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == dto.Id);
            if (course == null)
                return NotFound();

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.StartDate = dto.StartDate ?? course.StartDate;
            course.EndDate = dto.EndDate ?? course.EndDate;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Kurs başarıyla güncellendi.";
            return RedirectToAction(nameof(InstructorsWithCourses));
        }

        // Materyal ekleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(int courseId, IFormFile materialFile)
        {
            if (materialFile == null || materialFile.Length == 0)
            {
                TempData["Error"] = "Lütfen geçerli bir dosya seçin.";
                return RedirectToAction("EditCourse", new { id = courseId });
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courseMaterials");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(materialFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await materialFile.CopyToAsync(stream);
            }

            var newMaterial = new CourseMaterial
            {
                CourseId = courseId,
                MaterialPath = "/uploads/courseMaterials/" + uniqueFileName
            };

            _context.CourseMaterials.Add(newMaterial);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Materyal başarıyla yüklendi.";
            return RedirectToAction("EditCourse", new { id = courseId });
        }

        // Materyal silme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterial(int materialId, int courseId)
        {
            var material = await _context.CourseMaterials.FindAsync(materialId);
            if (material == null)
            {
                TempData["Error"] = "Materyal bulunamadı.";
                return RedirectToAction("EditCourse", new { id = courseId });
            }

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", material.MaterialPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.CourseMaterials.Remove(material);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Materyal başarıyla silindi.";
            return RedirectToAction("EditCourse", new { id = courseId });
        }

        // Kursa kayıtlı personel raporu
        public async Task<IActionResult> CourseReport(int courseId)
        {
            ViewData["Layout"] = "_LayoutAdmin";

            var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId);
            var personnelList = await _userService.GetUsersByRoleAsync(UserRole.Personnel);

            var report = enrollments
                .Select(e => personnelList.FirstOrDefault(p => p.TCNo == e.PersonnelTCNo))
                .Where(p => p != null)
                .ToList();

            return View(report);
        }

        // Kullanıcıyı banla
        [HttpPost]
        public async Task<IActionResult> BanUser(string tcNo)
        {
            if (string.IsNullOrEmpty(tcNo))
                return BadRequest();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TCNo == tcNo);
            if (user == null)
                return NotFound();

            user.Status = UserStatus.Banned;
            await _context.SaveChangesAsync();

            TempData["bannedsuccess"] = "Kullanıcı başarıyla yasaklandı.";
            return RedirectToAction(nameof(Users));
        }

        // Ban kaldır
        [HttpPost]
        public async Task<IActionResult> UnbanUser(string tcNo)
        {
            if (string.IsNullOrEmpty(tcNo))
                return BadRequest();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TCNo == tcNo);
            if (user == null)
                return NotFound();

            user.Status = UserStatus.Active;
            await _context.SaveChangesAsync();

            TempData["unbannedsucces"] = "Kullanıcının yasağı kaldırıldı.";
            return RedirectToAction(nameof(BannedUsers));
        }

        // Banlanan kullanıcıları listele
        public async Task<IActionResult> BannedUsers()
        {
            var bannedUsers = await _context.Users
                .Where(u => u.Status == UserStatus.Banned)
                .ToListAsync();

            return View(bannedUsers);
        }

        // Şifreyi SHA256 ile hashle
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);
            var builder = new StringBuilder();
            foreach (var b in hashBytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }

        // Şifre değiştirme sayfası GET
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var tcNo = User.Identity.Name;
            if (string.IsNullOrEmpty(tcNo))
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetUserByTCNoAsync(tcNo);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // Şifre değiştirme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string TCNo, string NewPassword, string ConfirmPassword)
        {
            if (string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ModelState.AddModelError("", "Lütfen tüm alanları doldurun.");
                var user = await _userService.GetUserByTCNoAsync(TCNo);
                return View("EditUser", user);
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Yeni şifre ve onaylama şifresi eşleşmiyor.");
                var user = await _userService.GetUserByTCNoAsync(TCNo);
                return View("EditUser", user);
            }

            var userToUpdate = await _userService.GetUserByTCNoAsync(TCNo);
            if (userToUpdate == null)
                return NotFound();

            userToUpdate.PasswordHash = HashPassword(NewPassword);
            await _userService.UpdateUserAsync(userToUpdate);

            TempData["sifredegisti"] = "Şifre başarıyla değiştirildi.";
            return RedirectToAction(nameof(Users));
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
                return Json(new { success = false, message = "Sadece .jpg, .jpeg veya .png uzantılı dosyalar yüklenebilir." });

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

                // Claims güncelleme ve cookie yenileme
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var existingClaim = identity.FindFirst("ProfileImage");
                    if (existingClaim != null)
                        identity.RemoveClaim(existingClaim);

                    identity.AddClaim(new Claim("ProfileImage", user.ProfileImagePath));

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity));
                }

                return Json(new { success = true, imageUrl = user.ProfileImagePath });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Dosya yüklenirken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseMaterials)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                TempData["Error"] = "Kurs bulunamadı.";
                return RedirectToAction(nameof(InstructorsWithCourses));
            }

            // Kursa ait materyallerin fiziksel dosyalarını sil
            foreach (var material in course.CourseMaterials)
            {
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    material.MaterialPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            // Veritabanından materyal kayıtlarını kaldır
            _context.CourseMaterials.RemoveRange(course.CourseMaterials);

            // Kursa ait personel kayıtlarını kaldır
            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == id)
                .ToListAsync();

            _context.Enrollments.RemoveRange(enrollments);

            // Kursu kaldır
            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            TempData["CourseDeletedSuccess"] = "Kurs başarıyla silindi.";
            return RedirectToAction(nameof(InstructorsWithCourses));
        }


        //ŞİFRE DEĞİŞTİR
        // Şifre değiştirme sayfasını gösterir
        [HttpGet]
        public IActionResult AdminChangePassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var tcNo = User.Identity?.Name;
            if (string.IsNullOrEmpty(tcNo))
                return Unauthorized();

            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                TempData["Error"] = "Lütfen tüm alanları doldurunuz.";
                return RedirectToAction("Profile");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "Yeni şifre ve tekrar şifre eşleşmiyor.";
                return RedirectToAction("Profile");
            }

            var user = await _userService.GetUserByTCNoAsync(tcNo);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Profile");
            }

            var hashedCurrent = _userService.HashPassword(CurrentPassword);
            var storedHash = await _userService.GetPasswordHashByTCNoAsync(tcNo);
            if (hashedCurrent != storedHash)
            {
                TempData["Error"] = "Mevcut şifre yanlış.";
                return RedirectToAction("Profile");
            }

            var newHashedPassword = _userService.HashPassword(NewPassword);
            user.PasswordHash = newHashedPassword;

            await _userService.UpdateUserAsync(user);

            TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
            return RedirectToAction("Profile");
        }

    }
}
