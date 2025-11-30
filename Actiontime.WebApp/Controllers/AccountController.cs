using Actiontime.Services;
using Actiontime.WebApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Actiontime.Data.Context;
using System.Security.Claims;
using Actiontime.Data.Entities;
using NETCore.Encrypt.Extensions;
using Actiontime.DataCloud.Context;
using Actiontime.Models;
using System.Runtime.ConstrainedExecution;

namespace Actiontime.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ApplicationCloudDbContext _cdbContext;
        private readonly IConfiguration _configuration;

        public AccountController(ApplicationDbContext databaseContext, ApplicationCloudDbContext cdbContext, IConfiguration configuration)
        {
            _dbContext = databaseContext;
            _cdbContext = cdbContext;
            _configuration = configuration;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            AuthEmployee auth = new AuthEmployee();

            if (ModelState.IsValid)
            {
                string hashedPassword = model.Password.MD5();

                if (model.Username.ToLower() == "administrator")
                {
                    var cuser = _cdbContext.Employees.SingleOrDefault(x => x.Username.ToLower() == model.Username.ToLower() && x.Password.ToLower() == hashedPassword.ToLower());

                    if (cuser != null)
                    {
                        auth.Id = cuser.EmployeeId;
                        auth.UID = cuser.EmployeeUid.ToString();
                        auth.Username = cuser.Username;
                        auth.Fullname = cuser.FullName;
                    }

                }
                else
                {
                    Employee user = _dbContext.Employees.SingleOrDefault(x => x.Username.ToLower() == model.Username.ToLower() && x.Password.ToLower() == hashedPassword.ToLower());

                    if (user != null)
                    {
                        auth.Id = user.Id;
                        auth.UID = user.EmployeeUid.ToString();
                        auth.Username = user.Username;
                        auth.Fullname = user.FullName;
                    }
                }

                if (auth != null && auth.Id > 0)
                {

                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, auth.Id.ToString()));
                    claims.Add(new Claim(ClaimTypes.Name, auth.Fullname ?? string.Empty));
                    claims.Add(new Claim("Username", auth.Username));
                    claims.Add(new Claim("UID", auth.UID));

                    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Username or password is incorrect.");
                }

            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
