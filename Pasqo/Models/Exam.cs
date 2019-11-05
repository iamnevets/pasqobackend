using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pasqo.Models
{
    public class Exam
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public long SchoolId { get; set; }
        public virtual School School { get; set; }
        public long ProgrammeId { get; set; }
        public virtual Programme Programme { get; set; }
        public long CourseId { get; set; }
        public virtual Course Course { get; set; }
        public DateTime Year { get; set; }
    }
}