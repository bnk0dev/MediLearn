using Medilearn.Data.Entities;
using Medilearn.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Medilearn.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(string instructorTcNo);
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<bool> CreateCourseAsync(CourseDto courseDto);
        Task<bool> UpdateCourseAsync(CourseDto courseDto);
        Task<bool> AddCourseAsync(CourseDto courseDto, string instructorTcNo);
        Task<CourseDto> GetCourseByIdAsync(int courseId);
        Task<CourseDto> GetLatestCourseByInstructor(string instructorTcNo);
        Task<int> AddCourseAndReturnIdAsync(CourseDto dto, string instructorTcNo);
        Task<bool> DeleteCourseAsync(int courseId);
        Task<List<CourseDto>> GetCoursesByPersonnelAsync(string personnelTcNo);
        Task<int> GetTotalCoursesAsync();
        Task<IEnumerable<CourseDto>> GetRecentCoursesAsync();
    }
}
