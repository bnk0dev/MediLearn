namespace Medilearn.Models.DTOs
{
    public class MyCourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string InstructorTCNo { get; set; }  
        public string MaterialFileName { get; set; }
    }
}
