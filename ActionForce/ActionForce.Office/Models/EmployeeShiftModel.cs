using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class EmployeeShiftModel : LayoutControlModel
    {
        public Employee Employee { get; set; }
        public Location Location { get; set; }
        public Schedule EmployeeSchedule { get; set; }
        public EmployeeShift EmployeeShift { get; set; }
        public List<EmployeeShift> EmployeeBreaks { get; set; }
        public Result<EmployeeShift> Result { get; set; }
    }
}