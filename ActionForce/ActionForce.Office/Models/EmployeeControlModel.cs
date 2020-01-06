using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class EmployeeControlModel : LayoutControlModel
    {
        public IEnumerable<Employee> EmployeeList { get; set; }
        public IEnumerable<VEmployeeLocation> employeeLocations { get; set; }
        public IEnumerable<VEmployeeLocationList> employeeLocationLists { get; set; }

        public Employee CurrentEmployee { get; set; }
        public Employee Employee { get; set; }

        public IEnumerable<ShiftType> ShiftTypeList { get; set; }
        public IEnumerable<EmployeeStatus> StatusList { get; set; }
        public IEnumerable<Role> RoleList { get; set; }
        public IEnumerable<RoleGroup> RoleGroupList { get; set; }

        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        public IEnumerable<BankAccount> BankAccountList { get; set; }

        

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result<CashActions> Result { get; set; }
        public IEnumerable<ApplicationLog> History { get; set; }

    }
}