using Actiontime.Data.Entities;
using Actiontime.Models.SerializeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services.Interfaces
{
    public interface IEmployeeService
    {
        Employee? CheckEmployeeLogin(string Username, string Password);
        Employee? GetEmployee(int employeeID);
        List<Employee>? GetEmployeeList();
        EmployeeSchedule? GetEmployeeSchedule(DateTime date, int employeeId);
        List<EmployeeSchedule>? GetEmployeeSchedules(DateTime date, int employeeId);
        void GetSchedules();
        void GetShifts();
        void GetLookups();
        PersonInfo GetPersonInfo(int employeeID);
        List<PersonInfo> GetPersonInfoList();
        string CheckEmployeeShift(int employeeId);
        string CheckEmployeeBreak(int employeeId);

    }
}
