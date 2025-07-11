using Medilearn.Data.Enums;

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

    }
}
