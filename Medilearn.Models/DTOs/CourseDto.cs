using System.ComponentModel.DataAnnotations;
namespace Medilearn.Models.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurs başlığı zorunludur.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        public DateTime EndDate { get; set; }

        public string InstructorTCNo { get; set; } = string.Empty;  
        public string InstructorId { get; set; }   
        public string MaterialFileName { get; set; }  
    }
}