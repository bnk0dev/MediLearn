using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Medilearn.Models.DTOs
{
    public class ProfileImageDto
    {
        public string TCNo { get; set; }

        [Required(ErrorMessage = "Lütfen bir resim dosyası seçiniz.")]
        [DataType(DataType.Upload)]
        public IFormFile ProfileImage { get; set; }
    }
}
