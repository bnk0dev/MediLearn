using Medilearn.Models.DTOs;

namespace Medilearn.Models.ViewModels
{
    public class PersonnelDashboardViewModel
    {
        public List<CourseDto> CompletedCourses { get; set; }
        public List<CourseDto> EnrolledCourses { get; set; }
    }
}
