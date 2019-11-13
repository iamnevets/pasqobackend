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
    [RoutePrefix("api/contactus")]
    public class ContactUsController : ApiController
    {
        private readonly ContactUsRepository _contactUsRepository = new ContactUsRepository();

        [HttpPost]
        [Route("create")]
        public ReturnObject Create(ContactUs contact)
        {
            try
            {
                var newContact = _contactUsRepository.Create(contact);
                if (newContact == false) throw new Exception("Could not create contact, Sorry!");

                return WebHelpers.BuildResponse(newContact, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getone")]
        public ReturnObject GetOne(long id)
        {
            try
            {
                var contact = _contactUsRepository.GetOne(id);

                return WebHelpers.BuildResponse(contact, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("getall")]
        public ReturnObject GetAll()
        {
            try
            {
                var contacts = _contactUsRepository.GetAll();

                return WebHelpers.BuildResponse(contacts, "Successful", true, contacts.Count);
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
                var deleteContact = _contactUsRepository.Delete(id);
                if (deleteContact == false) throw new Exception("Could not delete contact, Sorry!");

                return WebHelpers.BuildResponse(deleteContact, "Successful", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }
    }
}
