using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pasqo.Models
{
    public class Course
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? ProgrammeId { get; set; }
        public virtual Programme Programme { get; set; }
    }
}