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
        public ActionResult AddEmployee(int? employeeID)
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

            model.EmpList = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == employeeID);

            

            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult EmployeeDetail(int? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            


            model.EmpList = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == id);
            model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Employee" && x.ProcessID == model.EmpList.EmployeeID.ToString()).ToList();
            TempData["Model"] = model;

            return PartialView("_PartialEmployeeDetail", model);
        }

        [AllowAnonymous]
        public PartialViewResult EmployeeUpdate(int? id)
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

            model.EmpList = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == id);
            model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Employee" && x.ProcessID == model.EmpList.EmployeeID.ToString()).ToList();
            TempData["Model"] = model;

            return PartialView("_PartialEmployeeUpdate", model);
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

            model.CurrentDate = datekey;

            model.EmployeeList = Db.VEmployeeList.Where(x => x.EmployeeID == id).ToList();
            model.EmpList = model.EmployeeList.FirstOrDefault(x => x.EmployeeID == id);

            model.EmployeeSchedules = Db.VSchedule.Where(x => x.EmployeeID == id).ToList();


            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == id && x.IsWorkTime == true).ToList();

            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == id && x.BreakDurationMinute > 0).ToList();
            model.LocationList = Db.Location.ToList();
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == id);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == id && x.BreakDurationMinute == null);
            //var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            //var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);



            //var refresh = Db.SetShiftDates();

            //model.CurrentDate = datekey;
            //model.TodayDateCode = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date.ToString("yyyy-MM-dd");
            //model.CurrentDateCode = _date.ToString("yyyy-MM-dd");
            //model.PrevDateCode = _date.AddDays(-1).Date.ToString("yyyy-MM-dd");
            //model.NextDateCode = _date.AddDays(1).Date.ToString("yyyy-MM-dd");

            //model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);


            //model.EmpList = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == id);

            //model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == id && x.ShiftDate == model.CurrentDate.DateKey).ToList();
            //model.EmployeeSchedule = model.EmployeeSchedules.FirstOrDefault(x => x.EmployeeID == id);

            //model.EmployeeShifts = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsWorkTime == true).ToList();
            //model.EmployeeBreaks = Db.EmployeeShift.Where(x => x.ShiftDate == model.CurrentDate.DateKey && x.IsBreakTime == true).ToList();

            //model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == id);
            //model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == id && x.BreakDurationMinute > 0).ToList();
            //model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == id && x.BreakDurationMinute == null);

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
            model.EmployeeList = Db.VEmployeeList.ToList();
            

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

            model.EmployeeList = Db.VEmployeeList.ToList();

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