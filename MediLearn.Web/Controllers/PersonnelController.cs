using Medilearn.Data.Entities;
using Medilearn.Models.DTOs;
using Medilearn.Models.ViewModels;
using Medilearn.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Personnel")]
public class PersonnelController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IUserService _userService;

    public PersonnelController(ICourseService courseService, IEnrollmentService enrollmentService, IUserService userService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var tcNo = User.Identity?.Name;
        if (string.IsNullOrEmpty(tcNo))
            return Forbid();

        var user = await _userService.GetUserByTCNoAsync(tcNo);
        ViewBag.FullName = user != null ? $"{user.FirstName} {user.LastName}" : "Personel";

        return View();
    }

    // Kurslara genel bakış
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

    // Kayıtlı olunan kurslar
    public async Task<IActionResult> RegisteredCourses()
    {
        var tcNo = User.Identity?.Name;
        if (string.IsNullOrEmpty(tcNo))
            return Forbid();

        var enrolledCourses = await _enrollmentService.GetEnrolledCoursesByPersonnelAsync(tcNo);
        return View("RegisteredCourses", enrolledCourses);
    }

    // Kurs detayları
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

        return View(courseDto); // View CourseDto bekliyor
    }


    // Kursa kayıt sayfası (GET)
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

        return View("Enroll", model);
    }

    // Kursa kayıt işlemi (POST)
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
        return RedirectToAction("RegisteredCourses");
    }

    // Kursu tamamlama (AJAX POST)
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

    public class CompleteCourseRequest
    {
        public int CourseId { get; set; }
    }

    // Profil görüntüleme
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

    // Profil güncelleme (TCNo değiştirilemez)
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
