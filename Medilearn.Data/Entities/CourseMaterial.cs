using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medilearn.Data.Entities
{
    public class CourseMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string MaterialPath { get; set; } 

        public DateTime UploadDate { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}
