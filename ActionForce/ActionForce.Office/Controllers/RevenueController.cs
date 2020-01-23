using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class RevenueController : BaseController
    {

        public ActionResult Index(int? WeekYear, int? WeekNumber, int? LocationID, DateTime? date)
        {
            RevenueControlModel model = new RevenueControlModel();

            var currentdate = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var selecteddate = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            if (date != null)
            {
                selecteddate = date.Value.Date;
            }

            DateList cdatelist = Db.DateList.FirstOrDefault(x => x.DateKey == currentdate);
            DateList sdatelist = Db.DateList.FirstOrDefault(x => x.DateKey == selecteddate);
            model.CurrentDate = cdatelist;
            model.SelectedDate = sdatelist;

            model.WeekNumber = WeekNumber ?? sdatelist.WeekNumber.Value;
            model.WeekYear = WeekYear ?? sdatelist.WeekYear.Value;

            var weekdatekeys = Db.DateList.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber).ToList();
            model.WeekList = weekdatekeys;
            model.FirstWeekDay = weekdatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastWeekDay = weekdatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            var prevdate = model.FirstWeekDay.DateKey.AddDays(-1).Date;
            var nextdate = model.LastWeekDay.DateKey.AddDays(1).Date;

            model.PrevWeek = Db.DateList.FirstOrDefault(x => x.DateKey == prevdate);
            model.NextWeek = Db.DateList.FirstOrDefault(x => x.DateKey == nextdate);

            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            model.Revenues = Db.VRevenue.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            List<long> revids = model.Revenues.Select(x => x.ID).Distinct().ToList();

            model.RevenueLines = Db.VRevenueLines.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber && revids.Contains(x.RevenueID.Value)).ToList();

            return View(model);
        }

        public ActionResult Compute(int? WeekYear, int? WeekNumber, int? LocationID)
        {
            RevenueControlModel model = new RevenueControlModel();

            model.WeekYear = WeekYear ?? 0;
            model.WeekNumber = WeekNumber ?? 0;

            if (WeekYear > 0 && WeekNumber > 0)
            {
                if (LocationID > 0)
                {
                    var res = Db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, LocationID);
                }
                else
                {
                    model.Locations = Db.Location.ToList();

                    foreach (var location in model.Locations)
                    {
                        var res = Db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, location.LocationID);
                    }
                }
            }

            model.Revenues = Db.VRevenue.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber).ToList();

            return View(model);
        }

        public PartialViewResult RecalculateRevenue(int WeekYear, int WeekNumber, int LocationID)
        {
            RevenueDetailModel model = new RevenueDetailModel();

            var res = Db.ComputeLocationWeekRevenue(WeekNumber, WeekYear, LocationID);

            model.Revenue = Db.VRevenue.FirstOrDefault(x => x.WeekYear == WeekYear && x.WeekNumber == WeekNumber && x.LocationID == LocationID);
            model.RevenueLines = Db.VRevenueLines.Where(x => x.WeekYear == WeekYear && x.WeekNumber == WeekNumber && x.LocationID == LocationID).ToList();

            return PartialView("_PartialRevenueDetail", model);
        }

        public ActionResult RecalculateRevenueAll(int? WeekYear, int? WeekNumber)
        {
            RevenueControlModel model = new RevenueControlModel();

            if (WeekYear > 0 && WeekNumber > 0)
            {
                model.Locations = Db.Location.ToList();

                foreach (var location in model.Locations)
                {
                    var res = Db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, location.LocationID);
                }
            }

            return RedirectToAction("Index", "Revenue",new { WeekYear, WeekNumber });
        }
    }
}