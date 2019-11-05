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
    [RoutePrefix("api/programmes")]
    public class ProgrammesController : ApiController
    {
        private readonly ProgrammesRepository _ProgrammesRepository = new ProgrammesRepository();

        [HttpPost]
        [Route("create")]
        public ReturnObject Create(Programme programme)
        {
            try
            {
                var newProgramme = _ProgrammesRepository.Create(programme);
                if (newProgramme == false) throw new Exception("Could not create programme, Sorry!");

                return WebHelpers.BuildResponse(newProgramme, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getone")]
        public ReturnObject GetOneProgramme(long id)
        {
            try
            {
                var programme = _ProgrammesRepository.GetOneProgramme(id);

                return WebHelpers.BuildResponse(programme, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getall")]
        public ReturnObject GetAllProgrammes()
        {
            try
            {
                var dbContext = new ApplicationDbContext();
                var programmes = _ProgrammesRepository.GetAllProgrammes().Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.SchoolId,
                    x.School,
                    NumberOfCourses = dbContext.Courses.Where(y => y.ProgrammeId == x.Id).ToList().Count
                }
                ).ToList();
                
                return WebHelpers.BuildResponse(programmes, "Successful", true, programmes.Count);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpPut]
        [Route("update")]
        public ReturnObject Update(Programme programme)
        {
            try
            {
                var updateProgramme = _ProgrammesRepository.Update(programme);
                if (updateProgramme == false) throw new Exception("Could not update programme, Sorry!");

                return WebHelpers.BuildResponse(updateProgramme, "Successful", true, 1);
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
                var deleteProgramme = _ProgrammesRepository.Delete(id);
                if (deleteProgramme == false) throw new Exception($"Could not delete programme.\nNo programme with id:{id} found!");

                return WebHelpers.BuildResponse(deleteProgramme, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }
    }
}
