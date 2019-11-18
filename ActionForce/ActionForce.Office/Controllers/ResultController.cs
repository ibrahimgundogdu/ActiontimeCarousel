using ActionForce.Entity;
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
                model.ResultMessage = TempData["result"] as Result<Result> ?? null;
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

            model.ResultList = Db.VResult.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            if (model.Filters.LocationID > 0)
            {
                model.ResultList = model.ResultList.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }

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

            return RedirectToAction("Index", "Result");
        }

        [AllowAnonymous]
        public ActionResult New()
        {
            ResultControlModel model = new ResultControlModel();

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CheckNew(int? locationId, DateTime? resultDate)
        {
            ResultFilterModel model = new ResultFilterModel();

            model.LocationID = locationId;
            model.ResultDate = resultDate;

            if (resultDate == null)
            {
                model.ResultDate = DateTime.Now.Date;
            }

            var isExists = Db.Result.FirstOrDefault(x => x.LocationID == model.LocationID && x.Date == model.ResultDate);

            if (isExists != null)
            {
                model.ResultID = isExists.ID;
                TempData["filter"] = model;
                return RedirectToAction("Edit", "Result",new {id = model.ResultID });
            }
            else
            {
                TempData["filter"] = model;
                return RedirectToAction("Add", "Result");
            }
        }

        [AllowAnonymous]
        public ActionResult Add()
        {
            ResultControlModel model = new ResultControlModel();
            ResultFilterModel filtermodel = new ResultFilterModel();

            if (TempData["filter"] != null)
            {
                filtermodel = TempData["filter"] as ResultFilterModel;
            }

            model.CurrentResult = Db.VResult.FirstOrDefault(x => x.LocationID == filtermodel.LocationID && x.Date == filtermodel.ResultDate);
            if (model.CurrentResult != null)
            {
                model.Result = Db.Result.FirstOrDefault(x => x.ID == model.CurrentResult.ID);
            }

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Edit(int id)
        {
            ResultControlModel model = new ResultControlModel();

            model.CurrentResult = Db.VResult.FirstOrDefault(x => x.ID == id);
            model.Result = Db.Result.FirstOrDefault(x => x.ID == id);

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Detail(int id)
        {
            ResultControlModel model = new ResultControlModel();

            model.CurrentResult = Db.VResult.FirstOrDefault(x => x.ID == id);
            model.Result = Db.Result.FirstOrDefault(x => x.ID == id);

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            return View(model);
        }
    }
}