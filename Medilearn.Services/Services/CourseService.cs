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
        public async Task<IEnumerable<CourseDto>> GetCoursesByPersonnelAsync(string personnelTcNo)
        {
            // Örnek içerik, kendi mantığına göre düzenle
            var enrollments = await _context.Enrollments
                .Where(e => e.PersonnelTCNo == personnelTcNo)
                .Include(e => e.Course)
                .Select(e => new CourseDto
                {
                    Id = e.Course.Id,
                    Title = e.Course.Title,
                    Description = e.Course.Description,
                    StartDate = e.Course.StartDate,
                    EndDate = e.Course.EndDate,
                    InstructorTCNo = e.Course.InstructorTCNo,
                    MaterialFileName = e.Course.MaterialPath,
                    PptxFileName = e.Course.PptxFileName
                })
                .ToListAsync();

            return enrollments;
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
                    InstructorTCNo = c.InstructorTCNo,
                    MaterialFileName = c.MaterialPath // BURAYA DİKKAT

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
                InstructorTCNo = courseDto.InstructorTCNo!,
                IsActive = DateTime.Now >= courseDto.StartDate && DateTime.Now <= courseDto.EndDate
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return true;
        }


        // Personele ait kayıtlı kursları getirir
        public async Task<List<EnrollmentDto>> GetEnrollmentsByPersonnelAsync(string personnelTcNo)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)  // Include eklendi
                .Where(e => e.PersonnelTCNo == personnelTcNo)
                .ToListAsync();

            return enrollments.Select(e => new EnrollmentDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                PersonnelTCNo = e.PersonnelTCNo,
                EnrollmentDate = e.EnrollmentDate,
                Completed = e.Completed,
                CourseTitle = e.Course.Title
            }).ToList();
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
                .AsNoTracking() // Böylece EF cache kullanmaz, her seferinde DB'den getirir
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
                PptxFileName = course.PptxFileName,    // burada db'deki güncel değer gelmeli
                MaterialFileName = course.MaterialPath  // varsa güncel PDF yolu
            };
        }

        // Var olan kursu günceller
        public async Task<bool> UpdateCourseAsync(CourseDto courseDto)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseDto.Id);
            if (course == null)
                return false;

            course.Title = courseDto.Title;
            course.Description = courseDto.Description;
            course.StartDate = courseDto.StartDate;
            course.EndDate = courseDto.EndDate;
            course.MaterialPath = courseDto.MaterialFileName;
            course.PptxFileName = courseDto.PptxFileName;

            course.IsActive = DateTime.Now >= courseDto.StartDate && DateTime.Now <= courseDto.EndDate;

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
                InstructorTCNo = instructorTcNo,
                IsActive = DateTime.Now >= courseDto.StartDate && DateTime.Now <= courseDto.EndDate
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
                InstructorTCNo = instructorTcNo,
                IsActive = DateTime.Now >= dto.StartDate && DateTime.Now <= dto.EndDate
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
