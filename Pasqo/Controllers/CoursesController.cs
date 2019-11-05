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
    [RoutePrefix("api/courses")]
    public class CoursesController : ApiController
    {
        private readonly CoursesRepository _CoursesRepository = new CoursesRepository();

        [HttpPost]
        [Route("create")]
        public ReturnObject Create(Course course)
        {
            try
            {
                var newCourse = _CoursesRepository.Create(course);
                if (newCourse == false) throw new Exception("Could not create course, sorry!");

                return WebHelpers.BuildResponse(newCourse, "Successful", true, 1);
            }
            catch(Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getone")]
        public ReturnObject GetOneCourse(long id)
        {
            try
            {
                var course = _CoursesRepository.GetOneCourse(id);

                return WebHelpers.BuildResponse(course, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getall")]
        public ReturnObject GetAllCourses()
        {
            try
            {
                var courses = _CoursesRepository.GetAllCourses();

                return WebHelpers.BuildResponse(courses, "Successful", true, courses.Count);
            }
            catch(Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpPut]
        [Route("update")]
        public ReturnObject Update(Course course)
        {
            try
            {
                var courseToUpdate = _CoursesRepository.Update(course);
                if (courseToUpdate == false) throw new Exception("Could not update course, Sorry!");

                return WebHelpers.BuildResponse(courseToUpdate, "Successful", true, 1);
            }
            catch(Exception e)
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
                var courseDeleted = _CoursesRepository.Delete(id);
                if (courseDeleted == false) throw new Exception($"Could not delete course.\nNo course with Id: {id} found");

                return WebHelpers.BuildResponse(courseDeleted, "Successful", true, 1);
            }
            catch(Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }
    }
}
