using Medilearn.Data.Entities;
using Medilearn.Data.Enums;
using Medilearn.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medilearn.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> CreateUserAsync(UserCreateDto userCreateDto);
        Task<string> GetPasswordHashByTCNoAsync(string tcNo);

        Task<List<UserDto>> GetUsersByRoleAsync(UserRole role);
        Task<IEnumerable<UserDto>> GetUsersByRoleAndStatusAsync(UserRole role, UserStatus status);
        Task<bool> ApproveUserAsync(string tcNo);

        string HashPassword(string password);
        Task<User> GetByTCNoAsync(string tcNo);
        Task UpdateAsync(User user);

        Task<int> GetTotalUsersAsync();
        Task<int> GetTotalPersonnelAsync();
        Task<UserDto?> GetUserByTcNoAsync(string tcNo);
        Task<List<CourseDto>> GetEnrolledCoursesByPersonnelAsync(string personnelTcNo);
        Task<User> GetUserByTCNoAsync(string tcNo);

        Task UpdateUserAsync(User user);

    }
}
