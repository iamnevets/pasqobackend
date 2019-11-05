using Pasqo.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Pasqo.Repositories
{
    public class CoursesRepository
    {
        private readonly ApplicationDbContext dbContext = new ApplicationDbContext();

        public bool Create(Course course)
        {
            if (course == null) return false;

            dbContext.Courses.Add(course);
            dbContext.SaveChanges();

            return true;
        }

        public Course GetOneCourse(long id)
        {
            var course = dbContext.Courses.FirstOrDefault(x => x.Id == id);
            if (course == null) throw new Exception("Could not find course");

            return course;
        }

        public List<Course> GetAllCourses()
        {
            var courses = dbContext.Courses.ToList();
            if (courses == null) throw new Exception("Could not find any course");

            return courses;
        }

        public bool Update(Course course)
        {
            var courseToUpdate = dbContext.Courses.FirstOrDefault(x => x.Id == course.Id);
            if (courseToUpdate == null) return false;

            courseToUpdate.Name = course.Name;
            courseToUpdate.ProgrammeId = course.ProgrammeId;
            courseToUpdate.Programme = course.Programme;

            dbContext.SaveChanges();

            return true;
        }

        public bool Delete(long id)
        {
            var course = dbContext.Courses.FirstOrDefault(x => x.Id == id);
            if (course == null) return false;

            dbContext.Courses.Remove(course);
            dbContext.SaveChanges();

            return true;
        }
    }
}