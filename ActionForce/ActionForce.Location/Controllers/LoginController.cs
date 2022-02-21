using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ActionForce.Location.Controllers
{
    public class LoginController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            LoginControlModel model = new LoginControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(string Username, string Password)
        {
            ActionTimeEntities db = new ActionTimeEntities();

            string passMD5 = LocationHelper.makeMD5(Password).ToUpper();
            var date = DateTime.Now.Date;

            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            var User = db.Employee.FirstOrDefault(x => x.Username == Username && x.Password.ToUpper() == passMD5 && x.StatusID == 1);

            if (User != null)
            {
                var ourCompany = db.OurCompany.FirstOrDefault(x => x.CompanyID == User.OurCompanyID);
                var roleGroup = db.RoleGroup.FirstOrDefault(x => x.ID == User.RoleGroupID);
                var position = db.EmployeePositions.FirstOrDefault(x => x.ID == User.PositionID);
                Entity.Location location = null;
                var currentlocations = db.EmployeeLocation.Where(x => x.EmployeeID == User.EmployeeID && x.IsActive == true).ToList();
                List<int> locationids = currentlocations.Select(x => x.LocationID).Distinct().ToList();
                var locationlist = db.Location.Where(x => locationids.Contains(x.LocationID)).ToList();


                if (roleGroup != null && roleGroup.RoleLevel1.LevelNumber >= 1 && User.IsTemp == false && User.IsActive == true && (User.IsDismissal == false || User.IsDismissal == null))
                {

                    if (currentlocations != null && currentlocations.Count > 0)
                    {



                        if (roleGroup.RoleLevel1.LevelNumber >= 2)
                        {
                            int? locationid = currentlocations.FirstOrDefault(x => x.IsMaster == true)?.LocationID;
                            if (locationid > 0)
                            {
                                location = locationlist.FirstOrDefault(x => x.LocationID == locationid);
                            }
                            else
                            {
                                locationid = currentlocations.FirstOrDefault(x => x.IsActive == true)?.LocationID;

                                location = locationlist.FirstOrDefault(x => x.LocationID == locationid);
                            }
                        }
                        else
                        {
                            foreach (var loc in locationlist)
                            {
                                var currentdate = loc.LocalDate;

                                if (db.Schedule.Any(x => x.LocationID == loc.LocationID && x.EmployeeID == User.EmployeeID && x.ShiftDate == currentdate))
                                {
                                    location = loc;
                                }
                            }

                            if (location == null)
                            {
                                result.IsSuccess = false;
                                result.Message = "Takvim tanımlanmış lokasyonunuz bulunamadı.";

                                TempData["result"] = result;
                                return RedirectToAction("Index", "Login");
                            }

                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Lokasyon Tanımınız Eksik";

                        TempData["result"] = result;
                        return RedirectToAction("Index", "Login");
                    }

                    if (location != null)
                    {
                        var dayresultid = db.GetDayResultID(location.LocationID, location.LocalDate, 1, 3, User.EmployeeID, "", "").FirstOrDefault();

                        var authModel = new AuthenticationModel()
                        {

                            CurrentEmployee = new LocationEmployee()
                            {
                                EmployeeID = User.EmployeeID,
                                //EMail = User.EMail,
                                FullName = User.FullName,
                                Username = User.Username,
                                FotoFile = User.FotoFile,
                                //Mobile = User.Mobile,
                                Position = position != null ? position.PositionName : User.Title,
                                Token = User.EmployeeUID
                            },

                            CurrentLocation = new LocationInfo()
                            {
                                Currency = location.Currency,
                                FullName = $"{location.LocationName} {location.Description} {location.State}",
                                ID = location.LocationID,
                                //IsActive = location.IsActive.Value,
                                //OurCompanyID = location.OurCompanyID,
                                //Latitude = location.Latitude,
                                //Longitude = location.Longitude,
                                //SortBy = location.SortBy,
                                TimeZone = location.Timezone ?? 3,
                                UID = location.LocationUID
                            },

                            CurrentOurCompany = new LocationOurCompany()
                            {
                                ID = ourCompany.CompanyID,
                                Code = ourCompany.Code,
                                Culture = ourCompany.Culture,
                                Currency = ourCompany.Currency,
                                Name = ourCompany.CompanyName,
                                TimeZone = ourCompany.TimeZone.Value
                            },

                            CurrentRoleGroup = new LocationRoleGroup()
                            {
                                ID = roleGroup.ID,
                                RoleLevel = roleGroup.RoleLevel.Value,
                                GroupName = roleGroup.GroupName
                            },

                            Culture = ourCompany.Culture
                        };

                        result.IsSuccess = true;
                        result.Message = "Giriş Başarılı";

                        LocationHelper.AddApplicationLog("Location", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, true, $"{User.Username} başarılı bir giriş yaptı.", string.Empty, DateTime.UtcNow, User.FullName, LocationHelper.GetIPAddress(), string.Empty, authModel);

                        var userData = Newtonsoft.Json.JsonConvert.SerializeObject(authModel);
                        var ticket = new FormsAuthenticationTicket(2, User.Username, DateTime.Now, DateTime.Now.AddMinutes(1440), true, userData, FormsAuthentication.FormsCookiePath);
                        string hash = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);

                        if (ticket.IsPersistent)
                        {
                            cookie.Expires = ticket.Expiration;
                        }

                        Response.Cookies.Add(cookie);
                        ChangeCulture(ourCompany.Culture);

                        if (authModel.CurrentLocation != null && authModel.CurrentLocation.ID > 0)
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return RedirectToAction("SetCurrentLocation", "Location");
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Lokasyon Tanımınız Eksik";

                        TempData["result"] = result;
                        return RedirectToAction("Index", "Login");
                    }

                }
                else
                {
                    if (User.RoleGroupID == null || User.RoleGroupID <= 0)
                    {
                        result.Message += " Kullanıcıya Rol Grubu Tanımı Yapılmamıştır. ";
                        LocationHelper.AddApplicationLog("Location", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı pasif durumdadır.", string.Empty, DateTime.UtcNow, User.FullName, LocationHelper.GetIPAddress(), string.Empty, null);
                    }

                    if (User.IsActive == false)
                    {
                        result.Message += " Kullanıcı Pasif Durumdadır. ";
                        LocationHelper.AddApplicationLog("Location", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı pasif durumdadır.", string.Empty, DateTime.UtcNow, User.FullName, LocationHelper.GetIPAddress(), string.Empty, null);
                    }

                    if (User.IsDismissal == true)
                    {
                        result.Message += " Kullanıcı kilitli durumdadır. Sistem yöneticinize başvurunuz. ";
                        LocationHelper.AddApplicationLog("Location", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı kilitli durumdadır. Sistem yöneticinize başvurunuz.", string.Empty, DateTime.UtcNow, User.FullName, LocationHelper.GetIPAddress(), string.Empty, null);
                    }

                    if (roleGroup.RoleLevel1.LevelNumber < 2)
                    {
                        result.Message += " Kullanıcı yetkiniz bulunmamaktadır. Sistem yöneticinize başvurunuz. ";
                        LocationHelper.AddApplicationLog("Location", "Login", "Select", User.EmployeeID.ToString(), "Login", "Login", null, false, $"{User.Username} kullanıcısı yetkiniz bulunmamaktadır. Sistem yöneticinize başvurunuz.", string.Empty, DateTime.UtcNow, User.FullName, LocationHelper.GetIPAddress(), string.Empty, null);
                    }
                }
            }
            else
            {
                result.Message = " Kullanıcı geçersizdir. ";
                LocationHelper.AddApplicationLog("Location", "Login", "Select", string.Empty, "Login", "Login", null, false, $"{Username} kullanıcısı bulunmamaktadır. Sistem yöneticinize başvurunuz.", string.Empty, DateTime.UtcNow, Username, LocationHelper.GetIPAddress(), string.Empty, null);
            }

            TempData["result"] = result;
            return RedirectToAction("Index", "Login");
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