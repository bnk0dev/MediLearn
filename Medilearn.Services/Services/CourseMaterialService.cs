using Medilearn.Data.Contexts;
using Medilearn.Data.Entities;
using Medilearn.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task AddCourseMaterialAsync(CourseMaterial material)
        {
            _context.CourseMaterials.Add(material);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CourseMaterial>> GetCourseMaterialsByCourseIdAsync(int courseId)
        {
            return await _context.CourseMaterials
                .AsNoTracking()
                .Where(cm => cm.CourseId == courseId)
                .ToListAsync();
        }
    
}
}
