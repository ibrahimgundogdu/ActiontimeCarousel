using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ResultController : BaseController
    {
        // GET: Result
        [AllowAnonymous]
        public ActionResult Index()
        {
            ResultControlModel model = new ResultControlModel();

            if (TempData["result"] != null)
            {
                //model.Result = TempData["result"] as Result<Entity.Result> ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            //model.CashCollections = Db.VDocumentCashCollections.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            //if (model.Filters.LocationID > 0)
            //{
            //    model.CashCollections = model.CashCollections.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            //}



            return View(model);
        }
    }
}