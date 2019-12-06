using Pasqo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pasqo.Repositories
{
    public class ExamRecordsRepository
    {
        private readonly ApplicationDbContext dbContext = new ApplicationDbContext();

        public bool Create(ExamRecord examRecord)
        {
            if (examRecord == null) return false;

            dbContext.ExamRecords.Add(examRecord);
            dbContext.SaveChanges();

            return true;
        }

        public ExamRecord GetOneExamRecord(long id)
        {
            var examRecord = dbContext.ExamRecords.FirstOrDefault(x => x.Id == id);
            if (examRecord == null) throw new Exception($"No exam record with id: {id} found, Sorry!");

            return examRecord;
        }

        public List<ExamRecord> GetAllExamRecords(string userId)
        {
            var examRecords = dbContext.ExamRecords.Where(x => x.UserId == userId).ToList();
            if (examRecords == null) throw new Exception($"No exam record with UserId: {userId} found");

            return examRecords;
        }

        public bool Delete(long id)
        {
            var examToDelete = dbContext.ExamRecords.FirstOrDefault(x => x.Id == id);
            if (examToDelete == null) return false;

            dbContext.ExamRecords.Remove(examToDelete);
            dbContext.SaveChanges();

            return true;
        }

    }
}