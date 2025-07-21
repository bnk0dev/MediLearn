using Medilearn.Data.Entities;

namespace Medilearn.Models.DTOs
{
    internal class PersonnelDashboardViewModel
    {
        public List<Course> AttendedCourses { get; set; }
        public List<Course> OngoingCourses { get; set; }
        public List<Course> PastCourses { get; set; }
    }
}
