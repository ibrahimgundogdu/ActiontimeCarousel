using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ActionForce.Office.Controllers
{
    public class EmployeeController : BaseController
    {
        // GET: Employee
        [AllowAnonymous]
        public ActionResult Index(int? employeeID)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            
            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();
                model.Filters = filterModel;
            }
            model.VEmployee = Db.VEmployee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.DepartmentList = Db.Department.Where(x => x.IsActive == true).ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true).ToList();
            model.EmployeeList = Db.VEmployeeList.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();

            if (model.Filters.EmployeeID != null)
            {
                model.EmployeeList = Db.VEmployeeList.Where(x => x.EmployeeID == model.Filters.EmployeeID).OrderBy(x => x.FullName).ToList();
            }
            if (model.Filters.DepartmentID != null)
            {
                model.EmployeeList = Db.VEmployeeList.Where(x => x.DepartmentID == model.Filters.DepartmentID).OrderBy(x => x.FullName).ToList();
            }

            if (model.Filters.PositionID != null)
            {
                model.EmployeeList = Db.VEmployeeList.Where(x => x.PositionID == model.Filters.PositionID).OrderBy(x => x.FullName).ToList();
            }
            
            if (!string.IsNullOrEmpty(model.Filters.IsActive))
            {
                if (model.Filters.IsActive == "act")
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == true).ToList();
                }
                else if (model.Filters.IsActive == "psv")
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == false).ToList();
                }

            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EmployeeFilter(int? locationId, int? positionId, int? departmentId, int? employeeID)
        {
            FilterModel model = new FilterModel();

            model.LocationID = locationId;
            model.EmployeeID = employeeID;
            model.DepartmentID = departmentId;
            model.PositionID = positionId;
            TempData["filter"] = model;

            return RedirectToAction("Index", "Employee");
        }
        
        [AllowAnonymous]
        public ActionResult EmployeeSearch(string active)
        {
            FilterModel model = new FilterModel();
            model.IsActive = active;
            TempData["filter"] = model;

            return RedirectToAction("Index", "Employee");
        }

        [AllowAnonymous]
        public ActionResult Detail(int? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");

            model.EmpList = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == id);

            return View(model);
        }



        [AllowAnonymous]
        public PartialViewResult EmployeeCalendar(int? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            

            model.CurrentLocation = Db.VEmployeeLocation.FirstOrDefault(x => x.EmployeeID == id);

            if (id != null)
            {
                model.CurrentLocation = Db.VEmployeeLocation.FirstOrDefault(x => x.EmployeeID == id);
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



            model.EmpSchedule = Db.VSchedule.Where(x => x.EmployeeID == id).ToList();
            model.EmpList = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == id);
            
            TempData["Model"] = model;

            return PartialView("_PartialEmployeeCalendar", model);
        }

        [AllowAnonymous]
        public PartialViewResult EmployeeShift(int? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

           

            var refresh = Db.SetShiftDates();

            model.CurrentDate = datekey;
            model.TodayDateCode = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date.ToString("yyyy-MM-dd");
            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");
            model.PrevDateCode = _date.AddDays(-1).Date.ToString("yyyy-MM-dd");
            model.NextDateCode = _date.AddDays(1).Date.ToString("yyyy-MM-dd");

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);


            model.EmpList = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == id);

            model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == id && x.ShiftDate == model.CurrentDate.DateKey).ToList();
            model.EmployeeSchedule = model.EmployeeSchedules.FirstOrDefault(x => x.EmployeeID == id);

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsWorkTime == true).ToList();
            model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsBreakTime == true).ToList();

            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == id);
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == id && x.BreakDurationMinute > 0).ToList();
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == id && x.BreakDurationMinute == null);

            TempData["Model"] = model;

            return PartialView("_PartialEmployeeShift", model);
        }









        public PartialViewResult AddEmployeeSchedule(int empid, string week)
        {


            EmployeeControlModel model = new EmployeeControlModel();

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


            var schedulelist = Db.VSchedule.Where(x => x.WeekCode.Trim() == weekcode && x.EmployeeID == empid).ToList();
            model.EmpSchedule = schedulelist;



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

            EmployeeControlModel model = new EmployeeControlModel();

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
                        OfficeHelper.AddApplicationLog("Office", "Schedule", "Update", empschedule.Id.ToString(), "Schedule", "AddUpdateEmployeeSchedule", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

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

            return RedirectToAction("Detail", "Employee", new { week = weekcode });
        }



        

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewEmployee(NewEmployee employee)
        {
            Result<Employee> result = new Result<Employee>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            EmployeeControlModel model = new EmployeeControlModel();

            if (employee != null)
            {
                var emp = Db.Employee.FirstOrDefault(x => x.FullName == employee.FullName || x.IdentityNumber == employee.Tc || x.EMail == employee.EMail || x.Mobile == employee.Mobile);

                if (emp == null)
                {
                    Employees empdoc = new Employees();

                    empdoc.FullName = employee.FullName;
                    empdoc.Tc = employee.Tc;
                    empdoc.EMail = employee.EMail;
                    empdoc.Mobile = employee.Mobile;


                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.AddEmployee(empdoc, model.Authentication);

                    TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

                    if (result.IsSuccess == true)
                    {
                        return RedirectToAction("Detail", "Employee", new { id = empdoc.EmployeeID });
                    }
                    else
                    {
                        return RedirectToAction("AddEmployee", "Employee");
                    }
                }
                else
                {
                    result.Message = $"Çalışan mevcut.";
                }
            }
            else
            {
                result.Message = $"Form bilgileri gelmedi.";
            }

            TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };
            return RedirectToAction("AddEmployee", "Employee");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditEmployee(NewEmployee employee)
        {
            Result<Employee> result = new Result<Employee>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            EmployeeControlModel model = new EmployeeControlModel();

            if (employee != null)
            {
                var emp = Db.Employee.FirstOrDefault(x => x.FullName == employee.FullName || x.IdentityNumber == employee.Tc || x.EMail == employee.EMail || x.Mobile == employee.Mobile);

                if (emp == null)
                {
                    Employees empdoc = new Employees();

                    empdoc.FullName = employee.FullName;
                    empdoc.Tc = employee.Tc;
                    empdoc.EMail = employee.EMail;
                    empdoc.Mobile = employee.Mobile;


                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.AddEmployee(empdoc, model.Authentication);

                    TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

                    if (result.IsSuccess == true)
                    {
                        return RedirectToAction("Detail", "Employee", new { id = empdoc.EmployeeID });
                    }
                    else
                    {
                        return RedirectToAction("AddEmployee", "Employee");
                    }
                }
                else
                {
                    result.Message = $"Çalışan mevcut.";
                }
            }
            else
            {
                result.Message = $"Form bilgileri gelmedi.";
            }

            TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };
            return RedirectToAction("AddEmployee", "Employee");
        }
    }
}