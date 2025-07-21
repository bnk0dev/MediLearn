using Medilearn.Models.DTOs;

namespace Medilearn.Models.ViewModels
{
    public class CoursesViewModel
    {
        public IEnumerable<CourseDto> AllCourses { get; set; }
        public IEnumerable<CourseDto> EnrolledCourses { get; set; }
        public DateTime CurrentDate { get; set; }
    }
}
