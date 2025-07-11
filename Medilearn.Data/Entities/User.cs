using Medilearn.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medilearn.Data.Entities
{
    public class User
    {
        
        [Key]
        [Required]
        [StringLength(11, MinimumLength = 11)]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik Numarası 11 haneli ve rakamlardan oluşmalıdır.")]
        public string TCNo { get; set; }

        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Ad rakam içeremez.")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Soyad rakam içeremez.")]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public UserStatus Status { get; set; }

        public virtual ICollection<Course> Courses { get; set; }

    }
}
