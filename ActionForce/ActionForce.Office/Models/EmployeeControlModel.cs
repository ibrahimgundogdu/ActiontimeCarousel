using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class EmployeeControlModel : LayoutControlModel
    {
        public IEnumerable<VEmployeeList> EmployeeList { get; set; }
        public IEnumerable<VEmployee> VEmployee { get; set; }

        public Employee CurrentEmployee { get; set; }
        public VEmployeeList Employee { get; set; }

        public IEnumerable<EmployeeShiftType> ShiftTypeList { get; set; }
        public IEnumerable<EmployeeStatus> StatusList { get; set; }
        public IEnumerable<Role> RoleList { get; set; }
        public IEnumerable<RoleGroup> RoleGroupList { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<Department> DepartmentList { get; set; }
        public IEnumerable<EmployeePositions> PositionList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }

        

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result<CashActions> Result { get; set; }
        public IEnumerable<ApplicationLog> History { get; set; }

    }
}