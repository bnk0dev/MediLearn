using Medilearn.Models.DTOs;

namespace Medilearn.Models.ViewModels
{
    public class EnrollCourseViewModel
    {
        public CourseDto Course { get; set; }
        public bool IsAlreadyEnrolled { get; set; }
    }

}
