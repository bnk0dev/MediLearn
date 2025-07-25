using Medilearn.Data.Contexts;
using Medilearn.Data.Enums;
using Medilearn.Models.DTOs;
using Medilearn.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medilearn.Services.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly MedilearnDbContext _context;

        public InstructorService(MedilearnDbContext context)
        {
            _context = context;
        }

        // Tüm eğitmenleri ve her birinin kurslarını getirir
        public async Task<List<InstructorWithCoursesDto>> GetAllWithCoursesAsync()
        {
            // Eğitmen rolündeki tüm kullanıcıları getirir
            var instructors = await _context.Users
                .Where(u => u.Role == UserRole.Instructor)
                .ToListAsync();

            // Tüm kursları getirir
            var courses = await _context.Courses.ToListAsync();

            // Eğitmenlerle ilgili DTO oluşturur, her eğitmenin kurslarını eşler
            return instructors.Select(i => new InstructorWithCoursesDto
            {
                InstructorId = i.TCNo,
                FullName = $"{i.FirstName} {i.LastName}",
                Courses = courses
                    .Where(c => c.InstructorTCNo == i.TCNo)
                    .Select(c => new CourseDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Description = c.Description
                    }).ToList()
            }).ToList();
        }
    }
}
