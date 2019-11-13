using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class CashController : BaseController
    {
        // GET: Cash
        [AllowAnonymous]
        public ActionResult Index(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.LocationID = locationId != null ? locationId : Db.Location.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).LocationID;
                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x=> x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashCollections = Db.VDocumentCashCollections.Where(x => x.LocationID == model.Filters.LocationID && x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x=> x.RecordDate).ToList();
            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Filter(int? locationId, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();

            model.LocationID = locationId;
            model.DateBegin = beginDate;
            model.DateEnd = endDate;

            if (beginDate == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Index", "Action");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashCollection(NewCashCollect cashCollect)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>();
            CashControlModel model = new CashControlModel();

            if (cashCollect != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashCollect.LocationID);
                var fromPrefix = cashCollect.FromID.Substring(0,1);
                var fromID = Convert.ToInt32(cashCollect.FromID.Substring(1, cashCollect.FromID.Length-1));
                var amount = Convert.ToDouble(cashCollect.Amount);
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashCollect.DocumentDate,out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashCollect.LocationID, cashCollect.Currency);
                // tahsilat eklenir.

                try
                {
                    DocumentCashCollections newCashColl = new DocumentCashCollections();

                    newCashColl.ActionTypeID = actType.ID;
                    newCashColl.ActionTypeName = actType.Name;
                    newCashColl.Amount = amount;
                    newCashColl.CashID = cash.ID;
                    newCashColl.Currency = currency;
                    newCashColl.Date = docDate;
                    newCashColl.Description = cashCollect.Description;
                    newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "CC");





                }
                catch (Exception ex)
                {


                }





            }

            TempData["result"] = result;

            return RedirectToAction("Index", "Action");
        }
    }
}