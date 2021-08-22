using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class LocationController : BaseController
    {
        public DocumentManager documentManager;

        public ActionResult Index()
        {
            LocationControlModel model = new LocationControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;

            model.Schedule = Db.LocationSchedule.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ShiftDate == processDate).Select(x => new LocationScheduleInfo()
            {
                LocationID = x.LocationID.Value,
                ScheduleDate = x.ShiftDate.Value,
                DateStart = x.ShiftDateStart.Value,
                DateEnd = x.ShiftdateEnd,
                Duration = x.ShiftDuration
            }).FirstOrDefault();

            model.Shift = Db.LocationShift.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ShiftDate == processDate).Select(x => new LocationShiftInfo()
            {
                LocationID = x.LocationID,
                ScheduleDate = x.ShiftDate,
                DateStart = x.ShiftDateStart.Value,
                DateEnd = x.ShiftDateFinish,
                Duration = x.ShiftDuration
            }).FirstOrDefault();


            return View(model);
        }

        public ActionResult LocationShiftStart()
        {
            LocationControlModel model = new LocationControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentLocation.OurCompanyID,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var serviceresult = documentManager.LocationShiftStart(model.Authentication.CurrentEmployee.Token.ToString(), processDate, model.Authentication.CurrentLocation.ID);

            TempData["Result"] = new Result() { IsSuccess = serviceresult.IsSuccess, Message = serviceresult?.Message };

            return RedirectToAction("Index");
        }

        public ActionResult LocationShiftEnd()
        {
            LocationControlModel model = new LocationControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentLocation.OurCompanyID,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var serviceresult = documentManager.LocationShiftEnd(model.Authentication.CurrentEmployee.Token.ToString(), processDate, model.Authentication.CurrentLocation.ID);

            TempData["Result"] = new Result() { IsSuccess = serviceresult.IsSuccess, Message = serviceresult?.Message };

            return RedirectToAction("Index");
        }
    }
}