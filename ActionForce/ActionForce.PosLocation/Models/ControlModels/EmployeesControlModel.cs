using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class EmployeesControlModel : LayoutControlModel
    {
        public IEnumerable<EmployeeModel> Employees { get; set; }
    }
}