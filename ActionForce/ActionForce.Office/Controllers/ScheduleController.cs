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
        public ActionResult Location(string week, string date)
        {
            ScheduleControlModel model = new ScheduleControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(week))
            {
                var weekparts = week.Split('-');
                int _year = Convert.ToInt32(weekparts[0]);
                int _week = Convert.ToInt32(weekparts[1]);
                datekey = Db.DateList.Where(x => x.WeekYear == _year && x.WeekNumber == _week).OrderBy(x => x.DateKey).FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(date))
            {
                DateTime? _dateurl = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _dateurl);
            }

            string weekcode = $"{datekey.WeekYear}-{datekey.WeekNumber}";
            var weekdatekeys = Db.DateList.Where(x => x.WeekYear == datekey.WeekYear && x.WeekNumber == datekey.WeekNumber).ToList();

            model.WeekCode = weekcode;

            model.CurrentDate = datekey;
            model.WeekList = weekdatekeys;
            model.FirstWeekDay = weekdatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastWeekDay = weekdatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            var prevdate = model.FirstWeekDay.DateKey.AddDays(-1).Date;
            var nextdate = model.LastWeekDay.DateKey.AddDays(1).Date;

            var prevday = Db.DateList.FirstOrDefault(x => x.DateKey == prevdate);
            var nextday = Db.DateList.FirstOrDefault(x => x.DateKey == nextdate);


            model.NextWeekCode = $"{nextday.WeekYear}-{nextday.WeekNumber}";
            model.PrevWeekCode = $"{prevday.WeekYear}-{prevday.WeekNumber}";


            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();



            var schedulelist = Db.VLocationSchedule.Where(x => x.WeekCode.Trim() == weekcode && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.VLocationSchedule = schedulelist;

            List<int> scheduledlocationids = model.VLocationSchedule.Select(x => x.LocationID.Value).ToList();

            model.ScheduledLocationList = model.LocationList.Where(x => scheduledlocationids.Contains(x.LocationID)).ToList();
            model.NonScheduledLocationList = model.LocationList.Where(x => !scheduledlocationids.Contains(x.LocationID)).ToList();

            model.SuccessCount = model.ScheduledLocationList.Count();
            model.WaitingCount = model.NonScheduledLocationList.Count();
            model.TotalCount = (model.SuccessCount + model.WaitingCount);
            model.SuccessRate = (int)(100 * (decimal)((decimal)model.SuccessCount / (decimal)model.TotalCount));

            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult AddLocationSchedule(int id, string week)
        {
            ScheduleControlModel model = new ScheduleControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(week))
            {
                var weekparts = week.Split('-');
                int _year = Convert.ToInt32(weekparts[0]);
                int _week = Convert.ToInt32(weekparts[1]);
                datekey = Db.DateList.Where(x => x.WeekYear == _year && x.WeekNumber == _week).OrderBy(x => x.DateKey).FirstOrDefault();
            }

            string weekcode = $"{datekey.WeekYear}-{datekey.WeekNumber}";
            var weekdatekeys = Db.DateList.Where(x => x.WeekYear == datekey.WeekYear && x.WeekNumber == datekey.WeekNumber).ToList();

            model.WeekCode = weekcode;
            model.CurrentDate = datekey;
            model.WeekList = weekdatekeys;
            model.FirstWeekDay = weekdatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastWeekDay = weekdatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            //model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == id);


            var schedulelist = Db.VLocationSchedule.Where(x => x.WeekCode.Trim() == weekcode && x.LocationID == id).ToList();
            model.VLocationSchedule = schedulelist;



            return PartialView("_PartialAddLocationSchedule", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddUpdateLocationSchedule(LocationScheduleEdit[] schedulelist)
        {
            ScheduleControlModel model = new ScheduleControlModel();

            string weekcode = "";

            foreach (var item in schedulelist)
            {
                weekcode = item.weekCode;

                if (item.scheduleID > 0)
                {
                    DateTime? startdate = Convert.ToDateTime(item.ShiftBeginDate);
                    TimeSpan? starttime = Convert.ToDateTime(item.ShiftBeginTime + ":00").TimeOfDay;
                    DateTime? startdatetime = startdate.Value.Add(starttime.Value);

                    DateTime? enddate = Convert.ToDateTime(item.ShiftEndDate);
                    TimeSpan? endtime = Convert.ToDateTime(item.ShiftEndTime + ":00").TimeOfDay;
                    DateTime? enddatetime = enddate.Value.Add(endtime.Value);

                    var locschedule = Db.LocationSchedule.FirstOrDefault(x => x.ID == item.scheduleID);

                    if (!string.IsNullOrEmpty(item.isActive))
                    {
                        locschedule.ShiftDateStart = startdatetime;
                        locschedule.ShiftdateEnd = enddatetime;
                        locschedule.UpdateDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
                        locschedule.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        locschedule.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();
                    }
                    else
                    {
                        Db.LocationSchedule.Remove(locschedule);
                        Db.SaveChanges();
                    }
                }
                else
                {
                    DateTime? _dateKey = Convert.ToDateTime(item.dateKey);

                    DateTime? startdate = Convert.ToDateTime(item.ShiftBeginDate);
                    TimeSpan? starttime = Convert.ToDateTime(item.ShiftBeginTime + ":00").TimeOfDay;
                    DateTime? startdatetime = startdate.Value.Add(starttime.Value);

                    DateTime? enddate = Convert.ToDateTime(item.ShiftEndDate);
                    TimeSpan? endtime = Convert.ToDateTime(item.ShiftEndTime + ":00").TimeOfDay;
                    DateTime? enddatetime = enddate.Value.Add(endtime.Value);

                    var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _dateKey);

                    if (!string.IsNullOrEmpty(item.isActive))
                    {
                        LocationSchedule locschedule = new LocationSchedule();

                        locschedule.Day = datekey.Day;
                        locschedule.DayName = datekey.DayNameTR;
                        locschedule.LocationID = item.locationID;
                        locschedule.Month = datekey.Month;
                        locschedule.ShiftDate = _dateKey;
                        locschedule.StatusID = 2;
                        locschedule.Week = datekey.WeekNumber;
                        locschedule.Year = datekey.WeekYear;

                        locschedule.ShiftDateStart = startdatetime;
                        locschedule.ShiftdateEnd = enddatetime;
                        locschedule.RecordDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
                        locschedule.RecordEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        locschedule.RecordIP = OfficeHelper.GetIPAddress();

                        Db.LocationSchedule.Add(locschedule);
                        Db.SaveChanges();
                    }


                }
            }

            return RedirectToAction("Location", "Schedule", new { week = weekcode });
        }

        [AllowAnonymous]
        public ActionResult Employee(int? locationid, string week, string date)
        {
            ScheduleControlModel model = new ScheduleControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(week))
            {
                var weekparts = week.Split('-');
                int _year = Convert.ToInt32(weekparts[0]);
                int _week = Convert.ToInt32(weekparts[1]);
                datekey = Db.DateList.Where(x => x.WeekYear == _year && x.WeekNumber == _week).OrderBy(x => x.DateKey).FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(date))
            {
                DateTime? _dateurl = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _dateurl);
            }

            string weekcode = $"{datekey.WeekYear}-{datekey.WeekNumber}";
            var weekdatekeys = Db.DateList.Where(x => x.WeekYear == datekey.WeekYear && x.WeekNumber == datekey.WeekNumber).ToList();

            model.WeekCode = weekcode;

            model.CurrentDate = datekey;
            model.WeekList = weekdatekeys;
            model.FirstWeekDay = weekdatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastWeekDay = weekdatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            var prevdate = model.FirstWeekDay.DateKey.AddDays(-1).Date;
            var nextdate = model.LastWeekDay.DateKey.AddDays(1).Date;

            var prevday = Db.DateList.FirstOrDefault(x => x.DateKey == prevdate);
            var nextday = Db.DateList.FirstOrDefault(x => x.DateKey == nextdate);


            model.NextWeekCode = $"{nextday.WeekYear}-{nextday.WeekNumber}";
            model.PrevWeekCode = $"{prevday.WeekYear}-{prevday.WeekNumber}";


            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            if (locationid != null)
            {
                model.CurrentLocation = Db.VLocation.FirstOrDefault(x=> x.LocationID == locationid);
            }
            


            var schedulelist = Db.VLocationSchedule.Where(x => x.WeekCode.Trim() == weekcode && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.VLocationSchedule = schedulelist;

            List<int> scheduledlocationids = model.VLocationSchedule.Select(x => x.LocationID.Value).ToList();

            model.ScheduledLocationList = model.LocationList.Where(x => scheduledlocationids.Contains(x.LocationID)).ToList();
            model.NonScheduledLocationList = model.LocationList.Where(x => !scheduledlocationids.Contains(x.LocationID)).ToList();

            model.SuccessCount = model.ScheduledLocationList.Count();
            model.WaitingCount = model.NonScheduledLocationList.Count();
            model.TotalCount = (model.SuccessCount + model.WaitingCount);
            model.SuccessRate = (int)(100 * (decimal)((decimal)model.SuccessCount / (decimal)model.TotalCount));

            return View(model);
        }
    }
}