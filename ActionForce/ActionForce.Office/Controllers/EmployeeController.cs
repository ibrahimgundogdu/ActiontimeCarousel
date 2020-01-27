using ActionForce.Entity;
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
            
           

            if (!string.IsNullOrEmpty(model.FilterModel.IsActive))
            {
                if (model.FilterModel.IsActive == "act")
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == true).ToList();
                }
                else if (model.FilterModel.IsActive == "psv")
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == false).ToList();
                }

            }

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

            if (!string.IsNullOrEmpty(model.FilterModel.IsActive))
            {
                if (model.FilterModel.IsActive == "act")
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == true).ToList();
                }
                else if (model.FilterModel.IsActive == "psv")
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == false).ToList();
                }

            }

            return PartialView("_PartialEmployeeList", model);
        }
        

        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

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

            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();

            model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == model.EmpList.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.EmpList.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.LocationList = Db.Location.ToList();
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute == null);


            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");

            model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Employee" && x.ProcessID == model.EmpList.EmployeeID.ToString()).ToList();
            model.EmployeeLocationList = Db.VEmployeeLocation.Where(x => x.EmployeeUID == id).ToList();

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

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Shift(Guid? id, string week)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(week))
            {
                var weekparts = week.Split('-');
                int _year = Convert.ToInt32(weekparts[0]);
                int _week = Convert.ToInt32(weekparts[1]);
                datekey = Db.DateList.Where(x => x.WeekYear == _year && x.WeekNumber == _week).OrderBy(x => x.DateKey).FirstOrDefault();
            }
            model.CurrentDate = datekey;
            
            string weekcode = $"{datekey.WeekYear}-{datekey.WeekNumber}";
            var weekdatekeys = Db.DateList.Where(x => x.WeekYear == datekey.WeekYear && x.WeekNumber == datekey.WeekNumber).ToList();

            model.WeekCode = weekcode;

            model.WeekList = weekdatekeys;
            model.FirstWeekDay = weekdatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastWeekDay = weekdatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            var prevdate = model.FirstWeekDay.DateKey.AddDays(-1).Date;
            var nextdate = model.LastWeekDay.DateKey.AddDays(1).Date;

            var prevday = Db.DateList.FirstOrDefault(x => x.DateKey == prevdate);
            var nextday = Db.DateList.FirstOrDefault(x => x.DateKey == nextdate);


            model.NextWeekCode = $"{nextday.WeekYear}-{nextday.WeekNumber}";
            model.PrevWeekCode = $"{prevday.WeekYear}-{prevday.WeekNumber}";

            model.CurrentDate = datekey;

            List<DateTime> datelist = model.WeekList.Select(x => x.DateKey).Distinct().ToList();

            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();
            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            
            model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == model.EmpList .EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.EmpList.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.EmpList.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.LocationList = Db.Location.ToList();
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID);
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
            //model.EmpSchedule = Db.VSchedule.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();

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

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.EmployeeList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, null, 0).ToList();
            model.EmpList = model.EmployeeList.FirstOrDefault(x => x.EmployeeUID == id);
            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.EmpList.EmployeeID);

            model.EmployeeActionList = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.EmpList.EmployeeID).OrderBy(x => x.ProcessDate).ToList();

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
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true && x.RoleLevel <= rolLevel).ToList();
            model.AreaCategoryList = Db.EmployeeAreaCategory.Where(x => x.IsActive == true).ToList();
            model.DepartmentList = Db.Department.Where(x => x.IsActive == true).ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.ShiftTypeList = Db.EmployeeShiftType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategoryList = Db.EmployeeSalaryCategory.Where(x => x.IsActive == true).ToList();
            model.SequenceList = Db.EmployeeSequence.Where(x => x.IsActive == true).ToList();

            model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();



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
        public PartialViewResult EmployeeStatus(string IdentityNumber, string FullName, string EMail, string Mobile)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["wizard"] != null)
            {
                model.Wizard = TempData["wizard"] as WizardModel;
            }
            else
            {
                WizardModel wizardModel = new WizardModel();

                wizardModel.IdentityNumber = IdentityNumber;
                wizardModel.FullName = FullName;
                wizardModel.EMail = EMail;
                wizardModel.Mobile = Mobile;

                model.Wizard = wizardModel;
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

            if (model.Wizard.IdentityNumbers?.Count() > 0 || model.Wizard.FullNames?.Count() > 0 || model.Wizard.EMails?.Count() > 0 || model.Wizard.Mobiles?.Count() > 0)
            {
                
                return PartialView("_PartialEmployeeAddStatus", model);
            }
            else
            {
                
                return PartialView("_PartialEmployeeAddNew", model);
            }
            

        }


        
        
        [AllowAnonymous]
        public PartialViewResult EmployeeList(string IdentityNumber, string FullName, string EMail, string Mobile)
        {
            EmployeeControlModel model = new EmployeeControlModel();


            if (TempData["wizard"] != null)
            {
                model.Wizard = TempData["wizard"] as WizardModel;
            }
            else
            {
                WizardModel wizardModel = new WizardModel();
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
                Employees empdoc = new Employees();

                empdoc.AreaCategoryID = employee.AreaCategoryID;
                empdoc.DepartmentID = employee.DepartmentID;
                empdoc.Description = employee.Description;
                empdoc.Mobile2 = employee.Mobile2;
                empdoc.PositionID = employee.PositionID;
                empdoc.RoleGroupID = employee.RoleGroupID;
                empdoc.SalaryCategoryID = employee.SalaryCategoryID;
                empdoc.SequenceID = employee.SequenceID;
                empdoc.ShiftTypeID = employee.ShiftTypeID;
                empdoc.StatusID = employee.StatusID;
                empdoc.Whatsapp = employee.Whatsapp;
                empdoc.Username = employee.Username;
                empdoc.OurCompanyID = employee.OurCompanyID;
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
                Employees empdoc = new Employees();

                empdoc.AreaCategoryID = employee.AreaCategoryID;
                empdoc.DepartmentID = employee.DepartmentID;
                empdoc.Description = employee.Description;
                empdoc.Mobile2 = employee.Mobile2;
                empdoc.PositionID = employee.PositionID;
                empdoc.RoleGroupID = employee.RoleGroupID;
                empdoc.SalaryCategoryID = employee.SalaryCategoryID;
                empdoc.SequenceID = employee.SequenceID;
                empdoc.ShiftTypeID = employee.ShiftTypeID;
                empdoc.StatusID = employee.StatusID;
                empdoc.Whatsapp = employee.Whatsapp;
                empdoc.Username = employee.Username;
                empdoc.OurCompanyID = employee.OurCompanyID;
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
    }
}