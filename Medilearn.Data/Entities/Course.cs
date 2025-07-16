using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medilearn.Data.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string InstructorTCNo { get; set; }

        [ForeignKey("InstructorTCNo")]
        public User Instructor { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? MaterialPath { get; set; }
        public List<Enrollment>? Enrollments { get; set; }
        public ICollection<CourseMaterial> CourseMaterials { get; set; } = new List<CourseMaterial>();




    }
}
