namespace Medilearn.Models.DTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public string? PersonnelTCNo { get; set; }
        public int CourseId { get; set; }
        public System.DateTime EnrollmentDate { get; set; }
        public bool Completed { get; set; }
        public string CourseTitle { get; set; }

    }
}
