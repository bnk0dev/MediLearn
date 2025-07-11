using Medilearn.Data.Entities;
using Medilearn.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.ViewModels
{
    public class EnrollCourseViewModel
    {
        public CourseDto Course { get; set; }
        public bool IsAlreadyEnrolled { get; set; }
    }

}
