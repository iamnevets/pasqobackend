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
    [RoutePrefix("api/questions")]
    public class QuestionsController : ApiController
    {
        private readonly QuestionsRepository _questionsRepository = new QuestionsRepository();

        [HttpPost]
        [Route("create")]
        public ReturnObject Create(Question question)
        {
            try
            {
                var newQuestionCreated = _questionsRepository.Create(question);
                if (newQuestionCreated == false) throw new Exception("Could not create question, Sorry!");

                return WebHelpers.BuildResponse(newQuestionCreated, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getone")]
        public ReturnObject GetOneQuestion(long id)
        {
            try
            {
                var question = _questionsRepository.GetOneQuestion(id);

                return WebHelpers.BuildResponse(question, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getall")]
        public ReturnObject GetAllQuestions(long id)
        {
            try
            {
                var questions = _questionsRepository.GetAllQuestions(id);

                return WebHelpers.BuildResponse(questions, "Successful", true, questions.Count);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpPut]
        [Route("update")]
        public ReturnObject Update(Question question)
        {
            try
            {
                var questionUpdated = _questionsRepository.Update(question);
                if (questionUpdated == false) throw new Exception($"No question with id: {question.Id} found.\nQuestion could not be updated, Sorry!");

                return WebHelpers.BuildResponse(questionUpdated, "Successful", true, 1);
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
                var questionDeleted = _questionsRepository.Delete(id);
                if (questionDeleted == false) throw new Exception($"No question with id: {id} found.\nCould not delete question, Sorry!");

                return WebHelpers.BuildResponse(questionDeleted, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }
    }
}
