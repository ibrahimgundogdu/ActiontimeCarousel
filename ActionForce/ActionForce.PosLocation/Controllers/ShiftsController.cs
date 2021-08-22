using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class ShiftsController : BaseController
    {
        // GET: Shifts
        public ActionResult Index()
        {
            ShiftControlModel model = new ShiftControlModel();

            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;

            var isdatekey = Db.DateList.FirstOrDefault(x => x.DateKey == processDate);

            model.DateList = Db.DateList.Where(x => x.WeekKey == isdatekey.WeekKey).ToList();
            List<DateTime> datelist = model.DateList.Select(x => x.DateKey).Distinct().ToList();
            model.LocationSchedules = Db.LocationSchedule.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Year == isdatekey.Year && x.Month == isdatekey.Month && x.Week == isdatekey.Week).ToList();
            model.EmployeeSchedules = Db.Schedule.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Year == isdatekey.Year && x.Month == isdatekey.Month && x.Week == isdatekey.Week).ToList();
            var employees = Db.GetLocationEmployees(model.Authentication.CurrentLocation.ID).Where(x => x.Active == true).ToList();
            model.Employees = employees.Select(x => new DataEmployee()
            {
                EmployeeFullname = x.FullName,
                EmployeeID = x.EmployeeID
            }).ToList();

            model.LocationShifts = Db.LocationShift.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && datelist.Contains(x.ShiftDate)).ToList();
            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && datelist.Contains(x.ShiftDate.Value)).ToList();

            return View(model);
        }
    }
}