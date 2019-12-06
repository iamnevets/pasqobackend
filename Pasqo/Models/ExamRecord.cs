using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pasqo.Models
{
    public class ExamRecord
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public long? ExamId { get; set; }
        public virtual Exam Exam { get; set; }
        public string ExamType { get; set; }
        public int? Score { get; set; }
        public string TimeTaken { get; set; }
        public  List<UserSelectedAnswer> SelectedAnswers { get; set; }
        public int? NumberOfQuestionsAnswered { get; set; }
        public int TotalNumberOfQuestions { get; set; }
    }

    public class UserSelectedAnswer
    {
        public long Id { get; set; }
        public long? ExamRecordId { get; set; }
        public long? ExamId { get; set; }
        public long QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
    }
}