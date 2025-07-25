using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medilearn.Services.Services
{
    public class CourseMaterialService : ICourseMaterialService
    {
        private readonly MedilearnDbContext _context;

        public CourseMaterialService(MedilearnDbContext context)
        {
            _context = context;
        }

        // Kurs materyali veritabanına eklenir
        public async Task AddCourseMaterialAsync(CourseMaterial material)
        {
            _context.CourseMaterials.Add(material);
            await _context.SaveChangesAsync();
        }

        // Belirli bir kursa ait materyaller listelenir
        public async Task<IEnumerable<CourseMaterial>> GetCourseMaterialsByCourseIdAsync(int courseId)
        {
            return await _context.CourseMaterials
                .AsNoTracking()
                .Where(cm => cm.CourseId == courseId)
                .ToListAsync();
        }
        public async Task<CourseMaterial> GetByIdAsync(int id)
        {
            return await _context.CourseMaterials.FindAsync(id);
        }


        // Kurs materyali silinir (sadece DB'den)
        public async Task DeleteCourseMaterialAsync(int materialId)
        {
            var material = await _context.CourseMaterials.FindAsync(materialId);
            if (material != null)
            {
                _context.CourseMaterials.Remove(material);
                await _context.SaveChangesAsync();
            }
        }
    }
}
