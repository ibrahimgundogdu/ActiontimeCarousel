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
        public ActionResult Index()
        {
            EmployeeControlModel model = new EmployeeControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
            }

            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();
            model.ShiftTypeList = Db.ShiftType.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.RoleList = Db.Role.Where(x => x.IsActive == true).ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult EmployeeSearch(string key, string active) //
        {
            EmployeeControlModel model = new EmployeeControlModel();

            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();
            model.ShiftTypeList = Db.ShiftType.Where(x => x.IsActive == true).ToList();
            model.StatusList = Db.EmployeeStatus.Where(x => x.IsActive == true).ToList();
            model.RoleList = Db.Role.Where(x => x.IsActive == true).ToList();
            model.RoleGroupList = Db.RoleGroup.Where(x => x.IsActive == true).ToList();

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