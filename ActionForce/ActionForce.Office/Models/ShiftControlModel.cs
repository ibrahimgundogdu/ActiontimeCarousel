using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ShiftControlModel : LayoutControlModel
    {
        public IEnumerable<Location> Locations { get; set; }
        public IEnumerable<LocationShift> LocationShifts { get; set; }
        public IEnumerable<EmployeeShift> EmployeeShifts { get; set; }
    }
}