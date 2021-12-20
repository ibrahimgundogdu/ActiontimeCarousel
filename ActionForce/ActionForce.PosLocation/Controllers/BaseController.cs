using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class BaseController : Controller
    {
        public ActionTimeEntities Db { get; set; } = new ActionTimeEntities();
        public AuthenticationModel AuthenticationData { get; set; }

        public BaseController()
        {
            Db = new ActionTimeEntities();
            AuthenticationData = new AuthenticationModel();
        }
        protected override void OnActionExecuting(ActionExecutingContext context)
        {

            // 01. lokasyon Kontrolü
            if (context.RouteData.Values["controller"].ToString() != "Setup")
            {
                HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

                if (locationCookie == null)
                {
                    context.Result = new RedirectResult("/Setup/Index");
                    return;
                }

                HttpCookie tokencookie = System.Web.HttpContext.Current.Request.Cookies["AuthenticationToken"];

                if (tokencookie != null && !string.IsNullOrEmpty(tokencookie.Value))
                {
                    string token = tokencookie.Value;
                    var tokenparsed = token.Split('|').ToList();
                    var locationuid = tokenparsed[0];
                    var employeeuid = tokenparsed[1];
                    var lockedEmployeeuid = tokenparsed[2];

                    if (string.IsNullOrEmpty(locationuid))
                    {
                        context.Result = new RedirectResult("/Setup/Index");
                        return;
                    }

                    if (!string.IsNullOrEmpty(employeeuid) && string.IsNullOrEmpty(lockedEmployeeuid))
                    {
                        var employee = Db.Employee.FirstOrDefault(x => x.EmployeeUID.ToString() == employeeuid);
                        var location = Db.Location.FirstOrDefault(x => x.LocationUID.ToString() == locationuid);

                        AuthenticationData.CurrentEmployee = new CurrentEmployee()
                        {
                            EmployeeID = employee.EmployeeID,
                            FotoFile = employee.FotoFile,
                            FullName = employee.FullName,
                            Token = employee.EmployeeUID,
                            Username = employee.Username
                        };
                        AuthenticationData.CurrentLocation = new CurrentLocation()
                        {
                            Currency = location.Currency,
                            FullName = location.LocationFullName,
                            ID = location.LocationID,
                            OurCompanyID = location.OurCompanyID,
                            TimeZone = location.Timezone ?? 3,
                            UID = location.LocationUID,
                            LocationTypeID = location.LocationTypeID ?? 0
                        };
                        AuthenticationData.IsCardSystem = location.UseCardSysteme ?? false;

                        return;
                    }

                    if (string.IsNullOrEmpty(employeeuid) && !string.IsNullOrEmpty(lockedEmployeeuid))
                    {
                        context.Result = new RedirectResult("/Setup/Lock");
                        return;
                    }

                    if (string.IsNullOrEmpty(employeeuid) && string.IsNullOrEmpty(lockedEmployeeuid))
                    {
                        context.Result = new RedirectResult("/Setup/Login");
                        return;
                    }

                }
                else
                {
                    context.Result = new RedirectResult("/Setup/Employee");
                    return;
                }

            }





            base.OnActionExecuting(context);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Db != null)
            {
                Db.Dispose();
            }

            base.Dispose(disposing);
        }



    }
}