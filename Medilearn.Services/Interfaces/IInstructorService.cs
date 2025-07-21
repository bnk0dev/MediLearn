using Medilearn.Models.DTOs;

namespace Medilearn.Services.Interfaces
{
    public interface IInstructorService
    {
        // Tüm eğitmenleri ve onlara ait kursları birlikte getirir
        Task<List<InstructorWithCoursesDto>> GetAllWithCoursesAsync();
    }
}
