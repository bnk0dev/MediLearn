using Medilearn.Data.Entities;
using Medilearn.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medilearn.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<List<CourseDto>> GetCompletedCoursesByPersonnelAsync(string tcNo);
        Task<IEnumerable<CourseDto>> GetCoursesByPersonnelTcNoAsync(string personnelTcNo);
        Task<bool> EnrollPersonnelAsync(string personnelTcNo, int courseId);
        Task<IEnumerable<EnrollmentDto>> GetAllEnrollmentsAsync();
        Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourseAsync(int courseId);
        Task<List<EnrollmentDto>> GetEnrollmentsByPersonnelAsync(string personnelTcNo);
        Task<bool> MarkEnrollmentCompleteAsync(string personnelTcNo, int enrollmentId);
        Task<bool> MarkEnrollmentCompleteAsync(int enrollmentId, string personnelTcNo);
        Task<List<CourseDto>> GetEnrolledCoursesByPersonnelAsync(string personnelTcNo);
        Task<bool> IsEnrolledAsync(string personnelTcNo, int courseId);
        Task<IEnumerable<CourseDto>> GetCoursesByPersonnelAsync(string personnelTcNo);
        Task<bool> MarkCourseCompletedAsync(string personnelTcNo, int courseId);
        Task EnrollAsync(string tcNo, int courseId);
        Task<List<Course>> GetRegisteredCoursesAsync(string tcNo);
        Task<CourseMaterialViewModel> GetCourseMaterialAsync(int courseId);
        Task MarkCourseCompleteAsync(string tcNo, int courseId);

    }
}
