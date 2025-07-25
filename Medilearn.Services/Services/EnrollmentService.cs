using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Models.DTOs;
using Medilearn.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medilearn.Services
{
    // Personel ve kurs kayıt işlemlerini yöneten servis sınıfı
    public class EnrollmentService : IEnrollmentService
    {
        private readonly MedilearnDbContext _context;

        public EnrollmentService(MedilearnDbContext context)
        {
            _context = context;
        }

        // Personelin kayıtlı olduğu kursları getirir
        public async Task<IEnumerable<CourseDto>> GetCoursesByPersonnelAsync(string personnelTcNo)
        {
            var courses = await _context.Enrollments
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
                })
                .ToListAsync();

            return courses;
        }

        // Personelin tamamladığı kursları getirir
        public async Task<List<CourseDto>> GetCompletedCoursesByPersonnelAsync(string personnelTcNo)
        {
            var completedEnrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.PersonnelTCNo == personnelTcNo && e.IsCompleted)
                .ToListAsync();

            return completedEnrollments.Select(e => new CourseDto
            {
                Id = e.Course.Id,
                Title = e.Course.Title,
                Description = e.Course.Description,
                StartDate = e.Course.StartDate,
                EndDate = e.Course.EndDate,
                MaterialFileName = e.Course.MaterialPath
            }).ToList();
        }

        // Personelin belirli kursu tamamladığını işaretler
        public async Task<bool> MarkCourseCompletedAsync(string personnelTcNo, int courseId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.PersonnelTCNo == personnelTcNo && e.CourseId == courseId);

            if (enrollment == null)
                return false;

            enrollment.IsCompleted = true;
            enrollment.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // Personeli belirtilen kursa kaydeder, zaten kayıtlı ise false döner
        public async Task<bool> EnrollPersonnelAsync(string personnelTcNo, int courseId)
        {
            var exists = await _context.Enrollments
                .AnyAsync(e => e.PersonnelTCNo == personnelTcNo && e.CourseId == courseId);

            if (exists)
                return false;

            var enrollment = new Enrollment
            {
                PersonnelTCNo = personnelTcNo,
                CourseId = courseId
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        // Personelin kayıtlı olduğu kursları getirir
        public async Task<IEnumerable<CourseDto>> GetCoursesByPersonnelTcNoAsync(string personnelTcNo)
        {
            var courses = await _context.Enrollments
                .Where(e => e.PersonnelTCNo == personnelTcNo)
                .Include(e => e.Course)
                .Select(e => new CourseDto
                {
                    Id = e.Course.Id,
                    Title = e.Course.Title,
                    Description = e.Course.Description,
                    StartDate = e.Course.StartDate,
                    EndDate = e.Course.EndDate
                })
                .ToListAsync();

            return courses;
        }

        // Tüm kayıtları EnrollmentDto olarak getirir
        public async Task<IEnumerable<EnrollmentDto>> GetAllEnrollmentsAsync()
        {
            return await _context.Enrollments
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    PersonnelTCNo = e.PersonnelTCNo,
                    CourseId = e.CourseId
                })
                .ToListAsync();
        }

        // Belirli bir kursa yapılan kayıtları EnrollmentDto olarak getirir
        public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    PersonnelTCNo = e.PersonnelTCNo,
                    CourseId = e.CourseId
                })
                .ToListAsync();
        }

        // Personelin kayıtlı olduğu kursları getirir
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
                    InstructorTCNo = e.Course.InstructorTCNo,
                    MaterialFileName = e.Course.MaterialPath
                })
                .ToListAsync();
        }

        // Personelin belirli kursa kayıtlı olup olmadığını kontrol eder
        public async Task<bool> IsEnrolledAsync(string personnelTcNo, int courseId)
        {
            return await _context.Enrollments.AnyAsync(e => e.PersonnelTCNo == personnelTcNo && e.CourseId == courseId);
        }

        // Personelin kayıtlarını EnrollmentDto olarak getirir
        public async Task<List<EnrollmentDto>> GetEnrollmentsByPersonnelAsync(string personnelTcNo)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.PersonnelTCNo == personnelTcNo)
                .ToListAsync();

            return enrollments.Select(e => new EnrollmentDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                PersonnelTCNo = e.PersonnelTCNo,
                EnrollmentDate = e.EnrollmentDate,
                Completed = e.Completed
            }).ToList();
        }

        // Belirtilen kayıtı tamamlanmış olarak işaretler
        public async Task<bool> MarkEnrollmentCompleteAsync(string personnelTcNo, int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.PersonnelTCNo == personnelTcNo);

            if (enrollment == null)
                return false;

            enrollment.Completed = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // Personeli kursa kaydeder, eğer kayıt yoksa yeni kayıt oluşturur
        public async Task EnrollAsync(string tcNo, int courseId)
        {
            var exists = await _context.Enrollments
                .AnyAsync(e => e.PersonnelTCNo == tcNo && e.CourseId == courseId);

            if (!exists)
            {
                var enrollment = new Enrollment
                {
                    PersonnelTCNo = tcNo,
                    CourseId = courseId,
                    EnrollmentDate = DateTime.Now,
                    IsCompleted = false
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }
        }

        // Personelin kayıtlı olduğu kursları Course entity olarak getirir
        public async Task<List<Course>> GetRegisteredCoursesAsync(string tcNo)
        {
            return await _context.Enrollments
                .Where(e => e.PersonnelTCNo == tcNo)
                .Include(e => e.Course)
                .Select(e => e.Course)
                .ToListAsync();
        }

        // Personelin belirli kursu tamamladığını işaretler (geri dönüşsüz)
        public async Task MarkCourseCompleteAsync(string tcNo, int courseId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.PersonnelTCNo == tcNo && e.CourseId == courseId);

            if (enrollment != null)
            {
                enrollment.IsCompleted = true;
                enrollment.CompletedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // Belirtilen kayıtı tamamlanmış olarak işaretler (parametre sırası farklı overload)
        public async Task<bool> MarkEnrollmentCompleteAsync(int enrollmentId, string personnelTcNo)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.PersonnelTCNo == personnelTcNo);

            if (enrollment == null)
                return false;

            enrollment.Completed = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
