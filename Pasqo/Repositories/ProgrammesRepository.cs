using Pasqo.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Pasqo.Repositories
{
    public class ProgrammesRepository
    {
        private readonly ApplicationDbContext dbContext = new ApplicationDbContext();

        public bool Create (Programme programme)
        {
            var currentUser = HttpContext.Current.User.Identity.Name;
            if (programme == null) return false;

            //if (currentUser != "Administrator")
            //{
            //    var user = dbContext.Users.FirstOrDefault(x => x.Name == currentUser);
            //    var school = user.School;

            //    var newProgramme = new Programme
            //    {
            //        Name = programme.Name,
            //        SchoolId = school.Id,
            //        School = school
            //    };
            //    dbContext.Programmes.Add(newProgramme);
            //    dbContext.SaveChanges();
            //    return true;
            //}

            //var myProgramme = new Programme
            //{
            //    Name = programme.Name,
            //    SchoolId = programme.SchoolId
            //};

            dbContext.Programmes.Add(programme);
            dbContext.SaveChanges();
            return true;
        }

        public Programme GetOneProgramme(long id)
        {
            var programme = dbContext.Programmes.FirstOrDefault(x => x.Id == id);
            if (programme == null) throw new Exception("Could not find programme!");

            return programme;
        }

        public List<Programme> GetAllProgrammes()
        {
            var programmes = dbContext.Programmes.Include(x => x.School).ToList();
            if (programmes == null) throw new Exception("Could not find any programme");

            return programmes;
        }

        public bool Update(Programme programme)
        {
            var programmeToUpdate = dbContext.Programmes.FirstOrDefault(x => x.Id == programme.Id);
            if (programmeToUpdate == null) return false;

            programmeToUpdate.Name = programme.Name;
            programmeToUpdate.SchoolId = programme.SchoolId;
            programmeToUpdate.School = programme.School;

            dbContext.SaveChanges();

            return true;
        }

        public bool Delete(long id)
        {
            var programme = dbContext.Programmes.FirstOrDefault(x => x.Id == id);
            if (programme == null) return false;

            dbContext.Programmes.Remove(programme);
            dbContext.SaveChanges();

            return true;
        }
    }
}