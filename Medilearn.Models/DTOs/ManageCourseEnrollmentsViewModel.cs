namespace Medilearn.Models.DTOs
{
    public class ManageCourseEnrollmentsViewModel
    {
        public CourseDto? Course { get; set; }
        public IEnumerable<EnrollmentDto>? Enrollments { get; set; }
        public IEnumerable<UserDto>? PersonnelList { get; set; }
    }
}
