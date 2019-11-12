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

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashCollections = Db.VDocumentCashCollections.Where(x => x.LocationID == model.Filters.LocationID && x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x=> x.RecordDate).ToList();


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
    }
}