using Medilearn.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.ViewModels
{
    public class CoursesViewModel
    {
        public IEnumerable<CourseDto> AllCourses { get; set; }
        public IEnumerable<CourseDto> EnrolledCourses { get; set; }
        public DateTime CurrentDate { get; set; }
    }
}
