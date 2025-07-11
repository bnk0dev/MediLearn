using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medilearn.Models.ViewModels
{
    public class ProfileDto
    {
        public string TCNo { get; set; } = string.Empty;      // Gösterilecek ama düzenlenmeyecek
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
