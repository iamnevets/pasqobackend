using Pasqo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pasqo.Repositories
{
    public class ContactUsRepository
    {
        private readonly ApplicationDbContext dbContext = new ApplicationDbContext();

        public bool Create(ContactUs contact)
        {
            if (contact == null) return false;

            dbContext.ContactUs.Add(contact);
            dbContext.SaveChanges();

            return true;
        }

        public ContactUs GetOne(long id)
        {
            var contact = dbContext.ContactUs.FirstOrDefault(x => x.Id == id);
            if (contact == null) throw new Exception($"Could not find contact with id: {id}");

            return contact;
        }

        public List<ContactUs> GetAll()
        {
            var contacts = dbContext.ContactUs.ToList();
            if (contacts == null) throw new Exception("Could not find any contacts");

            return contacts;
        }

        public bool Delete(long id)
        {
            var contactToDelete = dbContext.ContactUs.FirstOrDefault(x => x.Id == id);
            if (contactToDelete == null) return false;

            dbContext.ContactUs.Remove(contactToDelete);

            return true;
        }

    }
}