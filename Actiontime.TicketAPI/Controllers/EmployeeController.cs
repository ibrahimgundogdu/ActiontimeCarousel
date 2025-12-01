using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Actiontime.TicketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ApplicationCloudDbContext _cdb;
        EmployeeService _employeeService;
        public EmployeeController(ApplicationDbContext db, ApplicationCloudDbContext cdb)
        {
            _employeeService = new EmployeeService(db, cdb);
        }



        [HttpGet()]
        public EmployeeResult? Login(string Username, string Password)
        {
            EmployeeResult? result = new EmployeeResult() { Success = 0, Message = string.Empty, Employee = null };

            var _employee = _employeeService.CheckEmployeeLogin(Username, Password);

            if (_employee != null)
            {
                result.Success = 1;
                result.Employee = _employee;
                result.Message = _employee.FullName ?? "Ok";
            }
            else
            {
                result.Message = "User not found!";
            }

            return result;
        }

        [HttpGet()]
        public EmployeeResult? GetEmployeeResult(int Id)
        {
            EmployeeResult? result = new EmployeeResult() { Success = 0, Message = string.Empty, Employee = null };

            var _employee = _employeeService.GetEmployee(Id);

            if (_employee != null)
            {
                result.Success = 1;
                result.Employee = _employee;
                result.Message = _employee.FullName ?? "Ok";
            }
            else
            {
                result.Message = "User not found!";
            }

            return result;
        }

        [HttpGet()]
        public Employee? GetEmployee(int Id)
        {

            return _employeeService.GetEmployee(Id);
        }

        [HttpGet()]
        public List<Employee>? GetEmployees()
        {
            EmployeeResult? result = new EmployeeResult() { Success = 0, Message = string.Empty, Employee = null };

            return _employeeService.GetEmployeeList();
        }

        [HttpGet()]
        public void GetSchedules()
        {
            _employeeService.GetSchedules();
        }

        [HttpGet()]
        public void GetShifts()
        {
            _employeeService.GetShifts();
        }




        [HttpGet()]
        public void GetLookups()
        {
            _employeeService.GetLookups();
        }

        [HttpGet()]
        public EmployeeSchedule? GetEmployeeSchedule(string Date, int employeeID)
        {
            var dateKey = DateTime.Now;

            DateTime.TryParse(Date, out dateKey);

            var schedule = _employeeService.GetEmployeeSchedule(dateKey.Date, employeeID);

            return schedule;
        }

        [HttpGet()]
        public List<EmployeeSchedule>? GetEmployeeSchedules(string Date, int employeeID)
        {
            var dateKey = DateTime.Now;

            DateTime.TryParse(Date, out dateKey);

            return _employeeService.GetEmployeeSchedules(dateKey.Date, employeeID);
        }

        [HttpGet()]
        public PersonInfo GetPersonInfo(int Id)
        {
            return _employeeService.GetPersonInfo(Id);
        }

        [HttpGet()]
        public List<PersonInfo> GetPersonInfoList()
        {
            return _employeeService.GetPersonInfoList();
        }

        [HttpGet()]
        public string CheckEmployeeShift(int id)
        {
            return _employeeService.CheckEmployeeShift(id);
        }

        [HttpGet()]
        public string CheckEmployeeBreak(int id)
        {
            return _employeeService.CheckEmployeeBreak(id);
        }



    }
}
