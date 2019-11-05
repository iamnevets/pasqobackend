using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pasqo.Models
{
    public class Programme
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? SchoolId { get; set; }
        public virtual School School { get; set; }
        //public List<Course> Courses { get; set; }
    }
}