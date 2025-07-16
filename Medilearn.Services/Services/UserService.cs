using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Data.Enums;
using Medilearn.Models.DTOs;
using Medilearn.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Services.Services
{
    public class UserService : IUserService
    {
        private readonly MedilearnDbContext _context;

        public UserService(MedilearnDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto?> GetUserByTcNoAsync(string tcNo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.TCNo == tcNo);
            if (user == null) return null;

            return new UserDto
            {
                TCNo = user.TCNo,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                ProfileImagePath = user.ProfileImagePath
            };
        }

        public async Task<User> GetByTCNoAsync(string tcNo)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.TCNo == tcNo);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalUsersAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetTotalPersonnelAsync()
        {
            return await _context.Users.CountAsync(u => u.Role == UserRole.Personnel);
        }

        public async Task<List<CourseDto>> GetEnrolledCoursesByPersonnelAsync(string personnelTcNo)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.PersonnelTCNo == personnelTcNo)
                .Select(e => new CourseDto
                {
                    Id = e.Course.Id,
                    Title = e.Course.Title,
                    Description = e.Course.Description,
                    StartDate = e.Course.StartDate,
                    EndDate = e.Course.EndDate,
                    InstructorTCNo = e.Course.InstructorTCNo
                })
                .ToListAsync();
        }

        public async Task<User> GetUserByTCNoAsync(string tcNo)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.TCNo == tcNo);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users.Select(user => new UserDto
            {
                TCNo = user.TCNo,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                ProfileImagePath = user.ProfileImagePath,
                CreatedDate = user.CreatedDate
            }).ToListAsync();
        }

        public async Task<bool> CreateUserAsync(UserCreateDto userCreateDto)
        {
            var exists = await _context.Users.AnyAsync(u =>
                u.TCNo == userCreateDto.TCNo || u.Email == userCreateDto.Email);
            if (exists) return false;

            var hashedPassword = HashPassword(userCreateDto.Password);

            var role = (UserRole)userCreateDto.Role;
            var status = role == UserRole.Instructor ? UserStatus.Pending : UserStatus.Active;

            var user = new User
            {
                TCNo = userCreateDto.TCNo,
                FirstName = userCreateDto.FirstName,
                LastName = userCreateDto.LastName,
                Email = userCreateDto.Email,
                PasswordHash = hashedPassword,
                Role = role,
                Status = status,
                ProfileImagePath = userCreateDto.ProfileImagePath ?? "/uploads/profiles/default.png", // ✅ doğru klasör
                CreatedDate = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            var builder = new StringBuilder();
            foreach (var b in hash)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }

        public async Task<string> GetPasswordHashByTCNoAsync(string tcNo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.TCNo == tcNo);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                throw new Exception("Kullanıcı bulunamadı veya şifre boş.");

            return user.PasswordHash;
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAndStatusAsync(UserRole role, UserStatus status)
        {
            return await _context.Users
                .Where(u => u.Role == role && u.Status == status)
                .Select(user => new UserDto
                {
                    TCNo = user.TCNo,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    Status = user.Status,
                    ProfileImagePath = user.ProfileImagePath
                }).ToListAsync();
        }

        public async Task<bool> ApproveUserAsync(string tcNo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.TCNo == tcNo);
            if (user == null) return false;

            user.Status = UserStatus.Active;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserDto>> GetUsersByRoleAsync(UserRole role)
        {
            var users = await _context.Users.Where(u => u.Role == role).ToListAsync();

            return users.Select(u => new UserDto
            {
                TCNo = u.TCNo,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
                Status = u.Status,
                ProfileImagePath = u.ProfileImagePath
            }).ToList();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
