using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Principal;


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
                var ourCompany = db.OurCompany.FirstOrDefault(x => x.CompanyID == User.OurCompanyID);
                var roleGroup = db.RoleGroup.FirstOrDefault(x => x.ID == User.RoleGroupID);


                if (roleGroup != null && roleGroup.RoleLevel1.LevelNumber >= 3 && User.IsTemp == false && User.IsActive == true && (User.IsDismissal == false || User.IsDismissal == null))
                {

                    var authModel = new AuthenticationModel()
                    {
                        ActionEmployee = new ActionEmployee()
                        {
                            EmployeeID = User.EmployeeID,
                            EMail = User.EMail,
                            FotoFile = User.FotoFile,
                            FullName = User.FullName,
                            RoleGroupID = User.RoleGroupID,
                            Mobile = User.Mobile,
                            RoleGroup = new ActionRoleGroup() { ID = roleGroup.ID, GroupName = roleGroup.GroupName, RoleLevel = roleGroup.RoleLevel.Value },
                            OurCompanyID = User.OurCompanyID,
                            Title = User.Title,
                            OurCompany = ourCompany,
                            Token = User.EmployeeUID.ToString()
                        },
                        Culture = ourCompany.Culture
                    };

                    result.IsSuccess = true;
                    result.Message = "Giriş Başarılı";
                    result.Data = User;

                    OfficeHelper.AddApplicationLog("Office", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, true, $"{User.Username} başarılı bir giriş yaptı.", string.Empty, DateTime.UtcNow, User.FullName, OfficeHelper.GetIPAddress(), string.Empty, authModel);

                    var userData = Newtonsoft.Json.JsonConvert.SerializeObject(authModel);
                    var ticket = new FormsAuthenticationTicket(2, User.Username, DateTime.Now, DateTime.Now.AddMinutes(1440), false, userData, FormsAuthentication.FormsCookiePath);
                    string hash = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);

                    if (ticket.IsPersistent)
                    {
                        cookie.Expires = ticket.Expiration;
                    }
                    Response.Cookies.Add(cookie);
                    ChangeCulture(ourCompany.Culture);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (User.IsActive == false)
                    {
                        result.Message += " Kullanıcı Pasif Durumdadır. ";
                        OfficeHelper.AddApplicationLog("Office", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı pasif durumdadır.", string.Empty, DateTime.UtcNow, User.FullName, OfficeHelper.GetIPAddress(), string.Empty,null);
                    }

                    if (User.IsDismissal == true)
                    {
                        result.Message += " Kullanıcı kilitli durumdadır. Sistem yöneticinize başvurunuz. ";
                        OfficeHelper.AddApplicationLog("Office", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı kilitli durumdadır. Sistem yöneticinize başvurunuz.", string.Empty, DateTime.UtcNow, User.FullName, OfficeHelper.GetIPAddress(), string.Empty,null);
                    }

                    if (roleGroup.RoleLevel1.LevelNumber < 3)
                    {
                        result.Message += " Kullanıcı yetkiniz bulunmamaktadır. Sistem yöneticinize başvurunuz. ";
                        OfficeHelper.AddApplicationLog("Office", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı yetkiniz bulunmamaktadır. Sistem yöneticinize başvurunuz.", string.Empty, DateTime.UtcNow, User.FullName, OfficeHelper.GetIPAddress(), string.Empty,null);
                    }


                }
            }
            else
            {
                result.Message = " Kullanıcı geçersizdir. ";
                OfficeHelper.AddApplicationLog("Office", "Login", "Select", string.Empty, "Login", "Login", null, false, $"{Username} kullanıcısı bulunmamaktadır. Sistem yöneticinize başvurunuz.", string.Empty, DateTime.UtcNow, Username, OfficeHelper.GetIPAddress(), string.Empty,null);
            }

            TempData["result"] = result;
            return RedirectToAction("Index", "Login");


        }

        [AllowAnonymous]
        public string ChangeOurCompany(int id, string url)
        {
            AuthenticationModel Authentication = new AuthenticationModel();
            
            if (HttpContext.User != null && HttpContext.User is GenericPrincipal principal && principal.Identity is FormsIdentity identity)
            {
                Authentication = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticationModel>(identity.Ticket.UserData);

                using (ActionTimeEntities db = new ActionTimeEntities())
                {
                    var ourcompany = db.OurCompany.FirstOrDefault(x => x.CompanyID == id);
                    Authentication.ActionEmployee.OurCompany = ourcompany;
                    Authentication.ActionEmployee.OurCompanyID = id;
                    ChangeCulture(ourcompany.Culture);
                }

                OfficeHelper.AddApplicationLog("Office", "Login", "Select", Authentication.ActionEmployee.EmployeeID.ToString(), "Login", "ChangeOurCompany", null, true, $"{Authentication.ActionEmployee.FullName} başarılı şekilde {id} ülkesine değişim işlemi yaptı.", string.Empty, DateTime.UtcNow, Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty,null);

                var userData = Newtonsoft.Json.JsonConvert.SerializeObject(Authentication);
                var ticket = new FormsAuthenticationTicket(1, identity.Name, DateTime.Now, DateTime.Now.AddMinutes(1440), false, userData, FormsAuthentication.FormsCookiePath);
                string hash = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);

                if (ticket.IsPersistent)
                {
                    cookie.Expires = ticket.Expiration;
                }

                Response.Cookies.Add(cookie);

            }

            return url;

        }

        public void ChangeCulture(string lang)
        {
            Response.Cookies.Remove("Language");

            HttpCookie languageCookie = System.Web.HttpContext.Current.Request.Cookies["Language"];

            if (languageCookie == null) languageCookie = new HttpCookie("Language");

            languageCookie.Value = lang;

            languageCookie.Expires = DateTime.Now.AddDays(10);

            Response.SetCookie(languageCookie);

            //Response.Redirect(Request.UrlReferrer.ToString());
        }
    }
}