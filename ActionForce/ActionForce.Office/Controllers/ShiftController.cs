using ActionForce.Entity;
using ActionForce.Integration.UfeService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ShiftController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index(string date)
        {
            ShiftControlModel model = new ShiftControlModel();


            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<EmployeeShift> ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }

            var refresh = Db.SetShiftDates();

            model.CurrentDate = datekey;
            model.TodayDateCode = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date.ToString("yyyy-MM-dd");
            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");
            model.PrevDateCode = _date.AddDays(-1).Date.ToString("yyyy-MM-dd");
            model.NextDateCode = _date.AddDays(1).Date.ToString("yyyy-MM-dd");

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            List<int> locationids = model.Locations.Select(x => x.LocationID).ToList();

            model.LocationSchedules = Db.LocationSchedule.Where(x => locationids.Contains(x.LocationID.Value) && x.ShiftDate == datekey.DateKey && x.StatusID == 2).ToList();
            model.LocationShifts = Db.LocationShift.Where(x => locationids.Contains(x.LocationID) && x.ShiftDate == datekey.DateKey).ToList();

            model.EmployeeSchedules = Db.Schedule.Where(x => locationids.Contains(x.LocationID.Value) && x.ShiftDate == model.CurrentDate.DateKey).ToList();
            List<int> employeeids = model.EmployeeSchedules.Select(x => x.EmployeeID.Value).ToList();

            model.Employees = Db.Employee.Where(x => employeeids.Contains(x.EmployeeID)).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsWorkTime == true).ToList();
            model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsBreakTime == true).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult LocationShift(string date)
        {
            ShiftControlModel model = new ShiftControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<EmployeeShift> ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;

            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }

            var refresh = Db.SetShiftDates();

            model.CurrentDate = datekey;
            model.TodayDateCode = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date.ToString("yyyy-MM-dd");
            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");
            model.PrevDateCode = _date.AddDays(-1).Date.ToString("yyyy-MM-dd");
            model.NextDateCode = _date.AddDays(1).Date.ToString("yyyy-MM-dd");

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            List<int> locationids = model.Locations.Select(x => x.LocationID).ToList();

            model.LocationSchedules = Db.LocationSchedule.Where(x => locationids.Contains(x.LocationID.Value) && x.ShiftDate == datekey.DateKey && x.StatusID == 2).ToList();
            model.LocationShifts = Db.LocationShift.Where(x => locationids.Contains(x.LocationID) && x.ShiftDate == datekey.DateKey).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult EmployeeShift(string date)
        {
            ShiftControlModel model = new ShiftControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<EmployeeShift> ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;

            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }

            var refresh = Db.SetShiftDates();

            model.CurrentDate = datekey;
            model.TodayDateCode = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date.ToString("yyyy-MM-dd");
            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");
            model.PrevDateCode = _date.AddDays(-1).Date.ToString("yyyy-MM-dd");
            model.NextDateCode = _date.AddDays(1).Date.ToString("yyyy-MM-dd");

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            List<int> locationids = model.Locations.Select(x => x.LocationID).ToList();

            model.LocationSchedules = Db.LocationSchedule.Where(x => locationids.Contains(x.LocationID.Value) && x.ShiftDate == datekey.DateKey && x.StatusID == 2).ToList();
            model.LocationShifts = Db.LocationShift.Where(x => locationids.Contains(x.LocationID) && x.ShiftDate == datekey.DateKey).ToList();

            model.EmployeeSchedules = Db.Schedule.Where(x => locationids.Contains(x.LocationID.Value) && x.ShiftDate == model.CurrentDate.DateKey).ToList();
            List<int> employeeids = model.EmployeeSchedules.Select(x => x.EmployeeID.Value).ToList();

            model.Employees = Db.Employee.Where(x => employeeids.Contains(x.EmployeeID) && x.IsActive == true).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsWorkTime == true).ToList();
            model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsBreakTime == true).ToList();

            return View(model);
        }


        //OpenLocation

        [HttpPost]
        [AllowAnonymous]
        public JsonResult OpenLocation(int? locationid, int? environmentid, string date)
        {

            ResultControlModel model = new ResultControlModel();
            UfeServiceClient service = new UfeServiceClient(model.Authentication.ActionEmployee.Token);

            var serviceresult = service.LocationShiftStart(locationid.Value, environmentid, 0, 0, date);

            string content = $"<a href='#' onclick='OpenLocation({locationid},{environmentid})'> <i class='ion ion-md-alarm tx-sm-24'></i></a>";
            if (serviceresult?.IsSuccess == true)
            {
                content = $"<a href='#' onclick='CloseLocation({locationid},{environmentid})'> <i class='ion ion-md-sunny tx-sm-24'></i></a>";
            }
            serviceresult.Content = content;

            return Json(serviceresult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult CloseLocation(int? locationid, int? environmentid, string date)
        {

            ResultControlModel model = new ResultControlModel();
            UfeServiceClient service = new UfeServiceClient(model.Authentication.ActionEmployee.Token);

            var serviceresult = service.LocationShiftEnd(locationid.Value, environmentid, 0, 0, date);

            string content = $"<a href='#' onclick='CloseLocation({locationid},{environmentid})'> <i class='ion ion-md-sunny tx-sm-24'></i></a>";

            if (serviceresult.IsSuccess == true)
            {
                content = $"<a href='#'> <i class='ion ion-md-moon tx-sm-24'></i></a>";
            }
            serviceresult.Content = content;

            return Json(serviceresult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult StartEmployeeShift(int locationid, int employeeid, int environmentid, string date)
        {

            EmployeeShiftModel model = new EmployeeShiftModel();

            UfeServiceClient service = new UfeServiceClient(model.Authentication.ActionEmployee.Token);



            var serviceresult = service.EmployeeShiftStart(locationid, employeeid, environmentid, 0, 0, date);

            model.Result = new Result<EmployeeShift>() { IsSuccess = serviceresult.IsSuccess, Message = serviceresult.Message };
            model.Employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employeeid);
            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == locationid);

            var shiftdate = model.Location.LocalDateTime.Value.Date;
            if (!string.IsNullOrEmpty(date))
            {
                shiftdate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            model.CurrentDateCode = shiftdate.ToString("yyyy-MM-dd");
            model.EmployeeShift = Db.EmployeeShift.FirstOrDefault(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.IsWorkTime == true);
            model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.IsBreakTime == true).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.StatusID == 2);


            return PartialView("_PartialEmployeeShiftBreak", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FinishEmployeeShift(int locationid, int employeeid, int environmentid, string date)
        {

            EmployeeShiftModel model = new EmployeeShiftModel();

            UfeServiceClient service = new UfeServiceClient(model.Authentication.ActionEmployee.Token);



            var serviceresult = service.EmployeeShiftEnd(locationid, employeeid, environmentid, 0, 0, date);

            model.Result = new Result<EmployeeShift>() { IsSuccess = serviceresult.IsSuccess, Message = serviceresult.Message };
            model.Employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employeeid);
            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == locationid);

            var shiftdate = model.Location.LocalDateTime.Value.Date;
            if (!string.IsNullOrEmpty(date))
            {
                shiftdate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            model.CurrentDateCode = shiftdate.ToString("yyyy-MM-dd");
            model.EmployeeShift = Db.EmployeeShift.FirstOrDefault(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.IsWorkTime == true);
            model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.IsBreakTime == true).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.StatusID == 2);


            return PartialView("_PartialEmployeeShiftBreak", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult StartEmployeeBreak(int locationid, int employeeid, int environmentid, string date)
        {

            EmployeeShiftModel model = new EmployeeShiftModel();

            UfeServiceClient service = new UfeServiceClient(model.Authentication.ActionEmployee.Token);



            var serviceresult = service.EmployeeBreakStart(locationid, employeeid, environmentid, 0, 0, date);

            model.Result = new Result<EmployeeShift>() { IsSuccess = serviceresult.IsSuccess, Message = serviceresult.Message };
            model.Employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employeeid);
            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == locationid);

            var shiftdate = model.Location.LocalDateTime.Value.Date;
            if (!string.IsNullOrEmpty(date))
            {
                shiftdate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            model.CurrentDateCode = shiftdate.ToString("yyyy-MM-dd");
            model.EmployeeShift = Db.EmployeeShift.FirstOrDefault(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.IsWorkTime == true);
            model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.IsBreakTime == true).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.StatusID == 2);


            return PartialView("_PartialEmployeeShiftBreak", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FinishEmployeeBreak(int locationid, int employeeid, int environmentid, string date)
        {

            EmployeeShiftModel model = new EmployeeShiftModel();

            UfeServiceClient service = new UfeServiceClient(model.Authentication.ActionEmployee.Token);



            var serviceresult = service.EmployeeBreakEnd(locationid, employeeid, environmentid, 0, 0, date);

            model.Result = new Result<EmployeeShift>() { IsSuccess = serviceresult.IsSuccess, Message = serviceresult.Message };
            model.Employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employeeid);
            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == locationid);

            var shiftdate = model.Location.LocalDateTime.Value.Date;
            if (!string.IsNullOrEmpty(date))
            {
                shiftdate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            model.CurrentDateCode = shiftdate.ToString("yyyy-MM-dd");
            model.EmployeeShift = Db.EmployeeShift.FirstOrDefault(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.IsWorkTime == true);
            model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.IsBreakTime == true).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.LocationID == locationid && x.EmployeeID == employeeid && x.ShiftDate == shiftdate && x.StatusID == 2);


            return PartialView("_PartialEmployeeShiftBreak", model);
        }


        [AllowAnonymous]
        public PartialViewResult EditLocationShift(int id, string date)
        {
            ShiftControlModel model = new ShiftControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }


            model.CurrentDate = datekey;

            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == id);


            model.LocationSchedule = Db.LocationSchedule.FirstOrDefault(x => x.ShiftDate == datekey.DateKey && x.LocationID == id);
            model.LocationShift = Db.LocationShift.FirstOrDefault(x => x.ShiftDate == datekey.DateKey && x.LocationID == id);



            return PartialView("_PartialEditLocationShift", model);
        }

        [AllowAnonymous]
        public PartialViewResult EditEmployeeShift(int id, int empid, string date)
        {
            ShiftControlModel model = new ShiftControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }

            model.CurrentDate = datekey;
            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == empid);

            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.ShiftDate == datekey.DateKey && x.LocationID == id && x.EmployeeID == empid);
            model.EmployeeShift = Db.EmployeeShift.FirstOrDefault(x => x.ShiftDate == datekey.DateKey && x.LocationID == id && x.EmployeeID == empid);

            return PartialView("_PartialEditEmployeeShift", model);
        }

        [AllowAnonymous]
        public PartialViewResult EditEmployeeBreak(int id, int empid, string date)
        {
            ShiftControlModel model = new ShiftControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }

            model.CurrentDate = datekey;
            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == empid);

            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.ShiftDate == datekey.DateKey && x.LocationID == id && x.EmployeeID == empid);
            model.EmployeeShift = Db.EmployeeShift.FirstOrDefault(x => x.ShiftDate == datekey.DateKey && x.LocationID == id && x.EmployeeID == empid && x.IsWorkTime == true);
            model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.ShiftDate == datekey.DateKey && x.LocationID == id && x.EmployeeID == empid && x.IsBreakTime == true).ToList();

            return PartialView("_PartialEditEmployeeBreaks", model);
        }



        [AllowAnonymous]
        [HttpPost]
        public ActionResult UpdateLocationShift(long ShiftID, string ShiftBeginDate, string ShiftBeginTime, string ShiftEndDate, string ShiftEndTime)
        {
            ShiftControlModel model = new ShiftControlModel();

            var locationshift = Db.LocationShift.FirstOrDefault(x => x.ID == ShiftID);

            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == locationshift.ShiftDate);

            DateTime? shiftBeginDate = null;
            DateTime? shiftEndDate = null;

            if (!string.IsNullOrEmpty(ShiftBeginDate))
            {
                shiftBeginDate = DateTime.ParseExact(ShiftBeginDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(ShiftBeginTime))
                {
                    TimeSpan shiftBeginTime;
                    if (!TimeSpan.TryParse(ShiftBeginTime, out shiftBeginTime))
                    { }

                    shiftBeginDate = shiftBeginDate.Value.Add(shiftBeginTime);
                }
            }
            else
            {
                locationshift.ShiftDateStart = null;
            }

            if (!string.IsNullOrEmpty(ShiftEndDate))
            {
                shiftEndDate = DateTime.ParseExact(ShiftEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(ShiftEndTime))
                {
                    TimeSpan shiftEndTime;
                    if (!TimeSpan.TryParse(ShiftEndTime, out shiftEndTime))
                    { }
                    shiftEndDate = shiftEndDate.Value.Add(shiftEndTime);
                }
            }
            else
            {
                locationshift.ShiftDateFinish = null;
            }

            if (shiftBeginDate != null && shiftBeginDate is DateTime)
            {
                locationshift.ShiftDateStart = shiftBeginDate;
            }

            if (shiftEndDate != null && shiftEndDate is DateTime)
            {
                locationshift.ShiftDateFinish = shiftEndDate;
            }

            Db.SaveChanges();

            return RedirectToAction("LocationShift", new { date = datekey.DateKey.ToString("yyyy-MM-dd") });
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult UpdateEmployeeShift(long ShiftID, string ShiftBeginDate, string ShiftBeginTime, string ShiftEndDate, string ShiftEndTime)
        {
            ShiftControlModel model = new ShiftControlModel();

            var employeeshift = Db.EmployeeShift.FirstOrDefault(x => x.ID == ShiftID);

            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == employeeshift.ShiftDate);

            DateTime? shiftBeginDate = null;
            DateTime? shiftEndDate = null;

            if (!string.IsNullOrEmpty(ShiftBeginDate))
            {
                shiftBeginDate = DateTime.ParseExact(ShiftBeginDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(ShiftBeginTime))
                {
                    TimeSpan shiftBeginTime;
                    if (!TimeSpan.TryParse(ShiftBeginTime, out shiftBeginTime))
                    { }

                    shiftBeginDate = shiftBeginDate.Value.Add(shiftBeginTime);
                }
            }
            else
            {
                employeeshift.ShiftDateStart = null;
            }

            if (!string.IsNullOrEmpty(ShiftEndDate))
            {
                shiftEndDate = DateTime.ParseExact(ShiftEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(ShiftEndTime))
                {
                    TimeSpan shiftEndTime;
                    if (!TimeSpan.TryParse(ShiftEndTime, out shiftEndTime))
                    { }
                    shiftEndDate = shiftEndDate.Value.Add(shiftEndTime);
                }
            }
            else
            {
                employeeshift.ShiftDateEnd = null;
            }

            if (shiftBeginDate != null && shiftBeginDate is DateTime)
            {
                employeeshift.ShiftDateStart = shiftBeginDate;
            }

            if (shiftEndDate != null && shiftEndDate is DateTime)
            {
                employeeshift.ShiftDateEnd = shiftEndDate;
            }

            Db.SaveChanges();

            return RedirectToAction("Index", new { date = datekey.DateKey.ToString("yyyy-MM-dd") });
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult UpdateEmployeeBreaks(EmployeeBreakEdit[] employeebreaks)
        {
            ShiftControlModel model = new ShiftControlModel();

            string datekey = string.Empty;

            foreach (var item in employeebreaks)
            {
                var employeebreak = Db.EmployeeShift.FirstOrDefault(x => x.ID == item.ShiftID);

                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == employeebreak.ShiftDate)?.DateKey.ToString("yyyy-MM-dd");

                DateTime? breakBeginDate = null;
                DateTime? breakEndDate = null;

                if (!string.IsNullOrEmpty(item.BreakBeginDate))
                {
                    breakBeginDate = DateTime.ParseExact(item.BreakBeginDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    if (!string.IsNullOrEmpty(item.BreakBeginTime))
                    {
                        TimeSpan breakBeginTime;
                        if (!TimeSpan.TryParse(item.BreakBeginTime, out breakBeginTime))
                        { }

                        breakBeginDate = breakBeginDate.Value.Add(breakBeginTime);
                    }
                }
                else
                {
                    employeebreak.BreakDateStart = null;
                }

                if (!string.IsNullOrEmpty(item.BreakEndDate))
                {
                    breakEndDate = DateTime.ParseExact(item.BreakEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    if (!string.IsNullOrEmpty(item.BreakEndTime))
                    {
                        TimeSpan breakEndTime;
                        if (!TimeSpan.TryParse(item.BreakEndTime, out breakEndTime))
                        { }
                        breakEndDate = breakEndDate.Value.Add(breakEndTime);
                    }
                }
                else
                {
                    employeebreak.BreakDateEnd = null;
                }

                if (breakBeginDate != null && breakBeginDate is DateTime)
                {
                    employeebreak.BreakDateStart = breakBeginDate;
                }

                if (breakEndDate != null && breakEndDate is DateTime)
                {
                    employeebreak.BreakDateEnd = breakEndDate;
                }

                Db.SaveChanges();
            }



            return RedirectToAction("Index", new { date = datekey });
        }




    }
}