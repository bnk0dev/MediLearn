using Medilearn.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Medilearn.Models.DTOs
{
    public class CourseEditDto
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public List<CourseMaterial> Materials { get; set; } = new List<CourseMaterial>();
    }

}
