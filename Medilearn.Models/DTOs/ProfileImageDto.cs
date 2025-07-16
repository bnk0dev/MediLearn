using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.DTOs
{
    public class ProfileImageDto
    {
        public string TCNo { get; set; }

        [Required(ErrorMessage = "Lütfen bir resim dosyası seçiniz.")]
        [DataType(DataType.Upload)]
       // [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Sadece .jpg, .jpeg veya .png dosyaları yüklenebilir.")]
        public IFormFile ProfileImage { get; set; }
    }
}
