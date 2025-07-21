using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Models.DTOs;
using Medilearn.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medilearn.Services.Services
{
    public class CourseService : ICourseService
    {
        private readonly MedilearnDbContext _context;
        private readonly IEnrollmentService _enrollmentService;

        // DbContext ve kayıt servisi dependency injection ile alınır
        public CourseService(MedilearnDbContext context, IEnrollmentService enrollmentService)
        {
            _context = context;
            _enrollmentService = enrollmentService;
        }

        // Belirli bir eğitmene ait kursları getirir
        public async Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(string instructorTcNo)
        {
            return await _context.Courses
                .Where(c => c.InstructorTCNo == instructorTcNo)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    InstructorTCNo = c.InstructorTCNo
                })
                .ToListAsync();
        }

        // Toplam kurs sayısını döndürür
        public async Task<int> GetTotalCoursesAsync()
        {
            return await _context.Courses.CountAsync();
        }

        // Son 1 ayda eklenen kursları getirir
        public async Task<IEnumerable<CourseDto>> GetRecentCoursesAsync()
        {
            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            return await _context.Courses
                .Where(c => c.StartDate >= oneMonthAgo)
                .OrderByDescending(c => c.StartDate)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    InstructorTCNo = c.InstructorTCNo
                })
                .ToListAsync();
        }

        // Yeni kurs oluşturur
        public async Task<bool> CreateCourseAsync(CourseDto courseDto)
        {
            var course = new Course
            {
                Title = courseDto.Title!,
                Description = courseDto.Description!,
                StartDate = courseDto.StartDate,
                EndDate = courseDto.EndDate,
                InstructorTCNo = courseDto.InstructorTCNo!
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return true;
        }

        // Personele ait kayıtlı kursları getirir
        public async Task<List<CourseDto>> GetCoursesByPersonnelAsync(string personnelTcNo)
        {
            var enrollments = await _enrollmentService.GetEnrollmentsByPersonnelAsync(personnelTcNo);
            var courseIds = enrollments.Select(e => e.CourseId).Distinct();

            return await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    InstructorTCNo = c.InstructorTCNo
                })
                .ToListAsync();
        }

        // Sistemdeki tüm kursları getirir
        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    InstructorTCNo = c.InstructorTCNo
                })
                .ToListAsync();
        }

        // Kursu ID'sine göre getirir
        public async Task<CourseDto> GetCourseByIdAsync(int courseId)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return null;

            return new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                InstructorTCNo = course.InstructorTCNo,
                MaterialFileName = course.MaterialPath
            };
        }

        // Var olan kursu günceller
        public async Task<bool> UpdateCourseAsync(CourseDto courseDto)
        {
            var course = await _context.Courses.FindAsync(courseDto.Id);
            if (course == null) return false;

            course.Title = courseDto.Title;
            course.Description = courseDto.Description;
            course.StartDate = courseDto.StartDate;
            course.EndDate = courseDto.EndDate;
            course.MaterialPath = courseDto.MaterialFileName;

            await _context.SaveChangesAsync();
            return true;
        }

        // Eğitmenin TC'siyle birlikte yeni kurs ekler
        public async Task<bool> AddCourseAsync(CourseDto courseDto, string instructorTcNo)
        {
            var course = new Course
            {
                Title = courseDto.Title,
                Description = courseDto.Description,
                StartDate = courseDto.StartDate,
                EndDate = courseDto.EndDate,
                InstructorTCNo = instructorTcNo
            };

            _context.Courses.Add(course);
            return await _context.SaveChangesAsync() > 0;
        }

        // Eğitmenin oluşturduğu son kursu getirir
        public async Task<CourseDto> GetLatestCourseByInstructor(string instructorTcNo)
        {
            var course = await _context.Courses
                .Where(c => c.InstructorTCNo == instructorTcNo)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (course == null) return null;

            return new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate
            };
        }

        // Yeni kurs ekleyip, eklenen kursun ID'sini döndürür
        public async Task<int> AddCourseAndReturnIdAsync(CourseDto dto, string instructorTcNo)
        {
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                InstructorTCNo = instructorTcNo
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return course.Id;
        }

        // Kursu veritabanından siler
        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return false;

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
