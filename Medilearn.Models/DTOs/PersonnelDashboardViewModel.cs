using Medilearn.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.DTOs
{
    internal class PersonnelDashboardViewModel
    {
        public List<Course> AttendedCourses { get; set; }
        public List<Course> OngoingCourses { get; set; }
        public List<Course> PastCourses { get; set; }
    }
}
