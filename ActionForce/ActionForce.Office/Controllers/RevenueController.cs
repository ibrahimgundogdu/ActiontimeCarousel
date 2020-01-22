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

            model.Locations = Db.Location.ToList();

            model.Revenues = Db.VRevenue.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber).ToList();
            model.RevenueLines = Db.VRevenueLines.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber).ToList();

            return View(model);
        }

        public ActionResult Compute(int? WeekYear, int? WeekNumber)
        {
            RevenueControlModel model = new RevenueControlModel();

            model.WeekYear = WeekYear ?? 0;
            model.WeekNumber = WeekNumber ?? 0;

            if (WeekYear > 0 && WeekNumber > 0)
            {
                model.Locations = Db.Location.ToList();

                foreach (var location in model.Locations)
                {
                    var res = Db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, location.LocationID);
                }
            }

            model.Revenues = Db.VRevenue.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber).ToList();

            return View(model);
        }
    }
}