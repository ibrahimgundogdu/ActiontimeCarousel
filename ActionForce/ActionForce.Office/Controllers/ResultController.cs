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
        public ActionResult Envelope(string date)
        {
            ResultControlModel model = new ResultControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<DayResult> ?? null;
            }

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }

            model.CurrentDate = datekey;
            model.TodayDateCode = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date.ToString("yyyy-MM-dd");
            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");
            model.PrevDateCode = _date.AddDays(-1).Date.ToString("yyyy-MM-dd");
            model.NextDateCode = _date.AddDays(1).Date.ToString("yyyy-MM-dd");

            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.DayResultList = Db.VDayResult.Where(x => x.Date == datekey.DateKey).ToList();

            return View(model);
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

            var isExists = Db.DayResult.FirstOrDefault(x => x.LocationID == model.LocationID && x.Date == model.ResultDate);

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

            model.CurrentDayResult = Db.VDayResult.FirstOrDefault(x => x.LocationID == filtermodel.LocationID && x.Date == filtermodel.ResultDate);
            if (model.CurrentDayResult != null)
            {
                model.DayResult = Db.DayResult.FirstOrDefault(x => x.ID == model.CurrentDayResult.ID);
            }

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Edit(int id)
        {
            ResultControlModel model = new ResultControlModel();

            model.CurrentDayResult = Db.VDayResult.FirstOrDefault(x => x.ID == id);
            model.DayResult = Db.DayResult.FirstOrDefault(x => x.ID == id);

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Detail(int id)
        {
            ResultControlModel model = new ResultControlModel();

            model.CurrentDayResult = Db.VDayResult.FirstOrDefault(x => x.ID == id);
            model.DayResult = Db.DayResult.FirstOrDefault(x => x.ID == id);

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            return View(model);
        }
    }
}