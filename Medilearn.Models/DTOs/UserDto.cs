using Medilearn.Data.Enums;
using Microsoft.AspNetCore.Http;

namespace Medilearn.Models.DTOs
{
    public class UserDto
    {
        public string? TCNo { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }

        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public string? ProfileImagePath { get; set; }
        public IFormFile? ProfileImageFile { get; set; }


        public DateTime CreatedDate { get; set; }

    }
}
