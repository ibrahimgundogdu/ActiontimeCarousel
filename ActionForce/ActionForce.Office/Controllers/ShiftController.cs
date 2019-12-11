using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ShiftController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index(string date)
        {
            ShiftControlModel model = new ShiftControlModel();


            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<EmployeeShift> ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }

            model.CurrentDate = datekey;
            model.TodayDateCode = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date.ToString("yyyy-MM-dd");
            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");
            model.PrevDateCode = _date.AddDays(-1).Date.ToString("yyyy-MM-dd");
            model.NextDateCode = _date.AddDays(1).Date.ToString("yyyy-MM-dd");

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            List<int> locationids = model.Locations.Select(x => x.LocationID).ToList();

            model.LocationSchedules = Db.LocationSchedule.Where(x => locationids.Contains(x.LocationID.Value) && x.ShiftDate == datekey.DateKey && x.StatusID == 2).ToList();
            model.LocationShifts = Db.LocationShift.Where(x => locationids.Contains(x.LocationID) && x.ShiftDate == datekey.DateKey).ToList();

            model.EmployeeSchedules = Db.Schedule.Where(x => locationids.Contains(x.LocationID.Value) && x.ShiftDate == model.CurrentDate.DateKey).ToList();
            List<int> employeeids = model.EmployeeSchedules.Select(x => x.EmployeeID.Value).ToList();

            model.Employees = Db.Employee.Where(x => employeeids.Contains(x.EmployeeID) && x.IsActive == true).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsWorkTime == true).ToList();
            model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsBreakTime == true).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult LocationShift()
        {
            ShiftControlModel model = new ShiftControlModel();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult EmployeeShift()
        {
            ShiftControlModel model = new ShiftControlModel();

            return View(model);
        }
    }
}