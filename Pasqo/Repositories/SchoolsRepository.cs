using Pasqo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;

namespace Pasqo.Repositories
{
    public class SchoolsRepository
    {
        private readonly ApplicationDbContext dbContext = new ApplicationDbContext();

        public bool Create(School school)
        {
            if (school == null) return false;

            dbContext.Schools.Add(school);
            dbContext.SaveChanges();
            return true;
        }

        public School GetSchoolDetails(long id)
        {
            var school = dbContext.Schools.FirstOrDefault(s => s.Id == id);
            if (school == null) throw new Exception("Could not find school");

            return school;
        }

        public List<School> GetSchools()
        {
            var schools = dbContext.Schools.Include(x => x.ApplicationUsers).ToList();
            if (schools == null) throw new Exception("Could not find any school");

            return schools;
        }

        public bool Update(School school)
        {
            var schoolToUpdate = dbContext.Schools.FirstOrDefault(s => s.Id == school.Id);
            if (schoolToUpdate == null) return false;

            schoolToUpdate.Name = school.Name;
            schoolToUpdate.Location = school.Location;
            dbContext.SaveChanges();

            return true;
        }

        public bool Delete (long id)
        {
            var school = dbContext.Schools.FirstOrDefault(s => s.Id == id);
            if (school == null) return false;

            dbContext.Schools.Remove(school);
            dbContext.SaveChanges();

            return true;
        }
    }
}