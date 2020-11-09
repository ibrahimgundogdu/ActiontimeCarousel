using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly DocumentManager documentManager;
        public EmployeeController()
        {
            LayoutControlModel model = new LayoutControlModel();

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               LocationHelper.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentOurCompany.ID,
                   Name = model.Authentication.CurrentOurCompany.Name,
                   Currency = model.Authentication.CurrentOurCompany.Currency,
                   TimeZone = model.Authentication.CurrentOurCompany.TimeZone
               }
           );
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            EmployeeControlModel model = new EmployeeControlModel();

            LocationServiceManager manager = new LocationServiceManager(Db, model.Authentication.CurrentLocation);


            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.Employees = manager.GetLocationEmployeesToday();


            return View(model);
        }

        [AllowAnonymous]
        public ActionResult EmployeeShiftEnd(int EmployeeID, string Token)
        {
            EmployeeControlModel model = new EmployeeControlModel();
            DateTime processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            var breakresult = EmployeeBreakEnd(EmployeeID, Token);

            var result = documentManager.EmployeeShiftEnd(Token, processDate, model.Location.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult EmployeeShiftStart(int EmployeeID, string Token)
        {
            EmployeeControlModel model = new EmployeeControlModel();
            DateTime processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            var result = documentManager.EmployeeShiftStart(Token, processDate, model.Location.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult EmployeeBreakStart(int EmployeeID, string Token)
        {
            EmployeeControlModel model = new EmployeeControlModel();
            DateTime processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            var result = documentManager.EmployeeBreakStart(Token, processDate, model.Location.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult EmployeeBreakEnd(int EmployeeID, string Token)
        {
            EmployeeControlModel model = new EmployeeControlModel();
            DateTime processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            var result = documentManager.EmployeeBreakEnd(Token, processDate, model.Location.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }

    }
}