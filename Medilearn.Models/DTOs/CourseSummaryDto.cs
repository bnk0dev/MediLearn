namespace MediLearn.Models.DTOs
{
    public class CourseSummaryDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public int EnrolledPersonnelCount { get; set; }
        public int CompletedPersonnelCount { get; set; }
        public double CompletionRate { get; set; }
        public bool IsActive { get; set; }
    }
}
