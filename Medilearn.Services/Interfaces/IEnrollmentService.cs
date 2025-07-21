using Medilearn.Data.Entities;
using Medilearn.Models.DTOs;

namespace Medilearn.Services.Interfaces
{
    public interface IEnrollmentService
    {
        // Personelin tamamladığı kursları getirir
        Task<List<CourseDto>> GetCompletedCoursesByPersonnelAsync(string tcNo);

        // Personelin kayıtlı olduğu kursları getirir
        Task<IEnumerable<CourseDto>> GetCoursesByPersonnelTcNoAsync(string personnelTcNo);

        // Personeli belirtilen kursa kaydeder
        Task<bool> EnrollPersonnelAsync(string personnelTcNo, int courseId);

        // Tüm kayıtları getirir
        Task<IEnumerable<EnrollmentDto>> GetAllEnrollmentsAsync();

        // Belirli bir kursa yapılan tüm kayıtları getirir
        Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourseAsync(int courseId);

        // Belirli bir personele ait kayıtları getirir
        Task<List<EnrollmentDto>> GetEnrollmentsByPersonnelAsync(string personnelTcNo);

        // Personelin kaydını tamamlanmış olarak işaretler
        Task<bool> MarkEnrollmentCompleteAsync(string personnelTcNo, int enrollmentId);

        // Personelin kaydını tamamlanmış olarak işaretler
        Task<bool> MarkEnrollmentCompleteAsync(int enrollmentId, string personnelTcNo);

        // Personelin kayıtlı olduğu kursları getirir
        Task<List<CourseDto>> GetEnrolledCoursesByPersonnelAsync(string personnelTcNo);

        // Personelin belirtilen kursa kayıtlı olup olmadığını kontrol eder
        Task<bool> IsEnrolledAsync(string personnelTcNo, int courseId);

        // Personelin kayıtlı olduğu kursları getirir
        Task<IEnumerable<CourseDto>> GetCoursesByPersonnelAsync(string personnelTcNo);

        // Personelin kursu tamamladığını işaretler
        Task<bool> MarkCourseCompletedAsync(string personnelTcNo, int courseId);

        // Personeli belirtilen kursa kaydeder
        Task EnrollAsync(string tcNo, int courseId);

        // Personelin kayıtlı olduğu kursları getirir
        Task<List<Course>> GetRegisteredCoursesAsync(string tcNo);

        // Kursu tamamlanmış olarak işaretler
        Task MarkCourseCompleteAsync(string tcNo, int courseId);
    }
}
