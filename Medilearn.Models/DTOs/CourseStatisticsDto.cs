using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.DTOs
{
    public class CourseStatisticsDto
    {
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public int TotalPersonnel { get; set; }
        public int CompletedPersonnel { get; set; }

        public double CompletionRate => TotalPersonnel == 0 ? 0 : Math.Round((double)CompletedPersonnel / TotalPersonnel * 100, 2);
    }
}
