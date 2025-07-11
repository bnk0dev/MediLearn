using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.DTOs
{
    public class CourseMaterialViewModel
    {
        public int CourseId { get; set; }
        public string PdfFile { get; set; } = string.Empty;      
        public string BaseFile { get; set; } = string.Empty;     
        public int TotalPages { get; set; }
    }
}
