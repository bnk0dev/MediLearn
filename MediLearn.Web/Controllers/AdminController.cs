using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Data.Enums;
using Medilearn.Models.DTOs;
using Medilearn.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

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
        public AdminController(MedilearnDbContext context, IUserService userService, IEnrollmentService enrollmentService, ICourseService courseService, IInstructorService instructorService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userService = userService;
            _enrollmentService = enrollmentService;
            _courseService = courseService;
            _instructorService = instructorService;
        }

        // Eğitmenler ve onların kurslarını listeler
        public async Task<IActionResult> Courses()
        {
            var instructors = await _instructorService.GetAllWithCoursesAsync();
            return View(instructors);
        }

        // Admin panel ana sayfası için dashboard verilerini hazırlar
        public async Task<IActionResult> Index()
        {
            var allCourses = await _courseService.GetAllCoursesAsync();
            var allUsers = await _userService.GetAllUsersAsync();
            var oneWeekAgo = DateTime.Now.AddDays(-2);

            var newInstructors = allUsers
                .Where(u => u.Role == UserRole.Instructor && u.CreatedDate >= oneWeekAgo)
                .ToList();

            var newPersonnel = allUsers
                .Where(u => u.Role == UserRole.Personnel && u.CreatedDate >= oneWeekAgo)
                .ToList();

            var pendingInstructors = allUsers
                .Where(u => u.Role == UserRole.Instructor && u.Status == UserStatus.Pending && u.CreatedDate >= oneWeekAgo)
                .ToList();

            var model = new AdminDashboardViewModel
            {
                PendingInstructors = pendingInstructors,
                NewInstructors = newInstructors,
                NewPersonnel = newPersonnel,
                TotalCourses = allCourses.Count(),
                TotalInstructors = allUsers.Count(u => u.Role == UserRole.Instructor),
                TotalPersonnel = allUsers.Count(u => u.Role == UserRole.Personnel)
            };

            return View(model);
        }

        // Sistemdeki kullanıcı ve kurslarla ilgili istatistik verilerini hazırlar
        public async Task<IActionResult> Statistics()
        {
            var instructors = await _context.Users
                .Where(u => u.Role == UserRole.Instructor)
                .ToListAsync();

            var personnel = await _context.Users
                .Where(u => u.Role == UserRole.Personnel)
                .ToListAsync();

            var instructorCourseCounts = await _context.Courses
                .GroupBy(c => c.InstructorTCNo)
                .Select(g => new { InstructorTCNo = g.Key, Count = g.Count() })
                .ToListAsync();

            var personnelCourseEnrollments = await _context.Enrollments
                .GroupBy(e => e.PersonnelTCNo)
                .Select(g => new { PersonnelTCNo = g.Key, Count = g.Count() })
                .ToListAsync();

            var enrolledPersonnelTCs = await _context.Enrollments
                .Select(e => e.PersonnelTCNo)
                .Distinct()
                .ToListAsync();

            int registeredCount = personnel.Count(p => enrolledPersonnelTCs.Contains(p.TCNo));
            int unregisteredCount = personnel.Count - registeredCount;

            var model = new AdminStatisticsDto
            {
                TotalInstructors = instructors.Count,
                TotalPersonnel = personnel.Count,
                RegisteredPersonnelCount = registeredCount,
                UnregisteredPersonnelCount = unregisteredCount,
                InstructorCourseCounts = instructorCourseCounts.ToDictionary(x => x.InstructorTCNo, x => x.Count),
                PersonnelCourseEnrollments = personnelCourseEnrollments.ToDictionary(x => x.PersonnelTCNo, x => x.Count)
            };

            return View(model);
        }

        // Eğitmen ve personel kullanıcıların listesini döner
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Where(u => u.Role == UserRole.Instructor || u.Role == UserRole.Personnel)
                .ToListAsync();

            return View(users);
        }

        // Kullanıcı düzenleme sayfasını açar
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

        // Kullanıcı düzenleme işlemini yapar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User updatedUser)
        {
            ModelState.Remove("PasswordHash");

            if (!ModelState.IsValid)
            {
                return View(updatedUser);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TCNo == updatedUser.TCNo);
            if (user == null)
                return NotFound();

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;
            user.Status = updatedUser.Status;

            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        // Kullanıcı silme işlemi
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Users");
        }

        // Eğitmenler ve kursları listesi
        public async Task<IActionResult> InstructorsWithCourses()
        {
            var instructors = await _instructorService.GetAllWithCoursesAsync();

            return View(instructors);
        }

        // Kurs düzenleme sayfasını açar
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            var dto = new CourseEditDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description
            };

            return View(dto);
        }

        // Kurs düzenleme işlemini yapar
        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseEditDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var course = await _context.Courses.FindAsync(dto.Id);
            if (course == null)
                return NotFound();

            course.Title = dto.Title;
            course.Description = dto.Description;

            await _context.SaveChangesAsync();
            return RedirectToAction("InstructorsWithCourses");
        }

        // Eğitmen onaylama işlemi
        [HttpPost]
        public async Task<IActionResult> ApproveInstructor(string tcNo)
        {
            if (string.IsNullOrEmpty(tcNo))
            {
                TempData["Error"] = "Geçersiz TC kimlik numarası.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userService.ApproveUserAsync(tcNo);

            if (result)
                TempData["Success"] = "Eğitmen başarıyla onaylandı.";
            else
                TempData["Error"] = "Onaylama işlemi başarısız oldu.";

            return RedirectToAction(nameof(Index));
        }

        // Belirli bir kursa kayıtlı personel raporu gösterir
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

        // Kullanıcıyı banlar
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

            TempData["Success"] = "Kullanıcı başarıyla yasaklandı.";
            return RedirectToAction("Users");
        }

        // Ban kaldırma işlemi
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

            TempData["Success"] = "Kullanıcının yasağı kaldırıldı.";
            return RedirectToAction("BannedUsers");
        }

        // Banlanan kullanıcıları listeler
        public async Task<IActionResult> BannedUsers()
        {
            var bannedUsers = await _context.Users
                .Where(u => u.Status == UserStatus.Banned)
                .ToListAsync();

            return View(bannedUsers);
        }

        // Şifreyi SHA256 ile hashler
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = sha256.ComputeHash(bytes);
                var builder = new StringBuilder();
                foreach (var b in hashBytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        // Şifre değiştirme sayfasını gösterir
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

        // Şifre değiştirme işlemini yapar
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

            TempData["Success"] = "Şifre başarıyla değiştirildi.";
            return RedirectToAction("Users");
        }
    }
}