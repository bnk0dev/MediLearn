using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.ViewModels
{
    public class AddMaterialViewModel
    {
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Materyal Dosyası")]
        [DataType(DataType.Upload)]
       // [FileExtensions(Extensions = "ppt,pptx", ErrorMessage = "Sadece PowerPoint dosyaları (.ppt, .pptx) yükleyebilirsiniz.")]
        public IFormFile MaterialFile { get; set; }
    }
}
