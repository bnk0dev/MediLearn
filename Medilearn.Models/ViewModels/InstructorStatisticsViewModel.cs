using Medilearn.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.ViewModels
{
    public class InstructorStatisticsViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public List<CourseStatisticsDto> Courses { get; set; } = new();
    }
}
