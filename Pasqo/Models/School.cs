using System.Collections.Generic;

namespace Pasqo.Models
{
    public class School
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public List<ApplicationUser> ApplicationUsers { get; set; }
    }
}