using Medilearn.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Services.Interfaces
{
    public interface IInstructorService
    {
        Task<List<InstructorWithCoursesDto>> GetAllWithCoursesAsync();
    }

}
