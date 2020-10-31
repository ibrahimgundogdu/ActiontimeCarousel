using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class CashController : BaseController
    {
        private readonly DocumentManager documentManager;
        public CashController()
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
            CashControlModel model = new CashControlModel();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Collect()
        {
            CashControlModel model = new CashControlModel();
            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.Currencies = Db.Currency.ToList();
            model.CurrentCash = LocationHelper.GetCash(model.Location.ID, model.Location.Currency);
            model.CashCollections = Db.DocumentCashCollections.Where(x => x.LocationID == model.Location.ID && x.Date == documentDate).OrderByDescending(x => x.RecordDate).ToList();


            return View(model);
        }

        //AddCashCollection
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCashCollection(FormCashCollect collect)
        {
            CashControlModel model = new CashControlModel();

            var amount = LocationHelper.GetStringToAmount(collect.Amount);
            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());

            CashCollectionModel documentModel = new CashCollectionModel()
            {
                ActionTypeID = 23,
                ActionTypeName = "Kasa Tahsilatı",
                Amount = amount,
                Currency = collect.Currency,
                Description = collect.Description,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                FromCustomerID = model.Authentication.CurrentOurCompany.ID == 1 ? 1 : 2,
                IsActive = true,
                LocationID = model.Location.ID,
                ResultID = dayResultID                
            };


            var result = documentManager.AddCashCollection(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message};

            return RedirectToAction("Collect");
        }
    }
}