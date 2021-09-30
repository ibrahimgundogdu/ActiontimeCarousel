using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class SchedulesController : BaseController
    {
        // GET: Schedules
        public ActionResult Index()
        {
            ScheduleControlModel model = new ScheduleControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;

            var isdatekey = Db.DateList.FirstOrDefault(x => x.DateKey == processDate);

            model.DateList = Db.DateList.Where(x => x.WeekKey == isdatekey.WeekKey).ToList();
            model.LocationSchedules = Db.LocationSchedule.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Year == isdatekey.Year && x.Week == isdatekey.Week).ToList();
            model.EmployeeSchedules = Db.Schedule.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Year == isdatekey.Year && x.Week == isdatekey.Week).ToList();
            var employees = Db.GetLocationEmployees(model.Authentication.CurrentLocation.ID).Where(x=> x.Active == true).ToList();
            model.Employees = employees.Select(x => new DataEmployee()
            {
                EmployeeFullname = x.FullName,
                EmployeeID = x.EmployeeID
            }).ToList();



            return View(model);
        }
    }
}