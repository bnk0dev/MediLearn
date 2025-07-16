using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public DateTime UploadDate { get; set; } = DateTime.Now;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }
    }
}
