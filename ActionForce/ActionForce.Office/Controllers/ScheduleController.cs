using ActionForce.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ScheduleController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            ScheduleControlModel model = new ScheduleControlModel();

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            var date = DateTime.Now.Date;
            var datekey = Db.DateList.Where(x => x.DateKey == date);
            var schedulelist = Db.VLocationSchedule.Where(x => x.Year == date.Year && x.Month == date.Month && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.StatusID == 2).Select(x => new ScheduleItem()
            {
                id = x.ID.ToString(),
                start = x.ShiftDateStartString,
                end = x.ShiftDateEndString,
                title = x.LocationFullName
            }).ToArray();

            model.calendarEvents = JsonConvert.SerializeObject(schedulelist);

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
                model.DateBegin = DateTime.Now.AddDays(-7).Date;
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.AddDays(7).Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Index", "Schedule");
        }


        [AllowAnonymous]
        public ActionResult Location()
        {
            ScheduleControlModel model = new ScheduleControlModel();

            return View(model);
        }
        [AllowAnonymous]
        public ActionResult Employee()
        {
            ScheduleControlModel model = new ScheduleControlModel();

            return View(model);
        }
    }
}