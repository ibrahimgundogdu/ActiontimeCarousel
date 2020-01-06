using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class EmployeeController : BaseController
    {
        // GET: Employee
        [AllowAnonymous]
        public ActionResult Index(int? locationId)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            
            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.LocationID = locationId != null ? locationId : Db.Location.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).LocationID;
                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true);
            model.ShiftTypeList = Db.ShiftType.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.RoleList = Db.Role.Where(x => x.IsActive == true).ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();

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
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Index", "Employee");
        }

        [AllowAnonymous]
        public PartialViewResult EmployeeSearch(string key, string active) //
        {
            EmployeeControlModel model = new EmployeeControlModel();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true);
            model.ShiftTypeList = Db.ShiftType.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.RoleList = Db.Role.Where(x => x.IsActive == true).ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();

            if (!string.IsNullOrEmpty(key))
            {
                key = key.ToUpper().Replace("İ", "I").Replace("Ü", "U").Replace("Ğ", "G").Replace("Ş", "S").Replace("Ç", "C").Replace("Ö", "O");
                model.EmployeeList = model.EmployeeList.Where(x => x.FullNameSearch.Contains(key)).ToList();
            }

            if (!string.IsNullOrEmpty(active))
            {
                if (active == "act")
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == true).ToList();
                }
                else if (active == "psv")
                {
                    model.EmployeeList = model.EmployeeList.Where(x => x.IsActive == false).ToList();
                }

            }


            return PartialView("_PartialEmployeeList", model);
        }

        [AllowAnonymous]
        public PartialViewResult EmployeeList(int? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == id);
            model.ShiftTypeList = Db.ShiftType.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.RoleList = Db.Role.Where(x => x.IsActive == true).ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.EmployeeID == id).ToList();
            model.Employee = model.EmployeeList.FirstOrDefault();

            return PartialView("_PartialAddEmployee", model);
        }

        [AllowAnonymous]
        public PartialViewResult LocationList(int? id)
        {
            EmployeeControlModel model = new EmployeeControlModel();

            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == id);

            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.EmployeeID == id).ToList();
            model.Employee = model.EmployeeList.FirstOrDefault();

            return PartialView("_PartialAddLocation", model);
        }
    }
}