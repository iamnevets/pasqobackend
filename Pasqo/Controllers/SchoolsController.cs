using Pasqo.Helpers;
using Pasqo.Models;
using Pasqo.Repositories;
using System;
using System.Linq;
using System.Web.Http;

namespace Pasqo.Controllers
{
    [Authorize]
    [RoutePrefix("api/schools")]
    public class SchoolsController : ApiController
    {
        SchoolsRepository schoolRepository = new SchoolsRepository();

        [HttpPost]
        [Route("create")]
        public ReturnObject Create(School school)
        {
            try
            {
                var newSchool = schoolRepository.Create(school);
                if (!newSchool) throw new Exception("Could not create school");

                return WebHelpers.BuildResponse(newSchool, "Successful", true, 1);
            }
            catch(Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getschooldetails")]
        public ReturnObject GetSchoolDetails(long id)
        {
            try
            {
                var school = schoolRepository.GetSchoolDetails(id);

                return WebHelpers.BuildResponse(school, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getschools")]
        public ReturnObject GetSchools()
        {
            try
            {
                var schools = schoolRepository.GetSchools().Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Location,
                    NumberOfUsers = s.ApplicationUsers.Count
                }).ToList();

                return WebHelpers.BuildResponse(schools, "Successful", true, schools.Count);
            }
            catch(Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpPut]
        [Route("update")]
        public ReturnObject Update(School school)
        {
            try
            {
                var schoolToUpdate = schoolRepository.Update(school);
                if (!schoolToUpdate) throw new Exception("Could not update school");

                return WebHelpers.BuildResponse(schoolToUpdate, "Successful", true, 1);
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
                var school = schoolRepository.Delete(id);
                if (!school) throw new Exception("Could not delete school");

                return WebHelpers.BuildResponse(school, "Successful", true, 1);
            }
            catch(Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }
    }
}
