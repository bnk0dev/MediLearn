using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Medilearn.Models.ViewModels
{
    public class AddMaterialViewModel
    {
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Materyal Dosyası")]
        [DataType(DataType.Upload)]
        public IFormFile MaterialFile { get; set; }
    }
}
