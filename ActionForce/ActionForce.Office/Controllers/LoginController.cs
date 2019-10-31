using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ActionForce.Office.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(string Username, string Password)
        {
            ActionTimeEntities db = new ActionTimeEntities();

            string passMD5 = OfficeHelper.makeMD5(Password).ToUpper();
            var date = DateTime.Now.Date;
            

            string message = string.Empty;

            Result<Employee> result = new Result<Employee>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            var User = db.Employee.FirstOrDefault(x => x.Username == Username && x.Password.ToUpper() == passMD5);

            if (User != null)
            {

                if (User.RoleGroup.RoleLevel1.LevelNumber >= 3 && User.IsTemp == false && User.IsActive == true && User.IsDismissal == false)
                {

                    var authModel = new AuthenticationModel()
                    {
                        Employee = User
                    };

                    result.IsSuccess = true;
                    result.Message = "Giriş Başarılı";
                    result.Data = User;

                    OfficeHelper.AddApplicationLog("Office", "Login", "Select", User.EmployeeID.ToString(), "Login","Login", null, true, $"{User.Username} başarılı bir giriş yaptı.", string.Empty, DateTime.UtcNow,User.FullName,OfficeHelper.GetIPAddress(),string.Empty);

                    var userData = Newtonsoft.Json.JsonConvert.SerializeObject(authModel);
                    var ticket = new FormsAuthenticationTicket(1, User.Username, DateTime.Now, DateTime.Now.AddMinutes(1440), false, userData, FormsAuthentication.FormsCookiePath);
                    string hash = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);

                    if (ticket.IsPersistent)
                    {
                        cookie.Expires = ticket.Expiration;
                    }
                    Response.Cookies.Add(cookie);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (User.IsActive == false)
                    {
                        result.Message += " Kullanıcı Pasif Durumdadır. ";
                        OfficeHelper.AddApplicationLog("Office", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı pasif durumdadır.", string.Empty, DateTime.UtcNow, User.FullName, OfficeHelper.GetIPAddress(), string.Empty);
                    }

                    if (User.IsDismissal == true)
                    {
                        result.Message += " Kullanıcı kilitli durumdadır. Sistem yöneticinize başvurunuz. ";
                        OfficeHelper.AddApplicationLog("Office", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı kilitli durumdadır. Sistem yöneticinize başvurunuz.", string.Empty, DateTime.UtcNow, User.FullName, OfficeHelper.GetIPAddress(), string.Empty);
                    }

                    if (User.RoleGroup.RoleLevel1.LevelNumber < 3)
                    {
                        result.Message += " Kullanıcı yetkiniz bulunmamaktadır. Sistem yöneticinize başvurunuz. ";
                        OfficeHelper.AddApplicationLog("Office", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı yetkiniz bulunmamaktadır. Sistem yöneticinize başvurunuz.", string.Empty, DateTime.UtcNow, User.FullName, OfficeHelper.GetIPAddress(), string.Empty);
                    }


                }
            }
            else
            {
                result.Message = " Kullanıcı geçersizdir. ";
                OfficeHelper.AddApplicationLog("Office", "Login", "Select", string.Empty, "Login", "Login", null, false, $"{Username} kullanıcısı bulunmamaktadır. Sistem yöneticinize başvurunuz.", string.Empty, DateTime.UtcNow, Username, OfficeHelper.GetIPAddress(), string.Empty);
            }

            TempData["result"] = result;
            return RedirectToAction("Index", "Login");


        }
    }
}