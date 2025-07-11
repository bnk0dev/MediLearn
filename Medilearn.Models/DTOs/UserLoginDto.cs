using System.ComponentModel.DataAnnotations;

namespace Medilearn.Models.DTOs
{
    public class UserLoginDto
    {
        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır.")]
        public string TCNo { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        public string Password { get; set; }
    }
}
