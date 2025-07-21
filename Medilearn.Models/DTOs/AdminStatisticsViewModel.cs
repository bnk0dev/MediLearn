namespace Medilearn.Models.DTOs
{
    public class AdminStatisticsViewModel
    {
        public IEnumerable<UserDto> PendingInstructors { get; set; } = new List<UserDto>();

        public int TotalCourses { get; set; }

        public int TotalInstructors { get; set; }

        public int TotalPersonnel { get; set; }

        public int PersonnelCount { get; set; }
        public int InstructorCount { get; set; }
        public int CourseCount { get; set; }
        public int EnrollmentCount { get; set; }

    }
}
