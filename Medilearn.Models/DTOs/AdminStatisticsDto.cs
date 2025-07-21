namespace Medilearn.Models.DTOs
{
    public class AdminStatisticsDto
    {
        public int RegisteredPersonnelCount { get; set; }
        public int UnregisteredPersonnelCount { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalPersonnel { get; set; }
        public Dictionary<string, int> InstructorCourseCounts { get; set; }
        public Dictionary<string, int> PersonnelCourseEnrollments { get; set; }



    }
}
