using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medilearn.Services.Services
{
    public class CourseMaterialService : ICourseMaterialService
    {
        private readonly MedilearnDbContext _context;

        // Veritabanı bağlamı dependency injection ile alınır
        public CourseMaterialService(MedilearnDbContext context)
        {
            _context = context;
        }

        // Yeni bir kurs materyali veritabanına eklenir
        public async Task AddCourseMaterialAsync(CourseMaterial material)
        {
            _context.CourseMaterials.Add(material);
            await _context.SaveChangesAsync();
        }

        // Belirli bir kursa ait materyaller getirilir
        public async Task<IEnumerable<CourseMaterial>> GetCourseMaterialsByCourseIdAsync(int courseId)
        {
            return await _context.CourseMaterials
                .AsNoTracking()
                .Where(cm => cm.CourseId == courseId)
                .ToListAsync();
        }
    }
}
