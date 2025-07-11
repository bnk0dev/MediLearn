using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.DTOs
{
    public class CourseEditDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurs başlığı zorunludur")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur")]
        public string Description { get; set; }
    }

}
