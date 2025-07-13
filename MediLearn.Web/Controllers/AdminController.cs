using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Data.Enums;
using Medilearn.Models.DTOs;
using Medilearn.Services.Interfaces;
using Medilearn.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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


        public AdminController(MedilearnDbContext context,IUserService userService, IEnrollmentService enrollmentService, ICourseService courseService, IInstructorService instructorService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userService = userService;
            _enrollmentService = enrollmentService;
            _courseService = courseService;
            _instructorService = instructorService;
        }
       

        public async Task<IActionResult> Courses()
        {
            var instructors = await _instructorService.GetAllWithCoursesAsync();
            return View(instructors);
        }

        public async Task<IActionResult> Index()
        {
            var allCourses = await _courseService.GetAllCoursesAsync();
            var allUsers = await _userService.GetAllUsersAsync(); // UserDto listesi
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
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Where(u => u.Role == UserRole.Instructor || u.Role == UserRole.Personnel)
                .ToListAsync();

            return View(users);
        }

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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User updatedUser)
        {
            ModelState.Remove("PasswordHash"); // Şifre boş geliyorsa validasyonu kaldır

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



        public async Task<IActionResult> InstructorsWithCourses()
        {
            var instructors = await _instructorService.GetAllWithCoursesAsync();

            return View(instructors); 
        }

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
    }
}
