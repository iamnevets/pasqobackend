namespace Pasqo.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Pasqo.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public Configuration() 
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        public Configuration(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            var privileges = new List<IdentityRole>
            {
                new IdentityRole { Name = Privileges.CanViewRoles },
                new IdentityRole { Name = Privileges.CanManageRoles },
                new IdentityRole { Name = Privileges.CanViewUsers },
                new IdentityRole { Name = Privileges.CanManageUsers },
                new IdentityRole { Name = Privileges.CanViewSettings },
                new IdentityRole { Name = Privileges.CanManageSettings },
                new IdentityRole { Name = Privileges.CanViewSchools },
                new IdentityRole { Name = Privileges.CanManageSchools },
                new IdentityRole { Name = Privileges.CanViewProgrammes },
                new IdentityRole { Name = Privileges.CanManageProgrammes },
                new IdentityRole { Name = Privileges.CanViewCourses },
                new IdentityRole { Name = Privileges.CanManageCourses },
                new IdentityRole { Name = Privileges.CanViewExams },
                new IdentityRole { Name = Privileges.CanManageExams },
                new IdentityRole { Name = Privileges.CanViewQuestions },
                new IdentityRole { Name = Privileges.CanManageQuestions }
            };
            privileges.ForEach(p => context.Roles.AddOrUpdate(q => q.Name, p));

            var adminPrivileges = "";
            privileges.ForEach(p => adminPrivileges += p.Name + ",");


            #region Roles/Profiles
            var administrator = new UserRole
            {
                Name = "Administrator",
                Notes = "A user that will be assigned to manage past questions for an institution",
                Privileges = adminPrivileges.Trim(',')
            };
            var student = new UserRole
            {
                Name = "Student",
                Notes = "A user that registers as a student of an institution to get access to past questions from that institution",
                Privileges = $"{Privileges.CanViewProgrammes},{Privileges.CanViewCourses},{Privileges.CanViewExams}"
            };
            context.UserRoles.AddOrUpdate(r => r.Name, administrator, student);
            context.SaveChanges();
            #endregion


            #region School for root Administrator
            var emptySchoolforAdmin = new School
            {
                Name = "",
                Location = ""
            };
            context.Schools.AddOrUpdate(r => r.Id, emptySchoolforAdmin);
            context.SaveChanges();
            #endregion


            #region Users
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context))
            {
                UserValidator = new UserValidator<ApplicationUser>(UserManager)
                {
                    AllowOnlyAlphanumericUserNames = false
                }
            };

            //Create Admin User
            if (UserManager.FindByNameAsync("admin").Result == null)
            {
                var res = userManager.CreateAsync(new ApplicationUser
                {
                    Name = "Administrator",
                    UserRoleId = administrator.Id,
                    SchoolId = emptySchoolforAdmin.Id,
                    UserName = "admin",
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    Locked = true,
                }, "password");

                if (res.Result.Succeeded)
                {
                    var userId = userManager.FindByNameAsync("admin").Result.Id;
                    privileges.ForEach(q => userManager.AddToRole(userId, q.Name));
                }
            }

            #endregion
        }
    }
}
