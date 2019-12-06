using Pasqo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pasqo.Repositories
{
    public class QuestionsRepository
    {
        private readonly ApplicationDbContext dbContext = new ApplicationDbContext();

        public bool Create(Question question)
        {
            if (question == null) return false;

            dbContext.Questions.Add(question);
            dbContext.SaveChanges();

            return true;
        }

        public Question GetOneQuestion(long id)
        {
            var question = dbContext.Questions.FirstOrDefault(x => x.Id == id);
            if (question == null) throw new Exception($"No question with Id: {id} found, Sorry!");

            return question;
        }

        public List<Question> GetAllQuestions(long examId)
        {
            var questions = dbContext.Questions.Where(x => x.ExamId == examId).ToList();
            if (questions == null) throw new Exception($"Could not find any question with exam Id: {examId}, Sorry!");

            return questions;
        }

        public bool Update(Question question)
        {
            var questionToUpdate = dbContext.Questions.FirstOrDefault(x => x.Id == question.Id);
            if (questionToUpdate == null) return false;

            //questionToUpdate.Number = question.Number;
            questionToUpdate.QuestionText = question.QuestionText;
            questionToUpdate.ExamId = question.ExamId;
            questionToUpdate.Exam = question.Exam;
            questionToUpdate.Answer1 = question.Answer1;
            questionToUpdate.Answer2 = question.Answer2;
            questionToUpdate.Answer3 = question.Answer3;
            questionToUpdate.Answer4 = question.Answer4;
            questionToUpdate.Answer5 = question.Answer5;
            questionToUpdate.Answer6 = question.Answer6;
            questionToUpdate.CorrectAnswer = question.CorrectAnswer;

            dbContext.SaveChanges();

            return true;
        }

        public bool Delete(long id)
        {
            var questionToDelete = dbContext.Questions.FirstOrDefault(x => x.Id == id);
            if (questionToDelete == null) return false;

            dbContext.Questions.Remove(questionToDelete);
            dbContext.SaveChanges();

            return true;
        }
    }
}