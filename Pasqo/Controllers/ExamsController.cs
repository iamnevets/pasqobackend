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
    [RoutePrefix("api/exams")]
    public class ExamsController : ApiController
    {
        private readonly ExamsRepository _examsRepository = new ExamsRepository();

        [HttpPost]
        [Route("create")]
        public ReturnObject Create(Exam exam)
        {
            try
            {
                var newExam = _examsRepository.Create(exam);
                if (newExam == false) throw new Exception("Could not create Exam, sorry!");

                return WebHelpers.BuildResponse(newExam, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getone")]
        public ReturnObject GetOneExam(long id)
        {
            try
            {
                var exam = _examsRepository.GetOneExam(id);

                return WebHelpers.BuildResponse(exam, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getall")]
        public ReturnObject GetAllExams()
        {
            try
            {
                var exams = _examsRepository.GetAllExams();

                return WebHelpers.BuildResponse(exams, "Successful", true, exams.Count);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpPut]
        [Route("update")]
        public ReturnObject Update(Exam exam)
        {
            try
            {
                var examToUpdate = _examsRepository.Update(exam);
                if (examToUpdate == false) throw new Exception("Could not update Exam, Sorry!");

                return WebHelpers.BuildResponse(examToUpdate, "Successful", true, 1);
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
                var examToDelete = _examsRepository.Delete(id);
                if (examToDelete == false) throw new Exception($"Could not delete Exam.\nNo Exam with id:{id} found");

                return WebHelpers.BuildResponse(examToDelete, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }
    }
}
