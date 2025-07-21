namespace Medilearn.Models.DTOs
{
    public class AdminDashboardViewModel
    {
        public List<UserDto> PendingInstructors { get; set; }
        public List<UserDto> NewInstructors { get; set; }
        public List<UserDto> NewPersonnel { get; set; }
        public int TotalCourses { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalPersonnel { get; set; }

        public int TotalUsers => TotalInstructors + TotalPersonnel;
    }
}
