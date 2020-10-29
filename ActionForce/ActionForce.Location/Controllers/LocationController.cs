using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ActionForce.Location.Controllers
{
    public class LocationController : BaseController
    {
        // GET: Location
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SetCurrentLocation()
        {
            LocationControlModel model = new LocationControlModel();

            List<mLocation> mLocations = new List<mLocation>();

            List<int> locationisd = Db.EmployeeLocation.Where(x => x.EmployeeID == model.Authentication.CurrentEmployee.EmployeeID).Select(x => x.LocationID).ToList();

            var Locations = Db.Location.Where(x => locationisd.Contains(x.LocationID) && x.IsActive == true).ToList();
            var ourcompanies = Db.OurCompany.ToList();
            var locationtypes = Db.LocationType.ToList();


            foreach (var location in Locations)
            {
                var locationtype = locationtypes.FirstOrDefault(x => x.ID == location.LocationTypeID);
                var ourcompany = ourcompanies.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);

                var shift = Db.GetLocationCurrentShift(location.LocationID, location.LocalDate).Select(x => new mLocationShift()
                {
                    LocationID = x.LocationID,
                    DurationMinute = x.DurationMinute,
                    ShiftDate = x.ShiftDate,
                    ShiftStart = x.ShiftDateStart,
                    ShiftEnd = x.ShiftDateFinish
                }).FirstOrDefault();

                mLocations.Add(new mLocation()
                {
                    LocationID = location.LocationID,
                    LocationName = $"{location.LocationName} {location.Description} {location.State}",
                    SortBy = location.SortBy,
                    TypeName = locationtype.TypeName ?? location.Description,
                    Status = shift == null ? LocationStatus.Beklemede : shift != null && shift.DurationMinute > 0 ? LocationStatus.Kapalı : LocationStatus.Açık,
                    CompanyCode = ourcompany.Code,
                    UID = location.LocationUID
                });
            }

            model.Locations = mLocations;

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult InitSelectedLocation(Guid? id)
        {
            LocationControlModel model = new LocationControlModel();
            model.Result = new Result();

            if (id != null)
            {
                var location = Db.Location.FirstOrDefault(x => x.LocationUID == id);

                if (location != null)
                {

                    if (model.Authentication.CurrentRoleGroup.RoleLevel == 2)
                    {
                        if (Db.EmployeeLocation.Any(x => x.EmployeeID == model.Authentication.CurrentEmployee.EmployeeID && x.LocationID == location.LocationID && x.IsActive == true))
                        {
                            var res = ChangeLocation(location, model.Authentication);

                            model.Result.IsSuccess = res.IsSuccess;
                            model.Result.Message = res.Message;
                        }
                        else
                        {
                            model.Result.Message = "Kullanıcı Seçilen Lokasyonda Tanımlı Değil";
                        }
                    }
                    else if (model.Authentication.CurrentRoleGroup.RoleLevel >= 3)
                    {
                        var res = ChangeLocation(location, model.Authentication);

                        model.Result.IsSuccess = res.IsSuccess;
                        model.Result.Message = res.Message;
                    }
                    else
                    {
                        model.Result.Message = "Kullanıcı Lokasyon Değişimine Yetkili Değil";
                    }
                }
                else
                {
                    model.Result.Message = "Lokasyon Bulunamadı";
                }

                TempData["result"] = model.Result;

                if (model.Result.IsSuccess)
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
                model.Result.Message = "Lokasyon ID si Geçersiz";
                TempData["result"] = model.Result;

                return RedirectToAction("SetCurrentLocation", "Location");
            }
        }

        [AllowAnonymous]
        public Result ChangeLocation(Entity.Location location, AuthenticationModel user)
        {
            Result result = new Result();

            if (location != null && user != null)
            {
                if (this.HttpContext.User != null && this.HttpContext.User is GenericPrincipal principal && principal.Identity is FormsIdentity identity)
                {
                    AuthenticationModel Authentication = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticationModel>(identity.Ticket.UserData);

                    Authentication.CurrentLocation = new LocationInfo()
                    {
                        //Currency = location.Currency,
                        FullName = $"{location.LocationName} {location.Description} {location.State}",
                        ID = location.LocationID,
                        //IsActive = location.IsActive.Value,
                        //OurCompanyID = location.OurCompanyID,
                        //Latitude = location.Latitude,
                        //Longitude = location.Longitude,
                        //SortBy = location.SortBy,
                        TimeZone = location.Timezone ?? 3,
                        UID = location.LocationUID
                    };

                    LocationHelper.AddApplicationLog("Location", "ChangeLocation", "Select", user.CurrentEmployee.EmployeeID.ToString(), "Location", "InitSelectedLocation", null, true, $"{Authentication.CurrentLocation.ID} lokasyonuna başarılı bir değişim yapıldı.", string.Empty, DateTime.UtcNow, user.CurrentEmployee.FullName, LocationHelper.GetIPAddress(), string.Empty, null);

                    var userData = Newtonsoft.Json.JsonConvert.SerializeObject(Authentication);
                    var ticket = new FormsAuthenticationTicket(2, user.CurrentEmployee.Username, DateTime.Now, DateTime.Now.AddMinutes(1440), true, userData, FormsAuthentication.FormsCookiePath);
                    string hash = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);
                    cookie.SameSite = SameSiteMode.Lax;

                    if (ticket.IsPersistent)
                    {
                        cookie.Expires = ticket.Expiration;
                    }

                    Response.Cookies.Add(cookie);

                    result.IsSuccess = true;
                    result.Message = " Lokasyon değişimi tanımlandı ";
                }
            }
            else
            {
                result.Message = " Lokasyon veya Kullanıcı bilgisi bulunamadı ";
            }

            return result;
        }
    }
}