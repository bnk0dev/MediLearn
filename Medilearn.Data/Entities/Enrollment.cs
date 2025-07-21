using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medilearn.Data.Entities
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PersonnelTCNo { get; set; }

        [ForeignKey("PersonnelTCNo")]
        public User Personnel { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public DateTime EnrollmentDate { get; set; }  

        public bool Completed { get; set; }
        public bool IsCompleted { get; set; } = false; 
        public DateTime? CompletedDate { get; set; }

    }
}
