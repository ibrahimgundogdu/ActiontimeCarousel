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

            Db.sp_employeeUID();
            model.VEmployee = Db.VEmployee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.DepartmentList = Db.Department.Where(x => x.IsActive == true).ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true).ToList();
            model.AreaCategoryList = Db.EmployeeAreaCategory.Where(x => x.IsActive == true).ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();
            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();


            if (TempData["EmployeeFilter"] != null)
            {
                model.FilterModel = TempData["EmployeeFilter"] as EmployeeFilterModel;

                if (!string.IsNullOrEmpty(model.FilterModel.SearchKey))
                {
                    string searchkey = model.FilterModel.SearchKey.Trim();
                    model.EmployeeList = model.EmployeeList.Where(x => (!string.IsNullOrEmpty(x.FullName) && x.FullName.Contains(searchkey)) || (!string.IsNullOrEmpty(x.IdentityNumber) && x.IdentityNumber.Contains(searchkey)) || (!string.IsNullOrEmpty(x.Mobile) && x.Mobile.Contains(searchkey)) || (!string.IsNullOrEmpty(x.EMail) && x.EMail.Contains(searchkey))).OrderBy(x => x.FullName).ToList();
                }

                if (model.FilterModel.LocationID > 0)
                {
                    List<int> empids = Db.EmployeeLocation.Where(x => x.LocationID == model.FilterModel.LocationID.Value).Select(x => x.EmployeeID).ToList();

                    model.EmployeeList = model.EmployeeList.Where(x => empids.Contains(x.EmployeeID)).OrderBy(x => x.FullName).ToList();
                }

                if (model.FilterModel.AreaID != null)
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.AreaCategoryID == model.FilterModel.AreaID).OrderBy(x => x.FullName).ToList();
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

            TempData["EmployeeFilter"] = model.FilterModel;
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

            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();


            if (filterModel != null)
            {
                model.FilterModel = filterModel;

                if (!string.IsNullOrEmpty(model.FilterModel.SearchKey))
                {
                    string searchkey = model.FilterModel.SearchKey.Trim();
                    model.EmployeeList = model.EmployeeList.Where(x => (!string.IsNullOrEmpty(x.FullName) && x.FullName.Contains(searchkey)) || (!string.IsNullOrEmpty(x.IdentityNumber) && x.IdentityNumber.Contains(searchkey)) || (!string.IsNullOrEmpty(x.Mobile) && x.Mobile.Contains(searchkey)) || (!string.IsNullOrEmpty(x.EMail) && x.EMail.Contains(searchkey))).OrderBy(x => x.FullName).ToList();
                }

                if (model.FilterModel.LocationID > 0)
                {
                    List<int> empids = Db.EmployeeLocation.Where(x => x.LocationID == model.FilterModel.LocationID.Value).Select(x => x.EmployeeID).ToList();

                    model.EmployeeList = model.EmployeeList.Where(x => empids.Contains(x.EmployeeID)).OrderBy(x => x.FullName).ToList();
                }

                if (model.FilterModel.AreaID != null)
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.AreaCategoryID == model.FilterModel.AreaID).OrderBy(x => x.FullName).ToList();
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

            TempData["EmployeeFilter"] = filterModel;

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

            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);


            model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == model.Employee.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.LocationList = Db.Location.ToList();
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null);


            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");

            model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Employee" && x.ProcessID == model.Employee.EmployeeID.ToString()).ToList();
            model.EmployeeLocationList = Db.VEmployeeLocation.Where(x => x.EmployeeUID == id).ToList();

            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();

            model.EmployeeActionList = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.Employee.EmployeeID && datelist.Contains(x.ProcessDate.Value)).OrderBy(x => x.ProcessDate).ToList();

            var balanceData = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.Employee.EmployeeID && datelist.Contains(x.ProcessDate.Value)).ToList();
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
            Db.Configuration.LazyLoadingEnabled = false;

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            var rolLevel = model.Authentication.ActionEmployee.RoleGroup.RoleLevel;


            model.OurList = Db.OurCompany.ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true).ToList();
            model.AreaCategoryList = Db.EmployeeAreaCategory.Where(x => x.IsActive == true).ToList();
            model.DepartmentList = Db.Department.Where(x => x.IsActive == true).ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.ShiftTypeList = Db.EmployeeShiftType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategoryList = Db.EmployeeSalaryCategory.Where(x => x.IsActive == true).ToList();
            model.SequenceList = Db.EmployeeSequence.Where(x => x.IsActive == true).ToList();
            model.PhoneCodes = Db.CountryPhoneCode.Where(x => x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);
            model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Employee" && x.ProcessID == model.Employee.EmployeeID.ToString()).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date).ToList();
            var employeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true && x.EmployeeID == model.Employee.EmployeeID).ToList();

            model.EmployeeBreaks = employeeBreaks.Where(x => x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = employeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null).OrderByDescending(x => x.ShiftDate).FirstOrDefault();

            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult EditEmployee(EditedEmployee employee, HttpPostedFileBase FotoFile)
        {
            Db.Configuration.LazyLoadingEnabled = false;

            Result<Employee> result = new Result<Employee>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            EmployeeControlModel model = new EmployeeControlModel();
            DateTime daterecord = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);

            var isEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employee.EmployeeID && x.EmployeeUID == employee.EmployeeUID);

            if (employee != null && isEmployee != null)
            {
                try
                {
                    bool isActive = !string.IsNullOrEmpty(employee.IsActive) && employee.IsActive == "1" ? true : false;

                    var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == isEmployee.OurCompanyID);
                    Employee self = new Employee()
                    {
                        EmployeeID = isEmployee.EmployeeID,
                        FullName = isEmployee.FullName,
                        IdentityNumber = isEmployee.IdentityNumber,
                        EMail = isEmployee.EMail,
                        Mobile = isEmployee.Mobile,
                        RecordDate = isEmployee.RecordDate,
                        RecordEmployeeID = isEmployee.RecordEmployeeID,
                        RecordIP = isEmployee.RecordIP,
                        AreaCategoryID = employee.AreaCategoryID,
                        DepartmentID = isEmployee.DepartmentID,
                        Description = isEmployee.Description,
                        EmployeeUID = isEmployee.EmployeeUID,
                        IsActive = isEmployee.IsActive,
                        IsTemp = isEmployee.IsTemp,
                        Mobile2 = isEmployee.Mobile2,
                        Username = isEmployee.Username,
                        Password = isEmployee.Password,
                        PositionID = isEmployee.PositionID,
                        RoleGroupID = isEmployee.RoleGroupID,
                        SalaryCategoryID = isEmployee.SalaryCategoryID,
                        SequenceID = isEmployee.SequenceID,
                        ShiftTypeID = isEmployee.ShiftTypeID,
                        StatusID = isEmployee.StatusID,
                        Title = isEmployee.Title = employee.Title,
                        Whatsapp = isEmployee.Whatsapp,
                        OurCompanyID = isEmployee.OurCompanyID,
                        CountryPhoneCode = isEmployee.CountryPhoneCode,
                        DateStart = isEmployee.DateStart,
                        DateEnd = isEmployee.DateEnd,
                        DismissDescription = isEmployee.DismissDescription,
                        FotoFile = isEmployee.FotoFile,
                        IdentityType = isEmployee.IdentityType,
                        FullNameSearch = isEmployee.FullNameSearch,
                        IsDismissal = isEmployee.IsDismissal,
                        SmsNumber = isEmployee.SmsNumber,
                        UpdateDate = isEmployee.UpdateDate,
                        UpdateEmployeeID = isEmployee.UpdateEmployeeID,
                        UpdateIP = isEmployee.UpdateIP
                    };

                    isEmployee.AreaCategoryID = employee.AreaCategoryID;
                    isEmployee.DepartmentID = employee.DepartmentID;
                    isEmployee.Description = employee.Description;
                    isEmployee.Mobile2 = employee.Mobile2;
                    isEmployee.Username = employee.Username;
                    isEmployee.PositionID = employee.PositionID;
                    isEmployee.RoleGroupID = employee.RoleGroupID;
                    isEmployee.SalaryCategoryID = employee.SalaryCategoryID;
                    isEmployee.SequenceID = employee.SequenceID;
                    isEmployee.ShiftTypeID = employee.ShiftTypeID;
                    isEmployee.StatusID = employee.StatusID;
                    isEmployee.Title = employee.Title;
                    isEmployee.Whatsapp = employee.Whatsapp;
                    isEmployee.UpdateDate = DateTime.UtcNow.AddHours(ourcompany.TimeZone.Value);
                    isEmployee.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    isEmployee.UpdateIP = OfficeHelper.GetIPAddress();
                    isEmployee.IdentityType = employee.IdentityType;
                    isEmployee.IdentityNumber = employee.IdentityNumber;
                    isEmployee.Mobile = employee.Mobile.Replace("(", "").Replace(")", "").Replace(" ", "");
                    isEmployee.FullName = employee.FullName;
                    isEmployee.IsActive = isActive;
                    isEmployee.CountryPhoneCode = employee.CountryPhoneCode;
                    isEmployee.EMail = employee.EMail;
                    isEmployee.OurCompanyID = employee.OurCompanyID;

                    if (isEmployee.StatusID == 1 && isEmployee.DateStart == null)
                    {
                        isEmployee.DateStart = daterecord.Date;
                    }
                    else if (isEmployee.StatusID == 2 && isEmployee.DateEnd == null)
                    {
                        isEmployee.DateEnd = daterecord.Date;
                    }


                    if (!string.IsNullOrEmpty(employee.Password))
                    {
                        isEmployee.Password = OfficeHelper.makeMD5(employee.Password);
                    }

                    if (FotoFile != null && FotoFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(FotoFile.FileName);
                        isEmployee.FotoFile = filename;
                        string path = "/Document/Employee";

                        try
                        {
                            FotoFile.SaveAs(Path.Combine(Server.MapPath(path), filename));

                            if (!Request.IsLocal)
                            {
                                string sourcePath = @"C:\inetpub\wwwroot\Action\Document\Employee";
                                string targetPath = @"C:\inetpub\wwwroot\Office\img\Employee";
                                string sourceFile = System.IO.Path.Combine(sourcePath, filename);
                                string destFile = System.IO.Path.Combine(targetPath, filename);
                                System.IO.File.Copy(sourceFile, destFile, true);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }

                    Db.SaveChanges();

                    result.IsSuccess = true;
                    result.Message = $"{isEmployee.EmployeeID} nolu Çalışan başarı ile güncellendi";

                    // kontroller yapılır.





                    // log atılır.
                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<Employee>(self, isEmployee, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "Employee", "Update", isEmployee.EmployeeID.ToString(), "Employee", "Detail", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

                    if (result.IsSuccess == true)
                    {

                        return RedirectToAction("Edit", "Employee", new { id = isEmployee.EmployeeUID });
                    }
                    else
                    {
                        return RedirectToAction("AddEmployee", "Employee");
                    }
                }
                catch (Exception ex)
                {

                    result.Message = $"Çalışan güncellenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Employee", "Update", isEmployee.EmployeeID.ToString(), "Employee", "Detail", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, employee);
                }
            }
            else
            {
                result.Message = $"Form bilgileri gelmedi.";
            }

            TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };
            return RedirectToAction("AddEmployee", "Employee");
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

            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);

            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();

            model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == model.Employee.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();

            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.LocationList = Db.Location.ToList();

            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null);

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

            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);

            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            model.EmployeeSchedules = Db.Schedule.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();

            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && datelist.Contains(x.ShiftDate.Value)).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute > 0).ToList();

            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null);

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
            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);

            //model.EmpList = model.EmployeeList.FirstOrDefault(x => x.EmployeeUID == id);
            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID);

            model.EmployeeActionList = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.Employee.EmployeeID).OrderBy(x => x.ProcessDate).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null);

            var balanceData = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();
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
        public ActionResult Contacts(Guid? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);

            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID);

            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date && x.StatusID == 2);
            model.EmployeeShift = Db.EmployeeShift.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date && x.IsWorkTime == true);
            model.EmployeeBreak = Db.EmployeeShift.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date && x.IsBreakTime == true && x.BreakDurationMinute == null);

            return View(model);
        }
        [AllowAnonymous]
        public ActionResult Location(Guid? id)
        {

            EmployeeControlModel model = new EmployeeControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;

            model.OurList = Db.OurCompany.ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true && x.IsLocation == true).ToList();

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);

            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();

            model.EmployeeLocationPositions = Db.VEmployeeLocationPosition.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();

            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date && x.StatusID == 2);
            model.EmployeeShift = Db.EmployeeShift.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date && x.IsWorkTime == true);
            model.EmployeeBreak = Db.EmployeeShift.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date && x.IsBreakTime == true && x.BreakDurationMinute == null);

            

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

            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);


            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null);

            //model.SetcardParameter = Db.SetcardParameter.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.Year == datekey.Year);

            //model.SalaryEarn = Db.VDocumentSalaryEarn.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();
            model.CashAction = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.ActionTypeID == 39 && datelist.Contains(x.ProcessDate.Value)).ToList();

            var balanceData = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.ActionTypeID == 39 && datelist.Contains(x.ProcessDate.Value)).ToList();
            if (balanceData != null && balanceData.Count > 0)
            {
                List<TotalFood> headerTotals = new List<TotalFood>();


                headerTotals.Add(new TotalFood()
                {
                    Currency = "TRL",
                    Type = "FoodCard",
                    Amount = balanceData.Where(x => x.Currency == "TRL").Sum(x => x.Collection) ?? 0,
                    Total = balanceData.Where(x => x.Currency == "TRL").Sum(x => x.Amount) ?? 0
                });

                headerTotals.Add(new TotalFood()
                {
                    Currency = "USD",
                    Type = "FoodCard",
                    Amount = balanceData.Where(x => x.Currency == "USD").Sum(x => x.Collection) ?? 0,
                    Total = balanceData.Where(x => x.Currency == "USD").Sum(x => x.Amount) ?? 0
                });

                headerTotals.Add(new TotalFood()
                {
                    Currency = "EUR",
                    Type = "FoodCard",
                    Amount = balanceData.Where(x => x.Currency == "EUR").Sum(x => x.Collection) ?? 0,
                    Total = balanceData.Where(x => x.Currency == "EUR").Sum(x => x.Amount) ?? 0
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
        public ActionResult Salary(Guid? id)
        {

            EmployeeControlModel model = new EmployeeControlModel();

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            var rolLevel = Db.VEmployeeList.FirstOrDefault(x => x.EmployeeID == model.Authentication.ActionEmployee.EmployeeID)?.RoleLevel;


            model.OurList = Db.OurCompany.ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true && x.RoleLevel <= rolLevel).ToList();

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);

            model.EmployeeLocationList = Db.VEmployeeLocation.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null);

            model.EmployeeSalary = Db.EmployeeSalary.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Period(Guid? id)
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

            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null);

            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Document(Guid? id)
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

            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null);

            model.PersonalDocument = Db.VPersonalDocument.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();
            model.PersonalDocumentType = Db.PersonalDocumentType.Where(x => x.IsActive == true).ToList();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Score(Guid? id, string week, string date)
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



            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeUID == id);

            model.EmployeeShifts = Db.EmployeeShift.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date).ToList();
            model.EmployeeBreaks = model.EmployeeShifts.Where(x => x.IsBreakTime == true).ToList();
            model.EmployeeBreaks = model.EmployeeBreaks.Where(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute > 0).ToList();
            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeShift = model.EmployeeShifts.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date);
            model.EmployeeBreak = model.EmployeeBreaks.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.BreakDurationMinute == null);

            model.EmployeeCheck = Db.VSystemCheckEmployeeRows.Where(x => x.EmployeeID == model.Employee.EmployeeID && datelist.Contains(x.DateKey.Value)).ToList();
            return View(model);
        }



        [AllowAnonymous]
        public ActionResult AddEmployee()
        {
            EmployeeControlModel model = new EmployeeControlModel();


            if (TempData["CheckEmployee"] != null)
            {
                model.CheckEmployee = TempData["CheckEmployee"] as CheckEmployee;
            }

            model.PhoneCodes = Db.CountryPhoneCode.Where(x => x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult AddEmployeeCheck(CheckEmployee employee)
        {
            EmployeeControlModel model = new EmployeeControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            List<Employee> absoluteEmployees = new List<Employee>();
            List<Employee> optionalEmployees = new List<Employee>();
            employee.FullName = employee.FullName.ToUpper().Trim();
            TempData["CheckEmployee"] = employee;

            if (employee != null)
            {
                model.CheckEmployee = employee;

                var rolLevel = model.Authentication.ActionEmployee.RoleGroup.RoleLevel;

                model.OurList = Db.OurCompany.ToList();
                model.PhoneCodes = Db.CountryPhoneCode.Where(x => x.IsActive == true).OrderBy(x => x.SortBy).ToList();
                model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();

                var absemployeeid = Db.Employee.Where(x => (x.IdentityType == employee.IdentityType && x.IdentityNumber == employee.IdentityNumber)).ToList();
                absoluteEmployees.AddRange(absemployeeid.Distinct());

                if (!string.IsNullOrEmpty(employee.EMail))
                {
                    var absemployeeem = Db.Employee.Where(x => (x.EMail.Trim() == employee.EMail.Trim())).ToList();
                    absoluteEmployees.AddRange(absemployeeem.Distinct());
                }

                if (!string.IsNullOrEmpty(employee.Mobile))
                {
                    var absemployeeph = Db.Employee.Where(x => (x.CountryPhoneCode == employee.CountryPhoneCode && x.Mobile.Trim() == employee.Mobile.Trim())).ToList();
                    absoluteEmployees.AddRange(absemployeeph.Distinct());
                }

                if (!string.IsNullOrEmpty(employee.FullName))
                {
                    var optemployee = Db.Employee.Where(x => x.FullName.ToUpper().Trim().Contains(employee.FullName)).ToList();
                    optionalEmployees.AddRange(optemployee.Distinct());
                }

                model.AbsoluteEmployees = absoluteEmployees;
                model.OptionalEmployees = optionalEmployees.Where(x => !absoluteEmployees.Contains(x));

                model.Result.IsSuccess = true;
                model.Result.Message = $"{model.AbsoluteEmployees.Count()} adet tam uyumlu, {model.OptionalEmployees.Count()} adet benzer kayıt bulundu";
            }
            else
            {
                model.Result.Message = "Girilen bilgilere ulaşılamadı";
                return RedirectToAction("AddEmployee", "Employee");
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult AddEmployeeComplete()
        {
            EmployeeControlModel model = new EmployeeControlModel();


            if (TempData["CheckEmployee"] != null)
            {
                model.CheckEmployee = TempData["CheckEmployee"] as CheckEmployee;

                model.PhoneCodes = Db.CountryPhoneCode.Where(x => x.IsActive == true).OrderBy(x => x.SortBy).ToList();
                model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();
                model.OurList = Db.OurCompany.ToList();
                model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true).ToList();
                model.AreaCategoryList = Db.EmployeeAreaCategory.Where(x => x.IsActive == true).ToList();
                model.DepartmentList = Db.Department.Where(x => x.IsActive == true).ToList();
                model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true).ToList();
                model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
                model.ShiftTypeList = Db.EmployeeShiftType.Where(x => x.IsActive == true).ToList();
                model.SalaryCategoryList = Db.EmployeeSalaryCategory.Where(x => x.IsActive == true).ToList();
                model.SequenceList = Db.EmployeeSequence.Where(x => x.IsActive == true).ToList();
                model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

                TempData["CheckEmployee"] = model.CheckEmployee;

                return View(model);

            }
            else
            {
                return RedirectToAction("AddEmployee", "Employee");
            }

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddEmployeeCompleteForm(CheckedEmployee employee, HttpPostedFileBase FotoFile)
        {
            Result<Employee> result = new Result<Employee>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            EmployeeControlModel model = new EmployeeControlModel();

            Db.Configuration.LazyLoadingEnabled = false;

            if (employee != null)
            {

                bool isActive = true;
                DateTime daterecord = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
                employee.Mobile = employee.Mobile.Replace("(", "").Replace(")", "").Replace(" ", "");

                Employee empdoc = new Employee();

                empdoc.IdentityType = employee.IdentityType;
                empdoc.IdentityNumber = employee.IdentityNumber;
                empdoc.Title = employee.Title;
                empdoc.EMail = employee.EMail;
                empdoc.FullName = employee.FullName.ToUpper();
                empdoc.CountryPhoneCode = employee.CountryPhoneCode;
                empdoc.Mobile = employee.Mobile;

                empdoc.AreaCategoryID = employee.AreaCategoryID;
                empdoc.DepartmentID = employee.DepartmentID;
                empdoc.Description = employee.Description;
                empdoc.PositionID = employee.PositionID;
                empdoc.RoleGroupID = employee.RoleGroupID;
                empdoc.SalaryCategoryID = employee.SalaryCategoryID;
                empdoc.SequenceID = employee.SequenceID;
                empdoc.ShiftTypeID = employee.ShiftTypeID;
                empdoc.StatusID = employee.StatusID;
                empdoc.Username = employee.Username;
                empdoc.OurCompanyID = employee.OurCompanyID.Value;
                empdoc.IsActive = isActive;
                empdoc.IsTemp = false;
                empdoc.RecordDate = daterecord;
                empdoc.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                empdoc.RecordIP = OfficeHelper.GetIPAddress();
                empdoc.EmployeeUID = Guid.NewGuid();

                if (empdoc.StatusID == 1)
                {
                    empdoc.DateStart = daterecord.Date;
                }
                else if (empdoc.StatusID == 2)
                {
                    empdoc.DateEnd = daterecord.Date;
                }

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

                        if (!Request.IsLocal)
                        {
                            string sourcePath = @"C:\inetpub\wwwroot\Action\Document\Employee";
                            string targetPath = @"C:\inetpub\wwwroot\Office\img\Employee";
                            string sourceFile = System.IO.Path.Combine(sourcePath, filename);
                            string destFile = System.IO.Path.Combine(targetPath, filename);
                            System.IO.File.Copy(sourceFile, destFile, true);
                        }

                    }
                    catch (Exception)
                    {
                    }
                }

                Db.Employee.Add(empdoc);
                Db.SaveChanges();


                var our = Db.OurCompany.FirstOrDefault(x => x.CompanyID == empdoc.OurCompanyID);


                result.IsSuccess = true;
                result.Message = "Çalışan başarı ile eklendi";

                // emaili ejklenir
                if (!string.IsNullOrEmpty(empdoc.EMail))
                {
                    EmployeeEmails empemail = new EmployeeEmails()
                    {
                        EmployeeID = empdoc.EmployeeID,
                        EMail = empdoc.EMail,
                        IsActive = true,
                        IsMaster = true,
                        TypeID = 1
                    };
                    Db.EmployeeEmails.Add(empemail);
                    Db.SaveChanges();
                }
                // Telefonu eklenir
                if (!string.IsNullOrEmpty(empdoc.Mobile))
                {
                    EmployeePhones empphone = new EmployeePhones()
                    {
                        EmployeeID = empdoc.EmployeeID,
                        PhoneNumber = empdoc.Mobile,
                        IsActive = true,
                        IsMaster = true,
                        PhoneTypeID = 1,
                        CountryPhoneCode = empdoc.CountryPhoneCode
                    };
                    Db.EmployeePhones.Add(empphone);
                    Db.SaveChanges();
                }
                // periyotları eklenir.
                EmployeePeriods empperiods = new EmployeePeriods();
                empperiods.AreaCategoryID = empdoc.AreaCategoryID;
                empperiods.ContractStartDate = empdoc.RecordDate;
                empperiods.DepartmentID = empdoc.DepartmentID;
                empperiods.EmployeeID = empdoc.EmployeeID;
                empperiods.EmployeeStatusID = empdoc.StatusID;
                empperiods.OurCompanyID = empdoc.OurCompanyID;
                empperiods.PositionID = empdoc.PositionID;
                empperiods.RecordDate = empdoc.RecordDate;
                empperiods.RecordedEmployeeID = empdoc.RecordEmployeeID;
                empperiods.RecordIP = empdoc.RecordIP;
                empperiods.RoleGroupID = empdoc.RoleGroupID;
                empperiods.SalaryCategoryID = empdoc.SalaryCategoryID;
                empperiods.SequenceID = empdoc.SequenceID;
                empperiods.ShiftTypeID = empdoc.ShiftTypeID;
                empperiods.StartDate = empdoc.DateStart;

                Db.EmployeePeriods.Add(empperiods);
                Db.SaveChanges();

                // lokasyon ilişkisi eklenir.

                if (employee.LocationID > 0)
                {
                    EmployeeLocation emploc = new EmployeeLocation();
                    emploc.EmployeeID = empdoc.EmployeeID;
                    emploc.IsActive = true;
                    emploc.IsMaster = true;
                    emploc.LocationID = employee.LocationID.Value;
                    emploc.RoleID = 1;

                    Db.EmployeeLocation.Add(emploc);
                    Db.SaveChanges();
                }


                // log atılır
                OfficeHelper.AddApplicationLog("Office", "Employee", "Insert", empdoc.EmployeeID.ToString(), "Employee", "AddEmployee", null, true, $"{result.Message}", string.Empty, empdoc.RecordDate.Value, model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, empdoc);



                TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

                if (result.IsSuccess == true)
                {
                    return RedirectToAction("Detail", "Employee", new { id = empdoc.EmployeeUID });
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
        public ActionResult GetLocationList(int ourCompanyId)
        {
            var locationList = Db.Location.Where(x => x.OurCompanyID == ourCompanyId && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var location in locationList)
            {
                list.Add(new SelectListItem()
                {
                    Value = location.LocationID.ToString(),
                    Text = location.LocationFullName
                });
            }

            return Json(list, JsonRequestBehavior.AllowGet);
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

            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();

            //model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == model.EmpList.EmployeeID).ToList();
            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.IdentityTypes = Db.IdentityType.Where(x => x.IsActive == true).ToList();
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

            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            TempData["Model"] = model;

            return PartialView("_PartialEmployeeAddNew", model);
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

                Employee empdoc = new Employee();
                empdoc.IdentityType = employee.IdentityType;
                empdoc.IdentityNumber = employee.IdentityNumber;
                empdoc.Title = employee.Title;
                empdoc.EMail = employee.EMail;
                empdoc.FullName = employee.FullName.ToUpper();
                empdoc.Mobile = employee.Mobile;
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
                empdoc.RecordDate = DateTime.Now;
                empdoc.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                empdoc.RecordIP = OfficeHelper.GetIPAddress();
                empdoc.EmployeeUID = Guid.NewGuid();

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

                        string sourcePath = @"C:\inetpub\wwwroot\Action\Document\Employee";
                        string targetPath = @"C:\inetpub\wwwroot\Office\img\Employee";
                        string sourceFile = System.IO.Path.Combine(sourcePath, filename);
                        string destFile = System.IO.Path.Combine(targetPath, filename);
                        System.IO.File.Copy(sourceFile, destFile, true);
                    }
                    catch (Exception)
                    {
                    }
                }



                Db.Employee.Add(empdoc);
                Db.SaveChanges();

                var our = Db.OurCompany.FirstOrDefault(x => x.CompanyID == empdoc.OurCompanyID);


                result.IsSuccess = true;
                result.Message = "Çalışan başarı ile eklendi";

                // log atılır
                OfficeHelper.AddApplicationLog("Office", "Employee", "Insert", empdoc.EmployeeID.ToString(), "Employee", "AddEmployee", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(our.TimeZone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, empdoc);



                TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

                if (result.IsSuccess == true)
                {
                    return RedirectToAction("Detail", "Employee", new { id = empdoc.EmployeeUID });
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
            EmployeeControlModel model = new EmployeeControlModel();

            Result<EmployeeLocation> result = new Result<EmployeeLocation>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            if (location != null)
            {
                bool isActive = !string.IsNullOrEmpty(location.IsActive) && location.IsActive == "1" ? true : false;
                bool isMaster = !string.IsNullOrEmpty(location.IsMaster) && location.IsMaster == "1" ? true : false;

                EmployeesLocation empdoc = new EmployeesLocation();

                empdoc.EmployeeID = location.EmployeeID;
                empdoc.LocationID = location.LocationID;
                empdoc.PositionID = location.PositionID;
                empdoc.IsMaster = isMaster;
                empdoc.IsActive = isActive;

                DocumentManager documentManager = new DocumentManager();
                result = documentManager.AddEmployeeLocation(empdoc, model.Authentication);


                TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };
            }
            else
            {
                result.Message = $"Form bilgileri gelmedi.";
            }

            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;

            model.OurList = Db.OurCompany.ToList();
            model.PositionList = Db.EmployeePositions.Where(x => x.IsActive == true && x.IsLocation == true).ToList();

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.EmployeeList = Db.VEmployeeAll.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.Employee = Db.VEmployeeAll.FirstOrDefault(x => x.EmployeeID == location.EmployeeID);

            //model.EmpList = Db.GetEmployeeAll(model.Authentication.ActionEmployee.OurCompanyID, id, 0).FirstOrDefault();

            model.EmployeeLocationPositions = Db.VEmployeeLocationPosition.Where(x => x.EmployeeID == model.Employee.EmployeeID).ToList();

            model.EmployeeSchedule = Db.Schedule.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date && x.StatusID == 2);
            model.EmployeeShift = Db.EmployeeShift.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date && x.IsWorkTime == true);
            model.EmployeeBreak = Db.EmployeeShift.FirstOrDefault(x => x.EmployeeID == model.Employee.EmployeeID && x.ShiftDate == _date && x.IsBreakTime == true && x.BreakDurationMinute == null);


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
        [AllowAnonymous]
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

                    EmployeePeriods self = new EmployeePeriods()
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

        [AllowAnonymous]
        public ActionResult DeleteEmployeeLocation(int id)
        {
            EmployeeControlModel model = new EmployeeControlModel();
            Db.Configuration.LazyLoadingEnabled = false;

            var employeelocation = Db.EmployeeLocation.FirstOrDefault(x => x.ID == id);
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == employeelocation.EmployeeID);

            if (employeelocation != null && employee != null)
            {
                OfficeHelper.AddApplicationLog("Office", "EmployeeLocation", "Delete", id.ToString(), "Employee", "EmployeeLocation", null, true, $"Çalışan Lokasyonu Silindi", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, employeelocation);

                Db.EmployeeLocation.Remove(employeelocation);
                Db.SaveChanges();
            }

            model.EmployeePeriods = Db.EmployeePeriods.Where(x => x.EmployeeID == employee.EmployeeID).ToList();

            return RedirectToAction("Location","Employee", new { id = employee.EmployeeUID });
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

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddEmployeeDocument(NewDocument document, HttpPostedFileBase DocumentFile)
        {
            EmployeeControlModel model = new EmployeeControlModel();
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == document.EmployeeID);
            var isdocument = Db.PersonalDocument.FirstOrDefault(x => x.EmployeeID == document.EmployeeID);


            if (employee != null)
            {

                PersonalDocument param = new PersonalDocument();
                param.EmployeeID = employee.EmployeeID;
                param.DocumentTypeID = document.DocumentTypeID;
                param.DocumentDescription = document.DocumentDescription;
                param.RecordDate = DateTime.UtcNow.AddHours(3);
                param.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                param.RecordIP = OfficeHelper.GetIPAddress();

                if (DocumentFile != null && DocumentFile.ContentLength > 0)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(DocumentFile.FileName);
                    param.DocumentFile = filename;
                    string path = "/Document/Employee/Document";
                    param.DocumentPath = path;

                    try
                    {
                        DocumentFile.SaveAs(Path.Combine(Server.MapPath(path), filename));

                    }
                    catch (Exception)
                    {
                    }
                }

                Db.PersonalDocument.Add(param);
                Db.SaveChanges();

                OfficeHelper.AddApplicationLog("Office", "PersonalDocument", "Insert", param.ID.ToString(), "Employee", "AddEmployeeDocument", null, true, $"Çalışan Özlük Belgesi Eklendi", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, param);

            }

            model.PersonalDocument = Db.VPersonalDocument.Where(x => x.EmployeeID == document.EmployeeID).ToList();

            return PartialView("_PartialEmployeeDocumentDetail", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult DeleteEmployeeDocument(int id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            var periodParameter = Db.PersonalDocument.FirstOrDefault(x => x.ID == id);
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == periodParameter.EmployeeID);

            if (periodParameter != null && employee != null)
            {
                OfficeHelper.AddApplicationLog("Office", "PersonalDocument", "Delete", id.ToString(), "Employee", "DeleteEmployeeDocument", null, true, $"Çalışan Doküman Silindi", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, periodParameter);

                Db.PersonalDocument.Remove(periodParameter);
                Db.SaveChanges();
            }

            model.PersonalDocument = Db.VPersonalDocument.Where(x => x.EmployeeID == employee.EmployeeID).ToList();

            return PartialView("_PartialEmployeeDocumentDetail", model);
        }




        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddEmployeeSalary(NewSalary empSalary)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            EmployeeControlModel model = new EmployeeControlModel();
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == empSalary.EmployeeID);
            var issalary = Db.EmployeeSalary.FirstOrDefault(x => x.EmployeeID == empSalary.EmployeeID);
            if (issalary?.DateStart != null)
            {
                if (issalary.DateStart == null)
                {
                    return RedirectToAction("Salary", new { id = employee.EmployeeUID });
                }
            }

            if (!string.IsNullOrEmpty(empSalary.DateStart))
            {
                var our = Db.VEmployee.FirstOrDefault(x => x.EmployeeID == empSalary.EmployeeID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == our.OurCompanyID);

                var hourly = Convert.ToDouble(empSalary.Hourly.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var monthly = Convert.ToDouble(empSalary.Monthly.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var hourlyExtent = Convert.ToDouble(empSalary.HourlyExtend.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var extendMultiplyRate = Convert.ToDouble(empSalary.ExtendMultiplyRate.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                var docDate = DateTime.Now.Date;

                if (DateTime.TryParse(empSalary.DateStart, out docDate))
                {
                    docDate = Convert.ToDateTime(empSalary.DateStart).Date;
                }

                var isSalary = Db.EmployeeSalary.FirstOrDefault(x => x.EmployeeID == empSalary.EmployeeID && x.DateStart == docDate);

                if (isSalary == null)
                {
                    try
                    {
                        EmployeeSalary newEmpSalary = new EmployeeSalary();

                        newEmpSalary.EmployeeID = empSalary.EmployeeID;
                        newEmpSalary.DateStart = docDate;
                        newEmpSalary.Hourly = hourly;
                        newEmpSalary.Monthly = monthly;
                        newEmpSalary.Money = ourcompany.Currency;
                        newEmpSalary.HourlyExtend = hourlyExtent;
                        newEmpSalary.ExtendMultiplyRate = extendMultiplyRate;

                        Db.EmployeeSalary.Add(newEmpSalary);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = "Employee saatlik ücret başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", newEmpSalary.ID.ToString(), "Salary", "Unit", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newEmpSalary);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Emplopyee saatlik ücret eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "Unit", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"{our.FullName} Employee { isSalary.DateStart } tarihinde ücret girişi mevcuttur. Kontrol edip güncel tarihli ücret girişi yapabilirsiniz.";

                }

                model.EmployeeSalary = Db.EmployeeSalary.Where(x => x.EmployeeID == empSalary.EmployeeID).ToList();

            }


            TempData["result"] = result;

            model.Result = result;


            return PartialView("_PartialEmployeeSalary", model);
        }


        [HttpPost]
        public PartialViewResult DeleteEmployeeSalary(int id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            var isSalary = Db.EmployeeSalary.FirstOrDefault(x => x.ID == id);
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == isSalary.EmployeeID);

            if (isSalary != null && employee != null)
            {
                OfficeHelper.AddApplicationLog("Office", "EmployeeSalary", "Delete", id.ToString(), "Employee", "DeleteEmployeeSalary", null, true, $"Çalışan ücret tanımı Silindi", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isSalary);

                Db.EmployeeSalary.Remove(isSalary);
                Db.SaveChanges();
            }

            model.EmployeeSalary = Db.EmployeeSalary.Where(x => x.EmployeeID == employee.EmployeeID).ToList();

            return PartialView("_PartialEmployeeSalary", model);
        }
    }
}