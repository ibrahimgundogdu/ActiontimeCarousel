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
    }
}