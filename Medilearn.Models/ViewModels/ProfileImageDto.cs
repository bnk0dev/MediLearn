using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.ViewModels
{
    public class ProfileImageDto
    {
        [Required]
        public string TCNo { get; set; }

        [Required]
        public IFormFile ProfileImage { get; set; }
    }
}
