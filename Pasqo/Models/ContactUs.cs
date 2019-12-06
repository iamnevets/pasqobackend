using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pasqo.Models
{
    public class ContactUs
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public long SchoolId { get; set; }
        public virtual School School { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}