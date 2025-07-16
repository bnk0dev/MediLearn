using Medilearn.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Services.Interfaces
{
    public interface ICourseMaterialService
    {
        Task AddCourseMaterialAsync(CourseMaterial material);
        Task<IEnumerable<CourseMaterial>> GetCourseMaterialsByCourseIdAsync(int courseId);
    }
}
