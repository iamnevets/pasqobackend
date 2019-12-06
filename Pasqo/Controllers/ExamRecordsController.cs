using Pasqo.Helpers;
using Pasqo.Models;
using Pasqo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Pasqo.Controllers
{
    [Authorize]
    [RoutePrefix("api/examrecords")]
    public class ExamRecordsController : ApiController
    {
        private readonly ExamRecordsRepository _examRecordsRepository = new ExamRecordsRepository();

        [HttpPost]
        [Route("create")]
        public ReturnObject Create(ExamRecord examRecord)
        {
            try
            {
                var dbContext = new ApplicationDbContext();

                var ExamRecordCreated = _examRecordsRepository.Create(examRecord);
                if (!ExamRecordCreated) throw new Exception("Could not create exam record, Sorry!");

                examRecord.SelectedAnswers.ForEach(x => x.ExamRecordId = examRecord.Id);
                examRecord.SelectedAnswers.ForEach(x => x.ExamId = examRecord.ExamId);
                examRecord.SelectedAnswers.ForEach(x => dbContext.SelectedAnswers.Add(x));
                dbContext.SaveChanges();

                var newExamRecord = new ExamRecord
                {
                    Id = examRecord.Id,
                    UserId = examRecord.UserId,
                    User = examRecord.User,
                    ExamId = examRecord.ExamId,
                    Exam = examRecord.Exam,
                    ExamType = examRecord.ExamType,
                    Score = examRecord.Score,
                    TimeTaken = examRecord.TimeTaken,
                    SelectedAnswers = examRecord.SelectedAnswers,
                    NumberOfQuestionsAnswered = examRecord.NumberOfQuestionsAnswered
                };

                return WebHelpers.BuildResponse(newExamRecord, "Exam record created", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getone")]
        public ReturnObject GetOneExamRecord(long id)
        {
            try
            {
                var examRecord = _examRecordsRepository.GetOneExamRecord(id);

                var newExamRecord = new ExamRecord
                {
                    Id = examRecord.Id,
                    //UserId = examRecord.UserId,
                    //User = examRecord.User,
                    ExamId = examRecord.ExamId,
                    Exam = examRecord.Exam,
                    ExamType = examRecord.ExamType,
                    Score = examRecord.Score,
                    TimeTaken = examRecord.TimeTaken,
                    SelectedAnswers = examRecord.SelectedAnswers,
                    NumberOfQuestionsAnswered = examRecord.NumberOfQuestionsAnswered,
                    TotalNumberOfQuestions = examRecord.TotalNumberOfQuestions
                };

                return WebHelpers.BuildResponse(newExamRecord, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getall")]
        public ReturnObject GetAllExamRecords(string userId)
        {
            try
            {
                var dbContext = new ApplicationDbContext();
                var examRecords = _examRecordsRepository.GetAllExamRecords(userId).Select(x => new
                {
                    x.Id,
                    x.ExamId,
                    x.Exam,
                    x.ExamType,
                    //x.UserId,
                    //x.User,
                    SelectedAnswers = dbContext.SelectedAnswers.Where(s => s.ExamRecordId == x.Id && s.ExamId == x.ExamId).ToList(),
                    x.NumberOfQuestionsAnswered,
                    x.TotalNumberOfQuestions,
                    x.Score,
                    x.TimeTaken
                }).ToList();

                return WebHelpers.BuildResponse(examRecords, "Successful", true, examRecords.Count);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpDelete]
        [Route("delete")]
        public ReturnObject Delete(long id)
        {
            try
            {
                var examDeleted = _examRecordsRepository.Delete(id);
                if (!examDeleted) throw new Exception("Could not delete exam, Sorry!");

                return WebHelpers.BuildResponse(examDeleted, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

    }
}
