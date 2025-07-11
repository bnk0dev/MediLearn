using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Medilearn.Models.ViewModels
{ 
    public class AddMaterialViewModel
    {
        public int CourseId { get; set; }
        public IFormFile File { get; set; }
    }

}
