namespace MediLearn.Models.DTOs
{
    public class AdminStatisticsViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalPersonnel { get; set; }
        public int TotalBannedUsers { get; set; }

        public int TotalEnrollments { get; set; }
        public int TotalCompletedEnrollments { get; set; }
        public double OverallCompletionRate { get; set; }

        public List<CourseSummaryDto> CoursesSummary { get; set; }
    }
}
