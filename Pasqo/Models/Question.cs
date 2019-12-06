using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pasqo.Models
{
    public class Question
    {
        public long Id { get; set; }
        //public long Number { get; set; }
        public string QuestionText { get; set; }
        public long? ExamId { get; set; }
        public virtual Exam Exam { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public string Answer4 { get; set; }
        public string Answer5 { get; set; }
        public string Answer6 { get; set; }
        public string CorrectAnswer { get; set; }

        [NotMapped]
        public string SelectedAnswer { get; set; }
    }
}