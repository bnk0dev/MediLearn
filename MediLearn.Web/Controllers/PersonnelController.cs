using Medilearn.Data.Entities;
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

        var user = await _userService.GetUserByTcNoAsync(tcNo);
        ViewBag.FullName = user != null ? $"{user.FirstName} {user.LastName}" : "Personel";

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var tcNo = User.FindFirst("TCNo")?.Value;
        if (tcNo == null) return Unauthorized();

        await _enrollmentService.EnrollAsync(tcNo, courseId);
        return RedirectToAction("EnrolledCourses");
    }

    public async Task<IActionResult> CourseDetails(int id)
    {
        var tcNo = User.Identity.Name;
        if (string.IsNullOrEmpty(tcNo))
            return Unauthorized();

        var isEnrolled = await _enrollmentService.IsEnrolledAsync(tcNo, id);
        if (!isEnrolled)
            return Forbid();

        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null)
            return NotFound();

        return View(course);
    }
    public async Task<IActionResult> RegisteredCourses()
    {
        var tcNo = User.Identity?.Name;
        if (string.IsNullOrEmpty(tcNo))
            return Forbid();

        var enrolledCourses = await _enrollmentService.GetEnrolledCoursesByPersonnelAsync(tcNo);

        return View("RegisteredCourses", enrolledCourses);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteCourse([FromBody] CompleteCourseRequest request)
    {
        var tcNo = User.Identity.Name;
        if (string.IsNullOrEmpty(tcNo))
            return Unauthorized();

        var result = await _enrollmentService.MarkCourseCompletedAsync(tcNo, request.CourseId);

        if (result)
            return Ok();

        return BadRequest();
    }

    public class CompleteCourseRequest
    {
        public int CourseId { get; set; }
    }

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

}
