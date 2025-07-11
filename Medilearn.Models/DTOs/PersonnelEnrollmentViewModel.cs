using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.DTOs
{
    public class PersonnelEnrollmentViewModel
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public bool Completed { get; set; }
    }

}
