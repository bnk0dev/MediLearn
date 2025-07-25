using Medilearn.Data.Entities;

namespace Medilearn.Services.Interfaces
{
    public interface ICourseMaterialService
    {
        Task DeleteCourseMaterialAsync(int materialId);
        Task<CourseMaterial> GetByIdAsync(int id);

        Task AddCourseMaterialAsync(CourseMaterial material);

        Task<IEnumerable<CourseMaterial>> GetCourseMaterialsByCourseIdAsync(int courseId);
    }
}
