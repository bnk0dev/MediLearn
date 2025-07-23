using Medilearn.Data.Entities;

namespace Medilearn.Services.Interfaces
{
    public interface ICourseMaterialService
    {
        Task DeleteCourseMaterialAsync(int materialId);
        Task<CourseMaterial> GetByIdAsync(int id);

        // Yeni bir kurs materyali ekler
        Task AddCourseMaterialAsync(CourseMaterial material);

        // Belirli bir kursa ait tüm materyalleri getirir
        Task<IEnumerable<CourseMaterial>> GetCourseMaterialsByCourseIdAsync(int courseId);
    }
}
