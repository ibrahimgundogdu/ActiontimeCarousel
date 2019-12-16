using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class EmployeeShiftModel
    {
        public Employee Employee { get; set; }
        public Location Location { get; set; }
        public Schedule EmployeeSchedule { get; set; }
        public EmployeeShift EmployeeShift { get; set; }
        public List<EmployeeShift> EmployeeBreaks { get; set; }
    }
}