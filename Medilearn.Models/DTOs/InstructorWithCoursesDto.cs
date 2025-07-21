namespace Medilearn.Models.DTOs
{
    public class InstructorWithCoursesDto
    {
        public string InstructorId { get; set; }
        public string FullName { get; set; }
        public List<CourseDto> Courses { get; set; }
    }

}
