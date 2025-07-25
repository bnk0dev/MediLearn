using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.DTOs
{
    public class CourseStatisticsDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public List<PersonnelDto> Personnel { get; set; } = new();
        public int CompletedCount { get; set; }
    }
}
