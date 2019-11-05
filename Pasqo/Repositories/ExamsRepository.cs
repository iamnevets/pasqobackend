using Pasqo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pasqo.Repositories
{
    public class ExamsRepository
    {
        private readonly ApplicationDbContext dbContext = new ApplicationDbContext();

        public bool Create(Exam exam)
        {
            if (exam == null) return false;

            dbContext.Exams.Add(exam);
            dbContext.SaveChanges();

            return true;
        }

        public Exam GetOneExam (long id)
        {
            var exam = dbContext.Exams.FirstOrDefault(x => x.Id == id);
            if (exam == null) throw new Exception($"Could not find any Exam with id:{id}, sorry!");

            return exam;
        }

        public List<Exam> GetAllExams ()
        {
            var exams = dbContext.Exams.ToList();
            if (exams == null) throw new Exception("Could not find any Exam, sorry!");

            return exams;
        }

        public bool Update (Exam exam)
        {
            var examToUpdate = dbContext.Exams.FirstOrDefault(x => x.Id == exam.Id);
            if (examToUpdate == null) return false;

            examToUpdate.Title = exam.Title;
            examToUpdate.SchoolId = exam.SchoolId;
            examToUpdate.School = exam.School;
            examToUpdate.ProgrammeId = exam.ProgrammeId;
            examToUpdate.Programme = exam.Programme;
            examToUpdate.Year = exam.Year;

            dbContext.SaveChanges();

            return true;
        }

        public bool Delete (long id)
        {
            var examToDelete = dbContext.Exams.FirstOrDefault(x => x.Id == id);
            if (examToDelete == null) return false;

            dbContext.Exams.Remove(examToDelete);
            dbContext.SaveChanges();

            return true;
        }
    }
}