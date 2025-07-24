using Medilearn.Data.Entities;
using Medilearn.Data.Enums;
using Medilearn.Models.DTOs;
using Medilearn.Models.ViewModels;

namespace Medilearn.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetPendingInstructorsAsync();
        Task<bool> UpdateUserProfileAsync(ProfileDto model);
        Task UpdateUserStatusAsync(string tcNo, UserStatus newStatus);
        Task DeleteUserAsync(string tcNo);

        // Sistemdeki tüm kullanıcıları getirir
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        // Yeni bir kullanıcı oluşturur
        Task<bool> CreateUserAsync(UserCreateDto userCreateDto);

        // TC kimlik numarasına göre kullanıcının şifre hash'ini getirir
        Task<string> GetPasswordHashByTCNoAsync(string tcNo);

        // Belirli role sahip kullanıcıları getirir
        Task<List<UserDto>> GetUsersByRoleAsync(UserRole role);

        // Belirli role ve duruma sahip kullanıcıları getirir
        Task<IEnumerable<UserDto>> GetUsersByRoleAndStatusAsync(UserRole role, UserStatus status);

        // Belirtilen TC kimlik numarasına sahip kullanıcıyı onaylar
        Task<bool> ApproveUserAsync(string tcNo);

        // Şifreyi hashler
        string HashPassword(string password);

        // TC kimlik numarasına göre kullanıcıyı getirir
        Task<User> GetByTCNoAsync(string tcNo);

        // Kullanıcıyı günceller
        Task UpdateAsync(User user);

        // Toplam kullanıcı sayısını döndürür
        Task<int> GetTotalUsersAsync();

        // Toplam personel sayısını döndürür
        Task<int> GetTotalPersonnelAsync();

        // TC kimlik numarasına göre kullanıcıyı getirir
        Task<UserDto?> GetUserByTcNoAsync(string tcNo);

        // Personelin kayıtlı olduğu kursları getirir
        Task<List<CourseDto>> GetEnrolledCoursesByPersonnelAsync(string personnelTcNo);

        // TC kimlik numarasına göre kullanıcıyı getirir
        Task<User> GetUserByTCNoAsync(string tcNo);

        // Kullanıcıyı günceller
        Task UpdateUserAsync(User user);

    }
}
