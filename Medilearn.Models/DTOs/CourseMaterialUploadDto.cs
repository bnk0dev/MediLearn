using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.DTOs
{
    public class CourseMaterialUploadDto
    {
        public IFormFile PowerPointFile { get; set; }
        public int CourseId { get; set; }
    }
}
