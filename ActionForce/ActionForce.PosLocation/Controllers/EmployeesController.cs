using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class EmployeesController : BaseController
    {
        public DocumentManager documentManager;

        public ActionResult Index()
        {
            EmployeesControlModel model = new EmployeesControlModel();
            model.Authentication = this.AuthenticationData;

            PosManager manager = new PosManager();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }
            var location = Db.Location.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID);
            model.Employees = manager.GetLocationEmployeeModelsToday(location);

            return View(model);
        }


        public ActionResult EmployeeShiftEnd(int EmployeeID, string Token)
        {
            EmployeesControlModel model = new EmployeesControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            var breakresult = EmployeeBreakEnd(EmployeeID, Token);


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

            var result = documentManager.EmployeeShiftEnd(Token, processDate, model.Authentication.CurrentLocation.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }

        public ActionResult EmployeeShiftStart(int EmployeeID, string Token)
        {
            EmployeesControlModel model = new EmployeesControlModel();
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

            var result = documentManager.EmployeeShiftStart(Token, processDate, model.Authentication.CurrentLocation.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }

        public ActionResult EmployeeBreakStart(int EmployeeID, string Token)
        {
            EmployeesControlModel model = new EmployeesControlModel();
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

            var result = documentManager.EmployeeBreakStart(Token, processDate, model.Authentication.CurrentLocation.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }

        public ActionResult EmployeeBreakEnd(int EmployeeID, string Token)
        {
            EmployeesControlModel model = new EmployeesControlModel();
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

            var result = documentManager.EmployeeBreakEnd(Token, processDate, model.Authentication.CurrentLocation.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }


















    }
}