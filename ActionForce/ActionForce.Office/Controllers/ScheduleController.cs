using ActionForce.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ActionForce.Office.Controllers
{
    public class ScheduleController : BaseController
    {
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

        public ActionResult Location(string week, string date)
        {
            ScheduleControlModel model = new ScheduleControlModel();

            if (TempData["result"] != null)
            {
                model.ResultMessage = TempData["result"] as Result<Schedule> ?? null;
            }

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

            TempData["Model"] = model;

            return View(model);
        }

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
        public ActionResult AddUpdateLocationSchedule(LocationScheduleEdit[] schedulelist)
        {
            Result<Schedule> result = new Result<Schedule>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

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

                        LocationSchedule self = new LocationSchedule()
                        {
                            Day = locschedule.Day,
                            DayName = locschedule.DayName,
                            DurationMinute = locschedule.DurationMinute,
                            ID = locschedule.ID,
                            LocationID = locschedule.LocationID,
                            Month = locschedule.Month,
                            RecordDate = locschedule.RecordDate,
                            RecordEmployee = locschedule.RecordEmployee,
                            RecordIP = locschedule.RecordIP,
                            ShiftDate = locschedule.ShiftDate,
                            ShiftdateEnd = locschedule.ShiftdateEnd,
                            ShiftDateStart = locschedule.ShiftDateStart,
                            StatusID = locschedule.StatusID,
                            UpdateDate = locschedule.UpdateDate,
                            UpdateEmployee = locschedule.UpdateEmployee,
                            UpdateIP = locschedule.UpdateIP,
                            Week = locschedule.Week,
                            Year = locschedule.Year
                        };





                        locschedule.ShiftDateStart = startdatetime;
                        locschedule.ShiftdateEnd = enddatetime;
                        locschedule.UpdateDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
                        locschedule.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        locschedule.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message += $"{locschedule.LocationID} ID li lokasyonun {locschedule.ShiftDate.Value.ToShortDateString()} tarihli takvimi güncellendi.";

                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<LocationSchedule>(self, locschedule, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Schedule", "Update", locschedule.ID.ToString(), "Schedule", "AddUpdateLocationSchedule", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty,null);


                    }
                    else
                    {
                        result.IsSuccess = true;
                        result.Message += $"{locschedule.LocationID} ID li lokasyonun {locschedule.ShiftDate.Value.ToShortDateString()} tarihli takvimi silindi.";

                        OfficeHelper.AddApplicationLog("Office", "Schedule", "Delete", locschedule.ID.ToString(), "Schedule", "AddUpdateLocationSchedule", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, locschedule);


                        Db.LocationSchedule.Remove(locschedule);
                        Db.SaveChanges();

                        
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.isActive))
                    {
                        try
                        {


                            DateTime? _dateKey = Convert.ToDateTime(item.dateKey);

                            DateTime? startdate = Convert.ToDateTime(item.ShiftBeginDate);
                            TimeSpan? starttime = Convert.ToDateTime(item.ShiftBeginTime + ":00").TimeOfDay;
                            DateTime? startdatetime = startdate.Value.Add(starttime.Value);

                            DateTime? enddate = Convert.ToDateTime(item.ShiftEndDate);
                            TimeSpan? endtime = Convert.ToDateTime(item.ShiftEndTime + ":00").TimeOfDay;
                            DateTime? enddatetime = enddate.Value.Add(endtime.Value);

                            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _dateKey);

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

                            result.IsSuccess = true;
                            result.Message += $"{locschedule.LocationID} ID li lokasyonun {locschedule.ShiftDate.Value.ToShortDateString()} tarihli takvimi eklendi.";

                            OfficeHelper.AddApplicationLog("Office", "Schedule", "Insert", locschedule.ID.ToString(), "Schedule", "AddUpdateLocationSchedule", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, locschedule);

                        }
                        catch (Exception ex)
                        {
                            result.Message += $"{item.locationID} ID li lokasyonun {item.dateKey} tarihli takviminde hata oluştu : " + ex.Message;
                            OfficeHelper.AddApplicationLog("Office", "Schedule", "Insert", "0", "Schedule", "AddUpdateLocationSchedule", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                    }


                }
            }

            TempData["result"] = result;

            return RedirectToAction("Location", "Schedule", new { week = weekcode });
        }

        public ActionResult Employee(int? locationid, string week, string date)
        {
            ScheduleControlModel model = new ScheduleControlModel();

            if (TempData["result"] != null)
            {
                model.ResultMessage = TempData["result"] as Result<Schedule> ?? null;
            }

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

            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true);

            if (locationid != null)
            {
                model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == locationid);
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

            var schedulelist = Db.VLocationSchedule.Where(x => x.WeekCode.Trim() == weekcode && x.LocationID == model.CurrentLocation.LocationID).ToList();
            model.VLocationSchedule = schedulelist;

            model.EmployeeSchedule = Db.VSchedule.Where(x => x.WeekCode.Trim() == weekcode && x.LocationID == model.CurrentLocation.LocationID).ToList();

            List<int> employeeids = model.EmployeeSchedule.Select(x => x.EmployeeID.Value).Distinct().ToList();
            List<int> employeeids2 = Db.EmployeeLocation.Where(x => x.LocationID == model.CurrentLocation.LocationID && x.IsActive == true && x.Employee.IsActive == true && x.Employee.Role.Stage > 0).Select(x=> x.EmployeeID).Distinct().ToList();

            employeeids.AddRange(employeeids2);
            employeeids = employeeids.Distinct().ToList();

            model.Employees = Db.Employee.Where(x => employeeids.Contains(x.EmployeeID)).ToList();
            model.EmployeeLocations = Db.EmployeeLocation.Where(x => employeeids.Contains(x.EmployeeID) && x.LocationID == model.CurrentLocation.LocationID && x.IsActive == true && x.Employee.IsActive == true && x.Employee.Role.Stage > 0).ToList();


            TempData["Model"] = model;

            return View(model);
        }

        public PartialViewResult AddEmployeeSchedule(int id, int empid, string week)
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

            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == empid);
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == id);


            var schedulelist = Db.VSchedule.Where(x => x.WeekCode.Trim() == weekcode && x.LocationID == id && x.EmployeeID == empid).ToList();
            model.EmployeeSchedule = schedulelist;



            return PartialView("_PartialAddEmployeeSchedule", model);
        }

        [HttpPost]
        public ActionResult AddUpdateEmployeeSchedule(EmployeeScheduleEdit[] schedulelist)
        {
            Result<Schedule> result = new Result<Schedule>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ScheduleControlModel model = new ScheduleControlModel();

            string weekcode = "";
            int? locationID = 0;

            foreach (var item in schedulelist)
            {
                weekcode = item.weekCode;
                locationID = item.locationID;

                if (item.scheduleID > 0)
                {
                    DateTime? startdate = Convert.ToDateTime(item.ShiftBeginDate);
                    TimeSpan? starttime = Convert.ToDateTime(item.ShiftBeginTime + ":00").TimeOfDay;
                    DateTime? startdatetime = startdate.Value.Add(starttime.Value);

                    DateTime? enddate = Convert.ToDateTime(item.ShiftEndDate);
                    TimeSpan? endtime = Convert.ToDateTime(item.ShiftEndTime + ":00").TimeOfDay;
                    DateTime? enddatetime = enddate.Value.Add(endtime.Value);

                    var empschedule = Db.Schedule.FirstOrDefault(x => x.Id == item.scheduleID);

                    if (!string.IsNullOrEmpty(item.isActive))
                    {
                        Schedule self = new Schedule()
                        {
                            Day = empschedule.Day,
                            DayName = empschedule.DayName,
                            DurationMinute = empschedule.DurationMinute,
                            Id = empschedule.Id,
                            LocationID = empschedule.LocationID,
                            Month = empschedule.Month,
                            RecordDate = empschedule.RecordDate,
                            RecordEmployee = empschedule.RecordEmployee,
                            RecordIP = empschedule.RecordIP,
                            ShiftDate = empschedule.ShiftDate,
                            ShiftdateEnd = empschedule.ShiftdateEnd,
                            ShiftDateStart = empschedule.ShiftDateStart,
                            StatusID = empschedule.StatusID,
                            UpdateDate = empschedule.UpdateDate,
                            UpdateEmployee = empschedule.UpdateEmployee,
                            UpdateIP = empschedule.UpdateIP,
                            Week = empschedule.Week,
                            Year = empschedule.Year,
                            EmployeeID = empschedule.EmployeeID,
                            ShiftEnd = empschedule.ShiftEnd,
                            ShiftStart = empschedule.ShiftStart
                        };

                        empschedule.ShiftDateStart = startdatetime;
                        empschedule.ShiftdateEnd = enddatetime;
                        empschedule.ShiftStart = starttime;
                        empschedule.ShiftEnd = endtime;
                        empschedule.UpdateDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
                        empschedule.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        empschedule.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message += $"{empschedule.EmployeeID} ID li çalışanın {empschedule.LocationID} ID li lokasyonda {empschedule.ShiftDate.Value.ToShortDateString()} tarihli takvimi güncellendi.";

                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<Schedule>(self, empschedule, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Schedule", "Update", empschedule.Id.ToString(), "Schedule", "AddUpdateEmployeeSchedule", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty,null);

                    }
                    else
                    {
                        result.IsSuccess = true;
                        result.Message += $"{empschedule.EmployeeID} ID li çalışanın {empschedule.LocationID} ID li lokasyonda {empschedule.ShiftDate.Value.ToShortDateString()} tarihli takvimi silindi.";

                        OfficeHelper.AddApplicationLog("Office", "Schedule", "Delete", empschedule.Id.ToString(), "Schedule", "AddUpdateEmployeeSchedule", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, empschedule);


                        Db.Schedule.Remove(empschedule);
                        Db.SaveChanges();

                        

                    }
                }
                else
                {


                    if (!string.IsNullOrEmpty(item.isActive))
                    {
                        try
                        {


                            DateTime? _dateKey = Convert.ToDateTime(item.dateKey);

                            DateTime? startdate = Convert.ToDateTime(item.ShiftBeginDate);
                            TimeSpan? starttime = Convert.ToDateTime(item.ShiftBeginTime + ":00").TimeOfDay;
                            DateTime? startdatetime = startdate.Value.Add(starttime.Value);

                            DateTime? enddate = Convert.ToDateTime(item.ShiftEndDate);
                            TimeSpan? endtime = Convert.ToDateTime(item.ShiftEndTime + ":00").TimeOfDay;
                            DateTime? enddatetime = enddate.Value.Add(endtime.Value);

                            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _dateKey);

                            Schedule locschedule = new Schedule();

                            locschedule.Day = datekey.Day;
                            locschedule.DayName = datekey.DayNameTR;
                            locschedule.LocationID = item.locationID;
                            locschedule.EmployeeID = item.employeeID;
                            locschedule.Month = datekey.Month;
                            locschedule.ShiftDate = _dateKey;
                            locschedule.StatusID = 2;
                            locschedule.Week = datekey.WeekNumber;
                            locschedule.Year = datekey.WeekYear;

                            locschedule.ShiftStart = starttime;
                            locschedule.ShiftEnd = endtime;

                            locschedule.ShiftDateStart = startdatetime;
                            locschedule.ShiftdateEnd = enddatetime;
                            locschedule.RecordDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
                            locschedule.RecordEmployee = model.Authentication.ActionEmployee.EmployeeID;
                            locschedule.RecordIP = OfficeHelper.GetIPAddress();

                            Db.Schedule.Add(locschedule);
                            Db.SaveChanges();

                            result.IsSuccess = true;
                            result.Message += $"{locschedule.EmployeeID} ID li çalışanın {locschedule.LocationID} ID li lokasyonda {locschedule.ShiftDate.Value.ToShortDateString()} tarihli takvimi eklendi.";

                            OfficeHelper.AddApplicationLog("Office", "Schedule", "Insert", locschedule.Id.ToString(), "Schedule", "AddUpdateEmployeeSchedule", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, locschedule);

                        }
                        catch (Exception ex)
                        {
                            result.Message += $"{item.employeeID} ID li çalışanın {item.locationID} ID li lokasyonda {item.dateKey} tarihli takvimi eklenemedi : " + ex.Message;
                            OfficeHelper.AddApplicationLog("Office", "Schedule", "Insert", "0", "Schedule", "AddUpdateEmployeeSchedule", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                        }
                    }


                }
            }

            TempData["result"] = result;

            return RedirectToAction("Employee", "Schedule", new { week = weekcode, locationid = locationID });
        }

        public ActionResult ExportToExcel()
        {
            ScheduleControlModel model = new ScheduleControlModel();

            if (TempData["Model"] != null)
            {
                model = TempData["Model"] as ScheduleControlModel;
            }

            // log alınacak
            string fileName = "ScheduleFile";

            var grid = new GridView();

            if (model.VLocationSchedule != null && model.VLocationSchedule.Count() > 0)
            {
                grid.DataSource = model.VLocationSchedule;
                fileName = $"Location{fileName}{model.WeekCode}";
            }

            if (model.EmployeeSchedule != null && model.EmployeeSchedule.Count() > 0)
            {
                grid.DataSource = model.EmployeeSchedule;
                fileName = $"Employee{fileName}{model.WeekCode}";
            }

            grid.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename="+ fileName + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.Unicode;
            Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());

            Response.Charset = "UTF-8";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

            return View();
        }

        [AllowAnonymous]
        public ActionResult SetLocationSchedule(int? OurCompanyID, int? WeekNumber, int? Year, int? EmployeeID)
        {
            Db.SetLocationSchedules(OurCompanyID, Year, WeekNumber, EmployeeID);

            string weekkey = $"{Year}-{WeekNumber}";

            return RedirectToAction("Location","Schedule",new { week = weekkey });
        }
        
    }
}