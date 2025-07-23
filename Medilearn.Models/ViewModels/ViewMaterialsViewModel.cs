using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Medilearn.Data.Entities;

namespace Medilearn.Models.ViewModels
{
    public class ViewMaterialsViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public List<CourseMaterial> Materials { get; set; }
    }
}
