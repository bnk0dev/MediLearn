using Medilearn.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.ViewModels
{
    public class PersonnelDashboardViewModel
    {
        public List<CourseDto> CompletedCourses { get; set; }
        public List<CourseDto> EnrolledCourses { get; set; }
    }
}
