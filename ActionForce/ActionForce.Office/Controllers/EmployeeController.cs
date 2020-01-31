using ActionForce.Entity;
using ActionForce.Office.Models.Document;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
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


            if (TempData["EmployeeFilter"] != null)
            {
                model.FilterModel = TempData["EmployeeFilter"] as EmployeeFilterModel;
            }
            else
            {
                EmployeeFilterModel filterModel = new EmployeeFilterModel();
                model.FilterModel = filterModel;
            }
            model.VEmployee = Db.VEmployee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.DepartmentList = Db.Department.Where(x => x.IsActive == true).ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true).ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();
            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();


            if (TempData["EmployeeFilter"] != null)
            {
                model.FilterModel = TempData["EmployeeFilter"] as EmployeeFilterModel;

                
                if (model.FilterModel.EmployeeID != null)
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.EmployeeID == model.FilterModel.EmployeeID).OrderBy(x => x.FullName).ToList();
                }
                if (model.FilterModel.DepartmentID != null)
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.DepartmentID == model.FilterModel.DepartmentID).OrderBy(x => x.FullName).ToList();
                }

                if (model.FilterModel.PositionID != null)
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.PositionID == model.FilterModel.PositionID).OrderBy(x => x.FullName).ToList();
                }
            }
            
           

            //if (!string.IsNullOrEmpty(model.FilterModel.IsActive))
            //{
            //    if (model.FilterModel.IsActive == "act")
            //    {
            //        model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == true).ToList();
            //    }
            //    else if (model.FilterModel.IsActive == "psv")
            //    {
            //        model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == false).ToList();
            //    }

            //}

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult EmployeeFilter(EmployeeFilterModel filterModel)
        {
            TempData["EmployeeFilter"] = filterModel;

            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult EmployeeSearch(EmployeeFilterModel filterModel)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();


            if (TempData["EmployeeFilter"] != null)
            {
                model.FilterModel = TempData["EmployeeFilter"] as EmployeeFilterModel;

                if (!String.IsNullOrEmpty(model.FilterModel.FullName))
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.FullNameSearch.Contains(model.FilterModel.FullName.ToUpper())).ToList();
                }
                if (model.FilterModel.DepartmentID != null)
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.DepartmentID == model.FilterModel.DepartmentID).OrderBy(x => x.FullName).ToList();
                }

                if (model.FilterModel.PositionID != null)
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.PositionID == model.FilterModel.PositionID).OrderBy(x => x.FullName).ToList();
                }

                
            }
            bool? isActive = filterModel.IsActive == 0 ? false : filterModel.IsActive == 1 ? true : (bool?)null;

            if (isActive != null)
            {
                model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == isActive.Value).ToList();
            }
            //if (!string.IsNullOrEmpty(model.FilterModel.IsActive))
            //{
            //    if (model.FilterModel.IsActive == "act")
            //    {
            //        model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == true).ToList();
            //    }
            //    else if (model.FilterModel.IsActive == "psv")
            //    {
            //        model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == false).ToList();
            //    }

            //}

            return PartialView("_PartialEmployeeList", model);
        }
        

        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();
            

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;

            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            
            model.CurrentDate = datekey;

            string weekcode = $"{datekey.WeekYear}-{datekey.WeekNumber}";
            var weekdatekeys = Db.DateList.Where(x => x.WeekYear == datekey.WeekYear && x.WeekNumber == datekey.WeekNumber).ToList();

            model.WeekCode = weekcode;

            model.WeekList = weekdatekeys;
            model.FirstWeekDay = weekdatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastWeekDay = weekdatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            List<DateTime> datelist = model.WeekList.Select(x => x.DateKey).Distinct().ToList();

            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();
            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();

            model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == model.EmpList.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.EmpList.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.LocationList = Db.Location.ToList();
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute == null);


            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");

            model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Employee" && x.ProcessID == model.EmpList.EmployeeID.ToString()).ToList();
            model.EmployeeLocationList = Db.VEmployeeLocation.Where(x => x.EmployeeUID == id).ToList();

            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();

            model.EmployeeActionList = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.EmpList.EmployeeID && datelist.Contains(x.ProcessDate.Value)).OrderBy(x => x.ProcessDate).ToList();

            var balanceData = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.EmpList.EmployeeID && datelist.Contains(x.ProcessDate.Value)).ToList();
            if (balanceData != null && balanceData.Count > 0)
            {
                List<TotalModel> headerTotals = new List<TotalModel>();


                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Salary",
                    Total = balanceData.Where(x => x.Currency == "TRL").Sum(x => x.Amount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Salary",
                    Total = balanceData.Where(x => x.Currency == "USD").Sum(x => x.Amount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Salary",
                    Total = balanceData.Where(x => x.Currency == "EUR").Sum(x => x.Amount) ?? 0
                });

                model.HeaderTotals = headerTotals;
            }
            else
            {
                List<TotalModel> headerTotals = new List<TotalModel>();

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Salary",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Salary",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Salary",
                    Total = 0
                });

                model.HeaderTotals = headerTotals;
            }




            List<TotalModel> footerTotals = new List<TotalModel>();

            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Salary",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Currency == "TRL").Total
            });



            footerTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Salary",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Currency == "USD").Total
            });



            footerTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Salary",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Currency == "EUR").Total
            });





            model.FooterTotals = footerTotals;



            List<TotalModel> middleTotals = new List<TotalModel>();

            middleTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Salary",
                Total = 0
            });

            middleTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Salary",
                Total = 0
            });

            middleTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Salary",
                Total = 0
            });

            model.MiddleTotals = middleTotals;

            return View(model);
        }
        
        [AllowAnonymous]
        public ActionResult Edit(Guid? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            var rolLevel = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == model.Authentication.ActionEmployee.EmployeeID)?.RoleLevel;


            model.OurList = Db.OurCompany.ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true && x.RoleLevel <= rolLevel).ToList();
            model.AreaCategoryList = Db.EmployeeAreaCategory.Where(x => x.IsActive == true).ToList();
            model.DepartmentList = Db.Department.Where(x => x.IsActive == true).ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.ShiftTypeList = Db.EmployeeShiftType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategoryList = Db.EmployeeSalaryCategory.Where(x => x.IsActive == true).ToList();
            model.SequenceList = Db.EmployeeSequence.Where(x => x.IsActive == true).ToList();

            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Employee" && x.ProcessID == model.EmpList.EmployeeID.ToString()).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute == null);

            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Shift(Guid? id, string month, string date)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(month))
            {
                var moonparts = month.Split('-');
                int _year = Convert.ToInt32(moonparts[0]);
                int _moon = Convert.ToInt32(moonparts[1]);
                datekey = Db.DateList.Where(x => x.Year == _year && x.Month == _moon).OrderBy(x => x.DateKey).FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(date))
            {
                DateTime? _dateurl = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _dateurl);
            }
            model.CurrentDate = datekey;
            
            string mooncode = $"{datekey.Year}-{datekey.Month}";
            var moondatekeys = Db.DateList.Where(x => x.Year == datekey.Year && x.Month == datekey.Month).ToList();

            model.MoonCode = mooncode;

            model.WeekList = moondatekeys;
            model.FirstMoonDay = moondatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastMoonDay = moondatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            var prevdate = model.FirstMoonDay.DateKey.AddDays(-1).Date;
            var nextdate = model.LastMoonDay.DateKey.AddDays(1).Date;

            var prevday = Db.DateList.FirstOrDefault(x => x.DateKey == prevdate);
            var nextday = Db.DateList.FirstOrDefault(x => x.DateKey == nextdate);


            model.NextMoonCode = $"{nextday.Year}-{nextday.Month}";
            model.PrevMoonCode = $"{prevday.Year}-{prevday.Month}";

            model.CurrentDate = datekey;

            List<DateTime> datelist = model.WeekList.Select(x => x.DateKey).Distinct().ToList();

            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();
            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            
            model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == model.EmpList .EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.EmpList.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.LocationList = Db.Location.ToList();

            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute == null);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Schedule(Guid? id, string week)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
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

            model.CurrentLocation = Db.VEmployeeLocation.FirstOrDefault(x => x.EmployeeUID == id);

            if (id != null)
            {
                model.CurrentLocation = Db.VEmployeeLocation.FirstOrDefault(x => x.EmployeeUID == id);
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
            

            List<DateTime> datelist = model.WeekList.Select(x => x.DateKey).Distinct().ToList();

            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();
            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();

            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.EmpList.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute > 0).ToList();

            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute == null);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Action(Guid? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["EmployeeFilter"] != null)
            {
                model.FilterModel = TempData["EmployeeFilter"] as EmployeeFilterModel;
            }
            else
            {
                EmployeeFilterModel filterModel = new EmployeeFilterModel();
                model.FilterModel = filterModel;
            }

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();
            model.EmpList = model.EmployeeList.FirstOrDefault(x => x.EmployeeUID == id);
            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID);

            model.EmployeeActionList = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.EmpList.EmployeeID).OrderBy(x => x.ProcessDate).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute == null);

            var balanceData = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();
            if (balanceData != null && balanceData.Count > 0)
            {
                List<TotalModel> headerTotals = new List<TotalModel>();


                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Salary",
                    Total = balanceData.Where(x => x.Currency == "TRL").Sum(x => x.Amount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Salary",
                    Total = balanceData.Where(x => x.Currency == "USD").Sum(x => x.Amount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Salary",
                    Total = balanceData.Where(x => x.Currency == "EUR").Sum(x => x.Amount) ?? 0
                });

                model.HeaderTotals = headerTotals;
            }
            else
            {
                List<TotalModel> headerTotals = new List<TotalModel>();

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Salary",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Salary",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Salary",
                    Total = 0
                });

                model.HeaderTotals = headerTotals;
            }




            List<TotalModel> footerTotals = new List<TotalModel>();

            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Salary",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Currency == "TRL").Total
            });



            footerTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Salary",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Currency == "USD").Total
            });



            footerTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Salary",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Currency == "EUR").Total
            });





            model.FooterTotals = footerTotals;



            List<TotalModel> middleTotals = new List<TotalModel>();

            middleTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Salary",
                Total = 0
            });

            middleTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Salary",
                Total = 0
            });

            middleTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Salary",
                Total = 0
            });

            model.MiddleTotals = middleTotals;




            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Location(Guid? id, Guid? locationID)
        {
            
            EmployeeControlModel model = new EmployeeControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            var rolLevel = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == model.Authentication.ActionEmployee.EmployeeID)?.RoleLevel;


            model.OurList = Db.OurCompany.ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true && x.RoleLevel <= rolLevel).ToList();

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();
            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();

            model.EmployeeLocationList = Db.VEmployeeLocation.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute == null);

            EmployeeFilterModel filterModel = new EmployeeFilterModel();
            filterModel.EmployeeUID = id;
            filterModel.EmployeeID = model.EmpList.EmployeeID;

            if (locationID != null)
            {
                
                filterModel.LocationUID = locationID;
                model.CurrentLocation = model.EmployeeLocationList.FirstOrDefault(x => x.LocationUID == locationID && x.EmployeeUID == id);
            }

            model.FilterModel = filterModel;
            TempData["EmployeeFilter"] = filterModel;

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult FoodCard(Guid? id, string month, string date)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["EmployeeFilter"] != null)
            {
                model.FilterModel = TempData["EmployeeFilter"] as EmployeeFilterModel;
            }
            else
            {
                EmployeeFilterModel filterModel = new EmployeeFilterModel();
                model.FilterModel = filterModel;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(month))
            {
                var moonparts = month.Split('-');
                int _year = Convert.ToInt32(moonparts[0]);
                int _moon = Convert.ToInt32(moonparts[1]);
                datekey = Db.DateList.Where(x => x.Year == _year && x.Month == _moon).OrderBy(x => x.DateKey).FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(date))
            {
                DateTime? _dateurl = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _dateurl);
            }
            model.CurrentDate = datekey;

            string mooncode = $"{datekey.Year}-{datekey.Month}";
            var moondatekeys = Db.DateList.Where(x => x.Year == datekey.Year && x.Month == datekey.Month).ToList();

            model.MoonCode = mooncode;

            model.WeekList = moondatekeys;
            model.FirstMoonDay = moondatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastMoonDay = moondatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            var prevdate = model.FirstMoonDay.DateKey.AddDays(-1).Date;
            var nextdate = model.LastMoonDay.DateKey.AddDays(1).Date;

            var prevday = Db.DateList.FirstOrDefault(x => x.DateKey == prevdate);
            var nextday = Db.DateList.FirstOrDefault(x => x.DateKey == nextdate);


            model.NextMoonCode = $"{nextday.Year}-{nextday.Month}";
            model.PrevMoonCode = $"{prevday.Year}-{prevday.Month}";

            model.CurrentDate = datekey;

            List<DateTime> datelist = model.WeekList.Select(x => x.DateKey).Distinct().ToList();

            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();
            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute == null);

            model.SetcardParameter = Db.SetcardParameter.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.Year == datekey.Year);

            model.SalaryEarn = Db.VDocumentSalaryEarn.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();


            var balanceData = Db.VDocumentSalaryEarn.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();
            if (balanceData != null && balanceData.Count > 0)
            {
                List<TotalFood> headerTotals = new List<TotalFood>();


                headerTotals.Add(new TotalFood()
                {
                    Currency = "TRL",
                    Type = "FoodCard",
                    Amount = balanceData.Where(x => x.Currency == "TRL").Sum(x => x.QuantityHourSalary * x.UnitPrice) ?? 0,
                    Total = balanceData.Where(x => x.Currency == "TRL").Sum(x => x.QuantityHourFood * model.SetcardParameter.EarnHour * model.SetcardParameter.Amount) ?? 0
                });

                headerTotals.Add(new TotalFood()
                {
                    Currency = "USD",
                    Type = "FoodCard",
                    Amount = balanceData.Where(x => x.Currency == "USD").Sum(x => x.QuantityHourSalary * x.UnitPrice) ?? 0,
                    Total = balanceData.Where(x => x.Currency == "USD").Sum(x => x.QuantityHourFood * model.SetcardParameter.EarnHour * model.SetcardParameter.Amount) ?? 0
                });

                headerTotals.Add(new TotalFood()
                {
                    Currency = "EUR",
                    Type = "FoodCard",
                    Amount = balanceData.Where(x => x.Currency == "EUR").Sum(x => x.QuantityHourSalary * x.UnitPrice) ?? 0,
                    Total = balanceData.Where(x => x.Currency == "EUR").Sum(x => x.QuantityHourFood * model.SetcardParameter.EarnHour * model.SetcardParameter.Amount) ?? 0
                });

                model.HeaderTotal = headerTotals;
            }
            else
            {
                List<TotalFood> headerTotals = new List<TotalFood>();

                headerTotals.Add(new TotalFood()
                {
                    Currency = "TRL",
                    Type = "FoodCard",
                    Amount = 0,
                    Total = 0
                });

                headerTotals.Add(new TotalFood()
                {
                    Currency = "USD",
                    Type = "FoodCard",
                    Amount = 0,
                    Total = 0
                });

                headerTotals.Add(new TotalFood()
                {
                    Currency = "EUR",
                    Type = "FoodCard",
                    Amount = 0,
                    Total = 0
                });

                model.HeaderTotal = headerTotals;
            }




            List<TotalFood> footerTotals = new List<TotalFood>();

            footerTotals.Add(new TotalFood()
            {
                Currency = "TRL",
                Type = "FoodCard",
                Amount = model.HeaderTotal.FirstOrDefault(x => x.Currency == "TRL").Amount,
                Total = model.HeaderTotal.FirstOrDefault(x => x.Currency == "TRL").Total
            });



            footerTotals.Add(new TotalFood()
            {
                Currency = "USD",
                Type = "FoodCard",
                Amount = model.HeaderTotal.FirstOrDefault(x => x.Currency == "USD").Amount,
                Total = model.HeaderTotal.FirstOrDefault(x => x.Currency == "USD").Total
            });



            footerTotals.Add(new TotalFood()
            {
                Currency = "EUR",
                Type = "FoodCard",
                Amount = model.HeaderTotal.FirstOrDefault(x => x.Currency == "EUR").Amount,
                Total = model.HeaderTotal.FirstOrDefault(x => x.Currency == "EUR").Total
            });





            model.FooterTotal = footerTotals;



            List<TotalFood> middleTotals = new List<TotalFood>();

            middleTotals.Add(new TotalFood()
            {
                Currency = "TRL",
                Type = "FoodCard",
                Amount = 0,
                Total = 0
            });

            middleTotals.Add(new TotalFood()
            {
                Currency = "USD",
                Type = "FoodCard",
                Amount = 0,
                Total = 0
            });

            middleTotals.Add(new TotalFood()
            {
                Currency = "EUR",
                Type = "FoodCard",
                Amount = 0,
                Total = 0
            });

            model.MiddleTotal = middleTotals;

            return View(model);
        }






        [AllowAnonymous]
        public ActionResult AddEmployee(Guid? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            
            if (TempData["wizard"] != null)
            {
                model.Wizard = TempData["wizard"] as WizardModel;
            }
            else
            {
                WizardModel wizardModel = new WizardModel();
                
                model.Wizard = wizardModel;
            }

            var rolLevel = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == model.Authentication.ActionEmployee.EmployeeID)?.RoleLevel;

            
            model.OurList = Db.OurCompany.ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true && x.RoleLevel <= rolLevel).ToList();
            model.AreaCategoryList = Db.EmployeeAreaCategory.Where(x => x.IsActive == true).ToList();
            model.DepartmentList = Db.Department.Where(x => x.IsActive == true).ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.ShiftTypeList = Db.EmployeeShiftType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategoryList = Db.EmployeeSalaryCategory.Where(x => x.IsActive == true).ToList();
            model.SequenceList = Db.EmployeeSequence.Where(x => x.IsActive == true).ToList();

            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();

            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();

            return View(model);
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





        
        [AllowAnonymous]
        public PartialViewResult EmployeeStatus(string Identity, string IdentityNumber, string FullName, string EMail, string Mobile)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["wizard"] != null)
            {
                model.Wizard = TempData["wizard"] as WizardModel;
            }
            else
            {
                WizardModel wizardModel = new WizardModel();
                wizardModel.Identity = Identity;
                wizardModel.IdentityNumber = IdentityNumber;
                wizardModel.FullName = FullName;
                wizardModel.EMail = EMail;
                wizardModel.Mobile = Mobile;

                model.Wizard = wizardModel;
            }
            if (model.Wizard.Identity != "")
            {
                var idnt = Db.Employee.Where(x => SqlFunctions.PatIndex("%" + Identity + "%", x.IdentityType) > 0).ToList();
                List<string> _identy = idnt.Select(x => x.IdentityType).ToList();
                model.Wizard.Identitys = _identy;
            }
            if (model.Wizard.IdentityNumber != "")
            {
                var idnt = Db.Employee.Where(x => SqlFunctions.PatIndex("%" + IdentityNumber + "%", x.IdentityNumber) > 0).ToList();
                List<string> _identy = idnt.Select(x => x.IdentityNumber).ToList();
                model.Wizard.IdentityNumbers = _identy;
            }
            if (model.Wizard.FullName != "")
            {
                var idnt = Db.Employee.Where(x => SqlFunctions.PatIndex("%" + FullName + "%", x.FullName) > 0).ToList();
                List<string> _identy = idnt.Select(x => x.FullName).ToList();
                model.Wizard.FullNames = _identy;
            }
            if (model.Wizard.EMail != "")
            {
                var idnt = Db.Employee.Where(x => SqlFunctions.PatIndex("%" + EMail + "%", x.EMail) > 0).ToList();
                List<string> _identy = idnt.Select(x => x.EMail).ToList();
                model.Wizard.EMails = _identy;
            }
            if (model.Wizard.Mobile != "")
            {
                var idnt = Db.Employee.Where(x => SqlFunctions.PatIndex("%" + Mobile + "%", x.Mobile) > 0).ToList();
                List<string> _identy = idnt.Select(x => x.Mobile).ToList();
                model.Wizard.Mobiles = _identy;
            }
            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();


            TempData["Model"] = model;

            if (model.Wizard.Identitys?.Count() > 0 || model.Wizard.IdentityNumbers?.Count() > 0 || model.Wizard.FullNames?.Count() > 0 || model.Wizard.EMails?.Count() > 0 || model.Wizard.Mobiles?.Count() > 0)
            {
                
                return PartialView("_PartialEmployeeAddStatus", model);
            }
            else
            {
                
                return PartialView("_PartialEmployeeAddNew", model);
            }
            

        }


        
        
        [AllowAnonymous]
        public PartialViewResult EmployeeList(string Identity, string IdentityNumber, string FullName, string EMail, string Mobile)
        {
            EmployeeControlModel model = new EmployeeControlModel();


            if (TempData["wizard"] != null)
            {
                model.Wizard = TempData["wizard"] as WizardModel;
            }
            else
            {
                WizardModel wizardModel = new WizardModel();
                wizardModel.Identity = Identity;
                wizardModel.IdentityNumber = IdentityNumber;
                wizardModel.FullName = FullName;
                wizardModel.EMail = EMail;
                wizardModel.Mobile = Mobile;

                model.Wizard = wizardModel;
            }

            var rolLevel = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == model.Authentication.ActionEmployee.EmployeeID)?.RoleLevel;


            model.OurList = Db.OurCompany.ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true && x.RoleLevel <= rolLevel).ToList();
            model.AreaCategoryList = Db.EmployeeAreaCategory.Where(x => x.IsActive == true).ToList();
            model.DepartmentList = Db.Department.Where(x => x.IsActive == true).ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.ShiftTypeList = Db.EmployeeShiftType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategoryList = Db.EmployeeSalaryCategory.Where(x => x.IsActive == true).ToList();
            model.SequenceList = Db.EmployeeSequence.Where(x => x.IsActive == true).ToList();

            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();

            TempData["Model"] = model;

            return PartialView("_PartialEmployeeAddNew", model);
        }




        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewEmployee(NewEmployee employee, HttpPostedFileBase FotoFile)
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

                bool isActive = !string.IsNullOrEmpty(employee.IsActive) && employee.IsActive == "1" ? true : false;
                bool isTemp = !string.IsNullOrEmpty(employee.IsTemp) && employee.IsTemp == "1" ? true : false;

                Employees empdoc = new Employees();
                empdoc.IdentityType = employee.IdentityType;
                empdoc.IdentityNumber = employee.IdentityNumber;
                empdoc.Title = employee.Title;
                empdoc.EMail = employee.EMail;
                empdoc.FullName = employee.FullName;
                empdoc.Mobile = empdoc.Mobile;
                empdoc.Mobile2 = employee.Mobile2;
                empdoc.AreaCategoryID = employee.AreaCategoryID;
                empdoc.DepartmentID = employee.DepartmentID;
                empdoc.Description = employee.Description;
                empdoc.PositionID = employee.PositionID;
                empdoc.RoleGroupID = employee.RoleGroupID;
                empdoc.SalaryCategoryID = employee.SalaryCategoryID;
                empdoc.SequenceID = employee.SequenceID;
                empdoc.ShiftTypeID = employee.ShiftTypeID;
                empdoc.StatusID = employee.StatusID;
                empdoc.Whatsapp = employee.Whatsapp;
                empdoc.Username = employee.Username;
                empdoc.OurCompanyID = employee.OurCompanyID;
                empdoc.IsActive = isActive;
                empdoc.IsTemp = isTemp;

                if (!string.IsNullOrEmpty(employee.Password))
                {
                    empdoc.Password = OfficeHelper.makeMD5(employee.Password);
                }
                if (FotoFile != null && FotoFile.ContentLength > 0)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(FotoFile.FileName);
                    empdoc.FotoFile = filename;
                    string path = "/Document/Employee";

                    try
                    {
                        FotoFile.SaveAs(Path.Combine(Server.MapPath(path), filename));
                    }
                    catch (Exception)
                    {
                    }
                }

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
                result.Message = $"Form bilgileri gelmedi.";
            }

            TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };
            return RedirectToAction("AddEmployee", "Employee");
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditEmployee(NewEmployee employee, HttpPostedFileBase FotoFile)
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
                bool isActive = !string.IsNullOrEmpty(employee.IsActive) && employee.IsActive == "1" ? true : false;
                bool isTemp = !string.IsNullOrEmpty(employee.IsTemp) && employee.IsTemp == "1" ? true : false;

                Employees empdoc = new Employees();
                empdoc.IdentityType = employee.IdentityType;
                empdoc.IdentityNumber = employee.IdentityNumber;
                empdoc.Title = employee.Title;
                empdoc.EMail = employee.EMail;
                empdoc.FullName = employee.FullName;
                empdoc.Mobile = empdoc.Mobile;
                empdoc.Mobile2 = employee.Mobile2;
                empdoc.AreaCategoryID = employee.AreaCategoryID;
                empdoc.DepartmentID = employee.DepartmentID;
                empdoc.Description = employee.Description;
                empdoc.PositionID = employee.PositionID;
                empdoc.RoleGroupID = employee.RoleGroupID;
                empdoc.SalaryCategoryID = employee.SalaryCategoryID;
                empdoc.SequenceID = employee.SequenceID;
                empdoc.ShiftTypeID = employee.ShiftTypeID;
                empdoc.StatusID = employee.StatusID;
                empdoc.Whatsapp = employee.Whatsapp;
                empdoc.Username = employee.Username;
                empdoc.OurCompanyID = employee.OurCompanyID;
                empdoc.IsActive = isActive;
                empdoc.IsTemp = isTemp;
                if (!string.IsNullOrEmpty(employee.Password))
                {
                    empdoc.Password = OfficeHelper.makeMD5(employee.Password);
                }
                if (FotoFile != null && FotoFile.ContentLength > 0)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(FotoFile.FileName);
                    empdoc.FotoFile = filename;
                    string path = "/Document/Employee";

                    try
                    {
                        FotoFile.SaveAs(Path.Combine(Server.MapPath(path), filename));
                    }
                    catch (Exception)
                    {
                    }
                }

                DocumentManager documentManager = new DocumentManager();
                result = documentManager.AddEmployee(empdoc, model.Authentication);

                
                TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

                if (result.IsSuccess == true)
                {
                    return RedirectToAction("Edit", "Employee", new { id = empdoc.EmployeeID });
                }
                else
                {
                    return RedirectToAction("AddEmployee", "Employee");
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
        public PartialViewResult AddEmployeeLocation(NewEmployeeLocation location)
        {
            
            Result<EmployeeLocation> result = new Result<EmployeeLocation>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            EmployeeControlModel model = new EmployeeControlModel();

            if (location != null)
            {
                if (TempData["EmployeeFilter"] != null)
                {
                    model.FilterModel = TempData["EmployeeFilter"] as EmployeeFilterModel;

                    if (model.FilterModel.EmployeeUID != null)
                    {
                        if (model.FilterModel.LocationUID == null)
                        {
                            bool isActive = !string.IsNullOrEmpty(location.IsActive) && location.IsActive == "1" ? true : false;
                            bool isMaster = !string.IsNullOrEmpty(location.IsMaster) && location.IsMaster == "1" ? true : false;
                            EmployeesLocation empdoc = new EmployeesLocation();

                            empdoc.EmployeeID = (int)model.FilterModel.EmployeeID;
                            empdoc.LocationID = location.LocationID;
                            empdoc.IsMaster = isMaster;
                            empdoc.IsActive = isActive;

                            DocumentManager documentManager = new DocumentManager();
                            result = documentManager.AddEmployeeLocation(empdoc, model.Authentication);


                            TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };
                            
                        }
                        else
                        {
                            bool isActive = !string.IsNullOrEmpty(location.IsActive) && location.IsActive == "1" ? true : false;
                            bool isMaster = !string.IsNullOrEmpty(location.IsMaster) && location.IsMaster == "1" ? true : false;

                            EmployeesLocation empdoc = new EmployeesLocation();

                            empdoc.EmployeeID = (int)model.FilterModel.EmployeeID;
                            empdoc.LocationID = location.LocationID;
                            empdoc.IsMaster = isMaster;
                            empdoc.IsActive = isActive;

                            DocumentManager documentManager = new DocumentManager();
                            result = documentManager.EditEmployeeLocation(empdoc, model.Authentication);


                            TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };
                            
                        }

                    }

                }
            }
            else
            {
                result.Message = $"Form bilgileri gelmedi.";
            }

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();
            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, model.FilterModel.EmployeeUID, 0).FirstOrDefault();

            model.EmployeeLocationList = Db.VEmployeeLocation.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();

            TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return PartialView("_PartialEmployeeLocationList", model);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddEmployeePeriods(NewPeriods period)
        {
            EmployeeControlModel model = new EmployeeControlModel();
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == period.EmployeeID);
            var isperiod = Db.EmployeePeriods.FirstOrDefault(x => x.EmployeeID == period.EmployeeID);
            if (isperiod?.ContractStartDate != null)
            {
                if (isperiod.ContractFinishDate == null)
                {
                    return RedirectToAction("Edit", new { id = employee.EmployeeUID });
                }
            }
            if (!string.IsNullOrEmpty(period.startdate))
            {
                DateTime startDate = Convert.ToDateTime(period.startdate);
                DateTime? endDate = !string.IsNullOrEmpty(period.enddate) ? Convert.ToDateTime(period.enddate) : (DateTime?)null;

                if (employee != null)
                {

                    EmployeePeriods param = new EmployeePeriods();
                    param.ContractStartDate = startDate;
                    param.ContractFinishDate = endDate;
                    param.OurCompanyID = period.EmployeeID;
                    param.RecordDate = DateTime.UtcNow.AddHours(3);
                    param.RecordedEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    param.RecordIP = OfficeHelper.GetIPAddress();
                    param.Description = period.Description;
                    param.OurCompanyID = period.OurCompanyID;

                    Db.EmployeePeriods.Add(param);
                    Db.SaveChanges();

                    OfficeHelper.AddApplicationLog("Office", "EmployeePeriods", "Insert", param.ID.ToString(), "Employee", "AddPeriodParameter", null, true, $"Çalışan Periyot Parametresi Eklendi", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, param);

                }
            }
            
            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == period.EmployeeID).ToList();

            return PartialView("_PartialEmployeePeriodsDetail", model);
        }

        [HttpPost]
        public PartialViewResult EditEmployeePeriods(NewPeriods period)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            var periodParameter = Db.EmployeePeriods.FirstOrDefault(x => x.ID == period.ID);

            if (!string.IsNullOrEmpty(period.startdate))
            {

                DateTime startDate = Convert.ToDateTime(period.startdate);
                DateTime? endDate = !string.IsNullOrEmpty(period.enddate) ? Convert.ToDateTime(period.enddate) : (DateTime?)null;

                var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == periodParameter.EmployeeID);

                if (periodParameter != null && employee != null)
                {

                    EmployeePeriods self = new EmployeePeriods ()
                    {
                        ID = periodParameter.ID,
                        FinalFinishDate = periodParameter.FinalFinishDate,
                        Description = periodParameter.Description,
                        ContractStartDate = periodParameter.ContractStartDate,
                        ContractFinishDate = periodParameter.ContractFinishDate,
                        EmployeeID = periodParameter.EmployeeID,
                        OurCompanyID = periodParameter.OurCompanyID,
                        RecordDate = periodParameter.RecordDate,
                        RecordedEmployeeID = periodParameter.RecordedEmployeeID,
                        UpdateEmployeeID = periodParameter.UpdateEmployeeID,
                        RecordIP = periodParameter.RecordIP,
                        UpdateDate = periodParameter.UpdateDate,
                        UpdateIP = periodParameter.UpdateIP
                    };

                    periodParameter.ContractStartDate = startDate;
                    periodParameter.ContractFinishDate = endDate;
                    periodParameter.Description = period.Description;
                    periodParameter.UpdateDate = DateTime.UtcNow.AddHours(3);
                    periodParameter.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    periodParameter.UpdateIP = OfficeHelper.GetIPAddress();


                    Db.SaveChanges();

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<EmployeePeriods>(self, periodParameter, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "EmployeePeriods", "Update", periodParameter.ID.ToString(), "Employee", "EditEmployeePeriods", isequal, true, $"{periodParameter.ID} ID li Çalışan Periyodu Güncellendi", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                }
                
            }

            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == period.EmployeeID).ToList();

            return PartialView("_PartialEmployeePeriodsDetail", model);
        }

        [HttpPost]
        public PartialViewResult DeleteEmployeePeriods(int id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            var periodParameter = Db.EmployeePeriods.FirstOrDefault(x => x.ID == id);
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == periodParameter.EmployeeID);

            if (periodParameter != null && employee != null)
            {
                OfficeHelper.AddApplicationLog("Office", "EmployeePeriods", "Delete", id.ToString(), "Employee", "DeleteEmployeePeriods", null, true, $"Çalışan Periyodu Silindi", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, periodParameter);

                Db.EmployeePeriods.Remove(periodParameter);
                Db.SaveChanges();
            }
            
            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == employee.EmployeeID).ToList();

            return PartialView("_PartialEmployeePeriodsDetail", model);
        }
    }
}