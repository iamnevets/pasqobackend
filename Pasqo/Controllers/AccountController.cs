#region Using statements
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Pasqo.Helpers;
using Pasqo.Models;
using Pasqo.Providers;
using Pasqo.Results;
#endregion Using Statements

namespace Pasqo.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        #region Constructors

        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        #endregion



        #region UserInfo
        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }
        #endregion UserInfo


        #region Logins

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<ReturnObject> Login(LoginModel model)
        {
            try
            {
                //if (!ModelState.IsValid) throw new Exception("Please check the login details");

                var user = await UserManager.FindAsync(model.Username, model.Password);

                if (user == null) throw new Exception("Invalid Username or Password");

                var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe }, identity);

                var ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
                var token = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);

                var data = new
                {
                    user.Id,
                    user.UserName,
                    user.Name,
                    user.PhoneNumber,
                    user.Email,
                    user.SchoolId,
                    School = new
                    {
                        user.School.Id,
                        user.School.Name,
                        user.School.Location
                    },
                    user.UserRoleId,
                    UserRole = new
                    {
                        user.UserRole.Id,
                        user.UserRole.Name,
                        Privileges = user.UserRole.Privileges.Split(',')
                    },
                    user.CreatedAt,
                    user.ModifiedAt,
                    Token = token
                };

                return WebHelpers.BuildResponse(data, "Login Successful", true, 0);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        #region External Logins
        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                   OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }
        #endregion External Logins

        #endregion Logins


        #region Logout
        // POST api/Account/Logout
        [AllowAnonymous]
        [HttpGet]
        [Route("Logout")]
        public ReturnObject Logout()
        {
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return WebHelpers.BuildResponse(new { }, "User Logged Out", true, 0);
        }
        #endregion Logout


        #region ManageInfo
        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }
        #endregion ManageInfo


        #region Register User

        // POST api/Account/Register
        [HttpPost]
        [Route("CreateUser")]
        public async Task<ReturnObject> CreateUser(ApplicationUser model)
        {
            try
            {
                //var db = new ApplicationDbContext();
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);
                var userRole = db.UserRoles.Where(x => x.Id == model.UserRoleId).First();

                var user = new ApplicationUser
                {
                    Name = model.Name,
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    UserRoleId = model.UserRoleId,
                    SchoolId = model.SchoolId,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                var identityResult = await UserManager.CreateAsync(user, model.Password);
                if (!identityResult.Succeeded) return WebHelpers.ProcessException(identityResult);


                //Add Priviledges in selected Role to user
                if (!string.IsNullOrEmpty(userRole.Privileges))
                {
                    userRole.Privileges.Split(',').ForEach(r => UserManager.AddToRole(user.Id, r.Trim()));
                }
                db.SaveChanges();

                return WebHelpers.BuildResponse(user, "User Created Successfully", true, 1);

            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }

        [HttpGet]
        [Route("GetRoles")]
        public ReturnObject GetRoles()
        {
            try
            {
                var roles = db.UserRoles.ToList().Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Notes,
                    x.Privileges,
                    x.Users
                }
                );

                if (roles == null) throw new Exception("Could not find any role");

                return WebHelpers.BuildResponse(roles, "Successful", true, roles.Count());
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        #endregion Register User

        #region Get User/s
        [HttpGet]
        [Route("GetUsers")]
        public ReturnObject GetUsers()
        {
            try
            {
                var db = new ApplicationDbContext();
                var data = db.Users.Where(x => x.Id != null)
                    .Include(x => x.UserRole)
                    .Include(s => s.School)
                    .ToList()
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.SchoolId,
                        School = new
                        {
                            x.School.Id,
                            x.School.Name,
                            x.School.Location
                        },
                        x.Email,
                        x.PhoneNumber,
                        x.UserName,
                        x.CreatedAt,
                        x.ModifiedAt,
                        x.UserRoleId,
                        UserRole = new
                        {
                            x.UserRole.Id,
                            x.UserRole.Name
                        }
                    }).ToList();
                return WebHelpers.BuildResponse(data, "", true, data.Count);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }
        }

        [HttpGet]
        [Route("GetUserDetails")]
        public ReturnObject GetUserDetails(string id)
        {
            try
            {
                //var db = new ApplicationDbContext();
                var data = db.Users.Where(x => x.Id == id)
                    .Include(x => x.UserRole)
                    .Include(s => s.School)
                    .ToList()
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.SchoolId,
                        School = new
                        {
                            x.School.Id,
                            x.School.Name,
                            x.School.Location
                        },
                        x.Email,
                        x.PhoneNumber,
                        x.UserName,
                        x.CreatedAt,
                        x.UserRoleId,
                        Profile = new
                        {
                            x.UserRole.Id,
                            x.UserRole.Name
                        },
                        x.Password,
                        x.ConfirmPassword
            }).FirstOrDefault();
                return WebHelpers.BuildResponse(data, "Successful", true, 1);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }
        }
        #endregion Get User/s


        #region Update User
        [HttpPut]
        [Route("UpdateUser")]
        public ReturnObject UpdateUser(ApplicationUser model)
        {
            try
            {
                //var db = new ApplicationDbContext();

                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);

                var user = db.Users.Where(x => x.UserName == model.UserName).Include(x => x.UserRole).First();
                var userRole = db.UserRoles.Where(x => x.Id == model.UserRoleId).First();
                var school = db.Schools.Where(x => x.Id == model.SchoolId).First();

                if (user == null) return WebHelpers.BuildResponse(null, "User not found. Please update an existing user", false, 0);

                user.UserRoleId = userRole.Id;
                user.SchoolId = school.Id;
                user.Name = model.Name;
                user.UserName = model.UserName;
                user.ModifiedAt = DateTime.UtcNow;
                user.PhoneNumber = model.PhoneNumber;
                user.Email = model.Email;
                db.SaveChanges();

                var oldPrivileges = user.UserRole.Privileges.Split(',');
                //Remove old roles
                oldPrivileges.ForEach(x => UserManager.RemoveFromRole(user.Id, x));

                //Add Priviledges in selected Role to user
                if (!string.IsNullOrEmpty(userRole.Privileges))
                {
                    userRole.Privileges.Split(',').ForEach(r => UserManager.AddToRole(user.Id, r.Trim()));
                }

                var data = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.PhoneNumber,
                    user.UserName,
                    user.UserRoleId,
                    Role = new { user.UserRole.Id, user.UserRole.Name },
                    user.SchoolId,
                    School = new {user.School.Id, user.School.Name, user.School.Location}
                };

                return WebHelpers.BuildResponse(data, "User Updated Successfully", true, 1);

            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }
        #endregion Update User

        #region Delete User
        [HttpDelete]
        [Route("DeleteUser")]
        public ReturnObject DeleteUser(string id)
        {
            ReturnObject results;
            try
            {
                var userToDelete = db.Users.Find(id);
                if (userToDelete == null) throw new Exception($"No user with id: {id} found");

                db.Users.Remove(userToDelete);
                db.SaveChanges();

                results = WebHelpers.BuildResponse(id, "Succesfully Deleted", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }
        #endregion Delete User

        #region Manage Password
        // POST api/Account/ChangePassword
        [Authorize]
        [Route("ChangePassword")]
        public async Task<ReturnObject> ChangePassword(ChangePasswordBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);

                var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(),
                    model.OldPassword, model.NewPassword);

                return !result.Succeeded ? WebHelpers.ProcessException(result)
                    : WebHelpers.BuildResponse(model, "Password changed sucessfully.", true, 1);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }

        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<ReturnObject> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return WebHelpers.ProcessException(ModelState.Values);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return WebHelpers.ProcessException(result);
            }

            return WebHelpers.BuildResponse(model, "Successfully set password", true, 1);
        }
        #endregion Manage Password



        #region External Registration
        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result); 
            }
            return Ok();
        }
        #endregion External Registration

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                //_userManager.Dispose();
                //_userManager = null;
            }

            base.Dispose(disposing);
        }



        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
